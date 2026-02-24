using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// RPC 注入器 — 扫描游戏程序集中所有 [PunRPC] 方法，提供统一调用接口。
/// 三层策略:
///   1. 直接调用: 如果我们是 MasterClient，正常发送 RPC
///   2. 伪装调用: 临时伪造 ActorNumber 后发送 (ForceHost.SendRPCAsHost)
///   3. 本地调用: 直接反射调用本地方法（仅影响本地客户端）
/// </summary>
public static class RpcInjector
{
    // ─── 缓存 ──────────────────────────────────────────────
    private static Dictionary<string, List<RpcMethodInfo>> _rpcCache;
    private static bool _scanned = false;
    public static string statusMessage = "";

    // ─── RPC 队列兜底 ────────────────────────────────────
    private static readonly List<QueuedRpc> _rpcQueue = new List<QueuedRpc>();
    private const float RPC_QUEUE_TIMEOUT = 30f; // 队列超时（秒）
    public static int QueuedCount => _rpcQueue.Count;

    private struct QueuedRpc
    {
        public string RpcName;
        public RpcTarget Target;
        public object[] Args;
        public int PhotonViewId; // -1 = auto-find
        public float QueueTime;
    }

    public struct RpcMethodInfo
    {
        public string Name;
        public Type DeclaringType;
        public MethodInfo Method;
        public ParameterInfo[] Params;

        public string Signature =>
            $"{DeclaringType.Name}.{Name}({string.Join(", ", Params.Select(p => p.ParameterType.Name))})";
    }

    // ─── 高价值 RPC 白名单（快速搜索） ─────────────────────
    public static readonly string[] HighValueRPCs = new string[]
    {
        // 升级相关
        "UpgradePlayerGrabStrengthRPC",
        "UpgradePlayerSprintSpeedRPC",
        "UpgradePlayerExtraJumpRPC",
        "UpgradePlayerTumbleLaunchRPC",
        "UpgradePlayerGrabRangeRPC",
        "UpgradePlayerHealthRPC",
        "UpgradePlayerThrowStrengthRPC",
        "UpgradePlayerGrabThrowRPC",
        // 生命值
        "HealRPC",
        "UpdateHealthRPC",
        "HurtOtherRPC",
        // 生成/复活
        "SpawnRPC",
        "ReviveRPC",
        "PlayerDeathRPC",
        // 经济
        "DollarValueSetRPC",
        "CrownPlayerRPC",
        // 物品
        "ItemSetup",
        "SpawnSpecificEnemy",
        // 提取点
        "ExtractionPointActivateRPC",
    };

    // ─── 扫描 ──────────────────────────────────────────────

    /// <summary>
    /// 扫描所有已加载程序集中的 [PunRPC] 方法并缓存
    /// </summary>
    public static void ScanAllRPCs(bool force = false)
    {
        if (_scanned && !force) return;

        _rpcCache = new Dictionary<string, List<RpcMethodInfo>>(StringComparer.OrdinalIgnoreCase);
        int total = 0;

        try
        {
            // 扫描主游戏程序集 + Assembly-CSharp
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly asm in assemblies)
            {
                string asmName = asm.GetName().Name;
                // 只扫描游戏相关程序集
                if (!asmName.StartsWith("Assembly-CSharp") &&
                    !asmName.Contains("r.e.p.o") &&
                    !asmName.StartsWith("Photon"))
                    continue;

                try
                {
                    foreach (Type type in asm.GetTypes())
                    {
                        if (!typeof(MonoBehaviour).IsAssignableFrom(type)) continue;

                        foreach (MethodInfo method in type.GetMethods(
                            BindingFlags.Instance | BindingFlags.Static |
                            BindingFlags.Public | BindingFlags.NonPublic))
                        {
                            if (method.GetCustomAttribute<PunRPC>() == null) continue;

                            var info = new RpcMethodInfo
                            {
                                Name = method.Name,
                                DeclaringType = type,
                                Method = method,
                                Params = method.GetParameters()
                            };

                            if (!_rpcCache.ContainsKey(method.Name))
                                _rpcCache[method.Name] = new List<RpcMethodInfo>();
                            _rpcCache[method.Name].Add(info);
                            total++;
                        }
                    }
                }
                catch { /* 忽略无法加载的程序集 */ }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[RpcInjector] 扫描异常: " + ex.Message);
        }

        _scanned = true;
        statusMessage = $"已扫描 {total} 个 RPC 方法";
        Debug.Log($"[RpcInjector] 扫描完成: {total} 个 RPC 方法, {_rpcCache.Count} 个唯一名称");
    }

