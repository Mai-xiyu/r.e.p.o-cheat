using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

public static class ChatHijack
{
	private static Dictionary<object, string> originalPlayerNames = new Dictionary<object, string>();

	private static bool isSpoofingActive = false;

	public static void MakeChat(string message, string targetName, List<object> playerList, List<string> playerNames)
	{
		// 聊天命令拦截: 如果以 ! 或 / 开头则作为命令处理
		if (!string.IsNullOrEmpty(message) && (message.StartsWith("!") || message.StartsWith("/")))
		{
			if (ChatCommands.TryExecuteCommand(message))
				return; // 命令已处理，不发送聊天消息
		}

		for (int i = 0; i < playerList.Count; i++)
		{
			object obj = playerList[i];
			string text = playerNames[i];
			if (targetName != "All" && text != targetName)
			{
				continue;
			}
			FieldInfo field = obj.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (!(field == null))
			{
				object value = field.GetValue(obj);
				PhotonView val = (PhotonView)((value is PhotonView) ? value : null);
				if (!((Object)(object)val == (Object)null))
				{
					val.RPC("ChatMessageSendRPC", (RpcTarget)0, new object[2] { message, false });
				}
			}
		}
	}

	private static string StripStatusTags(string name)
	{
		return Regex.Replace(name, "\\[(LIVE|DEAD)\\]\\s*", "");
	}

	public static void ToggleNameSpoofing(bool enable, string spoofName, string targetName, List<object> playerList, List<string> playerNames)
	{
		if (enable)
		{
			if (!isSpoofingActive)
			{
				StoreOriginalNames(playerList, playerNames);
				isSpoofingActive = true;
			}
			SendCustomNameRPC(spoofName, targetName, playerList, playerNames);
		}
		else
		{
			RestoreOriginalNames(targetName, playerList, playerNames);
			isSpoofingActive = false;
		}
	}

	private static void StoreOriginalNames(List<object> playerList, List<string> playerNames)
	{
		for (int i = 0; i < playerList.Count; i++)
		{
			if (!originalPlayerNames.ContainsKey(playerList[i]))
			{
				string value = StripStatusTags(playerNames[i]);
				originalPlayerNames[playerList[i]] = value;
			}
		}
	}

	private static void RestoreOriginalNames(string targetName, List<object> playerList, List<string> playerNames)
	{
		for (int i = 0; i < playerList.Count; i++)
		{
			object obj = playerList[i];
			string name = playerNames[i];
			string text = StripStatusTags(targetName);
			string text2 = StripStatusTags(name);
			if ((text != "All" && text2 != text) || !originalPlayerNames.ContainsKey(obj))
			{
				continue;
			}
			string text3 = originalPlayerNames[obj];
			FieldInfo field = obj.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (!(field == null))
			{
				object value = field.GetValue(obj);
				PhotonView val = (PhotonView)((value is PhotonView) ? value : null);
				if (!((Object)(object)val == (Object)null))
				{
					val.RPC("AddToStatsManagerRPC", (RpcTarget)3, new object[2] { text3, "472644" });
				}
			}
		}
	}

	public static void SendCustomNameRPC(string spoofName, string targetName, List<object> playerList, List<string> playerNames)
	{
		if (playerList == null || playerNames == null || playerList.Count != playerNames.Count)
		{
			return;
		}
		string text = StripStatusTags(targetName);
		for (int i = 0; i < playerList.Count; i++)
		{
			string name = playerNames[i];
			object obj = playerList[i];
			string text2 = StripStatusTags(name);
			if (text != "All" && text2 != text)
			{
				continue;
			}
			FieldInfo field = obj.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (!(field == null))
			{
				object value = field.GetValue(obj);
				PhotonView val = (PhotonView)((value is PhotonView) ? value : null);
				if (!((Object)(object)val == (Object)null))
				{
					val.RPC("AddToStatsManagerRPC", (RpcTarget)3, new object[2] { spoofName, "472644" });
				}
			}
		}
	}

	public static void ChangePlayerColor(int colorIndex, string targetName, List<object> playerList, List<string> playerNames)
	{
		if (playerList == null || playerNames == null || playerList.Count != playerNames.Count)
		{
			return;
		}
		string text = StripStatusTags(targetName);
		for (int i = 0; i < playerList.Count; i++)
		{
			string name = playerNames[i];
			object obj = playerList[i];
			string text2 = StripStatusTags(name);
			if (text != "All" && text2 != text)
			{
				continue;
			}
			FieldInfo field = obj.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (!(field == null))
			{
				object value = field.GetValue(obj);
				PhotonView val = (PhotonView)((value is PhotonView) ? value : null);
				if (!((Object)(object)val == (Object)null))
				{
					val.RPC("SetColorRPC", (RpcTarget)3, new object[1] { colorIndex });
				}
			}
		}
	}

	public static void ClearStoredNames()
	{
		originalPlayerNames.Clear();
		isSpoofingActive = false;
	}

	/// <summary>
	/// 仅修改本地玩家自己的名称（立即生效）
	/// </summary>
	public static bool SpoofLocalPlayerName(string newName)
	{
		try
		{
			if (string.IsNullOrEmpty(newName)) return false;

			List<PlayerAvatar> players = SemiFunc.PlayerGetList();
			if (players == null) return false;

			foreach (PlayerAvatar player in players)
			{
				FieldInfo field = ((object)player).GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field == null) continue;

				object value = field.GetValue(player);
				PhotonView pv = (PhotonView)((value is PhotonView) ? value : null);
				if ((Object)(object)pv == (Object)null || !pv.IsMine) continue;

				pv.RPC("AddToStatsManagerRPC", (RpcTarget)3, new object[2] { newName, "472644" });
				return true;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("SpoofLocalPlayerName 失败: " + ex.Message));
		}
		return false;
	}
}
