using System;
using System.Collections;
using HarmonyLib;
using UnityEngine;

public static class CursorController
{
	[HarmonyPatch(typeof(Cursor), "set_lockState")]
	public class SetLockStatePatch
	{
		private static void Prefix(ref CursorLockMode value)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			if (!currentlySettingCursor)
			{
				lastLockState = value;
				if (cheatMenuOpen)
				{
					value = (CursorLockMode)0;
				}
			}
		}
	}

	[HarmonyPatch(typeof(Cursor), "set_visible")]
	public class SetVisiblePatch
	{
		private static void Prefix(ref bool value)
		{
			if (!currentlySettingCursor)
			{
				lastCursorVisible = value;
				if (cheatMenuOpen)
				{
					value = true;
				}
			}
		}
	}

	public static bool cheatMenuOpen = false;

	public static bool overrideCursorSetting = false;

	private static bool currentlySettingCursor = false;

	private static CursorLockMode lastLockState = Cursor.lockState;

	private static Vector2 lastCursorPosition;

	private static bool lastCursorVisible = Cursor.visible;

	private static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

	public static void Init()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		new Harmony("com.mycompany.mycheat.cursorcontroller").PatchAll();
	}

	public static void UpdateCursorState()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Invalid comparison between Unknown and I4
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			currentlySettingCursor = true;
			if (cheatMenuOpen)
			{
				Cursor.lockState = (CursorLockMode)0;
				Cursor.visible = true;
				lastCursorPosition = (Vector2)Input.mousePosition;
			}
			else
			{
				if ((int)lastLockState == 1)
				{
					Cursor.lockState = (CursorLockMode)0;
				}
				Cursor.lockState = lastLockState;
				Cursor.visible = lastCursorVisible;
			}
			currentlySettingCursor = false;
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("CursorController.UpdateCursorState error: " + ex));
		}
	}

	public static IEnumerator UnlockCoroutine()
	{
		while (true)
		{
			yield return waitForEndOfFrame;
			UpdateCursorState();
		}
	}
}
