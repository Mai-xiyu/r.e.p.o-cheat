using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using r.e.p.o_cheat.Compatibility;
using r.e.p.o_cheat.Localization;

namespace r.e.p.o_cheat;

public class Loader
{
	private const string LoadGameObjectName = "r.e.p.o_cheat_Load";

	private static readonly object InitLock = new object();

	private static object harmonyInstance;

	private static GameObject Load;

	private static bool _initialized;

	private static ResolveEventHandler _resolveHandler;

	private static Application.LogCallback _logCallback;

	public static bool hasTriggeredRecovery;

	private static void HandleUnityLog(string condition, string stackTrace, LogType type)
	{
		// quiet the per-glyph log spam TMP produces when a font lacks a character
		// (relevant while the Chinese fallback font is active)
		if ((int)type == 2 && condition.Contains("Unicode value"))
		{
			condition.Contains("font asset");
		}
	}

	/// <summary>
	/// Injection entry point (SharpMonoInjector: -c Loader -m Init).
	/// Idempotent: repeated injection into the same process reuses the existing
	/// GameObject and Harmony instance instead of duplicating them.
	/// </summary>
	public static void Init()
	{
		try
		{
			Directory.CreateDirectory("C:\\temp");
			File.AppendAllText("C:\\temp\\inject_debug.txt", "Init() reached\n");
			lock (InitLock)
			{
				if (_initialized)
				{
					File.AppendAllText("C:\\temp\\inject_debug.txt", "Init() skipped: already initialized\n");
					return;
				}
				_initialized = true;
				EnsureAssemblyResolver();
				EnsureLogCallback();
				EnsureLoadObject();
				LogDiagnostics();
				File.AppendAllText("C:\\temp\\inject_debug.txt", "Init() completed\n");
			}
		}
		catch (Exception ex)
		{
			File.WriteAllText("C:\\temp\\inject_error.txt", ex.ToString());
		}
	}

	private static void EnsureAssemblyResolver()
	{
		if (_resolveHandler != null)
		{
			return;
		}
		_resolveHandler = delegate(object sender, ResolveEventArgs args)
		{
			try
			{
				string resourceName = args.Name.Split(',')[0] + ".dll";
				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				string text = executingAssembly.GetManifestResourceNames().FirstOrDefault((string r) => r.EndsWith(resourceName));
				if (text != null)
				{
					using (Stream stream = executingAssembly.GetManifestResourceStream(text))
					{
						if (stream != null)
						{
							byte[] array = new byte[stream.Length];
							stream.Read(array, 0, array.Length);
							return Assembly.Load(array);
						}
					}
				}
			}
			catch
			{
			}
			return null;
		};
		AppDomain.CurrentDomain.AssemblyResolve += _resolveHandler;
	}

	private static void EnsureLogCallback()
	{
		if (_logCallback != null)
		{
			return;
		}
		_logCallback = HandleUnityLog;
		Application.logMessageReceived += _logCallback;
	}

	private static void EnsureLoadObject()
	{
		Load = GameObject.Find(LoadGameObjectName);
		if (Load == null)
		{
			Load = new GameObject(LoadGameObjectName);
			Object.DontDestroyOnLoad((Object)(object)Load);
		}
		if (Load.GetComponent<Hax2>() == null)
		{
			Load.AddComponent<Hax2>();
		}
		if (Load.GetComponent<PatchDelay>() == null)
		{
			Load.AddComponent<PatchDelay>();
		}
		if (Load.GetComponent<GameLocalizationManager>() == null)
		{
			Load.AddComponent<GameLocalizationManager>();
		}
	}

	private static void LogDiagnostics()
	{
		try
		{
			string gameRoot = null;
			string dataPath = Application.dataPath;
			if (!string.IsNullOrEmpty(dataPath))
			{
				gameRoot = Path.GetDirectoryName(dataPath);
			}
			Debug.Log("[Loader] game root: " + (gameRoot ?? "<unknown>"));
			Debug.Log("[Loader] " + GameVersionInfo.GetDiagnostics());
		}
		catch
		{
		}
	}

