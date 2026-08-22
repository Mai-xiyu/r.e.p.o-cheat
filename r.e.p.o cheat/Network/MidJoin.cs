using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// Keeps the Steam lobby joinable after start.
/// Active-level late join has a deterministic historical-state gap: the vanilla
/// LoadingLevelAnimationCompletedRPC is OwnerOnly and sent to RpcTarget.All, not AllBuffered,
/// while LoadingUI waits for every PlayerAvatar.levelAnimationCompleted flag. A newly joined
/// receiver therefore starts with at least one old avatar flag false and cannot cross the
/// barrier naturally. Player count changes only how many flags are absent.
///
/// Host-only compatibility uses a receiver-local PUN OwnershipUpdate relay. Only the joining
/// actor temporarily sees one old PlayerAvatar view as master-owned, the host replays the
/// vanilla loading-complete RPC, and the original owner is restored before moving to the next
/// view. Room-wide ownership, Photon event senders and other receivers are never changed.
/// Every historical avatar is replayed unconditionally; the host reflection flag is diagnostic.
///
/// The catch-up pipeline follows the vanilla generation barriers for map construction, then
/// releases the joining client's own loading barrier with an owner-authenticated RPC.  The
/// original game only fires that RPC from a UI animation which itself waits for every avatar;
/// a late receiver can therefore deadlock before its own animation is ever allowed to finish.
/// The local bootstrap runs only for a client that joined an already-running room, waits for
/// Generated, and calls the same public PlayerAvatar method the UI event would call.
/// </summary>
public static class MidJoin
{
	public static bool Enabled;

	private const string RunProp = "MJ";
	// Capability and ready fields are deliberately separate from MJ.Diag.  The host can
	// distinguish a receiver that is merely still loading from one whose injected client has
	// reached Generated and emitted its own owner-authenticated completion RPC.
	private const string PeerProtocolProp = "MJ.PeerProtocol";
	private const string PeerReadyProp = "MJ.PeerReady";
	private const string PeerModuleCountProp = "MJ.PeerModules";
	private const string PeerStageProp = "MJ.PeerStage";
	// Version 3 adds the receiver-side, LoadingLevelAnimationCompletedRPC-only
	// master relay. Do not treat a version-2 client as capable: it lacks that
	// narrow bypass and must continue through the legacy ownership relay.
	private const int PeerProtocolVersion = 3;
	// Player custom properties are cached by Photon and visible to the host.  This
	// is deliberately diagnostics-only: the joining client remains authoritative
	// for its own local LoadingUI state.
	private const string DiagnosticProp = "MJ.Diag";
	private const float DiagnosticPublishInterval = 0.75f;

	private static readonly FieldInfo LobbyTypeField = AccessTools.Field(typeof(GameManager), "lobbyType");
	private static readonly FieldInfo ConnectRandomField = AccessTools.Field(typeof(GameManager), "connectRandom");
	private static readonly FieldInfo PlayerAnimDone = AccessTools.Field(typeof(PlayerAvatar), "levelAnimationCompleted");
	private static readonly FieldInfo LoadingUiLevelDone = AccessTools.Field(typeof(LoadingUI), "levelAnimationCompleted");
	private static readonly FieldInfo LoadingUiLevelStarted = AccessTools.Field(typeof(LoadingUI), "levelAnimationStarted");
	private static readonly FieldInfo CosmeticsFirstSetup = AccessTools.Field(typeof(PlayerCosmetics), "firstSetup");
	private static readonly FieldInfo CosmeticsColors = AccessTools.Field(typeof(PlayerCosmetics), "colorsEquipped");
	private static readonly FieldInfo MetaSaveReady = AccessTools.Field(typeof(MetaManager), "saveReady");
	private static readonly FieldInfo PlayerIsLocal = AccessTools.Field(typeof(PlayerAvatar), "isLocal");
	private static readonly FieldInfo PlayerSpawned = AccessTools.Field(typeof(PlayerAvatar), "spawned");
	private static readonly FieldInfo PlayerName = AccessTools.Field(typeof(PlayerAvatar), "playerName");
	private static readonly FieldInfo PlayerSteamId = AccessTools.Field(typeof(PlayerAvatar), "steamID");
	private static readonly FieldInfo PlayerNameUi = AccessTools.Field(typeof(PlayerAvatar), "worldSpaceUIPlayerName");
	private static readonly FieldInfo PlayerVoiceChatField = AccessTools.Field(typeof(PlayerAvatar), "voiceChat");
	private static readonly FieldInfo PlayerDeathHeadField = AccessTools.Field(typeof(PlayerAvatar), "playerDeathHead");
	private static readonly FieldInfo PlayerTumbleField = AccessTools.Field(typeof(PlayerAvatar), "tumble");
	private static readonly FieldInfo EnemyParentEnemyField = AccessTools.Field(typeof(EnemyParent), "Enemy");
	private static readonly FieldInfo RunManagerPunField = AccessTools.Field(typeof(RunManager), "runManagerPUN");
	private static readonly FieldInfo RunGameOverField = AccessTools.Field(typeof(RunManager), "gameOver");
	private static readonly FieldInfo OutroDoneField = AccessTools.Field(typeof(PlayerAvatar), "outroDone");
	private static readonly FieldInfo ModTop = AccessTools.Field(typeof(Module), "ConnectingTop");
	private static readonly FieldInfo ModBottom = AccessTools.Field(typeof(Module), "ConnectingBottom");
	private static readonly FieldInfo ModRight = AccessTools.Field(typeof(Module), "ConnectingRight");
	private static readonly FieldInfo ModLeft = AccessTools.Field(typeof(Module), "ConnectingLeft");
	private static readonly FieldInfo ModFirst = AccessTools.Field(typeof(Module), "First");
	private static readonly FieldInfo ModDone = AccessTools.Field(typeof(Module), "SetupDone");
	private static readonly FieldInfo LevelModuleAmount = AccessTools.Field(typeof(LevelGenerator), "ModuleAmount");
	private static readonly FieldInfo ValSet = AccessTools.Field(typeof(ValuableObject), "dollarValueSet");
	private static readonly FieldInfo ValCurrent = AccessTools.Field(typeof(ValuableObject), "dollarValueCurrent");
	private static readonly FieldInfo ValDisc = AccessTools.Field(typeof(ValuableObject), "discovered");
	private static readonly FieldInfo ItemValue = AccessTools.Field(typeof(ItemAttributes), "value");
	private static readonly FieldInfo ExtractionCurrentState = AccessTools.Field(typeof(ExtractionPoint), "currentState");
	private static readonly MethodInfo OwnershipUpdateMethod = typeof(PhotonNetwork).GetMethod(
		"OwnershipUpdate",
		BindingFlags.Static | BindingFlags.NonPublic,
		null,
		new[] { typeof(int[]), typeof(int) },
		null);
	private const byte ExtractionLockEvent = 171;

	private static bool _publicSnapshot;
	private static float _nextPhotonPulse;
	private static float _nextUnstick;
	private static float _nextDiagnosticPublish;
	private static bool _localLateJoinPending;
	private static bool _localLoadingComplete;
	private static bool _localLateJoinBootstrapRunning;
	private static bool _localLateJoinBootstrapDone;
	private static bool _localForcedOwnCompletion;
	private static string _localBootstrapStage = "idle";
	private static int _localBootstrapModuleCount = -1;
	private static int _localIdentityResendCount;
	private static int _localPhysicsRepairs;
	private static bool _postBarrierDiagnosticPending;
	private static string _lastDiagnosticBody;
	private static string _lastLocalBarrierBlocker;
	private static string _lastCompletionRpc = "none";
	private static int _diagnosticRevision;
	private static int _completionRpcReceived;
	private static int _completionRpcApplied;
	private static int _completionRpcUnchanged;
	private static int _completionRpcMasterRelayed;
	private static int _remoteAnimationFlagsRepaired;
	private static bool _transitionLock;
	private static int _unlockGen;
	private static readonly HashSet<int> JoiningActors = new HashSet<int>();
	private static readonly HashSet<int> PendingActors = new HashSet<int>();
	private static readonly HashSet<int> SpawnedRpcActors = new HashSet<int>();
	private static readonly HashSet<int> ModulesReadyActors = new HashSet<int>();
	private static readonly HashSet<int> LevelSpawnedActors = new HashSet<int>();
	private static readonly HashSet<int> RunningPipelines = new HashSet<int>();
	private static readonly HashSet<int> NeedsLateJoinSpawn = new HashSet<int>();
	private static readonly HashSet<int> SpawnCompleted = new HashSet<int>();
	private static readonly HashSet<int> GenerateDoneSentActors = new HashSet<int>();
	private static readonly Dictionary<int, float> PendingSince = new Dictionary<int, float>();
	private static readonly Dictionary<int, int> CatchupAttempts = new Dictionary<int, int>();
	private static readonly Dictionary<int, float> RetryAfter = new Dictionary<int, float>();
	private static readonly HashSet<int> CompletionRetryActors = new HashSet<int>();
	private static readonly HashSet<int> PlayableActors = new HashSet<int>();
	private static readonly HashSet<int> ActivityArmedActors = new HashSet<int>();
	private static readonly Dictionary<int, Vector3> ActivityOrigins = new Dictionary<int, Vector3>();
	private static readonly Dictionary<int, float> ActivityWatchStarted = new Dictionary<int, float>();
	private static readonly Dictionary<int, float> ActivityArmedAt = new Dictionary<int, float>();
	private static readonly float[] CompletionReplaySchedule = { 0f, 0.35f, 1f, 2f, 3.5f, 5.5f };
	private const float CompletionRecoveryTimeout = 6.5f;
	private const float ForceOwnCompletionAfter = 0.35f;
	private const float ActivityWatchTimeout = 30f;
	private const float ActivityHorizontalDistance = 0.45f;
	// Actors that entered through MidJoin but have not yet proven a complete
	// loading handshake.  This set intentionally survives AbortAllCatchups()
	// so scene-transition cleanup can ignore their incomplete outro state.
	private static readonly HashSet<int> TransitionUnsafeActors = new HashSet<int>();
	private static float _restartSceneWaitSince;
	private static bool _restartSceneForceLogged;
	private static bool _photonEventHooked;
	private static bool _applyingRemoteExtractionLock;
	private static int _preparedGenerateDoneViewId;
	private static int _preparedAllPlayersReadyViewId;
	private const int MaxCatchupAttempts = 3;
	private const float ModulesReadyTimeout = 20f;
	private const float PlayerSpawnedTimeout = 12f;
	private const float OwnerLoadingTimeout = 20f;
	private const float LocalGeneratedTimeout = 35f;
	private const float LegacyOwnershipMappingSettleSeconds = 1.5f;

	public static bool TransitionLocked => _transitionLock;

	public sealed class ActorJoinStatus
	{
		public int Actor;
		public string Name;
		public bool InRoom;
		public bool HasAvatar;
		public bool SpawnedRpc;
		public bool ModulesReady;
		public bool SpawnSent;
		public bool GenerateDone;
		public bool OwnerLoadingReady;
		public bool Complete;
		public bool Running;
		public float WaitSeconds;
		public string RemoteDiagnostic;
		public bool RemoteReady;
	}

	public static void Apply()
	{
		if (Enabled)
		{
			CaptureVisibility();
			if (!_transitionLock)
			{
				Open();
			}
			EnsurePhotonEventHook();
			return;
		}
		_transitionLock = false;
		AbortAllCatchups();
		TransitionUnsafeActors.Clear();
		_restartSceneWaitSince = 0f;
		_restartSceneForceLogged = false;
		if (!InWaitingLobby())
		{
			Close();
		}
	}

	public static void Tick()
	{
		if (!PhotonNetwork.InRoom)
		{
			_localLateJoinPending = false;
			_localLoadingComplete = false;
			ResetLocalBootstrapState();
			_transitionLock = false;
			AbortAllCatchups();
			TransitionUnsafeActors.Clear();
			_restartSceneWaitSince = 0f;
			_restartSceneForceLogged = false;
			_preparedGenerateDoneViewId = 0;
			_preparedAllPlayersReadyViewId = 0;
			ResetLocalDiagnostics();
			return;
		}
		if (Enabled)
		{
			EnsurePhotonEventHook();
		}
		PublishLocalDiagnostic();
		ForgetFinishedJoiners();
		if (!Enabled || !PhotonNetwork.IsMasterClient)
		{
			return;
		}
		if (_transitionLock)
		{
			if (InWaitingLobby())
			{
				ReleaseWaitingLobbyLock("waiting lobby");
			}
			else if (Time.unscaledTime >= _nextPhotonPulse)
			{
				_nextPhotonPulse = Time.unscaledTime + 1.5f;
				CloseRoomOnly();
			}
			return;
		}
		if (Time.unscaledTime < _nextPhotonPulse)
		{
			return;
		}
		_nextPhotonPulse = Time.unscaledTime + 1.5f;
		TryStartPendingCatchups();
		if (NeedsLateJoinSpawn.Count > 0)
		{
			TrySpawnLateJoiners();
		}
		Open();
	}

	internal static void CaptureVisibility()
	{
		_publicSnapshot = DetectPublicLobby();
	}

	internal static void OnLocalJoinedRoom()
	{
		// The room property is part of the join response. Capture it once here:
		// a normal lobby client must not become a "late joiner" merely because
		// the host marks the room as in-run when starting the next level.
		_localLateJoinPending = RoomInRun();
		_nextUnstick = 0f;
		_localLoadingComplete = false;
		ResetLocalDiagnostics();
		ResetLocalBootstrapState();
		PublishPeerState("joined", ready: false, moduleCount: -1);
		AbortAllCatchups();
		TransitionUnsafeActors.Clear();
		_restartSceneWaitSince = 0f;
		_restartSceneForceLogged = false;
		_preparedGenerateDoneViewId = 0;
		_preparedAllPlayersReadyViewId = 0;
		Debug.Log("[MJ.DIAG] joined room actor=" + LocalActorNumber() +
			" roomRun=" + (_localLateJoinPending ? "1" : "0") +
			" localEnabled=" + (Enabled ? "1" : "0") +
			" localRepair=" + (IsLocalLateJoinRepairActive() ? "1" : "0") +
			" state=" + CurrentGameState());
		PublishLocalDiagnostic(true);
	}

	internal static void OnLevelGeneratorStarted()
	{
		_localLoadingComplete = false;
		_nextUnstick = 0f;
		_preparedGenerateDoneViewId = 0;
		_preparedAllPlayersReadyViewId = 0;
		ModulesReadyActors.Clear();
		LevelSpawnedActors.Clear();
		GenerateDoneSentActors.Clear();
		PlayableActors.Clear();
		ActivityArmedActors.Clear();
		ActivityOrigins.Clear();
		ActivityWatchStarted.Clear();
		ActivityArmedAt.Clear();
		_lastLocalBarrierBlocker = null;
		_nextDiagnosticPublish = 0f;
		if (IsLocalLateJoinRepairActive())
		{
			Debug.Log("[MJ.DIAG] LevelGenerator.Start actor=" + LocalActorNumber() +
				" state=" + CurrentGameState() + " localEnabled=" + (Enabled ? "1" : "0"));
			PublishLocalDiagnostic(true);
		}
	}

	// MidJoin.Enabled is a host-side room-control preference stored in each
	// client's local config.  It is not a replicated setting.  Once a client has
	// joined a room already marked as in-run, its local LoadingUI repair must not
	// depend on that unrelated host toggle being enabled locally.
	private static bool IsLocalLateJoinRepairActive()
	{
		return PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient && _localLateJoinPending;
	}

	internal static bool ShouldTrackLocalLoadingCompletion()
	{
		return Enabled || IsLocalLateJoinRepairActive();
	}

	private static int LocalActorNumber()
	{
		Player local = PhotonNetwork.LocalPlayer;
		return local != null ? local.ActorNumber : 0;
	}

	private static string CurrentGameState()
	{
		GameDirector director = GameDirector.instance;
		return director != null ? director.currentState.ToString() : "none";
	}

	private static void ResetLocalDiagnostics()
	{
		_nextDiagnosticPublish = 0f;
		_lastDiagnosticBody = null;
		_lastLocalBarrierBlocker = null;
		_lastCompletionRpc = "none";
		_postBarrierDiagnosticPending = false;
		_diagnosticRevision = 0;
		_completionRpcReceived = 0;
		_completionRpcApplied = 0;
		_completionRpcUnchanged = 0;
		_completionRpcMasterRelayed = 0;
		_remoteAnimationFlagsRepaired = 0;
	}

	private static void ResetLocalBootstrapState()
	{
		_localLateJoinBootstrapRunning = false;
		_localLateJoinBootstrapDone = false;
		_localForcedOwnCompletion = false;
		_localBootstrapStage = "idle";
		_localBootstrapModuleCount = -1;
		_localIdentityResendCount = 0;
		_localPhysicsRepairs = 0;
	}

	private static void PublishPeerState(string stage, bool ready, int moduleCount)
	{
		if (!PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null)
		{
			return;
		}

		try
		{
			ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
			{
				{ PeerProtocolProp, PeerProtocolVersion },
				{ PeerReadyProp, ready ? System.DateTime.UtcNow.Ticks : 0L },
				{ PeerModuleCountProp, moduleCount },
				{ PeerStageProp, stage ?? string.Empty }
			};
			if (!PhotonNetwork.LocalPlayer.SetCustomProperties(properties))
			{
				Debug.LogWarning("[MJ.DIAG] peer state was not queued actor=" + LocalActorNumber() + " stage=" + stage);
			}
			else
			{
				Debug.Log("[MJ.DIAG] peer state actor=" + LocalActorNumber() + " protocol=" + PeerProtocolVersion +
					" ready=" + (ready ? "1" : "0") + " modules=" + moduleCount + " stage=" + stage);
			}
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("[MJ.DIAG] peer state publish failed actor=" + LocalActorNumber() +
				" stage=" + stage + ": " + ex.GetType().Name + ": " + ex.Message);
		}
	}

