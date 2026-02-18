using System;
using System.Collections.Generic;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// ESP增强 - 追踪线 + ESP预设快速切换
/// </summary>
public static class ESPEnhancements
{
    // === 追踪线 ===
    public static bool showTraceLinesEnemy = false;
    public static bool showTraceLinesItem = false;
    public static bool showTraceLinesPlayer = false;

    private static Texture2D lineTex;

    // === ESP预设 ===
    public enum ESPPreset
    {
        Custom,         // 自定义（当前设置）
        HighValueOnly,  // 只看高价物品
        EnemyOnly,      // 只看敌人
        PlayerOnly,     // 只看玩家
        Everything,     // 全部显示
        Stealth         // 隐蔽模式（只看敌人距离）
    }

    public static ESPPreset currentPreset = ESPPreset.Custom;

    // 预设名称
    public static readonly string[] presetNames = new string[]
    {
        "自定义",
        "高价物品",
        "仅敌人",
        "仅玩家",
        "全部显示",
        "隐蔽模式"
    };

    private static void EnsureLineTex()
    {
        if (lineTex == null)
        {
            lineTex = new Texture2D(1, 1);
            lineTex.SetPixel(0, 0, Color.white);
            lineTex.Apply();
        }
    }

    /// <summary>
    /// 在 OnGUI 中绘制追踪线（从屏幕底部中央到目标）
    /// </summary>
    public static void DrawTraceLines()
    {
        if (!showTraceLinesEnemy && !showTraceLinesItem && !showTraceLinesPlayer) return;

        try
        {
            EnsureLineTex();

            Camera cam = Camera.main;
            if ((Object)(object)cam == (Object)null) return;

            Vector2 screenBottom = new Vector2(Screen.width / 2f, Screen.height);

            // 敌人追踪线
            if (showTraceLinesEnemy)
            {
                try
                {
                    Enemy[] enemies = UnityEngine.Object.FindObjectsOfType<Enemy>();
                    if (enemies != null)
                    {
                        foreach (Enemy enemy in enemies)
                        {
                            if ((Object)(object)enemy == (Object)null) continue;
                            Vector3 worldPos = ((Component)enemy).transform.position;
                            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
                            if (screenPos.z > 0)
                            {
                                DrawGUILine(screenBottom, new Vector2(screenPos.x, Screen.height - screenPos.y), new Color(1f, 0.2f, 0.2f, 0.5f), 1.5f);
                            }
                        }
                    }
                }
                catch { }
            }

            // 物品追踪线
            if (showTraceLinesItem)
            {
                try
                {
                    ValuableObject[] items = UnityEngine.Object.FindObjectsOfType<ValuableObject>();
                    if (items != null)
                    {
                        foreach (ValuableObject item in items)
                        {
                            if ((Object)(object)item == (Object)null) continue;
                            Vector3 worldPos = ((Component)item).transform.position;
                            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
                            if (screenPos.z > 0)
                            {
                                DrawGUILine(screenBottom, new Vector2(screenPos.x, Screen.height - screenPos.y), new Color(1f, 1f, 0f, 0.4f), 1f);
                            }
                        }
                    }
                }
                catch { }
            }

            // 玩家追踪线
            if (showTraceLinesPlayer)
            {
                try
                {
                    GameObject localPlayer = DebugCheats.GetLocalPlayer();
                    List<PlayerAvatar> players = SemiFunc.PlayerGetList();
                    if (players != null)
                    {
                        foreach (PlayerAvatar pa in players)
                        {
                            if ((Object)(object)pa == (Object)null) continue;
                            GameObject go = ((Component)pa).gameObject;
                            if ((Object)(object)localPlayer != (Object)null && go == localPlayer) continue;
                            Vector3 worldPos = go.transform.position;
                            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
                            if (screenPos.z > 0)
                            {
                                DrawGUILine(screenBottom, new Vector2(screenPos.x, Screen.height - screenPos.y), new Color(0f, 1f, 0f, 0.5f), 1.5f);
                            }
                        }
                    }
                }
                catch { }
            }
        }
        catch { }
    }

