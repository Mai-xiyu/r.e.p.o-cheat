using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// ESP presets. Trace lines are drawn by EspOverlay.
/// </summary>
public static class ESPEnhancements
{
	public static bool showTraceLinesEnemy;
	public static bool showTraceLinesItem;
	public static bool showTraceLinesPlayer;

	public enum ESPPreset
	{
		Custom,
		HighValueOnly,
		EnemyOnly,
		PlayerOnly,
		Everything,
		Stealth
	}

	public static ESPPreset currentPreset = ESPPreset.Custom;

	public static string[] GetPresetNames()
	{
		return new string[]
		{
			L.T("preset.custom"),
			L.T("preset.high_value"),
			L.T("preset.enemy_only"),
			L.T("preset.player_only"),
			L.T("preset.everything"),
			L.T("preset.stealth")
		};
	}

	public static readonly string[] presetNames = new string[]
	{
		"Custom",
		"High Value",
		"Enemy",
		"Player",
		"All",
		"Stealth"
	};

	public static void DrawTraceLines()
	{
	}

	public static void ApplyPreset(ESPPreset preset)
	{
		currentPreset = preset;
		switch (preset)
		{
			case ESPPreset.HighValueOnly:
				DebugCheats.drawEspBool = false;
				DebugCheats.drawItemEspBool = true;
				DebugCheats.drawPlayerEspBool = false;
				DebugCheats.drawExtractionPointEspBool = true;
				DebugCheats.showItemValue = true;
				DebugCheats.showItemNames = true;
				DebugCheats.showItemDistance = true;
				DebugCheats.minItemValue = 500;
				showTraceLinesEnemy = false;
				showTraceLinesItem = true;
				showTraceLinesPlayer = false;
				break;
			case ESPPreset.EnemyOnly:
				DebugCheats.drawEspBool = true;
				DebugCheats.drawItemEspBool = false;
				DebugCheats.drawPlayerEspBool = false;
				DebugCheats.drawExtractionPointEspBool = false;
				DebugCheats.showEnemyNames = true;
				DebugCheats.showEnemyDistance = true;
				DebugCheats.showEnemyHP = true;
				showTraceLinesEnemy = true;
				showTraceLinesItem = false;
				showTraceLinesPlayer = false;
				break;
			case ESPPreset.PlayerOnly:
				DebugCheats.drawEspBool = false;
				DebugCheats.drawItemEspBool = false;
				DebugCheats.drawPlayerEspBool = true;
				DebugCheats.drawExtractionPointEspBool = false;
				DebugCheats.showPlayerNames = true;
				DebugCheats.showPlayerDistance = true;
				DebugCheats.showPlayerHP = true;
				showTraceLinesEnemy = false;
				showTraceLinesItem = false;
				showTraceLinesPlayer = true;
				break;
			case ESPPreset.Everything:
				DebugCheats.drawEspBool = true;
				DebugCheats.drawItemEspBool = true;
				DebugCheats.drawPlayerEspBool = true;
				DebugCheats.drawExtractionPointEspBool = true;
				DebugCheats.showEnemyNames = true;
				DebugCheats.showEnemyDistance = true;
				DebugCheats.showEnemyHP = true;
				DebugCheats.showEnemyBox = true;
				DebugCheats.showItemNames = true;
				DebugCheats.showItemValue = true;
				DebugCheats.showItemDistance = true;
				DebugCheats.showPlayerNames = true;
				DebugCheats.showPlayerDistance = true;
				DebugCheats.showPlayerHP = true;
				DebugCheats.showExtractionNames = true;
				DebugCheats.showExtractionDistance = true;
				showTraceLinesEnemy = true;
				showTraceLinesItem = true;
				showTraceLinesPlayer = true;
				break;
			case ESPPreset.Stealth:
				DebugCheats.drawEspBool = true;
				DebugCheats.drawItemEspBool = false;
				DebugCheats.drawPlayerEspBool = false;
				DebugCheats.drawExtractionPointEspBool = true;
				DebugCheats.showEnemyNames = false;
				DebugCheats.showEnemyDistance = true;
				DebugCheats.showEnemyHP = false;
				DebugCheats.showEnemyBox = false;
				DebugCheats.drawChamsBool = false;
				showTraceLinesEnemy = false;
				showTraceLinesItem = false;
				showTraceLinesPlayer = false;
				break;
		}
	}
}