	public static IEnumerator DelayedPatchRoutine()
	{
		File.AppendAllText("C:\\temp\\inject_debug.txt", "DelayedPatchRoutine started\n");
		int waits = 0;
		while (Type.GetType("SpectateCamera, Assembly-CSharp") == null || Type.GetType("InputManager, Assembly-CSharp") == null)
		{
			yield return (object)new WaitForSeconds(0.5f);
			waits++;
			if (waits % 10 == 0)
			{
				File.AppendAllText("C:\\temp\\inject_debug.txt",
					"Still waiting for game types (" + waits + " tries; SpectateCamera=" + (Type.GetType("SpectateCamera, Assembly-CSharp") != null) + ", InputManager=" + (Type.GetType("InputManager, Assembly-CSharp") != null) + ")\n");
			}
		}
		try
		{
			File.AppendAllText("C:\\temp\\inject_debug.txt", "Types found, creating Harmony...\n");
			var harmony = new Harmony("dark_cheat");
			// patch per class so one broken patch class can never take the rest down,
			// and every feature's patch state is visible in this log
			Assembly cheatAssembly = typeof(Patches).Assembly;
			var failures = new System.Collections.Generic.List<string>();
			int patched = 0;
			foreach (Type type in cheatAssembly.GetTypes())
			{
				if (type.GetCustomAttributes(typeof(HarmonyPatch), inherit: false).Length == 0)
				{
					continue;
				}
				try
				{
					harmony.CreateClassProcessor(type).Patch();
					patched++;
				}
				catch (Exception patchEx)
				{
					failures.Add(type.Name + ": " + patchEx.GetBaseException().Message);
				}
			}
			harmonyInstance = harmony;
			string summary = "Harmony patches: " + patched + " classes patched" +
				(failures.Count > 0 ? ", " + failures.Count + " FAILED: " + string.Join("; ", failures) : ", all OK") + "\n";
			File.AppendAllText("C:\\temp\\inject_debug.txt", summary);
			if (failures.Count > 0)
			{
				Debug.LogWarning("[Loader] " + summary.TrimEnd());
			}
		}
		catch (Exception ex)
		{
			File.AppendAllText("C:\\temp\\inject_debug.txt", "Harmony error: " + ex + "\n");
		}
	}

	/// <summary>
	/// Unload: destroys the cheat GameObject (Hax2/PatchDelay/Localization managers clean
	/// themselves up in OnDestroy), unpaches Harmony, restores localization table overrides,
	/// unsubscribes process-wide handlers. Idempotent.
	/// </summary>
	public static void UnloadCheat()
	{
		try
		{
			lock (InitLock)
			{
				GameLocalizationManager localization = null;
				if ((Object)(object)Load != (Object)null)
				{
					localization = Load.GetComponent<GameLocalizationManager>();
					Object.Destroy((Object)(object)Load);
				}
				Load = null;
				if (localization != null)
				{
					// OnDestroy already ran RevertAll paths; make missing-translation flush deterministic
					localization.CleanupForUnload();
				}
				if (harmonyInstance != null)
				{
					harmonyInstance.GetType().GetMethod("UnpatchSelf")?.Invoke(harmonyInstance, null);
					harmonyInstance = null;
				}
				if (_logCallback != null)
				{
					Application.logMessageReceived -= _logCallback;
					_logCallback = null;
				}
				if (_resolveHandler != null)
				{
					AppDomain.CurrentDomain.AssemblyResolve -= _resolveHandler;
					_resolveHandler = null;
				}
				_initialized = false;
				GC.Collect();
				File.AppendAllText("C:\\temp\\inject_debug.txt", "UnloadCheat() completed\n");
			}
		}
		catch (Exception ex)
		{
			File.WriteAllText("C:\\temp\\unload_error.txt", ex.ToString());
		}
	}
}
