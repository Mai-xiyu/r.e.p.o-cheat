using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// 聊天命令系统 — 在聊天输入 !command 触发作弊功能
/// 命令不会广播给其他玩家
/// </summary>
public static class ChatCommands
{
    private static string lastFeedback = "";
    private static float feedbackTime = 0f;

    /// <summary>
    /// 检查消息是否为命令，如果是则执行并返回 true（不发送到聊天）
    /// </summary>
    public static bool TryExecuteCommand(string message)
    {
        if (string.IsNullOrEmpty(message)) return false;
        message = message.Trim();
        if (!message.StartsWith("!") && !message.StartsWith("/")) return false;

        string cmd = message.Substring(1).Trim();
        string[] parts = cmd.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return true;

        string command = parts[0].ToLower();
        string[] args = parts.Skip(1).ToArray();

        try
        {
            switch (command)
            {
                case "god":
                    ToggleGodMode();
                    break;
                case "noclip":
                case "nc":
                    ToggleNoclip();
                    break;
                case "tp":
                    TeleportToPlayer(args);
                    break;
                case "kill":
                    KillSelectedEnemy();
                    break;
                case "killall":
                case "ka":
                    KillAllEnemies();
                    break;
                case "speed":
                    SetGameSpeed(args);
                    break;
                case "heal":
                    HealSelf();
                    break;
                case "items":
                    TeleportAllItems();
                    break;
                case "maxup":
                case "upgrade":
                    MaxAllUpgrades();
                    break;
                case "freeze":
                    ToggleFreezeEnemies();
                    break;
                case "inflate":
                    InflateAllItems(args);
                    break;
                case "dup":
                case "duplicate":
                    DuplicateHeldItem();
                    break;
                case "stealth":
                    ToggleStealth();
                    break;
                case "help":
                case "h":
                    ShowHelp();
                    break;
                default:
                    SetFeedback(L.T("cmd.unknown", command));
                    break;
            }
        }
        catch (Exception ex)
        {
            SetFeedback(L.T("cmd.error", ex.Message));
            Debug.LogWarning("[ChatCmd] Error: " + ex.Message);
        }

        return true; // command was handled, don't send to chat
    }

    private static void ToggleGodMode()
    {
        Hax2.godModeActive = !Hax2.godModeActive;
        PlayerController.SetGodMode(Hax2.godModeActive);
        SetFeedback(Hax2.godModeActive ? L.T("cmd.god_on") : L.T("cmd.god_off"));
    }

    private static void ToggleNoclip()
    {
        NoclipController.ToggleNoclip();
        SetFeedback(NoclipController.noclipActive ? L.T("cmd.noclip_on") : L.T("cmd.noclip_off"));
    }

    private static void TeleportToPlayer(string[] args)
    {
        if (args.Length == 0)
        {
            SetFeedback(L.T("cmd.tp_usage"));
            return;
        }
        string target = string.Join(" ", args).ToLower();
        var players = SemiFunc.PlayerGetList();
        PlayerAvatar found = null;
        foreach (var p in players)
        {
            string name = SemiFunc.PlayerGetName(p)?.ToLower() ?? "";
            if (name.Contains(target))
            {
                found = p;
                break;
            }
        }

        if (found == null)
        {
            SetFeedback(L.T("cmd.tp_not_found", target));
            return;
        }

        GameObject localPlayer = DebugCheats.GetLocalPlayer();
        if (localPlayer != null)
        {
            localPlayer.transform.position = ((Component)found).transform.position + Vector3.up * 0.5f;
            SetFeedback(L.T("cmd.tp_done", SemiFunc.PlayerGetName(found)));
        }
    }

    private static void KillSelectedEnemy()
    {
        var enemies = DebugCheats.enemyList;
        if (enemies == null || enemies.Count == 0)
        {
            SetFeedback(L.T("cmd.no_enemies"));
            return;
        }
        // Kill nearest enemy
        GameObject localPlayer = DebugCheats.GetLocalPlayer();
        if (localPlayer == null) return;

        Enemy nearest = null;
        float nearestDist = float.MaxValue;
        foreach (var e in enemies)
        {
            if (e == null) continue;
            float dist = Vector3.Distance(localPlayer.transform.position, ((Component)e).transform.position);
            if (dist < nearestDist) { nearestDist = dist; nearest = e; }
        }
        if (nearest != null)
        {
            FieldInfo healthField = nearest.GetType().GetField("Health", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (healthField != null)
            {
                object healthObj = healthField.GetValue(nearest);
                if (healthObj != null)
                {
                    MethodInfo hurtMethod = healthObj.GetType().GetMethod("Hurt", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    hurtMethod?.Invoke(healthObj, new object[] { 9999, Vector3.zero });
                }
            }
            SetFeedback(L.T("cmd.kill_done"));
        }
    }

    private static void KillAllEnemies()
    {
        Enemies.KillAllEnemies();
        SetFeedback(L.T("cmd.killall_done"));
    }

    private static void SetGameSpeed(string[] args)
    {
        if (args.Length == 0)
        {
            SetFeedback(L.T("cmd.speed_usage"));
            return;
        }
        if (float.TryParse(args[0], out float speed))
        {
            speed = Mathf.Clamp(speed, 0.1f, 5.0f);
            Time.timeScale = speed;
            Time.fixedDeltaTime = 0.02f * speed;
            SetFeedback(L.T("cmd.speed_set", speed.ToString("F1")));
        }
    }

    private static void HealSelf()
    {
        PlayerController.MaxHealth();
        SetFeedback(L.T("cmd.heal_done"));
    }

    private static void TeleportAllItems()
    {
        ItemTeleport.TeleportAllItemsToMe();
        SetFeedback(L.T("cmd.items_done"));
    }

    private static void MaxAllUpgrades()
    {
        UpgradeHelper.MaxAllUpgrades();
        SetFeedback(L.T("cmd.maxup_done"));
    }

    private static void ToggleFreezeEnemies()
    {
        Enemies.freezeAllEnemies = !Enemies.freezeAllEnemies;
        if (!Enemies.freezeAllEnemies)
            Enemies.UnfreezeAllEnemies();
        SetFeedback(Enemies.freezeAllEnemies ? L.T("cmd.freeze_on") : L.T("cmd.freeze_off"));
    }

    private static void InflateAllItems(string[] args)
    {
        float value = 99999f;
        if (args.Length > 0) float.TryParse(args[0], out value);
        ItemInflater.InflateAll(value);
        SetFeedback(L.T("cmd.inflate_done", value.ToString("N0")));
    }

    private static void DuplicateHeldItem()
    {
        bool success = ItemDuplicator.DuplicateHeldItem();
        SetFeedback(success ? L.T("cmd.dup_done") : L.T("cmd.dup_fail"));
    }

    private static void ToggleStealth()
    {
        StealthMode.isEnabled = !StealthMode.isEnabled;
        StealthMode.Apply();
        SetFeedback(StealthMode.isEnabled ? L.T("cmd.stealth_on") : L.T("cmd.stealth_off"));
    }

    private static void ShowHelp()
    {
        SetFeedback(L.T("cmd.help_text"));
    }

    public static void SetFeedback(string msg)
    {
        lastFeedback = msg;
        feedbackTime = Time.time;
    }

    public static string GetFeedback()
    {
        if (Time.time - feedbackTime > 5f) return "";
        return lastFeedback;
    }
}
