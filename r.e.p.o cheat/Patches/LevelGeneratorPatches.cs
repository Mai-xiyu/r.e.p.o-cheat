using HarmonyLib;

namespace r.e.p.o_cheat;

[HarmonyPatch(typeof(LevelGenerator))]
public static class LevelGeneratorPatches
{
	[HarmonyPatch("ItemSetup")]
	[HarmonyPrefix]
	public static bool ItemSetupPrefix()
	{
		return !AntiCrashProtection.ShouldBlockRpc("ItemSetup");
	}

	[HarmonyPatch("NavMeshSetupRPC")]
	[HarmonyPrefix]
	public static bool NavMeshSetupRPCPrefix()
	{
		return !AntiCrashProtection.ShouldBlockRpc("NavMeshSetupRPC");
	}
}
