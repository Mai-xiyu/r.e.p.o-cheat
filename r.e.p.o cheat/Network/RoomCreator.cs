using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Photon.Pun;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// 快速创建房间 — 完整模拟游戏原生 Host 流程
/// 1. 创建 Steam Lobby
/// 2. 枚举并设置 SteamManager 所有 Lobby 相关字段
/// 3. 按照 LobbyFinder.JoinLobbyAndPlay 完全相同的顺序触发游戏连接
/// </summary>
public static class RoomCreator
{
    public static bool IsCreating { get; private set; }
    public static string StatusText = "";

    // ===== 反射缓存 =====
    private static bool _cacheReady;
    private static FieldInfo _joinLobbyField;
    private static FieldInfo[] _allSmFields;
    private static FieldInfo _lobbyJoinField; // RunManager.lobbyJoin

    private static void EnsureCache()
    {
        if (_cacheReady) return;
        _cacheReady = true;

        try
        {
            Type smType = typeof(SteamManager);
            BindingFlags all = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

            _joinLobbyField = smType.GetField("joinLobby", all);
            _allSmFields = smType.GetFields(all);

            // 枚举所有字段/方法到日志 (仅首次, 用于调试)
            foreach (var f in _allSmFields)
            {
                string val = "";
                try
                {
                    if (SteamManager.instance != null && !f.IsStatic)
                        val = $" = {f.GetValue(SteamManager.instance)}";
                    else if (f.IsStatic)
                        val = $" = {f.GetValue(null)}";
                }
                catch { }
                Debug.Log($"[RC] SM field: {f.Name} ({f.FieldType.Name}){val}");
            }

            var methods = smType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (var m in methods)
            {
                var pars = m.GetParameters();
                string parStr = string.Join(", ", pars.Select(p => p.ParameterType.Name + " " + p.Name));
                Debug.Log($"[RC] SM method: {m.Name}({parStr}) -> {m.ReturnType.Name}");
            }

            // RunManager.lobbyJoin
            _lobbyJoinField = typeof(RunManager).GetField("lobbyJoin", BindingFlags.Instance | BindingFlags.NonPublic);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[RC] Cache error: " + ex.Message);
        }
    }

    /// <summary>
    /// 创建房间 (支持中文名)
    /// </summary>
    public static async void CreateRoom(string roomName, int maxPlayers)
    {
        if (IsCreating)
        {
            Debug.Log("[RC] Already creating...");
            return;
        }

        IsCreating = true;
        maxPlayers = Mathf.Clamp(maxPlayers, 2, 20);
        StatusText = L.T("room.creating");

        try
        {
            EnsureCache();

            // ========== 1. 创建 Steam Lobby ==========
            Debug.Log($"[RC] Creating lobby: name={roomName}, max={maxPlayers}");
            Lobby? result = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);

            if (!result.HasValue)
            {
                StatusText = L.T("room.failed");
                Debug.LogError("[RC] CreateLobbyAsync returned null");
                IsCreating = false;
                return;
            }

            Lobby lobby = result.Value;

            // ========== 2. 配置 Lobby 属性 ==========
            lobby.SetPublic();
            lobby.SetJoinable(true);
            if (!string.IsNullOrWhiteSpace(roomName))
                lobby.SetData("RoomName", roomName);
            lobby.SetData("Region", GetLocalRegion());
            lobby.MaxMembers = maxPlayers;

            Debug.Log($"[RC] Lobby created: id={lobby.Id} owner={lobby.Owner.Id} members={lobby.MemberCount}");

            // ========== 3. 设置 SteamManager 中所有 Lobby/SteamId 字段 ==========
            // SteamManager 可能有 currentLobby / activeLobby 等字段需要设置
            if (SteamManager.instance != null && _allSmFields != null)
            {
                foreach (var field in _allSmFields)
                {
                    try
                    {
                        if (field.IsStatic) continue;
                        if (field.FieldType == typeof(Lobby))
                        {
                            field.SetValue(SteamManager.instance, lobby);
                            Debug.Log($"[RC] Set SM.{field.Name} = Lobby({lobby.Id})");
                        }
                        else if (field.FieldType == typeof(Lobby?))
                        {
                            field.SetValue(SteamManager.instance, (Lobby?)lobby);
                            Debug.Log($"[RC] Set SM.{field.Name} = Lobby?({lobby.Id})");
                        }
                        else if (field.FieldType == typeof(SteamId) && field.Name.ToLower().Contains("lobby"))
                        {
                            field.SetValue(SteamManager.instance, lobby.Id);
                            Debug.Log($"[RC] Set SM.{field.Name} = SteamId({lobby.Id})");
                        }
                    }
                    catch { }
                }
            }

            // ========== 4. 菜单导航 (与 LobbyFinder.JoinLobbyAndPlay 完全一致) ==========
            try
            {
                if (MenuManager.instance != null)
                {
                    MenuManager.instance.PageCloseAll();
                    MenuManager.instance.PageOpen((MenuPageIndex)0, false);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[RC] Menu navigation error: " + ex.Message);
            }

            // ========== 5. 检查是否需要场景切换 (和 LobbyFinder 一致) ==========
            try
            {
                if (RunManager.instance != null &&
                    (UnityEngine.Object)RunManager.instance.levelCurrent != (UnityEngine.Object)RunManager.instance.levelMainMenu)
                {
                    foreach (PlayerAvatar p in GameDirector.instance.PlayerList)
                    {
                        p.OutroStartRPC(default(Photon.Pun.PhotonMessageInfo));
                    }
                    _lobbyJoinField?.SetValue(RunManager.instance, true);
                    RunManager.instance.ChangeLevel(true, false, (RunManager.ChangeLevelType)3);
                }
            }
            catch { }

            // ========== 6. 设置 joinLobby = true (最后! 和 LobbyFinder 完全一致) ==========
            try
            {
                _joinLobbyField?.SetValue(SteamManager.instance, true);
                Debug.Log("[RC] Set joinLobby = true");
            }
            catch { }

            StatusText = L.T("room.success");
            Debug.Log($"[RC] Room creation complete: {lobby.Id}");
        }
        catch (Exception ex)
        {
            StatusText = L.T("room.error");
            Debug.LogError("[RC] Error: " + ex);
        }
        finally
        {
            IsCreating = false;
        }
    }

    private static string GetLocalRegion()
    {
        try
        {
            if (PhotonNetwork.IsConnected && !string.IsNullOrEmpty(PhotonNetwork.CloudRegion))
                return PhotonNetwork.CloudRegion;
        }
        catch { }
        return "unknown";
    }
}
