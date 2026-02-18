using System;
using System.Collections;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

public class CrashLobby : MonoBehaviourPunCallbacks
{
	private static readonly MethodInfo instantiateMethod = typeof(PhotonNetwork).GetMethod("NetworkInstantiate", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[3]
	{
		typeof(InstantiateParameters),
		typeof(bool),
		typeof(bool)
	}, null);

	private static readonly FieldInfo currentLevelPrefixField = typeof(PhotonNetwork).GetField("currentLevelPrefix", BindingFlags.Static | BindingFlags.NonPublic);

	private static CrashLobby _instance;

	public static CrashLobby Instance
	{
		get
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_instance == (Object)null)
			{
				_instance = new GameObject("CrashLobby").AddComponent<CrashLobby>();
				Object.DontDestroyOnLoad((Object)(object)((Component)_instance).gameObject);
			}
			return _instance;
		}
	}

	public static void Crash(Vector3 position)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		((MonoBehaviour)Instance).StartCoroutine(CrashCoroutine(position));
		((MonoBehaviour)Instance).StartCoroutine(CrashCoroutine(position));
		((MonoBehaviour)Instance).StartCoroutine(CrashCoroutine(position));
	}

	private static IEnumerator CrashCoroutine(Vector3 position)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		while (PhotonNetwork.InRoom)
		{
			SpawnItem(position);
			yield return (object)new WaitForSeconds(0.01f);
		}
		Debug.Log((object)"Not In A Room or Exited");
	}

	private static void EnsureItemVisibility(GameObject item)
	{
		item.SetActive(true);
		Renderer[] componentsInChildren = item.GetComponentsInChildren<Renderer>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
		item.layer = LayerMask.NameToLayer("Default");
	}

	public static void SpawnItem(Vector3 position)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_009f: Expected O, but got Unknown
		if (SemiFunc.IsMultiplayer())
		{
			GameObject surplusValuableSmall = AssetManager.instance.surplusValuableSmall;
			object[] array = new object[1] { 4500 };
			object value = currentLevelPrefixField.GetValue(null);
			InstantiateParameters val = new InstantiateParameters("Valuables/" + ((Object)surplusValuableSmall).name, position, Quaternion.identity, (byte)0, array, (byte)value, null, PhotonNetwork.LocalPlayer, PhotonNetwork.ServerTimestamp);
			GameObject val2 = (GameObject)instantiateMethod.Invoke(null, new object[3] { val, true, false });
			EnsureItemVisibility(val2);
			Object.Destroy((Object)val2);
		}
	}
}
