using System;
using System.Reflection;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// 影子房主模式 — 通过 Harmony 拦截 PhotonNetwork.IsMasterClient 的 getter，
/// 使其始终返回 true，而无需实际修改 MasterClient 变量。
/// 
/// 优势：
///   - 零掉线风险（不修改任何网络层变量）
///   - Photon 心跳检测完全无感
///   - 所有游戏内 IsMasterClient 检查均被拦截
///   - 可随时开关，无副作用
/// 
/// 注意：服务端仍知道真实 MasterClient，某些服务端强制校验的操作可能被拒绝。
/// 对于需要真正的 Host 权限的操作，应结合 ForceHost 的实际夺权方法。
/// </summary>
public static class ShadowHostMode
{
    /// <summary>全局开关</summary>
    public static bool isEnabled = false;

    /// <summary>内部标记 — 当为 true 时 Postfix 不拦截（用于读取真实值）</summary>
    internal static bool _bypassPatch = false;

    public static string statusMessage = "";

    // ─────────────────────────────────────────────────────────
    // Harmony Patch: 拦截 PhotonNetwork.IsMasterClient getter
    // 由 Loader.cs 中的 PatchAll() 自动扫描并应用
    // ─────────────────────────────────────────────────────────

    [HarmonyPatch(typeof(PhotonNetwork), "get_IsMasterClient")]
    public static class Patch_IsMasterClient
    {
        /// <summary>
        /// Postfix: 在原始 getter 返回后，如果 Shadow Host 已启用，
        /// 将结果强制覆盖为 true。
        /// </summary>
        static void Postfix(ref bool __result)
        {
            if (_bypassPatch) return;
            if (isEnabled && PhotonNetwork.InRoom)
            {
                // 仅在完全加入房间后才覆盖，避免干扰加载 / 场景切换流程
                if (PhotonNetwork.NetworkClientState != Photon.Realtime.ClientState.Joined) return;
                __result = true;
            }
        }
    }

    // ─────────────────────────────────────────────────────────
    // 工具方法
    // ─────────────────────────────────────────────────────────

    /// <summary>
    /// 读取 **真实的** IsMasterClient 值，绕过 Shadow Host patch。
    /// 用于内部逻辑（如 ForceHost 判断是否需要继续夺权）。
    /// </summary>
    public static bool IsTrueMasterClient()
    {
        _bypassPatch = true;
        try
        {
            return PhotonNetwork.IsMasterClient;
        }
        finally
        {
            _bypassPatch = false;
        }
    }

    /// <summary>
    /// 切换 Shadow Host 模式
    /// </summary>
    public static void Toggle()
    {
        isEnabled = !isEnabled;
        if (isEnabled)
        {
            statusMessage = "Shadow Host ON — 本地视为房主";
            Debug.Log("[ShadowHost] 已启用 — 所有 IsMasterClient 检查将返回 true");
        }
        else
        {
            statusMessage = "Shadow Host OFF";
            Debug.Log("[ShadowHost] 已禁用");
        }
    }

    /// <summary>
    /// 获取当前状态的诊断信息
    /// </summary>
    public static string GetDiagnostics()
    {
        if (!PhotonNetwork.InRoom) return "未在房间中";

        bool trueMaster = IsTrueMasterClient();
        bool perceived = isEnabled || trueMaster;

        return $"真实房主: {(trueMaster ? "是 ★" : "否")} | 感知房主: {(perceived ? "是" : "否")} | Shadow: {(isEnabled ? "ON" : "OFF")}";
    }
}
