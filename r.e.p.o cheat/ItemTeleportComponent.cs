using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace r.e.p.o_cheat;

public class ItemTeleportComponent : MonoBehaviour, IPunOwnershipCallbacks
{
	private PhotonView photonView;

	private void Awake()
	{
		photonView = ((Component)this).GetComponent<PhotonView>();
		if ((Object)(object)photonView == (Object)null)
		{
			photonView = ((Component)this).gameObject.AddComponent<PhotonView>();
		}
		PhotonNetwork.AddCallbackTarget((object)this);
	}

	private void OnDestroy()
	{
		PhotonNetwork.RemoveCallbackTarget((object)this);
	}

	[PunRPC]
	private void TeleportItemRPC(Vector3 targetPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = targetPosition;
	}

	public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
	{
		_ = (Object)(object)targetView == (Object)(object)photonView;
	}

	public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
	{
		_ = (Object)(object)targetView == (Object)(object)photonView;
	}

	public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
	{
		_ = (Object)(object)targetView == (Object)(object)photonView;
	}
}
