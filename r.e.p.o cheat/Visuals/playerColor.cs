using System;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

internal class playerColor
{
	public static bool isRandomizing = false;

	private static float lastColorChangeTime = 0f;

	private static float changeInterval = 0.1f;

	private static Type colorControllerType;

	private static object colorControllerInstance;

	private static FieldInfo playerCosmeticsField;

	private static object playerCosmeticsInstance;

	private static FieldInfo colorsEquippedField;

	private static MethodInfo setupColorsMethod;

	private static PhotonView playerPhotonView;

	private static bool isInitialized = false;

	/// <summary>
	/// 保存的原始颜色索引，-1 表示尚未保存
	/// </summary>
	private static int savedOriginalColorIndex = -1;

	private static void Initialize()
	{
		if (isInitialized)
		{
			return;
		}
		colorControllerType = Type.GetType("PlayerAvatar, Assembly-CSharp");
		if (colorControllerType == null)
		{
			return;
		}
		colorControllerInstance = null;
		if (PhotonNetwork.IsConnected)
		{
			PhotonView[] array = Object.FindObjectsOfType<PhotonView>();
			foreach (PhotonView val in array)
			{
				if ((Object)(object)val != (Object)null && val.IsMine)
				{
					Component component = ((Component)val).gameObject.GetComponent(colorControllerType);
					if ((Object)(object)component != (Object)null)
					{
						colorControllerInstance = component;
						playerPhotonView = val;
						break;
					}
				}
			}
		}
		else
		{
			Object val2 = Object.FindObjectOfType(colorControllerType);
			if (val2 != (Object)null)
			{
				colorControllerInstance = val2;
				Object obj = ((val2 is MonoBehaviour) ? val2 : null);
				playerPhotonView = ((obj != null) ? ((Component)obj).GetComponent<PhotonView>() : null);
			}
			else
			{
				GameObject localPlayer = DebugCheats.GetLocalPlayer();
				if ((Object)(object)localPlayer != (Object)null)
				{
					Component component2 = localPlayer.GetComponent(colorControllerType);
					if ((Object)(object)component2 != (Object)null)
					{
						colorControllerInstance = component2;
						playerPhotonView = localPlayer.GetComponent<PhotonView>();
					}
				}
			}
		}
		if (colorControllerInstance != null)
		{
			playerCosmeticsField = colorControllerType.GetField("playerCosmetics", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (playerCosmeticsField != null)
			{
				playerCosmeticsInstance = playerCosmeticsField.GetValue(colorControllerInstance);
				if (playerCosmeticsInstance != null)
				{
					colorsEquippedField = playerCosmeticsInstance.GetType().GetField("colorsEquipped", BindingFlags.Instance | BindingFlags.NonPublic);
					setupColorsMethod = playerCosmeticsInstance.GetType().GetMethod("SetupColors", BindingFlags.Instance | BindingFlags.Public);
					if (setupColorsMethod != null)
					{
						isInitialized = true;
					}
				}
			}
		}
	}

	public static void colorRandomizer()
	{
		Initialize();
		if (isInitialized && Time.time - lastColorChangeTime > 5f)
		{
			Reset();
			Initialize();
		}
		if (!isInitialized || colorControllerInstance == null || setupColorsMethod == null || !isRandomizing || !(Time.time - lastColorChangeTime >= changeInterval))
		{
			return;
		}
		if (PhotonNetwork.IsConnected && ((Object)(object)playerPhotonView == (Object)null || !IsPhotonViewValid(playerPhotonView)))
		{
			Reset();
			Initialize();
			return;
		}
		// 首次变色前保存原始颜色索引
		if (savedOriginalColorIndex == -1)
		{
			try
			{
				// 从 PlayerCosmetics.colorsEquipped 读取主颜色（索引 5）
				if (colorsEquippedField != null && colorsEquippedField.GetValue(playerCosmeticsInstance) is int[] colors)
				{
					if (colors.Length > 5)
					{
						savedOriginalColorIndex = colors[5];
					}
				}
				// 最终回退：默认 0
				if (savedOriginalColorIndex == -1)
				{
					savedOriginalColorIndex = 0;
				}
			}
			catch
			{
				savedOriginalColorIndex = 0;
			}
		}
		int num = new System.Random().Next(0, 36);
		try
		{
			SetColor(num);
			lastColorChangeTime = Time.time;
		}
		catch (Exception)
		{
		}
	}

	private static void SetColor(int colorIndex)
	{
		if (playerCosmeticsInstance == null || setupColorsMethod == null)
		{
			return;
		}
		int[] colorsArray = null;
		if (colorsEquippedField != null && colorsEquippedField.GetValue(playerCosmeticsInstance) is int[] currentColors)
		{
			colorsArray = (int[])currentColors.Clone();
		}
		if (colorsArray == null || colorsArray.Length <= 5)
		{
			colorsArray = new int[6];
			for (int i = 0; i < colorsArray.Length; i++)
			{
				colorsArray[i] = -1;
			}
		}
		colorsArray[5] = colorIndex;
		setupColorsMethod.Invoke(playerCosmeticsInstance, new object[2] { true, colorsArray });
	}

	private static bool IsPhotonViewValid(PhotonView view)
	{
		if ((Object)(object)view == (Object)null)
		{
			return false;
		}
		if ((Object)(object)((Component)view).gameObject == (Object)null || !((Component)view).gameObject.activeInHierarchy)
		{
			return false;
		}
		if (view.ViewID != 0)
		{
			return view.Owner != null;
		}
		return false;
	}

	/// <summary>
	/// 恢复原始颜色（关闭 RGB 时调用）
	/// </summary>
	public static void RestoreOriginalColor()
	{
		try
		{
			Initialize();
			if (savedOriginalColorIndex >= 0 && isInitialized && playerCosmeticsInstance != null && setupColorsMethod != null)
			{
				SetColor(savedOriginalColorIndex);
			}
		}
		catch { }
		finally
		{
			savedOriginalColorIndex = -1;
		}
	}

	public static void Reset()
	{
		isInitialized = false;
		colorControllerType = null;
		colorControllerInstance = null;
		playerCosmeticsField = null;
		playerCosmeticsInstance = null;
		colorsEquippedField = null;
		setupColorsMethod = null;
		playerPhotonView = null;
		savedOriginalColorIndex = -1;
	}
}
