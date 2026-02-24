using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace r.e.p.o_cheat;

public static class ConfigManager
{
	public static float CurrentSpreadMultiplier = 1f;

	public static bool NoWeaponCooldownEnabled = false;

	public static void SaveToggle(string key, bool value)
	{
		PlayerPrefs.SetInt(key, value ? 1 : 0);
	}

	public static bool LoadToggle(string key, bool defaultValue = false)
	{
		return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
	}

	public static void SaveFloat(string key, float value)
	{
		PlayerPrefs.SetFloat(key, value);
	}

	public static float LoadFloat(string key, float defaultValue = 0f)
	{
		return PlayerPrefs.GetFloat(key, defaultValue);
	}

	public static void SaveColor(string key, Color color)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		PlayerPrefs.SetFloat(key + "_r", color.r);
		PlayerPrefs.SetFloat(key + "_g", color.g);
		PlayerPrefs.SetFloat(key + "_b", color.b);
		PlayerPrefs.SetFloat(key + "_a", color.a);
	}

	public static Color LoadColor(string key, Color defaultColor)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		float num = PlayerPrefs.GetFloat(key + "_r", defaultColor.r);
		float num2 = PlayerPrefs.GetFloat(key + "_g", defaultColor.g);
		float num3 = PlayerPrefs.GetFloat(key + "_b", defaultColor.b);
		float num4 = PlayerPrefs.GetFloat(key + "_a", defaultColor.a);
		return new Color(num, num2, num3, num4);
	}

	public static void SaveInt(string key, int value)
	{
		PlayerPrefs.SetInt(key, value);
	}

	public static int LoadInt(string key, int defaultValue = 0)
	{
		return PlayerPrefs.GetInt(key, defaultValue);
	}

	public static void SaveAllToggles()
	{
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		SaveToggle("God_Mode", Hax2.godModeActive);
		SaveToggle("inf_health", Hax2.infiniteHealthActive);
		SaveToggle("No_Clip", NoclipController.noclipActive);
		SaveToggle("inf_stam", Hax2.stamineState);
		SaveToggle("rgb_player", playerColor.isRandomizing);
		SaveToggle("No_Fog", MiscFeatures.NoFogEnabled);
		SaveToggle("WaterMark_Toggle", Hax2.showWatermark);
		SaveToggle("no_weapon_recoil", Patches.NoWeaponRecoil._isEnabledForConfig);
		SaveToggle("no_weapon_cooldown", NoWeaponCooldownEnabled);
		SaveFloat("strength", Hax2.sliderValueStrength);
		SaveFloat("throw_strength", Hax2.throwStrength);
		SaveFloat("speed", Hax2.sliderValue);
		SaveFloat("grab_Range", Hax2.grabRange);
		SaveFloat("stam_Recharge_Delay", Hax2.staminaRechargeDelay);
		SaveFloat("stam_Recharge_Rate", Hax2.staminaRechargeRate);
		SaveInt("extra_jumps", Hax2.extraJumps);
		SaveFloat("tumble_launch", Hax2.tumbleLaunch);
		SaveFloat("jump_force", Hax2.jumpForce);
		SaveFloat("gravity", Hax2.customGravity);
		SaveFloat("crouch_delay", Hax2.crouchDelay);
		SaveFloat("crouch_speed", Hax2.crouchSpeed);
		SaveFloat("slide_decay", Hax2.slideDecay);
		SaveFloat("flashlight_intensity", Hax2.flashlightIntensity);
		SaveFloat("field_of_view", Hax2.fieldOfView);
		SaveFloat("max_item_distance", DebugCheats.maxItemEspDistance);
		SaveFloat("weapon_spread_multiplier", CurrentSpreadMultiplier);
		SaveInt("min_item_value", DebugCheats.minItemValue);
		SaveToggle("drawEspBool", DebugCheats.drawEspBool);
		SaveToggle("showEnemyBox", DebugCheats.showEnemyBox);
		SaveToggle("drawChamsBool", DebugCheats.drawChamsBool);
		SaveColor("enemy_visible_color", DebugCheats.enemyVisibleColor);
		SaveColor("enemy_invisible_color", DebugCheats.enemyHiddenColor);
		SaveToggle("showEnemyNames", DebugCheats.showEnemyNames);
		SaveToggle("showEnemyDistance", DebugCheats.showEnemyDistance);
		SaveToggle("showEnemyHP", DebugCheats.showEnemyHP);
		SaveToggle("drawItemEspBool", DebugCheats.drawItemEspBool);
		SaveToggle("showItemBox", DebugCheats.draw3DItemEspBool);
		SaveToggle("drawItemChamsBool", DebugCheats.drawItemChamsBool);
		SaveColor("item_visible_color", DebugCheats.itemVisibleColor);
		SaveColor("item_invisible_color", DebugCheats.itemHiddenColor);
		SaveToggle("showItemNames", DebugCheats.showItemNames);
		SaveToggle("showItemDistance", DebugCheats.showItemDistance);
		SaveToggle("showItemValue", DebugCheats.showItemValue);
		SaveToggle("showDeathHeads", DebugCheats.showPlayerDeathHeads);
		SaveToggle("enable_extract_esp", DebugCheats.drawExtractionPointEspBool);
		SaveToggle("show_extract_names", DebugCheats.showExtractionNames);
		SaveToggle("show_extract_distance", DebugCheats.showExtractionDistance);
		SaveToggle("enable_player_esp", DebugCheats.drawPlayerEspBool);
		SaveToggle("show_2d_box_player", DebugCheats.draw2DPlayerEspBool);
		SaveToggle("show_3d_box_player", DebugCheats.draw3DPlayerEspBool);
		SaveToggle("show_names_player", DebugCheats.showPlayerNames);
		SaveToggle("show_distance_player", DebugCheats.showPlayerDistance);
		SaveToggle("show_health_player", DebugCheats.showPlayerHP);
		SaveToggle("show_alive_dead_list", Hax2.showPlayerStatus);
		SaveToggle("blind_enemies", Hax2.blindEnemies);
		PlayerPrefs.Save();
		Debug.Log((object)"Config saved.");
	}

	public static void LoadAllToggles()
	{
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Expected O, but got Unknown
		Hax2.godModeActive = LoadToggle("God_Mode");
		if (Hax2.godModeActive)
		{
			PlayerController.GodMode();
		}
		Hax2.infiniteHealthActive = LoadToggle("inf_health");
		if (Hax2.infiniteHealthActive)
		{
			PlayerController.MaxHealth();
		}
		NoclipController.noclipActive = LoadToggle("No_Clip");
		if (NoclipController.noclipActive)
		{
			NoclipController.ToggleNoclip();
		}
		Hax2.stamineState = LoadToggle("inf_stam");
		if (Hax2.stamineState)
		{
			PlayerController.MaxStamina();
		}
		playerColor.isRandomizing = LoadToggle("rgb_player");
		MiscFeatures.NoFogEnabled = LoadToggle("No_Fog");
		Hax2.showWatermark = LoadToggle("WaterMark_Toggle", defaultValue: true);
		Hax2.debounce = LoadToggle("grab_guard", defaultValue: true);
		Hax2.sliderValueStrength = LoadFloat("strength", Hax2.sliderValueStrength);
		Hax2.throwStrength = LoadFloat("throw_strength", Hax2.throwStrength);
		Hax2.sliderValue = LoadFloat("speed", Hax2.sliderValue);
		Hax2.grabRange = LoadFloat("grab_Range", Hax2.grabRange);
		Hax2.staminaRechargeDelay = LoadFloat("stam_Recharge_Delay", Hax2.staminaRechargeDelay);
		Hax2.staminaRechargeRate = LoadFloat("stam_Recharge_Rate", Hax2.staminaRechargeRate);
		Hax2.extraJumps = LoadInt("extra_jumps", Hax2.extraJumps);
		Hax2.tumbleLaunch = LoadFloat("tumble_launch", Hax2.tumbleLaunch);
		Hax2.jumpForce = LoadFloat("jump_force", Hax2.jumpForce);
		Hax2.customGravity = LoadFloat("gravity", Hax2.customGravity);
		Hax2.crouchDelay = LoadFloat("crouch_delay", Hax2.crouchDelay);
		Hax2.crouchSpeed = LoadFloat("crouch_speed", Hax2.crouchSpeed);
		Hax2.slideDecay = LoadFloat("slide_decay", Hax2.slideDecay);
		Hax2.flashlightIntensity = LoadFloat("flashlight_intensity", Hax2.flashlightIntensity);
		Hax2.fieldOfView = LoadFloat("field_of_view", Hax2.fieldOfView);
		DebugCheats.maxItemEspDistance = LoadFloat("max_item_distance", DebugCheats.maxItemEspDistance);
		DebugCheats.minItemValue = LoadInt("min_item_value", DebugCheats.minItemValue);
		DebugCheats.drawEspBool = LoadToggle("drawEspBool");
		DebugCheats.showEnemyBox = LoadToggle("showEnemyBox", defaultValue: true);
		DebugCheats.drawChamsBool = LoadToggle("drawChamsBool");
		DebugCheats.enemyVisibleColor = LoadColor("enemy_visible_color", DebugCheats.enemyVisibleColor);
		DebugCheats.enemyHiddenColor = LoadColor("enemy_invisible_color", DebugCheats.enemyHiddenColor);
		DebugCheats.showEnemyNames = LoadToggle("showEnemyNames", defaultValue: true);
		DebugCheats.showEnemyDistance = LoadToggle("showEnemyDistance", defaultValue: true);
		DebugCheats.showEnemyHP = LoadToggle("showEnemyHP", defaultValue: true);
		DebugCheats.drawItemEspBool = LoadToggle("drawItemEspBool");
		DebugCheats.draw3DItemEspBool = LoadToggle("showItemBox");
		DebugCheats.drawItemChamsBool = LoadToggle("drawItemChamsBool");
		DebugCheats.itemVisibleColor = LoadColor("item_visible_color", DebugCheats.itemVisibleColor);
		DebugCheats.itemHiddenColor = LoadColor("item_invisible_color", DebugCheats.itemHiddenColor);
		DebugCheats.showItemNames = LoadToggle("showItemNames", defaultValue: true);
		DebugCheats.showItemDistance = LoadToggle("showItemDistance", defaultValue: true);
		DebugCheats.showItemValue = LoadToggle("showItemValue", defaultValue: true);
		DebugCheats.showPlayerDeathHeads = LoadToggle("showDeathHeads", defaultValue: true);
		DebugCheats.drawExtractionPointEspBool = LoadToggle("enable_extract_esp");
		DebugCheats.showExtractionNames = LoadToggle("show_extract_names", defaultValue: true);
		DebugCheats.showExtractionDistance = LoadToggle("show_extract_distance", defaultValue: true);
		DebugCheats.drawPlayerEspBool = LoadToggle("enable_player_esp");
		DebugCheats.draw2DPlayerEspBool = LoadToggle("show_2d_box_player", defaultValue: true);
		DebugCheats.draw3DPlayerEspBool = LoadToggle("show_3d_box_player");
		DebugCheats.showPlayerNames = LoadToggle("show_names_player", defaultValue: true);
		DebugCheats.showPlayerDistance = LoadToggle("show_distance_player", defaultValue: true);
		DebugCheats.showPlayerHP = LoadToggle("show_health_player", defaultValue: true);
		Hax2.showPlayerStatus = LoadToggle("show_alive_dead_list", defaultValue: true);
		Hax2.blindEnemies = LoadToggle("blind_enemies");
		if (PhotonNetwork.IsConnected && PhotonNetwork.LocalPlayer != null)
		{
			Hashtable val = new Hashtable();
			val[(object)"isBlindEnabled"] = Hax2.blindEnemies;
			PhotonNetwork.LocalPlayer.SetCustomProperties(val, (Hashtable)null, (WebFlags)null);
		}
	}
}
