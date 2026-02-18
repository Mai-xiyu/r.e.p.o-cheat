using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace r.e.p.o_cheat;

public class ModernESP : MonoBehaviour
{
	private static Dictionary<ValuableObject, TextMeshPro> trackedItems = new Dictionary<ValuableObject, TextMeshPro>();

	private static Dictionary<Enemy, TextMeshPro> trackedEnemies = new Dictionary<Enemy, TextMeshPro>();

	public static float itemTextSize = 3f;

	public static float enemyTextSize = 3f;

	public static float sortFromPrice = 0f;

	public static float sortToPrice = 9999f;

	public static bool sortByPrice = false;

	public static void Render()
	{
		if (Hax2.useModernESP)
		{
			RenderItems();
			RenderEnemies();
		}
		else
		{
			ClearItemLabels();
			ClearEnemyLabels();
		}
	}

	private static void RenderItems()
	{
		if (!DebugCheats.drawItemEspBool)
		{
			ClearItemLabels();
			return;
		}
		List<ValuableObject> list = ValuableDirector.instance?.valuableList;
		if (list == null)
		{
			return;
		}
		Camera main = Camera.main;
		foreach (ValuableObject item in list)
		{
			if (!((Object)(object)item == (Object)null) && !((Object)(object)((Component)item).gameObject == (Object)null))
			{
				if (!trackedItems.TryGetValue(item, out var value))
				{
					CreateItemLabel(item, main);
				}
				else
				{
					UpdateItemLabel(item, value, main);
				}
			}
		}
	}

