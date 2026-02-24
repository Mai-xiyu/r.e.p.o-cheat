using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// JSON 配置文件系统 - 替代 PlayerPrefs，支持导入/导出
/// 配置文件保存在游戏目录下的 DarkCheat_Config.json
/// </summary>
public static class JsonConfig
{
    private static string ConfigPath => Path.Combine(Application.persistentDataPath, "DarkCheat_Config.json");

    [Serializable]
    public class CheatConfig
    {
        // 基础开关
        public bool godMode;
        public bool infiniteHealth;
        public bool noclip;
        public bool infiniteStamina;
        public bool creativeMode;
        public bool rgbPlayer;
        public bool noFog;
        public bool showWatermark = true;
        public bool noWeaponRecoil;
        public bool noWeaponCooldown;
        public bool grabThroughWalls;
        public bool blindEnemies;

        // 新功能开关
        public bool autoPickup;
        public bool autoSell;
        public bool autoDodge;
        public bool miniRadar;

        // 属性值
        public float strength = 1f;
        public float throwStrength;
        public float speed = 1f;
        public float grabRange = 1f;
        public float staminaRechargeDelay = 1f;
        public float staminaRechargeRate = 1f;
        public int extraJumps;
        public float tumbleLaunch;
        public float jumpForce = 1f;
        public float gravity = 1f;
        public float crouchDelay;
        public float crouchSpeed;
        public float slideDecay;
        public float flashlightIntensity;
        public float fieldOfView = 70f;
        public float weaponSpreadMultiplier = 1f;

        // ESP 设置
        public bool drawEnemyEsp;
        public bool showEnemyBox = true;
        public bool drawEnemyChams;
        public bool showEnemyNames = true;
        public bool showEnemyDistance = true;
        public bool showEnemyHP = true;

        public bool drawItemEsp;
        public bool showItemBox;
        public bool drawItemChams;
        public bool showItemNames = true;
        public bool showItemDistance = true;
        public bool showItemValue = true;
        public bool showDeathHeads = true;
        public float maxItemDistance = 200f;
        public int minItemValue;

        public bool drawExtractionEsp;
        public bool showExtractionNames = true;
        public bool showExtractionDistance = true;

        public bool drawPlayerEsp;
        public bool show2DPlayerBox = true;
        public bool show3DPlayerBox;
        public bool showPlayerNames = true;
        public bool showPlayerDistance = true;
        public bool showPlayerHP = true;
        public bool showPlayerStatus = true;

        // ESP增强
        public bool traceLineEnemy;
        public bool traceLineItem;
        public bool traceLinePlayer;
        public int espPreset;

        // 颜色
        public float[] enemyVisibleColor = new float[] { 1, 0, 0, 1 };
        public float[] enemyHiddenColor = new float[] { 1, 0.5f, 0, 1 };
        public float[] itemVisibleColor = new float[] { 0, 1, 0, 1 };
        public float[] itemHiddenColor = new float[] { 1, 1, 0, 1 };

        // 自动拾取参数
        public float pickupRadius = 30f;
        public int minPickupValue = 100;
        public float pickupInterval = 1.5f;

        // 自动闪避参数
        public float dodgeDistance = 8f;
        public float dodgeCooldown = 3f;

        // 雷达参数
        public float radarRange = 50f;
        public float radarRadius = 120f;

        // 语言
        public string language = "zh";

        // 传送点
        public List<WaypointData> waypoints = new List<WaypointData>();
    }

    [Serializable]
    public class WaypointData
    {
        public string name;
        public float x, y, z;
        public float rx, ry, rz, rw;
    }

