using UnityEngine;

namespace r.e.p.o_cheat
{
	/// <summary>
	/// macOS-style UI texture & style generator for OnGUI.
	/// All textures are generated once at runtime with anti-aliased edges.
	/// </summary>
	public static class UIStyles
	{
		// === Cached textures ===
		private static Texture2D _windowBg;
		private static Texture2D _pillBg;
		private static Texture2D _buttonBg;
		private static Texture2D _buttonHoverBg;
		private static Texture2D _tabBg;
		private static Texture2D _tabSelectedBg;
		private static Texture2D _toggleTrackOff;
		private static Texture2D _toggleTrackOn;
		private static Texture2D _toggleKnob;
		private static Texture2D _contentBoxBg;

		// === Cached styles ===
		private static GUIStyle _windowStyle;
		private static GUIStyle _pillStyle;

		// === Config ===
		private static int _windowRadius = 18;
		private static int _pillRadius = 22;
		private static int _buttonRadius = 8;
		private static int _toggleRadius = 11;

		public static bool IsInitialized { get; private set; }

		public static void Init(ThemeData theme)
		{
			if (theme == null) return;

			// Destroy old textures
			DestroyTex(ref _windowBg);
			DestroyTex(ref _pillBg);
			DestroyTex(ref _buttonBg);
			DestroyTex(ref _buttonHoverBg);
			DestroyTex(ref _tabBg);
			DestroyTex(ref _tabSelectedBg);
			DestroyTex(ref _toggleTrackOff);
			DestroyTex(ref _toggleTrackOn);
			DestroyTex(ref _toggleKnob);
			DestroyTex(ref _contentBoxBg);

			// Generate anti-aliased rounded textures
			_windowBg = GenerateRoundedRect(64, 64, _windowRadius, theme.windowBg);
			_pillBg = GenerateRoundedRect(64, 44, _pillRadius, new Color(0.06f, 0.06f, 0.07f, 0.97f));
			_buttonBg = GenerateRoundedRect(32, 32, _buttonRadius, theme.buttonNormalBg);
			_buttonHoverBg = GenerateRoundedRect(32, 32, _buttonRadius, theme.buttonHoverBg);
			_tabBg = GenerateRoundedRect(32, 32, 6, theme.tabNormalBg);
			_tabSelectedBg = GenerateRoundedRect(32, 32, 6, theme.tabSelectedBg);
			_toggleTrackOff = GenerateRoundedRect(46, 22, _toggleRadius, theme.toggleTrackOff);
			_toggleTrackOn = GenerateRoundedRect(46, 22, _toggleRadius, theme.toggleTrackOn);
			_toggleKnob = GenerateCircle(18, Color.white);
			_contentBoxBg = GenerateRoundedRect(32, 32, 10, theme.boxBg);

			// Window style (main menu)
			_windowStyle = MakeStyle(_windowBg, _windowRadius);

			// Pill style (collapsed Dynamic Island)
			_pillStyle = MakeStyle(_pillBg, _pillRadius);

			IsInitialized = true;
		}

		// === Public accessors ===
		public static GUIStyle GetWindowStyle() => _windowStyle ?? GUI.skin.window;
		public static GUIStyle GetPillStyle() => _pillStyle ?? GUI.skin.window;
		public static Texture2D ButtonBg => _buttonBg;
		public static Texture2D ButtonHoverBg => _buttonHoverBg;
		public static Texture2D TabBg => _tabBg;
		public static Texture2D TabSelectedBg => _tabSelectedBg;
		public static Texture2D ToggleTrackOff => _toggleTrackOff;
		public static Texture2D ToggleTrackOn => _toggleTrackOn;
		public static Texture2D ToggleKnob => _toggleKnob;
		public static Texture2D ContentBoxBg => _contentBoxBg;

		// === Texture generators ===

		/// <summary>Anti-aliased rounded rectangle with 9-slice support.</summary>
		public static Texture2D GenerateRoundedRect(int w, int h, int radius, Color color)
		{
			Texture2D tex = new Texture2D(w, h, TextureFormat.ARGB32, false);
			tex.filterMode = FilterMode.Bilinear;
			Color[] px = new Color[w * h];
			float r = radius;

			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					float dist = CornerDistance(x, y, w, h, r);
					float alpha = 0f;
					if (dist < r - 1.2f)
						alpha = 1f;
					else if (dist < r + 0.2f)
						alpha = Mathf.Clamp01(r - dist + 0.2f); // AA edge
					px[y * w + x] = new Color(color.r, color.g, color.b, color.a * alpha);
				}
			}
			tex.SetPixels(px);
			tex.Apply(false, false);
			return tex;
		}

		/// <summary>Anti-aliased filled circle texture.</summary>
		public static Texture2D GenerateCircle(int size, Color color)
		{
			Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
			tex.filterMode = FilterMode.Bilinear;
			Color[] px = new Color[size * size];
			float center = size / 2f;
			float r = center - 1f;

			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(center, center));
					float alpha = Mathf.Clamp01(r - dist + 0.8f);
					px[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
				}
			}
			tex.SetPixels(px);
			tex.Apply(false, false);
			return tex;
		}

		/// <summary>Draw a rounded-rect box manually (for tab sidebar, content area, etc.)</summary>
		public static void DrawRoundedBox(Rect rect, Texture2D tex, int radius)
		{
			GUIStyle s = new GUIStyle();
			s.normal.background = tex;
			s.border = new RectOffset(radius, radius, radius, radius);
			GUI.Box(rect, GUIContent.none, s);
		}

		// === Internal helpers ===

		private static float CornerDistance(int x, int y, int w, int h, float r)
		{
			// Only compute for corner zones
			float cx, cy;
			if (x < r && y < r) { cx = r; cy = r; }
			else if (x >= w - r && y < r) { cx = w - r - 1; cy = r; }
			else if (x < r && y >= h - r) { cx = r; cy = h - r - 1; }
			else if (x >= w - r && y >= h - r) { cx = w - r - 1; cy = h - r - 1; }
			else return 0f; // Inside non-corner zone = fully inside
			return Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
		}

		private static GUIStyle MakeStyle(Texture2D bg, int radius)
		{
			GUIStyle s = new GUIStyle(GUI.skin.window);
			s.normal.background = bg;
			s.onNormal.background = bg;
			s.hover.background = bg;
			s.onHover.background = bg;
			s.focused.background = bg;
			s.onFocused.background = bg;
			s.border = new RectOffset(radius, radius, radius, radius);
			s.margin = new RectOffset(0, 0, 0, 0);
			s.padding = new RectOffset(0, 0, 0, 0);
			s.overflow = new RectOffset(0, 0, 0, 0);
			s.contentOffset = Vector2.zero;
			s.normal.textColor = Color.white;
			return s;
		}

		private static void DestroyTex(ref Texture2D tex)
		{
			if (tex != null) { Object.Destroy(tex); tex = null; }
		}
	}
}
