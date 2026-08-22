using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace r.e.p.o_cheat;

[HarmonyPatch(typeof(ItemGun), "UpdateMaster")]
public class NoWeaponCooldown
{
	private static FieldInfo _shootCooldownTimerField = AccessTools.Field(typeof(ItemGun), "shootCooldown");

	[HarmonyPrefix]
	public static bool Prefix(ItemGun __instance)
	{
		if (!ConfigManager.NoWeaponCooldownEnabled)
		{
			return true;
		}
		if (!BulletTrack.IsLocalShot(__instance))
		{
			return true;
		}
		try
		{
			if (_shootCooldownTimerField != null)
			{
				_shootCooldownTimerField.SetValue(__instance, 0f);
			}
		}
		catch (Exception)
		{
		}
		return true;
	}
}
