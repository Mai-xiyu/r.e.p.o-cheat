using System;
using System.Collections.Generic;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

public static class Teleport
{
	public static void TeleportPlayerToMe(int selectedPlayerIndex, List<object> playerList, List<string> playerNames)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		if (selectedPlayerIndex < 0 || selectedPlayerIndex >= playerList.Count)
		{
			return;
		}
		object obj = playerList[selectedPlayerIndex];
		if (obj == null)
		{
			return;
		}
		try
		{
			GameObject localPlayer = DebugCheats.GetLocalPlayer();
			if ((Object)(object)localPlayer == (Object)null)
			{
				return;
			}
			FieldInfo field = obj.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				return;
			}
			object value = field.GetValue(obj);
			PhotonView val = (PhotonView)((value is PhotonView) ? value : null);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			MonoBehaviour val2 = (MonoBehaviour)((obj is MonoBehaviour) ? obj : null);
			if ((Object)(object)val2 == (Object)null)
			{
				return;
			}
			Transform transform = ((Component)val2).transform;
			if (!((Object)(object)transform == (Object)null))
			{
				Vector3 val3 = (transform.position = localPlayer.transform.position + Vector3.up * 1.5f);
				if (PhotonNetwork.IsConnected && (Object)(object)val != (Object)null)
				{
					val.RPC("SpawnRPC", (RpcTarget)3, new object[2] { val3, transform.rotation });
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public static void TeleportMeToPlayer(int selectedPlayerIndex, List<object> playerList, List<string> playerNames)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		if (selectedPlayerIndex < 0 || selectedPlayerIndex >= playerList.Count)
		{
			return;
		}
		object obj = playerList[selectedPlayerIndex];
		if (obj == null)
		{
			return;
		}
		try
		{
			GameObject localPlayer = DebugCheats.GetLocalPlayer();
			if ((Object)(object)localPlayer == (Object)null)
			{
				return;
			}
			PhotonView component = localPlayer.GetComponent<PhotonView>();
			if ((Object)(object)component == (Object)null)
			{
				return;
			}
			PhotonView val = component;
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			MonoBehaviour val2 = (MonoBehaviour)((obj is MonoBehaviour) ? obj : null);
			if ((Object)(object)val2 == (Object)null)
			{
				return;
			}
			Transform transform = ((Component)val2).transform;
			if (!((Object)(object)transform == (Object)null))
			{
				Vector3 val3 = transform.position + Vector3.up * 1.5f;
				localPlayer.transform.position = val3;
				if (PhotonNetwork.IsConnected && (Object)(object)val != (Object)null)
				{
					val.RPC("SpawnRPC", (RpcTarget)3, new object[2]
					{
						val3,
						localPlayer.transform.rotation
					});
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public static void SendSelectedPlayerToVoid(int selectedPlayerIndex, List<object> playerList, List<string> playerNames)
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		if (selectedPlayerIndex < 0 || selectedPlayerIndex >= playerList.Count)
		{
			return;
		}
		object obj = playerList[selectedPlayerIndex];
		if (obj == null)
		{
			return;
		}
		try
		{
			FieldInfo field = obj.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				return;
			}
			object value = field.GetValue(obj);
			PhotonView val = (PhotonView)((value is PhotonView) ? value : null);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			MonoBehaviour val2 = (MonoBehaviour)((obj is MonoBehaviour) ? obj : null);
			if ((Object)(object)val2 == (Object)null)
			{
				return;
			}
			Transform transform = ((Component)val2).transform;
			if (!((Object)(object)transform == (Object)null))
			{
				Vector3 val3 = new Vector3(0f, -10f, 0f);
				transform.position = val3;
				if (PhotonNetwork.IsConnected && (Object)(object)val != (Object)null)
				{
					val.RPC("SpawnRPC", (RpcTarget)3, new object[2] { val3, transform.rotation });
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public static void ExecuteTeleportWithIndices(int sourceIndex, int destIndex, string[] teleportOptions, List<object> playerList)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (sourceIndex == destIndex)
			{
				return;
			}
			object obj = null;
			_ = teleportOptions[sourceIndex];
			if (sourceIndex < teleportOptions.Length - 1 && sourceIndex >= 0 && sourceIndex < playerList.Count)
			{
				obj = playerList[sourceIndex];
			}
			if (obj == null)
			{
				return;
			}
			_ = teleportOptions[destIndex];
			Vector3 val;
			if (destIndex == teleportOptions.Length - 1)
			{
				val = new Vector3(0f, -10f, 0f);
			}
			else
			{
				if (destIndex < 0 || destIndex >= playerList.Count)
				{
					return;
				}
				object obj2 = playerList[destIndex];
				MonoBehaviour val2 = (MonoBehaviour)((obj2 is MonoBehaviour) ? obj2 : null);
				if ((Object)(object)val2 == (Object)null)
				{
					return;
				}
				val = ((Component)val2).transform.position + Vector3.up * 1.5f;
			}
			object obj3 = obj;
			bool flag = false;
			GameObject val3 = (GameObject)((obj3 is GameObject) ? obj3 : null);
			if (val3 != null)
			{
				flag = (Object)(object)val3 == (Object)(object)DebugCheats.GetLocalPlayer();
			}
			else
			{
				MonoBehaviour val4 = (MonoBehaviour)((obj3 is MonoBehaviour) ? obj3 : null);
				if (val4 != null)
				{
					flag = (Object)(object)((Component)val4).gameObject == (Object)(object)DebugCheats.GetLocalPlayer();
				}
			}
			if (flag)
			{
				GameObject localPlayer = DebugCheats.GetLocalPlayer();
				PhotonView component = localPlayer.GetComponent<PhotonView>();
				localPlayer.transform.position = val;
				if (PhotonNetwork.IsConnected && (Object)(object)component != (Object)null)
				{
					component.RPC("SpawnRPC", (RpcTarget)3, new object[2]
					{
						val,
						localPlayer.transform.rotation
					});
				}
				return;
			}
			FieldInfo field = obj3.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				return;
			}
			object value = field.GetValue(obj3);
			PhotonView val5 = (PhotonView)((value is PhotonView) ? value : null);
			if ((Object)(object)val5 == (Object)null)
			{
				return;
			}
			MonoBehaviour val6 = (MonoBehaviour)((obj3 is MonoBehaviour) ? obj3 : null);
			if ((Object)(object)val6 == (Object)null)
			{
				return;
			}
			Transform transform = ((Component)val6).transform;
			if (!((Object)(object)transform == (Object)null))
			{
				transform.position = val;
				if (PhotonNetwork.IsConnected && (Object)(object)val5 != (Object)null)
				{
					val5.RPC("SpawnRPC", (RpcTarget)3, new object[2] { val, transform.rotation });
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public static void ExecuteTeleportWithSeparateOptions(int sourceIndex, int destIndex, string[] sourceOptions, string[] destOptions, List<object> playerList)
	{
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (sourceIndex == 0)
			{
				TeleportAllPlayers(destIndex, destOptions, playerList);
			}
			else
			{
				if (sourceIndex < 1 || sourceIndex - 1 >= playerList.Count || destIndex < 0 || destIndex >= destOptions.Length)
				{
					return;
				}
				int num = sourceIndex - 1;
				object obj = null;
				_ = sourceOptions[sourceIndex];
				if (num < playerList.Count)
				{
					obj = playerList[num];
				}
				if (obj == null)
				{
					return;
				}
				_ = destOptions[destIndex];
				Vector3 val;
				if (destIndex == destOptions.Length - 1)
				{
					val = new Vector3(0f, -10f, 0f);
				}
				else
				{
					if (destIndex < 0 || destIndex >= playerList.Count)
					{
						return;
					}
					object obj2 = playerList[destIndex];
					MonoBehaviour val2 = (MonoBehaviour)((obj2 is MonoBehaviour) ? obj2 : null);
					if ((Object)(object)val2 == (Object)null)
					{
						return;
					}
					val = ((Component)val2).transform.position + Vector3.up * 1.5f;
				}
				object obj3 = obj;
				if (CheckIfPlayerIsLocal(obj3))
				{
					GameObject localPlayer = DebugCheats.GetLocalPlayer();
					PhotonView component = localPlayer.GetComponent<PhotonView>();
					localPlayer.transform.position = val;
					if (PhotonNetwork.IsConnected && (Object)(object)component != (Object)null)
					{
						component.RPC("SpawnRPC", (RpcTarget)3, new object[2]
						{
							val,
							localPlayer.transform.rotation
						});
					}
					return;
				}
				FieldInfo field = obj3.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field == null)
				{
					return;
				}
				object value = field.GetValue(obj3);
				PhotonView val3 = (PhotonView)((value is PhotonView) ? value : null);
				if ((Object)(object)val3 == (Object)null)
				{
					return;
				}
				MonoBehaviour val4 = (MonoBehaviour)((obj3 is MonoBehaviour) ? obj3 : null);
				if ((Object)(object)val4 == (Object)null)
				{
					return;
				}
				Transform transform = ((Component)val4).transform;
				if (!((Object)(object)transform == (Object)null))
				{
					transform.position = val;
					if (PhotonNetwork.IsConnected && (Object)(object)val3 != (Object)null)
					{
						val3.RPC("SpawnRPC", (RpcTarget)3, new object[2] { val, transform.rotation });
					}
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private static bool CheckIfPlayerIsLocal(object playerObj)
	{
		if (playerObj == null)
		{
			return false;
		}
		GameObject localPlayer = DebugCheats.GetLocalPlayer();
		if ((Object)(object)localPlayer == (Object)null)
		{
			return false;
		}
		GameObject val = (GameObject)((playerObj is GameObject) ? playerObj : null);
		if (val != null)
		{
			return (Object)(object)val == (Object)(object)localPlayer;
		}
		MonoBehaviour val2 = (MonoBehaviour)((playerObj is MonoBehaviour) ? playerObj : null);
		if (val2 != null)
		{
			return (Object)(object)((Component)val2).gameObject == (Object)(object)localPlayer;
		}
		return false;
	}

	private static void TeleportAllPlayers(int destIndex, string[] destOptions, List<object> playerList)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		if (destIndex < 0 || destIndex >= destOptions.Length)
		{
			return;
		}
		Vector3 val;
		if (destIndex == destOptions.Length - 1)
		{
			val = new Vector3(0f, -10f, 0f);
		}
		else
		{
			if (destIndex >= playerList.Count)
			{
				return;
			}
			object obj = playerList[destIndex];
			if (obj == null)
			{
				return;
			}
			MonoBehaviour val2 = (MonoBehaviour)((obj is MonoBehaviour) ? obj : null);
			if ((Object)(object)val2 == (Object)null)
			{
				return;
			}
			val = ((Component)val2).transform.position + Vector3.up * 1.5f;
			_ = destOptions[destIndex];
		}
		int num = 0;
		for (int i = 0; i < playerList.Count; i++)
		{
			if (destIndex < destOptions.Length - 1 && i == destIndex)
			{
				continue;
			}
			object obj2 = playerList[i];
			if (obj2 == null)
			{
				continue;
			}
			try
			{
				FieldInfo field = obj2.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field == null)
				{
					continue;
				}
				object value = field.GetValue(obj2);
				PhotonView val3 = (PhotonView)((value is PhotonView) ? value : null);
				if ((Object)(object)val3 == (Object)null)
				{
					continue;
				}
				MonoBehaviour val4 = (MonoBehaviour)((obj2 is MonoBehaviour) ? obj2 : null);
				if ((Object)(object)val4 == (Object)null)
				{
					continue;
				}
				Transform transform = ((Component)val4).transform;
				if (!((Object)(object)transform == (Object)null))
				{
					transform.position = val;
					if (PhotonNetwork.IsConnected && (Object)(object)val3 != (Object)null)
					{
						val3.RPC("SpawnRPC", (RpcTarget)3, new object[2] { val, transform.rotation });
					}
					num++;
					CheckIfPlayerIsLocal(obj2);
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
