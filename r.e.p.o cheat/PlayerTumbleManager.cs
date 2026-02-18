using System;
using System.Reflection;

namespace r.e.p.o_cheat;

internal static class PlayerTumbleManager
{
	private static Type playerTumbleType = Type.GetType("PlayerTumble, Assembly-CSharp");

	private static object playerTumbleInstance;

	private static readonly byte[] disableBytes = new byte[1] { 195 };

	private static readonly byte[] enableBytes = new byte[1] { 85 };

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
		if (!(methodName == "Update") && !(methodName == "Setup"))
		{
			ModifyMethod(methodName, disableBytes);
		}
	}

	public static void EnableMethod(string methodName)
	{
		ModifyMethod(methodName, enableBytes);
	}

	private unsafe static void ModifyMethod(string methodName, byte[] patch)
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
