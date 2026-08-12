using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace r.e.p.o_cheat;

public class UnlimitedBattery : MonoBehaviour
{
	public bool unlimitedBatteryEnabled;

	private float updateInterval = 2f;

	private List<ItemBattery> batteries = new List<ItemBattery>();

	private float nextScanTime;

	private const float SCAN_INTERVAL = 2f;

	private Dictionary<Type, FieldInfo> equippedFieldCache = new Dictionary<Type, FieldInfo>();

	private void Awake()
	{
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		((MonoBehaviour)this).StartCoroutine(BatteryUpdateCoroutine());
	}

	private void OnDestroy()
	{
		((MonoBehaviour)this).StopAllCoroutines();
	}

	private bool IsLocalPlayerHolding(ItemBattery battery)
	{
		if ((Object)(object)battery == (Object)null)
		{
			return false;
		}
		PhysGrabObject component = ((Component)battery).GetComponent<PhysGrabObject>();
		if ((Object)(object)component == (Object)null)
		{
			return false;
		}
		if (component.playerGrabbing != null && component.playerGrabbing.Count > 0)
		{
			foreach (PhysGrabber item in component.playerGrabbing)
			{
				if ((Object)(object)item != (Object)null && item.isLocal)
				{
					return true;
				}
			}
		}
		ItemEquippable component2 = ((Component)battery).GetComponent<ItemEquippable>();
		if ((Object)(object)component2 != (Object)null)
		{
			Type type = ((object)component2).GetType();
			if (!equippedFieldCache.TryGetValue(type, out FieldInfo field))
			{
				field = type.GetField("isEquipped", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				equippedFieldCache[type] = field;
			}
			if (field != null && (bool)field.GetValue(component2))
			{
				if (Inventory.instance != null && Inventory.instance.IsItemEquipped(component2))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void UpdateBatteryCache()
	{
		if (!(Time.time >= nextScanTime))
		{
			return;
		}
		batteries.RemoveAll((ItemBattery b) => (Object)(object)b == (Object)null);
		ItemBattery[] array = SceneCache.GetObjects<ItemBattery>(1f);
		foreach (ItemBattery val in array)
		{
			if (!batteries.Contains(val) && (Object)(object)val != (Object)null)
			{
				batteries.Add(val);
			}
		}
		nextScanTime = Time.time + 2f;
	}

	private IEnumerator BatteryUpdateCoroutine()
	{
		yield return (object)new WaitForSeconds(1f);
		while (true)
		{
			if (unlimitedBatteryEnabled)
			{
				UpdateBatteryCache();
				for (int i = 0; i < batteries.Count; i++)
				{
					ItemBattery val = batteries[i];
					if ((Object)(object)val == (Object)null)
					{
						continue;
					}
					if (IsLocalPlayerHolding(val))
					{
						// the game's own full-charge path: battery level + int bars + visuals
						val.SetBatteryLife(100);
						val.batteryDrainRate = 0f;
					}
					if ((i + 1) % 5 == 0)
					{
						yield return null;
					}
				}
			}
			yield return (object)new WaitForSeconds(updateInterval);
		}
	}
}
