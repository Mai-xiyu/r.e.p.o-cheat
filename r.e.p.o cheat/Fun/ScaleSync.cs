using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// 大小变换 — 修改本地角色的 localScale
/// 通过修改 PlayerAvatar 的 transform.localScale 来改变大小
/// </summary>
public static class ScaleSync
{
    public static bool isEnabled = false;
    public static float targetScale = 1f;

    private static Transform _cachedPlayerTransform;
    private static Vector3 _originalScale = Vector3.one;
    private static bool _hasOriginalScale = false;

    /// <summary>
    /// 每帧调用 (由 Hax2.Update 调用)
    /// </summary>
    public static void Update()
    {
        if (!isEnabled) return;

        if (_cachedPlayerTransform == null)
        {
            var localAvatar = SemiFunc.PlayerAvatarLocal();
            if (localAvatar != null)
            {
                _cachedPlayerTransform = ((Component)localAvatar).transform;
                if (!_hasOriginalScale)
                {
                    _originalScale = _cachedPlayerTransform.localScale;
                    _hasOriginalScale = true;
                }
            }
        }

        if (_cachedPlayerTransform != null)
        {
            Vector3 target = _originalScale * targetScale;
            _cachedPlayerTransform.localScale = Vector3.Lerp(
                _cachedPlayerTransform.localScale, target, Time.deltaTime * 5f);
        }
    }

    /// <summary>
    /// 禁用时恢复原始大小
    /// </summary>
    public static void Restore()
    {
        if (_cachedPlayerTransform != null && _hasOriginalScale)
        {
            _cachedPlayerTransform.localScale = _originalScale;
        }
        _cachedPlayerTransform = null;
    }
}