	private static void PublishLocalDiagnostic(bool force = false)
	{
		if (!PhotonNetwork.InRoom || PhotonNetwork.IsMasterClient ||
			(!_localLateJoinPending && !_postBarrierDiagnosticPending && !force))
		{
			return;
		}
		if (!force && Time.unscaledTime < _nextDiagnosticPublish)
		{
			return;
		}
		_nextDiagnosticPublish = Time.unscaledTime + DiagnosticPublishInterval;

		Player local = PhotonNetwork.LocalPlayer;
		if (local == null)
		{
			return;
		}

		string body = BuildLocalDiagnostic();
		if (!force && string.Equals(body, _lastDiagnosticBody, StringComparison.Ordinal))
		{
			return;
		}
		_diagnosticRevision++;
		string payload = "r=" + _diagnosticRevision + ";" + body;
		if (payload.Length > 900)
		{
			payload = payload.Substring(0, 900) + ";cut=1";
		}

		try
		{
			ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
			{
				{ DiagnosticProp, payload }
			};
			if (local.SetCustomProperties(properties))
			{
				_lastDiagnosticBody = body;
				if (_postBarrierDiagnosticPending && CurrentGameState() == GameDirector.gameState.Main.ToString())
				{
					_postBarrierDiagnosticPending = false;
				}
				Debug.Log("[MJ.DIAG] local " + payload);
			}
			else
			{
				Debug.LogWarning("[MJ.DIAG] diagnostic property was not queued actor=" + LocalActorNumber());
			}
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("[MJ.DIAG] diagnostic publish failed: " + ex.GetType().Name + ": " + ex.Message);
		}
	}

	private static string BuildLocalDiagnostic()
	{
		GameDirector director = GameDirector.instance;
		LevelGenerator generator = LevelGenerator.Instance;
		LoadingUI loadingUi = LoadingUI.instance;
		string uiActive = "0";
		if (loadingUi != null)
		{
			try
			{
				uiActive = loadingUi.gameObject.activeInHierarchy ? "1" : "0";
			}
			catch
			{
				uiActive = "!";
			}
		}

		List<string> avatars = new List<string>();
		string localCosmetics = "none";
		if (director?.PlayerList != null)
		{
			for (int i = 0; i < director.PlayerList.Count; i++)
			{
				PlayerAvatar avatar = director.PlayerList[i];
				if (avatar == null)
				{
					avatars.Add("null");
					continue;
				}
				PhotonView view = avatar.photonView;
				int owner = GetActorNumber(view != null ? view.Owner : null);
				int viewId = view != null ? view.ViewID : 0;
				string mine = view != null && view.IsMine ? "1" : "0";
				if (mine == "1" || ReadDiagnosticBool(PlayerIsLocal, avatar) == "1")
				{
					localCosmetics = DescribeCosmetics(avatar.playerCosmetics);
				}
				avatars.Add(owner + "@" + viewId + ":d" + ReadDiagnosticBool(PlayerAnimDone, avatar) +
					"l" + ReadDiagnosticBool(PlayerIsLocal, avatar) +
					"s" + ReadDiagnosticBool(PlayerSpawned, avatar) + "m" + mine +
					"p" + DescribeAvatarPhysics(avatar));
			}
		}

		return "late=" + (_localLateJoinPending ? "1" : "0") +
			";cfg=" + (Enabled ? "1" : "0") +
			";repair=" + (IsLocalLateJoinRepairActive() ? "1" : "0") +
			";state=" + CurrentGameState() +
			";gen=" + (generator != null && generator.Generated ? "1" : "0") +
			";own=" + (_localLoadingComplete ? "1" : "0") +
			";boot=" + _localBootstrapStage + "/" + (_localLateJoinBootstrapRunning ? "1" : "0") +
			"/" + (_localLateJoinBootstrapDone ? "1" : "0") + "/" + (_localForcedOwnCompletion ? "1" : "0") +
			"/m" + _localBootstrapModuleCount + "/id" + _localIdentityResendCount + "/phy" + _localPhysicsRepairs +
			";meta=" + ReadDiagnosticBool(MetaSaveReady, MetaManager.instance) +
			";ui=" + uiActive + "/" + ReadDiagnosticBool(LoadingUiLevelStarted, loadingUi) +
			"/" + ReadDiagnosticBool(LoadingUiLevelDone, loadingUi) +
			";rpc=" + _completionRpcReceived + "/" + _completionRpcApplied + "/" + _completionRpcUnchanged +
			"/m" + _completionRpcMasterRelayed +
			";fix=" + _remoteAnimationFlagsRepaired +
			";cos=" + localCosmetics +
			";last=" + _lastCompletionRpc +
			";players=" + string.Join("|", avatars.ToArray());
	}

	private static string ReadDiagnosticBool(FieldInfo field, object instance)
	{
		if (field == null || instance == null)
		{
			return "?";
		}
		try
		{
			return field.GetValue(instance) is bool value ? (value ? "1" : "0") : "?";
		}
		catch
		{
			return "!";
		}
	}

