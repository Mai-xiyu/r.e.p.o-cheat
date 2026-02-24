using System;
using System.Collections.Generic;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// 玩家附身系统 - 摄像机跟随目标、禁用本地输入、同步旋转
/// </summary>
public static class PlayerPossession
{
    public static bool isPossessing = false;
    public static string possessTargetName = "";
    private static Transform targetTransform = null;
    private static GameObject localPlayer = null;
    private static List<Renderer> hiddenRenderers = new List<Renderer>();
    private static Vector3 savedPosition;
    private static Quaternion savedRotation;
    private static bool savedRenderersState = false;

    // 摄像机偏移（头部位置）
    private static Vector3 cameraOffset = new Vector3(0f, 1.6f, 0f);
    // 输入禁用相关
    private static bool wasNoclipActive = false;

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

        // 保存当前位置和旋转
        savedPosition = localPlayer.transform.position;
        savedRotation = localPlayer.transform.rotation;

        // 如果Noclip开启则关闭（避免冲突）
        wasNoclipActive = NoclipController.noclipActive;
        if (wasNoclipActive)
        {
            NoclipController.ToggleNoclip();
        }

        // 隐藏自身模型
        HideLocalPlayer();

        // 禁用本地玩家的碰撞体（避免物理干扰）
        SetLocalColliders(false);

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

        // 恢复碰撞体
        SetLocalColliders(true);

        // 传送回保存的位置
        if (localPlayer != null && (UnityEngine.Object)(object)localPlayer != (UnityEngine.Object)null)
        {
            localPlayer.transform.position = savedPosition;
            localPlayer.transform.rotation = savedRotation;
        }

        possessTargetName = "";
        targetTransform = null;

        Debug.Log("[Possess] Stopped possession");
    }

    /// <summary>
    /// 每帧更新 - 跟随目标位置+旋转，同步摄像机
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

            // 同步本地玩家位置和旋转到目标
            Vector3 targetPos = targetTransform.position;
            localPlayer.transform.position = targetPos;
            localPlayer.transform.rotation = targetTransform.rotation;

            // 同步摄像机到目标头部位置
            Camera cam = Camera.main;
            if ((UnityEngine.Object)(object)cam != (UnityEngine.Object)null)
            {
                // 尝试找到目标头部骨骼
                Transform headBone = FindHeadBone(targetTransform);
                if (headBone != null)
                {
                    cam.transform.position = headBone.position + headBone.up * 0.15f;
                    cam.transform.rotation = headBone.rotation;
                }
                else
                {
                    // 使用头部偏移的fallback
                    cam.transform.position = targetPos + cameraOffset;
                    cam.transform.rotation = targetTransform.rotation;
                }
            }

            // 持续禁用本地输入相关定时器（保持摄像机锁定不被覆盖）
            SuppressLocalInput();
        }
        catch
        {
            StopPossession();
        }
    }

    /// <summary>
    /// LateUpdate 中调用 - 确保摄像机在所有Update之后再次同步
    /// </summary>
    public static void LateUpdatePossession()
    {
        if (!isPossessing || targetTransform == null) return;

        try
        {
            if ((UnityEngine.Object)(object)targetTransform == (UnityEngine.Object)null) return;

            Camera cam = Camera.main;
            if ((UnityEngine.Object)(object)cam == (UnityEngine.Object)null) return;

            Transform headBone = FindHeadBone(targetTransform);
            if (headBone != null)
            {
                cam.transform.position = headBone.position + headBone.up * 0.15f;
                cam.transform.rotation = headBone.rotation;
            }
            else
            {
                cam.transform.position = targetTransform.position + cameraOffset;
                cam.transform.rotation = targetTransform.rotation;
            }
        }
        catch { }
    }

    private static Transform cachedHeadBone = null;
    private static Transform cachedHeadTarget = null;

    private static Transform FindHeadBone(Transform target)
    {
        // 缓存头部骨骼避免每帧查找
        if (cachedHeadTarget == target && cachedHeadBone != null &&
            (UnityEngine.Object)(object)cachedHeadBone != (UnityEngine.Object)null)
            return cachedHeadBone;

        cachedHeadTarget = target;
        cachedHeadBone = null;

        // 尝试通过Animator获取头部骨骼
        Animator animator = target.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
            if (head != null)
            {
                cachedHeadBone = head;
                return head;
            }
        }

        // Fallback: 递归查找名称包含"head"的Transform
        Transform found = FindChildRecursive(target, "head");
        if (found != null)
        {
            cachedHeadBone = found;
            return found;
        }

        return null;
    }

    private static Transform FindChildRecursive(Transform parent, string namePart)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name.IndexOf(namePart, StringComparison.OrdinalIgnoreCase) >= 0)
                return child;
            Transform result = FindChildRecursive(child, namePart);
            if (result != null) return result;
        }
        return null;
    }

    private static void SuppressLocalInput()
    {
        try
        {
            // 禁用 InputManager 的瞄准（防止游戏覆盖摄像机）
            if ((UnityEngine.Object)(object)InputManager.instance != (UnityEngine.Object)null)
            {
                FieldInfo field = typeof(InputManager).GetField("disableAimingTimer", BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(InputManager.instance, 9999f);
                }
            }
        }
        catch { }
    }

    private static void SetLocalColliders(bool enabled)
    {
        if (localPlayer == null) return;
        try
        {
            Collider[] colliders = localPlayer.GetComponentsInChildren<Collider>(true);
            foreach (Collider c in colliders)
            {
                if ((UnityEngine.Object)(object)c != (UnityEngine.Object)null)
                    c.enabled = enabled;
            }
        }
        catch { }
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
