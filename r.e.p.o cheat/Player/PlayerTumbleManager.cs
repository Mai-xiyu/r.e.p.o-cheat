using System;
using System.Collections.Generic;
using System.Reflection;

namespace r.e.p.o_cheat;

internal static class PlayerTumbleManager
{
	private static Type playerTumbleType = Type.GetType("PlayerTumble, Assembly-CSharp");

	private static object playerTumbleInstance;

	private static readonly byte[] disableBytes = new byte[1] { 195 }; // RET

	// 保存每个方法原始字节，而非硬编码 PUSH EBP
	private static readonly Dictionary<string, byte[]> originalBytesCache = new Dictionary<string, byte[]>();

	public static void Initialize()
	{
		if (!(playerTumbleType == null))
		{
			playerTumbleInstance = GameHelper.FindObjectOfType(playerTumbleType);
			_ = playerTumbleInstance;
		}
	}

	public static void DisableMethod(string methodName)
	{
		ModifyMethod(methodName, disableBytes);
	}

	public static void EnableMethod(string methodName)
	{
		// 从缓存恢复原始字节
		if (originalBytesCache.TryGetValue(methodName, out byte[] original))
		{
			ModifyMethod(methodName, original, isRestore: true);
		}
	}

	private unsafe static void ModifyMethod(string methodName, byte[] patch, bool isRestore = false)
	{
		if (playerTumbleType == null || playerTumbleInstance == null)
		{
			Initialize();
			if (playerTumbleInstance == null)
			{
				return;
			}
		}
		MethodInfo method = playerTumbleType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (!(method == null))
		{
			byte* ptr = (byte*)method.MethodHandle.GetFunctionPointer().ToPointer();

			// 第一次修改时保存原始字节
			if (!isRestore && !originalBytesCache.ContainsKey(methodName))
			{
				byte[] backup = new byte[patch.Length];
				for (int i = 0; i < patch.Length; i++)
				{
					backup[i] = ptr[i];
				}
				originalBytesCache[methodName] = backup;
			}

			for (int i = 0; i < patch.Length; i++)
			{
				ptr[i] = patch[i];
			}
		}
	}

	public static void DisableAll()
	{
		DisableMethod("ImpactHurtSet");
		DisableMethod("ImpactHurtSetRPC");
		DisableMethod("Update");
		DisableMethod("TumbleSet");
		DisableMethod("Setup");
	}

	public static void EnableAll()
	{
		EnableMethod("ImpactHurtSet");
		EnableMethod("ImpactHurtSetRPC");
		EnableMethod("Update");
		EnableMethod("TumbleSet");
		EnableMethod("Setup");
	}
}
