using UnityEngine;

namespace r.e.p.o_cheat
{
	public enum ThemeType
	{
		Classic,
		AppleDark
	}

	public class ThemeData
	{
		// Title
		public Color titleColor;

		// Tab
		public Color tabNormalBg;
		public Color tabNormalText;
		public Color tabHoverBg;
		public Color tabHoverText;
		public Color tabActiveBg;
		public Color tabActiveText;
		public Color tabSelectedBg;
		public Color tabSelectedText;
		public Color tabHighlightColor;

		// Section Header
		public Color sectionHeaderText;
		public Color sectionAccentColor;
		public Color sectionLineColor;

		// Box / Window
		public Color boxBg;
		public Color windowBg;

		// Button
		public Color buttonNormalBg;
		public Color buttonNormalText;
		public Color buttonHoverBg;
		public Color buttonHoverText;
		public Color buttonActiveBg;
		public Color buttonActiveText;

		// Label / Warning
		public Color labelText;
		public Color warningText;

		// TextField
		public Color textFieldBg;
		public Color textFieldFocusBg;
		public Color textFieldText;

		// Toggle
		public Color toggleTrackOn;
		public Color toggleTrackOff;
		public Color toggleKnob;

		// Slider thumb accent
		public Color sliderThumbNormal;
		public Color sliderThumbHover;
		public Color sliderThumbActive;

		// macOS extras
		public Color pillBg;           // Dynamic Island pill background
		public Color pillText;         // Dynamic Island pill text
		public Color pillDotColor;     // Status dots on pill
		public Color sidebarBg;        // Tab sidebar background
		public Color contentBg;        // Content area background
		public Color separatorColor;   // Subtle divider lines
		public Color scrollbarTrack;   // Scrollbar track
		public Color scrollbarThumb;   // Scrollbar thumb
		public Color subtitleText;     // Secondary text color

		public static ThemeData GetTheme(ThemeType type)
		{
			switch (type)
			{
				case ThemeType.AppleDark:
					return GetAppleDarkTheme();
				default:
					return GetClassicTheme();
			}
		}

		public static ThemeData GetClassicTheme()
		{
			return new ThemeData
			{
				titleColor = new Color(1f, 0.647f, 0f, 1f), // #FFA500
				tabNormalBg = new Color(0.157f, 0.157f, 0.157f, 1f),
				tabNormalText = new Color(0.85f, 0.85f, 0.85f),
				tabHoverBg = new Color(0.196f, 0.196f, 0.196f, 1f),
				tabHoverText = new Color(1f, 0.647f, 0f, 1f),
				tabActiveBg = new Color(0.078f, 0.078f, 0.078f, 1f),
				tabActiveText = Color.white,
				tabSelectedBg = new Color(0.078f, 0.078f, 0.118f, 1f),
				tabSelectedText = new Color(1f, 0.647f, 0f, 1f),
				tabHighlightColor = new Color(1f, 0.5f, 0f, 1f),
				sectionHeaderText = new Color(1f, 0.55f, 0f),
				sectionAccentColor = new Color(1f, 0.55f, 0f, 1f),
				sectionLineColor = new Color(1f, 0.55f, 0f, 0.2f),
				boxBg = new Color(0.098f, 0.11f, 0.137f, 0.86f),
				windowBg = new Color(0.059f, 0.071f, 0.098f, 0.92f),
				buttonNormalBg = new Color(0.176f, 0.176f, 0.216f, 1f),
				buttonNormalText = new Color(0.94f, 0.94f, 0.94f, 1f),
				buttonHoverBg = new Color(0.235f, 0.196f, 0.137f, 1f),
				buttonHoverText = new Color(1f, 0.647f, 0f, 1f),
				buttonActiveBg = new Color(0.353f, 0.235f, 0.118f, 1f),
				buttonActiveText = new Color(1f, 0.647f, 0f, 1f),
				labelText = Color.white,
				warningText = Color.yellow,
				textFieldBg = new Color(0.196f, 0.196f, 0.235f, 1f),
				textFieldFocusBg = new Color(0.235f, 0.235f, 0.314f, 1f),
				textFieldText = new Color(0.9f, 0.9f, 0.9f, 1f),
				toggleTrackOn = new Color(0.1f, 0.55f, 0.2f, 1f),
				toggleTrackOff = new Color(0.22f, 0.22f, 0.25f, 1f),
				toggleKnob = Color.white,
				sliderThumbNormal = new Color(0.353f, 0.353f, 0.47f, 1f),
				sliderThumbHover = new Color(0.43f, 0.43f, 0.55f, 1f),
				sliderThumbActive = new Color(0.51f, 0.51f, 0.627f, 1f),
				pillBg = new Color(0.06f, 0.06f, 0.07f, 0.97f),
				pillText = new Color(0.85f, 0.85f, 0.85f, 1f),
				pillDotColor = new Color(1f, 0.5f, 0f, 1f),
				sidebarBg = new Color(0.075f, 0.08f, 0.1f, 0.9f),
				contentBg = new Color(0.098f, 0.11f, 0.137f, 0.86f),
				separatorColor = new Color(1f, 1f, 1f, 0.06f),
				scrollbarTrack = new Color(0.1f, 0.1f, 0.1f, 0.5f),
				scrollbarThumb = new Color(0.4f, 0.4f, 0.4f, 0.6f),
				subtitleText = new Color(0.6f, 0.6f, 0.6f, 1f),
			};
		}