	private static bool TryReadBool(FieldInfo field, object instance, out bool value)
	{
		value = false;
		if (field == null || instance == null)
		{
			return false;
		}
		try
		{
			if (field.GetValue(instance) is bool result)
			{
				value = result;
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private static int GetActorNumber(Player player)
	{
		return player != null ? player.ActorNumber : 0;
	}

	private static string DescribeAvatarPhysics(PlayerAvatar avatar)
	{
		if (avatar == null)
		{
			return "?";
		}
		try
		{
			Rigidbody body = avatar.GetComponent<Rigidbody>();
			Collider hitbox = avatar.GetComponentInChildren<Collider>(true);
			string kinematic = body == null ? "?" : (body.isKinematic ? "1" : "0");
			string collider = hitbox == null ? "?" : (hitbox.enabled ? "1" : "0");
			return "k" + kinematic + "c" + collider;
		}
		catch
		{
			return "!";
		}
	}

	private static string DescribeCosmetics(PlayerCosmetics cosmetics)
	{
		if (cosmetics == null)
		{
			return "none";
		}
		try
		{
			string first = ReadDiagnosticBool(CosmeticsFirstSetup, cosmetics);
			int valid = 0;
			int total = 0;
			if (CosmeticsColors?.GetValue(cosmetics) is int[] colors)
			{
				total = colors.Length;
				for (int i = 0; i < colors.Length; i++)
				{
					if (colors[i] >= 0)
					{
						valid++;
					}
				}
			}
			return "f" + first + "c" + valid + "/" + total;
		}
		catch
		{
			return "!";
		}
	}

	internal static void HandlePlayerEntered(Player newPlayer)
	{
		if (!Enabled || newPlayer == null || newPlayer.IsLocal || !PhotonNetwork.InRoom)
		{
			return;
		}

		int actor = newPlayer.ActorNumber;
		if (_transitionLock)
		{
			// Photon already accepted this actor while a scene change is in
			// flight.  Keep them in the unsafe set so RestartScene / outro
			// does not wait on an incomplete late joiner.
			JoiningActors.Add(actor);
			TransitionUnsafeActors.Add(actor);
			Debug.Log("[MJ] actor=" + actor + " entered during transition lock; marked unsafe");
			return;
		}

		if (InWaitingLobby())
		{
			return;
		}

		JoiningActors.Add(actor);
		TransitionUnsafeActors.Add(actor);
		PendingActors.Add(actor);
		PendingSince[actor] = Time.unscaledTime;
		if (!SpawnCompleted.Contains(actor))
		{
			NeedsLateJoinSpawn.Add(actor);
		}
		Debug.Log("[MJ] actor=" + actor + " entered");
	}

	internal static void HandlePlayerLeft(Player otherPlayer)
	{
		if (otherPlayer == null)
		{
			return;
		}
		int actor = otherPlayer.ActorNumber;
		JoiningActors.Remove(actor);
		PendingActors.Remove(actor);
		SpawnedRpcActors.Remove(actor);
		RunningPipelines.Remove(actor);
		NeedsLateJoinSpawn.Remove(actor);
		SpawnCompleted.Remove(actor);
		PendingSince.Remove(actor);
		CompletionRetryActors.Remove(actor);
		PlayableActors.Remove(actor);
		ActivityArmedActors.Remove(actor);
		ActivityOrigins.Remove(actor);
		ActivityWatchStarted.Remove(actor);
		ActivityArmedAt.Remove(actor);
		TransitionUnsafeActors.Remove(actor);
		ModulesReadyActors.Remove(actor);
		LevelSpawnedActors.Remove(actor);
		GenerateDoneSentActors.Remove(actor);
		CatchupAttempts.Remove(actor);
		RetryAfter.Remove(actor);
	}

	internal static void HandlePlayerSpawnedRpc(Player sender)
	{
		if (sender == null)
		{
			return;
		}
		int actor = sender.ActorNumber;
		SpawnedRpcActors.Add(actor);
		if (!Enabled || !PhotonNetwork.IsMasterClient || _transitionLock || InWaitingLobby() || sender.IsLocal)
		{
			return;
		}
		if (!PendingActors.Contains(actor))
		{
			return;
		}
		Debug.Log("[MJ] actor=" + actor + " PlayerSpawnedRPC received");
		StartCatchup(sender);
	}

	internal static void HandleModulesReadyRpc(Player sender)
	{
		if (!Enabled || !PhotonNetwork.IsMasterClient || sender == null || sender.IsLocal)
		{
			return;
		}
		int actor = sender.ActorNumber;
		if (JoiningActors.Contains(actor) && ModulesReadyActors.Add(actor))
		{
			Debug.Log("[MJ] actor=" + actor + " ModulesReadyRPC received");
		}
	}

	internal static void HandleLevelSpawnedRpc(Player sender)
	{
		if (!Enabled || !PhotonNetwork.IsMasterClient || sender == null || sender.IsLocal)
		{
			return;
		}
		int actor = sender.ActorNumber;
		if (JoiningActors.Contains(actor) && LevelSpawnedActors.Add(actor))
		{
			Debug.Log("[MJ] actor=" + actor + " LevelGenerator.PlayerSpawnedRPC received");
		}
	}

	internal static void HandleLateJoinAvatarStarted(PlayerAvatar avatar)
	{
		PhotonView view = avatar != null ? avatar.photonView : null;
		Player owner = view != null ? view.Owner : null;
		if (view != null && view.IsMine && IsLocalLateJoinRepairActive())
		{
			StartLocalLateJoinBootstrap(avatar);
		}
		if (!Enabled || !PhotonNetwork.IsMasterClient || owner == null || owner.IsLocal)
		{
			return;
		}
		int actor = owner.ActorNumber;
		if (!JoiningActors.Contains(actor) && !PendingActors.Contains(actor))
		{
			return;
		}
		RegisterLateJoinerSubsystems(avatar);
		EnsureLateJoinSupportObjects(avatar);
	}

	private static void StartLocalLateJoinBootstrap(PlayerAvatar avatar)
	{
		if (avatar == null || avatar.photonView == null || !avatar.photonView.IsMine ||
			_localLateJoinBootstrapRunning || _localLateJoinBootstrapDone)
		{
			return;
		}

		_localLateJoinBootstrapRunning = true;
		_localBootstrapStage = "wait-generated";
		PublishPeerState("wait-generated", ready: false, moduleCount: -1);
		Debug.Log("[MJ.DIAG] local bootstrap begin actor=" + LocalActorNumber() +
			" view=" + avatar.photonView.ViewID + " state=" + CurrentGameState());
		Loader.RunCoroutine(LocalLateJoinBootstrap(avatar.photonView.ViewID));
	}

	private static IEnumerator LocalLateJoinBootstrap(int expectedViewId)
	{
		try
		{
			float generatedDeadline = Time.unscaledTime + LocalGeneratedTimeout;
			while (Time.unscaledTime < generatedDeadline)
			{
				if (!IsLocalLateJoinRepairActive())
				{
					_localBootstrapStage = "cancelled";
					yield break;
				}
				LevelGenerator generator = LevelGenerator.Instance;
				if (generator != null && generator.Generated)
				{
					break;
				}
				yield return null;
			}

			LevelGenerator readyGenerator = LevelGenerator.Instance;
			if (readyGenerator == null || !readyGenerator.Generated)
			{
				_localBootstrapStage = "generated-timeout";
				Debug.LogWarning("[MJ.DIAG] local bootstrap timeout waiting for Generated actor=" + LocalActorNumber() +
					" state=" + CurrentGameState());
				PublishPeerState("generated-timeout", ready: false, moduleCount: -1);
				yield break;
			}

			_localBootstrapStage = "wait-avatar";
			PlayerAvatar localAvatar = null;
			float avatarDeadline = Time.unscaledTime + 4f;
			while (Time.unscaledTime < avatarDeadline)
			{
				if (!IsLocalLateJoinRepairActive())
				{
					_localBootstrapStage = "cancelled";
					yield break;
				}
				localAvatar = FindLocalAvatar(expectedViewId);
				if (localAvatar != null)
				{
					break;
				}
				yield return null;
			}
			if (localAvatar == null)
			{
				_localBootstrapStage = "avatar-timeout";
				Debug.LogWarning("[MJ.DIAG] local bootstrap timeout waiting for own avatar actor=" + LocalActorNumber() +
					" expectedView=" + expectedViewId);
				PublishPeerState("avatar-timeout", ready: false, moduleCount: -1);
				yield break;
			}

			// The native GenerateDone path creates a large number of components in the
			// same frame.  Let Awake/Start queues drain before touching cosmetics or
			// emitting the owner-authenticated completion RPC.
			yield return null;
			yield return new WaitForSecondsRealtime(0.15f);

			_localBootstrapStage = "refresh-1";
			int cosmeticsFirst = RefreshLateJoinCosmetics(localAvatar, syncLocal: true);
			int voiceFirst = RefreshLateJoinVoiceMixers();
			yield return new WaitForSecondsRealtime(0.2f);
			int cosmeticsSecond = RefreshLateJoinCosmetics(localAvatar, syncLocal: false);
			int voiceSecond = RefreshLateJoinVoiceMixers();
			bool physicsRepaired = RepairLocalAvatarPhysics(localAvatar);

			_localBootstrapModuleCount = CountLoadedModules();
			_localBootstrapStage = "identity";
			bool identityRepublished = RepublishLocalIdentity(localAvatar);

			_localBootstrapStage = "owner-complete";
			bool alreadyComplete = TryReadBool(PlayerAnimDone, localAvatar, out bool animationComplete) && animationComplete;
			if (!alreadyComplete)
			{
				// This invokes the game's public method, which sends
				// LoadingLevelAnimationCompletedRPC from this avatar's real owner.  It is
				// not a host-side field write and passes the vanilla OwnerOnlyRPC guard.
				if (!TryEmitLocalOwnerCompletion(localAvatar))
				{
					_localBootstrapStage = "owner-rpc-failed";
					PublishPeerState("owner-rpc-failed", ready: false, moduleCount: _localBootstrapModuleCount);
					yield break;
				}
				_localForcedOwnCompletion = true;
				Debug.Log("[MJ.DIAG] local bootstrap emitted owner completion actor=" + LocalActorNumber() +
					" view=" + localAvatar.photonView.ViewID);
			}
			else
			{
				Debug.Log("[MJ.DIAG] local bootstrap found own completion already set actor=" + LocalActorNumber() +
					" view=" + localAvatar.photonView.ViewID);
			}

			// The Harmony postfix normally observes the method above.  Marking this
			// locally as well covers a version where another patch short-circuits the
			// public method after the owner RPC has already been queued.
			MarkLocalLoadingComplete();
			_localLateJoinBootstrapDone = true;
			_localBootstrapStage = "ready";
			PublishPeerState("ready", ready: true, moduleCount: _localBootstrapModuleCount);
			Debug.Log("[MJ.DIAG] local bootstrap ready actor=" + LocalActorNumber() +
				" modules=" + _localBootstrapModuleCount + " cosmetics=" + cosmeticsFirst + "/" + cosmeticsSecond +
				" voice=" + voiceFirst + "/" + voiceSecond + " identity=" + (identityRepublished ? "1" : "0") +
				" physicsRepair=" + (physicsRepaired ? "1" : "0"));
		}
		finally
		{
			_localLateJoinBootstrapRunning = false;
			_nextDiagnosticPublish = 0f;
			PublishLocalDiagnostic(true);
		}
	}

	private static bool TryEmitLocalOwnerCompletion(PlayerAvatar avatar)
	{
		try
		{
			avatar.LoadingLevelAnimationCompleted();
			return true;
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("[MJ.DIAG] local owner completion emit failed actor=" + LocalActorNumber() + ": " +
				ex.GetType().Name + ": " + ex.Message);
			return false;
		}
	}

	private static PlayerAvatar FindLocalAvatar(int expectedViewId)
	{
		GameDirector director = GameDirector.instance;
		if (director?.PlayerList == null)
		{
			return null;
		}
		for (int i = 0; i < director.PlayerList.Count; i++)
		{
			PlayerAvatar avatar = director.PlayerList[i];
			PhotonView view = avatar != null ? avatar.photonView : null;
			if (view == null || !view.IsMine)
			{
				continue;
			}
			if (expectedViewId == 0 || view.ViewID == expectedViewId)
			{
				return avatar;
			}
		}
		return null;
	}

	private static int RefreshLateJoinCosmetics(PlayerAvatar localAvatar, bool syncLocal)
	{
		GameDirector director = GameDirector.instance;
		if (director?.PlayerList == null)
		{
			return 0;
		}
		int refreshed = 0;
		for (int i = 0; i < director.PlayerList.Count; i++)
		{
			PlayerAvatar avatar = director.PlayerList[i];
			PlayerCosmetics cosmetics = avatar != null ? avatar.playerCosmetics : null;
			PhotonView view = avatar != null ? avatar.photonView : null;
			if (cosmetics == null || view == null)
			{
				continue;
			}
			try
			{
				if (view.IsMine)
				{
					if (syncLocal)
					{
						// Mirror the native FirstSetup path. Forcing here can wipe a
						// just-created cosmetic list before MetaManager has finished
						// loading the local save, which is exactly how a remote client
						// ends up observing the fallback/basic appearance.
						bool saveReady = MetaSaveReady == null ||
							(TryReadBool(MetaSaveReady, MetaManager.instance, out bool loaded) && loaded);
						if (!saveReady)
						{
							Debug.LogWarning("[MJ.DIAG] local cosmetics relay deferred actor=" + LocalActorNumber() +
								" saveReady=0 view=" + view.ViewID);
							continue;
						}
						cosmetics.SetupCosmetics(true, false);
						cosmetics.SetupColors(true);
						refreshed++;
					}
				}
				else
				{
					cosmetics.SetupCosmetics(false, true);
					cosmetics.SetupColors(false);
					refreshed++;
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogWarning("[MJ.DIAG] cosmetics refresh failed view=" + view.ViewID + ": " +
					ex.GetType().Name + ": " + ex.Message);
			}
		}
		return refreshed;
	}

	private static int RefreshLateJoinVoiceMixers()
	{
		GameDirector director = GameDirector.instance;
		if (director?.PlayerList == null)
		{
			return 0;
		}
		int refreshed = 0;
		MethodInfo toggleMixer = AccessTools.Method(typeof(PlayerVoiceChat), "ToggleMixer", new[] { typeof(bool), typeof(bool) });
		if (toggleMixer == null)
		{
			return 0;
		}
		for (int i = 0; i < director.PlayerList.Count; i++)
		{
			PlayerAvatar avatar = director.PlayerList[i];
			PhotonView view = avatar != null ? avatar.photonView : null;
			PlayerVoiceChat voice = PlayerVoiceChatField != null ? PlayerVoiceChatField.GetValue(avatar) as PlayerVoiceChat : null;
			if (avatar == null || view == null || view.IsMine || voice == null)
			{
				continue;
			}
			try
			{
				toggleMixer.Invoke(voice, new object[] { false, false });
				refreshed++;
			}
			catch
			{
			}
		}
		return refreshed;
	}

	private static int CountLoadedModules()
	{
		try
		{
			return Object.FindObjectsOfType<Module>().Length;
		}
		catch
		{
			return -1;
		}
	}

	private static bool RepairLocalAvatarPhysics(PlayerAvatar avatar)
	{
		if (avatar == null || avatar.photonView == null || !avatar.photonView.IsMine)
		{
			return false;
		}
		if (!TryReadBool(PlayerSpawned, avatar, out bool spawned) || !spawned)
		{
			Debug.LogWarning("[MJ.DIAG] local physics repair skipped actor=" + LocalActorNumber() + " spawned=0");
			return false;
		}
		try
		{
			Rigidbody body = avatar.GetComponent<Rigidbody>();
			if (body == null || !body.isKinematic)
			{
				return false;
			}
			// This is the same post-Generated transition performed by
			// PlayerAvatar.FixedUpdate.  Restrict it to the local late-joiner, after
			// SpawnRPC, so a stalled loading state cannot leave its body permanently
			// kinematic and without collision response.
			body.isKinematic = false;
			body.WakeUp();
			_localPhysicsRepairs++;
			Debug.Log("[MJ.DIAG] local physics repaired actor=" + LocalActorNumber() +
				" view=" + avatar.photonView.ViewID + " kinematic=1>0");
			return true;
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("[MJ.DIAG] local physics repair failed actor=" + LocalActorNumber() + ": " +
				ex.GetType().Name + ": " + ex.Message);
			return false;
		}
	}

	private static bool RepublishLocalIdentity(PlayerAvatar avatar)
	{
		if (avatar == null || avatar.photonView == null || !avatar.photonView.IsMine)
		{
			return false;
		}
		try
		{
			string playerName = ReadAvatarString(PlayerName, avatar);
			if (IsPlaceholderName(playerName))
			{
				playerName = PhotonNetwork.NickName;
			}
			string steamId = ReadAvatarString(PlayerSteamId, avatar);
			if (string.IsNullOrWhiteSpace(steamId))
			{
				steamId = SemiFunc.PlayerGetSteamID(avatar);
			}
			if (string.IsNullOrWhiteSpace(playerName))
			{
				Debug.LogWarning("[MJ.DIAG] identity re-publish skipped: no local name actor=" + LocalActorNumber());
				return false;
			}
			if (string.IsNullOrWhiteSpace(steamId))
			{
				steamId = "0";
			}
			avatar.photonView.RPC("AddToStatsManagerRPC", RpcTarget.AllBuffered, playerName, steamId);
			PhotonNetwork.SendAllOutgoingCommands();
			_localIdentityResendCount++;
			Debug.Log("[MJ.DIAG] identity re-published actor=" + LocalActorNumber() +
				" view=" + avatar.photonView.ViewID + " name=" + playerName + " steam=" + steamId);
			return true;
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("[MJ.DIAG] identity re-publish failed actor=" + LocalActorNumber() + ": " +
				ex.GetType().Name + ": " + ex.Message);
			return false;
		}
	}

	internal static void HandlePlayerIdentityRpc(PlayerAvatar avatar, string rpcName, PhotonMessageInfo info)
	{
		if (avatar == null || avatar.photonView == null)
		{
			return;
		}
		Player owner = avatar.photonView.Owner;
		if (info.Sender != null && owner != null && info.Sender.ActorNumber != owner.ActorNumber)
		{
			return;
		}
		RefreshAvatarNameUi(avatar, rpcName, "identity-rpc");
	}

	internal static void HandlePlayerNameUiCreated(PlayerAvatar avatar)
	{
		RefreshAvatarNameUi(avatar, null, "ui-create");
	}

	private static void RefreshAvatarNameUi(PlayerAvatar avatar, string rpcName, string reason)
	{
		if (avatar == null || PlayerNameUi == null)
		{
			return;
		}
		string displayName = !IsPlaceholderName(rpcName)
			? rpcName.Trim()
			: GetDisplayName(avatar);
		if (IsPlaceholderName(displayName))
		{
			return;
		}
		try
		{
			WorldSpaceUIPlayerName nameUi = PlayerNameUi.GetValue(avatar) as WorldSpaceUIPlayerName;
			if (nameUi == null || nameUi.text == null)
			{
				return;
			}
			string before = nameUi.text.text;
			if (string.Equals(before, displayName, System.StringComparison.Ordinal))
			{
				return;
			}
			nameUi.text.text = displayName;
			Debug.Log("[MJ.DIAG] player name UI refreshed reason=" + reason + " actor=" +
				GetActorNumber(avatar.photonView.Owner) + " view=" + avatar.photonView.ViewID +
				" old=" + (before ?? "<null>") + " new=" + displayName);
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("[MJ.DIAG] player name UI refresh failed reason=" + reason + ": " +
				ex.GetType().Name + ": " + ex.Message);
		}
	}

	internal static void HandleLoadingCompletedRpc(PlayerAvatar avatar, PhotonMessageInfo info)
	{
		if (!Enabled || !PhotonNetwork.IsMasterClient || avatar == null || avatar.photonView == null || info.Sender == null)
		{
			return;
		}

		Player owner = avatar.photonView.Owner;
		if (owner == null || owner.ActorNumber != info.Sender.ActorNumber)
		{
			return;
		}

		int actor = owner.ActorNumber;
		if (!JoiningActors.Contains(actor) && !TransitionUnsafeActors.Contains(actor))
		{
			return;
		}

		// This is the joiner's natural owner-only acknowledgement. The running
		// pipeline will now replay historical avatars and release transition safety.
		Debug.Log("[MJ] actor=" + actor + " owner loading completion observed peer=" +
			(DescribePeerState(owner) ?? string.Empty));
		Room room = PhotonNetwork.CurrentRoom;
		Player target = room != null ? room.GetPlayer(actor) : null;
		if (target != null)
		{
			SendHostLoadingCompleteTo(target, "joiner-ready");
		}
	}

	internal static void BeginTransitionLock(RunManager.ChangeLevelType changeType)
	{
		if (!Enabled || !PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom)
		{
			return;
		}

		// Host Game from the menu also uses ChangeLevel(LobbyMenu).  Closing
		// that brand-new room is what made the waiting lobby look locked.
		if (changeType == RunManager.ChangeLevelType.LobbyMenu
			|| changeType == RunManager.ChangeLevelType.MainMenu)
		{
			if (InWaitingLobby())
			{
				return;
			}
			_transitionLock = true;
			_preparedGenerateDoneViewId = 0;
			_preparedAllPlayersReadyViewId = 0;
			_unlockGen++;
			AbortAllCatchups();
			CloseRoomOnly();
			Debug.Log("[MJ] leaving a run for waiting lobby; room stays closed until lobby scene is ready");
			return;
		}

		_transitionLock = true;
		_localLoadingComplete = false;
		_preparedGenerateDoneViewId = 0;
		_preparedAllPlayersReadyViewId = 0;
		_restartSceneWaitSince = 0f;
		_restartSceneForceLogged = false;

		// Do not clear TransitionUnsafeActors here.  The old implementation
		// cleared JoiningActors before RunManager.RestartScene could inspect it,
		// which made vanilla wait forever on an incomplete late-join outro.
		ClearRunManagerBufferedRpcCache();
		AbortAllCatchups();
		_unlockGen++;
		int gen = _unlockGen;
		CloseRoomOnly();
		Loader.RunCoroutine(UnlockAfterScene(gen));
	}

	internal static void BeginEndOfRunLock()
	{
		if (!Enabled || !PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom)
		{
			return;
		}

		// Reference LateJoinNow 1.0.3 closes the room when the final extraction
		// completes.  Waiting until ChangeLevel leaves a window in which a new
		// actor can enter while the run is already tearing down.
		if (!_transitionLock)
		{
			Debug.Log("[MJ] final extraction complete; locking room before outro");
		}
		_transitionLock = true;
		_preparedGenerateDoneViewId = 0;
		_preparedAllPlayersReadyViewId = 0;
		CloseRoomOnly();
		AbortAllCatchups();
	}

	private static void ClearRunManagerBufferedRpcCache()
	{
		if (!PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom || RunManagerPunField == null)
		{
			return;
		}

		try
		{
			RunManager rm = RunManager.instance;
			object pun = rm != null ? RunManagerPunField.GetValue(rm) : null;
			PhotonView view = pun as PhotonView;
			if (view == null && pun is Component component)
			{
				view = component.GetComponent<PhotonView>();
			}
			if (view != null && view.ViewID != 0)
			{
				PhotonNetwork.RemoveBufferedRPCs(view.ViewID, "UpdateLevelRPC", null);
				Debug.Log("[MJ] cleared buffered UpdateLevelRPC view=" + view.ViewID);
			}
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("[MJ] RunManager RPC cache clear failed: " + ex.GetType().Name + ": " + ex.Message);
		}
	}

	internal static void PrepareRestartScene()
	{
		if (!Enabled || !PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom || OutroDoneField == null)
		{
			return;
		}

		if (InWaitingLobby())
		{
			return;
		}

		if (!_transitionLock)
		{
			_transitionLock = true;
			CloseRoomOnly();
		}

		if (_restartSceneWaitSince <= 0f)
		{
			_restartSceneWaitSince = Time.unscaledTime;
		}

		MarkTransitionUnsafeOutrosDone();

		// Vanilla RunManager.RestartScene has no timeout: one stale avatar with
		// outroDone=false blocks PhotonNetwork.LoadLevel("Reload") forever.
		// Mirror the reference mod's 15-second hard fallback while leaving the
		// normal vanilla path untouched before that deadline.
		if (Time.unscaledTime - _restartSceneWaitSince >= 15f)
		{
			GameDirector gd = GameDirector.instance;
			if (gd?.PlayerList != null)
			{
				for (int i = 0; i < gd.PlayerList.Count; i++)
				{
					PlayerAvatar avatar = gd.PlayerList[i];
					if (avatar != null)
					{
						OutroDoneField.SetValue(avatar, true);
					}
				}
			}

			if (!_restartSceneForceLogged)
			{
				_restartSceneForceLogged = true;
				Debug.LogWarning("[MJ] RestartScene outro barrier exceeded 15s; forcing transition");
			}
		}
	}

	internal static void HandleOutroStart(PlayerAvatar avatar)
	{
		if (!Enabled || !PhotonNetwork.IsMasterClient || avatar == null || avatar.photonView == null || OutroDoneField == null)
		{
			return;
		}

		Player owner = avatar.photonView.Owner;
		if (owner != null && TransitionUnsafeActors.Contains(owner.ActorNumber))
		{
			OutroDoneField.SetValue(avatar, true);
			Debug.Log("[MJ] actor=" + owner.ActorNumber + " skipped incomplete late-join outro");
		}
	}

	private static void MarkTransitionUnsafeOutrosDone()
	{
		if (TransitionUnsafeActors.Count == 0 || OutroDoneField == null)
		{
			return;
		}

		GameDirector gd = GameDirector.instance;
		if (gd?.PlayerList == null)
		{
			return;
		}

		for (int i = 0; i < gd.PlayerList.Count; i++)
		{
			PlayerAvatar avatar = gd.PlayerList[i];
			Player owner = avatar != null && avatar.photonView != null ? avatar.photonView.Owner : null;
			if (owner != null && TransitionUnsafeActors.Contains(owner.ActorNumber))
			{
				OutroDoneField.SetValue(avatar, true);
			}
		}
	}

	private static void ReleaseWaitingLobbyLock(string reason)
	{
		bool wasLocked = _transitionLock;
		_unlockGen++;
		_transitionLock = false;
		_restartSceneWaitSince = 0f;
		_restartSceneForceLogged = false;
		_preparedGenerateDoneViewId = 0;
		_preparedAllPlayersReadyViewId = 0;
		if (wasLocked)
		{
			Debug.Log("[MJ] " + reason + "; lobby left joinable");
		}
		TransitionUnsafeActors.Clear();
		if (Enabled)
		{
			Open();
		}
	}

	public static void Open()
	{
		if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient || _transitionLock)
		{
			return;
		}
		if (!InWaitingLobby())
		{
			LevelGenerator generator = LevelGenerator.Instance;
			PhotonView generatorView = generator != null ? generator.PhotonView : null;
			if (generatorView == null || generatorView.ViewID == 0 ||
				_preparedGenerateDoneViewId != generatorView.ViewID)
			{
				// Queue close -> cache cleanup -> open in that order so a new actor
				// cannot intentionally enter through a known stale terminal cache.
				CloseRoomOnly();
			}
			if (!PrepareRunCacheForLateJoin())
			{
				CloseRoomOnly();
				return;
			}
		}
		bool pub = WantPublic();
		OpenPhoton(pub);
		try
		{
			SteamManager.instance?.UnlockLobby(pub);
		}
		catch
		{
		}
		MarkRunProperty(!InWaitingLobby());
	}

	private static bool PrepareRunCacheForLateJoin()
	{
		LevelGenerator generator = LevelGenerator.Instance;
		PhotonView view = generator != null ? generator.PhotonView : null;
		if (generator == null || !generator.Generated || view == null || view.ViewID == 0)
		{
			return false;
		}
		PhotonView networkView = NetworkManager.instance != null ? NetworkManager.instance.photonView : null;
		if (networkView == null || networkView.ViewID == 0)
		{
			return false;
		}
		if (_preparedGenerateDoneViewId == view.ViewID &&
			_preparedAllPlayersReadyViewId == networkView.ViewID)
		{
			return true;
		}

		try
		{
			// GenerateDone is a terminal event and AllPlayerSpawnedRPC releases the
			// generation coroutine. Replaying either from cache can run the joiner
			// before its targeted UpdateLevel arrives, leaving LevelGenerator.Level
			// null/stale. Both are replaced with targeted sends in CatchupPipeline.
			if (!PhotonNetwork.RemoveBufferedRPCs(view.ViewID, "GenerateDone", null))
			{
				Debug.LogWarning("[MJ] buffered GenerateDone cleanup could not be queued; room stays closed");
				return false;
			}
			if (!PhotonNetwork.RemoveBufferedRPCs(networkView.ViewID, "AllPlayerSpawnedRPC", null))
			{
				Debug.LogWarning("[MJ] buffered AllPlayerSpawnedRPC cleanup could not be queued; room stays closed");
				return false;
			}
			_preparedGenerateDoneViewId = view.ViewID;
			_preparedAllPlayersReadyViewId = networkView.ViewID;
			PhotonNetwork.SendAllOutgoingCommands();
			Debug.Log("[MJ] stale run barriers removed generateView=" + view.ViewID +
				" networkView=" + networkView.ViewID);
			return true;
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("[MJ] buffered GenerateDone cleanup failed: " + ex.GetType().Name + ": " + ex.Message);
			return false;
		}
	}

	private static void TryStartPendingCatchups()
	{
		if (PendingActors.Count == 0 || PhotonNetwork.CurrentRoom == null || !CatchupWorldReady())
		{
			return;
		}

		List<int> ready = null;
		foreach (int actor in PendingActors)
		{
			if (RunningPipelines.Contains(actor))
			{
				continue;
			}
			if (RetryAfter.TryGetValue(actor, out float retryAt) && Time.unscaledTime < retryAt)
			{
				continue;
			}
			bool spawnBarrierSeen = SpawnedRpcActors.Contains(actor);
			if (!spawnBarrierSeen && (!PendingSince.TryGetValue(actor, out float since) || Time.unscaledTime - since < 3f))
			{
				continue;
			}

			Player target = PhotonNetwork.CurrentRoom.GetPlayer(actor);
			if (target == null || FindAvatar(target) == null)
			{
				continue;
			}

			if (ready == null)
			{
				ready = new List<int>();
			}
			ready.Add(actor);
		}

		if (ready == null)
		{
			return;
		}

		for (int i = 0; i < ready.Count; i++)
		{
			int actor = ready[i];
			Player target = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.GetPlayer(actor) : null;
			if (target == null)
			{
				continue;
			}
			if (!SpawnedRpcActors.Contains(actor))
			{
				Debug.LogWarning("[MJ] actor=" + actor + " PlayerSpawnedRPC barrier missed; avatar-ready fallback");
			}
			StartCatchup(target);
		}
	}

	private static bool CatchupWorldReady()
	{
		if (_transitionLock || InWaitingLobby())
		{
			return false;
		}
		GameDirector director = GameDirector.instance;
		LevelGenerator generator = LevelGenerator.Instance;
		return director != null && director.currentState == GameDirector.gameState.Main
			&& generator != null && generator.Generated;
	}

	private static void StartCatchup(Player newPlayer)
	{
		if (newPlayer == null || !PhotonNetwork.IsMasterClient || !CatchupWorldReady())
		{
			return;
		}
		int actor = newPlayer.ActorNumber;
		if (!RunningPipelines.Add(actor))
		{
			return;
		}
		PendingActors.Remove(actor);
		PendingSince.Remove(actor);
		RetryAfter.Remove(actor);
		CatchupAttempts.TryGetValue(actor, out int previousAttempts);
		CatchupAttempts[actor] = previousAttempts + 1;
		Debug.Log("[MJ] actor=" + actor + " catchup attempt=" + (previousAttempts + 1));
		Loader.RunCoroutine(CatchupPipeline(newPlayer));
	}

	private static IEnumerator CatchupPipeline(Player newPlayer)
	{
		int actor = newPlayer.ActorNumber;
		Debug.Log("[MJ] actor=" + actor + " catchup start nick=" + (newPlayer.NickName ?? string.Empty) +
			" peer=" + (string.IsNullOrEmpty(DescribePeerState(newPlayer)) ? "none" : DescribePeerState(newPlayer)));

		float avatarDeadline = Time.unscaledTime + 8f;
		PlayerAvatar avatar = null;
		while (Time.unscaledTime < avatarDeadline)
		{
			if (!StillCatching(actor))
			{
				yield break;
			}

			Room room = PhotonNetwork.CurrentRoom;
			newPlayer = room != null ? room.GetPlayer(actor) : null;
			avatar = FindAvatar(newPlayer);
			if (avatar != null)
			{
				break;
			}
			yield return null;
		}

		if (!StillCatching(actor) || avatar == null || newPlayer == null)
		{
			FailCatchup(actor, "avatar timeout");
			yield break;
		}

		Debug.Log("[MJ] actor=" + actor + " avatar ready view=" + (avatar.photonView != null ? avatar.photonView.ViewID : 0));
		RegisterLateJoinerSubsystems(avatar);
		EnsureLateJoinSupportObjects(avatar);
		SendUpdateLevel(newPlayer);
		// ModuleConnectionSetRPC is a non-buffered vanilla RPC.  It is not enough
		// to send AllPlayerSpawnedRPC here: a late receiver otherwise waits for
		// ModulesSpawned forever and never reaches ModulesReadyRPC.  Queue the
		// module count and the current module links before opening that barrier.
		yield return SendPreGenerationCatchup(newPlayer);
		if (!StillCatching(actor))
		{
			yield break;
		}

		Room preGenerationRoom = PhotonNetwork.CurrentRoom;
		newPlayer = preGenerationRoom != null ? preGenerationRoom.GetPlayer(actor) : null;
		if (newPlayer == null)
		{
			FailCatchup(actor, "target disappeared before generation barrier");
			yield break;
		}
		SendAllPlayersReady(newPlayer);
		PhotonNetwork.SendAllOutgoingCommands();

		// This acknowledgement proves the joiner's Generate coroutine received the
		// current level, finished module creation and reached the vanilla barrier.
		yield return WaitForActorSignal(ModulesReadyActors, actor, ModulesReadyTimeout, "modules-ready");
		if (!StillCatching(actor))
		{
			yield break;
		}
		if (!ModulesReadyActors.Contains(actor))
		{
			FailCatchup(actor, "ModulesReadyRPC acknowledgement timeout");
			yield break;
		}

		Room currentRoom = PhotonNetwork.CurrentRoom;
		newPlayer = currentRoom != null ? currentRoom.GetPlayer(actor) : null;
		if (newPlayer == null)
		{
			FailCatchup(actor, "target disappeared before snapshot");
			yield break;
		}

		SendPostModuleBootstrap(newPlayer);
		yield return SendWorldCatchup(newPlayer, includeModules: false);
		if (!StillCatching(actor))
		{
			yield break;
		}

		currentRoom = PhotonNetwork.CurrentRoom;
		newPlayer = currentRoom != null ? currentRoom.GetPlayer(actor) : null;
		if (newPlayer == null)
		{
			FailCatchup(actor, "target disappeared before spawn");
			yield break;
		}

		yield return EnsureLateJoinSpawn(actor, 2f);
		if (!StillCatching(actor))
		{
			yield break;
		}
		if (!SpawnCompleted.Contains(actor))
		{
			FailCatchup(actor, "spawn timeout before GenerateDone");
			yield break;
		}

		// Generated must not flip until the joiner's own PlayerSpawnedRPC reaches
		// the host. Position/collision movement is not an acknowledgement.
		yield return WaitForActorSignal(LevelSpawnedActors, actor, PlayerSpawnedTimeout, "spawn-ready");
		if (!StillCatching(actor))
		{
			yield break;
		}
		if (!LevelSpawnedActors.Contains(actor))
		{
			FailCatchup(actor, "PlayerSpawnedRPC acknowledgement timeout");
			yield break;
		}

		currentRoom = PhotonNetwork.CurrentRoom;
		newPlayer = currentRoom != null ? currentRoom.GetPlayer(actor) : null;
		if (newPlayer == null)
		{
			FailCatchup(actor, "target disappeared before GenerateDone");
			yield break;
		}

		SendEnemyReadyAll(newPlayer);
		if (!SendGenerateDone(newPlayer))
		{
			FailCatchup(actor, "GenerateDone send failed");
			yield break;
		}
		Debug.Log("[MJ] actor=" + actor + " GenerateDone sent once");
		// ModuleConnectionSetRPC increments ModulesSpawned and is not idempotent.
		// ModulesReadyRPC above proves the pre-generation replay completed, so do
		// not replay the same module links after GenerateDone.

		// A receiver without the local bootstrap cannot produce its own animation
		// completion until LoadingUI sees every *other* avatar complete.  Sending
		// those historical completions after waiting for the receiver's own one is
		// circular.  Break that cycle only for legacy receivers; injected clients
		// perform the same release themselves immediately after Generated.
		if (!PeerSupportsLocalBootstrap(newPlayer))
		{
			Debug.Log("[MJ] actor=" + actor + " legacy receiver: replaying historical completions before owner ack");
			yield return new WaitForSecondsRealtime(0.25f);
			yield return ReplayHistoricalLoadingCompletions(newPlayer, "pre-owner-legacy");
			if (!StillCatching(actor))
			{
				yield break;
			}
		}
		else
		{
			Debug.Log("[MJ] actor=" + actor + " bootstrap-capable receiver: waiting for owner ack");
		}

		// GenerateDone starts the joiner's native loading animation.  The owner
		// acknowledgement remains the completion proof, while legacy receivers have
		// already received the missing historical flags above.
		yield return WaitForJoinerLoadingReady(actor, OwnerLoadingTimeout);
		if (!StillCatching(actor))
		{
			yield break;
		}
		if (!IsJoinerLoadingReady(actor))
		{
			Room recoveryRoom = PhotonNetwork.CurrentRoom;
			Player recoveryTarget = recoveryRoom != null ? recoveryRoom.GetPlayer(actor) : null;
			if (recoveryTarget != null)
			{
				Debug.LogWarning("[MJ] actor=" + actor + " owner ack timeout; issuing final historical replay");
				yield return ReplayHistoricalLoadingCompletions(recoveryTarget, "owner-timeout-recovery");
				yield return WaitForJoinerLoadingReady(actor, 6f);
			}
			if (!IsJoinerLoadingReady(actor))
			{
				FailCatchup(actor, "owner loading-animation acknowledgement timeout after historical recovery");
				yield break;
			}
		}

		// Only now replay old players' unbuffered completion state. This prevents
		// an early synthetic barrier from masking a failed Generate coroutine.
		yield return RecoverCompletionBarrier(actor);
		if (!StillCatching(actor))
		{
			yield break;
		}

		currentRoom = PhotonNetwork.CurrentRoom;
		newPlayer = currentRoom != null ? currentRoom.GetPlayer(actor) : null;
		if (newPlayer == null)
		{
			FailCatchup(actor, "target disappeared before final catchup");
			yield break;
		}

		bool naturalReady = IsJoinerLoadingReady(actor);
		SendOwnCatchupTo(newPlayer);
		MarkJoinerPlayable(actor, "strict loading handshake");
		Debug.Log("[MJ] actor=" + actor + " completed naturalReady=" + naturalReady);
		CompleteCatchup(actor);
	}

	private static IEnumerator WaitForActorSignal(HashSet<int> signals, int actor, float timeout, string label)
	{
		float deadline = Time.unscaledTime + timeout;
		while (Time.unscaledTime < deadline && !signals.Contains(actor))
		{
			if (!StillCatching(actor))
			{
				yield break;
			}
			yield return null;
		}
		if (signals.Contains(actor))
		{
			Debug.Log("[MJ] actor=" + actor + " barrier=" + label);
		}
		else
		{
			Debug.LogWarning("[MJ] actor=" + actor + " barrier timeout=" + label);
		}
	}

	private static bool IsJoinerLoadingReady(int actor)
	{
		if (PlayerAnimDone == null || PhotonNetwork.CurrentRoom == null)
		{
			return false;
		}

		Player target = PhotonNetwork.CurrentRoom.GetPlayer(actor);
		PlayerAvatar avatar = FindAvatar(target);
		if (avatar == null)
		{
			return false;
		}

		try
		{
			return PlayerAnimDone.GetValue(avatar) is bool done && done;
		}
		catch
		{
			return false;
		}
	}

	private static IEnumerator WaitForJoinerLoadingReady(int actor, float timeout)
	{
		float deadline = Time.unscaledTime + timeout;
		while (Time.unscaledTime < deadline && !IsJoinerLoadingReady(actor))
		{
			if (!StillCatching(actor))
			{
				yield break;
			}
			yield return null;
		}
		if (IsJoinerLoadingReady(actor))
		{
			Player target = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.GetPlayer(actor) : null;
			Debug.Log("[MJ] actor=" + actor + " barrier=owner-loading-complete peer=" +
				(string.IsNullOrEmpty(DescribePeerState(target)) ? "none" : DescribePeerState(target)));
		}
		else
		{
			Player target = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.GetPlayer(actor) : null;
			Debug.LogWarning("[MJ] actor=" + actor + " barrier timeout=owner-loading-complete peer=" +
				(string.IsNullOrEmpty(DescribePeerState(target)) ? "none" : DescribePeerState(target)) +
				" diag=" + ReadRemoteDiagnostic(target));
		}
	}


	private static bool StillCatching(int actor)
	{
		if (!Enabled || _transitionLock || !PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient)
		{
			FinishCatchup(actor);
			return false;
		}
		if (!RunningPipelines.Contains(actor))
		{
			return false;
		}
		if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.GetPlayer(actor) == null)
		{
			FinishCatchup(actor);
			return false;
		}
		return true;
	}

	private static void FinishCatchup(int actor)
	{
		RunningPipelines.Remove(actor);
		PendingActors.Remove(actor);
		PendingSince.Remove(actor);
	}

	private static void CompleteCatchup(int actor)
	{
		FinishCatchup(actor);
		CatchupAttempts.Remove(actor);
		RetryAfter.Remove(actor);
		TransitionUnsafeActors.Remove(actor);
	}

	private static void FailCatchup(int actor, string reason)
	{
		RunningPipelines.Remove(actor);
		PendingActors.Remove(actor);
		PendingSince.Remove(actor);
		CompletionRetryActors.Remove(actor);
		ClearJoinerActivityObservation(actor);

		Room room = PhotonNetwork.CurrentRoom;
		bool online = room != null && room.GetPlayer(actor) != null;
		CatchupAttempts.TryGetValue(actor, out int attempts);
		bool canRetry = online && Enabled && PhotonNetwork.IsMasterClient && !_transitionLock
			&& !GenerateDoneSentActors.Contains(actor) && attempts < MaxCatchupAttempts;
		if (canRetry)
		{
			if (reason.IndexOf("PlayerSpawnedRPC", StringComparison.Ordinal) >= 0)
			{
				// Reliable delivery does not prove the receiver processed SpawnRPC.
				// Re-arm the targeted, idempotent spawn stage for the next attempt.
				SpawnCompleted.Remove(actor);
				LevelSpawnedActors.Remove(actor);
				NeedsLateJoinSpawn.Add(actor);
			}
			float delay = Mathf.Min(5f, 1.5f * Mathf.Max(1, attempts));
			PendingActors.Add(actor);
			PendingSince[actor] = Time.unscaledTime;
			RetryAfter[actor] = Time.unscaledTime + delay;
			if (!SpawnCompleted.Contains(actor))
			{
				NeedsLateJoinSpawn.Add(actor);
			}
			Debug.LogWarning("[MJ] actor=" + actor + " catchup attempt=" + attempts + " failed: " + reason
				+ "; retry in " + delay.ToString("0.0") + "s");
			return;
		}

		Player abortedTarget = room != null ? room.GetPlayer(actor) : null;
		Debug.LogWarning("[MJ] actor=" + actor + " catchup aborted after attempt=" + attempts + ": " + reason +
			" peer=" + (string.IsNullOrEmpty(DescribePeerState(abortedTarget)) ? "none" : DescribePeerState(abortedTarget)) +
			" diag=" + ReadRemoteDiagnostic(abortedTarget));
		JoiningActors.Remove(actor);
		NeedsLateJoinSpawn.Remove(actor);
		SpawnCompleted.Remove(actor);
		CatchupAttempts.Remove(actor);
		RetryAfter.Remove(actor);
	}

	private static void AbortAllCatchups()
	{
		JoiningActors.Clear();
		PendingActors.Clear();
		SpawnedRpcActors.Clear();
		ModulesReadyActors.Clear();
		LevelSpawnedActors.Clear();
		RunningPipelines.Clear();
		NeedsLateJoinSpawn.Clear();
		SpawnCompleted.Clear();
		GenerateDoneSentActors.Clear();
		PendingSince.Clear();
		CatchupAttempts.Clear();
		RetryAfter.Clear();
		CompletionRetryActors.Clear();
		PlayableActors.Clear();
		ActivityArmedActors.Clear();
		ActivityOrigins.Clear();
		ActivityWatchStarted.Clear();
		ActivityArmedAt.Clear();
	}

	private static void RegisterLateJoinerSubsystems(PlayerAvatar avatar)
	{
		PhotonView view = avatar != null ? avatar.photonView : null;
		if (!PhotonNetwork.IsMasterClient || view == null || view.ViewID == 0)
		{
			return;
		}

		int enemies = 0;
		int screens = 0;
		try
		{
			List<EnemyParent> spawned = EnemyDirector.instance != null
				? EnemyDirector.instance.enemiesSpawned
				: null;
			if (spawned != null)
			{
				for (int i = 0; i < spawned.Count; i++)
				{
					EnemyParent parent = spawned[i];
					Enemy enemy = parent != null && EnemyParentEnemyField != null
						? EnemyParentEnemyField.GetValue(parent) as Enemy
						: null;
					if (enemy == null)
					{
						continue;
					}
					enemy.PlayerAdded(view.ViewID);
					enemies++;
				}
			}
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("[MJ] enemy registration failed view=" + view.ViewID + ": " +
				ex.GetType().Name + ": " + ex.Message);
		}

		// Some EnemyOnScreen components can temporarily exist outside the director's
		// spawned list. PlayerAdded is idempotent, so cover those as a fallback too.
		try
		{
			EnemyOnScreen[] allScreens = Object.FindObjectsOfType<EnemyOnScreen>();
			if (allScreens != null)
			{
				for (int i = 0; i < allScreens.Length; i++)
				{
					EnemyOnScreen screen = allScreens[i];
					if (screen == null)
					{
						continue;
					}
					screen.PlayerAdded(view.ViewID);
					screens++;
				}
			}
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("[MJ] enemy-screen registration failed view=" + view.ViewID + ": " +
				ex.GetType().Name + ": " + ex.Message);
		}
		Debug.Log("[MJ] actor subsystem registration view=" + view.ViewID +
			" enemies=" + enemies + " enemyScreens=" + screens);
	}

	private static void EnsureLateJoinSupportObjects(PlayerAvatar avatar)
	{
		if (!PhotonNetwork.IsMasterClient || avatar == null || LevelGenerator.Instance == null)
		{
			return;
		}
		try
		{
			if (SemiFunc.RunIsLobby())
			{
				return;
			}
		}
		catch
		{
		}

		Vector3 parked = new Vector3(0f, 3000f, 0f);

		int created = 0;
		LevelGenerator generator = LevelGenerator.Instance;
		try
		{
			PlayerDeathHead existing = PlayerDeathHeadField != null
				? PlayerDeathHeadField.GetValue(avatar) as PlayerDeathHead
				: null;
			if (existing == null && generator.PlayerDeathHeadPrefab != null)
			{
				GameObject go = PhotonNetwork.Instantiate(generator.PlayerDeathHeadPrefab.name,
					parked, Quaternion.identity, 0);
				PlayerDeathHead head = go != null ? go.GetComponent<PlayerDeathHead>() : null;
				if (head != null)
				{
					head.playerAvatar = avatar;
					PlayerDeathHeadField?.SetValue(avatar, head);
					created++;
				}
			}
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("[MJ] death-head creation failed: " + ex.GetType().Name + ": " + ex.Message);
		}

		try
		{
			PlayerTumble existing = PlayerTumbleField != null
				? PlayerTumbleField.GetValue(avatar) as PlayerTumble
				: null;
			if (existing == null && generator.PlayerTumblePrefab != null)
			{
				GameObject go = PhotonNetwork.Instantiate(generator.PlayerTumblePrefab.name,
					parked, Quaternion.identity, 0);
				PlayerTumble tumble = go != null ? go.GetComponent<PlayerTumble>() : null;
				if (tumble != null)
				{
					tumble.playerAvatar = avatar;
					PlayerTumbleField?.SetValue(avatar, tumble);
					created++;
				}
			}
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("[MJ] tumble creation failed: " + ex.GetType().Name + ": " + ex.Message);
		}

		if (created > 0)
		{
			PhotonNetwork.SendAllOutgoingCommands();
			Debug.Log("[MJ] support objects created=" + created + " actor=" +
				(avatar.photonView != null && avatar.photonView.Owner != null
					? avatar.photonView.Owner.ActorNumber : 0));
		}
	}

	internal static bool TrySpawnJoinerAtSpawnPoint(PlayerAvatar joiner)
	{
		if (joiner == null || !PhotonNetwork.IsMasterClient)
		{
			return false;
		}
		if (!TryGetSpawnPose(out Vector3 pos, out Quaternion rot))
		{
			return false;
		}
		try
		{
			joiner.Spawn(pos, rot);
			int actor = GetActorNumber(joiner.photonView != null ? joiner.photonView.Owner : null);
			int viewId = joiner.photonView != null ? joiner.photonView.ViewID : 0;
			if (actor > 0 && viewId > 0)
			{
				Loader.RunCoroutine(LogHostJoinerRuntimeAfterSpawn(actor, viewId));
			}
			return true;
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("[MJ] spawn failed: " + ex.GetType().Name + ": " + ex.Message);
			return false;
		}
	}

	private static IEnumerator LogHostJoinerRuntimeAfterSpawn(int actor, int viewId)
	{
		// SpawnRPC has no acknowledgement at this level. Sample twice so logs show
		// whether the host itself applied the spawned/physics state before and after
		// the joiner's GenerateDone transition.
		yield return null;
		LogHostJoinerRuntime(actor, viewId, "spawn+1f");
		yield return new WaitForSecondsRealtime(1f);
		LogHostJoinerRuntime(actor, viewId, "spawn+1s");
	}

	private static void LogHostJoinerRuntime(int actor, int viewId, string stage)
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}
		GameDirector director = GameDirector.instance;
		PlayerAvatar avatar = director != null ? FindAvatarByActor(director, actor) : null;
		if (avatar == null || avatar.photonView == null || avatar.photonView.ViewID != viewId)
		{
			Debug.LogWarning("[MJ.DIAG] host avatar missing actor=" + actor + " view=" + viewId + " stage=" + stage);
			return;
		}
		string active = "?";
		try
		{
			active = avatar.gameObject.activeInHierarchy ? "1" : "0";
		}
		catch
		{
		}
		Player target = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.GetPlayer(actor) : null;
		Debug.Log("[MJ.DIAG] host avatar actor=" + actor + " view=" + viewId +
			" stage=" + stage + " state=" + CurrentGameState() +
			" active=" + active + " spawned=" + ReadDiagnosticBool(PlayerSpawned, avatar) +
			" anim=" + ReadDiagnosticBool(PlayerAnimDone, avatar) +
			" physics=" + DescribeAvatarPhysics(avatar) +
			" name=" + GetDisplayName(avatar, target != null ? target.NickName : string.Empty) +
			" peer=" + (string.IsNullOrEmpty(DescribePeerState(target)) ? "none" : DescribePeerState(target)));
	}