    /// <summary>
    /// 获取所有已缓存的 RPC 方法名列表
    /// </summary>
    public static List<string> GetAllRpcNames()
    {
        if (!_scanned) ScanAllRPCs();
        return _rpcCache?.Keys.OrderBy(k => k).ToList() ?? new List<string>();
    }

    /// <summary>
    /// 获取指定名称的 RPC 详细信息
    /// </summary>
    public static List<RpcMethodInfo> GetRpcInfo(string rpcName)
    {
        if (!_scanned) ScanAllRPCs();
        if (_rpcCache != null && _rpcCache.TryGetValue(rpcName, out var list))
            return list;
        return new List<RpcMethodInfo>();
    }

    /// <summary>
    /// 获取高价值 RPC 中在游戏中实际存在的列表
    /// </summary>
    public static List<string> GetAvailableHighValueRPCs()
    {
        if (!_scanned) ScanAllRPCs();
        var result = new List<string>();
        foreach (string name in HighValueRPCs)
        {
            if (_rpcCache != null && _rpcCache.ContainsKey(name))
                result.Add(name);
        }
        return result;
    }

    // ─── 调用 ──────────────────────────────────────────────

    /// <summary>
    /// 通用 RPC 调用 — 自动选择最佳策略
    /// </summary>
    /// <param name="rpcName">RPC 方法名</param>
    /// <param name="target">RPC 目标 (All, MasterClient, Others, AllBuffered…)</param>
    /// <param name="args">RPC 参数</param>
    /// <returns>是否成功发送</returns>
    public static bool CallRpc(string rpcName, RpcTarget target, params object[] args)
    {
        return CallRpcOnView(null, rpcName, target, args);
    }

    /// <summary>
    /// 在指定 PhotonView 上调用 RPC
    /// </summary>
    public static bool CallRpcOnView(PhotonView targetView, string rpcName, RpcTarget target, params object[] args)
    {
        if (!PhotonNetwork.InRoom)
        {
            statusMessage = "未在房间中";
            return false;
        }

        // 如果未指定 PhotonView，则自动寻找合适的
        if ((UnityEngine.Object)(object)targetView == (UnityEngine.Object)null)
        {
            targetView = FindViewForRpc(rpcName);
            if ((UnityEngine.Object)(object)targetView == (UnityEngine.Object)null)
            {
                statusMessage = $"未找到包含 {rpcName} 的 PhotonView";
                Debug.LogWarning($"[RpcInjector] 未找到包含 {rpcName} 的 PhotonView");
                return false;
            }
        }

        // 策略0: 如果 Shadow Host 启用，直接发送（本地认为自己是 MasterClient）
        if (ShadowHostMode.isEnabled)
        {
            try
            {
                targetView.RPC(rpcName, target, args);
                statusMessage = $"✓ [Shadow] 已发送 {rpcName}";
                Debug.Log($"[RpcInjector] Shadow Host 发送 {rpcName} → {target}");
                return true;
            }
            catch (Exception exS)
            {
                Debug.LogWarning($"[RpcInjector] Shadow Host 调用 {rpcName} 失败: {exS.Message}");
            }
        }

        // 策略1: 直接调用 (MasterClient 或无需权限的 RPC)
        try
        {
            targetView.RPC(rpcName, target, args);
            statusMessage = $"✓ 已发送 {rpcName}";
            Debug.Log($"[RpcInjector] 直接发送 {rpcName} → {target}");
            return true;
        }
        catch (Exception ex1)
        {
            Debug.LogWarning($"[RpcInjector] 直接调用 {rpcName} 失败: {ex1.Message}");
        }

        // 策略2: 伪装为主机发送
        try
        {
            ForceHost.SendRPCAsHost(targetView, rpcName, target, args);
            statusMessage = $"✓ 已伪装发送 {rpcName}";
            Debug.Log($"[RpcInjector] 伪装发送 {rpcName} → {target}");
            return true;
        }
        catch (Exception ex2)
        {
            Debug.LogWarning($"[RpcInjector] 伪装调用 {rpcName} 失败: {ex2.Message}");
        }

        // 策略3: 本地反射调用
        try
        {
            return CallRpcLocal(rpcName, targetView, args);
        }
        catch (Exception ex3)
        {
            Debug.LogError($"[RpcInjector] 所有策略均失败 {rpcName}: {ex3.Message}");
            statusMessage = $"✗ {rpcName} 调用失败";
            return false;
        }
    }

