using System;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

internal class playerColor
{
	public static bool isRandomizing = false;

	private static float lastColorChangeTime = 0f;

	private static float changeInterval = 0.1f;

	private static Type colorControllerType;

	private static object colorControllerInstance;

	private static MethodInfo playerSetColorMethod;

	private static PhotonView playerPhotonView;

	private static bool isInitialized = false;

	private static void Initialize()
	{
		if (isInitialized)
		{
			return;
		}
		colorControllerType = Type.GetType("PlayerAvatar, Assembly-CSharp");
		if (colorControllerType == null)
		{
			return;
		}
		colorControllerInstance = null;
		if (PhotonNetwork.IsConnected)
		{
			PhotonView[] array = Object.FindObjectsOfType<PhotonView>();
			foreach (PhotonView val in array)
			{
				if ((Object)(object)val != (Object)null && val.IsMine)
				{
					Component component = ((Component)val).gameObject.GetComponent(colorControllerType);
					if ((Object)(object)component != (Object)null)
					{
						colorControllerInstance = component;
						playerPhotonView = val;
						break;
					}
				}
			}
		}
		else
		{
			Object val2 = Object.FindObjectOfType(colorControllerType);
			if (val2 != (Object)null)
			{
				colorControllerInstance = val2;
				Object obj = ((val2 is MonoBehaviour) ? val2 : null);
				playerPhotonView = ((obj != null) ? ((Component)obj).GetComponent<PhotonView>() : null);
			}
			else
			{
				GameObject localPlayer = DebugCheats.GetLocalPlayer();
				if ((Object)(object)localPlayer != (Object)null)
				{
					Component component2 = localPlayer.GetComponent(colorControllerType);
					if ((Object)(object)component2 != (Object)null)
					{
						colorControllerInstance = component2;
						playerPhotonView = localPlayer.GetComponent<PhotonView>();
					}
				}
			}
		}
		if (colorControllerInstance != null)
		{
			playerSetColorMethod = colorControllerType.GetMethod("PlayerAvatarSetColor", BindingFlags.Instance | BindingFlags.Public);
			if (!(playerSetColorMethod == null))
			{
				isInitialized = true;
			}
		}
	}

	public static void colorRandomizer()
	{
		Initialize();
		if (isInitialized && Time.time - lastColorChangeTime > 5f)
		{
			Reset();
			Initialize();
		}
		if (!isInitialized || colorControllerInstance == null || playerSetColorMethod == null || !isRandomizing || !(Time.time - lastColorChangeTime >= changeInterval))
		{
			return;
		}
		if (PhotonNetwork.IsConnected && ((Object)(object)playerPhotonView == (Object)null || !IsPhotonViewValid(playerPhotonView)))
		{
			Reset();
			Initialize();
			return;
		}
		int num = new System.Random().Next(0, 36);
		try
		{
			playerSetColorMethod.Invoke(colorControllerInstance, new object[1] { num });
			lastColorChangeTime = Time.time;
		}
		catch (Exception)
		{
		}
	}

	private static bool IsPhotonViewValid(PhotonView view)
	{
		if ((Object)(object)view == (Object)null)
		{
			return false;
		}
		if ((Object)(object)((Component)view).gameObject == (Object)null || !((Component)view).gameObject.activeInHierarchy)
		{
			return false;
		}
		if (view.ViewID != 0)
		{
			return view.Owner != null;
		}
		return false;
	}

	public static void Reset()
	{
		isInitialized = false;
		colorControllerType = null;
		colorControllerInstance = null;
		playerSetColorMethod = null;
		playerPhotonView = null;
	}
}