    /// <summary>
    /// 应用ESP预设
    /// </summary>
    public static void ApplyPreset(ESPPreset preset)
    {
        currentPreset = preset;

        switch (preset)
        {
            case ESPPreset.HighValueOnly:
                DebugCheats.drawEspBool = false;
                DebugCheats.drawItemEspBool = true;
                DebugCheats.drawPlayerEspBool = false;
                DebugCheats.drawExtractionPointEspBool = true;
                DebugCheats.showItemValue = true;
                DebugCheats.showItemNames = true;
                DebugCheats.showItemDistance = true;
                DebugCheats.minItemValue = 500;
                showTraceLinesEnemy = false;
                showTraceLinesItem = true;
                showTraceLinesPlayer = false;
                break;

            case ESPPreset.EnemyOnly:
                DebugCheats.drawEspBool = true;
                DebugCheats.drawItemEspBool = false;
                DebugCheats.drawPlayerEspBool = false;
                DebugCheats.drawExtractionPointEspBool = false;
                DebugCheats.showEnemyNames = true;
                DebugCheats.showEnemyDistance = true;
                DebugCheats.showEnemyHP = true;
                showTraceLinesEnemy = true;
                showTraceLinesItem = false;
                showTraceLinesPlayer = false;
                break;

            case ESPPreset.PlayerOnly:
                DebugCheats.drawEspBool = false;
                DebugCheats.drawItemEspBool = false;
                DebugCheats.drawPlayerEspBool = true;
                DebugCheats.drawExtractionPointEspBool = false;
                DebugCheats.showPlayerNames = true;
                DebugCheats.showPlayerDistance = true;
                DebugCheats.showPlayerHP = true;
                showTraceLinesEnemy = false;
                showTraceLinesItem = false;
                showTraceLinesPlayer = true;
                break;

            case ESPPreset.Everything:
                DebugCheats.drawEspBool = true;
                DebugCheats.drawItemEspBool = true;
                DebugCheats.drawPlayerEspBool = true;
                DebugCheats.drawExtractionPointEspBool = true;
                DebugCheats.showEnemyNames = true;
                DebugCheats.showEnemyDistance = true;
                DebugCheats.showEnemyHP = true;
                DebugCheats.showEnemyBox = true;
                DebugCheats.showItemNames = true;
                DebugCheats.showItemValue = true;
                DebugCheats.showItemDistance = true;
                DebugCheats.showPlayerNames = true;
                DebugCheats.showPlayerDistance = true;
                DebugCheats.showPlayerHP = true;
                DebugCheats.showExtractionNames = true;
                DebugCheats.showExtractionDistance = true;
                showTraceLinesEnemy = true;
                showTraceLinesItem = true;
                showTraceLinesPlayer = true;
                break;

            case ESPPreset.Stealth:
                DebugCheats.drawEspBool = true;
                DebugCheats.drawItemEspBool = false;
                DebugCheats.drawPlayerEspBool = false;
                DebugCheats.drawExtractionPointEspBool = true;
                DebugCheats.showEnemyNames = false;
                DebugCheats.showEnemyDistance = true;
                DebugCheats.showEnemyHP = false;
                DebugCheats.showEnemyBox = false;
                DebugCheats.drawChamsBool = false;
                showTraceLinesEnemy = false;
                showTraceLinesItem = false;
                showTraceLinesPlayer = false;
                break;

            case ESPPreset.Custom:
                // 不改变任何设置
                break;
        }
    }

    private static void DrawGUILine(Vector2 start, Vector2 end, Color color, float width)
    {
        EnsureLineTex();

        Color savedColor = GUI.color;
        GUI.color = color;

        float length = Vector2.Distance(start, end);
        float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;

        GUIUtility.RotateAroundPivot(angle, start);
        GUI.DrawTexture(new Rect(start.x, start.y - width / 2f, length, width), lineTex);
        GUIUtility.RotateAroundPivot(-angle, start);

        GUI.color = savedColor;
    }
}
