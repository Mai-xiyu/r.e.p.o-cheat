using System;
using UnityEngine;

namespace r.e.p.o_cheat;

public static class MapTools
{
	public static bool showMapTweaks;

	public static bool mapDisableHiddenOverlayActive;

	public static bool mapCleanModeActive;

	public static void ClearMapValuables()
	{
		MapValuable[] array = (MapValuable[])(object)Object.FindObjectsOfType(Type.GetType("MapValuable, Assembly-CSharp"));
		for (int i = 0; i < array.Length; i++)
		{
			Object.Destroy((Object)(object)((Component)array[i]).gameObject);
		}
	}

	public static void changeOverlayStatus(bool status)
	{
		MapModule[] array = Object.FindObjectsOfType<MapModule>();
		if (array.Length != 0)
		{
			MapModule[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				((Component)array2[i]).gameObject.SetActive(!status);
			}
		}
	}

	public static void DiscoveryMapValuables()
	{
		foreach (object valuableObject in DebugCheats.valuableObjects)
		{
			ValuableObject val = (ValuableObject)((valuableObject is ValuableObject) ? valuableObject : null);
			if ((Object)(object)val != (Object)null)
			{
				Map.Instance.AddValuable(val);
			}
		}
	}
}
