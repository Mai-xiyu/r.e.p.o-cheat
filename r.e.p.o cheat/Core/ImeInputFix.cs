using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// Game text fields (MenuTextInput, chat, password, debug console) read Input.inputString
/// and never turn IME on, so Chinese/Japanese/Korean cannot be composed. IMGUI Auto mode
/// only helps the cheat menu. This keeps IME on while any of those fields is active,
/// shows the in-progress composition, and gives IMGUI a CJK OS font.
/// </summary>
public class ImeInputFix : MonoBehaviour
{
	private static readonly BindingFlags InstAll = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly string[] OsFontNames =
	{
		"Microsoft YaHei UI",
		"Microsoft YaHei",
		"微软雅黑",
		"Noto Sans SC",
		"Source Han Sans SC",
		"PingFang SC",
		"SimHei",
		"SimSun",
		"Segoe UI"
	};

	private static Font _imguiFont;

	private static bool _wantIme;

	private static Vector2 _cursorGui;

	private static bool _hasCursor;

	public static void Request()
	{
		_wantIme = true;
	}

	public static void RequestFromTransform(Transform target)
	{
		_wantIme = true;
		if (target == null)
		{
			return;
		}
		Vector3 screen = RectTransformUtility.WorldToScreenPoint(null, target.position);
		_cursorGui = new Vector2(screen.x, Screen.height - screen.y);
		_hasCursor = true;
	}

	public static void ApplyCjkFont(GUIStyle style)
	{
		if (style == null)
		{
			return;
		}
		Font font = GetImguiFont();
		if (font != null)
		{
			style.font = font;
		}
	}

	public static Font GetImguiFont()
	{
		if (_imguiFont != null)
		{
			return _imguiFont;
		}
		try
		{
			_imguiFont = Font.CreateDynamicFontFromOSFont(OsFontNames, 14);
		}
		catch
		{
			_imguiFont = null;
		}
		return _imguiFont;
	}

	internal static string GetInstanceString(object instance, string field)
	{
		if (instance == null)
		{
			return "";
		}
		return instance.GetType().GetField(field, InstAll)?.GetValue(instance) as string ?? "";
	}

	internal static void ShowComposition(TMP_Text tmp, string committed)
	{
		if (tmp == null)
		{
			return;
		}
		string composition = Input.compositionString;
		if (string.IsNullOrEmpty(composition))
		{
			return;
		}
		tmp.text = (committed ?? "") + composition;
	}

	private void LateUpdate()
	{
		if (_wantIme || Hax2.showMenu)
		{
			Input.imeCompositionMode = IMECompositionMode.On;
		}
	}

	private void OnGUI()
	{
		if (Hax2.showMenu)
		{
			_wantIme = true;
		}
		if (_wantIme)
		{
			Input.imeCompositionMode = IMECompositionMode.On;
			if (_hasCursor)
			{
				Input.compositionCursorPos = _cursorGui;
			}
		}
		else if (Input.imeCompositionMode != IMECompositionMode.Auto)
		{
			Input.imeCompositionMode = IMECompositionMode.Off;
		}
		if (Event.current != null && Event.current.type == EventType.Repaint)
		{
			_wantIme = false;
			_hasCursor = false;
		}
	}

	private void OnDestroy()
	{
		try
		{
			Input.imeCompositionMode = IMECompositionMode.Auto;
		}
		catch
		{
		}
	}
}

[HarmonyPatch(typeof(MenuManager), "TextInputActive")]
public static class ImeMenuTextInputActivePatch
{
	private static void Postfix()
	{
		ImeInputFix.Request();
	}
}

[HarmonyPatch(typeof(MenuTextInput), "InputTextSet")]
public static class ImeMenuTextInputSetPatch
{
	private static void Postfix(MenuTextInput __instance)
	{
		ImeInputFix.RequestFromTransform(__instance.textCursor != null ? __instance.textCursor.transform : __instance.transform);
		ImeInputFix.ShowComposition(__instance.textMain, ImeInputFix.GetInstanceString(__instance, "textCurrent"));
	}
}

[HarmonyPatch(typeof(MenuPagePassword), "PasswordTextSet")]
public static class ImePasswordTextSetPatch
{
	private static void Postfix(MenuPagePassword __instance)
	{
		ImeInputFix.RequestFromTransform(__instance.passwordCursor != null ? __instance.passwordCursor.transform : __instance.transform);
		string password = ImeInputFix.GetInstanceString(__instance, "password");
		object showingObj = typeof(MenuPagePassword).GetField("showing", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(__instance);
		bool showing = showingObj is bool flag && flag;
		string visible = showing ? password : new string('*', password.Length);
		ImeInputFix.ShowComposition(__instance.passwordText, visible);
	}
}

[HarmonyPatch(typeof(ChatManager), "StateActive")]
public static class ImeChatStateActivePatch
{
	private static void Prefix()
	{
		ImeInputFix.Request();
	}

	private static void Postfix(ChatManager __instance)
	{
		if (__instance.chatText != null)
		{
			ImeInputFix.RequestFromTransform(__instance.chatText.transform);
		}
		ImeInputFix.ShowComposition(__instance.chatText, ImeInputFix.GetInstanceString(__instance, "chatMessage"));
	}
}

[HarmonyPatch(typeof(DebugConsoleUI), "Update")]
public static class ImeDebugConsolePatch
{
	private static void Prefix(DebugConsoleUI __instance)
	{
		object active = typeof(DebugConsoleUI).GetField("chatActive", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(__instance);
		if (active is bool flag && flag)
		{
			ImeInputFix.Request();
		}
	}
}

[HarmonyPatch(typeof(SemiFunc), "InputDown")]
public static class ImeBlockConfirmWhileComposingPatch
{
	private static bool Prefix(InputKey key, ref bool __result)
	{
		if (string.IsNullOrEmpty(Input.compositionString))
		{
			return true;
		}
		if (key == InputKey.Confirm || key == InputKey.Back || key == InputKey.Chat || key == InputKey.ChatDelete || key == InputKey.Menu)
		{
			__result = false;
			return false;
		}
		return true;
	}
}
