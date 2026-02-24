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
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		if (physGrabberInstance == null)
		{
			return;
		}
		FieldInfo field = physGrabberInstance.GetType().GetField("grabStrength", BindingFlags.Instance | BindingFlags.Public);
		if (field != null && (float)field.GetValue(physGrabberInstance) != Hax2.sliderValueStrength)
		{
			field.SetValue(physGrabberInstance, Hax2.sliderValueStrength);
			if (Hax2.sliderValueStrength <= 1f)
			{
				ResetGrabbedObject();
			}
		}
		FieldInfo field2 = physGrabberInstance.GetType().GetField("grabbed", BindingFlags.Instance | BindingFlags.Public);
		bool flag = field2 != null && (bool)field2.GetValue(physGrabberInstance);
		if (!lastGrabbedState.HasValue || flag != lastGrabbedState)
		{
			lastGrabbedState = flag;
		}
		if (!flag)
		{
			return;
		}
		FieldInfo field3 = physGrabberInstance.GetType().GetField("grabbedObjectTransform", BindingFlags.Instance | BindingFlags.Public);
		Transform val = ((!(field3 != null)) ? ((Transform)null) : ((Transform)field3.GetValue(physGrabberInstance)));
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		PhysGrabObject component = ((Component)val).GetComponent<PhysGrabObject>();
		if ((Object)(object)component == (Object)null)
		{
			return;
		}
		Rigidbody rb = component.rb;
		if ((Object)(object)rb == (Object)null)
		{
			return;
		}
		FieldInfo field4 = physGrabberInstance.GetType().GetField("physGrabPoint", BindingFlags.Instance | BindingFlags.Public);
		Transform val2 = ((!(field4 != null)) ? ((Transform)null) : ((Transform)field4.GetValue(physGrabberInstance)));
		if ((Object)(object)val2 == (Object)null)
		{
			return;
		}
		FieldInfo field5 = physGrabberInstance.GetType().GetField("physGrabPointPullerPosition", BindingFlags.Instance | BindingFlags.Public);
		Vector3 val3 = (Vector3)((field5 != null) ? ((Vector3)field5.GetValue(physGrabberInstance)) : Vector3.zero);
		if (val3 == Vector3.zero && field5 == null)
		{
			return;
		}
		Vector3 val4 = val3 - val2.position;
		Vector3 normalized = val4.normalized;
		float num = Hax2.sliderValueStrength * 50000f;
		FieldInfo field6 = ((object)component).GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.NonPublic);
		PhotonView val5 = ((!(field6 != null)) ? ((PhotonView)null) : ((PhotonView)field6.GetValue(component)));
		if (!((Object)(object)val5 != (Object)null))
		{
			return;
		}
		if (!val5.IsMine)
		{
			val5.RequestOwnership();
		}
		if (val5.IsMine)
		{
			rb.AddForceAtPosition(normalized * num, val2.position, (ForceMode)0);
			return;
		}
		if (PhotonNetwork.IsMasterClient)
		{
			rb.AddForceAtPosition(normalized * num, val2.position, (ForceMode)0);
			return;
		}
		val5.RPC("ApplyExtraForceRPC", (RpcTarget)2, new object[3] { normalized, num, val2.position });
		if (Hax2.sliderValueStrength == lastAppliedStrength)
		{
			rb.AddForceAtPosition(normalized * num, val2.position, (ForceMode)0);
		}
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
			val.physGrabber.grabStrength = strength;
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
				punManagerPhotonView.RPC("UpgradePlayerGrabStrengthRPC", (RpcTarget)2, new object[2] { text, num });
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