    /// <summary>
    /// 保存所有设置到 JSON 文件
    /// </summary>
    public static void SaveToFile()
    {
        try
        {
            CheatConfig config = new CheatConfig();

            // 基础开关
            config.godMode = Hax2.godModeActive;
            config.infiniteHealth = Hax2.infiniteHealthActive;
            config.noclip = NoclipController.noclipActive;
            config.infiniteStamina = Hax2.stamineState;
            config.creativeMode = CreativeMode.isCreativeMode;
            config.rgbPlayer = playerColor.isRandomizing;
            config.noFog = MiscFeatures.NoFogEnabled;
            config.showWatermark = Hax2.showWatermark;
            config.noWeaponRecoil = Patches.NoWeaponRecoil._isEnabledForConfig;
            config.noWeaponCooldown = ConfigManager.NoWeaponCooldownEnabled;
            config.grabThroughWalls = Hax2.grabThroughWallsEnabled;
            config.blindEnemies = Hax2.blindEnemies;

            // 新功能
            config.autoPickup = AutoPickup.isAutoPickupEnabled;
            config.autoSell = AutoPickup.isAutoSellEnabled;
            config.autoDodge = AutoDodge.isAutoDodgeEnabled;
            config.miniRadar = MiniRadar.isRadarEnabled;

            // 属性值
            config.strength = Hax2.sliderValueStrength;
            config.throwStrength = Hax2.throwStrength;
            config.speed = Hax2.sliderValue;
            config.grabRange = Hax2.grabRange;
            config.staminaRechargeDelay = Hax2.staminaRechargeDelay;
            config.staminaRechargeRate = Hax2.staminaRechargeRate;
            config.extraJumps = Hax2.extraJumps;
            config.tumbleLaunch = Hax2.tumbleLaunch;
            config.jumpForce = Hax2.jumpForce;
            config.gravity = Hax2.customGravity;
            config.crouchDelay = Hax2.crouchDelay;
            config.crouchSpeed = Hax2.crouchSpeed;
            config.slideDecay = Hax2.slideDecay;
            config.flashlightIntensity = Hax2.flashlightIntensity;
            config.fieldOfView = Hax2.fieldOfView;
            config.weaponSpreadMultiplier = ConfigManager.CurrentSpreadMultiplier;

            // ESP
            config.drawEnemyEsp = DebugCheats.drawEspBool;
            config.showEnemyBox = DebugCheats.showEnemyBox;
            config.drawEnemyChams = DebugCheats.drawChamsBool;
            config.showEnemyNames = DebugCheats.showEnemyNames;
            config.showEnemyDistance = DebugCheats.showEnemyDistance;
            config.showEnemyHP = DebugCheats.showEnemyHP;

            config.drawItemEsp = DebugCheats.drawItemEspBool;
            config.showItemBox = DebugCheats.draw3DItemEspBool;
            config.drawItemChams = DebugCheats.drawItemChamsBool;
            config.showItemNames = DebugCheats.showItemNames;
            config.showItemDistance = DebugCheats.showItemDistance;
            config.showItemValue = DebugCheats.showItemValue;
            config.showDeathHeads = DebugCheats.showPlayerDeathHeads;
            config.maxItemDistance = DebugCheats.maxItemEspDistance;
            config.minItemValue = DebugCheats.minItemValue;

            config.drawExtractionEsp = DebugCheats.drawExtractionPointEspBool;
            config.showExtractionNames = DebugCheats.showExtractionNames;
            config.showExtractionDistance = DebugCheats.showExtractionDistance;

            config.drawPlayerEsp = DebugCheats.drawPlayerEspBool;
            config.show2DPlayerBox = DebugCheats.draw2DPlayerEspBool;
            config.show3DPlayerBox = DebugCheats.draw3DPlayerEspBool;
            config.showPlayerNames = DebugCheats.showPlayerNames;
            config.showPlayerDistance = DebugCheats.showPlayerDistance;
            config.showPlayerHP = DebugCheats.showPlayerHP;
            config.showPlayerStatus = Hax2.showPlayerStatus;

            // ESP增强
            config.traceLineEnemy = ESPEnhancements.showTraceLinesEnemy;
            config.traceLineItem = ESPEnhancements.showTraceLinesItem;
            config.traceLinePlayer = ESPEnhancements.showTraceLinesPlayer;
            config.espPreset = (int)ESPEnhancements.currentPreset;

            // 颜色
            config.enemyVisibleColor = ColorToArray(DebugCheats.enemyVisibleColor);
            config.enemyHiddenColor = ColorToArray(DebugCheats.enemyHiddenColor);
            config.itemVisibleColor = ColorToArray(DebugCheats.itemVisibleColor);
            config.itemHiddenColor = ColorToArray(DebugCheats.itemHiddenColor);

            // 自动功能参数
            config.pickupRadius = AutoPickup.pickupRadius;
            config.minPickupValue = AutoPickup.minPickupValue;
            config.pickupInterval = AutoPickup.pickupInterval;
            config.dodgeDistance = AutoDodge.dodgeDistance;
            config.dodgeCooldown = AutoDodge.dodgeCooldown;
            config.radarRange = MiniRadar.radarRange;
            config.radarRadius = MiniRadar.radarRadius;

            // 语言
            config.language = LanguageManager.currentLanguage;

            // 传送点
            config.waypoints = new List<WaypointData>();
            foreach (var wp in TeleportPlus.savedWaypoints)
            {
                config.waypoints.Add(new WaypointData
                {
                    name = wp.Name,
                    x = wp.Position.x, y = wp.Position.y, z = wp.Position.z,
                    rx = wp.Rotation.x, ry = wp.Rotation.y, rz = wp.Rotation.z, rw = wp.Rotation.w
                });
            }

            string json = JsonUtility.ToJson(config, true);
            File.WriteAllText(ConfigPath, json, Encoding.UTF8);
            Debug.Log((object)$"[配置] 已保存到: {ConfigPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError((object)$"[配置] JSON保存失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 从 JSON 文件加载设置
    /// </summary>
    public static void LoadFromFile()
    {
        try
        {
            if (!File.Exists(ConfigPath))
            {
                Debug.Log((object)"[配置] 未找到配置文件，使用默认设置");
                return;
            }

            string json = File.ReadAllText(ConfigPath, Encoding.UTF8);
            CheatConfig config = JsonUtility.FromJson<CheatConfig>(json);
            if (config == null) return;

            // 基础开关
            Hax2.godModeActive = config.godMode;
            Hax2.infiniteHealthActive = config.infiniteHealth;
            Hax2.stamineState = config.infiniteStamina;
            Hax2.showWatermark = config.showWatermark;
            Hax2.blindEnemies = config.blindEnemies;
            Hax2.grabThroughWallsEnabled = config.grabThroughWalls;

            Patches.NoWeaponRecoil._isEnabledForConfig = config.noWeaponRecoil;
            ConfigManager.NoWeaponCooldownEnabled = config.noWeaponCooldown;
            MiscFeatures.NoFogEnabled = config.noFog;
            playerColor.isRandomizing = config.rgbPlayer;

            // 新功能
            AutoPickup.isAutoPickupEnabled = config.autoPickup;
            AutoPickup.isAutoSellEnabled = config.autoSell;
            AutoDodge.isAutoDodgeEnabled = config.autoDodge;
            MiniRadar.isRadarEnabled = config.miniRadar;

            // 属性值
            Hax2.sliderValueStrength = config.strength;
            Hax2.throwStrength = config.throwStrength;
            Hax2.sliderValue = config.speed;
            Hax2.grabRange = config.grabRange;
            Hax2.staminaRechargeDelay = config.staminaRechargeDelay;
            Hax2.staminaRechargeRate = config.staminaRechargeRate;
            Hax2.extraJumps = config.extraJumps;
            Hax2.tumbleLaunch = config.tumbleLaunch;
            Hax2.jumpForce = config.jumpForce;
            Hax2.customGravity = config.gravity;
            Hax2.crouchDelay = config.crouchDelay;
            Hax2.crouchSpeed = config.crouchSpeed;
            Hax2.slideDecay = config.slideDecay;
            Hax2.flashlightIntensity = config.flashlightIntensity;
            Hax2.fieldOfView = config.fieldOfView;
            ConfigManager.CurrentSpreadMultiplier = config.weaponSpreadMultiplier;

            // ESP
            DebugCheats.drawEspBool = config.drawEnemyEsp;
            DebugCheats.showEnemyBox = config.showEnemyBox;
            DebugCheats.drawChamsBool = config.drawEnemyChams;
            DebugCheats.showEnemyNames = config.showEnemyNames;
            DebugCheats.showEnemyDistance = config.showEnemyDistance;
            DebugCheats.showEnemyHP = config.showEnemyHP;

            DebugCheats.drawItemEspBool = config.drawItemEsp;
            DebugCheats.draw3DItemEspBool = config.showItemBox;
            DebugCheats.drawItemChamsBool = config.drawItemChams;
            DebugCheats.showItemNames = config.showItemNames;
            DebugCheats.showItemDistance = config.showItemDistance;
            DebugCheats.showItemValue = config.showItemValue;
            DebugCheats.showPlayerDeathHeads = config.showDeathHeads;
            DebugCheats.maxItemEspDistance = config.maxItemDistance;
            DebugCheats.minItemValue = config.minItemValue;

            DebugCheats.drawExtractionPointEspBool = config.drawExtractionEsp;
            DebugCheats.showExtractionNames = config.showExtractionNames;
            DebugCheats.showExtractionDistance = config.showExtractionDistance;

            DebugCheats.drawPlayerEspBool = config.drawPlayerEsp;
            DebugCheats.draw2DPlayerEspBool = config.show2DPlayerBox;
            DebugCheats.draw3DPlayerEspBool = config.show3DPlayerBox;
            DebugCheats.showPlayerNames = config.showPlayerNames;
            DebugCheats.showPlayerDistance = config.showPlayerDistance;
            DebugCheats.showPlayerHP = config.showPlayerHP;
            Hax2.showPlayerStatus = config.showPlayerStatus;

            // ESP增强
            ESPEnhancements.showTraceLinesEnemy = config.traceLineEnemy;
            ESPEnhancements.showTraceLinesItem = config.traceLineItem;
            ESPEnhancements.showTraceLinesPlayer = config.traceLinePlayer;
            ESPEnhancements.currentPreset = (ESPEnhancements.ESPPreset)config.espPreset;

            // 颜色
            DebugCheats.enemyVisibleColor = ArrayToColor(config.enemyVisibleColor);
            DebugCheats.enemyHiddenColor = ArrayToColor(config.enemyHiddenColor);
            DebugCheats.itemVisibleColor = ArrayToColor(config.itemVisibleColor);
            DebugCheats.itemHiddenColor = ArrayToColor(config.itemHiddenColor);

            // 自动功能参数
            AutoPickup.pickupRadius = config.pickupRadius;
            AutoPickup.minPickupValue = config.minPickupValue;
            AutoPickup.pickupInterval = config.pickupInterval;
            AutoDodge.dodgeDistance = config.dodgeDistance;
            AutoDodge.dodgeCooldown = config.dodgeCooldown;
            MiniRadar.radarRange = config.radarRange;
            MiniRadar.radarRadius = config.radarRadius;

            // 语言
            LanguageManager.currentLanguage = config.language ?? "zh";

            // 传送点
            TeleportPlus.savedWaypoints.Clear();
            if (config.waypoints != null)
            {
                foreach (var wpd in config.waypoints)
                {
                    TeleportPlus.savedWaypoints.Add(new TeleportPlus.Waypoint(
                        wpd.name,
                        new Vector3(wpd.x, wpd.y, wpd.z),
                        new Quaternion(wpd.rx, wpd.ry, wpd.rz, wpd.rw)
                    ));
                }
            }

            Debug.Log((object)$"[配置] 已从文件加载: {ConfigPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError((object)$"[配置] JSON加载失败: {ex.Message}");
        }
    }

    public static string GetConfigPath() => ConfigPath;

    private static float[] ColorToArray(Color c) => new float[] { c.r, c.g, c.b, c.a };

    private static Color ArrayToColor(float[] arr)
    {
        if (arr == null || arr.Length < 4) return Color.white;
        return new Color(arr[0], arr[1], arr[2], arr[3]);
    }
}
