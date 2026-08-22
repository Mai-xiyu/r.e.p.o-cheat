using System;
using System.Reflection;
using HarmonyLib;
using Photon.Pun;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// Hosts through SemiFunc.MenuActionHostGame (save file + NetworkConnect prefab).
/// Pre-creating a Steam lobby and flipping joinLobby loads LobbyJoin and can stall on 加载中.
/// Photon room name stays the Steam lobby id; the typed name is stored as DisplayName.
/// Max player count is applied at Steam CreateLobby / Photon CreateRoom time only.
/// </summary>
public static class RoomCreator
{
	public const int MinPlayers = 2;
	public const int MaxPlayersCap = 20;

	public static bool IsCreating { get; private set; }

	public static string StatusText = "";

	public static bool KeepPublic;

	/// <summary>UI toggle. Default private, matching the game's Host Game path.</summary>
	public static bool CreatePublic;

	/// <summary>Desired lobby size. Must be set before hosting; locked once in a room.</summary>
	public static int MaxPlayers = 6;

	private static readonly BindingFlags InstAll = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static string _pendingDisplayName = "";
	private static int _pendingMaxPlayers;
	private static bool _hosting;

	public static int ClampMaxPlayers(int n)
	{
		return Mathf.Clamp(n, MinPlayers, MaxPlayersCap);
	}

	public static bool CanChangeMaxPlayers()
	{
		return !PhotonNetwork.InRoom && !IsCreating && !_hosting;
	}

	public static void SetMaxPlayers(int n)
	{
		MaxPlayers = ClampMaxPlayers(n);
	}

	public static void ApplyMaxPlayersToGame()
	{
		if (PhotonNetwork.InRoom || GameManager.instance == null)
		{
			return;
		}
		GameManager.instance.SetMaxPlayers(ClampMaxPlayers(MaxPlayers));
	}

	public static string GetPhotonRegion(int regionFilterIndex)
	{
		switch (regionFilterIndex)
		{
			case 1: return "asia";
			case 2: return "jp";
			case 3: return "eu";
			case 4: return "us";
			case 5: return "sa";
			case 6: return "au";
			case 7: return "za";
			case 8: return "uae";
			default: return "jp";
		}
	}

	public static void CreateRoom(string roomName, int maxPlayers, int regionFilterIndex)
	{
		if (IsCreating)
		{
			return;
		}
		if (MainMenuOpen.instance == null)
		{
			StatusText = L.T("room.need_menu");
			return;
		}
		maxPlayers = ClampMaxPlayers(maxPlayers);
		MaxPlayers = maxPlayers;
		string region = GetPhotonRegion(regionFilterIndex);
		try
		{
			SteamManager.instance?.LeaveLobby();
		}
		catch
		{
		}
		_pendingDisplayName = (roomName ?? "").Trim();
		_pendingMaxPlayers = maxPlayers;
		KeepPublic = CreatePublic;
		_hosting = true;
		IsCreating = true;
		StatusText = L.T("room.creating") + " [" + region + "]";
		try
		{
			if (GameManager.instance != null)
			{
				GameManager.instance.localTest = false;
				GameManager.instance.SetMaxPlayers(maxPlayers);
			}
			if (DataDirector.instance != null)
			{
				typeof(DataDirector).GetField("networkRegion", InstAll)?.SetValue(DataDirector.instance, region);
				DataDirector.instance.PhotonSetRegion();
			}
			ApplyHostVisibility();
			// MenuActionHostGame always writes Private + connectRandom=false, then Instantiates
			// NetworkConnect. Start() may run CreateLobby on the same stack (Photon already
			// disconnected) — visibility must already be applied, and Start prefix reapplies it.
			SemiFunc.MenuActionHostGame();
			ApplyHostVisibility();
			if (GameManager.instance != null)
			{
				GameManager.instance.SetMaxPlayers(maxPlayers);
			}
			StatusText = L.T("room.hosted_fmt", string.IsNullOrEmpty(_pendingDisplayName) ? region : _pendingDisplayName, region)
				+ "  " + L.T(CreatePublic ? "server.lobby_public" : "server.lobby_private");
		}
		catch (Exception ex)
		{
			KeepPublic = false;
			_hosting = false;
			_pendingDisplayName = "";
			StatusText = L.T("room.error") + " " + ex.Message;
			Debug.LogError("[RC] " + ex);
		}
		finally
		{
			IsCreating = false;
		}
	}

	internal static void ApplyHostVisibility()
	{
		if (!_hosting || GameManager.instance == null)
		{
			return;
		}
		if (KeepPublic)
		{
			string serverName = _pendingDisplayName;
			if (string.IsNullOrEmpty(serverName))
			{
				serverName = SteamClient.IsValid ? SteamClient.Name : "Room";
			}
			if (DataDirector.instance != null)
			{
				typeof(DataDirector).GetField("networkServerName", InstAll)?.SetValue(DataDirector.instance, serverName);
				typeof(DataDirector).GetField("networkPassword", InstAll)?.SetValue(DataDirector.instance, "");
			}
			GameManager.instance.SetConnectRandom(true);
			GameManager.instance.SetLobbyType(GameManager.LobbyTypes.Public);
			return;
		}
		GameManager.instance.SetConnectRandom(false);
		GameManager.instance.SetLobbyType(GameManager.LobbyTypes.Private);
	}

