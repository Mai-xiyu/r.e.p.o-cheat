using System;
using System.Collections.Generic;
using System.Reflection;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace r.e.p.o_cheat;

internal static class Strength
{
	public class PhysGrabObject : MonoBehaviour, IPunOwnershipCallbacks
	{
		public Rigidbody rb;

		private PhotonView photonView;

		private void Awake()
		{
			photonView = ((Component)this).GetComponent<PhotonView>();
			PhotonNetwork.AddCallbackTarget((object)this);
		}

		private void OnDestroy()
		{
			PhotonNetwork.RemoveCallbackTarget((object)this);
		}

		[PunRPC]
		private void ApplyExtraForceRPC(Vector3 direction, float forceMagnitude, Vector3 position)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if (PhotonNetwork.IsMasterClient)
			{
				rb.AddForceAtPosition(direction * forceMagnitude, position, (ForceMode)0);
			}
		}

		[PunRPC]
		private void ResetVelocityRPC()
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			if (PhotonNetwork.IsMasterClient)
			{
				rb.velocity = Vector3.zero;
				rb.angularVelocity = Vector3.zero;
			}
		}

		public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
		{
		}

		public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
		{
		}

		public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
		{
		}
	}

	private static object physGrabberInstance;

	private static float lastStrengthUpdateTime = 0f;

	private static float strengthUpdateCooldown = 0.1f;

	private static PhotonView physGrabberPhotonView;

	private static PhotonView punManagerPhotonView;

	private static float lastAppliedStrength = -1f;

	private static bool? lastGrabbedState = null;

	private static void InitializePlayerController()
	{
		if (!(PlayerController.playerControllerType == null) && PlayerController.playerControllerInstance == null)
		{
			PlayerController.playerControllerInstance = GameHelper.FindObjectOfType(PlayerController.playerControllerType);
			_ = PlayerController.playerControllerInstance;
		}
	}

	public static void MaxStrength()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Expected O, but got Unknown
		Type type = Type.GetType("PlayerController, Assembly-CSharp");
		if (type == null)
		{
			return;
		}
		object obj = GameHelper.FindObjectOfType(type);
		if (obj == null)
		{
			return;
		}
		FieldInfo field = type.GetField("playerAvatarScript", BindingFlags.Instance | BindingFlags.Public);
		if (field == null)
		{
			return;
		}
		object value = field.GetValue(obj);
		if (value == null)
		{
			return;
		}
		FieldInfo field2 = value.GetType().GetField("physGrabber", BindingFlags.Instance | BindingFlags.Public);
		if (field2 == null)
		{
			return;
		}
		physGrabberInstance = field2.GetValue(value);
		if (physGrabberInstance != null)
		{
			FieldInfo field3 = physGrabberInstance.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public);
			if (field3 != null)
			{
				physGrabberPhotonView = (PhotonView)field3.GetValue(physGrabberInstance);
			}
			_ = (Object)(object)physGrabberPhotonView == (Object)null;
			Type type2 = Type.GetType("PunManager, Assembly-CSharp");
			object obj2 = GameHelper.FindObjectOfType(type2);
			if (obj2 != null)
			{
				punManagerPhotonView = (PhotonView)(type2.GetField("photonView", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj2));
				_ = (Object)(object)punManagerPhotonView == (Object)null;
			}
			ApplyGrabStrength();
			SetServerGrabStrength(Hax2.sliderValueStrength);
		}
	}

	private static void ApplyGrabStrength()
	{
		if (physGrabberInstance == null)
		{
			return;
		}
		UpgradeHelper.RebuildLocalGrabPhysics();
	}

	private static void ResetGrabbedObject()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		FieldInfo field = physGrabberInstance.GetType().GetField("grabbedObjectTransform", BindingFlags.Instance | BindingFlags.Public);
		Transform val = ((!(field != null)) ? ((Transform)null) : ((Transform)field.GetValue(physGrabberInstance)));
		if (!((Object)(object)val != (Object)null))
		{
			return;
		}
		PhysGrabObject component = ((Component)val).GetComponent<PhysGrabObject>();
		if ((Object)(object)component != (Object)null && (Object)(object)component.rb != (Object)null)
		{
			component.rb.velocity = Vector3.zero;
			component.rb.angularVelocity = Vector3.zero;
			PhotonView component2 = ((Component)component).GetComponent<PhotonView>();
			if ((Object)(object)component2 != (Object)null && !component2.IsMine && PhotonNetwork.IsConnected)
			{
				component2.RPC("ResetVelocityRPC", (RpcTarget)2, Array.Empty<object>());
			}
		}
	}

	public static void UpdateStrength()
	{
		if (physGrabberInstance == null)
		{
			return;
		}
		ApplyGrabStrength();
		if (Hax2.sliderValueStrength != lastAppliedStrength)
		{
			SetServerGrabStrength(Hax2.sliderValueStrength);
			lastAppliedStrength = Hax2.sliderValueStrength;
			lastStrengthUpdateTime = Time.time;
			if (Hax2.sliderValueStrength <= 1f)
			{
				ResetGrabbedObject();
			}
		}
	}

	public static void SetServerGrabStrength(float strength)
	{
		if (physGrabberInstance == null)
		{
			MaxStrength();
			if (physGrabberInstance == null)
			{
				return;
			}
		}
		if ((Object)(object)punManagerPhotonView == (Object)null)
		{
			return;
		}
		string text = SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarLocal());
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		if (PhotonNetwork.IsMasterClient)
		{
			PlayerAvatar val = SemiFunc.PlayerAvatarGetFromSteamID(text);
			if (!((Object)(object)val != (Object)null))
			{
				return;
			}
			val.physGrabber.grabStrength = UpgradeHelper.GameGrabStrength(Mathf.RoundToInt(strength));
			Type type = Type.GetType("PunManager, Assembly-CSharp");
			object obj = GameHelper.FindObjectOfType(type);
			if (obj == null)
			{
				return;
			}
			object obj2 = type.GetField("statsManager", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
			if (obj2 != null)
			{
				Dictionary<string, int> dictionary = (Dictionary<string, int>)(obj2.GetType().GetField("playerUpgradeStrength", BindingFlags.Instance | BindingFlags.Public)?.GetValue(obj2));
				if (dictionary != null)
				{
					int value = Mathf.RoundToInt(strength);
					dictionary[text] = value;
				}
			}
		}
		else
		{
			int num = Mathf.RoundToInt(strength);
			int currentUpgradeCount = GetCurrentUpgradeCount(text);
			if (num != currentUpgradeCount)
			{
				UpgradeHelper.SetLocalLevel("playerUpgradeStrength", num, (id, v) => PunManager.instance.UpgradePlayerGrabStrength(id, v));
			}
		}
	}

	private static int GetCurrentUpgradeCount(string steamID)
	{
		Type type = Type.GetType("PunManager, Assembly-CSharp");
		object obj = GameHelper.FindObjectOfType(type);
		if (obj != null)
		{
			object obj2 = type.GetField("statsManager", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
			if (obj2 != null)
			{
				Dictionary<string, int> dictionary = (Dictionary<string, int>)(obj2.GetType().GetField("playerUpgradeStrength", BindingFlags.Instance | BindingFlags.Public)?.GetValue(obj2));
				if (dictionary != null && dictionary.ContainsKey(steamID))
				{
					return dictionary[steamID];
				}
			}
		}
		return 0;
	}
}
