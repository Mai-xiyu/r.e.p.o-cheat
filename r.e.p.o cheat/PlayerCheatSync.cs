using System;
using System.Collections.Generic;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

public class PlayerCheatSync : MonoBehaviourPunCallbacks
{
	public class MirroredItemMarker : MonoBehaviour
	{
	}

	public PhotonView photonView;

	private void Awake()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		photonView = ((Component)this).GetComponent<PhotonView>();
		if ((Object)(object)photonView == (Object)null)
		{
			photonView = ((Component)this).gameObject.AddComponent<PhotonView>();
			photonView.ViewID = PhotonNetwork.AllocateViewID(0);
			photonView.Synchronization = (ViewSynchronization)3;
			photonView.ObservedComponents = new List<Component> { (Component)(object)this };
		}
	}

	[PunRPC]
	public void SpawnItemRPC(string itemName, Vector3 position, int value)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (PhotonNetwork.IsMasterClient)
		{
			ItemSpawner.SpawnItem(itemName, position, value);
		}
	}

	[PunRPC]
	public void SpawnItemMirrorRPC(string itemName, Vector3 position, int value, int requestingClientId)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}
		try
		{
			Vector3 val = position + new Vector3(0f, 0.001f, 0f);
			GameObject val2 = null;
			val2 = PhotonNetwork.InstantiateRoomObject(ItemSpawner.GetPrefabPath(itemName), val, Quaternion.identity, (byte)0, (object[])null);
			if (!((Object)(object)val2 != (Object)null))
			{
				return;
			}
			Component component = val2.GetComponent(Type.GetType("ValuableObject, Assembly-CSharp"));
			if ((Object)(object)component != (Object)null && value > 0)
			{
				MethodInfo method = ((object)component).GetType().GetMethod("DollarValueSetRPC", BindingFlags.Instance | BindingFlags.Public);
				if (method != null)
				{
					method.Invoke(component, new object[1] { (float)value });
				}
			}
			val2.AddComponent<MirroredItemMarker>();
			photonView.RPC("SetItemVisibilityRPC", PhotonNetwork.LocalPlayer.GetNext(), new object[2]
			{
				PunExtensions.GetPhotonView(val2).ViewID,
				false
			});
		}
		catch (Exception)
		{
		}
	}

	[PunRPC]
	public void SetItemVisibilityRPC(int viewID, bool visible)
	{
		PhotonView val = PhotonView.Find(viewID);
		if ((Object)(object)val != (Object)null)
		{
			Renderer[] componentsInChildren = ((Component)val).gameObject.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = visible;
			}
		}
	}
}
