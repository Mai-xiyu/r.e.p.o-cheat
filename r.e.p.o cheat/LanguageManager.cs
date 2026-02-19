using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// i18n 国际化系统 - 从嵌入资源自动加载翻译文件
/// 资源文件格式: key=value (每行一条, # 开头为注释)
/// 自动识别所有 i18n/*.txt 嵌入资源
/// </summary>
public static class LanguageManager
{
    public static string currentLanguage = "zh";
    private static string[] detectedLanguages = null;

    // lang -> (key -> value)
    private static Dictionary<string, Dictionary<string, string>> langData
        = new Dictionary<string, Dictionary<string, string>>();

    private static bool loaded = false;

    /// <summary>
    /// 自动检测打包的i18n语言文件
    /// </summary>
    private static string[] DetectLanguages()
    {
        if (detectedLanguages != null) return detectedLanguages;
        var langs = new List<string>();
        var asm = Assembly.GetExecutingAssembly();
        string prefix = "r.e.p.o_cheat.i18n.";
        string suffix = ".txt";
        foreach (var name in asm.GetManifestResourceNames())
        {
            if (name.StartsWith(prefix) && name.EndsWith(suffix))
            {
                string lang = name.Substring(prefix.Length, name.Length - prefix.Length - suffix.Length);
                if (!string.IsNullOrEmpty(lang)) langs.Add(lang);
            }
        }
        if (langs.Count == 0) langs.Add("zh"); // fallback
        detectedLanguages = langs.ToArray();
        return detectedLanguages;
    }

    /// <summary>
    /// 获取所有可用语言代码
    /// </summary>
    public static string[] GetAvailableLanguages()
    {
        EnsureLoaded();
        return DetectLanguages();
    }

    /// <summary>
    /// 确保语言数据已加载
    /// </summary>
    private static void EnsureLoaded()
    {
        if (loaded) return;
        loaded = true;

        foreach (var lang in DetectLanguages())
        {
            var dict = new Dictionary<string, string>();
            langData[lang] = dict;

            // 嵌入资源名: {namespace}.i18n.{lang}.txt
            string resourceName = "r.e.p.o_cheat.i18n." + lang + ".txt";
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                using (var stream = asm.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        Debug.LogWarning("[i18n] Resource not found: " + resourceName);
                        continue;
                    }
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            line = line.Trim();
                            if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                                continue;
                            int eq = line.IndexOf('=');
                            if (eq <= 0) continue;
                            string key = line.Substring(0, eq).Trim();
                            string val = line.Substring(eq + 1);
                            dict[key] = val;
                        }
                    }
                }
                Debug.Log("[i18n] Loaded " + dict.Count + " keys for lang: " + lang);
            }
            catch (Exception ex)
            {
                Debug.LogError("[i18n] Failed to load " + resourceName + ": " + ex.Message);
            }
        }
    }

    /// <summary>
    /// 获取翻译文本 (短名: T)
    /// </summary>
    public static string T(string key)
    {
        EnsureLoaded();

        // 先查当前语言
        if (langData.TryGetValue(currentLanguage, out var dict) && dict.TryGetValue(key, out var val))
            return val;
        // fallback 到中文
        if (currentLanguage != "zh" && langData.TryGetValue("zh", out var zhDict) && zhDict.TryGetValue(key, out var zhVal))
            return zhVal;

        return key; // 最终 fallback 返回 key
    }

    /// <summary>
    /// 带格式化参数的翻译
    /// </summary>
    public static string T(string key, params object[] args)
    {
        string template = T(key);
        try { return string.Format(template, args); }
        catch { return template; }
    }

    /// <summary>
    /// 获取颜色名称 (从 color.{index} 键)
    /// </summary>
    public static string GetColorName(int index)
    {
        return T("color." + index);
    }

    /// <summary>
    /// 获取敌人的翻译名称
    /// 清理 "Enemy - " 前缀和 "(Clone)" 后缀后查找 enemy.name.{cleaned} 键
    /// 如果没有翻译则返回清理后的原始名称
    /// </summary>
    public static string GetEnemyName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName)) return T("common.enemy");
        string cleaned = rawName;
        if (cleaned.StartsWith("Enemy -", StringComparison.OrdinalIgnoreCase))
            cleaned = cleaned.Substring("Enemy -".Length).Trim();
        if (cleaned.StartsWith("Enemy-", StringComparison.OrdinalIgnoreCase))
            cleaned = cleaned.Substring("Enemy-".Length).Trim();
        if (cleaned.EndsWith("(Clone)", StringComparison.OrdinalIgnoreCase))
            cleaned = cleaned.Substring(0, cleaned.Length - "(Clone)".Length).Trim();
        if (string.IsNullOrEmpty(cleaned)) return T("common.enemy");

        string key = "enemy.name." + cleaned;
        string result = T(key);
        // 如果 T() 返回的是key本身，说明没有翻译，返回cleaned原名
        return result == key ? cleaned : result;
    }

    /// <summary>
    /// 获取物品的翻译名称
    /// 清理 "Valuable" 前缀和 "(Clone)" 后缀后查找 item.name.{cleaned} 键
    /// 如果没有翻译则返回清理后的原始名称
    /// </summary>
    public static string GetItemName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName)) return rawName;
        string cleaned = rawName;
        if (cleaned.StartsWith("Valuable", StringComparison.OrdinalIgnoreCase))
            cleaned = cleaned.Substring("Valuable".Length).Trim();
        if (cleaned.EndsWith("(Clone)", StringComparison.OrdinalIgnoreCase))
            cleaned = cleaned.Substring(0, cleaned.Length - "(Clone)".Length).Trim();
        if (string.IsNullOrEmpty(cleaned)) return rawName;

        string key = "item.name." + cleaned;
        string result = T(key);
        return result == key ? cleaned : result;
    }

    /// <summary>
    /// 兼容旧 API
    /// </summary>
    public static string Get(string key) => T(key);

    /// <summary>
    /// 切换到下一个语言
    /// </summary>
    public static void ToggleLanguage()
    {
        var langs = DetectLanguages();
        int idx = Array.IndexOf(langs, currentLanguage);
        currentLanguage = langs[(idx + 1) % langs.Length];
    }

    /// <summary>
    /// 直接设置指定语言
    /// </summary>
    public static void SetLanguage(string lang)
    {
        EnsureLoaded();
        if (langData.ContainsKey(lang))
            currentLanguage = lang;
    }

    /// <summary>
    /// 获取语言显示名称 (从语言文件的 lang.name 键读取)
    /// </summary>
    public static string GetLanguageDisplayName(string lang)
    {
        EnsureLoaded();
        if (langData.TryGetValue(lang, out var dict) && dict.TryGetValue("lang.name", out var name))
            return name;
        return lang.ToUpper();
    }

    /// <summary>
    /// 获取 Tab 名称数组
    /// </summary>
    public static string[] GetTabNames()
    {
        return new string[]
        {
            T("tab.self"), T("tab.visuals"), T("tab.combat"), T("tab.misc"),
            T("tab.enemies"), T("tab.items"), T("tab.hotkeys"), T("tab.trolling"),
            T("tab.config"), T("tab.server"), T("tab.teleport"), T("tab.menu")
        };
    }
}

/// <summary>
/// 短别名: L.T("key") 或 L.T("key", arg1, arg2)
/// </summary>
public static class L
{
    public static string T(string key) => LanguageManager.T(key);
    public static string T(string key, params object[] args) => LanguageManager.T(key, args);
}