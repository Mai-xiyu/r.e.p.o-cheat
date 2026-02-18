using System;
using System.Reflection;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

[HarmonyPatch(typeof(ItemGun), "ShootRPC")]
public class NoWeaponSpread
{
	private static float local_originalGunRandomSpread = -1f;

	private static FieldInfo _photonViewField = AccessTools.Field(typeof(ItemGun), "photonView");

	[HarmonyPrefix]
	public static void Prefix(ItemGun __instance)
	{
		local_originalGunRandomSpread = -1f;
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
			float gunRandomSpread = local_originalGunRandomSpread * currentSpreadMultiplier;
			__instance.gunRandomSpread = gunRandomSpread;
		}
		catch (Exception)
		{
		}
	}

	[HarmonyPostfix]
	public static void Postfix(ItemGun __instance)
	{
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
		if (flag && !(local_originalGunRandomSpread < 0f))
		{
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
}
