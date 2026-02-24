using System;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// 商店作弊: 免费购买 + 刷钱
/// </summary>
public static class ShopHack
{
    public static bool freeShopEnabled = false;
    public static int moneySpawnAmount = 45000;

    /// <summary>
    /// 在指定位置生成金钱物品
    /// </summary>
    public static void SpawnMoney(Vector3 position, int value)
    {
        try
        {
            // 使用已有的 ItemSpawner.SpawnMoney（如果可用）
            var spawnerType = typeof(Hax2).Assembly.GetType("r.e.p.o_cheat.ItemSpawner");
            if (spawnerType != null)
            {
                var method = spawnerType.GetMethod("SpawnMoney", BindingFlags.Public | BindingFlags.Static);
                if (method != null)
                {
                    method.Invoke(null, new object[] { position, value });
                    return;
                }
            }
            // Fallback: 直接使用 AssetManager + NetworkInstantiate
            DirectSpawnMoney(position, value);
        }
        catch (Exception ex) { Debug.LogWarning("[ShopHack] SpawnMoney error: " + ex.Message); }
    }

    private static void DirectSpawnMoney(Vector3 position, int value)
    {
        try
        {
            if (!SemiFunc.IsMultiplayer()) return;
            var amInstance = AssetManager.instance;
            if (amInstance == null) return;
            var surplusField = amInstance.GetType().GetField("surplusValuableSmall",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (surplusField == null) return;
            GameObject prefab = surplusField.GetValue(amInstance) as GameObject;
            if (prefab == null) return;

            var instantiateMethod = typeof(PhotonNetwork).GetMethod("NetworkInstantiate",
                BindingFlags.Static | BindingFlags.NonPublic, null,
                new Type[3] { typeof(InstantiateParameters), typeof(bool), typeof(bool) }, null);
            var levelPrefixField = typeof(PhotonNetwork).GetField("currentLevelPrefix",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (instantiateMethod == null || levelPrefixField == null) return;

            object levelPrefix = levelPrefixField.GetValue(null);
            object[] data = new object[1] { value };
            InstantiateParameters param = new InstantiateParameters(
                "Valuables/" + prefab.name, position, Quaternion.identity, (byte)0, data,
                (byte)levelPrefix, null, PhotonNetwork.LocalPlayer, PhotonNetwork.ServerTimestamp);
            instantiateMethod.Invoke(null, new object[3] { param, true, false });
        }
        catch (Exception ex) { Debug.LogWarning("[ShopHack] DirectSpawnMoney error: " + ex.Message); }
    }

    /// <summary>
    /// 在本地玩家位置生成金钱
    /// </summary>
    public static void SpawnMoneyAtPlayer(int value = 45000)
    {
        GameObject player = DebugCheats.GetLocalPlayer();
        if (player == null) return;
        SpawnMoney(player.transform.position + Vector3.up * 1.5f, value);
    }

    /// <summary>
    /// 尝试设置商店物品免费
    /// 搜索游戏中的 ShopItem 组件并将价格设为0
    /// </summary>
    public static int SetAllShopItemsFree()
    {
        int count = 0;
        try
        {
            // 搜索所有可能的商店物品类型
            string[] shopTypeNames = { "ShopItem", "ItemShop", "ShopManager", "Shop" };
            foreach (string typeName in shopTypeNames)
            {
                Type shopType = Type.GetType(typeName + ", Assembly-CSharp");
                if (shopType == null) continue;

                var items = UnityEngine.Object.FindObjectsOfType(shopType);
                foreach (var item in items)
                {
                    // 尝试常见的价格字段名
                    string[] priceFields = { "price", "cost", "itemPrice", "shopPrice", "dollarCost" };
                    foreach (string pf in priceFields)
                    {
                        FieldInfo field = shopType.GetField(pf, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (field != null && (field.FieldType == typeof(int) || field.FieldType == typeof(float)))
                        {
                            if (field.FieldType == typeof(int)) field.SetValue(item, 0);
                            else field.SetValue(item, 0f);
                            count++;
                        }
                    }
                }
            }
        }
        catch (Exception ex) { Debug.LogWarning("[ShopHack] SetFree error: " + ex.Message); }
        return count;
    }

    /// <summary>
    /// 尝试直接修改玩家/团队金币
    /// </summary>
    public static bool AddMoney(int amount)
    {
        try
        {
            // 尝试通过 SemiFunc 或 RunManager 找到金币字段
            string[] containerTypes = { "SemiFunc", "RunManager", "StatsManager", "GameDirector" };
            string[] moneyFields = { "money", "currency", "gold", "teamMoney", "playerMoney", "totalMoney", "haul" };

            foreach (string ct in containerTypes)
            {
                Type type = Type.GetType(ct + ", Assembly-CSharp");
                if (type == null) continue;

                foreach (string mf in moneyFields)
                {
                    FieldInfo field = type.GetField(mf, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field == null) continue;

                    object target = null;
                    if (!field.IsStatic)
                    {
                        // 尝试获取实例
                        var instanceField = type.GetField("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        if (instanceField != null) target = instanceField.GetValue(null);
                        if (target == null) continue;
                    }

                    if (field.FieldType == typeof(int))
                    {
                        int current = (int)field.GetValue(target);
                        field.SetValue(target, current + amount);
                        Debug.Log($"[ShopHack] {ct}.{mf}: {current} → {current + amount}");
                        return true;
                    }
                    else if (field.FieldType == typeof(float))
                    {
                        float current = (float)field.GetValue(target);
                        field.SetValue(target, current + (float)amount);
                        Debug.Log($"[ShopHack] {ct}.{mf}: {current} → {current + amount}");
                        return true;
                    }
                }
            }
        }
        catch (Exception ex) { Debug.LogWarning("[ShopHack] AddMoney error: " + ex.Message); }
        return false;
    }
}
