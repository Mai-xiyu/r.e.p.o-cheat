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

	private static TMP_FontAsset cachedCJKFont = null;
	private static bool fontSearched = false;

	/// <summary>
	/// 获取支持中文的字体 (从游戏资源中查找)
	/// </summary>
	private static TMP_FontAsset GetCJKFont()
	{
		if (cachedCJKFont != null) return cachedCJKFont;
		if (fontSearched) return null;
		fontSearched = true;
		try
		{
			var allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
			foreach (var font in allFonts)
			{
				if (font == null) continue;
				string fontName = font.name.ToLower();
				if (fontName.Contains("cjk") || fontName.Contains("chinese") || fontName.Contains("noto") ||
				    fontName.Contains("sourcehansans") || fontName.Contains("arial") || fontName.Contains("msyh"))
				{
					Debug.Log("[ModernESP] Found CJK font: " + font.name);
					cachedCJKFont = font;
					return font;
				}
			}
			// If no CJK font found by name, try any font with more than 5000 characters
			foreach (var font in allFonts)
			{
				if (font == null || font.name.Contains("Liberation")) continue;
				if (font.characterTable != null && font.characterTable.Count > 3000)
				{
					Debug.Log("[ModernESP] Using font with many chars: " + font.name + " (" + font.characterTable.Count + " chars)");
					cachedCJKFont = font;
					return font;
				}
			}
			Debug.LogWarning("[ModernESP] No CJK font found, using default");
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[ModernESP] Font search error: " + ex.Message);
		}
		return null;
	}

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
		if ((Object)(object)main == (Object)null) return;
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
		var cjkFont = GetCJKFont();
		if (cjkFont != null)
		{
			((TMP_Text)val2).font = cjkFont;
		}
		((TMP_Text)val2).fontSize = 3f;
		((Graphic)val2).color = new Color(1f, 0.95f, 0.3f, 1f); // 黄色
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
		if ((Object)(object)cam == (Object)null) return;
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
		((Graphic)label).color = new Color(1f, 0.95f, 0.3f, 1f); // 黄色
		((TMP_Text)label).text = ((sortByPrice && !flag) ? "" : GetItemInfo(item));
		if ((Object)(object)cam != (Object)null)
		{
			label.transform.rotation = Quaternion.LookRotation(((Component)cam).transform.forward);
		}
	}

	private static string GetItemInfo(ValuableObject item)
	{
		string text = LanguageManager.GetItemName(((Object)item).name);
		var parts = new List<string>();
		float num = 0f;
		FieldInfo fieldInfo = ((object)item).GetType().GetField("dollarValueCurrent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? ((object)item).GetType().GetField("dollarValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (fieldInfo != null)
		{
			num = Convert.ToSingle(fieldInfo.GetValue(item));
		}
		if (DebugCheats.showItemNames)
		{
			parts.Add(text);
		}
		if (DebugCheats.showItemValue)
		{
			parts.Add($"<b>{num}$</b>");
		}
		return string.Join("\n", parts);
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
		if ((Object)(object)main == (Object)null) return;
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
		var cjkFont = GetCJKFont();
		if (cjkFont != null)
		{
			((TMP_Text)val2).font = cjkFont;
		}
		((TMP_Text)val2).fontSize = 3f;
		((Graphic)val2).color = new Color(1f, 0.3f, 0.3f, 1f); // 红色
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
		if ((Object)(object)cam == (Object)null) return;
		float num = Vector3.Distance(((Component)cam).transform.position, ((Component)enemy).transform.position);
		float fontSize = Mathf.Clamp(0.2f + num - 1f, 0.2f, enemyTextSize);
		((TMP_Text)label).fontSize = fontSize;
		((Graphic)label).color = new Color(1f, 0.3f, 0.3f, 1f); // 红色
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
		string text = LanguageManager.GetEnemyName(enemyParent.enemyName);
		var parts = new List<string>();
		if (DebugCheats.showEnemyNames)
		{
			parts.Add(text);
		}
		if (DebugCheats.showEnemyHP)
		{
			object arg = typeof(EnemyHealth).GetField("healthCurrent", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(((Component)enemy).GetComponent<EnemyHealth>());
			parts.Add($"<b>{arg}HP</b>");
		}
		return string.Join("\n", parts);
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