	private static bool TryGetSpawnPose(out Vector3 pos, out Quaternion rot)
	{
		pos = Vector3.zero;
		rot = Quaternion.identity;
		SpawnPoint[] points = Object.FindObjectsOfType<SpawnPoint>();
		if (points != null && points.Length > 0)
		{
			SpawnPoint chosen = points[0];
			for (int i = 0; i < points.Length; i++)
			{
				if (points[i] != null && points[i].debug)
				{
					chosen = points[i];
					break;
				}
			}
			if (chosen != null)
			{
				pos = chosen.transform.position;
				rot = chosen.transform.rotation;
				return true;
			}
		}
		if (TruckSafetySpawnPoint.instance != null)
		{
			pos = TruckSafetySpawnPoint.instance.transform.position;
			rot = TruckSafetySpawnPoint.instance.transform.rotation;
			return true;
		}
		return false;
	}

	private static IEnumerator UnlockAfterScene(int gen)
	{
		float giveUp = Time.unscaledTime + 90f;
		bool reachedMain = false;
		while (Time.unscaledTime < giveUp && gen == _unlockGen)
		{
			GameDirector gd = GameDirector.instance;
			if (gd != null && gd.currentState == GameDirector.gameState.Main)
			{
				reachedMain = true;
				break;
			}
			yield return null;
		}
		if (gen != _unlockGen)
		{
			yield break;
		}
		if (InWaitingLobby())
		{
			ReleaseWaitingLobbyLock("waiting lobby after scene");
			yield break;
		}
		if (!reachedMain)
		{
			Debug.LogWarning("[MJ] UnlockAfterScene timed out before Main; keeping room locked");
			yield break;
		}
		float hold = Time.unscaledTime + 3f;
		while (Time.unscaledTime < hold && gen == _unlockGen)
		{
			yield return null;
		}
		if (gen != _unlockGen)
		{
			yield break;
		}
		_transitionLock = false;
		TransitionUnsafeActors.Clear();
		_restartSceneWaitSince = 0f;
		_restartSceneForceLogged = false;
		if (Enabled)
		{
			Open();
		}
	}

