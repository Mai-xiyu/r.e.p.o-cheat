using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace r.e.p.o_cheat;

public static class LobbyFinder
{
	public static List<Lobby> FoundLobbies { get; private set; } = new List<Lobby>();

	public static Lobby SelectedLobby { get; set; }

	public static bool IsRefreshing { get; private set; }

	public static int ListVersion { get; private set; }

	public static event Action OnLobbyListUpdated;

	public static string GetRoomName(Lobby lobby)
	{
		try
		{
			string display = lobby.GetData("DisplayName");
			if (IsHumanName(display))
			{
				return display;
			}
			string serverName = lobby.GetData("server_name");
			if (IsHumanName(serverName))
			{
				return serverName;
			}
			string hostName = lobby.GetData("HostName");
			if (IsHumanName(hostName))
			{
				return hostName;
			}
			foreach (KeyValuePair<string, string> pair in lobby.Data)
			{
				string key = pair.Key ?? "";
				if (key.Equals("Region", StringComparison.OrdinalIgnoreCase)
					|| key.Equals("BuildName", StringComparison.OrdinalIgnoreCase)
					|| key.Equals("HasPassword", StringComparison.OrdinalIgnoreCase)
					|| key.Equals("RoomName", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				if (IsHumanName(pair.Value))
				{
					return pair.Value;
				}
			}
			string room = lobby.GetData("RoomName");
			if (IsHumanName(room))
			{
				return room;
			}
			string host = GetHostLabel(lobby);
			if (IsHumanName(host) && host != "0")
			{
				return host;
			}
			if (!string.IsNullOrWhiteSpace(room) && !LooksLikeGuid(room))
			{
				return room;
			}
		}
		catch
		{
		}
		return string.Empty;
	}

	public static string GetRegion(Lobby lobby)
	{
		try
		{
			return lobby.GetData("Region") ?? string.Empty;
		}
		catch
		{
			return string.Empty;
		}
	}

	public static string GetHostLabel(Lobby lobby)
	{
		try
		{
			Friend owner = lobby.Owner;
			if (owner.Id.Value != 0uL)
			{
				string name = owner.Name;
				string id = owner.Id.ToString();
				if (!string.IsNullOrWhiteSpace(name) && name != "0")
				{
					return name + " (" + id + ")";
				}
				if (!string.IsNullOrWhiteSpace(id) && id != "0")
				{
					return id;
				}
			}
			string hostName = lobby.GetData("HostName");
			if (IsHumanName(hostName))
			{
				return hostName;
			}
		}
		catch
		{
		}
		return L.T("server.unknown");
	}

	public static bool LooksLikeGuid(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return false;
		}
		string s = value.Trim();
		if (s.Length < 32)
		{
			return false;
		}
		return Guid.TryParse(s, out _) || Guid.TryParse(s.Replace("-", ""), out _);
	}

	private static bool IsHumanName(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return false;
		}
		string s = value.Trim();
		if (s == "0" || s == "unknown")
		{
			return false;
		}
		if (LooksLikeGuid(s))
		{
			return false;
		}
		if (ulong.TryParse(s, out _))
		{
			return false;
		}
		return true;
	}

	public static bool IsListable(Lobby lobby)
	{
		if (IsHumanName(GetRoomName(lobby)))
		{
			return true;
		}
		string host = GetHostLabel(lobby);
		return IsHumanName(host) && host != L.T("server.unknown");
	}

	public static void RefreshLobbies(int maxResults = 100)
	{
		if (IsRefreshing || (UnityEngine.Object)Hax2.CoroutineHost == null)
		{
			return;
		}
		((MonoBehaviour)Hax2.CoroutineHost).StartCoroutine(RefreshLobbiesCoroutine(maxResults));
	}

	private static IEnumerator RefreshLobbiesCoroutine(int maxResults)
	{
		IsRefreshing = true;
		FoundLobbies.Clear();
		LobbyQuery query = SteamMatchmaking.LobbyList;
		query = query.WithMaxResults(maxResults);
		query = query.FilterDistanceWorldwide();
		Task<Lobby[]> requestTask = query.RequestAsync();
		while (!requestTask.IsCompleted)
		{
			yield return null;
		}
		if (requestTask.IsFaulted || requestTask.Result == null)
		{
			IsRefreshing = false;
			ListVersion++;
			LobbyFinder.OnLobbyListUpdated?.Invoke();
			yield break;
		}
		FoundLobbies.AddRange(requestTask.Result);
		int refreshCount = Math.Min(FoundLobbies.Count, 80);
		for (int i = 0; i < refreshCount; i++)
		{
			try
			{
				FoundLobbies[i].Refresh();
			}
			catch
			{
			}
		}
		float wait = 0f;
		while (wait < 1.25f)
		{
			wait += Time.unscaledDeltaTime;
			yield return null;
		}
		for (int j = 0; j < FoundLobbies.Count; j++)
		{
			Lobby lobby = FoundLobbies[j];
			Hax2.LobbyHostCache[lobby.Id] = GetHostLabel(lobby);
			List<string> members = new List<string>();
			try
			{
				foreach (Friend member in lobby.Members)
				{
					if (member.Id.Value == 0uL)
					{
						continue;
					}
					string n = string.IsNullOrWhiteSpace(member.Name) ? "Unknown" : member.Name;
					members.Add(n + " (" + member.Id + ")");
				}
			}
			catch
			{
			}
			Hax2.LobbyMemberCache[lobby.Id] = members;
		}
		ListVersion++;
		IsRefreshing = false;
		LobbyFinder.OnLobbyListUpdated?.Invoke();
		Debug.Log((object)("[LobbyFinder] listed " + FoundLobbies.Count + " steam lobbies"));
	}

	public static bool TryGetSelected(SteamId selectedId, out Lobby lobby)
	{
		lobby = default(Lobby);
		if (selectedId.Value == 0uL)
		{
			return false;
		}
		if (SelectedLobby.Id.Value == selectedId.Value)
		{
			lobby = SelectedLobby;
			return true;
		}
		lobby = FoundLobbies.Find((Lobby l) => l.Id.Value == selectedId.Value);
		return lobby.Id.Value != 0uL;
	}

	public static void JoinLobbyAndPlay(Lobby lobby)
	{
		if (lobby.Id.Value == 0uL || SteamManager.instance == null)
		{
			Debug.LogError((object)"[JoinLobby] invalid lobby");
			return;
		}
		SelectedLobby = lobby;
		try
		{
			typeof(SteamManager).GetMethod("JoinSteamLobby", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				?.Invoke(SteamManager.instance, new object[] { lobby });
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("[JoinLobby] " + ex));
		}
	}
}