	private static void CreateItemLabel(ValuableObject item, Camera cam)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("ESP_Label_Item_" + ((Object)item).name);
		val.transform.SetParent(((Component)item).transform, false);
		TextMeshPro val2 = val.AddComponent<TextMeshPro>();
		((TMP_Text)val2).fontSize = 3f;
		((Graphic)val2).color = new Color(1f, 1f, 1f, 1f);
		((TMP_Text)val2).alignment = (TextAlignmentOptions)514;
		((TMP_Text)val2).enableWordWrapping = false;
		((TMP_Text)val2).isOverlay = true;
		((TMP_Text)val2).fontSharedMaterial = ((TMP_Asset)((TMP_Text)val2).font).material;
		trackedItems[item] = val2;
		if ((Object)(object)cam != (Object)null)
		{
			val.transform.LookAt(((Component)cam).transform);
			val.transform.Rotate(0f, 180f, 0f);
		}
		UpdateItemLabel(item, val2, cam);
	}

	private static void UpdateItemLabel(ValuableObject item, TextMeshPro label, Camera cam)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Distance(((Component)cam).transform.position, ((Component)item).transform.position);
		float fontSize = Mathf.Clamp(0.2f + num - 1f, 0.2f, itemTextSize);
		float num2 = 0f;
		FieldInfo fieldInfo = ((object)item).GetType().GetField("dollarValueCurrent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? ((object)item).GetType().GetField("dollarValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (fieldInfo != null)
		{
			num2 = Convert.ToSingle(fieldInfo.GetValue(item));
		}
		bool flag = num2 >= sortFromPrice && num2 <= sortToPrice;
		((TMP_Text)label).fontSize = fontSize;
		((Graphic)label).color = new Color(1f, 1f, 1f, 1f);
		((TMP_Text)label).text = ((sortByPrice && !flag) ? "" : GetItemInfo(item));
		if ((Object)(object)cam != (Object)null)
		{
			label.transform.rotation = Quaternion.LookRotation(((Component)cam).transform.forward);
		}
	}

	private static string GetItemInfo(ValuableObject item)
	{
		string text = ((Object)item).name.Replace("Valuable", "").Replace("(Clone)", "").Trim();
		string text2 = "?";
		float num = 0f;
		FieldInfo fieldInfo = ((object)item).GetType().GetField("dollarValueCurrent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? ((object)item).GetType().GetField("dollarValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (fieldInfo != null)
		{
			num = Convert.ToSingle(fieldInfo.GetValue(item));
		}
		if (DebugCheats.showItemNames)
		{
			text2 = text2 + "\n" + text;
		}
		if (DebugCheats.showItemValue)
		{
			text2 += $"\n<b>{num}$</b>";
		}
		return text2;
	}

	public static void ClearItemLabels()
	{
		foreach (TextMeshPro value in trackedItems.Values)
		{
			if ((Object)(object)value != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)value).gameObject);
			}
		}
		trackedItems.Clear();
	}

	private static void RenderEnemies()
	{
		if (!DebugCheats.drawEspBool)
		{
			ClearEnemyLabels();
			return;
		}
		List<EnemyParent> list = EnemyDirector.instance?.enemiesSpawned;
		if (list == null || list.Count == 0)
		{
			return;
		}
		Camera main = Camera.main;
		foreach (EnemyParent item in list)
		{
			Enemy componentInChildren = ((Component)item).GetComponentInChildren<Enemy>();
			if (!((Object)(object)componentInChildren == (Object)null) && !((Object)(object)((Component)componentInChildren).gameObject == (Object)null))
			{
				if (!trackedEnemies.TryGetValue(componentInChildren, out var value))
				{
					CreateEnemyLabel(componentInChildren, main);
				}
				else
				{
					UpdateEnemyLabel(item, componentInChildren, value, main);
				}
			}
		}
	}

	private static void CreateEnemyLabel(Enemy enemy, Camera cam)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("ESP_Label_Enemy_" + ((Object)enemy).name);
		val.transform.SetParent(((Component)enemy).transform, false);
		TextMeshPro val2 = val.AddComponent<TextMeshPro>();
		((TMP_Text)val2).fontSize = 3f;
		((Graphic)val2).color = new Color(1f, 1f, 1f, 1f);
		((TMP_Text)val2).alignment = (TextAlignmentOptions)514;
		((TMP_Text)val2).enableWordWrapping = false;
		((TMP_Text)val2).isOverlay = true;
		((TMP_Text)val2).fontSharedMaterial = ((TMP_Asset)((TMP_Text)val2).font).material;
		trackedEnemies[enemy] = val2;
		if ((Object)(object)cam != (Object)null)
		{
			val.transform.LookAt(((Component)cam).transform);
			val.transform.Rotate(0f, 180f, 0f);
		}
		UpdateEnemyLabel(null, enemy, val2, cam);
	}

	private static void UpdateEnemyLabel(EnemyParent enemyParent, Enemy enemy, TextMeshPro label, Camera cam)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Distance(((Component)cam).transform.position, ((Component)enemy).transform.position);
		float fontSize = Mathf.Clamp(0.2f + num - 1f, 0.2f, enemyTextSize);
		((TMP_Text)label).fontSize = fontSize;
		((Graphic)label).color = new Color(1f, 1f, 1f, 1f);
		((TMP_Text)label).text = GetEnemyInfo(enemyParent, enemy);
		if ((Object)(object)cam != (Object)null)
		{
			label.transform.rotation = Quaternion.LookRotation(((Component)cam).transform.forward);
		}
	}

	private static string GetEnemyInfo(EnemyParent enemyParent, Enemy enemy)
	{
		if ((Object)(object)enemyParent == (Object)null)
		{
			return "";
		}
		string text = enemyParent.enemyName.Replace("Enemy -", "").Replace("(Clone)", "").Trim();
		string text2 = "?";
		if (DebugCheats.showEnemyNames)
		{
			text2 = text2 + "\n" + text;
		}
		if (DebugCheats.showEnemyHP)
		{
			object arg = typeof(EnemyHealth).GetField("healthCurrent", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(((Component)enemy).GetComponent<EnemyHealth>());
			text2 += $"\n<b>{arg}HP</b>";
		}
		return text2;
	}

	public static void ClearEnemyLabels()
	{
		foreach (TextMeshPro value in trackedEnemies.Values)
		{
			if ((Object)(object)value != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)value).gameObject);
			}
		}
		trackedEnemies.Clear();
	}
}
