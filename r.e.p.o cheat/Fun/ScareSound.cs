using System;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// 随机恐吓音效 — 触发游戏场景中已有的恐怖音效 RPC
/// 通过查找场景中的音效触发器/怪物，调用它们的音效 RPC
/// </summary>
public static class ScareSound
{
    public static string statusMessage = "";

    /// <summary>
    /// 尝试触发场景中的恐怖音效
    /// </summary>
    public static void TriggerRandomScare()
    {
        try
        {
            int triggered = 0;

            // 方案 1: 触发怪物的声音 RPC
            var enemies = UnityEngine.Object.FindObjectsOfType<EnemyParent>();
            if (enemies != null && enemies.Length > 0)
            {
                int idx = UnityEngine.Random.Range(0, enemies.Length);
                var enemy = enemies[idx];
                var pv = ((Component)enemy).GetComponentInChildren<PhotonView>();
                if (pv != null)
                {
                    // 尝试常见的音效 RPC
                    try { pv.RPC("MakeNoiseRPC", RpcTarget.All, new object[] { }); triggered++; } catch { }
                    if (triggered == 0)
                    {
                        try { pv.RPC("PlaySound", RpcTarget.All, new object[] { }); triggered++; } catch { }
                    }
                }
            }

            // 方案 2: 触发场景中的门/陷阱音效
            if (triggered == 0)
            {
                // 查找场景中的门对象 (通过名称匹配)
                var allPVs = UnityEngine.Object.FindObjectsOfType<PhotonView>();
                foreach (var doorPV in allPVs)
                {
                    if (doorPV != null && ((Component)doorPV).gameObject.name.ToLower().Contains("door"))
                    {
                        try { doorPV.RPC("DoorOpenRPC", RpcTarget.All, new object[] { }); triggered++; break; } catch { }
                    }
                }
            }

            // 方案 3: 播放本地恐怖音效 (备选)
            if (triggered == 0)
            {
                // 查找场景中所有 AudioSource 并播放一个低沉的音效
                var sources = UnityEngine.Object.FindObjectsOfType<AudioSource>();
                foreach (var src in sources)
                {
                    if (src.clip != null && src.clip.name != null &&
                        (src.clip.name.ToLower().Contains("scare") ||
                         src.clip.name.ToLower().Contains("horror") ||
                         src.clip.name.ToLower().Contains("monster") ||
                         src.clip.name.ToLower().Contains("scream") ||
                         src.clip.name.ToLower().Contains("growl") ||
                         src.clip.name.ToLower().Contains("roar")))
                    {
                        src.Play();
                        triggered++;
                        break;
                    }
                }
            }

            statusMessage = triggered > 0 ? "恐吓成功!" : "未找到可用音效";
        }
        catch (Exception ex)
        {
            statusMessage = "发生错误: " + ex.Message;
            Debug.LogWarning("[ScareSound] " + ex.Message);
        }
    }
}
