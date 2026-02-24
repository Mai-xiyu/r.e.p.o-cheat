using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// 一键全升级工具
/// </summary>
public static class UpgradeHelper
{
    /// <summary>
    /// 通过 StatsManager 字典直接修改升级属性（绕过无效的 RPC）
    /// </summary>
    public static void MaxAllUpgrades()
    {
        try
        {
            string steamID = PlayerController.GetLocalPlayerSteamID();
            if (string.IsNullOrEmpty(steamID)) return;

            ApplyUpgradeViaDictionary(steamID, 30, 30, 30, 30, 30, 20);

            // 也直接设置本地 physGrabber
            try
            {
                PlayerAvatar localAvatar = SemiFunc.PlayerAvatarLocal();
                if (localAvatar != null)
                {
                    localAvatar.physGrabber.grabStrength = 30f;
                }
            }
            catch { }

            // Update UI slider values
            Hax2.sliderValueStrength = 30f;
            Hax2.oldSliderValue = 30f;
            Hax2.sliderValue = 30f;
            Hax2.grabRange = 30f;
            Hax2.throwStrength = 30f;
            Hax2.extraJumps = 30;
            Hax2.tumbleLaunch = 20f;

            Debug.Log("[UpgradeHelper] All upgrades maxed via StatsManager!");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[UpgradeHelper] Error: " + ex.Message);
        }
    }

