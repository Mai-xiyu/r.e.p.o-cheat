using System;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// PhotonView ownership helpers.
/// Auto uses RequestOwnership, or TransferOwnership when we are the real host.
/// Player avatars and core managers are never taken — that desyncs movement and other tabs.
/// </summary>
public static class AuthoritySpoofing
{
    public static string statusMessage = "";

    // 统计
    private static int _lastTakeoverCount = 0;
    public static int LastTakeoverCount => _lastTakeoverCount;

    // ─── 策略选择枚举 ─────────────────────────────────────
    public enum Strategy
    {
        /// <summary>标准 RequestOwnership</summary>
        Request,
        /// <summary>SyncVar/反射篡改 ownerId</summary>
        SyncVarManipulation,
        /// <summary>Host TransferOwnership, otherwise RequestOwnership</summary>
        Auto
    }

    // ─── 批量操作 ──────────────────────────────────────────

    /// <summary>
    /// 夺取所有敌人的 PhotonView 所有权
    /// </summary>
    public static int TakeOverEnemies(Strategy strategy = Strategy.Auto)
    {
        int count = 0;
        try
        {
            // 查找所有 EnemyParent 类型
            Type enemyParentType = typeof(RunManager).Assembly.GetType("EnemyParent");
            if (enemyParentType == null)
            {
                statusMessage = "未找到 EnemyParent 类型";
                return 0;
            }

            var enemies = UnityEngine.Object.FindObjectsOfType(enemyParentType);
            foreach (var enemy in enemies)
            {
                Component comp = (Component)enemy;
                PhotonView pv = comp.GetComponent<PhotonView>();
                if ((UnityEngine.Object)(object)pv == (UnityEngine.Object)null)
                    pv = comp.GetComponentInChildren<PhotonView>();

                if ((UnityEngine.Object)(object)pv != (UnityEngine.Object)null && !pv.IsMine)
                {
                    if (TakeOwnership(pv, strategy))
                        count++;
                }
            }

            // 也处理 EnemyRigidbody 上的 PhotonView
            Type enemyRbType = typeof(RunManager).Assembly.GetType("EnemyRigidbody");
            if (enemyRbType != null)
            {
                var rigidbodies = UnityEngine.Object.FindObjectsOfType(enemyRbType);
                foreach (var rb in rigidbodies)
                {
                    PhotonView pv = ((Component)rb).GetComponent<PhotonView>();
                    if ((UnityEngine.Object)(object)pv != (UnityEngine.Object)null && !pv.IsMine)
                    {
                        if (TakeOwnership(pv, strategy))
                            count++;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[AuthoritySpoofing] TakeOverEnemies 异常: " + ex.Message);
        }

        _lastTakeoverCount = count;
        statusMessage = $"已夺取 {count} 个敌人的控制权";
        Debug.Log($"[AuthoritySpoofing] 已夺取 {count} 个敌人 PhotonView");
        return count;
    }

    /// <summary>
    /// 夺取所有物品的 PhotonView 所有权
    /// </summary>
    public static int TakeOverItems(Strategy strategy = Strategy.Auto)
    {
        int count = 0;
        try
        {
            // 查找所有 ValuableObject
            Type valuableType = typeof(RunManager).Assembly.GetType("ValuableObject");
            if (valuableType != null)
            {
                var valuables = UnityEngine.Object.FindObjectsOfType(valuableType);
                foreach (var val in valuables)
                {
                    PhotonView pv = ((Component)val).GetComponent<PhotonView>();
                    if ((UnityEngine.Object)(object)pv == (UnityEngine.Object)null)
                        pv = ((Component)val).GetComponentInParent<PhotonView>();

                    if ((UnityEngine.Object)(object)pv != (UnityEngine.Object)null && !pv.IsMine)
                    {
                        if (TakeOwnership(pv, strategy))
                            count++;
                    }
                }
            }

            // 查找所有 PhysGrabObject
            Type physGrabType = typeof(RunManager).Assembly.GetType("PhysGrabObject");
            if (physGrabType != null)
            {
                var grabs = UnityEngine.Object.FindObjectsOfType(physGrabType);
                foreach (var grab in grabs)
                {
                    PhotonView pv = ((Component)grab).GetComponent<PhotonView>();
                    if ((UnityEngine.Object)(object)pv != (UnityEngine.Object)null && !pv.IsMine)
                    {
                        if (TakeOwnership(pv, strategy))
                            count++;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[AuthoritySpoofing] TakeOverItems 异常: " + ex.Message);
        }

        _lastTakeoverCount = count;
        statusMessage = $"已夺取 {count} 个物品的控制权";
        Debug.Log($"[AuthoritySpoofing] 已夺取 {count} 个物品 PhotonView");
        return count;
    }

    /// <summary>
    /// 夺取所有玩家的 PhotonView 所有权 — blocked: this desyncs movement / noclip / teleport.
    /// </summary>
    public static int TakeOverPlayers(Strategy strategy = Strategy.Auto)
    {
        _lastTakeoverCount = 0;
        statusMessage = L.T("room.player_take_blocked");
        return 0;
    }

    /// <summary>
    /// 夺取场景中所有 PhotonView 的所有权（跳过玩家与核心管理器）
    /// </summary>
    public static int TakeOverAll(Strategy strategy = Strategy.Auto)
    {
        int count = 0;
        try
        {
            PhotonView[] allViews = UnityEngine.Object.FindObjectsOfType<PhotonView>();
            foreach (PhotonView pv in allViews)
            {
                if ((UnityEngine.Object)(object)pv == (UnityEngine.Object)null) continue;
                if (pv.IsMine) continue;
                if (IsProtectedView(pv)) continue;

                if (TakeOwnership(pv, strategy))
                    count++;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[AuthoritySpoofing] TakeOverAll 异常: " + ex.Message);
        }

        _lastTakeoverCount = count;
        statusMessage = $"已夺取 {count} 个 PhotonView 的控制权";
        Debug.Log($"[AuthoritySpoofing] 全部夺取完成: {count} 个 PhotonView");
        return count;
    }

    public static int ReleaseAll()
    {
        int count = 0;
        try
        {
            PhotonView[] allViews = UnityEngine.Object.FindObjectsOfType<PhotonView>();
            foreach (PhotonView pv in allViews)
            {
                if ((UnityEngine.Object)(object)pv == (UnityEngine.Object)null) continue;
                if (!pv.IsMine) continue;
                if (IsProtectedView(pv)) continue;
                if (ReleaseOwnership(pv))
                    count++;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[AuthoritySpoofing] ReleaseAll 异常: " + ex.Message);
        }

        _lastTakeoverCount = count;
        statusMessage = L.T("room.released_fmt", count);
        return count;
    }

    private static bool IsProtectedView(PhotonView pv)
    {
        try
        {
            Component host = (Component)(object)pv;
            if (host.GetComponent<PlayerAvatar>() != null || host.GetComponentInParent<PlayerAvatar>() != null)
                return true;
            if (host.GetComponent<PlayerTumble>() != null || host.GetComponentInParent<PlayerTumble>() != null)
                return true;
            if (host.GetComponent<PunManager>() != null || host.GetComponentInParent<PunManager>() != null)
                return true;
            if (host.GetComponent<NetworkManager>() != null)
                return true;
            if (host.GetComponent<LevelGenerator>() != null)
                return true;
            if (host.GetComponent<RunManager>() != null)
                return true;
            if (host.GetComponent<RoundDirector>() != null)
                return true;
        }
        catch { }
        return false;
    }

    // ─── 单个 PhotonView 所有权操作 ─────────────────────────

    /// <summary>
    /// 夺取单个 PhotonView 的所有权
    /// </summary>
    public static bool TakeOwnership(PhotonView pv, Strategy strategy = Strategy.Auto)
    {
        if ((UnityEngine.Object)(object)pv == (UnityEngine.Object)null) return false;
        if (pv.IsMine) return true;
        if (IsProtectedView(pv)) return false;

        switch (strategy)
        {
            case Strategy.Request:
                return TryRequestOwnership(pv);

            case Strategy.SyncVarManipulation:
                return TrySyncVarManipulation(pv);

            case Strategy.Auto:
            default:
                if (ShadowHostMode.IsTrueMasterClient())
                {
                    try
                    {
                        pv.TransferOwnership(PhotonNetwork.LocalPlayer);
                        return true;
                    }
                    catch { }
                }
                return TryRequestOwnership(pv);
        }
    }

    /// <summary>
    /// 释放单个 PhotonView 的所有权 (归还给 MasterClient)
    /// </summary>
    public static bool ReleaseOwnership(PhotonView pv)
    {
        if ((UnityEngine.Object)(object)pv == (UnityEngine.Object)null) return false;
        if (!pv.IsMine) return true;

        try
        {
            pv.TransferOwnership(PhotonNetwork.MasterClient);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // ─── 策略实现 ──────────────────────────────────────────

    /// <summary>
    /// 策略1: 标准 RequestOwnership
    /// </summary>
    private static bool TryRequestOwnership(PhotonView pv)
    {
        try
        {
            pv.RequestOwnership();
            // RequestOwnership 是异步的，无法立即验证
            // 但如果房间设置允许，应该会成功
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[AuthoritySpoofing] RequestOwnership 失败 (ViewID={pv.ViewID}): {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 策略2: SyncVar 篡改 — 直接修改 PhotonView 的内部 owner 字段
    /// </summary>
    private static bool TrySyncVarManipulation(PhotonView pv)
    {
        try
        {
            if (!ShadowHostMode.IsTrueMasterClient())
            {
                return false;
            }
            pv.TransferOwnership(PhotonNetwork.LocalPlayer);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AuthoritySpoofing] TransferOwnership 失败 (ViewID={pv.ViewID}): {ex.Message}");
            return false;
        }
    }

    // ─── 诊断工具 ──────────────────────────────────────────

    /// <summary>
    /// 获取场景中所有 PhotonView 的归属统计
    /// </summary>
    public static OwnershipStats GetOwnershipStats()
    {
        var stats = new OwnershipStats();
        try
        {
            PhotonView[] allViews = UnityEngine.Object.FindObjectsOfType<PhotonView>();
            stats.Total = allViews.Length;

            foreach (var pv in allViews)
            {
                if ((UnityEngine.Object)(object)pv == (UnityEngine.Object)null) continue;
                if (pv.IsMine) stats.Mine++;
                else stats.Others++;
            }
        }
        catch { }
        return stats;
    }

    public struct OwnershipStats
    {
        public int Total;
        public int Mine;
        public int Others;
        public override string ToString() => $"总计: {Total} | 我的: {Mine} | 他人: {Others}";
    }
}