		public static ThemeData GetAppleDarkTheme()
		{
			// Authentic macOS Sonoma dark mode palette
			Color appleBlue     = new Color(0.04f, 0.52f, 1f, 1f);      // #0A84FF
			Color appleGreen    = new Color(0.188f, 0.82f, 0.345f, 1f); // #30D158
			Color appleYellow   = new Color(1f, 0.84f, 0f, 1f);         // #FFD600
			Color textPrimary   = new Color(0.98f, 0.98f, 0.98f, 1f);   // Almost white
			Color textSecondary = new Color(0.56f, 0.56f, 0.58f, 1f);   // #8E8E93
			Color textTertiary  = new Color(0.38f, 0.38f, 0.40f, 1f);

			// macOS background layers (darkest to lightest)
			Color bgBase    = new Color(0.07f, 0.07f, 0.075f, 0.96f);   // Window
			Color bgElevated = new Color(0.11f, 0.11f, 0.118f, 0.95f);  // Content area
			Color bgCard     = new Color(0.14f, 0.14f, 0.15f, 0.92f);
			Color bgHover    = new Color(0.18f, 0.18f, 0.19f, 1f);
			Color bgSidebar  = new Color(0.09f, 0.09f, 0.095f, 0.95f);

			return new ThemeData
			{
				titleColor = textPrimary,
				tabNormalBg = Color.clear,  // Transparent tab buttons - macOS style
				tabNormalText = textSecondary,
				tabHoverBg = new Color(1f, 1f, 1f, 0.06f),
				tabHoverText = textPrimary,
				tabActiveBg = new Color(1f, 1f, 1f, 0.04f),
				tabActiveText = textPrimary,
				tabSelectedBg = new Color(appleBlue.r, appleBlue.g, appleBlue.b, 0.18f),
				tabSelectedText = appleBlue,
				tabHighlightColor = appleBlue,
				sectionHeaderText = textSecondary,
				sectionAccentColor = appleBlue,
				sectionLineColor = new Color(1f, 1f, 1f, 0.06f),
				boxBg = bgElevated,
				windowBg = bgBase,
				buttonNormalBg = new Color(0.18f, 0.18f, 0.2f, 1f),
				buttonNormalText = textPrimary,
				buttonHoverBg = bgHover,
				buttonHoverText = textPrimary,
				buttonActiveBg = appleBlue,
				buttonActiveText = Color.white,
				labelText = textPrimary,
				warningText = appleYellow,
				textFieldBg = new Color(0.12f, 0.12f, 0.13f, 1f),
				textFieldFocusBg = new Color(0.15f, 0.15f, 0.17f, 1f),
				textFieldText = textPrimary,
				toggleTrackOn = appleGreen,
				toggleTrackOff = new Color(0.24f, 0.24f, 0.26f, 1f),
				toggleKnob = Color.white,
				sliderThumbNormal = new Color(0.55f, 0.55f, 0.58f, 1f),
				sliderThumbHover = new Color(0.65f, 0.65f, 0.68f, 1f),
				sliderThumbActive = appleBlue,
				pillBg = new Color(0.05f, 0.05f, 0.055f, 0.98f),
				pillText = textPrimary,
				pillDotColor = appleBlue,
				sidebarBg = bgSidebar,
				contentBg = bgElevated,
				separatorColor = new Color(1f, 1f, 1f, 0.06f),
				scrollbarTrack = Color.clear,
				scrollbarThumb = new Color(1f, 1f, 1f, 0.12f),
				subtitleText = textTertiary,
			};
		}
	}
}