	private static IEnumerator SendPreGenerationCatchup(Player target)
	{
		if (!Enabled || !PhotonNetwork.IsMasterClient || target == null)
		{
			yield break;
		}

		LevelGenerator generator = LevelGenerator.Instance;
		PhotonView generatorView = generator != null ? generator.PhotonView : null;
		if (generatorView == null || generatorView.ViewID == 0)
		{
			Debug.LogWarning("[MJ] actor=" + target.ActorNumber + " pre-generation catchup skipped: generator view unavailable");
			yield break;
		}

		int moduleAmount = LevelModuleAmount?.GetValue(generator) is int amount ? amount : 0;
		if (moduleAmount <= 0)
		{
			// Do not overwrite a valid buffered module count with a reflection
			// failure. A generated game level always has a positive module amount.
			Debug.LogWarning("[MJ] actor=" + target.ActorNumber + " module amount unavailable; preserving buffered value");
		}
		else try
		{
			generatorView.RPC("ModuleAmountRPC", target, moduleAmount);
			Debug.Log("[MJ] actor=" + target.ActorNumber + " module amount sent=" + moduleAmount);
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[MJ] actor=" + target.ActorNumber + " module amount send failed: " + ex.GetType().Name);
		}

		yield return SendModuleCatchup(target, "pre-generate");
		PhotonNetwork.SendAllOutgoingCommands();
		// Keep the RPC packet ordering explicit: ModuleAmount/connection state must
		// be enqueued before AllPlayerSpawnedRPC permits the receiver to generate.
		yield return null;
	}

	private static void SendPostModuleBootstrap(Player target)
	{
		if (!Enabled || !PhotonNetwork.IsMasterClient || target == null)
		{
			return;
		}

		LevelGenerator generator = LevelGenerator.Instance;
		PhotonView generatorView = generator != null ? generator.PhotonView : null;
		if (generatorView == null || generatorView.ViewID == 0)
		{
			return;
		}

		try
		{
			generatorView.RPC("NavMeshSetupRPC", target);
			if (!SemiFunc.RunIsArena())
			{
				generatorView.RPC("ItemSetup", target);
			}
			PhotonNetwork.SendAllOutgoingCommands();
			Debug.Log("[MJ] actor=" + target.ActorNumber + " post-module navmesh/item setup sent");
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[MJ] actor=" + target.ActorNumber + " post-module bootstrap failed: " + ex.GetType().Name);
		}
	}

	private static IEnumerator SendWorldCatchup(Player target, bool includeModules)
	{
		if (!Enabled || !PhotonNetwork.IsMasterClient || target == null)
		{
			yield break;
		}
		Debug.Log("[MJ] actor=" + target.ActorNumber + " snapshot begin");
		if (includeModules)
		{
			yield return SendModuleCatchup(target, "snapshot");
		}
		int batched = 0;
		ValuableDirector director = ValuableDirector.instance;
		if (director?.valuableList != null)
		{
			List<GameObject> haul = RoundDirector.instance != null ? RoundDirector.instance.dollarHaulList : null;
			foreach (ValuableObject valuable in director.valuableList)
			{
				if (valuable == null)
				{
					continue;
				}
				PhotonView view = valuable.GetComponent<PhotonView>();
				if (view == null || view.ViewID == 0)
				{
					continue;
				}
				try
				{
					if (ValSet?.GetValue(valuable) is bool set && set)
					{
						float value = ValCurrent?.GetValue(valuable) is float current ? current : 0f;
						view.RPC("DollarValueSetRPC", target, value);
					}
					if (ValDisc?.GetValue(valuable) is bool discovered && discovered)
					{
						view.RPC("DiscoverRPC", target);
					}
					if (haul != null && haul.Contains(valuable.gameObject))
					{
						view.RPC("AddToDollarHaulListRPC", target);
					}
				}
				catch
				{
				}
				batched++;
				if (batched >= 6)
				{
					batched = 0;
					yield return null;
				}
			}
		}
		ItemAttributes[] items = null;
		try
		{
			items = Object.FindObjectsOfType<ItemAttributes>();
		}
		catch
		{
		}
		if (items != null && ItemValue != null)
		{
			for (int i = 0; i < items.Length; i++)
			{
				ItemAttributes item = items[i];
				if (item == null)
				{
					continue;
				}
				PhotonView view = item.GetComponent<PhotonView>();
				if (view == null || view.ViewID == 0)
				{
					continue;
				}
				try
				{
					if (ItemValue.GetValue(item) is int value && value > 0)
					{
						view.RPC("GetValueRPC", target, value);
					}
				}
				catch
				{
				}
				batched++;
				if (batched >= 8)
				{
					batched = 0;
					yield return null;
				}
			}
		}
		yield return SendExtractionCatchup(target, batched);
		Debug.Log("[MJ] actor=" + target.ActorNumber + " snapshot done");
	}

