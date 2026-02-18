using System;
using System.Reflection;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// 自动闪避 - 检测敌人接近时自动触发翻滚
/// </summary>
public static class AutoDodge
{
    public static bool isAutoDodgeEnabled = false;
    public static float dodgeDistance = 8f;        // 触发闪避的距离
    public static float dodgeCooldown = 3f;        // 闪避冷却（秒）

    private static float lastDodgeTime = 0f;

    /// <summary>
    /// 在 Update 中调用
    /// </summary>
    public static void UpdateAutoDodge()
    {
        if (!isAutoDodgeEnabled) return;
        if (Time.time - lastDodgeTime < dodgeCooldown) return;

        try
        {
            GameObject localPlayer = DebugCheats.GetLocalPlayer();
            if ((Object)(object)localPlayer == (Object)null) return;

            Vector3 playerPos = localPlayer.transform.position;

            // 检测附近敌人
            Enemy[] enemies = Object.FindObjectsOfType<Enemy>();
            if (enemies == null || enemies.Length == 0) return;

            Enemy closestEnemy = null;
            float closestDist = dodgeDistance;

            foreach (Enemy enemy in enemies)
            {
                if ((Object)(object)enemy == (Object)null) continue;
                float dist = Vector3.Distance(playerPos, ((Component)enemy).transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestEnemy = enemy;
                }
            }

            if (closestEnemy != null)
            {
                PerformDodge(localPlayer, closestEnemy);
                lastDodgeTime = Time.time;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError((object)$"[自动闪避] 错误: {ex.Message}");
        }
    }

    private static void PerformDodge(GameObject player, Enemy enemy)
    {
        try
        {
            // 计算远离敌人的方向
            Vector3 awayDir = (player.transform.position - ((Component)enemy).transform.position).normalized;

            // 尝试通过反射触发玩家的翻滚/tumble
            Type pcType = Type.GetType("PlayerController, Assembly-CSharp");
            if (pcType != null)
            {
                object pcInstance = Object.FindObjectOfType(pcType);
                if (pcInstance != null)
                {
                    // 尝试调用 Tumble 方法
                    MethodInfo tumbleMethod = pcType.GetMethod("Tumble", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (tumbleMethod != null)
                    {
                        tumbleMethod.Invoke(pcInstance, null);
                        Debug.Log((object)$"[自动闪避] 触发翻滚! 敌人距离: {Vector3.Distance(player.transform.position, ((Component)enemy).transform.position):F1}m");
                        return;
                    }

                    // fallback: 直接推开玩家
                    FieldInfo rbField = pcType.GetField("rb", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (rbField != null)
                    {
                        object rbObj = rbField.GetValue(pcInstance);
                        Rigidbody rb = (Rigidbody)(rbObj is Rigidbody ? rbObj : null);
                        if ((Object)(object)rb != (Object)null)
                        {
                            rb.AddForce(awayDir * 15f + Vector3.up * 3f, ForceMode.Impulse);
                            Debug.Log((object)"[自动闪避] 施加推力远离敌人");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError((object)$"[自动闪避] 闪避失败: {ex.Message}");
        }
    }
}
