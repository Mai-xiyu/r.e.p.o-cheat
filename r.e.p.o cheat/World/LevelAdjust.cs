using System;
using System.Collections.Generic;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// 关卡调整 — 查看和修改下一关卡
/// 通过反射访问 RunManager 来获取/设置关卡
/// </summary>
public static class LevelAdjust
{
    public static string statusMessage = "";
    public static int selectedLevelIndex = 0;
    public static string[] availableLevels = new string[0];

    private static Type _runManagerType;
    private static object _runManagerInstance;
    private static bool _searchedRunManager = false;

    private static void EnsureRunManager()
    {
        if (_searchedRunManager && _runManagerInstance != null) return;
        _searchedRunManager = true;
        try
        {
            _runManagerType = Type.GetType("RunManager, Assembly-CSharp");
            if (_runManagerType != null)
            {
                // 尝试单例 instance
                var instProp = _runManagerType.GetProperty("instance",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (instProp != null)
                {
                    _runManagerInstance = instProp.GetValue(null);
                }
                if (_runManagerInstance == null)
                {
                    var instField = _runManagerType.GetField("instance",
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (instField != null)
                        _runManagerInstance = instField.GetValue(null);
                }
                if (_runManagerInstance == null)
                {
                    _runManagerInstance = GameHelper.FindObjectOfType(_runManagerType);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[LevelAdjust] RunManager search error: " + ex.Message);
        }
    }

    /// <summary>
    /// 扫描可用关卡列表
    /// </summary>
    public static void RefreshLevels()
    {
        _searchedRunManager = false;
        EnsureRunManager();

        var levels = new List<string>();
        try
        {
            if (_runManagerType != null && _runManagerInstance != null)
            {
                // 尝试查找关卡列表字段
                var fields = _runManagerType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var f in fields)
                {
                    // 查找 List<Level> 或 Level[] 类型
                    if (f.FieldType.IsGenericType &&
                        f.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        Type elemType = f.FieldType.GetGenericArguments()[0];
                        if (elemType.Name == "Level" || elemType.Name.Contains("Level"))
                        {
                            var list = f.GetValue(_runManagerInstance) as System.Collections.IList;
                            if (list != null)
                            {
                                foreach (var item in list)
                                {
                                    string name = item?.ToString() ?? "Unknown";
                                    // 尝试获取 name 字段
                                    var nameField = item.GetType().GetField("name",
                                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                    if (nameField != null)
                                    {
                                        name = nameField.GetValue(item)?.ToString() ?? name;
                                    }
                                    else
                                    {
                                        // 尝试 NakedName 或类似属性
                                        var props = item.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                                        foreach (var p in props)
                                        {
                                            if (p.Name.ToLower().Contains("name"))
                                            {
                                                name = p.GetValue(item)?.ToString() ?? name;
                                                break;
                                            }
                                        }
                                    }
                                    levels.Add(name);
                                }
                            }
                        }
                    }
                }

                // 如果没找到 List<Level>，尝试找 levels / levelList 等命名字段
                if (levels.Count == 0)
                {
                    foreach (var f in fields)
                    {
                        string fn = f.Name.ToLower();
                        if ((fn.Contains("level") || fn.Contains("scene")) && f.FieldType == typeof(string[]))
                        {
                            var arr = f.GetValue(_runManagerInstance) as string[];
                            if (arr != null)
                            {
                                levels.AddRange(arr);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[LevelAdjust] Level scan error: " + ex.Message);
        }

        if (levels.Count == 0)
        {
            // 硬编码 fallback 常见关卡
            levels.AddRange(new[] { "Level1", "Level2", "Level3", "Level_Lobby", "Level_Shop", "Level_Arena" });
            statusMessage = "使用默认关卡列表 (未找到 RunManager)";
        }
        else
        {
            statusMessage = $"找到 {levels.Count} 个关卡";
        }

        availableLevels = levels.ToArray();
        if (selectedLevelIndex >= availableLevels.Length) selectedLevelIndex = 0;
    }

    /// <summary>
    /// 设置下一关卡
    /// </summary>
    public static void SetNextLevel()
    {
        if (availableLevels.Length == 0 || selectedLevelIndex >= availableLevels.Length)
        {
            statusMessage = "未选择关卡";
            return;
        }

        EnsureRunManager();
        string levelName = availableLevels[selectedLevelIndex];

        try
        {
            if (_runManagerType != null && _runManagerInstance != null)
            {
                // 尝试查找 "levelsInRun" / "currentLevelIndex" / "nextLevel" 等字段
                var fields = _runManagerType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                bool set = false;

                foreach (var f in fields)
                {
                    string fn = f.Name.ToLower();
                    if (fn.Contains("next") && fn.Contains("level") && !f.FieldType.IsValueType)
                    {
                        // 可能是 Level 对象引用
                        f.SetValue(_runManagerInstance, null); // 触发重新选择
                        set = true;
                    }
                    else if (fn.Contains("levelcurrent") || (fn.Contains("current") && fn.Contains("level")))
                    {
                        if (f.FieldType == typeof(int))
                        {
                            f.SetValue(_runManagerInstance, selectedLevelIndex);
                            set = true;
                        }
                    }
                }

                // 尝试通过 RPC 方式加载关卡
                if (!set)
                {
                    var punManager = UnityEngine.Object.FindObjectOfType<PunManager>();
                    if (punManager != null)
                    {
                        var pv = ((Component)punManager).GetComponent<PhotonView>();
                        if (pv != null)
                        {
                            // 尝试通用的关卡切换 RPC
                            try
                            {
                                pv.RPC("LoadLevelRPC", RpcTarget.All, new object[] { levelName });
                                set = true;
                            }
                            catch { }
                        }
                    }
                }

                statusMessage = set ? $"已设置下一关: {levelName}" : $"无法设置关卡 (未找到字段)";
            }
            else
            {
                statusMessage = "RunManager 未找到";
            }
        }
        catch (Exception ex)
        {
            statusMessage = "设置失败: " + ex.Message;
            Debug.LogWarning("[LevelAdjust] Set next level error: " + ex.Message);
        }
    }
}
