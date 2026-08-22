using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json;
using Photon.Pun;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace r.e.p.o_cheat;

public static class RoomHistory
{
	public const int MaxEntries = 25;

	[Serializable]
	public class Entry
	{
		public string LobbyId = "";
		public string RoomName = "";
		public string Region = "";
		public string Host = "";
		public string PhotonName = "";
		public int Players;
		public int MaxPlayers;
		public long JoinedAt;
	}

	private static List<Entry> _entries;
	private static readonly object Gate = new object();

	private static string FilePath => Path.Combine(Application.persistentDataPath, "DarkCheat_RoomHistory.json");

	public static List<Entry> GetEntries()
	{
		EnsureLoaded();
		lock (Gate)
		{
			return new List<Entry>(_entries);
		}
	}

	public static void Clear()
	{
		EnsureLoaded();
		lock (Gate)
		{
			_entries.Clear();
			Save();
		}
	}

	public static void Capture(Lobby lobby)
	{
		if (lobby.Id.Value == 0uL)
		{
			return;
		}
		Entry entry = new Entry
		{
			LobbyId = lobby.Id.Value.ToString(),
			RoomName = LobbyFinder.GetRoomName(lobby),
			Region = LobbyFinder.GetRegion(lobby),
			Host = LobbyFinder.GetHostLabel(lobby),
			Players = lobby.MemberCount,
			MaxPlayers = lobby.MaxMembers,
			JoinedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
		};
		if (string.IsNullOrWhiteSpace(entry.RoomName))
		{
			entry.RoomName = entry.Host;
		}
		try
		{
			if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null)
			{
				entry.PhotonName = PhotonNetwork.CurrentRoom.Name ?? "";
			}
		}
		catch
		{
		}
		Upsert(entry);
	}

	public static void CapturePhoton()
	{
		try
		{
			if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
			{
				return;
			}
			ulong steamId = 0uL;
			try
			{
				if (SteamManager.instance != null)
				{
					FieldInfo field = typeof(SteamManager).GetField("currentLobby", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (field?.GetValue(SteamManager.instance) is Lobby cur && cur.Id.Value != 0uL)
					{
						steamId = cur.Id.Value;
						Capture(cur);
						return;
					}
				}
			}
			catch
			{
			}
			if (steamId != 0uL)
			{
				return;
			}
			Entry entry = new Entry
			{
				LobbyId = "",
				PhotonName = PhotonNetwork.CurrentRoom.Name ?? "",
				RoomName = PhotonNetwork.CurrentRoom.Name ?? "",
				Region = PhotonNetwork.CloudRegion ?? "",
				Players = PhotonNetwork.CurrentRoom.PlayerCount,
				MaxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers,
				JoinedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			};
			try
			{
				object display = PhotonNetwork.CurrentRoom.CustomProperties["server_name"];
				if (display is string s && !string.IsNullOrWhiteSpace(s))
				{
					entry.RoomName = s;
				}
			}
			catch
			{
			}
			Upsert(entry);
		}
		catch
		{
		}
	}

	public static void Rejoin(Entry entry)
	{
		if (entry == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(entry.LobbyId) && ulong.TryParse(entry.LobbyId, out ulong id) && id != 0uL)
		{
			SteamId sid = default(SteamId);
			sid.Value = id;
			LobbyFinder.JoinLobbyAndPlay(new Lobby(sid));
			return;
		}
		if (string.IsNullOrEmpty(entry.PhotonName) || DataDirector.instance == null || GameManager.instance == null || RunManager.instance == null)
		{
			return;
		}
		try
		{
			string region = string.IsNullOrWhiteSpace(entry.Region) ? "jp" : entry.Region.Trim().TrimEnd('/');
			typeof(DataDirector).GetField("networkJoinServerName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(DataDirector.instance, entry.PhotonName);
			typeof(DataDirector).GetField("networkRegion", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(DataDirector.instance, region);
			DataDirector.instance.PhotonSetRegion();
			GameManager.instance.localTest = false;
			GameManager.instance.SetConnectRandom(true);
			GameManager.instance.SetLobbyType(GameManager.LobbyTypes.Public);
			RunManager.instance.ResetProgress();
			typeof(RunManager).GetField("waitToChangeScene", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(RunManager.instance, true);
			typeof(RunManager).GetField("lobbyJoin", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(RunManager.instance, true);
			RunManager.instance.ChangeLevel(true, false, RunManager.ChangeLevelType.LobbyMenu);
		}
		catch (Exception ex)
		{
			Debug.LogError("[RoomHistory] rejoin: " + ex);
		}
	}

	private static void Upsert(Entry entry)
	{
		EnsureLoaded();
		lock (Gate)
		{
			string key = DedupKey(entry);
			RoomHistory.Entry previous = _entries.Find((Entry e) => DedupKey(e) == key);
			if (previous != null)
			{
				if (string.IsNullOrWhiteSpace(entry.RoomName))
				{
					entry.RoomName = previous.RoomName;
				}
				if (string.IsNullOrWhiteSpace(entry.Region))
				{
					entry.Region = previous.Region;
				}
				if (string.IsNullOrWhiteSpace(entry.Host))
				{
					entry.Host = previous.Host;
				}
				if (string.IsNullOrWhiteSpace(entry.PhotonName))
				{
					entry.PhotonName = previous.PhotonName;
				}
			}
			_entries.RemoveAll((Entry e) => DedupKey(e) == key);
			_entries.Insert(0, entry);
			while (_entries.Count > MaxEntries)
			{
				_entries.RemoveAt(_entries.Count - 1);
			}
			Save();
		}
	}

	private static string DedupKey(Entry e)
	{
		if (!string.IsNullOrEmpty(e.LobbyId))
		{
			return "s:" + e.LobbyId;
		}
		return "p:" + (e.PhotonName ?? "") + "@" + (e.Region ?? "");
	}

	private static void EnsureLoaded()
	{
		lock (Gate)
		{
			if (_entries != null)
			{
				return;
			}
			_entries = new List<Entry>();
			try
			{
				if (File.Exists(FilePath))
				{
					List<Entry> loaded = JsonConvert.DeserializeObject<List<Entry>>(File.ReadAllText(FilePath));
					if (loaded != null)
					{
						_entries = loaded;
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[RoomHistory] load: " + ex.Message);
				_entries = new List<Entry>();
			}
		}
	}

	private static void Save()
	{
		try
		{
			File.WriteAllText(FilePath, JsonConvert.SerializeObject(_entries, Formatting.Indented));
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[RoomHistory] save: " + ex.Message);
		}
	}
}

[HarmonyPatch(typeof(SteamManager), "OnLobbyEntered")]
public static class RoomHistoryEnteredPatch
{
	private static void Postfix(Lobby _lobby)
	{
		try
		{
			RoomHistory.Capture(_lobby);
		}
		catch
		{
		}
	}
}

[HarmonyPatch(typeof(NetworkConnect), "OnJoinedRoom")]
public static class RoomHistoryPhotonPatch
{
	private static void Postfix()
	{
		try
		{
			RoomHistory.CapturePhoton();
		}
		catch
		{
		}
	}
}