    /// <summary>
    /// 本地直接反射调用 RPC 方法（不通过网络）
    /// </summary>
    public static bool CallRpcLocal(string rpcName, PhotonView targetView, params object[] args)
    {
        if (!_scanned) ScanAllRPCs();
        if (_rpcCache == null || !_rpcCache.TryGetValue(rpcName, out var methods))
        {
            statusMessage = $"本地未找到 {rpcName}";
            return false;
        }

        // 找到 targetView 所在 GameObject 上匹配的组件
        MonoBehaviour targetComponent = null;
        RpcMethodInfo matchedMethod = default;

        if ((UnityEngine.Object)(object)targetView != (UnityEngine.Object)null)
        {
            foreach (var info in methods)
            {
                var comp = ((Component)targetView).GetComponent(info.DeclaringType);
                if (comp != null)
                {
                    targetComponent = (MonoBehaviour)comp;
                    matchedMethod = info;
                    break;
                }
            }
        }

        // 如果在目标 View 上没找到，在场景中搜索
        if (targetComponent == null)
        {
            foreach (var info in methods)
            {
                var obj = UnityEngine.Object.FindObjectOfType(info.DeclaringType);
                if (obj != null)
                {
                    targetComponent = (MonoBehaviour)obj;
                    matchedMethod = info;
                    break;
                }
            }
        }

        if (targetComponent == null)
        {
            statusMessage = $"场景中未找到 {rpcName} 的宿主组件";
            return false;
        }

        try
        {
            // 调整参数数量以匹配方法签名
            object[] callArgs = args;
            if (matchedMethod.Params.Length != args.Length)
            {
                callArgs = new object[matchedMethod.Params.Length];
                for (int i = 0; i < matchedMethod.Params.Length; i++)
                {
                    if (i < args.Length)
                        callArgs[i] = args[i];
                    else if (matchedMethod.Params[i].HasDefaultValue)
                        callArgs[i] = matchedMethod.Params[i].DefaultValue;
                    else
                        callArgs[i] = matchedMethod.Params[i].ParameterType.IsValueType
                            ? Activator.CreateInstance(matchedMethod.Params[i].ParameterType)
                            : null;
                }
            }

            matchedMethod.Method.Invoke(targetComponent, callArgs);
            statusMessage = $"✓ 本地调用 {rpcName}";
            Debug.Log($"[RpcInjector] 本地调用 {matchedMethod.Signature}");
            return true;
        }
        catch (Exception ex)
        {
            statusMessage = $"✗ 本地调用 {rpcName} 异常: {ex.InnerException?.Message ?? ex.Message}";
            Debug.LogError($"[RpcInjector] 本地调用异常: {ex}");
            return false;
        }
    }

    // ─── 便捷方法: 高价值 RPC 快速调用 ─────────────────────

