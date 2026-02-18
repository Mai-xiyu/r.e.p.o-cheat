using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace r.e.p.o_cheat;

public class ForceHost : MonoBehaviourPunCallbacks
{
	public NetworkManager manager;

	public MenuManager menuManager;

	public PhotonView pview;

	private static ForceHost _instance;

	public static ForceHost Instance
	{
		get
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_instance == (Object)null)
			{
				_instance = new GameObject("ForceHost").AddComponent<ForceHost>();
				Object.DontDestroyOnLoad((Object)(object)((Component)_instance).gameObject);
			}
			return _instance;
		}
	}

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

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
	}
}
