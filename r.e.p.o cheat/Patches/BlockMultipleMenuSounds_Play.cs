using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace r.e.p.o_cheat;

[HarmonyPatch(typeof(AudioSource), "Play", new Type[] { })]
internal class BlockMultipleMenuSounds_Play
{
	private static readonly HashSet<string> BlockedClipNames = new HashSet<string>
	{
		"menu truck engine loop", "Ambience Loop Truck Driving", "msc main menu", "menu truck fire pass", "menu truck fire pass swerve01", "menu truck body rustle long01", "menu truck fire pass swerve02", "menu truck body rustle long02", "menu truck skeleton hit", "menu truck body rustle long03",
		"menu truck skeleton hit skull", "menu truck body rustle short02", "menu truck swerve fast01", "menu truck swerve fast02", "menu truck swerve", "menu truck speed up", "menu truck slow down"
	};

	private static bool Prefix(AudioSource __instance)
	{
		if ((Object)(object)((__instance != null) ? __instance.clip : null) != (Object)null)
		{
			string name = ((Object)__instance.clip).name;
			if (BlockedClipNames.Contains(name))
			{
				return false;
			}
		}
		return true;
	}
}
