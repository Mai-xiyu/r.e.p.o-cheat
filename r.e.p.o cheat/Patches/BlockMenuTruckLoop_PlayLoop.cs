using HarmonyLib;
using UnityEngine;

namespace r.e.p.o_cheat;

[HarmonyPatch(typeof(Sound), "PlayLoop")]
internal class BlockMenuTruckLoop_PlayLoop
{
	private static bool Prefix(object __instance, ref bool playing)
	{
		object value = AccessTools.Field(__instance.GetType(), "Source").GetValue(__instance);
		AudioSource val = (AudioSource)((value is AudioSource) ? value : null);
		if ((Object)(object)((val != null) ? val.clip : null) != (Object)null && ((Object)val.clip).name == "menu truck engine loop")
		{
			val.Stop();
			val.clip = null;
			playing = false;
			return false;
		}
		return true;
	}
}