    /// <summary>
    /// 升级玩家属性 (通过 RPC)
    /// </summary>
    public static bool UpgradePlayer(string steamID, string upgradeRpcName, int amount = 1)
    {
        var pv = FindPunManagerView();
        if ((UnityEngine.Object)(object)pv == (UnityEngine.Object)null)
        {
            statusMessage = "未找到 PunManager PhotonView";
            return false;
        }

        return CallRpcOnView(pv, upgradeRpcName, (RpcTarget)3, new object[] { steamID, amount });
    }

    /// <summary>
    /// 治疗/伤害玩家
    /// </summary>
    public static bool HealPlayer(string steamID, int amount)
    {
        var pv = FindPunManagerView();
        if ((UnityEngine.Object)(object)pv != (UnityEngine.Object)null)
        {
            return CallRpcOnView(pv, "HealRPC", (RpcTarget)3, new object[] { steamID, amount });
        }
        return false;
    }

    /// <summary>
    /// 复活玩家
    /// </summary>
    public static bool RevivePlayer(string steamID)
    {
        var pv = FindPunManagerView();
        if ((UnityEngine.Object)(object)pv != (UnityEngine.Object)null)
        {
            return CallRpcOnView(pv, "ReviveRPC", (RpcTarget)3, new object[] { steamID });
        }
        return false;
    }

    /// <summary>
    /// 设置金钱数值
    /// </summary>
    public static bool SetDollarValue(int value)
    {
        var pv = FindPunManagerView();
        if ((UnityEngine.Object)(object)pv != (UnityEngine.Object)null)
        {
            return CallRpcOnView(pv, "DollarValueSetRPC", (RpcTarget)3, new object[] { value });
        }
        return false;
    }

    /// <summary>
    /// 激活提取点
    /// </summary>
    public static bool ActivateExtractionPoint()
    {
        ExtractionPoint[] points = UnityEngine.Object.FindObjectsOfType<ExtractionPoint>();
        if (points.Length == 0) return false;

        foreach (var point in points)
        {
            PhotonView pv = ((Component)point).GetComponent<PhotonView>();
            if ((UnityEngine.Object)(object)pv != (UnityEngine.Object)null)
            {
                CallRpcOnView(pv, "ExtractionPointActivateRPC", (RpcTarget)0);
            }
        }
        return true;
    }

    // ─── RPC 队列兜底系统 ────────────────────────────────────

    /// <summary>
    /// 带主机权限兜底的 RPC 调用。
    /// 如果当前是主机（或 Shadow Host），直接发送；
    /// 否则入队并触发自动夺权，成功后自动执行队列中的 RPC。
    /// </summary>
    public static bool CallRpcWithHostFallback(PhotonView targetView, string rpcName, RpcTarget target, params object[] args)
    {
        // 已经是主机或 Shadow Host，直接发送
        if (ShadowHostMode.isEnabled || PhotonNetwork.IsMasterClient)
        {
            return CallRpcOnView(targetView, rpcName, target, args);
        }

        // 入队
        int viewId = ((UnityEngine.Object)(object)targetView != (UnityEngine.Object)null) ? targetView.ViewID : -1;
        _rpcQueue.Add(new QueuedRpc
        {
            RpcName = rpcName,
            Target = target,
            Args = args,
            PhotonViewId = viewId,
            QueueTime = Time.time
        });

        statusMessage = $"⏳ {rpcName} 已入队 (共 {_rpcQueue.Count} 个待执行)";
        Debug.Log($"[RpcInjector] {rpcName} 入队，当前队列: {_rpcQueue.Count}");

        // 注册回调 + 触发自动夺权
        ForceHost.EnsureHost(() => FlushRpcQueue());

        return true; // queued successfully
    }

    /// <summary>
    /// 快捷方法：自动查找 PhotonView 的带队列兜底版本
    /// </summary>
    public static bool CallRpcWithHostFallback(string rpcName, RpcTarget target, params object[] args)
    {
        return CallRpcWithHostFallback(null, rpcName, target, args);
    }

