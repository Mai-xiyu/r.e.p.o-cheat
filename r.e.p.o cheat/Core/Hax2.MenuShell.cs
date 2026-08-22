using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// Navigation and window layout for the IMGUI menu. Feature implementations stay in
/// Hax2.cs; this partial owns page metadata, responsive sizing and reusable shell chrome.
/// </summary>
public partial class Hax2
{
	private sealed class MenuPageDefinition
	{
		public readonly int Tab;
		public readonly string GroupKey;
		public readonly string Icon;
		public readonly string TitleKey;
		public readonly string DescriptionKey;

		public MenuPageDefinition(int tab, string groupKey, string icon, string titleKey, string descriptionKey)
		{
			Tab = tab;
			GroupKey = groupKey;
			Icon = icon;
			TitleKey = titleKey;
			DescriptionKey = descriptionKey;
		}
	}

	private static readonly string[] NavigationGroups =
	{
		"nav.group.play",
		"nav.group.world",
		"nav.group.online",
		"nav.group.system"
	};

	private static readonly MenuPageDefinition[] MenuPages =
	{
		new MenuPageDefinition(0, "nav.group.play", "●", "tab.self", "nav.desc.self"),
		new MenuPageDefinition(1, "nav.group.play", "◌", "tab.visuals", "nav.desc.visuals"),
		new MenuPageDefinition(2, "nav.group.play", "◆", "tab.combat", "nav.desc.combat"),
		new MenuPageDefinition(5, "nav.group.play", "↗", "tab.teleport", "nav.desc.teleport"),
		new MenuPageDefinition(3, "nav.group.world", "□", "tab.items", "nav.desc.items"),
		new MenuPageDefinition(4, "nav.group.world", "◇", "tab.enemies", "nav.desc.enemies"),
		new MenuPageDefinition(6, "nav.group.world", "✦", "tab.fun", "nav.desc.fun"),
		new MenuPageDefinition(7, "nav.group.online", "◎", "tab.trolling", "nav.desc.room"),
		new MenuPageDefinition(11, "nav.group.online", "≋", "tab.server", "nav.desc.server"),
		new MenuPageDefinition(8, "nav.group.online", "▣", "tab.admin", "nav.desc.admin"),
		new MenuPageDefinition(9, "nav.group.system", "⌘", "tab.hotkeys", "nav.desc.hotkeys"),
		new MenuPageDefinition(10, "nav.group.system", "◫", "tab.config", "nav.desc.config"),
		new MenuPageDefinition(12, "nav.group.system", "⚙", "tab.menu", "nav.desc.menu")
	};

	private const int ServerTabIndex = 11;
	private const float WidePageWidth = 1060f;
	private const float WidePageHeight = 710f;
	private const float SidebarWidth = 172f;
	private const float ShellPadding = 10f;
	private const float ShellHeaderHeight = 58f;
	private const float ShellFooterHeight = 28f;

	private readonly Dictionary<int, Vector2> _pageScrollPositions = new Dictionary<int, Vector2>();
	private Vector2 _navigationScroll;
	private int _lastMenuPageForSize = -1;
	private object _menuShellThemeOwner;
	private GUIStyle _menuShellHeaderTitleStyle;
	private GUIStyle _menuShellGroupStyle;
	private GUIStyle _menuShellPageTitleStyle;
	private GUIStyle _menuShellDescriptionStyle;
	private GUIStyle _menuShellMetaStyle;
	private GUIStyle _menuShellCloseStyle;
	private GUIStyle _menuShellFooterStyle;

	private bool DrawRefreshedMenuWindow(int windowID)
	{
		EnsureMenuShellStyles();
		if (_animProgress < 0.82f)
		{
			DrawMenuTransitionState();
			return true;
		}

		float contentAlpha = Mathf.Clamp01((_animProgress - 0.82f) / 0.18f);
		contentAlpha *= contentAlpha;
		Color savedColor = GUI.color;
		GUI.color = new Color(savedColor.r, savedColor.g, savedColor.b, savedColor.a * contentAlpha);

		DrawMenuShellHeader();
		DrawMenuShellBody();
		GUI.color = savedColor;
		return true;
	}

