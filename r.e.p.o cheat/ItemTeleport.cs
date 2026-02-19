using System;
using System.Collections.Generic;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

public static class ItemTeleport
{
	public class GameItem
	{
		public string Name { get; set; }

		public int Value { get; set; }

		public object ItemObject { get; set; }

		public GameItem(string name, int value, object itemObject = null)
		{
			Name = name;
			Value = value;
			ItemObject = itemObject;
		}
	}

	private static PhotonView punManagerPhotonView;

	public static void SetItemValue(GameItem selectedItem, int newValue)
	{
		if (selectedItem == null || selectedItem.ItemObject == null)
		{
			Debug.Log((object)"错误：所选物品或 ItemObject 为空！");
			return;
		}
		try
		{
			object itemObject = selectedItem.ItemObject;
			Object val = (Object)((itemObject is Object) ? itemObject : null);
			if (val == (Object)null)
			{
				Debug.Log((object)"错误：ItemObject 不是 UnityEngine.Object！");
				return;
			}
			object obj = ((val is GameObject) ? val : null);
			if (obj == null)
			{
				Object obj2 = ((val is Component) ? val : null);
				obj = ((obj2 != null) ? ((Component)obj2).gameObject : null);
			}
			GameObject val2 = (GameObject)obj;
			if ((Object)(object)val2 == (Object)null)
			{
				Debug.Log((object)"错误：无法从 ItemObject 获取 GameObject！");
				return;
			}
			PhotonView component = val2.GetComponent<PhotonView>();
			if ((Object)(object)component != (Object)null)
			{
				component.RPC("DollarValueSetRPC", (RpcTarget)3, new object[1] { (float)newValue });
				Debug.Log((object)$"已通过 RPC 将“{selectedItem.Name}”的价值设置为 ${newValue}");
			}
			else
			{
				Type type = selectedItem.ItemObject.GetType();
				FieldInfo fieldInfo = type.GetField("dollarValueCurrent", BindingFlags.Instance | BindingFlags.Public) ?? type.GetField("dollarValue", BindingFlags.Instance | BindingFlags.Public);
				if (fieldInfo == null)
				{
					Debug.Log((object)("错误：在 “" + selectedItem.Name + "” 中未找到 'dollarValueCurrent' 字段"));
					return;
				}
				fieldInfo.SetValue(selectedItem.ItemObject, newValue);
				Debug.Log((object)$"已在本地将“{selectedItem.Name}”的价值设置为 ${newValue}（未找到 PhotonView）");
			}
			selectedItem.Value = newValue;
		}
		catch (Exception ex)
		{
			Debug.Log((object)("设置“" + selectedItem.Name + "”的价值时出错：" + ex.Message));
		}
	}

