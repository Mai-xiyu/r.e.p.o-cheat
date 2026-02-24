using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// 陀螺旋转 — CS2 风格反爆头旋转
/// 本地视角完全正常，他人看到你在 Y 轴持续旋转。
/// 原理：在 PhotonTransformView 序列化写出瞬间替换旋转值，写完立即恢复。
/// </summary>
[HarmonyPatch(typeof(PhotonTransformView), "OnPhotonSerializeView")]
public static class GyroSpin
{
    public static bool isEnabled = false;
    public static float spinSpeed = 45f; // 度/秒

    [HarmonyPrefix]
    public static void Prefix(PhotonTransformView __instance, PhotonStream stream, ref Quaternion __state)
    {
        __state = Quaternion.identity;
        if (!stream.IsWriting) return;
        if (!isEnabled && !HeadGesture.IsActive) return;

        PhotonView pv = __instance.GetComponent<PhotonView>();
        if (pv == null || !pv.IsMine) return;

        // 只对 PlayerAvatar 生效，排除物品/敌人
        if (__instance.GetComponent<PlayerAvatar>() == null) return;

        // 保存真实旋转
        __state = __instance.transform.rotation;

        if (isEnabled)
        {
            // 陀螺旋转: 使用 Time.time 计算旋转角度
            float angle = (Time.time * spinSpeed) % 360f;
            __instance.transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
        else if (HeadGesture.IsActive)
        {
            // 表情动作: 在真实旋转基础上叠加手势偏移
            Quaternion gesture = HeadGesture.GetGestureRotation();
            __instance.transform.rotation = __state * gesture;
        }
    }

    [HarmonyPostfix]
    public static void Postfix(PhotonTransformView __instance, PhotonStream stream, Quaternion __state)
    {
        if (!stream.IsWriting) return;
        if (__state == Quaternion.identity) return;
        if (!isEnabled && !HeadGesture.IsActive) return;

        PhotonView pv = __instance.GetComponent<PhotonView>();
        if (pv == null || !pv.IsMine) return;
        if (__instance.GetComponent<PlayerAvatar>() == null) return;

        // 立即恢复真实旋转 — 本地玩家看不到任何变化
        __instance.transform.rotation = __state;
    }
}
