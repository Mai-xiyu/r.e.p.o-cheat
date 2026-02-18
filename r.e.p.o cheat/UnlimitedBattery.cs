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

	private Dictionary<Type, FieldInfo> batteryLifeIntCache = new Dictionary<Type, FieldInfo>();

	private Dictionary<Type, FieldInfo> batteryDrainRateCache = new Dictionary<Type, FieldInfo>();

	private Dictionary<Type, FieldInfo> drainTimerCache = new Dictionary<Type, FieldInfo>();

	private Dictionary<Type, MethodInfo> batteryFullPercentChangeCache = new Dictionary<Type, MethodInfo>();

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
			FieldInfo field = ((object)component2).GetType().GetField("isEquipped", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null && (bool)field.GetValue(component2))
			{
				PlayerAvatar val = SemiFunc.PlayerAvatarLocal();
				if ((Object)(object)val != (Object)null)
				{
					FieldInfo field2 = ((object)val).GetType().GetField("itemSlots", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (field2 != null && field2.GetValue(val) is IEnumerable enumerable)
					{
						foreach (object item2 in enumerable)
						{
							FieldInfo field3 = item2.GetType().GetField("item", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
							if (field3 != null)
							{
								object value = field3.GetValue(item2);
								GameObject val2 = (GameObject)((value is GameObject) ? value : null);
								if ((Object)(object)val2 != (Object)null && (Object)(object)val2 == (Object)(object)((Component)battery).gameObject)
								{
									return true;
								}
							}
						}
					}
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
		ItemBattery[] array = Object.FindObjectsOfType<ItemBattery>();
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
						val.batteryLife = 100f;
						Type type = ((object)val).GetType();
						if (!batteryLifeIntCache.TryGetValue(type, out var value))
						{
							value = type.GetField("batteryLifeInt", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
							batteryLifeIntCache[type] = value;
						}
						if (value != null && (int)value.GetValue(val) < 6)
						{
							value.SetValue(val, 6);
						}
						if (!batteryDrainRateCache.TryGetValue(type, out var value2))
						{
							value2 = type.GetField("batteryDrainRate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
							batteryDrainRateCache[type] = value2;
						}
						if (value2 != null)
						{
							value2.SetValue(val, 0f);
						}
						if (!drainTimerCache.TryGetValue(type, out var value3))
						{
							value3 = type.GetField("drainTimer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
							drainTimerCache[type] = value3;
						}
						if (value3 != null)
						{
							value3.SetValue(val, 0f);
						}
						if (!batteryFullPercentChangeCache.TryGetValue(type, out var value4))
						{
							value4 = type.GetMethod("BatteryFullPercentChange", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
							batteryFullPercentChangeCache[type] = value4;
						}
						if (value4 != null && ((value != null) ? ((int)value.GetValue(val)) : 0) < 6)
						{
							value4.Invoke(val, new object[2] { 6, true });
						}
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
