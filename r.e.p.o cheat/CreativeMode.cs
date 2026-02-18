using System;
using System.Reflection;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// 创造模式 - 类似Minecraft的创造模式
/// 组合: 上帝模式 + 穿墙 + 无限体力 + 穿墙抓取 + 无武器冷却 + 增强飞行
/// </summary>
public static class CreativeMode
{
    public static bool isCreativeMode = false;

    // 进入创造模式前的状态备份
    private static bool prevGodMode;
    private static bool prevNoclip;
    private static bool prevInfStamina;
    private static bool prevGrabThroughWalls;
    private static bool prevNoWeaponCooldown;
    private static bool prevNoFog;
    private static float prevSpeed;
    private static float prevGrabRange;

    // 创造模式飞行速度
    public static float flySpeed = 15f;
    public static float flySprintMultiplier = 3f;

    // 创造模式自定义抓取距离
    public static float creativeGrabRange = 20f;

    public static void ToggleCreativeMode()
    {
        isCreativeMode = !isCreativeMode;

        if (isCreativeMode)
        {
            EnterCreativeMode();
        }
        else
        {
            ExitCreativeMode();
        }
    }

    private static void EnterCreativeMode()
    {
        // 备份当前状态
        prevGodMode = Hax2.godModeActive;
        prevNoclip = NoclipController.noclipActive;
        prevInfStamina = Hax2.stamineState;
        prevGrabThroughWalls = Hax2.grabThroughWallsEnabled;
        prevNoWeaponCooldown = ConfigManager.NoWeaponCooldownEnabled;
        prevNoFog = MiscFeatures.NoFogEnabled;
        prevSpeed = Hax2.sliderValue;
        prevGrabRange = Hax2.grabRange;

        // 激活所有创造模式子功能
        if (!Hax2.godModeActive)
        {
            Hax2.godModeActive = true;
            PlayerController.GodMode();
        }

        if (!Hax2.infiniteHealthActive)
        {
            Hax2.infiniteHealthActive = true;
            PlayerController.MaxHealth();
        }

        if (!NoclipController.noclipActive)
        {
            NoclipController.noclipActive = true;
            NoclipController.ToggleNoclip();
        }

        if (!Hax2.stamineState)
        {
            Hax2.stamineState = true;
            PlayerController.MaxStamina();
        }

        if (!Hax2.grabThroughWallsEnabled)
        {
            Hax2.grabThroughWallsEnabled = true;
            Patches.ToggleGrabThroughWalls(true);
        }

        if (!ConfigManager.NoWeaponCooldownEnabled)
        {
            ConfigManager.NoWeaponCooldownEnabled = true;
        }

        if (!MiscFeatures.NoFogEnabled)
        {
            MiscFeatures.NoFogEnabled = true;
            MiscFeatures.ToggleNoFog();
        }

        Debug.Log((object)"[创造模式] 已启用 - 上帝模式 + 飞行 + 无限体力 + 穿墙抓取");
    }

    private static void ExitCreativeMode()
    {
        // 还原到之前的状态
        if (!prevGodMode && Hax2.godModeActive)
        {
            Hax2.godModeActive = false;
            PlayerController.GodMode();
        }

        if (!prevNoclip && NoclipController.noclipActive)
        {
            NoclipController.noclipActive = false;
            NoclipController.ToggleNoclip();
        }

        if (!prevInfStamina && Hax2.stamineState)
        {
            Hax2.stamineState = false;
            PlayerController.MaxStamina();
        }

        Hax2.infiniteHealthActive = prevGodMode ? Hax2.infiniteHealthActive : false;

        if (!prevGrabThroughWalls && Hax2.grabThroughWallsEnabled)
        {
            Hax2.grabThroughWallsEnabled = false;
            Patches.ToggleGrabThroughWalls(false);
        }

        if (!prevNoWeaponCooldown)
        {
            ConfigManager.NoWeaponCooldownEnabled = false;
        }

        if (!prevNoFog && MiscFeatures.NoFogEnabled)
        {
            MiscFeatures.NoFogEnabled = false;
            MiscFeatures.ToggleNoFog();
        }

        Debug.Log((object)"[创造模式] 已关闭 - 恢复之前状态");
    }

    /// <summary>
    /// 创造模式增强飞行 - 在Update中调用，替代普通noclip移动
    /// 更平滑，有加速度，支持上下飞行
    /// </summary>
    public static void UpdateCreativeFlight()
    {
        if (!isCreativeMode || !NoclipController.noclipActive)
            return;

        // 创造模式使用增强飞行控制（由NoclipController.UpdateMovement处理）
        // 这里可以添加额外效果，比如飞行粒子等
    }
}
