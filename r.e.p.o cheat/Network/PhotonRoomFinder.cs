using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// Lists public Photon rooms the same way the game's server browser does:
/// connect → JoinLobby(custom) → OnRoomListUpdate → disconnect.
/// Steam lobby metadata is often empty (name/host show as 未知); public names live in server_name.
/// </summary>
public class PhotonRoomFinder : MonoBehaviourPunCallbacks
{
	public class RoomRow
	{
		public string DisplayName;

		public string RoomName;

		public int Players;

		public int MaxPlayers;

		public string Region;
	}

	public static readonly List<RoomRow> Rooms = new List<RoomRow>();

	public static bool IsRefreshing { get; private set; }

	public static int ListVersion { get; private set; }

	public static string StatusText = "";

	private static readonly BindingFlags InstAll = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private bool _waiting;

	private float _timeout;

	private string _region = "jp";

	private void Update()
	{
		if (!_waiting)
		{
			return;
		}
		_timeout -= Time.unscaledDeltaTime;
		if (_timeout > 0f)
		{
			return;
		}
		Finish(disconnect: true);
		StatusText = L.T("server.photon_timeout");
	}

	public static void Refresh(string region)
	{
		if (PhotonNetwork.InRoom)
		{
			StatusText = L.T("server.photon_in_room");
			return;
		}
		if (Object.FindObjectOfType<NetworkConnect>() != null)
		{
			StatusText = L.T("server.photon_busy");
			return;
		}
		PhotonRoomFinder finder = Ensure();
		if (finder == null || IsRefreshing)
		{
			return;
		}
		finder.StartCoroutine(finder.RefreshRoutine(region));
	}

	public static void Join(string photonRoomName, string region)
	{
		if (string.IsNullOrEmpty(photonRoomName) || DataDirector.instance == null || GameManager.instance == null || RunManager.instance == null)
		{
			return;
		}
		if (MainMenuOpen.instance == null)
		{
			StatusText = L.T("room.need_menu");
			return;
		}
		try
		{
			string useRegion = string.IsNullOrWhiteSpace(region) ? "jp" : region.Trim().TrimEnd('/');
			typeof(DataDirector).GetField("networkJoinServerName", InstAll)?.SetValue(DataDirector.instance, photonRoomName);
			typeof(DataDirector).GetField("networkRegion", InstAll)?.SetValue(DataDirector.instance, useRegion);
			DataDirector.instance.PhotonSetRegion();
			GameManager.instance.localTest = false;
			GameManager.instance.SetConnectRandom(true);
			GameManager.instance.SetLobbyType(GameManager.LobbyTypes.Public);
			RunManager.instance.ResetProgress();
			if (StatsManager.instance != null)
			{
				typeof(StatsManager).GetField("saveFileCurrent", InstAll)?.SetValue(StatsManager.instance, "");
			}
			typeof(RunManager).GetField("lobbyJoin", InstAll)?.SetValue(RunManager.instance, true);
			RunManager.instance.ChangeLevel(true, false, RunManager.ChangeLevelType.LobbyMenu);
		}
		catch (System.Exception ex)
		{
			Debug.LogError("[PhotonRoomFinder] join: " + ex);
			StatusText = L.T("room.error") + " " + ex.Message;
		}
	}

	private static PhotonRoomFinder Ensure()
	{
		if (Hax2.CoroutineHost == null)
		{
			return null;
		}
		PhotonRoomFinder finder = Hax2.CoroutineHost.GetComponent<PhotonRoomFinder>();
		if (finder == null)
		{
			finder = Hax2.CoroutineHost.gameObject.AddComponent<PhotonRoomFinder>();
		}
		return finder;
	}

	private IEnumerator RefreshRoutine(string region)
	{
		IsRefreshing = true;
		_waiting = false;
		_region = string.IsNullOrWhiteSpace(region) ? "jp" : region;
		StatusText = L.T("server.fetching") + " [" + _region + "]";
		Rooms.Clear();
		ListVersion++;
		if (DataDirector.instance != null)
		{
			typeof(DataDirector).GetField("networkRegion", InstAll)?.SetValue(DataDirector.instance, _region);
			DataDirector.instance.PhotonSetRegion();
			DataDirector.instance.PhotonSetVersion();
			DataDirector.instance.PhotonSetAppId();
		}
		try
		{
			SteamManager.instance?.SendSteamAuthTicket();
			PhotonNetwork.Disconnect();
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("[PhotonRoomFinder] " + ex.Message);
			Finish(disconnect: false);
			StatusText = L.T("room.error") + " " + ex.Message;
			yield break;
		}
		while (PhotonNetwork.NetworkingClient != null
			&& PhotonNetwork.NetworkingClient.State != ClientState.Disconnected
			&& PhotonNetwork.NetworkingClient.State != ClientState.PeerCreated)
		{
			yield return null;
		}
		_waiting = true;
		_timeout = 10f;
		PhotonNetwork.ConnectUsingSettings();
	}

	public override void OnConnectedToMaster()
	{
		if (!_waiting)
		{
			return;
		}
		TypedLobby lobby = DataDirector.instance != null ? DataDirector.instance.customLobby : new TypedLobby("custom", LobbyType.Default);
		PhotonNetwork.JoinLobby(lobby);
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		if (!_waiting || roomList == null)
		{
			return;
		}
		Rooms.Clear();
		for (int i = 0; i < roomList.Count; i++)
		{
			RoomInfo room = roomList[i];
			if (room == null || room.RemovedFromList || !room.IsOpen)
			{
				continue;
			}
			string display = ReadServerName(room);
			if (string.IsNullOrWhiteSpace(display))
			{
				display = room.Name;
			}
			if (string.IsNullOrWhiteSpace(display) || LobbyFinder.LooksLikeGuid(display))
			{
				continue;
			}
			Rooms.Add(new RoomRow
			{
				DisplayName = display,
				RoomName = room.Name,
				Players = room.PlayerCount,
				MaxPlayers = room.MaxPlayers,
				Region = _region
			});
		}
		StatusText = L.T("server.photon_count", Rooms.Count, _region);
		Finish(disconnect: true);
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		if (_waiting && cause != DisconnectCause.DisconnectByClientLogic)
		{
			StatusText = L.T("room.error") + " " + cause;
			Finish(disconnect: false);
		}
	}

	private void Finish(bool disconnect)
	{
		_waiting = false;
		IsRefreshing = false;
		ListVersion++;
		if (disconnect && !PhotonNetwork.InRoom)
		{
			try
			{
				PhotonNetwork.Disconnect();
			}
			catch
			{
			}
		}
	}

	private static string ReadServerName(RoomInfo room)
	{
		try
		{
			if (room.CustomProperties == null || !room.CustomProperties.ContainsKey("server_name"))
			{
				return "";
			}
			return room.CustomProperties["server_name"] as string ?? "";
		}
		catch
		{
			return "";
		}
	}
}
