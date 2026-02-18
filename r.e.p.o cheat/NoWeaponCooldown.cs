using System;
using System.Reflection;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

[HarmonyPatch(typeof(ItemGun), "UpdateMaster")]
public class NoWeaponCooldown
{
	private static FieldInfo _photonViewField = AccessTools.Field(typeof(ItemGun), "photonView");

	private static FieldInfo _shootCooldownTimerField = AccessTools.Field(typeof(ItemGun), "shootCooldownTimer");

	[HarmonyPrefix]
	public static bool Prefix(ItemGun __instance)
	{
		if (!ConfigManager.NoWeaponCooldownEnabled)
		{
			return true;
		}
		bool flag = false;
		try
		{
			if ((Object)(object)__instance != (Object)null && _photonViewField != null)
			{
				object value = _photonViewField.GetValue(__instance);
				PhotonView val = (PhotonView)((value is PhotonView) ? value : null);
				if ((Object)(object)val != (Object)null)
				{
					flag = val.IsMine;
				}
			}
		}
		catch (Exception)
		{
		}
		if (!flag)
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
