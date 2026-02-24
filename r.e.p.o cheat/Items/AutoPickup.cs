using System;
using System.Collections.Generic;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// 自动拾取 - 自动将附近高价值物品传送到玩家
/// 自动卖出 - 自动将物品传送到撤离点
/// </summary>
public static class AutoPickup
{
    public static bool isAutoPickupEnabled = false;
    public static bool isAutoSellEnabled = false;

    // 自动拾取参数
    public static float pickupRadius = 30f;         // 拾取范围
    public static int minPickupValue = 100;          // 最小拾取价值
    public static float pickupInterval = 1.5f;       // 拾取间隔（秒）

    private static float lastPickupTime = 0f;
    private static float lastSellTime = 0f;
    private static float sellInterval = 2f;

    /// <summary>
    /// 在 Update 中调用
    /// </summary>
    public static void UpdateAutoPickup()
    {
        if (!isAutoPickupEnabled && !isAutoSellEnabled) return;

        try
        {
            if (isAutoPickupEnabled && Time.time - lastPickupTime >= pickupInterval)
            {
                lastPickupTime = Time.time;
                PickupNearbyItems();
            }

            if (isAutoSellEnabled && Time.time - lastSellTime >= sellInterval)
            {
                lastSellTime = Time.time;
                SellItemsToExtraction();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError((object)$"[自动拾取] 错误: {ex.Message}");
        }
    }

    private static void PickupNearbyItems()
    {
        try
        {
            GameObject localPlayer = DebugCheats.GetLocalPlayer();
            if ((Object)(object)localPlayer == (Object)null) return;

            Vector3 playerPos = localPlayer.transform.position;
            ValuableObject[] valuables = Object.FindObjectsOfType<ValuableObject>();

            if (valuables == null || valuables.Length == 0) return;

            foreach (ValuableObject valuable in valuables)
            {
                if ((Object)(object)valuable == (Object)null) continue;

                Transform t = ((Component)valuable).transform;
                if ((Object)(object)t == (Object)null) continue;

                float dist = Vector3.Distance(playerPos, t.position);
                if (dist > pickupRadius) continue;

                // 读取价值
                int value = GetItemValue(valuable);
                if (value < minPickupValue) continue;

                // 传送物品到玩家位置
                Vector3 targetPos = playerPos + Vector3.up * 0.5f + UnityEngine.Random.insideUnitSphere * 0.5f;
                targetPos.y = playerPos.y + 0.5f;

                PhotonView pv = ((Component)valuable).GetComponent<PhotonView>();
                if (pv != null && PhotonNetwork.IsConnected)
                {
                    t.position = targetPos;
                }
                else
                {
                    t.position = targetPos;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError((object)$"[自动拾取] 拾取失败: {ex.Message}");
        }
    }

    private static void SellItemsToExtraction()
    {
        try
        {
            GameObject localPlayer = DebugCheats.GetLocalPlayer();
            if ((Object)(object)localPlayer == (Object)null) return;

            // 找到最近的撤离点
            ExtractionPoint[] points = Object.FindObjectsOfType<ExtractionPoint>();
            if (points == null || points.Length == 0) return;

            ExtractionPoint nearest = null;
            float nearestDist = float.MaxValue;
            Vector3 playerPos = localPlayer.transform.position;

            foreach (var point in points)
            {
                if ((Object)(object)point == (Object)null) continue;
                float dist = Vector3.Distance(playerPos, ((Component)point).transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = point;
                }
            }

            if (nearest == null) return;

            Vector3 extractionPos = ((Component)nearest).transform.position;

            // 找到玩家附近（很近的）物品并传送到撤离点
            ValuableObject[] valuables = Object.FindObjectsOfType<ValuableObject>();
            if (valuables == null) return;

            int soldCount = 0;
            foreach (ValuableObject valuable in valuables)
            {
                if ((Object)(object)valuable == (Object)null) continue;
                Transform t = ((Component)valuable).transform;
                float dist = Vector3.Distance(playerPos, t.position);

                // 只传送玩家附近5米内的物品
                if (dist <= 5f)
                {
                    Vector3 sellPos = extractionPos + UnityEngine.Random.insideUnitSphere * 1f;
                    sellPos.y = extractionPos.y + 0.5f;
                    t.position = sellPos;
                    soldCount++;
                }
            }

            if (soldCount > 0)
            {
                Debug.Log((object)$"[自动卖出] 传送了 {soldCount} 个物品到撤离点");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError((object)$"[自动卖出] 失败: {ex.Message}");
        }
    }

    private static int GetItemValue(ValuableObject valuable)
    {
        try
        {
            FieldInfo dollarValueField = typeof(ValuableObject).GetField("dollarValueCurrent",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (dollarValueField != null)
            {
                return (int)(float)dollarValueField.GetValue(valuable);
            }

            // fallback
            FieldInfo valueField = typeof(ValuableObject).GetField("dollarValueOriginal",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (valueField != null)
            {
                return (int)(float)valueField.GetValue(valuable);
            }
        }
        catch { }
        return 0;
    }
}
