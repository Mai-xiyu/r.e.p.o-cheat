using System;
using System.Collections.Generic;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// 玩家附身系统 - 传送到目标玩家并隐藏自身，跟随目标移动
/// </summary>
public static class PlayerPossession
{
    public static bool isPossessing = false;
    public static string possessTargetName = "";
    private static Transform targetTransform = null;
    private static GameObject localPlayer = null;
    private static List<Renderer> hiddenRenderers = new List<Renderer>();
    private static Vector3 savedPosition;
    private static bool savedRenderersState = false;

    /// <summary>
    /// 开始附身指定玩家
    /// </summary>
    public static void StartPossession(List<object> playerList, List<string> playerNames, int selectedIndex)
    {
        if (selectedIndex < 0 || selectedIndex >= playerList.Count)
        {
            Debug.Log("[Possess] " + L.T("combat.possess_no_player"));
            return;
        }

        localPlayer = DebugCheats.GetLocalPlayer();
        if ((UnityEngine.Object)(object)localPlayer == (UnityEngine.Object)null)
        {
            Debug.Log("[Possess] Local player not found");
            return;
        }

        // 获取目标
        object target = playerList[selectedIndex];
        if (target == null) return;

        // 检查是否选中自己
        try
        {
            GameObject targetGo = null;
            if (target is PlayerAvatar pa)
            {
                targetGo = ((Component)pa).gameObject;
            }
            else if (target is Component comp)
            {
                targetGo = comp.gameObject;
            }

            if (targetGo != null && targetGo == localPlayer)
            {
                Debug.Log("[Possess] " + L.T("combat.possess_self_warning"));
                return;
            }

            if (targetGo != null)
            {
                targetTransform = targetGo.transform;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[Possess] Failed to get target transform: " + ex.Message);
            return;
        }

        if (targetTransform == null) return;

        possessTargetName = (selectedIndex < playerNames.Count) ? playerNames[selectedIndex] : "Unknown";

        // 保存当前位置
        savedPosition = localPlayer.transform.position;

        // 隐藏自身模型
        HideLocalPlayer();

        isPossessing = true;
        Debug.Log("[Possess] Started possessing: " + possessTargetName);
    }

    /// <summary>
    /// 停止附身
    /// </summary>
    public static void StopPossession()
    {
        if (!isPossessing) return;

        isPossessing = false;

        // 恢复自身模型
        ShowLocalPlayer();

        possessTargetName = "";
        targetTransform = null;

        Debug.Log("[Possess] Stopped possession");
    }

    /// <summary>
    /// 每帧更新 - 跟随目标位置
    /// 在 Update() 中调用
    /// </summary>
    public static void UpdatePossession()
    {
        if (!isPossessing) return;

        if (targetTransform == null || localPlayer == null ||
            (UnityEngine.Object)(object)localPlayer == (UnityEngine.Object)null)
        {
            StopPossession();
            return;
        }

        try
        {
            // 检查目标是否仍然有效
            if ((UnityEngine.Object)(object)targetTransform == (UnityEngine.Object)null)
            {
                StopPossession();
                return;
            }

            // 传送到目标位置（略微偏移避免碰撞）
            Vector3 targetPos = targetTransform.position;
            localPlayer.transform.position = targetPos;
        }
        catch
        {
            StopPossession();
        }
    }

    private static void HideLocalPlayer()
    {
        if (localPlayer == null) return;
        hiddenRenderers.Clear();
        savedRenderersState = true;

        Renderer[] renderers = localPlayer.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            if (r.enabled)
            {
                hiddenRenderers.Add(r);
                r.enabled = false;
            }
        }
    }

    private static void ShowLocalPlayer()
    {
        if (!savedRenderersState) return;

        foreach (Renderer r in hiddenRenderers)
        {
            if ((UnityEngine.Object)(object)r != (UnityEngine.Object)null)
            {
                r.enabled = true;
            }
        }
        hiddenRenderers.Clear();
        savedRenderersState = false;
    }
}