	private void DrawMenuTransitionState()
	{
		float phase = Mathf.Clamp01(_animProgress / 0.82f);
		GUIStyle title = _menuShellHeaderTitleStyle;
		title.fontSize = Mathf.RoundToInt(Mathf.Lerp(14f, 19f, phase));
		GUI.Label(new Rect(0f, 0f, menuRect.width, Mathf.Min(PILL_HEIGHT, menuRect.height * 0.6f)),
			L.T("menu.title"), title);

		if (menuRect.height > PILL_HEIGHT + 8f)
		{
			GUI.Label(new Rect(0f, PILL_HEIGHT * 0.5f, menuRect.width, 20f),
				L.T("nav.active_count", GetActiveFeatureCount()), _menuShellMetaStyle);
		}

		GUI.DragWindow(new Rect(0f, 0f, menuRect.width, menuRect.height));
	}

	private void DrawMenuShellHeader()
	{
		float titleWidth = Mathf.Clamp(menuRect.width * 0.34f, 180f, 280f);
		float sessionWidth = Mathf.Clamp(menuRect.width * 0.30f, 150f, 300f);
		float activeWidth = Mathf.Clamp(menuRect.width * 0.12f, 64f, 96f);
		GUILayout.BeginHorizontal(GUILayout.Height(ShellHeaderHeight));
		GUILayout.Space(16f);
		GUILayout.BeginVertical();
		GUILayout.Space(7f);
		GUILayout.Label(L.T("menu.title"), _menuShellHeaderTitleStyle, GUILayout.Width(titleWidth));
		GUILayout.Label(L.T(NativeGameApi.RoleKey()) + "  ·  " + SessionStatusLabel(), _menuShellMetaStyle,
			GUILayout.Width(sessionWidth));
		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		GUILayout.Label(L.T("nav.active_count", GetActiveFeatureCount()), _menuShellMetaStyle, GUILayout.Width(activeWidth));
		if (GUILayout.Button("×", _menuShellCloseStyle, GUILayout.Width(32f), GUILayout.Height(28f)))
		{
			_isExpanding = false;
		}
		GUILayout.Space(10f);
		GUILayout.EndHorizontal();

		// Reserve only the empty top strip for dragging so it never steals sidebar or
		// content interactions.
		GUI.DragWindow(new Rect(0f, 0f, Mathf.Max(0f, menuRect.width - 48f), 8f));
	}

	private void DrawMenuShellBody()
	{
		float bodyHeight = Mathf.Max(180f, menuRect.height - ShellHeaderHeight - ShellFooterHeight - ShellPadding * 2f);
		GUILayout.BeginHorizontal(GUILayout.Height(bodyHeight));
		GUILayout.Space(ShellPadding);
		DrawMenuNavigation(bodyHeight);
		GUILayout.Space(8f);

		GUILayout.BeginVertical(boxStyle, GUILayout.ExpandWidth(true), GUILayout.Height(bodyHeight));
		MenuPageDefinition page = CurrentMenuPage();
		DrawMenuPageHeading(page);

		Vector2 pageScroll = GetPageScroll(page.Tab);
		pageScroll = GUILayout.BeginScrollView(pageScroll, GUILayout.Height(Mathf.Max(100f, bodyHeight - 86f)));
		DrawCurrentMenuPage();
		GUILayout.EndScrollView();
		SetPageScroll(page.Tab, pageScroll);
		GUILayout.EndVertical();
		GUILayout.Space(ShellPadding);
		GUILayout.EndHorizontal();

		DrawMenuShellFooter();
	}

