using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// ASCII 艺术刷屏 — 在聊天中发送预设字符画
/// 注意：R.E.P.O. 会在空格处拆分消息为多条，所以使用全角空格 '\u3000' 或下划线替代
/// </summary>
public static class AsciiArtSpam
{
    public static int selectedArtIndex = 0;
    public static string statusMessage = "";

    // 预设字符画列表 (名称, 行数组)
    // 使用全角空格 \u3000 替代普通空格
    private static readonly List<(string Name, string[] Lines)> _artList = new List<(string, string[])>
    {
        ("♡ 爱心", new string[]
        {
            "＿＿♡♡＿＿＿♡♡＿＿",
            "＿♡＿＿＿♡♡＿＿＿♡",
            "♡＿＿＿＿＿＿＿＿＿♡",
            "＿♡＿＿＿＿＿＿＿♡",
            "＿＿♡＿＿＿＿＿♡",
            "＿＿＿♡＿＿＿♡",
            "＿＿＿＿♡＿♡",
            "＿＿＿＿＿♡"
        }),
        ("☠ 骷髅", new string[]
        {
            "＿＿╭━━━━╮＿＿",
            "＿╭┫●＿＿●┣╮＿",
            "＿┃┃＿▽＿＿┃┃＿",
            "＿╰┫┳┳┳┳┫╯＿",
            "＿＿┃┻┻┻┻┃＿＿"
        }),
        ("( ╯°□°)╯ 掀桌", new string[]
        {
            "(╯°□°)╯︵┻━┻",
            "┬─┬ノ(º_ºノ)",
            "(ノಠ益ಠ)ノ彡┻━┻"
        }),
        ("◤ 中指", new string[]
        {
            "＿＿＿╭∩╮＿＿＿",
            "＿＿＿(◣_◢)＿＿",
            "＿╭∩∩━━━∩∩╮＿",
            "＿┃＿＿＿＿＿┃＿"
        }),
        ("★ GG", new string[]
        {
            "█▀▀＿█▀▀",
            "█＿█＿█＿█",
            "▀▀▀＿▀▀▀"
        }),
        ("⚡ EZ", new string[]
        {
            "█▀▀＿▀▀█",
            "█▀▀＿█▀▀",
            "▀▀▀＿▀▀▀"
        })
    };

    public static string[] GetArtNames()
    {
        string[] names = new string[_artList.Count];
        for (int i = 0; i < _artList.Count; i++)
            names[i] = _artList[i].Name;
        return names;
    }

    public static int ArtCount => _artList.Count;

    /// <summary>
    /// 发送选中的 ASCII 字符画到聊天（需要通过协程逐行发送）
    /// </summary>
    public static void Send(string target, List<object> playerList, List<string> playerNames)
    {
        if (selectedArtIndex < 0 || selectedArtIndex >= _artList.Count) return;

        var art = _artList[selectedArtIndex];
        MonoBehaviour runner = Object.FindObjectOfType<MonoBehaviour>();
        if (runner != null)
        {
            runner.StartCoroutine(SendArtCoroutine(art.Lines, target, playerList, playerNames));
            statusMessage = $"正在发送 {art.Name}...";
        }
    }

    private static IEnumerator SendArtCoroutine(string[] lines, string target,
        List<object> playerList, List<string> playerNames)
    {
        foreach (string line in lines)
        {
            ChatHijack.MakeChat(line, target, playerList, playerNames);
            yield return new WaitForSeconds(0.15f);
        }
        statusMessage = "发送完成!";
        yield return new WaitForSeconds(2f);
        statusMessage = "";
    }
}
