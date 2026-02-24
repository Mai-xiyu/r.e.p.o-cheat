using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// 表情动作 — 摇头/点头
/// 在 PhotonTransformView 序列化时短暂修改旋转，使他人看到你在做手势。
/// 与 GyroSpin 共用同一个 Harmony Patch 入口，通过 HeadGesture 的静态状态判断。
/// 由于 GyroSpin 已经 Patch 了 OnPhotonSerializeView，这里提供额外的旋转叠加逻辑，
/// 由 GyroSpin 的 Prefix/Postfix 调用 HeadGesture.GetGestureRotation() 来合并。
/// </summary>
public static class HeadGesture
{
    public static bool isShaking = false;
    public static bool isNodding = false;

    // 动画参数
    private static float _gestureStartTime = 0f;
    private static float _gestureDuration = 1.5f; // 持续 1.5 秒
    private static float _gestureAmplitude = 30f;  // 最大角度偏移
    private static float _gestureFrequency = 6f;   // 摆动频率 (Hz)

    /// <summary>
    /// 开始摇头动作
    /// </summary>
    public static void StartShake()
    {
        isShaking = true;
        isNodding = false;
        _gestureStartTime = Time.time;
        _gestureAmplitude = 30f;
        _gestureFrequency = 6f;
        _gestureDuration = 1.5f;
    }

    /// <summary>
    /// 开始点头动作
    /// </summary>
    public static void StartNod()
    {
        isNodding = true;
        isShaking = false;
        _gestureStartTime = Time.time;
        _gestureAmplitude = 15f;
        _gestureFrequency = 4f;
        _gestureDuration = 1.2f;
    }

    /// <summary>
    /// 获取当前手势的旋转偏移。如果没有活跃手势，返回 Quaternion.identity。
    /// </summary>
    public static Quaternion GetGestureRotation()
    {
        if (!isShaking && !isNodding) return Quaternion.identity;

        float elapsed = Time.time - _gestureStartTime;
        if (elapsed > _gestureDuration)
        {
            isShaking = false;
            isNodding = false;
            return Quaternion.identity;
        }

        // 衰减振幅 (越到后面越小)
        float decay = 1f - (elapsed / _gestureDuration);
        float angle = Mathf.Sin(elapsed * _gestureFrequency * Mathf.PI * 2f) * _gestureAmplitude * decay;

        if (isShaking)
        {
            return Quaternion.Euler(0f, angle, 0f); // Y 轴左右摇头
        }
        else // isNodding
        {
            return Quaternion.Euler(angle, 0f, 0f); // X 轴上下点头
        }
    }

    /// <summary>
    /// 是否有活跃的手势
    /// </summary>
    public static bool IsActive => isShaking || isNodding;
}