	internal static void ApplyPendingLobbyData(SteamManager manager)
	{
		Lobby lobby = GetCurrentLobby(manager);
		if (lobby.Id.Value == 0uL)
		{
			return;
		}
		try
		{
			int cap = _pendingMaxPlayers > 0 ? _pendingMaxPlayers : ClampMaxPlayers(MaxPlayers);
			if (cap > 0 && (_hosting || _pendingMaxPlayers > 0))
			{
				lobby.MaxMembers = cap;
				if (GameManager.instance != null)
				{
					GameManager.instance.SetMaxPlayers(cap);
				}
			}
			WriteLobbyNames(lobby);
			if (KeepPublic)
			{
				lobby.SetPublic();
				lobby.SetJoinable(true);
			}
			else if (MidJoin.Enabled)
			{
				lobby.SetJoinable(true);
			}
			RoomHistory.Capture(lobby);
			MidJoin.CaptureVisibility();
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[RC] apply lobby data: " + ex.Message);
		}
	}

	internal static void ClearPending()
	{
		KeepPublic = false;
		_hosting = false;
		_pendingDisplayName = "";
		_pendingMaxPlayers = 0;
	}

	internal static void ForcePublic(SteamManager manager)
	{
		if (!KeepPublic)
		{
			return;
		}
		try
		{
			Lobby lobby = GetCurrentLobby(manager);
			if (lobby.Id.Value == 0uL)
			{
				return;
			}
			lobby.SetPublic();
			lobby.SetJoinable(true);
			if (_pendingMaxPlayers > 0)
			{
				lobby.MaxMembers = _pendingMaxPlayers;
			}
			WriteLobbyNames(lobby);
		}
		catch
		{
		}
	}

	private static void WriteLobbyNames(Lobby lobby)
	{
		if (!string.IsNullOrEmpty(_pendingDisplayName))
		{
			lobby.SetData("DisplayName", _pendingDisplayName);
		}
		if (SteamClient.IsValid && !string.IsNullOrWhiteSpace(SteamClient.Name))
		{
			lobby.SetData("HostName", SteamClient.Name);
		}
	}

	private static Lobby GetCurrentLobby(SteamManager manager)
	{
		if (manager == null)
		{
			return default(Lobby);
		}
		object raw = typeof(SteamManager).GetField("currentLobby", InstAll)?.GetValue(manager);
		return raw is Lobby lobby ? lobby : default(Lobby);
	}
}

[HarmonyPatch(typeof(SemiFunc), "MenuActionHostGame")]
public static class RoomCreatorMenuActionHostGamePatch
{
	private static void Prefix()
	{
		RoomCreator.ApplyMaxPlayersToGame();
	}
}

[HarmonyPatch(typeof(NetworkConnect), "Start")]
public static class RoomCreatorNetworkConnectStartPatch
{
	private static void Prefix()
	{
		RoomCreator.ApplyMaxPlayersToGame();
		RoomCreator.ApplyHostVisibility();
	}
}

[HarmonyPatch(typeof(GameManager), "SetConnectRandom")]
public static class RoomCreatorSetConnectRandomPatch
{
	private static void Prefix(ref bool _connectRandom)
	{
		if (RoomCreator.KeepPublic)
		{
			_connectRandom = true;
		}
	}
}

[HarmonyPatch(typeof(GameManager), "SetLobbyType")]
public static class RoomCreatorSetLobbyTypePatch
{
	private static void Prefix(ref GameManager.LobbyTypes _lobbyType)
	{
		if (RoomCreator.KeepPublic)
		{
			_lobbyType = GameManager.LobbyTypes.Public;
		}
	}
}

[HarmonyPatch(typeof(SteamManager), "HostLobby")]
public static class RoomCreatorHostLobbyPatch
{
	private static void Prefix(ref bool _open)
	{
		RoomCreator.ApplyMaxPlayersToGame();
		if (RoomCreator.KeepPublic)
		{
			_open = true;
		}
	}
}

[HarmonyPatch(typeof(SteamManager), "SetLobbyData")]
public static class RoomCreatorSetLobbyDataPatch
{
	private static void Postfix(SteamManager __instance)
	{
		RoomCreator.ApplyPendingLobbyData(__instance);
	}
}

[HarmonyPatch(typeof(SteamManager), "UnlockLobby")]
public static class RoomCreatorUnlockLobbyPatch
{
	private static void Prefix(ref bool _open)
	{
		if (RoomCreator.KeepPublic)
		{
			_open = true;
		}
	}

	private static void Postfix(SteamManager __instance)
	{
		RoomCreator.ForcePublic(__instance);
	}
}

[HarmonyPatch(typeof(SteamManager), "LeaveLobby")]
public static class RoomCreatorLeaveLobbyPatch
{
	private static void Prefix()
	{
		RoomCreator.ClearPending();
	}
}