	private static IEnumerator SendModuleCatchup(Player target, string reason)
	{
		if (!Enabled || !PhotonNetwork.IsMasterClient || target == null || ModDone == null)
		{
			yield break;
		}

		int actor = target.ActorNumber;
		Module[] modules = null;
		try
		{
			modules = Object.FindObjectsOfType<Module>();
		}
		catch
		{
		}
		if (modules == null)
		{
			yield break;
		}

		int sent = 0;
		int skipped = 0;
		int batched = 0;
		for (int i = 0; i < modules.Length; i++)
		{
			if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null ||
				PhotonNetwork.CurrentRoom.GetPlayer(actor) == null)
			{
				yield break;
			}

			Module mod = modules[i];
			if (mod == null || !(ModDone.GetValue(mod) is bool done) || !done)
			{
				skipped++;
				continue;
			}
			PhotonView view = mod.GetComponent<PhotonView>();
			if (view == null || view.ViewID == 0)
			{
				skipped++;
				continue;
			}
			try
			{
				view.RPC("ModuleConnectionSetRPC", target,
					ModTop?.GetValue(mod) is bool top && top,
					ModBottom?.GetValue(mod) is bool bottom && bottom,
					ModRight?.GetValue(mod) is bool right && right,
					ModLeft?.GetValue(mod) is bool left && left,
					ModFirst?.GetValue(mod) is bool first && first);
				sent++;
			}
			catch
			{
				skipped++;
			}

			batched++;
			if (batched >= 6)
			{
				batched = 0;
				yield return null;
			}
		}
		Debug.Log("[MJ] actor=" + actor + " modules reason=" + reason +
			" sent=" + sent + " skipped=" + skipped);
	}

	private static IEnumerator SendExtractionCatchup(Player target, int batched)
	{
		if (target == null)
		{
			yield break;
		}
		ExtractionPoint[] points = null;
		try
		{
			points = Object.FindObjectsOfType<ExtractionPoint>();
		}
		catch
		{
		}
		if (points == null)
		{
			yield break;
		}
		for (int i = 0; i < points.Length; i++)
		{
			ExtractionPoint point = points[i];
			if (point == null)
			{
				continue;
			}
			PhotonView view = point.GetComponent<PhotonView>();
			if (view == null || view.ViewID == 0)
			{
				continue;
			}
			try
			{
				object state = ExtractionCurrentState != null ? ExtractionCurrentState.GetValue(point) : null;
				if (state != null)
				{
					view.RPC("StateSetRPC", target, state);
				}
				SendExtractionLock(view.ViewID, point.isLocked, target.ActorNumber);
			}
			catch
			{
			}
			batched++;
			if (batched >= 6)
			{
				batched = 0;
				yield return null;
			}
		}
	}

	internal static void UnhookPhotonEvents()
	{
		if (!_photonEventHooked)
		{
			return;
		}
		try
		{
			if (PhotonNetwork.NetworkingClient != null)
			{
				PhotonNetwork.NetworkingClient.EventReceived -= OnPhotonEvent;
			}
		}
		catch
		{
		}
		_photonEventHooked = false;
	}

	internal static void EnsurePhotonEventHook()
	{
		if (_photonEventHooked)
		{
			return;
		}
		try
		{
			if (PhotonNetwork.NetworkingClient == null)
			{
				return;
			}
			PhotonNetwork.NetworkingClient.EventReceived += OnPhotonEvent;
			_photonEventHooked = true;
		}
		catch
		{
		}
	}

	private static void OnPhotonEvent(EventData photonEvent)
	{
		if (photonEvent == null || photonEvent.Code != ExtractionLockEvent || PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom)
		{
			return;
		}
		Player sender = null;
		try
		{
			Room room = PhotonNetwork.CurrentRoom;
			sender = room != null ? room.GetPlayer(photonEvent.Sender) : null;
		}
		catch
		{
		}
		if (sender == null || !sender.IsMasterClient)
		{
			return;
		}
		object[] args = photonEvent.CustomData as object[];
		if (args == null || args.Length < 2)
		{
			return;
		}
		int viewId;
		bool locked;
		try
		{
			viewId = Convert.ToInt32(args[0]);
			locked = Convert.ToBoolean(args[1]);
		}
		catch
		{
			return;
		}
		PhotonView view = PhotonView.Find(viewId);
		ExtractionPoint point = view != null ? view.GetComponent<ExtractionPoint>() : null;
		if (point == null)
		{
			return;
		}
		_applyingRemoteExtractionLock = true;
		try
		{
			point.UpdateLock(locked);
		}
		catch
		{
		}
		finally
		{
			_applyingRemoteExtractionLock = false;
		}
	}

	internal static void BroadcastExtractionLock(ExtractionPoint point, bool locked)
	{
		if (_applyingRemoteExtractionLock || point == null || !Enabled || !PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom)
		{
			return;
		}
		PhotonView view = point.GetComponent<PhotonView>();
		if (view == null || view.ViewID == 0)
		{
			return;
		}
		SendExtractionLock(view.ViewID, locked, 0);
	}

	private static void SendExtractionLock(int viewId, bool locked, int targetActor)
	{
		if (!PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom || viewId == 0)
		{
			return;
		}
		try
		{
			RaiseEventOptions options = new RaiseEventOptions();
			if (targetActor > 0)
			{
				options.TargetActors = new[] { targetActor };
			}
			else
			{
				options.Receivers = ReceiverGroup.Others;
			}
			PhotonNetwork.RaiseEvent(ExtractionLockEvent, new object[] { viewId, locked }, options, SendOptions.SendReliable);
		}
		catch
		{
		}
	}

	private static bool SendTargetedOwnershipUpdate(int targetActor, int viewId, int ownerActor)
	{
		if (!PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom || targetActor <= 0 || viewId == 0 || OwnershipUpdateMethod == null)
		{
			return false;
		}

		try
		{
			// PUN internal OwnershipUpdate sends event 212. With targetActor != -1 it
			// only rewrites the receiver's local OwnerActorNr/ControllerActorNr cache.
			// This is the same narrow mechanism used by the working reference mod:
			// no room-wide TransferOwnership and no mutation of B/C on other clients.
			OwnershipUpdateMethod.Invoke(null, new object[] { new[] { viewId, ownerActor }, targetActor });
			return true;
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("[MJ] targeted ownership update failed target=" + targetActor +
				" view=" + viewId + " owner=" + ownerActor + ": " +
				ex.GetType().Name + ": " + ex.Message);
			return false;
		}
	}

	private static IEnumerator ReplayHistoricalLoadingCompletions(Player newPlayer, string stage)
	{
		if (!Enabled || !PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom || newPlayer == null)
		{
			yield break;
		}

		int targetActor = newPlayer.ActorNumber;
		bool targetAdvertisesBootstrap = PeerSupportsLocalBootstrap(newPlayer);
		Player localPlayer = PhotonNetwork.LocalPlayer;
		int masterActor = localPlayer != null ? localPlayer.ActorNumber : 0;
		if (masterActor <= 0)
		{
			Debug.LogWarning("[MJ] actor=" + targetActor +
				" deterministic completion replay skipped stage=" + stage + ": no local master actor");
			yield break;
		}

		GameDirector gd = GameDirector.instance;
		if (gd?.PlayerList == null)
		{
			yield break;
		}

		// Snapshot IDs and owners before yielding. PlayerList and Unity object
		// references can mutate from join/leave callbacks between relay phases.
		List<int> replayViewIds = new List<int>();
		List<int> replayOwners = new List<int>();
		int hostFlagFalse = 0;
		for (int i = 0; i < gd.PlayerList.Count; i++)
		{
			PlayerAvatar avatar = gd.PlayerList[i];
			PhotonView view = avatar != null ? avatar.photonView : null;
			Player owner = view != null ? view.Owner : null;
			if (view == null || view.ViewID == 0 || owner == null || owner.ActorNumber == targetActor)
			{
				continue;
			}

			// Every pre-existing avatar is historical for this receiver. A false
			// host-side field can be stale or missed and must never suppress replay:
			// one missing receiver-local flag blocks LoadingUI deterministically.
			if (PlayerAnimDone != null)
			{
				try
				{
					if (!(PlayerAnimDone.GetValue(avatar) is bool done) || !done)
					{
						hostFlagFalse++;
					}
				}
				catch
				{
					hostFlagFalse++;
				}
			}

			replayViewIds.Add(view.ViewID);
			replayOwners.Add(owner.ActorNumber);
		}

		int expected = replayViewIds.Count;
		int sent = 0;
		int mapped = 0;
		int restored = 0;
		int missing = 0;
		int mapFailed = 0;
		for (int i = 0; i < replayViewIds.Count && i < replayOwners.Count; i++)
		{
			Room room = PhotonNetwork.CurrentRoom;
			newPlayer = room != null ? room.GetPlayer(targetActor) : null;
			if (newPlayer == null)
			{
				break;
			}

			int viewId = replayViewIds[i];
			int originalOwner = replayOwners[i];
			PhotonView view = PhotonView.Find(viewId);
			if (view == null || originalOwner <= 0)
			{
				missing++;
				continue;
			}

			bool ownerMapped = false;
			// A protocol-v3 receiver narrowly accepts the host's historical
			// LoadingLevelAnimationCompletedRPC itself. This removes the receiver
			// owner-cache race altogether. Older/uninjected receivers retain the
			// target-local ownership relay compatibility path below.
			if (!targetAdvertisesBootstrap && originalOwner != masterActor)
			{
				ownerMapped = SendTargetedOwnershipUpdate(targetActor, viewId, masterActor);
				if (!ownerMapped)
				{
					mapFailed++;
					continue;
				}
				mapped++;
				PhotonNetwork.SendAllOutgoingCommands();
				// Put the owner mapping in an earlier PUN service cycle than the RPC.
				yield return null;
			}

			// C# iterators may yield in a try/finally block but not in a try block
			// that has a catch. Keep RPC exception handling in a nested no-yield try.
			try
			{
				bool rpcQueued = false;
				try
				{
					room = PhotonNetwork.CurrentRoom;
					newPlayer = room != null ? room.GetPlayer(targetActor) : null;
					view = PhotonView.Find(viewId);
					if (newPlayer == null || view == null)
					{
						missing++;
					}
					else
					{
						view.RPC("LoadingLevelAnimationCompletedRPC", newPlayer);
						sent++;
						rpcQueued = true;
						PhotonNetwork.SendAllOutgoingCommands();
					}
				}
				catch (System.Exception ex)
				{
					Debug.LogWarning("[MJ] actor=" + targetActor +
						" deterministic loading completion failed stage=" + stage +
						" view=" + viewId + ": " + ex.GetType().Name + ": " + ex.Message);
				}

				if (rpcQueued)
				{
					// The target must process the completion RPC while its local view still
					// reports the temporary master owner.  Restoring immediately was the
					// race in the previous implementation: host-side send logs looked
					// successful, while the receiver's OwnerOnlyRPC rejected the payload.
					if (targetAdvertisesBootstrap)
					{
						yield return null;
					}
					else
					{
						yield return new WaitForSecondsRealtime(LegacyOwnershipMappingSettleSeconds);
					}
				}
			}
			finally
			{
				if (ownerMapped && PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null &&
					PhotonNetwork.CurrentRoom.GetPlayer(targetActor) != null)
				{
					if (SendTargetedOwnershipUpdate(targetActor, viewId, originalOwner))
					{
						restored++;
					}
					PhotonNetwork.SendAllOutgoingCommands();
				}
			}
		}

		PhotonNetwork.SendAllOutgoingCommands();
		Debug.Log("[MJ] actor=" + targetActor + " deterministic historical barrier stage=" + stage +
			" expected=" + expected + " sent=" + sent + " mapped=" + mapped +
			" restored=" + restored + " missing=" + missing + " mapFailed=" + mapFailed +
			" hostFlagFalseDiagnostic=" + hostFlagFalse +
			" ownerMapMode=" + (targetAdvertisesBootstrap ? "peer-direct" : "legacy-owner-map"));
	}

	private static bool SendHostLoadingCompleteTo(Player newPlayer, string reason)
	{
		if (!Enabled || newPlayer == null || !PhotonNetwork.IsMasterClient)
		{
			return false;
		}

		GameDirector gd = GameDirector.instance;
		if (gd == null || gd.currentState != GameDirector.gameState.Main)
		{
			return false;
		}

		PlayerAvatar local = PlayerAvatar.instance;
		if (local == null || local.photonView == null || !local.photonView.IsMine)
		{
			return false;
		}

		bool fieldDone = _localLoadingComplete;
		if (!fieldDone && PlayerAnimDone != null)
		{
			try
			{
				fieldDone = PlayerAnimDone.GetValue(local) is bool done && done;
			}
			catch
			{
			}
		}

		// Main is the vanilla post-loading state, so it is safe for the host to
		// resend its own historical completion even if the reflection latch was
		// missed because MidJoin was enabled after the level had already started.
		try
		{
			local.photonView.RPC("LoadingLevelAnimationCompletedRPC", newPlayer);
			Debug.Log("[MJ] actor=" + newPlayer.ActorNumber + " host loading complete sent reason=" + reason + " fieldDone=" + fieldDone);
			return true;
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("[MJ] actor=" + newPlayer.ActorNumber + " host loading complete failed: " + ex.GetType().Name + ": " + ex.Message);
			return false;
		}
	}

	private static void SendOwnCatchupTo(Player newPlayer)
	{
		if (!Enabled || newPlayer == null)
		{
			return;
		}

		SendHostLoadingCompleteTo(newPlayer, "pipeline");

		PlayerAvatar local = PlayerAvatar.instance;
		if (local == null || local.photonView == null || !local.photonView.IsMine)
		{
			return;
		}

		try
		{
			PlayerVoiceChat voice = PlayerVoiceChat.instance;
			if (voice != null)
			{
				PhotonView voiceView = voice.GetComponent<PhotonView>();
				if (voiceView != null)
				{
					local.photonView.RPC("UpdateMyPlayerVoiceChat", newPlayer, voiceView.ViewID);
				}
			}
		}
		catch
		{
		}
	}

	private static IEnumerator RecoverCompletionBarrier(int actor)
	{
		if (!CompletionRetryActors.Add(actor))
		{
			yield break;
		}

		float started = Time.unscaledTime;
		int replayStage = 0;
		Room initialRoom = PhotonNetwork.CurrentRoom;
		Player initialTarget = initialRoom != null ? initialRoom.GetPlayer(actor) : null;
		bool legacyTarget = !PeerSupportsLocalBootstrap(initialTarget);
		// Legacy replay keeps the owner mapping open for a real network window. A
		// pre-owner replay plus this pass and the final pass below give three tries,
		// matching the useful retry budget without holding the pipeline for six full
		// network windows per historical avatar.
		int maxReplayStages = legacyTarget ? 1 : CompletionReplaySchedule.Length;
		try
		{
			while (Time.unscaledTime - started < CompletionRecoveryTimeout &&
				replayStage < maxReplayStages)
			{
				if (!StillCatching(actor))
				{
					yield break;
				}

				float elapsed = Time.unscaledTime - started;
				if (elapsed < CompletionReplaySchedule[replayStage])
				{
					yield return null;
					continue;
				}

				Room room = PhotonNetwork.CurrentRoom;
				Player target = room != null ? room.GetPlayer(actor) : null;
				if (target == null)
				{
					yield break;
				}

				string stage = "post-generate-" + (replayStage + 1);
				yield return ReplayHistoricalLoadingCompletions(target, stage);
				SendOwnCatchupTo(target);

				PhotonNetwork.SendAllOutgoingCommands();
				replayStage++;
			}

			Room finalRoom = PhotonNetwork.CurrentRoom;
			Player finalTarget = finalRoom != null ? finalRoom.GetPlayer(actor) : null;
			if (finalTarget != null && StillCatching(actor))
			{
				yield return ReplayHistoricalLoadingCompletions(finalTarget, "post-generate-final");
				SendOwnCatchupTo(finalTarget);
				PhotonNetwork.SendAllOutgoingCommands();
			}
			Debug.Log("[MJ] actor=" + actor + " historical loading barrier replay complete mode=" +
				(legacyTarget ? "legacy" : "peer") + " stages=" + replayStage);
		}
		finally
		{
			CompletionRetryActors.Remove(actor);
		}
	}

	private static void BeginJoinerActivityObservation(int actor)
	{
		if (actor <= 0 || !PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom || PlayableActors.Contains(actor))
		{
			return;
		}

		if (!ActivityWatchStarted.ContainsKey(actor))
		{
			ActivityWatchStarted[actor] = Time.unscaledTime;
		}

		if (!ActivityArmedActors.Contains(actor))
		{
			GameDirector gd = GameDirector.instance;
			PlayerAvatar avatar = gd != null ? FindAvatarByActor(gd, actor) : null;
			if (avatar != null)
			{
				ActivityOrigins[actor] = avatar.transform.position;
			}
		}
	}

	private static void ObserveJoinerMovement()
	{
		if (ActivityWatchStarted.Count == 0)
		{
			return;
		}

		List<int> actors = new List<int>(ActivityWatchStarted.Keys);
		for (int i = 0; i < actors.Count; i++)
		{
			ObserveJoinerMovement(actors[i]);
		}
	}

	private static void ObserveJoinerMovement(int actor)
	{
		if (PlayableActors.Contains(actor))
		{
			return;
		}

		Room room = PhotonNetwork.CurrentRoom;
		if (room == null || room.GetPlayer(actor) == null)
		{
			ClearJoinerActivityObservation(actor);
			return;
		}

		if (!ActivityWatchStarted.TryGetValue(actor, out float watchStarted))
		{
			return;
		}
		if (Time.unscaledTime - watchStarted > ActivityWatchTimeout)
		{
			ClearJoinerActivityObservation(actor);
			return;
		}

		GameDirector gd = GameDirector.instance;
		PlayerAvatar avatar = gd != null ? FindAvatarByActor(gd, actor) : null;
		if (avatar == null)
		{
			return;
		}

		Vector3 current = avatar.transform.position;
		bool ownerCompletionObserved = IsJoinerLoadingReady(actor);
		bool forceWindowReached = Time.unscaledTime - watchStarted >= ForceOwnCompletionAfter + 0.5f;
		if (!ActivityArmedActors.Contains(actor))
		{
			// Ignore spawn/physics settling before the owner completion signal. If the
			// owner RPC never reaches the host, arm after the targeted fallback window.
			ActivityOrigins[actor] = current;
			if (!ownerCompletionObserved && !forceWindowReached)
			{
				return;
			}
			ActivityArmedActors.Add(actor);
			ActivityArmedAt[actor] = Time.unscaledTime;
			Debug.Log("[MJ] actor=" + actor + " movement proof armed ownerReady=" + ownerCompletionObserved);
			return;
		}

		if (!ActivityArmedAt.TryGetValue(actor, out float armedAt) || Time.unscaledTime - armedAt < 0.25f)
		{
			return;
		}
		if (!ActivityOrigins.TryGetValue(actor, out Vector3 origin))
		{
			ActivityOrigins[actor] = current;
			return;
		}

		Vector3 delta = current - origin;
		float horizontalSq = delta.x * delta.x + delta.z * delta.z;
		if (horizontalSq >= ActivityHorizontalDistance * ActivityHorizontalDistance)
		{
			MarkJoinerPlayable(actor, "movement " + Mathf.Sqrt(horizontalSq).ToString("0.00") + "m");
		}
	}

	internal static void HandleJoinerJumpRpc(PlayerAvatar avatar, PhotonMessageInfo info)
	{
		if (!Enabled || !PhotonNetwork.IsMasterClient || avatar == null || avatar.photonView == null || info.Sender == null)
		{
			return;
		}

		Player owner = avatar.photonView.Owner;
		if (owner == null || owner.ActorNumber != info.Sender.ActorNumber)
		{
			return;
		}

		int actor = owner.ActorNumber;
		if (!JoiningActors.Contains(actor) && !TransitionUnsafeActors.Contains(actor) &&
			!CompletionRetryActors.Contains(actor))
		{
			return;
		}
		MarkJoinerPlayable(actor, "owner JumpRPC");
	}

	private static void MarkJoinerPlayable(int actor, string source)
	{
		if (!PlayableActors.Add(actor))
		{
			return;
		}

		GameDirector gd = GameDirector.instance;
		PlayerAvatar avatar = gd != null ? FindAvatarByActor(gd, actor) : null;
		if (avatar != null && PlayerAnimDone != null)
		{
			try
			{
				// Host-local state only. The actor has now produced post-load gameplay
				// traffic, so retaining a false loading flag is demonstrably stale.
				PlayerAnimDone.SetValue(avatar, true);
			}
			catch (System.Exception ex)
			{
				Debug.LogWarning("[MJ] actor=" + actor + " host loading flag update failed: " +
					ex.GetType().Name + ": " + ex.Message);
			}
		}

		TransitionUnsafeActors.Remove(actor);
		ClearJoinerActivityObservation(actor, keepPlayable: true);
		Debug.Log("[MJ] actor=" + actor + " playable proof=" + source);
	}

	private static void ClearJoinerActivityObservation(int actor, bool keepPlayable = false)
	{
		ActivityArmedActors.Remove(actor);
		ActivityOrigins.Remove(actor);
		ActivityWatchStarted.Remove(actor);
		ActivityArmedAt.Remove(actor);
		if (!keepPlayable)
		{
			PlayableActors.Remove(actor);
		}
	}

	private static void SendUpdateLevel(Player newPlayer)
	{
		if (!Enabled || !PhotonNetwork.IsMasterClient || newPlayer == null)
		{
			return;
		}

		RunManager rm = RunManager.instance;
		if (rm == null || rm.levelCurrent == null || RunManagerPunField == null)
		{
			return;
		}

		try
		{
			object pun = RunManagerPunField.GetValue(rm);
			PhotonView view = pun as PhotonView;
			if (view == null && pun is Component component)
			{
				view = component.GetComponent<PhotonView>();
			}
			if (view == null || view.ViewID == 0)
			{
				return;
			}

			bool gameOver = RunGameOverField != null && RunGameOverField.GetValue(rm) is bool flag && flag;
			view.RPC("UpdateLevelRPC", newPlayer, rm.levelCurrent.name, rm.levelsCompleted, gameOver);
			Debug.Log("[MJ] actor=" + newPlayer.ActorNumber + " UpdateLevel sent level=" + rm.levelCurrent.name);
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("[MJ] actor=" + newPlayer.ActorNumber + " UpdateLevel failed: " + ex.GetType().Name + ": " + ex.Message);
		}
	}

	private static void SendAllPlayersReady(Player newPlayer)
	{
		if (!Enabled || !PhotonNetwork.IsMasterClient || newPlayer == null)
		{
			return;
		}
		try
		{
			PhotonView netView = NetworkManager.instance != null ? NetworkManager.instance.photonView : null;
			if (netView == null)
			{
				return;
			}
			netView.RPC("AllPlayerSpawnedRPC", newPlayer);
			Debug.Log("[MJ] actor=" + newPlayer.ActorNumber + " AllPlayerSpawned sent");
		}
		catch
		{
		}
	}

	private static void SendEnemyReadyAll(Player newPlayer)
	{
		if (!Enabled || !PhotonNetwork.IsMasterClient || newPlayer == null)
		{
			return;
		}
		try
		{
			LevelGenerator lg = LevelGenerator.Instance;
			if (lg != null && lg.PhotonView != null)
			{
				lg.PhotonView.RPC("EnemyReadyAllRPC", newPlayer);
			}
		}
		catch
		{
		}
	}

	private static bool SendGenerateDone(Player newPlayer)
	{
		if (!Enabled || !PhotonNetwork.IsMasterClient || newPlayer == null)
		{
			return false;
		}
		int actor = newPlayer.ActorNumber;
		if (GenerateDoneSentActors.Contains(actor))
		{
			Debug.LogWarning("[MJ] actor=" + actor + " duplicate GenerateDone suppressed");
			return true;
		}
		try
		{
			LevelGenerator lg = LevelGenerator.Instance;
			if (lg != null && lg.PhotonView != null)
			{
				lg.PhotonView.RPC("GenerateDone", newPlayer);
				GenerateDoneSentActors.Add(actor);
				PhotonNetwork.SendAllOutgoingCommands();
				return true;
			}
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("[MJ] actor=" + actor + " GenerateDone failed: " + ex.GetType().Name + ": " + ex.Message);
		}
		return false;
	}

	private static PlayerAvatar FindAvatar(Player player)
	{
		GameDirector gd = GameDirector.instance;
		if (gd?.PlayerList == null || player == null)
		{
			return null;
		}
		for (int i = 0; i < gd.PlayerList.Count; i++)
		{
			PlayerAvatar avatar = gd.PlayerList[i];
			if (avatar == null || avatar.photonView == null)
			{
				continue;
			}
			Player owner = avatar.photonView.Owner;
			if (owner != null && owner.ActorNumber == player.ActorNumber)
			{
				return avatar;
			}
		}
		return null;
	}

	internal static void MarkLocalLoadingComplete()
	{
		bool first = !_localLoadingComplete;
		_localLoadingComplete = true;
		if (IsLocalLateJoinRepairActive())
		{
			_lastLocalBarrierBlocker = null;
			_nextDiagnosticPublish = 0f;
			if (first)
			{
				Debug.Log("[MJ.DIAG] local owner loading animation completed actor=" + LocalActorNumber() +
					" state=" + CurrentGameState());
			}
			PublishLocalDiagnostic(true);
		}
	}

	internal struct LoadingRpcProbe
	{
		internal bool Tracked;
		internal bool Before;
		internal bool BeforeReadable;
		internal int ViewId;
		internal int OwnerBefore;
		internal int Sender;
	}

	internal static LoadingRpcProbe CaptureLoadingCompletedRpc(PlayerAvatar avatar, PhotonMessageInfo info)
	{
		LoadingRpcProbe probe = default(LoadingRpcProbe);
		if (!IsLocalLateJoinRepairActive() || avatar == null)
		{
			return probe;
		}

		PhotonView view = avatar.photonView;
		probe.Tracked = true;
		probe.ViewId = view != null ? view.ViewID : 0;
		probe.OwnerBefore = GetActorNumber(view != null ? view.Owner : null);
		probe.Sender = GetActorNumber(info.Sender);
		probe.BeforeReadable = TryReadBool(PlayerAnimDone, avatar, out probe.Before);
		return probe;
	}

	// The reference implementation allows the master to relay this one
	// historical loading-completion RPC at the late joiner. The vanilla method
	// is only a guarded "levelAnimationCompleted = true" assignment, so apply
	// that exact state transition here rather than weakening SemiFunc.OwnerOnlyRPC
	// globally. This is deliberately limited to a non-local historical avatar,
	// a real master sender, and the short local late-join repair window.
	internal static bool TryApplyMasterLoadingCompletion(PlayerAvatar avatar, PhotonMessageInfo info)
	{
		if (!IsLocalLateJoinRepairActive() || avatar == null || PlayerAnimDone == null ||
			info.Sender == null || !info.Sender.IsMasterClient)
		{
			return false;
		}

		PhotonView view = avatar.photonView;
		if (view == null || view.IsMine)
		{
			return false;
		}

		bool before = TryReadBool(PlayerAnimDone, avatar, out bool completed) && completed;
		TrySetBool(PlayerAnimDone, avatar, true);
		bool after = TryReadBool(PlayerAnimDone, avatar, out bool applied) && applied;
		_completionRpcMasterRelayed++;
		_nextDiagnosticPublish = 0f;
		Debug.Log("[MJ.DIAG] completion-rpc master relay actor=" + LocalActorNumber() +
			" view=" + view.ViewID + " sender=" + GetActorNumber(info.Sender) +
			" before=" + (before ? "1" : "0") + " after=" + (after ? "1" : "0"));
		return after;
	}

	internal static void ObserveLoadingCompletedRpc(PlayerAvatar avatar, PhotonMessageInfo info, LoadingRpcProbe probe)
	{
		if (!probe.Tracked)
		{
			return;
		}

		_completionRpcReceived++;
		bool after = false;
		bool afterReadable = TryReadBool(PlayerAnimDone, avatar, out after);
		PhotonView view = avatar != null ? avatar.photonView : null;
		int ownerAfter = GetActorNumber(view != null ? view.Owner : null);
		string outcome;
		if (!probe.BeforeReadable || !afterReadable)
		{
			outcome = "read-error";
		}
		else if (!probe.Before && after)
		{
			_completionRpcApplied++;
			outcome = "applied";
		}
		else if (probe.Before && after)
		{
			_completionRpcUnchanged++;
			outcome = "already";
		}
		else
		{
			_completionRpcUnchanged++;
			outcome = "unchanged";
		}

		_lastCompletionRpc = "v" + probe.ViewId + ",src" + probe.Sender +
			",o" + probe.OwnerBefore + ">" + ownerAfter + "," + outcome;
		_nextDiagnosticPublish = 0f;
		Debug.Log("[MJ.DIAG] completion-rpc actor=" + LocalActorNumber() +
			" view=" + probe.ViewId + " sender=" + probe.Sender +
			" owner=" + probe.OwnerBefore + ">" + ownerAfter +
			" before=" + (probe.BeforeReadable ? (probe.Before ? "1" : "0") : "?") +
			" after=" + (afterReadable ? (after ? "1" : "0") : "?") +
			" outcome=" + outcome);
	}

	private static bool TrySpawnLateJoinerActor(int actor, string source)
	{
		if (!Enabled || !PhotonNetwork.IsMasterClient || !NeedsLateJoinSpawn.Contains(actor))
		{
			return SpawnCompleted.Contains(actor);
		}

		GameDirector gd = GameDirector.instance;
		PlayerAvatar player = gd != null ? FindAvatarByActor(gd, actor) : null;
		if (player == null || player.photonView == null)
		{
			return false;
		}

		bool already = false;
		try
		{
			already = PlayerSpawned != null && PlayerSpawned.GetValue(player) is bool on && on;
		}
		catch
		{
		}

		if (already)
		{
			SpawnCompleted.Add(actor);
			NeedsLateJoinSpawn.Remove(actor);
			Debug.Log("[MJ] actor=" + actor + " spawn skipped already source=" + source);
			return true;
		}

		if (!TrySpawnJoinerAtSpawnPoint(player))
		{
			Debug.LogWarning("[MJ] actor=" + actor + " targeted spawn attempt failed source=" + source);
			return false;
		}

		SpawnCompleted.Add(actor);
		NeedsLateJoinSpawn.Remove(actor);
		Debug.Log("[MJ] actor=" + actor + " spawn sent source=" + source);
		return true;
	}

	private static IEnumerator EnsureLateJoinSpawn(int actor, float timeout)
	{
		float deadline = Time.unscaledTime + timeout;
		while (Time.unscaledTime < deadline)
		{
			if (!StillCatching(actor) || SpawnCompleted.Contains(actor))
			{
				yield break;
			}

			TrySpawnLateJoinerActor(actor, "pipeline");
			if (SpawnCompleted.Contains(actor))
			{
				yield break;
			}
			yield return null;
		}
	}

	internal static bool TrySpawnLateJoiners()
	{
		if (!Enabled || !PhotonNetwork.IsMasterClient || NeedsLateJoinSpawn.Count == 0)
		{
			return false;
		}

		List<int> actors = new List<int>(NeedsLateJoinSpawn);
		int spawned = 0;
		int failed = 0;
		for (int i = 0; i < actors.Count; i++)
		{
			if (TrySpawnLateJoinerActor(actors[i], "PlayerSpawn"))
			{
				spawned++;
			}
			else if (NeedsLateJoinSpawn.Contains(actors[i]))
			{
				failed++;
			}
		}

		if (spawned > 0)
		{
			return true;
		}

		// Still have late joiners we could not spawn this call.  Suppress vanilla
		// only when existing players already spawned; otherwise let vanilla run
		// the first spawn of the scene.
		return failed > 0 && SpawnCompleted.Count > 0;
	}

	private static void ForgetFinishedJoiners()
	{
		if (JoiningActors.Count == 0 || PlayerAnimDone == null)
		{
			return;
		}
		GameDirector gd = GameDirector.instance;
		if (gd?.PlayerList == null)
		{
			return;
		}
		List<int> done = null;
		foreach (int actor in JoiningActors)
		{
			PlayerAvatar avatar = FindAvatarByActor(gd, actor);
			if (avatar != null && PlayerAnimDone.GetValue(avatar) is bool complete && complete &&
				!TransitionUnsafeActors.Contains(actor))
			{
				if (done == null)
				{
					done = new List<int>();
				}
				done.Add(actor);
			}
		}
		if (done == null)
		{
			return;
		}
		for (int i = 0; i < done.Count; i++)
		{
			JoiningActors.Remove(done[i]);
		}
	}

	private static PlayerAvatar FindAvatarByActor(GameDirector gd, int actor)
	{
		for (int i = 0; i < gd.PlayerList.Count; i++)
		{
			PlayerAvatar avatar = gd.PlayerList[i];
			Player owner = avatar != null && avatar.photonView != null ? avatar.photonView.Owner : null;
			if (owner != null && owner.ActorNumber == actor)
			{
				return avatar;
			}
		}
		return null;
	}

	internal static void RepairLocalLoadingBarrier(LoadingUI loadingUi)
	{
		UnstickLateJoiner(loadingUi);
	}

	internal static bool GuardMissingLoadingLevel(LoadingUI loadingUi)
	{
		if (!IsLocalLateJoinRepairActive() || loadingUi == null)
		{
			return true;
		}
		LevelGenerator generator = LevelGenerator.Instance;
		if (generator == null || generator.Level != null)
		{
			return true;
		}

		// LoadingUI.LevelAnimationStart dereferences Level.  Targeted late-join
		// ordering can reach this callback one service cycle before UpdateLevel has
		// populated it; the stock callback then faults before the owner completion
		// event is emitted.  Preserve the UI's completed state and use the real
		// local avatar owner to issue that event instead of letting the null level
		// turn into an infinite loading screen.
		_localBootstrapStage = "ui-level-null";
		TrySetBool(LoadingUiLevelStarted, loadingUi, true);
		TrySetBool(LoadingUiLevelDone, loadingUi, true);
		PlayerAvatar localAvatar = FindLocalAvatar(0);
		bool emitted = false;
		if (localAvatar != null && (!TryReadBool(PlayerAnimDone, localAvatar, out bool done) || !done))
		{
			emitted = TryEmitLocalOwnerCompletion(localAvatar);
			if (emitted)
			{
				_localForcedOwnCompletion = true;
				MarkLocalLoadingComplete();
			}
		}
		PublishPeerState("ui-level-null", ready: false, moduleCount: _localBootstrapModuleCount);
		Debug.LogWarning("[MJ.DIAG] guarded LoadingUI.LevelAnimationStart with null Level actor=" +
			LocalActorNumber() + " ownerRpc=" + (emitted ? "1" : "0") +
			" generated=" + (generator.Generated ? "1" : "0"));
		return false;
	}

	private static void UnstickLateJoiner(LoadingUI loadingUi)
	{
		if (!IsLocalLateJoinRepairActive() || InWaitingLobby())
		{
			return;
		}

		GameDirector gd = GameDirector.instance;
		LevelGenerator lg = LevelGenerator.Instance;
		if (gd == null || lg == null || !lg.Generated)
		{
			// GenerateDone is authoritative. Do not fabricate generation counters,
			// AllPlayersReady or Generated before this point.
			NoteLocalBarrierBlocker("generation", gd, lg, loadingUi);
			return;
		}
		if (!_localLoadingComplete)
		{
			// The local bootstrap invokes the owner's real completion method after
			// Generated. Until then, do not fabricate generation counters or UI state.
			NoteLocalBarrierBlocker("owner-animation", gd, lg, loadingUi);
			return;
		}
		if (Time.unscaledTime < _nextUnstick)
		{
			return;
		}
		_nextUnstick = Time.unscaledTime + 0.2f;

		int repaired = MarkRemoteAnimationsComplete(gd);
		if (repaired > 0)
		{
			_remoteAnimationFlagsRepaired += repaired;
			_lastLocalBarrierBlocker = null;
			_nextDiagnosticPublish = 0f;
			Debug.Log("[MJ.DIAG] local historical flags repaired actor=" + LocalActorNumber() +
				" changed=" + repaired + " total=" + _remoteAnimationFlagsRepaired);
		}

		bool uiComplete = false;
		if (loadingUi != null && LoadingUiLevelDone != null)
		{
			try
			{
				uiComplete = LoadingUiLevelDone.GetValue(loadingUi) is bool done && done;
			}
			catch
			{
			}
		}

		// LoadingUI must consume the repaired snapshot before the latch is cleared.
		// A late PhotonView can be added between frames, so verify the full list again.
		bool allAvatarsComplete = AllPlayerAnimationsComplete(gd);
		if (uiComplete && allAvatarsComplete)
		{
			_localLateJoinPending = false;
			_postBarrierDiagnosticPending = true;
			_lastLocalBarrierBlocker = "complete";
			Debug.Log("[MJ] local deterministic loading barrier complete repairedRemote=" + repaired +
				" state=" + gd.currentState);
			PublishLocalDiagnostic(true);
		}
		else
		{
			NoteLocalBarrierBlocker(!uiComplete ? "loading-ui" : "avatar-flags", gd, lg, loadingUi);
		}
	}

	private static void NoteLocalBarrierBlocker(string blocker, GameDirector director, LevelGenerator generator, LoadingUI loadingUi)
	{
		if (string.Equals(_lastLocalBarrierBlocker, blocker, StringComparison.Ordinal))
		{
			return;
		}
		_lastLocalBarrierBlocker = blocker;
		_nextDiagnosticPublish = 0f;
		Debug.Log("[MJ.DIAG] local barrier blocked=" + blocker +
			" actor=" + LocalActorNumber() +
			" state=" + (director != null ? director.currentState.ToString() : "none") +
			" generated=" + (generator != null && generator.Generated ? "1" : "0") +
			" ownerDone=" + (_localLoadingComplete ? "1" : "0") +
			" ui=" + ReadDiagnosticBool(LoadingUiLevelStarted, loadingUi) +
			"/" + ReadDiagnosticBool(LoadingUiLevelDone, loadingUi));
	}

	private static void TrySetBool(FieldInfo field, object instance, bool value)
	{
		if (field == null || instance == null)
		{
			return;
		}
		try
		{
			field.SetValue(instance, value);
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("[MJ.DIAG] bool field write failed field=" + field.Name + ": " +
				ex.GetType().Name + ": " + ex.Message);
		}
	}

	private static int MarkRemoteAnimationsComplete(GameDirector gd)
	{
		if (gd == null || gd.PlayerList == null || PlayerAnimDone == null)
		{
			return 0;
		}

		int changed = 0;
		for (int i = 0; i < gd.PlayerList.Count; i++)
		{
			PlayerAvatar player = gd.PlayerList[i];
			if (player == null)
			{
				continue;
			}
			bool local = PlayerIsLocal != null && PlayerIsLocal.GetValue(player) is bool isLocal && isLocal;
			if (local)
			{
				continue;
			}
			try
			{
				bool done = PlayerAnimDone.GetValue(player) is bool complete && complete;
				if (!done)
				{
					PlayerAnimDone.SetValue(player, true);
					changed++;
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogWarning("[MJ] local historical loading flag repair failed: " +
					ex.GetType().Name + ": " + ex.Message);
			}
		}
		return changed;
	}

	private static bool AllPlayerAnimationsComplete(GameDirector gd)
	{
		if (gd == null || gd.PlayerList == null || gd.PlayerList.Count == 0 || PlayerAnimDone == null)
		{
			return false;
		}
		for (int i = 0; i < gd.PlayerList.Count; i++)
		{
			PlayerAvatar player = gd.PlayerList[i];
			if (player == null)
			{
				return false;
			}
			try
			{
				if (!(PlayerAnimDone.GetValue(player) is bool done) || !done)
				{
					return false;
				}
			}
			catch
			{
				return false;
			}
		}
		return true;
	}

	private static void Close()
	{
		CloseRoomOnly();
		MarkRunProperty(false);
	}

	private static void CloseRoomOnly()
	{
		if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient)
		{
			return;
		}
		try
		{
			Room room = PhotonNetwork.CurrentRoom;
			if (room != null)
			{
				room.IsOpen = false;
				room.IsVisible = false;
			}
		}
		catch
		{
		}
		try
		{
			SteamManager.instance?.LockLobby();
		}
		catch
		{
		}
	}

	private static void OpenPhoton(bool pub)
	{
		try
		{
			Room room = PhotonNetwork.CurrentRoom;
			if (room == null)
			{
				return;
			}
			if (!room.IsOpen)
			{
				room.IsOpen = true;
			}
			if (pub && !room.IsVisible)
			{
				room.IsVisible = true;
			}
		}
		catch
		{
		}
	}

	private static void MarkRunProperty(bool inRun)
	{
		try
		{
			Room room = PhotonNetwork.CurrentRoom;
			if (room == null)
			{
				return;
			}
			bool marked = RoomInRun();
			if (marked == inRun)
			{
				return;
			}
			room.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { RunProp, inRun } });
		}
		catch
		{
		}
	}

	private static bool RoomInRun()
	{
		try
		{
			Room room = PhotonNetwork.CurrentRoom;
			if (room?.CustomProperties == null)
			{
				return false;
			}
			return room.CustomProperties.TryGetValue(RunProp, out object value) && value is bool flag && flag;
		}
		catch
		{
			return false;
		}
	}

	private static bool WantPublic()
	{
		return _publicSnapshot || DetectPublicLobby();
	}

	private static bool DetectPublicLobby()
	{
		if (RoomCreator.KeepPublic || RoomCreator.CreatePublic)
		{
			return true;
		}
		GameManager gm = GameManager.instance;
		if (gm == null)
		{
			return false;
		}
		try
		{
			if (LobbyTypeField?.GetValue(gm) is GameManager.LobbyTypes lobbyType && lobbyType == GameManager.LobbyTypes.Public)
			{
				return true;
			}
			if (ConnectRandomField?.GetValue(gm) is bool connectRandom && connectRandom)
			{
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private static bool InWaitingLobby()
	{
		try
		{
			if (LobbyMenuOpen.instance != null || MainMenuOpen.instance != null)
			{
				return true;
			}
			if (SemiFunc.MenuLevel() || SemiFunc.RunIsLobbyMenu())
			{
				return true;
			}
		}
		catch
		{
		}
		RunManager rm = RunManager.instance;
		if (rm == null || rm.levelCurrent == null)
		{
			return false;
		}
		return rm.levelCurrent == rm.levelLobbyMenu
			|| rm.levelCurrent == rm.levelMainMenu
			|| rm.levelCurrent == rm.levelSplashScreen;
	}

	public static List<ActorJoinStatus> GetActorStatuses()
	{
		HashSet<int> actors = new HashSet<int>();
		AddTrackedActors(actors, JoiningActors);
		AddTrackedActors(actors, PendingActors);
		AddTrackedActors(actors, SpawnedRpcActors);
		AddTrackedActors(actors, ModulesReadyActors);
		AddTrackedActors(actors, LevelSpawnedActors);
		AddTrackedActors(actors, RunningPipelines);
		AddTrackedActors(actors, NeedsLateJoinSpawn);
		AddTrackedActors(actors, SpawnCompleted);
		AddTrackedActors(actors, GenerateDoneSentActors);
		AddTrackedActors(actors, CompletionRetryActors);
		AddTrackedActors(actors, TransitionUnsafeActors);

		Room room = PhotonNetwork.CurrentRoom;
		List<ActorJoinStatus> rows = new List<ActorJoinStatus>(actors.Count);
		foreach (int actor in actors)
		{
			Player player = room != null ? room.GetPlayer(actor) : null;
			string remoteDiagnostic = ReadRemoteDiagnostic(player);
			PlayerAvatar avatar = FindAvatar(player);
			float since;
			float wait = PendingSince.TryGetValue(actor, out since)
				? Mathf.Max(0f, Time.unscaledTime - since)
				: 0f;
			rows.Add(new ActorJoinStatus
			{
				Actor = actor,
				Name = GetDisplayName(avatar, player != null ? player.NickName : string.Empty),
				InRoom = player != null,
				HasAvatar = avatar != null,
				SpawnedRpc = SpawnedRpcActors.Contains(actor),
				ModulesReady = ModulesReadyActors.Contains(actor),
				SpawnSent = SpawnCompleted.Contains(actor),
				GenerateDone = GenerateDoneSentActors.Contains(actor),
				OwnerLoadingReady = IsJoinerLoadingReady(actor),
				Complete = GenerateDoneSentActors.Contains(actor) && !TransitionUnsafeActors.Contains(actor),
				Running = RunningPipelines.Contains(actor),
				WaitSeconds = wait,
				RemoteDiagnostic = remoteDiagnostic,
				RemoteReady = RemoteDiagnosticReportsMain(remoteDiagnostic) || PeerReportsReady(player)
			});
		}
		rows.Sort((left, right) => left.Actor.CompareTo(right.Actor));
		return rows;
	}

	public static string RetryCatchup(int actor)
	{
		if (!Enabled || !PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom)
		{
			return L.T("server.mid_join_off");
		}
		if (_transitionLock || !CatchupWorldReady())
		{
			return L.T("server.mid_join_switching");
		}
		if (RunningPipelines.Contains(actor))
		{
			return L.T("midjoin.running");
		}

		Room room = PhotonNetwork.CurrentRoom;
		Player target = room != null ? room.GetPlayer(actor) : null;
		if (target == null)
		{
			return L.T("midjoin.left");
		}

		JoiningActors.Add(actor);
		TransitionUnsafeActors.Add(actor);
		PendingActors.Add(actor);
		PendingSince[actor] = Time.unscaledTime;
		SpawnedRpcActors.Remove(actor);
		ModulesReadyActors.Remove(actor);
		LevelSpawnedActors.Remove(actor);
		CompletionRetryActors.Remove(actor);
		ClearJoinerActivityObservation(actor);
		CatchupAttempts.Remove(actor);
		RetryAfter.Remove(actor);
		if (!SpawnCompleted.Contains(actor))
		{
			NeedsLateJoinSpawn.Add(actor);
		}

		StartCatchup(target);
		return RunningPipelines.Contains(actor) ? L.T("midjoin.retry_ok") : L.T("server.mid_join_switching");
	}

	public static void ForgetActor(int actor)
	{
		JoiningActors.Remove(actor);
		PendingActors.Remove(actor);
		SpawnedRpcActors.Remove(actor);
		ModulesReadyActors.Remove(actor);
		LevelSpawnedActors.Remove(actor);
		RunningPipelines.Remove(actor);
		NeedsLateJoinSpawn.Remove(actor);
		SpawnCompleted.Remove(actor);
		GenerateDoneSentActors.Remove(actor);
		PendingSince.Remove(actor);
		CatchupAttempts.Remove(actor);
		RetryAfter.Remove(actor);
		CompletionRetryActors.Remove(actor);
		TransitionUnsafeActors.Remove(actor);
		ClearJoinerActivityObservation(actor);
		Debug.Log("[MJ] actor=" + actor + " cleared from the join pipeline");
	}

	private static void AddTrackedActors(HashSet<int> actors, IEnumerable<int> source)
	{
		foreach (int actor in source)
		{
			if (actor > 0)
			{
				actors.Add(actor);
			}
		}
	}

	private static string ReadRemoteDiagnostic(Player player)
	{
		if (player?.CustomProperties == null)
		{
			return string.Empty;
		}
		try
		{
			string diagnostic = player.CustomProperties.TryGetValue(DiagnosticProp, out object value) && value is string text
				? text
				: string.Empty;
			string peer = DescribePeerState(player);
			if (string.IsNullOrEmpty(peer))
			{
				return diagnostic;
			}
			return string.IsNullOrEmpty(diagnostic) ? peer : diagnostic + ";" + peer;
		}
		catch
		{
			return string.Empty;
		}
	}

	private static bool PeerSupportsLocalBootstrap(Player player)
	{
		return TryReadPlayerPropertyInt(player, PeerProtocolProp, out int version) && version >= PeerProtocolVersion;
	}

	private static bool PeerReportsReady(Player player)
	{
		if (!PeerSupportsLocalBootstrap(player) || player?.CustomProperties == null)
		{
			return false;
		}
		try
		{
			return player.CustomProperties.TryGetValue(PeerReadyProp, out object value) &&
				value != null && System.Convert.ToInt64(value) > 0L;
		}
		catch
		{
			return false;
		}
	}

	private static bool TryReadPlayerPropertyInt(Player player, string key, out int result)
	{
		result = 0;
		if (player?.CustomProperties == null)
		{
			return false;
		}
		try
		{
			if (!player.CustomProperties.TryGetValue(key, out object value) || value == null)
			{
				return false;
			}
			result = System.Convert.ToInt32(value);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static string DescribePeerState(Player player)
	{
		if (!PeerSupportsLocalBootstrap(player))
		{
			return string.Empty;
		}
		try
		{
			int protocol;
			if (!TryReadPlayerPropertyInt(player, PeerProtocolProp, out protocol))
			{
				protocol = 0;
			}
			int modules;
			if (!TryReadPlayerPropertyInt(player, PeerModuleCountProp, out modules))
			{
				modules = -1;
			}
			string stage = player.CustomProperties.TryGetValue(PeerStageProp, out object stageValue) && stageValue != null
				? stageValue.ToString()
				: "?";
			return "peer=p" + protocol + ",ready=" + (PeerReportsReady(player) ? "1" : "0") +
				",modules=" + modules + ",stage=" + stage;
		}
		catch
		{
			return "peer=read-error";
		}
	}

	internal static string GetDisplayName(PlayerAvatar avatar, string fallback = null)
	{
		string playerName = ReadAvatarString(PlayerName, avatar);
		if (!IsPlaceholderName(playerName))
		{
			return playerName.Trim();
		}
		string nickname = avatar != null && avatar.photonView != null && avatar.photonView.Owner != null
			? avatar.photonView.Owner.NickName
			: fallback;
		if (!IsPlaceholderName(nickname))
		{
			return nickname.Trim();
		}
		return !string.IsNullOrWhiteSpace(fallback) ? fallback.Trim() : "User";
	}

	private static string ReadAvatarString(FieldInfo field, PlayerAvatar avatar)
	{
		if (field == null || avatar == null)
		{
			return null;
		}
		try
		{
			return field.GetValue(avatar) as string;
		}
		catch
		{
			return null;
		}
	}

	private static bool IsPlaceholderName(string value)
	{
		return string.IsNullOrWhiteSpace(value) ||
			string.Equals(value.Trim(), "User", System.StringComparison.OrdinalIgnoreCase) ||
			string.Equals(value.Trim(), "Unknown", System.StringComparison.OrdinalIgnoreCase);
	}

	private static bool RemoteDiagnosticReportsMain(string diagnostic)
	{
		return !string.IsNullOrEmpty(diagnostic) &&
			diagnostic.IndexOf(";late=0", StringComparison.Ordinal) >= 0 &&
			diagnostic.IndexOf(";state=Main", StringComparison.Ordinal) >= 0;
	}

	public static string StatusKey()
	{
		if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
		{
			return Enabled ? "server.mid_join_armed" : "server.mid_join_off";
		}
		if (_transitionLock)
		{
			return "server.mid_join_switching";
		}
		return PhotonNetwork.CurrentRoom.IsOpen ? "server.mid_join_open" : "server.mid_join_closed";
	}
}

[HarmonyPatch(typeof(SteamManager), "LockLobby")]
public static class MidJoinLockLobbyPatch
{
	private static bool Prefix()
	{
		return !MidJoin.Enabled || MidJoin.TransitionLocked;
	}
}

[HarmonyPatch(typeof(MenuPageLobby), "ButtonStart")]
public static class MidJoinButtonStartPatch
{
	private static void Prefix()
	{
		MidJoin.CaptureVisibility();
	}

	private static void Postfix()
	{
		if (MidJoin.Enabled)
		{
			MidJoin.Open();
		}
	}
}

[HarmonyPatch(typeof(NetworkManager), "OnPlayerEnteredRoom")]
public static class MidJoinPlayerEnteredPatch
{
	private static void Postfix(Player newPlayer)
	{
		MidJoin.HandlePlayerEntered(newPlayer);
	}
}

[HarmonyPatch(typeof(NetworkManager), "OnPlayerLeftRoom")]
public static class MidJoinPlayerLeftPatch
{
	private static void Postfix(Player otherPlayer)
	{
		MidJoin.HandlePlayerLeft(otherPlayer);
	}
}

[HarmonyPatch(typeof(NetworkManager), "PlayerSpawnedRPC")]
public static class MidJoinPlayerSpawnedRpcPatch
{
	private static void Postfix(PhotonMessageInfo _info)
	{
		MidJoin.HandlePlayerSpawnedRpc(_info.Sender);
	}
}

[HarmonyPatch(typeof(LevelGenerator), "Start")]
public static class MidJoinLevelGeneratorStartPatch
{
	private static void Postfix()
	{
		MidJoin.OnLevelGeneratorStarted();
	}
}

[HarmonyPatch(typeof(LevelGenerator), "ModulesReadyRPC")]
public static class MidJoinModulesReadyRpcPatch
{
	private static void Postfix(PhotonMessageInfo _info)
	{
		MidJoin.HandleModulesReadyRpc(_info.Sender);
	}
}

[HarmonyPatch(typeof(LevelGenerator), "PlayerSpawnedRPC")]
public static class MidJoinLevelPlayerSpawnedRpcPatch
{
	private static void Postfix(PhotonMessageInfo _info)
	{
		MidJoin.HandleLevelSpawnedRpc(_info.Sender);
	}
}

[HarmonyPatch(typeof(PlayerAvatar), "Start")]
public static class MidJoinLateJoinAvatarStartPatch
{
	private static void Postfix(PlayerAvatar __instance)
	{
		MidJoin.HandleLateJoinAvatarStarted(__instance);
	}
}

[HarmonyPatch(typeof(PlayerAvatar), "AddToStatsManagerRPC")]
public static class MidJoinPlayerIdentityPatch
{
	private static void Postfix(PlayerAvatar __instance, string _playerName, PhotonMessageInfo _info)
	{
		MidJoin.HandlePlayerIdentityRpc(__instance, _playerName, _info);
	}
}

[HarmonyPatch(typeof(WorldSpaceUIParent), "PlayerName")]
public static class MidJoinPlayerNameUiPatch
{
	private static void Postfix(PlayerAvatar _player)
	{
		MidJoin.HandlePlayerNameUiCreated(_player);
	}
}

[HarmonyPatch(typeof(NetworkConnect), "OnJoinedRoom")]
public static class MidJoinLocalJoinedPatch
{
	private static void Postfix()
	{
		MidJoin.OnLocalJoinedRoom();
	}
}

[HarmonyPatch(typeof(RunManager), "ChangeLevel")]
public static class MidJoinChangeLevelPatch
{
	private static void Prefix(RunManager.ChangeLevelType _changeLevelType)
	{
		MidJoin.BeginTransitionLock(_changeLevelType);
	}
}

[HarmonyPatch(typeof(RunManager), "RestartScene")]
public static class MidJoinRestartScenePatch
{
	private static void Prefix()
	{
		MidJoin.PrepareRestartScene();
	}
}

[HarmonyPatch(typeof(PlayerAvatar), "OutroStart")]
public static class MidJoinOutroStartPatch
{
	private static void Postfix(PlayerAvatar __instance)
	{
		MidJoin.HandleOutroStart(__instance);
	}
}

[HarmonyPatch(typeof(RoundDirector), "ExtractionCompletedAllRPC")]
public static class MidJoinExtractionCompletedAllPatch
{
	private static void Postfix()
	{
		MidJoin.BeginEndOfRunLock();
	}
}

[HarmonyPatch(typeof(LevelGenerator), "PlayerSpawn")]
public static class MidJoinPlayerSpawnPatch
{
	private static bool Prefix(LevelGenerator __instance)
	{
		if (!PhotonNetwork.IsMasterClient || __instance == null || !__instance.Generated)
		{
			return true;
		}
		return !MidJoin.TrySpawnLateJoiners();
	}
}

[HarmonyPatch(typeof(PlayerAvatar), "JumpRPC")]
public static class MidJoinJumpRpcPatch
{
	private static void Postfix(PlayerAvatar __instance, bool _powerupEffect, PhotonMessageInfo _info)
	{
		MidJoin.HandleJoinerJumpRpc(__instance, _info);
	}
}

[HarmonyPatch(typeof(PlayerAvatar), "LoadingLevelAnimationCompleted")]
public static class MidJoinLoadingCompletePatch
{
	private static void Postfix(PlayerAvatar __instance)
	{
		if (!MidJoin.ShouldTrackLocalLoadingCompletion() || __instance == null || __instance.photonView == null || !__instance.photonView.IsMine)
		{
			return;
		}
		MidJoin.MarkLocalLoadingComplete();
	}
}

[HarmonyPatch(typeof(PlayerAvatar), "LoadingLevelAnimationCompletedRPC")]
public static class MidJoinLoadingCompleteRpcPatch
{
	private static bool Prefix(PlayerAvatar __instance, PhotonMessageInfo _info, out MidJoin.LoadingRpcProbe __state)
	{
		__state = MidJoin.CaptureLoadingCompletedRpc(__instance, _info);
		return !MidJoin.TryApplyMasterLoadingCompletion(__instance, _info);
	}

	private static void Postfix(PlayerAvatar __instance, PhotonMessageInfo _info, MidJoin.LoadingRpcProbe __state)
	{
		MidJoin.ObserveLoadingCompletedRpc(__instance, _info, __state);
		MidJoin.HandleLoadingCompletedRpc(__instance, _info);
	}
}

[HarmonyPatch(typeof(LoadingUI), "LateUpdate")]
public static class MidJoinLoadingUiLateUpdatePatch
{
	private static void Postfix(LoadingUI __instance)
	{
		MidJoin.RepairLocalLoadingBarrier(__instance);
	}
}

[HarmonyPatch(typeof(LoadingUI), "LevelAnimationStart")]
public static class MidJoinLoadingUiLevelAnimationStartPatch
{
	private static bool Prefix(LoadingUI __instance)
	{
		return MidJoin.GuardMissingLoadingLevel(__instance);
	}
}

[HarmonyPatch(typeof(ExtractionPoint), "UpdateLock")]
public static class MidJoinExtractionLockPatch
{
	private static void Prefix(ExtractionPoint __instance, bool locked, ref bool __state)
	{
		__state = __instance != null && __instance.isLocked != locked;
	}

	private static void Postfix(ExtractionPoint __instance, bool locked, bool __state)
	{
		if (!__state)
		{
			return;
		}
		MidJoin.BroadcastExtractionLock(__instance, locked);
	}
}
