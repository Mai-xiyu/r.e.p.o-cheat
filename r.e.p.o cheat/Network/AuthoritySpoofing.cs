using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace r.e.p.o_cheat;

/// <summary>
/// 权限欺骗系统 — 批量请求/夺取场景中 PhotonView 的 Ownership。
/// 三种策略:
///   1. RequestOwnership — 标准请求 (需要房间 OwnershipOption 允许)
///   2. SyncVar 篡改 — 修改 PhotonView 内部 ownerId 字段
///   3. 低级属性设置 — 通过 Photon 网络层设置 owner 属性
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
        /// <summary>自动: 先尝试 Request，失败则 SyncVar</summary>
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
    /// 夺取所有玩家的 PhotonView 所有权 (高风险，可能导致异常)
    /// </summary>
    public static int TakeOverPlayers(Strategy strategy = Strategy.Auto)
    {
        int count = 0;
        try
        {
            Type playerAvType = typeof(RunManager).Assembly.GetType("PlayerAvatar");
            if (playerAvType != null)
            {
                var players = UnityEngine.Object.FindObjectsOfType(playerAvType);
                foreach (var player in players)
                {
                    PhotonView pv = ((Component)player).GetComponent<PhotonView>();
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
            Debug.LogError("[AuthoritySpoofing] TakeOverPlayers 异常: " + ex.Message);
        }

        _lastTakeoverCount = count;
        statusMessage = $"已夺取 {count} 个玩家的控制权";
        return count;
    }

    /// <summary>
    /// 夺取场景中所有 PhotonView 的所有权
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

    // ─── 单个 PhotonView 所有权操作 ─────────────────────────

    /// <summary>
    /// 夺取单个 PhotonView 的所有权
    /// </summary>
    public static bool TakeOwnership(PhotonView pv, Strategy strategy = Strategy.Auto)
    {
        if ((UnityEngine.Object)(object)pv == (UnityEngine.Object)null) return false;
        if (pv.IsMine) return true;

        switch (strategy)
        {
            case Strategy.Request:
                return TryRequestOwnership(pv);

            case Strategy.SyncVarManipulation:
                return TrySyncVarManipulation(pv);

            case Strategy.Auto:
            default:
                // 先尝试标准请求
                if (TryRequestOwnership(pv))
                    return true;
                // 失败则强制 SyncVar 篡改
                return TrySyncVarManipulation(pv);
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
            int myActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

            // 方式 A: 通过 TransferOwnership API (如果我们是 MasterClient 或掌握主机)
            if (PhotonNetwork.IsMasterClient)
            {
                pv.TransferOwnership(PhotonNetwork.LocalPlayer);
                return true;
            }

            // 方式 B: 直接修改 ownerId 字段
            // PhotonView 内部存储 ownerActorNr / ownerId
            FieldInfo ownerField = typeof(PhotonView).GetField("ownerId",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (ownerField == null)
            {
                ownerField = typeof(PhotonView).GetField("ownerActorNr",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            }
            if (ownerField == null)
            {
                // PUN2 某些版本用属性
                ownerField = typeof(PhotonView).GetField("OwnerActorNr",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            }

            if (ownerField != null)
            {
                ownerField.SetValue(pv, myActorNumber);
                Debug.Log($"[AuthoritySpoofing] SyncVar 修改 ViewID={pv.ViewID} owner → {myActorNumber}");
            }

            // 方式 C: 修改 controllerActorNr
            FieldInfo controllerField = typeof(PhotonView).GetField("controllerActorNr",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (controllerField != null)
            {
                controllerField.SetValue(pv, myActorNumber);
            }

            // 方式 D: 通过 Owner 属性 setter (某些版本)
            PropertyInfo ownerProp = typeof(PhotonView).GetProperty("Owner",
                BindingFlags.Instance | BindingFlags.Public);
            if (ownerProp != null && ownerProp.CanWrite)
            {
                ownerProp.SetValue(pv, PhotonNetwork.LocalPlayer);
            }

            // 方式 E: 发送网络层的 ownership 更新
            TryNetworkOwnershipUpdate(pv, myActorNumber);

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AuthoritySpoofing] SyncVar 篡改失败 (ViewID={pv.ViewID}): {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 通过 Photon 网络层发送 ownership 变更
    /// </summary>
    private static void TryNetworkOwnershipUpdate(PhotonView pv, int newOwnerActorNr)
    {
        try
        {
            LoadBalancingClient client = PhotonNetwork.NetworkingClient;
            if (client == null) return;

            // 构建 ownership 转移操作
            // OpCode 227 = OwnershipUpdate 在某些 PUN 版本中
            Hashtable evData = new Hashtable();
            evData[(byte)0] = pv.ViewID;
            evData[(byte)1] = newOwnerActorNr;

            // 通过 RaiseEvent 通知其他客户端
            var raiseEventOptions = new Photon.Realtime.RaiseEventOptions
            {
                Receivers = Photon.Realtime.ReceiverGroup.All,
                CachingOption = Photon.Realtime.EventCaching.DoNotCache
            };

            // OperationCode 253 = Cyclic events (或自定义事件码)
            // PUN 的 Ownership Transfer 使用自定义事件
            PhotonNetwork.RaiseEvent(210, evData, raiseEventOptions,
                new ExitGames.Client.Photon.SendOptions { Reliability = true });
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[AuthoritySpoofing] 网络 ownership 更新失败: " + ex.Message);
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
