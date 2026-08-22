using System;
using HarmonyLib;
using UnityEngine;

namespace r.e.p.o_cheat;

[HarmonyPatch(typeof(ItemGun), "ShootRPC")]
public class NoWeaponSpread
{
	private static float local_originalGunRandomSpread = -1f;

	[HarmonyPrefix]
	public static void Prefix(ItemGun __instance)
	{
		local_originalGunRandomSpread = -1f;
		if (!BulletTrack.IsLocalShot(__instance))
		{
			return;
		}
		float currentSpreadMultiplier = ConfigManager.CurrentSpreadMultiplier;
		if (Mathf.Approximately(currentSpreadMultiplier, 1f))
		{
			return;
		}
		try
		{
			local_originalGunRandomSpread = __instance.gunRandomSpread;
			__instance.gunRandomSpread = local_originalGunRandomSpread * currentSpreadMultiplier;
		}
		catch (Exception)
		{
		}
	}

	[HarmonyPostfix]
	public static void Postfix(ItemGun __instance)
	{
		if (local_originalGunRandomSpread < 0f || !BulletTrack.IsLocalShot(__instance))
		{
			return;
		}
		try
		{
			__instance.gunRandomSpread = local_originalGunRandomSpread;
		}
		catch (Exception)
		{
		}
		local_originalGunRandomSpread = -1f;
	}
}