    /// <summary>
    /// 核心方法：通过反射直接修改 PunManager.statsManager 中的升级字典
    /// </summary>
    public static void ApplyUpgradeViaDictionary(string steamID, int grabStrength, int throwStrength,
        int sprintSpeed, int grabRange, int extraJump, int tumbleLaunch)
    {
        Type punType = Type.GetType("PunManager, Assembly-CSharp");
        if (punType == null) return;
        object punMgr = GameHelper.FindObjectOfType(punType);
        if (punMgr == null) return;

        object statsManager = punType.GetField("statsManager", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(punMgr);
        if (statsManager == null) return;

        Type smType = statsManager.GetType();
        SetDictValue(smType, statsManager, "playerUpgradeStrength", steamID, grabStrength);
        SetDictValue(smType, statsManager, "playerUpgradeThrowStrength", steamID, throwStrength);
        SetDictValue(smType, statsManager, "playerUpgradeSprintSpeed", steamID, sprintSpeed);
        SetDictValue(smType, statsManager, "playerUpgradeGrabRange", steamID, grabRange);
        SetDictValue(smType, statsManager, "playerUpgradeExtraJump", steamID, extraJump);
        SetDictValue(smType, statsManager, "playerUpgradeTumbleLaunch", steamID, tumbleLaunch);

        // 直接设置本地物理属性（立即生效，不依赖RPC）
        try
        {
            ApplyLocalPhysicsUpgrades(grabStrength, sprintSpeed, extraJump);
        }
        catch { }

        // 通过 RPC 广播给所有人（包括主机和自己）
        // 无论是否为主机都发送，利用 Photon 的 Trust Client 特性
        try
        {
            PhotonView pv = ((Component)UnityEngine.Object.FindObjectOfType<PunManager>()).GetComponent<PhotonView>();
            if (pv != null)
            {
                // AllBuffered(3) 广播给所有人包括自己，确保主机也收到并更新字典
                pv.RPC("UpgradePlayerGrabStrengthRPC", (RpcTarget)3, new object[] { steamID, grabStrength });
                pv.RPC("UpgradePlayerThrowStrengthRPC", (RpcTarget)3, new object[] { steamID, throwStrength });
                pv.RPC("UpgradePlayerSprintSpeedRPC", (RpcTarget)3, new object[] { steamID, sprintSpeed });
                pv.RPC("UpgradePlayerGrabRangeRPC", (RpcTarget)3, new object[] { steamID, grabRange });
                pv.RPC("UpgradePlayerExtraJumpRPC", (RpcTarget)3, new object[] { steamID, extraJump });
                pv.RPC("UpgradePlayerTumbleLaunchRPC", (RpcTarget)3, new object[] { steamID, tumbleLaunch });
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[Upgrade] RPC broadcast failed: " + ex.Message);
        }

        Debug.Log($"[Upgrade] Applied for {steamID}: GS={grabStrength} TS={throwStrength} SS={sprintSpeed} GR={grabRange} EJ={extraJump} TL={tumbleLaunch}");
    }

    /// <summary>
    /// 直接设置本地玩家的物理属性，绕过 RPC 验证
    /// </summary>
    public static void ApplyLocalPhysicsUpgrades(int grabStrength, int sprintSpeed, int extraJump)
    {
        try
        {
            PlayerAvatar localAvatar = SemiFunc.PlayerAvatarLocal();
            if (localAvatar == null) return;

            // 力量 - physGrabber.grabStrength
            if (localAvatar.physGrabber != null)
                localAvatar.physGrabber.grabStrength = grabStrength;

            // 速度 - 通过反射设置 PlayerController 内部的速度相关字段
            Type pcType = typeof(RunManager).Assembly.GetType("PlayerController");
            if (pcType != null)
            {
                object pcInst = GameHelper.FindObjectOfType(pcType);
                if (pcInst != null)
                {
                    // 尝试设置 sprintSpeed 相关字段
                    var sprintField = pcType.GetField("SprintSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        ?? pcType.GetField("sprintSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (sprintField != null && sprintField.FieldType == typeof(float))
                        sprintField.SetValue(pcInst, (float)sprintSpeed);

                    // 尝试设置 extraJump 相关字段
                    var jumpField = pcType.GetField("ExtraJump", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        ?? pcType.GetField("extraJump", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        ?? pcType.GetField("jumpExtra", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (jumpField != null)
                    {
                        if (jumpField.FieldType == typeof(int))
                            jumpField.SetValue(pcInst, extraJump);
                        else if (jumpField.FieldType == typeof(float))
                            jumpField.SetValue(pcInst, (float)extraJump);
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[Upgrade] Local physics apply failed: " + ex.Message);
        }
    }

    private static void SetDictValue(Type smType, object statsManager, string fieldName, string steamID, int value)
    {
        try
        {
            FieldInfo fi = smType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fi == null) return;
            var dict = fi.GetValue(statsManager) as Dictionary<string, int>;
            if (dict != null)
            {
                dict[steamID] = value;
            }
        }
        catch { }
    }
}

/// <summary>
/// 全物品价值膨胀工具
/// </summary>
public static class ItemInflater
{
    public static void InflateAll(float targetValue = 99999f)
    {
        try
        {
            var items = DebugCheats.valuableObjects;
            if (items == null || items.Count == 0) return;

            int count = 0;
            foreach (object item in items)
            {
                if (item == null) continue;
                try
                {
                    // Check if it's a Unity object that was destroyed
                    UnityEngine.Object unityObj = item as UnityEngine.Object;
                    if (unityObj != null && unityObj == null) continue;

                    PhotonView pv = null;
                    if (item is Component comp)
                        pv = comp.GetComponent<PhotonView>();
                    else
                    {
                        var pvField = item.GetType().GetField("photonView",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (pvField != null)
                            pv = pvField.GetValue(item) as PhotonView;
                    }

                    if (pv != null)
                    {
                        pv.RPC("DollarValueSetRPC", (RpcTarget)0, new object[] { targetValue });
                        count++;
                    }
                }
                catch { }
            }
            Debug.Log($"[ItemInflater] Inflated {count} items to ${targetValue}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[ItemInflater] Error: " + ex.Message);
        }
    }

    public static void MultiplyAll(float multiplier = 10f)
    {
        try
        {
            var items = DebugCheats.valuableObjects;
            if (items == null || items.Count == 0) return;

            foreach (object item in items)
            {
                if (item == null) continue;
                try
                {
                    UnityEngine.Object unityObj = item as UnityEngine.Object;
                    if (unityObj != null && unityObj == null) continue;

                    FieldInfo valueField = item.GetType().GetField("dollarValueCurrent",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        ?? item.GetType().GetField("dollarValue",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    float currentValue = 0f;
                    if (valueField != null)
                        currentValue = Convert.ToSingle(valueField.GetValue(item));

                    float newValue = currentValue * multiplier;

                    PhotonView pv = null;
                    if (item is Component comp)
                        pv = comp.GetComponent<PhotonView>();
                    else
                    {
                        var pvField = item.GetType().GetField("photonView",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (pvField != null)
                            pv = pvField.GetValue(item) as PhotonView;
                    }

                    if (pv != null)
                        pv.RPC("DollarValueSetRPC", (RpcTarget)0, new object[] { newValue });
                }
                catch { }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[ItemInflater] Multiply error: " + ex.Message);
        }
    }
}

/// <summary>
/// 搬运目标归零工具
/// </summary>
public static class HaulGoalZero
{
    public static string statusMessage = "";

    public static void ZeroHaulGoal()
    {
        try
        {
            RoundDirector instance = RoundDirector.instance;
            if (instance == null)
            {
                statusMessage = L.T("haul.no_round");
                return;
            }

            bool found = false;
            // Search for haul/goal related fields via reflection
            string[] fieldNames = { "haulGoal", "extractionHaulGoal", "goalAmount",
                "shopHaulGoal", "currentHaulGoal", "totalHaulGoal", "requiredHaul" };

            foreach (string name in fieldNames)
            {
                FieldInfo field = typeof(RoundDirector).GetField(name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    if (field.FieldType == typeof(int))
                        field.SetValue(instance, 0);
                    else if (field.FieldType == typeof(float))
                        field.SetValue(instance, 0f);
                    found = true;
                    Debug.Log("[HaulGoal] Set " + name + " = 0");
                }
            }

            // Also try property search
            if (!found)
            {
                var allFields = typeof(RoundDirector).GetFields(
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (var f in allFields)
                {
                    string nameLower = f.Name.ToLower();
                    if (nameLower.Contains("haul") || nameLower.Contains("goal") ||
                        nameLower.Contains("target") || nameLower.Contains("quota"))
                    {
                        try
                        {
                            if (f.FieldType == typeof(int))
                            { f.SetValue(instance, 0); found = true; }
                            else if (f.FieldType == typeof(float))
                            { f.SetValue(instance, 0f); found = true; }
                            Debug.Log("[HaulGoal] Found and zeroed: " + f.Name + " (" + f.FieldType + ")");
                        }
                        catch { }
                    }
                }
            }

            statusMessage = found ? L.T("haul.zeroed") : L.T("haul.no_field");
        }
        catch (Exception ex)
        {
            statusMessage = L.T("haul.error", ex.Message);
            Debug.LogWarning("[HaulGoal] Error: " + ex.Message);
        }
    }
}

/// <summary>
/// 物品复制机
/// </summary>
public static class ItemDuplicator
{
    public static bool DuplicateHeldItem()
    {
        try
        {
            object pc = PlayerReflectionCache.PlayerControllerInstance;
            if (pc == null) return false;

            FieldInfo avatarField = pc.GetType().GetField("playerAvatarScript",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (avatarField == null) return false;
            object avatar = avatarField.GetValue(pc);
            if (avatar == null) return false;

            FieldInfo grabberField = avatar.GetType().GetField("physGrabber",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (grabberField == null) return false;
            object grabber = grabberField.GetValue(avatar);
            if (grabber == null) return false;

            FieldInfo grabbedField = grabber.GetType().GetField("grabbed",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (grabbedField == null || !(bool)grabbedField.GetValue(grabber)) return false;

            FieldInfo transformField = grabber.GetType().GetField("grabbedObjectTransform",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (transformField == null) return false;
            Transform grabbedTransform = transformField.GetValue(grabber) as Transform;
            if (grabbedTransform == null) return false;

            string objName = grabbedTransform.gameObject.name;
            if (objName.EndsWith("(Clone)"))
                objName = objName.Substring(0, objName.Length - "(Clone)".Length).Trim();

            GameObject localPlayer = DebugCheats.GetLocalPlayer();
            if (localPlayer == null) return false;
            Vector3 spawnPos = localPlayer.transform.position + localPlayer.transform.forward * 1.5f + Vector3.up;

            // Try spawning via ItemSpawner
            ItemSpawner.SpawnItem(objName, spawnPos);
            Debug.Log("[ItemDuplicator] Duplicated: " + objName);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[ItemDuplicator] Error: " + ex.Message);
            return false;
        }
    }
}

/// <summary>
/// 幽灵模式 (Ghost Mode) — 轻量级网络隐身。
/// 
/// 策略（不使用 PhotonNetwork.Destroy，零崩溃风险）：
///   1. 停止 PhotonView 同步 (Synchronization = Off)
///   2. 禁用 PhotonTransformView / PhotonAnimatorView
///   3. 禁用自身所有远程可见的 Renderer 和 Collider
///   4. 广播虚假远离位置 (RPC)，使对方看到「消失」
///   5. 恢复时还原所有组件状态并广播正确位置
/// 
/// 效果：对方世界里玩家消失（或冻在地图外），
/// 但本地仍然正常游玩、可以攻击/拾取。
/// </summary>
public static class StealthMode
{
    public static bool isEnabled = false;
    public static string statusMessage = "";

    // 备份状态（用于恢复）
    private static ViewSynchronization _originalSync = (ViewSynchronization)3;
    private static readonly List<ComponentState> _disabledRenderers = new List<ComponentState>();
    private static readonly List<ComponentState> _disabledColliders = new List<ComponentState>();
    private static bool _hasBackup = false;

    private struct ComponentState
    {
        public Component component;
        public bool wasEnabled;
    }

    public static void Apply()
    {
        try
        {
            GameObject localPlayer = DebugCheats.GetLocalPlayer();
            if (localPlayer == null) return;

            PhotonView pv = GetPlayerPhotonView(localPlayer);

            if (isEnabled)
                EnableGhostMode(localPlayer, pv);
            else
                DisableGhostMode(localPlayer, pv);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[GhostMode] Error: " + ex.Message);
            statusMessage = "错误: " + ex.Message;
        }
    }

    private static void EnableGhostMode(GameObject localPlayer, PhotonView pv)
    {
        _disabledRenderers.Clear();
        _disabledColliders.Clear();
        _hasBackup = true;

        // ── 1. 停止 PhotonView 同步 ──
        if (pv != null)
        {
            _originalSync = pv.Synchronization;
            pv.Synchronization = (ViewSynchronization)0; // Off
        }

        // ── 2. 禁用 PhotonTransformView / PhotonAnimatorView ──
        DisablePhotonSyncComponents(localPlayer);

        // ── 3. 广播虚假位置 — 把自己「传送」到地图外 ──
        BroadcastFakePosition(localPlayer, pv);

        // ── 4. 禁用远程可见组件 ──
        // 注意：只禁用 Collider 让自己无法被碰到，Renderer 保留本地可见
        // 因为其他玩家看到的是我们最后的同步位置（已在地图外）
        DisableCollidersForGhost(localPlayer);

        statusMessage = "👻 幽灵模式 ON — 对方世界中已消失";
        Debug.Log("[GhostMode] 已启用 — 同步关闭, 虚假位置已广播, 碰撞器已禁用");
    }

    private static void DisableGhostMode(GameObject localPlayer, PhotonView pv)
    {
        // ── 1. 恢复 PhotonView 同步 ──
        if (pv != null && _hasBackup)
        {
            pv.Synchronization = _originalSync;
        }

        // ── 2. 恢复 PhotonTransformView / PhotonAnimatorView ──
        EnablePhotonSyncComponents(localPlayer);

        // ── 3. 恢复碰撞器 ──
        RestoreColliders();

        // ── 4. 广播真实位置 ──
        BroadcastRealPosition(localPlayer, pv);

        _hasBackup = false;
        statusMessage = "幽灵模式 OFF — 已恢复可见";
        Debug.Log("[GhostMode] 已禁用 — 同步恢复, 真实位置已广播");
    }

    // ─── 辅助方法 ──────────────────────────────────────────

    private static PhotonView GetPlayerPhotonView(GameObject localPlayer)
    {
        PhotonView pv = localPlayer.GetComponent<PhotonView>();
        if (pv == null)
        {
            var avatar = localPlayer.GetComponent<PlayerAvatar>();
            if (avatar != null)
            {
                var pvField = avatar.GetType().GetField("photonView",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (pvField != null)
                    pv = pvField.GetValue(avatar) as PhotonView;
            }
        }
        return pv;
    }

    private static void DisablePhotonSyncComponents(GameObject localPlayer)
    {
        try
        {
            // 禁用 PhotonTransformView
            var transformViews = localPlayer.GetComponentsInChildren<PhotonTransformView>(true);
            foreach (var tv in transformViews)
            {
                if ((UnityEngine.Object)(object)tv != (UnityEngine.Object)null && ((Behaviour)tv).enabled)
                {
                    ((Behaviour)tv).enabled = false;
                }
            }

            // 禁用 PhotonAnimatorView（通过反射，因为可能不在当前程序集中）
            Type photonAnimViewType = typeof(PhotonView).Assembly.GetType("Photon.Pun.PhotonAnimatorView");
            if (photonAnimViewType != null)
            {
                var animViews = localPlayer.GetComponentsInChildren(photonAnimViewType, true);
                foreach (var av in animViews)
                {
                    if (av != null && av is Behaviour b && b.enabled)
                    {
                        b.enabled = false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[GhostMode] 禁用同步组件警告: " + ex.Message);
        }
    }

    private static void EnablePhotonSyncComponents(GameObject localPlayer)
    {
        try
        {
            var transformViews = localPlayer.GetComponentsInChildren<PhotonTransformView>(true);
            foreach (var tv in transformViews)
            {
                if ((UnityEngine.Object)(object)tv != (UnityEngine.Object)null)
                    ((Behaviour)tv).enabled = true;
            }

            Type photonAnimViewType = typeof(PhotonView).Assembly.GetType("Photon.Pun.PhotonAnimatorView");
            if (photonAnimViewType != null)
            {
                var animViews = localPlayer.GetComponentsInChildren(photonAnimViewType, true);
                foreach (var av in animViews)
                {
                    if (av != null && av is Behaviour b)
                        b.enabled = true;
                }
            }
        }
        catch { }
    }

    private static void DisableCollidersForGhost(GameObject localPlayer)
    {
        try
        {
            Collider[] colliders = localPlayer.GetComponentsInChildren<Collider>(true);
            foreach (var col in colliders)
            {
                if ((UnityEngine.Object)(object)col == (UnityEngine.Object)null) continue;
                _disabledColliders.Add(new ComponentState { component = col, wasEnabled = col.enabled });
                col.enabled = false;
            }
        }
        catch { }
    }

    private static void RestoreColliders()
    {
        foreach (var state in _disabledColliders)
        {
            try
            {
                if (state.component != null && state.component is Collider col)
                    col.enabled = state.wasEnabled;
            }
            catch { }
        }
        _disabledColliders.Clear();
    }

    private static void BroadcastFakePosition(GameObject localPlayer, PhotonView pv)
    {
        try
        {
            if (pv == null || !PhotonNetwork.IsConnected) return;

            // 发送一次性的位置 RPC，把自己「传送」到地图外
            Vector3 fakePos = new Vector3(9999f, -999f, 9999f);

            var avatar = localPlayer.GetComponent<PlayerAvatar>();
            if (avatar != null && avatar.photonView != null)
            {
                try
                {
                    // 保存真实位置
                    Vector3 realPos = localPlayer.transform.position;
                    Quaternion realRot = localPlayer.transform.rotation;

                    // 临时恢复同步并设置假位置
                    pv.Synchronization = (ViewSynchronization)3;
                    localPlayer.transform.position = fakePos;

                    // 使用协程延迟关闭同步，确保 Photon 至少序列化一帧
                    var runner = UnityEngine.Object.FindObjectOfType<MonoBehaviour>();
                    if (runner != null)
                    {
                        runner.StartCoroutine(
                            DelayedSyncOff(pv, localPlayer, realPos, realRot));
                    }
                    else
                    {
                        // 后备方案：直接关闭（可能不会广播）
                        pv.Synchronization = (ViewSynchronization)0;
                        localPlayer.transform.position = realPos;
                        localPlayer.transform.rotation = realRot;
                    }

                    Debug.Log("[GhostMode] 已广播虚假位置 → (9999, -999, 9999)");
                }
                catch { }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[GhostMode] 广播虚假位置失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 延迟关闭同步并恢复真实位置，确保 Photon 有时间序列化假位置
    /// </summary>
    private static IEnumerator DelayedSyncOff(PhotonView pv, GameObject localPlayer, Vector3 realPos, Quaternion realRot)
    {
        // 等待几帧让 Photon 序列化并发送假位置
        yield return new WaitForSeconds(0.3f);

        if (pv != null)
            pv.Synchronization = (ViewSynchronization)0;
        if (localPlayer != null)
        {
            localPlayer.transform.position = realPos;
            localPlayer.transform.rotation = realRot;
        }
    }

    private static void BroadcastRealPosition(GameObject localPlayer, PhotonView pv)
    {
        try
        {
            if (pv == null || !PhotonNetwork.IsConnected) return;

            // 恢复同步后，Photon 会自动同步正确位置
            // 可以额外发一个位置 RPC 加速
            Debug.Log("[GhostMode] 同步恢复 — 真实位置将自动广播");
        }
        catch { }
    }
}

/// <summary>
/// 自动完成回合
/// </summary>
public static class AutoCompleteRound
{
    public static void Execute()
    {
        try
        {
            // Step 1: Activate all extraction points
            MiscFeatures.ForceActivateAllExtractionPoints();

            // Step 2: Teleport all items to nearest extraction point
            ExtractionPoint[] points = UnityEngine.Object.FindObjectsOfType<ExtractionPoint>();
            if (points.Length == 0)
            {
                Debug.LogWarning("[AutoComplete] No extraction points found");
                return;
            }

            GameObject localPlayer = DebugCheats.GetLocalPlayer();
            Vector3 refPos = localPlayer != null ? localPlayer.transform.position : Vector3.zero;

            // Find nearest extraction point
            ExtractionPoint nearest = null;
            float nearestDist = float.MaxValue;
            foreach (var ep in points)
            {
                float dist = Vector3.Distance(refPos, ((Component)ep).transform.position);
                if (dist < nearestDist) { nearestDist = dist; nearest = ep; }
            }

            if (nearest == null) return;
            Vector3 sellPos = ((Component)nearest).transform.position + Vector3.up * 0.5f;

            // Move all valuable objects to extraction
            var items = DebugCheats.valuableObjects;
            if (items != null)
            {
                foreach (object item in items)
                {
                    if (item == null) continue;
                    try
                    {
                        UnityEngine.Object unityObj = item as UnityEngine.Object;
                        if (unityObj != null && unityObj == null) continue;

                        Transform t = null;
                        if (item is Component comp)
                            t = comp.transform;
                        else
                        {
                            var prop = item.GetType().GetProperty("transform",
                                BindingFlags.Instance | BindingFlags.Public);
                            if (prop != null)
                                t = prop.GetValue(item) as Transform;
                        }
                        if (t != null)
                            t.position = sellPos;
                    }
                    catch { }
                }
            }

            // Step 3: Zero haul goal
            HaulGoalZero.ZeroHaulGoal();

            Debug.Log("[AutoComplete] Round auto-completed!");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[AutoComplete] Error: " + ex.Message);
        }
    }
}

/// <summary>
/// 门/陷阱禁用器
/// </summary>
public static class TrapDisabler
{
    public static int DisableAllTraps()
    {
        int count = 0;
        try
        {
            // Known trap types
            string[] trapTypeNames = { "ClownTrap", "TrapDoor", "TrapController",
                "TrapBear", "TrapFloor", "TrapCeiling", "TrapSpike",
                "Trap", "DoorController", "DoorLock" };

            var assembly = typeof(RunManager).Assembly;

            foreach (string typeName in trapTypeNames)
            {
                Type trapType = assembly.GetType(typeName);
                if (trapType == null) continue;

                var objects = UnityEngine.Object.FindObjectsOfType(trapType);
                foreach (var obj in objects)
                {
                    try
                    {
                        if (obj is Component comp && comp.gameObject.activeInHierarchy)
                        {
                            comp.gameObject.SetActive(false);
                            count++;
                        }
                    }
                    catch { }
                }
            }

            // Also search for any MonoBehaviour with "Trap" in name
            var allBehaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
            foreach (var mb in allBehaviours)
            {
                string typeName = mb.GetType().Name;
                if (typeName.Contains("Trap") && mb.gameObject.activeInHierarchy)
                {
                    try
                    {
                        mb.gameObject.SetActive(false);
                        count++;
                    }
                    catch { }
                }
            }

            Debug.Log($"[TrapDisabler] Disabled {count} traps/doors");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[TrapDisabler] Error: " + ex.Message);
        }
        return count;
    }
}

/// <summary>
/// 全队传送到撤离点
/// </summary>
public static class TeamTeleport
{
    public static void TeleportAllToExtraction()
    {
        try
        {
            // 先激活所有撤离点
            MiscFeatures.ForceActivateAllExtractionPoints();

            // 找到最近的撤离点
            var points = UnityEngine.Object.FindObjectsOfType<ExtractionPoint>();
            if (points == null || points.Length == 0)
            {
                Debug.LogWarning("[TeamTP] No extraction points found!");
                return;
            }

            GameObject localPlayer = DebugCheats.GetLocalPlayer();
            Vector3 refPos = (localPlayer != null) ? localPlayer.transform.position : Vector3.zero;

            ExtractionPoint nearest = null;
            float minDist = float.MaxValue;
            foreach (var ep in points)
            {
                float dist = Vector3.Distance(refPos, ((Component)ep).transform.position);
                if (dist < minDist) { minDist = dist; nearest = ep; }
            }
            if (nearest == null) return;

            Vector3 targetPos = ((Component)nearest).transform.position + Vector3.up * 1.5f;

            // 遍历所有玩家并传送
            List<PlayerAvatar> players = SemiFunc.PlayerGetList();
            if (players == null) return;

            int count = 0;
            foreach (var avatar in players)
            {
                if ((UnityEngine.Object)(object)avatar == (UnityEngine.Object)null) continue;
                try
                {
                    var field = ((object)avatar).GetType().GetField("photonView",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    PhotonView pv = field?.GetValue(avatar) as PhotonView;
                    if (pv == null)
                        pv = ((Component)avatar).GetComponent<PhotonView>();
                    if (pv == null) continue;

                    // 分散位置防止重叠
                    Vector3 offset = new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), 0, UnityEngine.Random.Range(-1.5f, 1.5f));
                    Vector3 pos = targetPos + offset;

                    ((Component)avatar).transform.position = pos;
                    pv.RPC("SpawnRPC", (RpcTarget)3, new object[2] { pos, ((Component)avatar).transform.rotation });
                    count++;
                }
                catch { }
            }
            Debug.Log($"[TeamTP] Teleported {count} players to extraction point.");
        }
        catch (Exception ex) { Debug.LogWarning("[TeamTP] Error: " + ex.Message); }
    }
}

/// <summary>
/// 玩家无敌光环: 持续治疗指定玩家
/// </summary>
public static class PlayerAura
{
    public static bool isEnabled = false;
    public static int targetActorNumber = -1; // -1 = 全部
    private static float lastHealTime = 0f;
    private static float healInterval = 0.5f;

    public static void Toggle()
    {
        isEnabled = !isEnabled;
        if (!isEnabled) targetActorNumber = -1;
    }

    /// <summary>
    /// 每帧调用: 持续为目标玩家满血
    /// </summary>
    public static void Update()
    {
        if (!isEnabled) return;
        if (Time.time - lastHealTime < healInterval) return;
        lastHealTime = Time.time;

        try
        {
            List<PlayerAvatar> players = SemiFunc.PlayerGetList();
            if (players == null) return;

            foreach (var avatar in players)
            {
                if ((UnityEngine.Object)(object)avatar == (UnityEngine.Object)null) continue;
                try
                {
                    var pvField = ((object)avatar).GetType().GetField("photonView",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    PhotonView pv = pvField?.GetValue(avatar) as PhotonView;
                    if (pv == null)
                        pv = ((Component)avatar).GetComponent<PhotonView>();
                    if (pv == null) continue;

                    // 过滤目标
                    if (targetActorNumber != -1 && pv.OwnerActorNr != targetActorNumber)
                        continue;

                    // 获取 playerHealth
                    var healthField = ((object)avatar).GetType().GetField("playerHealth",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (healthField == null) continue;
                    var health = healthField.GetValue(avatar);
                    if (health == null) continue;

                    var hpPV = ((Component)(MonoBehaviour)health).GetComponent<PhotonView>();
                    if (hpPV == null) continue;

                    // 获取 maxHealth
                    var maxField = health.GetType().GetField("maxHealth",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    int maxHP = 100;
                    if (maxField != null)
                    {
                        try { maxHP = (int)maxField.GetValue(health); } catch { }
                    }

                    hpPV.RPC("UpdateHealthRPC", (RpcTarget)3, new object[3] { maxHP, maxHP, true });
                }
                catch { }
            }
        }
        catch { }
    }
}

/// <summary>
/// 为指定玩家应用升级
/// </summary>
public static class UpgradeForPlayer
{
    /// <summary>
    /// 为指定玩家应用升级 — 使用 StatsManager 字典直接修改
    /// </summary>
    public static void ApplyUpgrades(string steamID, int grabStrength, int throwStrength,
        int sprintSpeed, int grabRange, int extraJump, int tumbleLaunch)
    {
        try
        {
            if (string.IsNullOrEmpty(steamID)) return;
            UpgradeHelper.ApplyUpgradeViaDictionary(steamID, grabStrength, throwStrength, sprintSpeed, grabRange, extraJump, tumbleLaunch);

            // 也直接设置目标玩家的 physGrabber
            try
            {
                PlayerAvatar avatar = SemiFunc.PlayerAvatarGetFromSteamID(steamID);
                if (avatar != null)
                {
                    avatar.physGrabber.grabStrength = grabStrength;
                }
            }
            catch { }
        }
        catch (Exception ex) { Debug.LogWarning("[Upgrade] Error: " + ex.Message); }
    }
}
