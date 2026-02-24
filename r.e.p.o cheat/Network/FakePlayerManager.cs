using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// 假人管理器 v2:
/// 1. Ghost 模式: 直接注入 StatsManager 字典 (不影响本地玩家身份)
/// 2. 3D 模式: NetworkInstantiate 生成独立 PlayerAvatar + 身份保护
/// </summary>
public static class FakePlayerManager
{
    public static List<string> fakePlayerNames = new List<string>();
    public static List<string> fakeSteamIDs = new List<string>();
    public static List<GameObject> fakePlayerObjects = new List<GameObject>();
    public static string customFakeName = "Bot";
    public static int fakeCount = 1;
    public static bool use3DMode = false;

    // NetworkInstantiate 反射缓存
    private static MethodInfo _instantiateMethod;
    private static FieldInfo _levelPrefixField;
    private static string _playerPrefabPath;

    static FakePlayerManager()
    {
        try
        {
            _instantiateMethod = typeof(PhotonNetwork).GetMethod(
                "NetworkInstantiate", BindingFlags.Static | BindingFlags.NonPublic, null,
                new Type[3] { typeof(InstantiateParameters), typeof(bool), typeof(bool) }, null);
            _levelPrefixField = typeof(PhotonNetwork).GetField(
                "currentLevelPrefix", BindingFlags.Static | BindingFlags.NonPublic);
        }
        catch { }
    }

    // ===== 生成真实格式的 Steam64 ID =====
    private static string GenerateFakeSteamID()
    {
        // Steam64 ID 格式: 7656119XXXXXXXXX (17位数字)
        long baseSteamID = 76561190000000000L;
        long random = (long)UnityEngine.Random.Range(100000000, 999999999) * 10L
                     + UnityEngine.Random.Range(0, 10);
        return (baseSteamID + random).ToString();
    }

    // ===== 查找 PlayerAvatar prefab 路径 =====
    private static string FindPlayerPrefabPath()
    {
        if (!string.IsNullOrEmpty(_playerPrefabPath)) return _playerPrefabPath;
        try
        {
            var poolProp = typeof(PhotonNetwork).GetProperty("PrefabPool", BindingFlags.Static | BindingFlags.Public);
            if (poolProp != null)
            {
                var pool = poolProp.GetValue(null);
                if (pool != null)
                {
                    var dictField = pool.GetType().GetField("ResourceCache",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (dictField != null)
                    {
                        var dict = dictField.GetValue(pool) as Dictionary<string, GameObject>;
                        if (dict != null)
                        {
                            foreach (var kvp in dict)
                            {
                                if (kvp.Key.Contains("Player") && kvp.Value != null &&
                                    kvp.Value.GetComponent<PlayerAvatar>() != null)
                                {
                                    _playerPrefabPath = kvp.Key;
                                    Debug.Log("[FakePlayer] Found prefab: " + _playerPrefabPath);
                                    return _playerPrefabPath;
                                }
                            }
                        }
                    }
                }
            }
            // 备选: 从已有 PlayerAvatar 实例猜测
            var avatars = Resources.FindObjectsOfTypeAll<PlayerAvatar>();
            foreach (var a in avatars)
            {
                string name = ((UnityEngine.Object)a).name.Replace("(Clone)", "").Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    _playerPrefabPath = name;
                    return _playerPrefabPath;
                }
            }
        }
        catch (Exception ex) { Debug.LogWarning("[FakePlayer] Prefab search error: " + ex.Message); }
        return null;
    }

    // ============================================================
    //  Ghost 模式: 直接注入 StatsManager (不使用 RPC, 不影响本地玩家)
    // ============================================================
    public static void AddGhostPlayers(string baseName, int count)
    {
        try
        {
            // 获取 StatsManager (通过 PunManager 反射链)
            Type punType = Type.GetType("PunManager, Assembly-CSharp");
            object punMgr = (punType != null) ? GameHelper.FindObjectOfType(punType) : null;
            object statsManager = null;
            Type smType = null;

            if (punMgr != null)
            {
                statsManager = punType.GetField("statsManager", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(punMgr);
                if (statsManager != null) smType = statsManager.GetType();
            }

            for (int i = 0; i < count; i++)
            {
                string name = (count > 1) ? $"{baseName} #{i + 1}" : baseName;
                string fakeID = GenerateFakeSteamID();

                // 注入 StatsManager 字典 (本地)
                if (smType != null && statsManager != null)
                {
                    InjectIntoStatsManager(smType, statsManager, fakeID, name);
                }

                fakePlayerNames.Add(name);
                fakeSteamIDs.Add(fakeID);
            }

            Debug.Log($"[FakePlayer] Added {count} ghost player(s): {baseName}");
        }
        catch (Exception ex) { Debug.LogWarning("[FakePlayer] Ghost add error: " + ex.Message); }
    }

    /// <summary>
    /// 向 StatsManager 注入假玩家的字典条目
    /// </summary>
    private static void InjectIntoStatsManager(Type smType, object sm, string steamID, string name)
    {
        BindingFlags all = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // 遍历所有 Dictionary<string, ?> 字段
        foreach (var fi in smType.GetFields(all))
        {
            if (!fi.FieldType.IsGenericType) continue;
            if (fi.FieldType.GetGenericTypeDefinition() != typeof(Dictionary<,>)) continue;

            var genArgs = fi.FieldType.GetGenericArguments();
            if (genArgs[0] != typeof(string)) continue;

            try
            {
                // Dictionary<string, int> — 注入 0 (升级/属性字典)
                if (genArgs[1] == typeof(int))
                {
                    var dict = fi.GetValue(sm) as Dictionary<string, int>;
                    if (dict != null && !dict.ContainsKey(steamID))
                    {
                        dict[steamID] = 0;
                    }
                }
                // Dictionary<string, string> — 可能是名称映射
                else if (genArgs[1] == typeof(string))
                {
                    string fname = fi.Name.ToLower();
                    if (fname.Contains("player") || fname.Contains("name") || fname.Contains("steam"))
                    {
                        var dict = fi.GetValue(sm) as Dictionary<string, string>;
                        if (dict != null)
                        {
                            dict[steamID] = name;
                        }
                    }
                }
            }
            catch { }
        }

        Debug.Log($"[FakePlayer] Injected: {name} (ID: {steamID})");
    }

    // ============================================================
    //  3D 模式: NetworkInstantiate + 身份保护
    // ============================================================
    public static void Add3DPlayers(string baseName, int count, Vector3 position)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[FakePlayer] 3D mode requires MasterClient!");
            return;
        }

