using System;
using System.Collections.Generic;

namespace r.e.p.o_cheat;

/// <summary>
/// 多语言支持 - 中文/English 一键切换
/// </summary>
public static class LanguageManager
{
    public static string currentLanguage = "zh"; // "zh" 或 "en"

    // 语言字典
    private static Dictionary<string, Dictionary<string, string>> translations = new Dictionary<string, Dictionary<string, string>>()
    {
        // === Tab 名称 ===
        { "tab_self", new Dictionary<string, string> { { "zh", "自身" }, { "en", "Self" } } },
        { "tab_visuals", new Dictionary<string, string> { { "zh", "视觉" }, { "en", "Visuals" } } },
        { "tab_combat", new Dictionary<string, string> { { "zh", "战斗" }, { "en", "Combat" } } },
        { "tab_misc", new Dictionary<string, string> { { "zh", "杂项" }, { "en", "Misc" } } },
        { "tab_enemies", new Dictionary<string, string> { { "zh", "敌人" }, { "en", "Enemies" } } },
        { "tab_items", new Dictionary<string, string> { { "zh", "物品" }, { "en", "Items" } } },
        { "tab_hotkeys", new Dictionary<string, string> { { "zh", "热键" }, { "en", "Hotkeys" } } },
        { "tab_trolling", new Dictionary<string, string> { { "zh", "恶搞" }, { "en", "Trolling" } } },
        { "tab_config", new Dictionary<string, string> { { "zh", "配置" }, { "en", "Config" } } },
        { "tab_servers", new Dictionary<string, string> { { "zh", "服务器" }, { "en", "Servers" } } },
        { "tab_creative", new Dictionary<string, string> { { "zh", "创造" }, { "en", "Creative" } } },
        { "tab_teleport", new Dictionary<string, string> { { "zh", "传送" }, { "en", "Teleport" } } },

        // === 自身 Tab ===
        { "health", new Dictionary<string, string> { { "zh", "生命值" }, { "en", "Health" } } },
        { "god_mode", new Dictionary<string, string> { { "zh", " 上帝模式" }, { "en", " God Mode" } } },
        { "inf_health", new Dictionary<string, string> { { "zh", " 无限生命" }, { "en", " Infinite Health" } } },
        { "movement", new Dictionary<string, string> { { "zh", "移动" }, { "en", "Movement" } } },
        { "noclip", new Dictionary<string, string> { { "zh", " 穿墙模式" }, { "en", " Noclip" } } },
        { "inf_stamina", new Dictionary<string, string> { { "zh", " 无限体力" }, { "en", " Infinite Stamina" } } },
        { "misc", new Dictionary<string, string> { { "zh", "杂项" }, { "en", "Misc" } } },
        { "rgb_player", new Dictionary<string, string> { { "zh", " 玩家RGB颜色" }, { "en", " Player RGB" } } },
        { "no_fog", new Dictionary<string, string> { { "zh", " 去除迷雾" }, { "en", " No Fog" } } },
        { "show_watermark", new Dictionary<string, string> { { "zh", " 显示水印" }, { "en", " Show Watermark" } } },
        { "grab_guard", new Dictionary<string, string> { { "zh", " 防抓取保护" }, { "en", " Grab Guard" } } },
        { "give_crown", new Dictionary<string, string> { { "zh", "给予皇冠" }, { "en", "Give Crown" } } },
        { "no_recoil", new Dictionary<string, string> { { "zh", " 无武器后坐力" }, { "en", " No Recoil" } } },
        { "no_cooldown", new Dictionary<string, string> { { "zh", " 无武器冷却" }, { "en", " No Cooldown" } } },
        { "grab_through_walls", new Dictionary<string, string> { { "zh", " 穿墙抓取" }, { "en", " Grab Through Walls" } } },
        { "player_stats", new Dictionary<string, string> { { "zh", "玩家属性" }, { "en", "Player Stats" } } },
        { "strength", new Dictionary<string, string> { { "zh", "力量" }, { "en", "Strength" } } },
        { "throw_str", new Dictionary<string, string> { { "zh", "投掷力量" }, { "en", "Throw Strength" } } },
        { "speed", new Dictionary<string, string> { { "zh", "速度" }, { "en", "Speed" } } },
        { "grab_range", new Dictionary<string, string> { { "zh", "抓取范围" }, { "en", "Grab Range" } } },
        { "stamina_delay", new Dictionary<string, string> { { "zh", "体力恢复延迟" }, { "en", "Stamina Delay" } } },
        { "stamina_rate", new Dictionary<string, string> { { "zh", "体力恢复速率" }, { "en", "Stamina Rate" } } },
        { "extra_jumps", new Dictionary<string, string> { { "zh", "额外跳跃" }, { "en", "Extra Jumps" } } },
        { "tumble_launch", new Dictionary<string, string> { { "zh", "翻滚发射力度" }, { "en", "Tumble Launch" } } },
        { "jump_force", new Dictionary<string, string> { { "zh", "跳跃力度" }, { "en", "Jump Force" } } },
        { "gravity", new Dictionary<string, string> { { "zh", "重力" }, { "en", "Gravity" } } },
        { "crouch_delay", new Dictionary<string, string> { { "zh", "蹲下延迟" }, { "en", "Crouch Delay" } } },
        { "crouch_speed", new Dictionary<string, string> { { "zh", "蹲下速度" }, { "en", "Crouch Speed" } } },
        { "slide_decay", new Dictionary<string, string> { { "zh", "滑行减速" }, { "en", "Slide Decay" } } },
        { "flashlight", new Dictionary<string, string> { { "zh", "手电筒亮度" }, { "en", "Flashlight" } } },
        { "fov", new Dictionary<string, string> { { "zh", "视野FOV" }, { "en", "FOV" } } },

        // === 创造模式 Tab ===
        { "creative_mode", new Dictionary<string, string> { { "zh", " 创造模式" }, { "en", " Creative Mode" } } },
        { "creative_desc", new Dictionary<string, string> { { "zh", "类似Minecraft创造模式:\n• 无敌 + 飞行 + 无限体力\n• 穿墙抓取 + 无武器冷却\n• 去除迷雾" }, { "en", "Like Minecraft Creative:\n• Invincible + Flight + Infinite Stamina\n• Grab Through Walls + No Cooldown\n• No Fog" } } },
        { "creative_status_on", new Dictionary<string, string> { { "zh", "状态: 创造模式已启用" }, { "en", "Status: Creative Mode ON" } } },
        { "creative_status_off", new Dictionary<string, string> { { "zh", "状态: 生存模式" }, { "en", "Status: Survival Mode" } } },
        { "creative_controls", new Dictionary<string, string> { { "zh", "飞行控制" }, { "en", "Flight Controls" } } },
        { "creative_wasd", new Dictionary<string, string> { { "zh", "WASD - 移动 | Space - 上升 | Ctrl - 下降\nShift - 加速飞行" }, { "en", "WASD - Move | Space - Ascend | Ctrl - Descend\nShift - Sprint Fly" } } },

        // === 传送 Tab ===
        { "teleport", new Dictionary<string, string> { { "zh", "传送功能" }, { "en", "Teleport" } } },
        { "tp_crosshair", new Dictionary<string, string> { { "zh", "传送到准心位置" }, { "en", "Teleport to Crosshair" } } },
        { "tp_extraction", new Dictionary<string, string> { { "zh", "传送到最近撤离点" }, { "en", "Teleport to Extraction" } } },
        { "tp_random", new Dictionary<string, string> { { "zh", "随机传送" }, { "en", "Random Teleport" } } },
        { "tp_waypoints", new Dictionary<string, string> { { "zh", "传送点管理" }, { "en", "Waypoint Manager" } } },
        { "tp_save_pos", new Dictionary<string, string> { { "zh", "保存当前位置" }, { "en", "Save Position" } } },
        { "tp_goto", new Dictionary<string, string> { { "zh", "传送到选中点" }, { "en", "Go to Waypoint" } } },
        { "tp_delete", new Dictionary<string, string> { { "zh", "删除选中点" }, { "en", "Delete Waypoint" } } },
        { "tp_name", new Dictionary<string, string> { { "zh", "传送点名称:" }, { "en", "Waypoint Name:" } } },
        { "no_waypoints", new Dictionary<string, string> { { "zh", "暂无保存的传送点" }, { "en", "No saved waypoints" } } },

        // === 自动功能 ===
        { "auto_pickup", new Dictionary<string, string> { { "zh", " 自动拾取" }, { "en", " Auto Pickup" } } },
        { "auto_sell", new Dictionary<string, string> { { "zh", " 自动卖出" }, { "en", " Auto Sell" } } },
        { "pickup_radius", new Dictionary<string, string> { { "zh", "拾取范围" }, { "en", "Pickup Radius" } } },
        { "min_value", new Dictionary<string, string> { { "zh", "最小价值" }, { "en", "Min Value" } } },
        { "pickup_interval", new Dictionary<string, string> { { "zh", "拾取间隔" }, { "en", "Pickup Interval" } } },

        // === 自动闪避 ===
        { "auto_dodge", new Dictionary<string, string> { { "zh", " 自动闪避" }, { "en", " Auto Dodge" } } },
        { "dodge_distance", new Dictionary<string, string> { { "zh", "闪避距离" }, { "en", "Dodge Distance" } } },
        { "dodge_cooldown", new Dictionary<string, string> { { "zh", "闪避冷却" }, { "en", "Dodge Cooldown" } } },

        // === 雷达 ===
        { "mini_radar", new Dictionary<string, string> { { "zh", " 小地图雷达" }, { "en", " Mini Radar" } } },
        { "radar_range", new Dictionary<string, string> { { "zh", "扫描范围" }, { "en", "Scan Range" } } },

        // === ESP增强 ===
        { "trace_lines", new Dictionary<string, string> { { "zh", "追踪线" }, { "en", "Trace Lines" } } },
        { "trace_enemy", new Dictionary<string, string> { { "zh", " 敌人追踪线" }, { "en", " Enemy Trace" } } },
        { "trace_item", new Dictionary<string, string> { { "zh", " 物品追踪线" }, { "en", " Item Trace" } } },
        { "trace_player", new Dictionary<string, string> { { "zh", " 玩家追踪线" }, { "en", " Player Trace" } } },
        { "esp_presets", new Dictionary<string, string> { { "zh", "ESP预设" }, { "en", "ESP Presets" } } },

        // === 配置 ===
        { "config", new Dictionary<string, string> { { "zh", "配置" }, { "en", "Config" } } },
        { "save_config", new Dictionary<string, string> { { "zh", "保存配置" }, { "en", "Save Config" } } },
        { "load_config", new Dictionary<string, string> { { "zh", "加载配置" }, { "en", "Load Config" } } },
        { "save_json", new Dictionary<string, string> { { "zh", "保存为JSON文件" }, { "en", "Save as JSON" } } },
        { "load_json", new Dictionary<string, string> { { "zh", "从JSON文件加载" }, { "en", "Load from JSON" } } },
        { "config_saved", new Dictionary<string, string> { { "zh", "配置已保存" }, { "en", "Config Saved" } } },
        { "config_loaded", new Dictionary<string, string> { { "zh", "配置已加载" }, { "en", "Config Loaded" } } },
        { "json_saved", new Dictionary<string, string> { { "zh", "JSON配置已保存" }, { "en", "JSON Config Saved" } } },
        { "json_loaded", new Dictionary<string, string> { { "zh", "JSON配置已加载" }, { "en", "JSON Config Loaded" } } },
        { "language", new Dictionary<string, string> { { "zh", "语言/Language" }, { "en", "Language/语言" } } },
        { "switch_lang", new Dictionary<string, string> { { "zh", "切换为English" }, { "en", "切换为中文" } } },
        { "waiting", new Dictionary<string, string> { { "zh", "等待操作..." }, { "en", "Waiting..." } } },

        // === 敌人 ===
        { "select_enemy", new Dictionary<string, string> { { "zh", "选择敌人:" }, { "en", "Select Enemy:" } } },
        { "enemy_esp", new Dictionary<string, string> { { "zh", "敌人透视" }, { "en", "Enemy ESP" } } },
        { "item_esp", new Dictionary<string, string> { { "zh", "物品透视" }, { "en", "Item ESP" } } },
        { "extract_esp", new Dictionary<string, string> { { "zh", "撤离点透视" }, { "en", "Extraction ESP" } } },
        { "player_esp", new Dictionary<string, string> { { "zh", "玩家透视" }, { "en", "Player ESP" } } },
        { "enable_enemy_esp", new Dictionary<string, string> { { "zh", " 启用敌人透视" }, { "en", " Enable Enemy ESP" } } },
        { "enable_item_esp", new Dictionary<string, string> { { "zh", " 启用物品透视" }, { "en", " Enable Item ESP" } } },
        { "enable_extract_esp", new Dictionary<string, string> { { "zh", " 启用撤离透视" }, { "en", " Enable Extract ESP" } } },
        { "enable_player_esp", new Dictionary<string, string> { { "zh", " 启用玩家透视" }, { "en", " Enable Player ESP" } } },
        { "use_modern_esp", new Dictionary<string, string> { { "zh", " 使用现代ESP" }, { "en", " Use Modern ESP" } } },

        // === 视觉子选项 ===
        { "show_2d_box", new Dictionary<string, string> { { "zh", " 显示2D方框" }, { "en", " Show 2D Box" } } },
        { "show_3d_box", new Dictionary<string, string> { { "zh", " 显示3D方框" }, { "en", " Show 3D Box" } } },
        { "show_chams", new Dictionary<string, string> { { "zh", " 显示发光模型" }, { "en", " Show Chams" } } },
        { "show_names", new Dictionary<string, string> { { "zh", " 显示名称" }, { "en", " Show Names" } } },
        { "show_distance", new Dictionary<string, string> { { "zh", " 显示距离" }, { "en", " Show Distance" } } },
        { "show_hp", new Dictionary<string, string> { { "zh", " 显示血量" }, { "en", " Show Health" } } },
        { "show_value", new Dictionary<string, string> { { "zh", " 显示价值" }, { "en", " Show Value" } } },
        { "show_death_heads", new Dictionary<string, string> { { "zh", " 显示死亡头颅" }, { "en", " Show Death Skulls" } } },
        { "show_alive_dead", new Dictionary<string, string> { { "zh", " 存活/死亡列表" }, { "en", " Alive/Dead List" } } },

        { "spread_multiplier", new Dictionary<string, string> { { "zh", "扩散倍率" }, { "en", "Spread Multiplier" } } },
    };

    /// <summary>
    /// 获取翻译文本
    /// </summary>
    public static string Get(string key)
    {
        if (translations.TryGetValue(key, out var langDict))
        {
            if (langDict.TryGetValue(currentLanguage, out var text))
            {
                return text;
            }
            // fallback to zh
            if (langDict.TryGetValue("zh", out var zhText))
            {
                return zhText;
            }
        }
        return key; // 返回 key 本身作为 fallback
    }

    /// <summary>
    /// 切换语言
    /// </summary>
    public static void ToggleLanguage()
    {
        currentLanguage = (currentLanguage == "zh") ? "en" : "zh";
    }

    /// <summary>
    /// 获取 Tab 名称数组
    /// </summary>
    public static string[] GetTabNames()
    {
        return new string[]
        {
            Get("tab_self"),
            Get("tab_visuals"),
            Get("tab_combat"),
            Get("tab_misc"),
            Get("tab_enemies"),
            Get("tab_items"),
            Get("tab_hotkeys"),
            Get("tab_trolling"),
            Get("tab_config"),
            Get("tab_servers"),
            Get("tab_creative"),
            Get("tab_teleport")
        };
    }
}
