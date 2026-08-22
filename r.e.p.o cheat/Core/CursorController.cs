using System;
using UnityEngine;

public static class CursorController
{
	private static bool _cheatMenuOpen;

	public static bool cheatMenuOpen
	{
		get => _cheatMenuOpen;
		set
		{
			if (value && !_cheatMenuOpen)
			{
				lastLockState = Cursor.lockState;
				lastCursorVisible = Cursor.visible;
				hasCapturedGameState = true;
			}
			_cheatMenuOpen = value;
		}
	}

	private static CursorLockMode lastLockState;
	private static bool lastCursorVisible;
	private static bool hasCapturedGameState;

	public static void UpdateCursorState()
	{
		try
		{
			if (cheatMenuOpen)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
			else if (hasCapturedGameState)
			{
				Cursor.lockState = lastLockState;
				Cursor.visible = lastCursorVisible;
				hasCapturedGameState = false;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("CursorController.UpdateCursorState error: " + ex));
		}
	}

	public static void RestoreGameCursor()
	{
		cheatMenuOpen = false;
		UpdateCursorState();
	}
}
