using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using SharpMonoInjector;

namespace DarkMenu.Injector;

/// <summary>
/// Elevated, single-purpose launcher for the bundled R.E.P.O. payload.
/// The payload remains an embedded resource; it is never written beside the EXE.
/// </summary>
internal static class Program
{
	private const string TargetProcessName = "repo";
	private const string PayloadResourceName = "DarkMenu.Injector.Payload.dll";
	private const string NoticesResourceName = "DarkMenu.Injector.ThirdPartyNotices.txt";

	[STAThread]
	private static int Main(string[] args)
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);

		if (args.Any(argument => string.Equals(argument, "--help", StringComparison.OrdinalIgnoreCase) ||
			string.Equals(argument, "-h", StringComparison.OrdinalIgnoreCase)))
		{
			ShowInfo("Usage", "Start R.E.P.O. first, then run this EXE.\n\n" +
				"Optional: --pid <R.E.P.O. process id>\n" +
				"This launcher only accepts repo.exe as its target.");
			return 0;
		}

		if (args.Any(argument => string.Equals(argument, "--licenses", StringComparison.OrdinalIgnoreCase)))
		{
			ShowInfo("Third-party notices", ReadEmbeddedText(NoticesResourceName));
			return 0;
		}

		try
		{
			using Process target = FindTargetProcess(args);
			byte[] payload = ReadPayload();
			Log("Target selected: pid=" + target.Id + ", name=" + target.ProcessName + ".");

			using var injector = new SharpMonoInjector.Injector(target.Id);
			if (!injector.Is64Bit)
			{
				throw new InvalidOperationException("The bundled launcher supports the x64 R.E.P.O. client only.");
			}

			IntPtr remoteAssembly = injector.Inject(payload, "r.e.p.o_cheat", "Loader", "Init");
			string address = "0x" + remoteAssembly.ToInt64().ToString("X16", CultureInfo.InvariantCulture);
			string success = "Injection completed.\n\n" +
				"Target: repo.exe (PID " + target.Id + ")\n" +
				"Assembly: " + address + "\n\n" +
				"Open or close the menu with Delete.";
			Log(success.Replace(Environment.NewLine, " | "));
			ShowInfo("DARK MENU Injector", success);
			return 0;
		}
		catch (Exception exception)
		{
			string failure = "Injection failed.\n\n" + exception.Message +
				"\n\nEnsure R.E.P.O. is running, uses its Mono runtime, and was started normally.\n" +
				"Diagnostics: " + GetLogPath();
			Log("Injection failed: " + exception);
			MessageBox.Show(failure, "DARK MENU Injector", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return 1;
		}
	}

	private static Process FindTargetProcess(IReadOnlyList<string> args)
	{
		int? requestedProcessId = ParseRequestedProcessId(args);
		if (requestedProcessId.HasValue)
		{
			Process requested = Process.GetProcessById(requestedProcessId.Value);
			if (!string.Equals(requested.ProcessName, TargetProcessName, StringComparison.OrdinalIgnoreCase))
			{
				requested.Dispose();
				throw new ArgumentException("--pid must identify R.E.P.O. (repo.exe).");
			}
			return requested;
		}

		Process[] candidates = Process.GetProcessesByName(TargetProcessName);
		if (candidates.Length == 0)
		{
			throw new InvalidOperationException("R.E.P.O. (repo.exe) is not running.");
		}

		Process selected = candidates
			.OrderByDescending(GetSafeStartTime)
			.First();
		foreach (Process candidate in candidates)
		{
			if (!ReferenceEquals(candidate, selected))
			{
				candidate.Dispose();
			}
		}

		if (candidates.Length > 1)
		{
			Log("Multiple repo.exe processes found; selected most recently started PID " + selected.Id + ".");
		}
		return selected;
	}

	private static DateTime GetSafeStartTime(Process process)
	{
		try
		{
			return process.StartTime;
		}
		catch
		{
			return DateTime.MinValue;
		}
	}

	private static int? ParseRequestedProcessId(IReadOnlyList<string> args)
	{
		for (int index = 0; index < args.Count; index++)
		{
			string argument = args[index];
			string? value = null;
			if (string.Equals(argument, "--pid", StringComparison.OrdinalIgnoreCase))
			{
				if (index + 1 >= args.Count)
				{
					throw new ArgumentException("--pid requires a process id.");
				}
				value = args[++index];
			}
			else if (argument.StartsWith("--pid=", StringComparison.OrdinalIgnoreCase))
			{
				value = argument.Substring("--pid=".Length);
			}

			if (value != null)
			{
				if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int processId) || processId <= 0)
				{
					throw new ArgumentException("--pid must be a positive integer.");
				}
				return processId;
			}
		}

		return null;
	}

	private static byte[] ReadPayload()
	{
		Assembly assembly = typeof(Program).Assembly;
		using Stream stream = assembly.GetManifestResourceStream(PayloadResourceName)
			?? throw new InvalidOperationException("The bundled payload resource is missing.");
		using var buffer = new MemoryStream();
		stream.CopyTo(buffer);
		byte[] payload = buffer.ToArray();
		if (payload.Length == 0)
		{
			throw new InvalidOperationException("The bundled payload resource is empty.");
		}
		return payload;
	}

	private static string ReadEmbeddedText(string resourceName)
	{
		Assembly assembly = typeof(Program).Assembly;
		using Stream stream = assembly.GetManifestResourceStream(resourceName)
			?? throw new InvalidOperationException("Embedded resource is missing: " + resourceName);
		using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
		return reader.ReadToEnd();
	}

	private static void ShowInfo(string title, string text)
	{
		MessageBox.Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
	}

	private static string GetLogPath()
	{
		string directory = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"DarkMenu");
		return Path.Combine(directory, "injector.log");
	}

	private static void Log(string message)
	{
		try
		{
			string path = GetLogPath();
			Directory.CreateDirectory(Path.GetDirectoryName(path)!);
			File.AppendAllText(path,
				DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture) +
				" " + message + Environment.NewLine);
		}
		catch
		{
			// Diagnostics must not change injection behavior.
		}
	}
}