    /// <summary>
    /// 执行队列中所有 RPC（在获得主机权限后调用）
    /// </summary>
    public static void FlushRpcQueue()
    {
        if (_rpcQueue.Count == 0) return;

        int success = 0, failed = 0, expired = 0;
        float now = Time.time;

        foreach (var queued in _rpcQueue)
        {
            // 超时检查
            if (now - queued.QueueTime > RPC_QUEUE_TIMEOUT)
            {
                expired++;
                Debug.LogWarning($"[RpcInjector] 队列 RPC 已超时: {queued.RpcName}");
                continue;
            }

            try
            {
                PhotonView pv = null;
                if (queued.PhotonViewId > 0)
                    pv = PhotonView.Find(queued.PhotonViewId);

                if (CallRpcOnView(pv, queued.RpcName, queued.Target, queued.Args))
                    success++;
                else
                    failed++;
            }
            catch (Exception ex)
            {
                failed++;
                Debug.LogError($"[RpcInjector] 队列 RPC 执行异常: {queued.RpcName} — {ex.Message}");
            }
        }

        _rpcQueue.Clear();
        statusMessage = $"队列执行完成: {success} 成功, {failed} 失败, {expired} 超时";
        Debug.Log($"[RpcInjector] 队列清空: {success} 成功, {failed} 失败, {expired} 超时");
    }

    /// <summary>
    /// 清空 RPC 队列
    /// </summary>
    public static void ClearQueue()
    {
        int count = _rpcQueue.Count;
        _rpcQueue.Clear();
        statusMessage = $"已清空 {count} 个排队 RPC";
    }

    /// <summary>
    /// 清理超时的队列项
    /// </summary>
    public static void PruneExpiredQueue()
    {
        float now = Time.time;
        _rpcQueue.RemoveAll(q => now - q.QueueTime > RPC_QUEUE_TIMEOUT);
    }

    // ─── 辅助方法 ──────────────────────────────────────────

    /// <summary>
    /// 寻找 PunManager 的 PhotonView
    /// </summary>
    private static PhotonView FindPunManagerView()
    {
        try
        {
            // PunManager 通常挂载在 "Game Manager PUN" 或同名 GameObject 上
            Type punMgrType = typeof(RunManager).Assembly.GetType("PunManager");
            if (punMgrType != null)
            {
                var instance = UnityEngine.Object.FindObjectOfType(punMgrType);
                if (instance != null)
                {
                    var pv = ((Component)instance).GetComponent<PhotonView>();
                    if ((UnityEngine.Object)(object)pv != (UnityEngine.Object)null)
                        return pv;
                }
            }

            // 备选: 通过 GameObject 名称查找
            string[] names = { "Game Manager PUN", "GameManager", "PunManager" };
            foreach (string name in names)
            {
                GameObject go = GameObject.Find(name);
                if (go != null)
                {
                    PhotonView pv = go.GetComponent<PhotonView>();
                    if ((UnityEngine.Object)(object)pv != (UnityEngine.Object)null)
                        return pv;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[RpcInjector] FindPunManagerView 异常: " + ex.Message);
        }
        return null;
    }

    /// <summary>
    /// 自动寻找包含指定 RPC 方法的 PhotonView
    /// </summary>
    private static PhotonView FindViewForRpc(string rpcName)
    {
        if (!_scanned) ScanAllRPCs();
        if (_rpcCache == null || !_rpcCache.TryGetValue(rpcName, out var methods))
            return null;

        foreach (var info in methods)
        {
            // 在场景中搜索该类型的实例
            var obj = UnityEngine.Object.FindObjectOfType(info.DeclaringType);
            if (obj != null)
            {
                PhotonView pv = ((Component)obj).GetComponent<PhotonView>();
                if ((UnityEngine.Object)(object)pv != (UnityEngine.Object)null)
                    return pv;

                // 尝试父对象
                pv = ((Component)obj).GetComponentInParent<PhotonView>();
                if ((UnityEngine.Object)(object)pv != (UnityEngine.Object)null)
                    return pv;
            }
        }
        return null;
    }
}
