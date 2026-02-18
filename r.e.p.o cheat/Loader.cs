using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace r.e.p.o_cheat;

public class Loader
{
	private static object harmonyInstance;

	private static GameObject Load;

	public static bool hasTriggeredRecovery;

	private static void HandleUnityLog(string condition, string stackTrace, LogType type)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)type == 2 && condition.Contains("Unicode value"))
		{
			condition.Contains("font asset");
		}
	}

	public static void Init()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		try
		{
			Directory.CreateDirectory("C:\\temp");
			File.WriteAllText("C:\\temp\\inject_debug.txt", "Init() reached\n");
			AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args)
			{
				string resourceName = args.Name.Split(',')[0] + ".dll";
				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				string text = executingAssembly.GetManifestResourceNames().FirstOrDefault((string r) => r.EndsWith(resourceName));
				if (text != null)
				{
					using Stream stream = executingAssembly.GetManifestResourceStream(text);
					if (stream != null)
					{
						byte[] array = new byte[stream.Length];
						stream.Read(array, 0, array.Length);
						return Assembly.Load(array);
					}
				}
				return (Assembly)null;
			};
			Load = new GameObject();
			Load.AddComponent<Hax2>();
			Object.DontDestroyOnLoad((Object)(object)Load);
			Load.AddComponent<PatchDelay>();
			Application.logMessageReceived += HandleUnityLog;
		}
		catch (Exception ex)
		{
			File.WriteAllText("C:\\temp\\inject_error.txt", ex.ToString());
		}
	}

	public static IEnumerator DelayedPatchRoutine()
	{
		while (Type.GetType("SpectateCamera, Assembly-CSharp") == null || Type.GetType("InputManager, Assembly-CSharp") == null)
		{
			yield return (object)new WaitForSeconds(0.5f);
			File.AppendAllText("C:\\temp\\inject_debug.txt", "Waiting for types...\n");
		}
		try
		{
			File.AppendAllText("C:\\temp\\inject_debug.txt", "Types found, creating Harmony...\n");
			new Harmony("dark_cheat").PatchAll(typeof(Patches).Assembly);
			File.AppendAllText("C:\\temp\\inject_debug.txt", "Harmony patches applied successfully\n");
		}
		catch (Exception ex)
		{
			File.AppendAllText("C:\\temp\\inject_debug.txt", "Harmony error: " + ex.ToString() + "\n");
		}
	}

	public static void UnloadCheat()
	{
		try
		{
			if ((Object)(object)Load != (Object)null)
			{
				Object.Destroy((Object)(object)Load);
				Load = null;
			}
			if (harmonyInstance != null)
			{
				harmonyInstance.GetType().GetMethod("UnpatchSelf")?.Invoke(harmonyInstance, null);
				harmonyInstance = null;
			}
			GC.Collect();
			File.AppendAllText("C:\\temp\\inject_debug.txt", "UnloadCheat() completed\n");
		}
		catch (Exception ex)
		{
			File.WriteAllText("C:\\temp\\unload_error.txt", ex.ToString());
		}
	}
}