	private void DrawMenuNavigation(float bodyHeight)
	{
		GUILayout.BeginVertical(boxStyle, GUILayout.Width(SidebarWidth), GUILayout.Height(bodyHeight));
		_navigationScroll = GUILayout.BeginScrollView(_navigationScroll, GUILayout.Height(Mathf.Max(100f, bodyHeight - 18f)));
		for (int groupIndex = 0; groupIndex < NavigationGroups.Length; groupIndex++)
		{
			string groupKey = NavigationGroups[groupIndex];
			GUILayout.Space(groupIndex == 0 ? 2f : 8f);
			GUILayout.Label(L.T(groupKey), _menuShellGroupStyle, GUILayout.Height(16f));
			for (int pageIndex = 0; pageIndex < MenuPages.Length; pageIndex++)
			{
				MenuPageDefinition page = MenuPages[pageIndex];
				if (page.GroupKey != groupKey)
				{
					continue;
				}
				DrawNavigationPageButton(page);
			}
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}

	private void DrawNavigationPageButton(MenuPageDefinition page)
	{
		bool selected = currentTab == page.Tab;
		GUIStyle style = selected ? tabSelectedStyle : tabStyle;
		if (GUILayout.Button(page.Icon + "  " + L.T(page.TitleKey), style, GUILayout.Height(29f)))
		{
			SelectMenuPage(page.Tab);
		}
		if (selected)
		{
			Rect row = GUILayoutUtility.GetLastRect();
			if (tabHighlightTex == null)
			{
				tabHighlightTex = MakeSolidBackground(activeTheme != null ? activeTheme.tabHighlightColor : Color.white);
			}
			GUI.DrawTexture(new Rect(row.x + 1f, row.y + 5f, 3f, Mathf.Max(2f, row.height - 10f)), tabHighlightTex);
		}
	}

	private void DrawMenuPageHeading(MenuPageDefinition page)
	{
		GUILayout.BeginVertical();
		GUILayout.Space(2f);
		GUILayout.Label(page.Icon + "  " + L.T(page.TitleKey), _menuShellPageTitleStyle);
		GUILayout.Label(L.T(page.DescriptionKey), _menuShellDescriptionStyle);
		if (!string.IsNullOrEmpty(NativeGameApi.LastStatus))
		{
			GUILayout.Label(NativeGameApi.LastStatus, warningStyle, GUILayout.Height(18f));
		}
		GUILayout.EndVertical();
	}

	private void DrawMenuShellFooter()
	{
		GUILayout.BeginHorizontal(GUILayout.Height(ShellFooterHeight));
		GUILayout.Space(16f);
		KeyCode toggleKey = hotkeyManager != null ? hotkeyManager.MenuToggleKey : KeyCode.Insert;
		GUILayout.Label(L.T("nav.footer_hint", toggleKey), _menuShellFooterStyle);
		GUILayout.FlexibleSpace();
		GUILayout.Label(SessionStatusLabel(), _menuShellFooterStyle, GUILayout.Width(150f));
		GUILayout.Space(16f);
		GUILayout.EndHorizontal();
	}

	private void DrawCurrentMenuPage()
	{
		switch (currentTab)
		{
			case 0: DrawSelfTab(); break;
			case 1: DrawVisualsTab(); break;
			case 2: DrawCombatTab(); break;
			case 3: DrawItemsTab(); break;
			case 4: DrawEnemiesTab(); break;
			case 5: DrawTeleportTab(); break;
			case 6: DrawFunTab(); break;
			case 7: DrawRoomTab(); break;
			case 8: DrawAdminTab(); break;
			case 9: DrawHotkeysTab(); break;
			case 10: DrawConfigTab(); break;
			case 11: DrawServersTab(); break;
			case 12: DrawMenuSettingsTab(); break;
			default: SelectMenuPage(0); break;
		}
	}

	private MenuPageDefinition CurrentMenuPage()
	{
		for (int i = 0; i < MenuPages.Length; i++)
		{
			if (MenuPages[i].Tab == currentTab)
			{
				return MenuPages[i];
			}
		}
		return MenuPages[0];
	}

	private void SelectMenuPage(int tab)
	{
		if (currentTab == tab)
		{
			return;
		}
		currentTab = tab;
		GUI.FocusControl(null);
	}

	private Vector2 GetPageScroll(int tab)
	{
		return _pageScrollPositions.TryGetValue(tab, out Vector2 scroll) ? scroll : Vector2.zero;
	}

	private void SetPageScroll(int tab, Vector2 scroll)
	{
		_pageScrollPositions[tab] = scroll;
	}

	private void ResetMenuLayout()
	{
		_menuSizeWasUserAdjusted = false;
		_expandedMenuSizeInitialized = false;
		_navigationScroll = Vector2.zero;
		_pageScrollPositions.Clear();
		_hasDragged = false;
		configstatus = L.T("menupage.layout_reset");
	}

	private Vector2 GetExpandedMenuSize()
	{
		bool wide = currentTab == ServerTabIndex;
		Vector2 maximum = GetMaximumExpandedMenuSize();
		float preferredWidth = wide ? WidePageWidth : MENU_WIDTH;
		float preferredHeight = wide ? WidePageHeight : MENU_HEIGHT;

		if (!_expandedMenuSizeInitialized || (_lastMenuPageForSize != currentTab && !_menuSizeWasUserAdjusted))
		{
			_expandedMenuSize = new Vector2(Mathf.Min(preferredWidth, maximum.x), Mathf.Min(preferredHeight, maximum.y));
			_expandedMenuSizeInitialized = true;
		}
		_lastMenuPageForSize = currentTab;
		_expandedMenuSize = ClampExpandedMenuSize(_expandedMenuSize);
		return _expandedMenuSize;
	}

	private Vector2 ClampExpandedMenuSize(Vector2 candidate)
	{
		Vector2 maximum = GetMaximumExpandedMenuSize();
		float minimumWidth = Mathf.Min(currentTab == ServerTabIndex ? 900f : 720f, maximum.x);
		float minimumHeight = Mathf.Min(currentTab == ServerTabIndex ? 580f : 500f, maximum.y);
		return new Vector2(
			Mathf.Clamp(candidate.x, minimumWidth, maximum.x),
			Mathf.Clamp(candidate.y, minimumHeight, maximum.y));
	}

	private static Vector2 GetMaximumExpandedMenuSize()
	{
		return new Vector2(
			Mathf.Max(420f, Screen.width - 32f),
			Mathf.Max(320f, Screen.height - 32f));
	}

	private void ClampMenuToScreen()
	{
		float visibleWidth = Mathf.Min(80f, menuRect.width);
		float visibleHeight = Mathf.Min(40f, menuRect.height);
		menuRect.x = Mathf.Clamp(menuRect.x, -menuRect.width + visibleWidth, Screen.width - visibleWidth);
		menuRect.y = Mathf.Clamp(menuRect.y, 0f, Screen.height - visibleHeight);
	}

	private string SessionStatusLabel()
	{
		if (!PhotonNetwork.InRoom)
		{
			return L.T("nav.session_offline");
		}
		return NativeGameApi.IsHost() ? L.T("nav.session_host") : L.T("nav.session_guest");
	}

	private int GetActiveFeatureCount()
	{
		int count = 0;
		if (godModeActive) count++;
		if (infiniteHealthActive) count++;
		if (stamineState) count++;
		if (unlimitedBatteryActive) count++;
		if (NoclipController.noclipActive) count++;
		if (DebugCheats.drawEspBool) count++;
		if (DebugCheats.drawItemEspBool) count++;
		if (DebugCheats.drawPlayerEspBool) count++;
		if (MidJoin.Enabled) count++;
		return count;
	}

	private void EnsureMenuShellStyles()
	{
		if (_menuShellThemeOwner == activeTheme && _menuShellHeaderTitleStyle != null)
		{
			return;
		}

		_menuShellThemeOwner = activeTheme;
		GUIStyle label = labelStyle ?? GUI.skin.label;
		GUIStyle button = buttonStyle ?? GUI.skin.button;
		GUIStyle section = sectionHeaderStyle ?? label;

		_menuShellHeaderTitleStyle = new GUIStyle(titleStyle ?? label)
		{
			alignment = TextAnchor.MiddleLeft,
			fontSize = 18,
			fontStyle = FontStyle.Bold,
			wordWrap = false
		};
		_menuShellGroupStyle = new GUIStyle(label)
		{
			fontSize = 10,
			fontStyle = FontStyle.Bold,
			alignment = TextAnchor.MiddleLeft,
			padding = new RectOffset(6, 2, 0, 0)
		};
		_menuShellGroupStyle.normal.textColor = activeTheme != null ? activeTheme.subtitleText : Color.gray;
		_menuShellPageTitleStyle = new GUIStyle(section)
		{
			fontSize = 17,
			fontStyle = FontStyle.Bold,
			alignment = TextAnchor.MiddleLeft
		};
		_menuShellDescriptionStyle = new GUIStyle(label)
		{
			fontSize = 11,
			wordWrap = true,
			alignment = TextAnchor.UpperLeft
		};
		_menuShellDescriptionStyle.normal.textColor = activeTheme != null ? activeTheme.subtitleText : new Color(0.7f, 0.7f, 0.7f);
		_menuShellMetaStyle = new GUIStyle(label)
		{
			fontSize = 10,
			alignment = TextAnchor.MiddleLeft,
			wordWrap = false
		};
		_menuShellMetaStyle.normal.textColor = activeTheme != null ? activeTheme.subtitleText : Color.gray;
		_menuShellCloseStyle = new GUIStyle(button)
		{
			fontSize = 18,
			fontStyle = FontStyle.Bold,
			alignment = TextAnchor.MiddleCenter,
			padding = new RectOffset(0, 0, 0, 2),
			margin = new RectOffset(0, 0, 0, 0)
		};
		_menuShellFooterStyle = new GUIStyle(_menuShellMetaStyle)
		{
			alignment = TextAnchor.MiddleLeft
		};
	}
}
