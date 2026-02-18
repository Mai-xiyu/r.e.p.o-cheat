using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

[HarmonyPatch(typeof(PlayerTumble))]
internal class PlayerTumblePatch
{
	public static bool Debounce;

	[HarmonyPrefix]
	[HarmonyPatch("Update")]
	public static void PreFixUpdate(PlayerTumble __instance, PhysGrabObject ___physGrabObject)
	{
		if (!((Object)(object)__instance == (Object)null) && !((Object)(object)___physGrabObject == (Object)null) && ___physGrabObject.grabbed && __instance.playerAvatar.photonView.IsMine && Hax2.debounce)
		{
			___physGrabObject.playerGrabbing.ForEach(delegate(PhysGrabber physGrabber)
			{
				physGrabber.photonView.RPC("ReleaseObjectRPC", (RpcTarget)0, new object[2] { true, 0.01f });
			});
		}
	}
}
