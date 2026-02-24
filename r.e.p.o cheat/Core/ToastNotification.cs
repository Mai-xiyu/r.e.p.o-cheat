using System.Collections.Generic;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// Minecraft 风格右下角 Toast 通知系统
/// 功能开关时弹出圆角通知，带滑入/滑出动画
/// </summary>
public static class ToastNotification
{
    // ═══════════════════════════════════════════════════
    // 配置
    // ═══════════════════════════════════════════════════
    private const float TOAST_WIDTH = 280f;
    private const float TOAST_HEIGHT = 48f;
    private const float TOAST_SPACING = 6f;
    private const float MARGIN_RIGHT = 20f;
    private const float MARGIN_BOTTOM = 80f;
    private const int MAX_VISIBLE = 5;

    // 动画时间（秒）
    private const float ANIM_IN = 0.3f;
    private const float ANIM_STAY = 2.2f;
    private const float ANIM_OUT = 0.4f;
    private const float TOTAL_DURATION = ANIM_IN + ANIM_STAY + ANIM_OUT;

    // ═══════════════════════════════════════════════════
    // 数据
    // ═══════════════════════════════════════════════════
    private struct ToastItem
    {
        public string title;
        public string message;
        public float spawnTime;
        public Color accentColor;
    }

    private static readonly List<ToastItem> _toasts = new List<ToastItem>();

    // 预生成纹理
    private static Texture2D _bgTex;
    private static Texture2D _accentTex;
    private static GUIStyle _titleStyle;
    private static GUIStyle _msgStyle;
    private static bool _stylesInited;

    // ═══════════════════════════════════════════════════
    // 公共 API
    // ═══════════════════════════════════════════════════

    /// <summary>
    /// 显示一条通知
    /// </summary>
    public static void Show(string title, string message, Color? accent = null)
    {
        var item = new ToastItem
        {
            title = title ?? "",
            message = message ?? "",
            spawnTime = Time.unscaledTime,
            accentColor = accent ?? new Color(0.3f, 0.7f, 1f, 1f)
        };
        _toasts.Add(item);

        // 限制队列大小
        while (_toasts.Count > MAX_VISIBLE + 3)
            _toasts.RemoveAt(0);
    }

    /// <summary>
    /// 显示功能 Toggle 通知（自动设置颜色和图标）
    /// </summary>
    public static void ShowToggle(string featureName, bool enabled)
    {
        // 去除 label 中的 emoji 前缀，保留纯文字
        string cleanName = featureName;
        if (cleanName.Length > 2 && !char.IsLetterOrDigit(cleanName[0]))
        {
            int idx = cleanName.IndexOf(' ');
            if (idx > 0 && idx < 4)
                cleanName = cleanName.Substring(idx + 1);
        }

        string icon = enabled ? "✓" : "✗";
        Color color = enabled
            ? new Color(0.2f, 0.78f, 0.35f, 1f)  // 绿色
            : new Color(0.85f, 0.25f, 0.25f, 1f); // 红色

        Show(icon + " " + cleanName, enabled ? "已启用" : "已禁用", color);
    }

    /// <summary>
    /// 显示信息通知
    /// </summary>
    public static void ShowInfo(string message)
    {
        Show("ℹ", message, new Color(0.3f, 0.7f, 1f, 1f));
    }

    /// <summary>
    /// 显示警告通知
    /// </summary>
    public static void ShowWarning(string message)
    {
        Show("⚠", message, new Color(1f, 0.75f, 0.2f, 1f));
    }

    // ═══════════════════════════════════════════════════
    // 渲染（在 OnGUI 中调用）
    // ═══════════════════════════════════════════════════