        string prefab = FindPlayerPrefabPath();
        if (string.IsNullOrEmpty(prefab) || _instantiateMethod == null || _levelPrefixField == null)
        {
            Debug.LogWarning("[FakePlayer] 3D mode failed - no prefab. Using ghost mode.");
            AddGhostPlayers(baseName, count);
            return;
        }

        // ===== 保存本地玩家身份 (用于恢复) =====
        string localName = null, localSteamID = null;
        PhotonView localPV = null;
        try
        {
            PlayerAvatar localAvatar = SemiFunc.PlayerAvatarLocal();
            if (localAvatar != null)
            {
                localName = SemiFunc.PlayerGetName(localAvatar);
                localSteamID = SemiFunc.PlayerGetSteamID(localAvatar);
                var pvField = ((object)localAvatar).GetType().GetField("photonView",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                localPV = pvField?.GetValue(localAvatar) as PhotonView;
            }
        }
        catch { }

        try
        {
            object levelPrefix = _levelPrefixField.GetValue(null);

            for (int i = 0; i < count; i++)
            {
                string name = (count > 1) ? $"{baseName} #{i + 1}" : baseName;
                Vector3 spawnPos = position + UnityEngine.Random.insideUnitSphere * 2f;
                spawnPos.y = position.y;

                InstantiateParameters param = new InstantiateParameters(
                    prefab, spawnPos, Quaternion.identity, (byte)0, null,
                    (byte)levelPrefix, null, PhotonNetwork.LocalPlayer, PhotonNetwork.ServerTimestamp);

                GameObject obj = (GameObject)_instantiateMethod.Invoke(null, new object[3] { param, true, false });
                if (obj != null)
                {
                    fakePlayerObjects.Add(obj);
                    string fakeID = GenerateFakeSteamID();
                    fakePlayerNames.Add(name);
                    fakeSteamIDs.Add(fakeID);

                    // 用新 Avatar 自己的 PhotonView 注册 (不干扰本地玩家)
                    var pv = obj.GetComponent<PhotonView>();
                    if (pv != null)
                    {
                        pv.RPC("AddToStatsManagerRPC", (RpcTarget)3, new object[2] { name, fakeID });
                    }
                }
            }

            // ===== 身份保护: 重新注册本地玩家 =====
            if (localPV != null && !string.IsNullOrEmpty(localName) && !string.IsNullOrEmpty(localSteamID))
            {
                localPV.RPC("AddToStatsManagerRPC", (RpcTarget)3, new object[2] { localName, localSteamID });
                Debug.Log($"[FakePlayer] Re-registered local player: {localName}");
            }

            Debug.Log($"[FakePlayer] Spawned {count} 3D player(s): {baseName}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[FakePlayer] 3D spawn error: " + ex.Message + ". Falling back to ghost.");
            AddGhostPlayers(baseName, count);
        }
    }

    // ============================================================
    //  清除所有假人
    // ============================================================
    public static void RemoveAll()
    {
        // 销毁 3D 对象
        foreach (var obj in fakePlayerObjects)
        {
            if (obj != null)
            {
                try { PhotonNetwork.Destroy(obj); } catch { }
                try { UnityEngine.Object.Destroy(obj); } catch { }
            }
        }

        // 清理 StatsManager 中的假人条目
        try
        {
            Type punType = Type.GetType("PunManager, Assembly-CSharp");
            object punMgr = (punType != null) ? GameHelper.FindObjectOfType(punType) : null;
            if (punMgr != null)
            {
                object statsManager = punType.GetField("statsManager", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(punMgr);
                if (statsManager != null)
                {
                    Type smType = statsManager.GetType();
                    foreach (string fakeID in fakeSteamIDs)
                    {
                        RemoveFromAllDicts(smType, statsManager, fakeID);
                    }
                }
            }
        }
        catch { }

        fakePlayerObjects.Clear();
        fakePlayerNames.Clear();
        fakeSteamIDs.Clear();
        Debug.Log("[FakePlayer] All fake players removed.");
    }

    private static void RemoveFromAllDicts(Type smType, object sm, string steamID)
    {
        BindingFlags all = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        foreach (var fi in smType.GetFields(all))
        {
            if (!fi.FieldType.IsGenericType) continue;
            if (fi.FieldType.GetGenericTypeDefinition() != typeof(Dictionary<,>)) continue;
            if (fi.FieldType.GetGenericArguments()[0] != typeof(string)) continue;

            try
            {
                object dict = fi.GetValue(sm);
                if (dict == null) continue;
                var removeMethod = fi.FieldType.GetMethod("Remove", new Type[] { typeof(string) });
                removeMethod?.Invoke(dict, new object[] { steamID });
            }
            catch { }
        }
    }
}
