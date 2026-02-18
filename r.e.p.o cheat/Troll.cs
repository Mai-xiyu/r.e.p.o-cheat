using System;
using System.Collections;
using System.Reflection;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace r.e.p.o_cheat;

internal static class Troll
{
	public static void InfiniteLoadingSelectedPlayer()
	{
		try
		{
			if (Hax2.selectedPlayerIndex < 0 || Hax2.selectedPlayerIndex >= Hax2.playerList.Count)
			{
				Debug.Log((object)"玩家索引无效！");
				return;
			}
			object obj = Hax2.playerList[Hax2.selectedPlayerIndex];
			if (obj == null)
			{
				Debug.Log((object)"所选玩家为空！");
				return;
			}
			Debug.Log((object)("正在尝试让 " + Hax2.playerNames[Hax2.selectedPlayerIndex] + " 进入无限加载界面……"));
			FieldInfo field = obj.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				Debug.Log((object)"未找到 PhotonView 字段！");
				return;
			}
			object value = field.GetValue(obj);
			PhotonView val = (PhotonView)((value is PhotonView) ? value : null);
			if ((Object)(object)val == (Object)null)
			{
				Debug.Log((object)"PhotonView 为 null！");
				return;
			}
			if (val.Owner == null)
			{
				Debug.Log((object)"无法从 PhotonView 获取 Photon 玩家信息！");
				return;
			}
			int actorNumber = PhotonNetwork.MasterClient.ActorNumber;
			FieldInfo field2 = typeof(Player).GetField("actorNumber", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field2 != null)
			{
				field2.SetValue(PhotonNetwork.LocalPlayer, actorNumber);
				val.RPC("OutroStartRPC", (RpcTarget)0, Array.Empty<object>());
				field2.SetValue(PhotonNetwork.LocalPlayer, PhotonNetwork.LocalPlayer.ActorNumber);
			}
		}
		catch (Exception ex)
		{
			Debug.Log((object)("让 " + Hax2.playerNames[Hax2.selectedPlayerIndex] + " 进入无限加载时出错：" + ex.Message));
		}
	}

	public static void SceneRecovery()
	{
		Debug.Log((object)"[恢复] === 开始场景恢复 ===");
		SceneManager.LoadScene("LobbyJoin", (LoadSceneMode)0);
		Debug.Log((object)"[恢复] 已加载 LobbyJoin 场景。");
		((MonoBehaviour)Hax2.CoroutineHost).StartCoroutine(LoadReloadSceneAfterDelay());
	}

	private static IEnumerator LoadReloadSceneAfterDelay()
	{
		yield return (object)new WaitForSeconds(3f);
		SceneManager.LoadScene("Reload", (LoadSceneMode)0);
		Debug.Log((object)"[恢复] 已加载 Reload 场景。");
		yield return (object)new WaitForSeconds(0.5f);
		PhotonNetwork.Disconnect();
		Debug.Log((object)"[恢复] === 场景恢复完成 ===");
	}

	public static void ForcePlayerGlitch()
	{
		if (Hax2.selectedPlayerIndex < 0 || Hax2.selectedPlayerIndex >= Hax2.playerList.Count)
		{
			Debug.Log((object)"玩家索引无效！");
			return;
		}
		object obj = Hax2.playerList[Hax2.selectedPlayerIndex];
		if (obj == null)
		{
			Debug.Log((object)"所选玩家为空！");
			return;
		}
		try
		{
			Debug.Log((object)("正在强制 " + Hax2.playerNames[Hax2.selectedPlayerIndex] + " 触发 Glitch。"));
			FieldInfo field = obj.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				Debug.Log((object)"未找到 PhotonView 字段！");
				return;
			}
			object value = field.GetValue(obj);
			PhotonView val = (PhotonView)((value is PhotonView) ? value : null);
			if ((Object)(object)val == (Object)null)
			{
				Debug.Log((object)"PhotonView 无效！");
				return;
			}
			val.RPC("PlayerGlitchShortRPC", (RpcTarget)0, Array.Empty<object>());
			Debug.Log((object)("已强制 " + Hax2.playerNames[Hax2.selectedPlayerIndex] + " 触发 Glitch。"));
		}
		catch (Exception ex)
		{
			Debug.Log((object)("强制 " + Hax2.playerNames[Hax2.selectedPlayerIndex] + " 触发 Glitch 时出错：" + ex.Message));
		}
	}
}
