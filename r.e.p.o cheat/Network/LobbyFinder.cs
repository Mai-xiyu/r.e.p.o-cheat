using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Photon.Pun;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace r.e.p.o_cheat;

public static class LobbyFinder
{
	public static HashSet<SteamId> AlreadyTriedLobbies = new HashSet<SteamId>();

	public static List<Lobby> FoundLobbies { get; private set; } = new List<Lobby>();

	public static Lobby SelectedLobby { get; set; }

	public static bool IsRefreshing { get; private set; }

	public static event Action OnLobbyListUpdated;

	public static void RefreshLobbies(int maxResults = 100)
	{
		if (!IsRefreshing)
		{
			Debug.Log((object)"开始协程：RefreshLobbiesCoroutine");
			((MonoBehaviour)Hax2.CoroutineHost).StartCoroutine(RefreshLobbiesCoroutine(maxResults));
		}
	}

	private static IEnumerator RefreshLobbiesCoroutine(int maxResults)
	{
		IsRefreshing = true;
		FoundLobbies.Clear();
		AlreadyTriedLobbies.Clear();
		LobbyQuery val = SteamMatchmaking.LobbyList;
		val = val.WithMaxResults(maxResults);
		val = val.FilterDistanceWorldwide();
		Task<Lobby[]> requestTask = val.RequestAsync();
		while (!requestTask.IsCompleted)
		{
			yield return null;
		}
		if (requestTask.IsFaulted || requestTask.Result == null)
		{
			IsRefreshing = false;
			LobbyFinder.OnLobbyListUpdated?.Invoke();
			yield break;
		}
		Lobby[] result = requestTask.Result;
		Debug.Log((object)$"[LobbyFinder] 找到 {result.Length} 个房间。");
		FoundLobbies.AddRange(result);
		yield return ((MonoBehaviour)Hax2.CoroutineHost).StartCoroutine(FakeJoinAndFetchLobbies(result));
		IsRefreshing = false;
		LobbyFinder.OnLobbyListUpdated?.Invoke();
	}

	private static IEnumerator FakeJoinAndFetchLobbies(Lobby[] lobbies)
	{
		int maxJoins = 50;
		int joinCount = 0;
		float timeout = 5f;
		for (int i = 0; i < lobbies.Length; i++)
		{
			Lobby lobby = lobbies[i];
			if (AlreadyTriedLobbies.Contains(lobby.Id))
			{
				continue;
			}
			AlreadyTriedLobbies.Add(lobby.Id);
			Hax2.LobbyHostCache[lobby.Id] = "Fetching...";
			Task<RoomEnter> joinTask = lobby.Join();
			float elapsed = 0f;
			while (!joinTask.IsCompleted && elapsed < timeout)
			{
				yield return null;
				elapsed += Time.deltaTime;
			}
			if (!joinTask.IsCompleted)
			{
				Debug.LogWarning((object)("[LobbyFinder] 加入房间超时：" + ((object)lobby.Id/*cast due to .constrained prefix*/).ToString()));
				continue;
			}
			if ((int)joinTask.Result == 1)
			{
				Friend owner = lobby.Owner;
				string name = owner.Name;
				if (lobby.Owner.Id.Value == 0L || string.IsNullOrWhiteSpace(name))
				{
					Debug.LogWarning((object)("[LobbyFinder] 跳过无效房间：" + ((object)lobby.Id/*cast due to .constrained prefix*/).ToString()));
					lobby.Leave();
					continue;
				}
				owner = lobby.Owner;
				string text = owner.Id.ToString();
				Hax2.LobbyHostCache[lobby.Id] = name + " (" + text + ")";
				List<string> list = new List<string>();
				foreach (Friend member in lobby.Members)
				{
					Friend current = member;
					string arg = (string.IsNullOrWhiteSpace(current.Name) ? "Unknown" : current.Name);
					list.Add($"{arg} ({current.Id})");
				}
				list.RemoveAll((string m) => m.Contains(SteamClient.Name) || m.Contains(((object)SteamClient.SteamId/*cast due to .constrained prefix*/).ToString()));
				Hax2.LobbyMemberCache[lobby.Id] = list;
				lobby.Leave();
			}
			else
			{
				Hax2.LobbyHostCache[lobby.Id] = $"Failed ({lobby.Owner.Id})";
			}
			joinCount++;
			if (joinCount >= maxJoins)
			{
				break;
			}
			yield return (object)new WaitForSeconds(0.15f);
		}
		GC.Collect();
		Debug.Log((object)"[LobbyFinder] 已完成对所有房间的假加入与信息抓取。");
	}

	public static async void JoinLobbyAndPlay(Lobby lobby)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Debug.Log((object)$"[JoinLobby] 正在尝试加入：{lobby.Id}");
			if ((int)(await lobby.Join()) == 1)
			{
				Debug.Log((object)"[JoinLobby] 加入房间成功。");
				MenuManager.instance.PageCloseAll();
				MenuManager.instance.PageOpen((MenuPageIndex)0, false);
				if ((Object)(object)RunManager.instance.levelCurrent != (Object)(object)RunManager.instance.levelMainMenu)
				{
					foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
					{
						player.OutroStartRPC(default(PhotonMessageInfo));
					}
					typeof(RunManager).GetField("lobbyJoin", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(RunManager.instance, true);
					RunManager.instance.ChangeLevel(true, false, (RunManager.ChangeLevelType)3);
				}
				typeof(SteamManager).GetField("joinLobby", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(SteamManager.instance, true);
			}
			else
			{
				Debug.LogError((object)"[JoinLobby] 加入房间失败。");
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("[JoinLobby] 异常：" + ex));
		}
	}
}
