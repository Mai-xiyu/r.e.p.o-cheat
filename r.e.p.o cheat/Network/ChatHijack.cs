using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace r.e.p.o_cheat;

public static class ChatHijack
{
    private static readonly FieldInfo IsCrouchingField = typeof(PlayerAvatar).GetField(
        "isCrouching", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    private static readonly FieldInfo IsDisabledField = typeof(PlayerAvatar).GetField(
        "isDisabled", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    private static readonly Regex RichTextTagRegex = new Regex("<.*?>", RegexOptions.Compiled);

    private static string _originalNickName;

    private static bool IsAllTarget(string targetName)
    {
        return targetName == "All" || targetName == L.T("common.all");
    }

    public static void MakeChat(string message, string targetName, List<object> playerList, List<string> playerNames)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }
        bool gameSlashCommand = message.StartsWith("/", StringComparison.Ordinal);
        if ((message.StartsWith("!", StringComparison.Ordinal) || gameSlashCommand) &&
            ChatCommands.TryExecuteCommand(message))
        {
            return;
        }

        try
        {
            PlayerAvatar local = SemiFunc.PlayerAvatarLocal();
            if (local == null || local.photonView == null)
            {
                return;
            }

            // Preserve the game's own slash-command path. Calling the RPC
            // directly would bypass PlayerAvatar.ChatMessageSend's command check.
            if (gameSlashCommand || !SemiFunc.IsMultiplayer() || IsAllTarget(targetName))
            {
                local.ChatMessageSend(message);
                return;
            }

            if (!TryResolveTarget(targetName, playerList, playerNames, out Player target))
            {
                Debug.LogWarning("[Chat] selected target is no longer available: " + StripRichText(targetName));
                return;
            }

            bool crouching = ReadBool(IsCrouchingField, local) || ReadBool(IsDisabledField, local);
            local.photonView.RPC("ChatMessageSendRPC", target, message, crouching);
        }
        catch (Exception ex)
        {
            Debug.LogError("[Chat] " + ex.GetType().Name + ": " + ex.Message);
        }
    }

    private static bool TryResolveTarget(string targetName, List<object> playerList,
        List<string> playerNames, out Player target)
    {
        target = null;
        if (playerList == null || playerNames == null)
        {
            return false;
        }

        int index = playerNames.IndexOf(targetName);
        if (index < 0)
        {
            string visibleTarget = StripRichText(targetName);
            int uniqueMatch = -1;
            int count = Math.Min(playerList.Count, playerNames.Count);
            for (int i = 0; i < count; i++)
            {
                PlayerAvatar avatar = playerList[i] as PlayerAvatar;
                if (avatar == null)
                {
                    continue;
                }

                string visibleEntry = StripRichText(playerNames[i]);
                string avatarName = SemiFunc.PlayerGetName(avatar) ?? string.Empty;
                bool matches = string.Equals(visibleEntry, visibleTarget, StringComparison.Ordinal) ||
                    string.Equals(avatarName, visibleTarget, StringComparison.Ordinal) ||
                    (!string.IsNullOrEmpty(avatarName) &&
                        visibleTarget.EndsWith(" " + avatarName, StringComparison.Ordinal));
                if (!matches)
                {
                    continue;
                }
                if (uniqueMatch >= 0)
                {
                    return false;
                }
                uniqueMatch = i;
            }
            index = uniqueMatch;
        }

        if (index < 0 || index >= playerList.Count)
        {
            return false;
        }

        PlayerAvatar selected = playerList[index] as PlayerAvatar;
        PhotonView view = selected != null ? selected.photonView : null;
        target = view != null ? view.Owner : null;
        return target != null;
    }

    private static bool ReadBool(FieldInfo field, object instance)
    {
        try
        {
            return field != null && field.GetValue(instance) is bool value && value;
        }
        catch
        {
            return false;
        }
    }

    private static string StripRichText(string text)
    {
        return RichTextTagRegex.Replace(text ?? string.Empty, string.Empty).Trim();
    }

    public static void ToggleNameSpoofing(bool enable, string spoofName, string targetName, List<object> playerList, List<string> playerNames)
    {
        if (enable)
        {
            SpoofLocalPlayerName(spoofName);
        }
        else
        {
            RestoreLocalName();
        }
    }

    public static void ChangePlayerColor(int colorIndex, string targetName, List<object> playerList, List<string> playerNames)
    {
        CosmeticFeatures.ApplyPaletteColor(colorIndex, sync: true);
    }

    public static void ClearStoredNames()
    {
        _originalNickName = null;
    }

    public static bool SpoofLocalPlayerName(string newName)
    {
        try
        {
            if (string.IsNullOrEmpty(newName))
            {
                return false;
            }
            PlayerAvatar local = SemiFunc.PlayerAvatarLocal();
            if (local == null || local.photonView == null || !local.photonView.IsMine)
            {
                return false;
            }
            if (string.IsNullOrEmpty(_originalNickName))
            {
                _originalNickName = PhotonNetwork.NickName;
                if (string.IsNullOrEmpty(_originalNickName))
                {
                    _originalNickName = SemiFunc.PlayerGetName(local);
                }
            }
            string steamId = SemiFunc.PlayerGetSteamID(local);
            PhotonNetwork.NickName = newName;
            if (PhotonNetwork.LocalPlayer != null)
            {
                PhotonNetwork.LocalPlayer.NickName = newName;
            }
            if (global::PlayerController.instance != null && !string.IsNullOrEmpty(steamId))
            {
                global::PlayerController.instance.PlayerSetName(newName, steamId);
            }
            if (SemiFunc.IsMultiplayer())
            {
                local.photonView.RPC("AddToStatsManagerRPC", RpcTarget.AllBuffered, newName, steamId);
            }
            else
            {
                local.AddToStatsManagerRPC(newName, steamId, default(PhotonMessageInfo));
            }
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError("SpoofLocalPlayerName 失败: " + ex.Message);
        }
        return false;
    }

    private static void RestoreLocalName()
    {
        if (!string.IsNullOrEmpty(_originalNickName))
        {
            SpoofLocalPlayerName(_originalNickName);
        }
        _originalNickName = null;
    }
}
