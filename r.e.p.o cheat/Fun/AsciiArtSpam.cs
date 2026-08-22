using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// Multiline chat-art sender. The game already accepts LF newlines in one chat
/// payload, so art is encoded once and sent as one message whenever possible.
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
        {
            names[i] = _artList[i].Name;
        }
        return names;
    }

    public static int ArtCount => _artList.Count;

    public static string GetArtText(int index)
    {
        if (index < 0 || index >= _artList.Count)
        {
            return "";
        }
        return string.Join("\n", _artList[index].Lines);
    }

    public static void Send(string target, List<object> playerList, List<string> playerNames)
    {
        SendCustom(GetArtText(selectedArtIndex), target, playerList, playerNames);
    }

    public static void SendCustom(string text, string target, List<object> playerList, List<string> playerNames)
    {
        string[] payloads = ChatArtCodec.BuildPayloads(text);
        if (payloads.Length == 0)
        {
            return;
        }

        if (payloads.Length == 1)
        {
            ChatHijack.MakeChat(payloads[0], target, playerList, playerNames);
            statusMessage = ChatArtCodec.LooksLikeArt(text) ? L.T("fun.ascii_done") : "";
            if (statusMessage.Length > 0)
            {
                Loader.RunCoroutine(ClearStatusCoroutine());
            }
            return;
        }

        statusMessage = L.T("fun.ascii_sending");
        Loader.RunCoroutine(SendPayloadsCoroutine(payloads, target, playerList, playerNames));
    }

    private static IEnumerator SendPayloadsCoroutine(string[] payloads, string target,
        List<object> playerList, List<string> playerNames)
    {
        for (int i = 0; i < payloads.Length; i++)
        {
            ChatHijack.MakeChat(payloads[i], target, playerList, playerNames);
            if (i + 1 < payloads.Length)
            {
                // A new TTS message stops the previous one. Chunking is only a
                // large-payload fallback, so leave enough time between chunks.
                yield return new WaitForSecondsRealtime(0.75f);
            }
        }
        statusMessage = L.T("fun.ascii_done");
        yield return new WaitForSecondsRealtime(2f);
        statusMessage = "";
    }

    private static IEnumerator ClearStatusCoroutine()
    {
        yield return new WaitForSecondsRealtime(2f);
        statusMessage = "";
    }
}