	private static void InitializePunManager()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		if ((Object)(object)punManagerPhotonView == (Object)null)
		{
			Type type = Type.GetType("PunManager, Assembly-CSharp");
			object obj = GameHelper.FindObjectOfType(type);
			if (obj != null)
			{
				punManagerPhotonView = (PhotonView)(type.GetField("photonView", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj));
				_ = (Object)(object)punManagerPhotonView == (Object)null;
			}
		}
	}

	public static List<GameItem> GetItemList()
	{
		List<GameItem> list = new List<GameItem>();
		foreach (object valuableObject in DebugCheats.valuableObjects)
		{
			if (valuableObject == null)
			{
				continue;
			}
			PropertyInfo property = valuableObject.GetType().GetProperty("transform", BindingFlags.Instance | BindingFlags.Public);
			if (property == null)
			{
				continue;
			}
			object value = property.GetValue(valuableObject);
			Transform val = (Transform)((value is Transform) ? value : null);
			if ((Object)(object)val == (Object)null || !((Component)val).gameObject.activeInHierarchy)
			{
				continue;
			}
			string text;
			try
			{
				text = valuableObject.GetType().GetProperty("name", BindingFlags.Instance | BindingFlags.Public)?.GetValue(valuableObject) as string;
				if (string.IsNullOrEmpty(text))
				{
					object obj = ((valuableObject is Object) ? valuableObject : null);
					text = ((obj != null) ? ((Object)obj).name : null) ?? "Unknown";
				}
			}
			catch (Exception)
			{
				object obj2 = ((valuableObject is Object) ? valuableObject : null);
				text = ((obj2 != null) ? ((Object)obj2).name : null) ?? "Unknown";
			}
			if (text.StartsWith("Valuable", StringComparison.OrdinalIgnoreCase))
			{
				text = text.Substring("Valuable".Length).Trim();
			}
			if (text.EndsWith("(Clone)", StringComparison.OrdinalIgnoreCase))
			{
				text = text.Substring(0, text.Length - "(Clone)".Length).Trim();
			}
			int value2 = 0;
			if (valuableObject.GetType().Name != "PlayerDeathHead")
			{
				FieldInfo fieldInfo = valuableObject.GetType().GetField("dollarValueCurrent", BindingFlags.Instance | BindingFlags.Public) ?? valuableObject.GetType().GetField("dollarValue", BindingFlags.Instance | BindingFlags.Public);
				if (fieldInfo != null)
				{
					try
					{
						value2 = Convert.ToInt32(fieldInfo.GetValue(valuableObject));
					}
					catch (Exception)
					{
					}
				}
			}
			list.Add(new GameItem(text, value2, valuableObject));
		}
		if (list.Count == 0)
		{
			list.Add(new GameItem(L.T("items.no_items"), 0));
		}
		return list;
	}

	public static void TeleportItemToMe(GameItem selectedItem)
	{
		if (selectedItem != null && selectedItem.ItemObject != null)
		{
			PerformTeleport(selectedItem);
		}
	}

	public static void TeleportAllItemsToMe()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			GameObject localPlayer = DebugCheats.GetLocalPlayer();
			if ((Object)(object)localPlayer == (Object)null)
			{
				return;
			}
			_ = localPlayer.transform.position + localPlayer.transform.forward * 1f + Vector3.up * 1.5f;
			List<GameItem> itemList = GetItemList();
			int num = 0;
			foreach (GameItem item in itemList)
			{
				if (item.ItemObject != null)
				{
					PerformTeleport(item);
					num++;
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public static void TeleportSelectedItemToMe(GameItem selectedItem)
	{
		if (selectedItem != null && selectedItem.ItemObject != null)
		{
			PerformTeleport(selectedItem);
		}
	}

	private static void PerformTeleport(GameItem item)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			GameObject localPlayer = DebugCheats.GetLocalPlayer();
			if ((Object)(object)localPlayer == (Object)null)
			{
				Debug.Log((object)"Local player not found!");
				return;
			}
			Vector3 val = localPlayer.transform.position + localPlayer.transform.forward * 1f + Vector3.up * 1.5f;
			Quaternion rotation = localPlayer.transform.rotation;
			Debug.Log((object)$"Target position for teleport of '{item.Name}': {val}");
			Transform val2 = null;
			PropertyInfo property = item.ItemObject.GetType().GetProperty("transform", BindingFlags.Instance | BindingFlags.Public);
			if (property != null)
			{
				object value = property.GetValue(item.ItemObject);
				val2 = (Transform)((value is Transform) ? value : null);
			}
			else
			{
				object itemObject = item.ItemObject;
				MonoBehaviour val3 = (MonoBehaviour)((itemObject is MonoBehaviour) ? itemObject : null);
				if ((Object)(object)val3 != (Object)null)
				{
					val2 = ((Component)val3).transform;
				}
			}
			if ((Object)(object)val2 == (Object)null)
			{
				Debug.Log((object)("Could not get Transform of item '" + item.Name + "'!"));
				return;
			}
			PhotonView component = ((Component)val2).GetComponent<PhotonView>();
			if ((Object)(object)component == (Object)null)
			{
				Debug.Log((object)("Item '" + item.Name + "' has no PhotonView, performing local teleport only."));
				val2.position = val;
				val2.rotation = rotation;
				return;
			}
			// 使用 ItemTeleportComponent 处理所有权和传送
			ItemTeleportComponent teleportComp = ((Component)val2).GetComponent<ItemTeleportComponent>();
			if ((Object)(object)teleportComp == (Object)null)
			{
				teleportComp = ((Component)val2).gameObject.AddComponent<ItemTeleportComponent>();
			}
			teleportComp.RequestTeleport(val, rotation);
			Debug.Log((object)("Teleport of item '" + item.Name + "' requested via ItemTeleportComponent."));
		}
		catch (Exception ex)
		{
			Debug.Log((object)("Error teleporting item '" + item.Name + "': " + ex.Message));
		}
	}
}
