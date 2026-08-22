using System;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// Local scare uses AudioScare (game jump-scare mixer). Host ambience uses
/// AmbienceBreakers.PlaySoundRPC, which is MasterOnlyRPC.
/// </summary>
public static class ScareSound
{
	public static string statusMessage = "";

	public static void TriggerRandomScare()
	{
		bool local = PlayLocalImpact(soft: UnityEngine.Random.value > 0.45f);
		bool ambience = PlayHostAmbience();
		if (local && ambience)
		{
			statusMessage = L.T("fun.scare_ok_both");
		}
		else if (local)
		{
			statusMessage = L.T("fun.scare_ok_local");
		}
		else if (ambience)
		{
			statusMessage = L.T("fun.scare_ok_host");
		}
		else
		{
			statusMessage = L.T("fun.scare_fail");
		}
	}

	public static bool PlayLocalImpact(bool soft)
	{
		try
		{
			AudioScare scare = AudioScare.instance;
			if (scare == null)
			{
				return false;
			}
			if (soft)
			{
				scare.PlaySoft();
			}
			else
			{
				scare.PlayImpact();
			}
			return true;
		}
		catch (Exception ex)
		{
			statusMessage = ex.Message;
			Debug.LogWarning("[ScareSound] " + ex.Message);
			return false;
		}
	}

	public static bool PlayHostAmbience()
	{
		try
		{
			if (NativeGameApi.IsGuest())
			{
				return false;
			}
			AmbienceBreakers breakers = AmbienceBreakers.instance;
			if (breakers == null)
			{
				return false;
			}
			AudioManager audio = AudioManager.instance;
			if (audio == null || audio.levelAmbiences == null || audio.levelAmbiences.Count == 0)
			{
				return false;
			}
			LevelAmbience preset = audio.levelAmbiences[UnityEngine.Random.Range(0, audio.levelAmbiences.Count)];
			if (preset == null || preset.breakers == null || preset.breakers.Count == 0)
			{
				return false;
			}
			int breaker = UnityEngine.Random.Range(0, preset.breakers.Count);
			Vector3 pos = Vector3.zero;
			PlayerAvatar local = SemiFunc.PlayerAvatarLocal();
			if (local != null)
			{
				Vector2 ring = UnityEngine.Random.insideUnitCircle.normalized;
				float dist = UnityEngine.Random.Range(8f, 15f);
				pos = local.transform.position + new Vector3(ring.x, 0f, ring.y) * dist;
			}
			if (!SemiFunc.IsMultiplayer())
			{
				breakers.PlaySoundRPC(pos, preset.name, breaker);
				return true;
			}
			PhotonView view = breakers.GetComponent<PhotonView>();
			if (view == null)
			{
				return false;
			}
			view.RPC("PlaySoundRPC", RpcTarget.All, pos, preset.name, breaker);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[ScareSound] ambience: " + ex.Message);
			return false;
		}
	}
}