    public static void DrawToasts()
    {
        if (_toasts.Count == 0) return;

        EnsureStyles();

        float now = Time.unscaledTime;

        // 清理过期的 toast
        _toasts.RemoveAll(t => now - t.spawnTime > TOTAL_DURATION);

        if (_toasts.Count == 0) return;

        // 从底部向上绘制（最新的在最下面）
        float baseX = Screen.width - TOAST_WIDTH - MARGIN_RIGHT;
        float baseY = Screen.height - MARGIN_BOTTOM;

        // 只取最近的 MAX_VISIBLE 个
        int startIdx = Mathf.Max(0, _toasts.Count - MAX_VISIBLE);
        int visibleCount = _toasts.Count - startIdx;

        Color savedColor = GUI.color;

        for (int i = 0; i < visibleCount; i++)
        {
            ToastItem toast = _toasts[startIdx + i];
            float age = now - toast.spawnTime;

            // 计算动画参数
            float alpha;
            float slideOffset; // 正值 = 向右偏移

            if (age < ANIM_IN)
            {
                // 滑入: 从右侧进入
                float t = age / ANIM_IN;
                t = EaseOutCubic(t);
                alpha = t;
                slideOffset = (1f - t) * (TOAST_WIDTH + MARGIN_RIGHT);
            }
            else if (age < ANIM_IN + ANIM_STAY)
            {
                // 停留
                alpha = 1f;
                slideOffset = 0f;
            }
            else
            {
                // 滑出: 向右退出
                float t = (age - ANIM_IN - ANIM_STAY) / ANIM_OUT;
                t = EaseInCubic(t);
                alpha = 1f - t;
                slideOffset = t * (TOAST_WIDTH + MARGIN_RIGHT);
            }

            // 从底部向上排列（索引 0 = 最上面的 toast）
            int posFromBottom = visibleCount - 1 - i;
            float y = baseY - (posFromBottom + 1) * (TOAST_HEIGHT + TOAST_SPACING);
            float x = baseX + slideOffset;

            DrawSingleToast(x, y, toast, alpha);
        }

        GUI.color = savedColor;
    }

    // ═══════════════════════════════════════════════════
    // 内部绘制
    // ═══════════════════════════════════════════════════

    private static void DrawSingleToast(float x, float y, ToastItem toast, float alpha)
    {
        if (alpha <= 0.01f) return;

        GUI.color = new Color(1f, 1f, 1f, alpha);

        // 背景
        Rect bgRect = new Rect(x, y, TOAST_WIDTH, TOAST_HEIGHT);
        GUI.DrawTexture(bgRect, _bgTex);

        // 左侧彩色竖条
        GUI.color = new Color(toast.accentColor.r, toast.accentColor.g, toast.accentColor.b, alpha);
        Rect accentRect = new Rect(x, y + 4f, 3f, TOAST_HEIGHT - 8f);
        GUI.DrawTexture(accentRect, _accentTex);

        // 标题文字
        GUI.color = new Color(1f, 1f, 1f, alpha);
        Rect titleRect = new Rect(x + 12f, y + 5f, TOAST_WIDTH - 20f, 22f);
        GUI.Label(titleRect, toast.title, _titleStyle);

        // 副标题文字
        GUI.color = new Color(0.7f, 0.7f, 0.7f, alpha * 0.85f);
        Rect msgRect = new Rect(x + 12f, y + 25f, TOAST_WIDTH - 20f, 18f);
        GUI.Label(msgRect, toast.message, _msgStyle);
    }

    // ═══════════════════════════════════════════════════
    // 样式 & 纹理初始化
    // ═══════════════════════════════════════════════════

    private static void EnsureStyles()
    {
        // 检查纹理是否被销毁（场景切换时可能发生）
        if (_bgTex != null && (Object)(object)_bgTex == (Object)null)
        {
            _bgTex = null;
            _stylesInited = false;
        }

        if (_stylesInited) return;
        _stylesInited = true;

        // 半透明深色背景（圆角）
        _bgTex = UIStyles.GenerateRoundedRect(
            (int)TOAST_WIDTH / 2, (int)TOAST_HEIGHT / 2,
            8, new Color(0.08f, 0.08f, 0.1f, 0.92f));

        // 白色小条纹理
        _accentTex = new Texture2D(1, 1);
        _accentTex.SetPixel(0, 0, Color.white);
        _accentTex.Apply();

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false,
            clipping = TextClipping.Clip
        };
        _titleStyle.normal.textColor = Color.white;
        _titleStyle.padding = new RectOffset(0, 0, 0, 0);
        _titleStyle.margin = new RectOffset(0, 0, 0, 0);

        _msgStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false,
            clipping = TextClipping.Clip
        };
        _msgStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        _msgStyle.padding = new RectOffset(0, 0, 0, 0);
        _msgStyle.margin = new RectOffset(0, 0, 0, 0);
    }

    // ═══════════════════════════════════════════════════
    // 缓动函数
    // ═══════════════════════════════════════════════════

    private static float EaseOutCubic(float t)
    {
        t -= 1f;
        return t * t * t + 1f;
    }

    private static float EaseInCubic(float t)
    {
        return t * t * t;
    }
}
