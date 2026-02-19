using System;
using System.Collections.Generic;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

internal class PlayerController
{
	public static object playerSpeedInstance;

	public static object reviveInstance;

	public static object enemyDirectorInstance;

	public static object playerControllerInstance;

	public static Type playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");

	private static float desiredDelayMultiplier = 1f;

	private static float desiredRateMultiplier = 1f;

	private static void InitializePlayerController()
	{
		if (!(playerControllerType == null))
		{
			playerControllerInstance = GameHelper.FindObjectOfType(playerControllerType);
			_ = playerControllerInstance;
		}
	}

	public static void GodMode()
	{
		SetGodMode(!Hax2.godModeActive);
	}

	public static void SetGodMode(bool enable)
	{
		if (PlayerReflectionCache.PlayerHealthInstance == null)
		{
			PlayerReflectionCache.CachePlayerControllerData();
		}
		if (PlayerReflectionCache.PlayerHealthInstance != null)
		{
			FieldInfo field = PlayerReflectionCache.PlayerHealthInstance.GetType().GetField("godMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				field.SetValue(PlayerReflectionCache.PlayerHealthInstance, enable);
				Hax2.godModeActive = enable;
			}
		}
	}

	public static void SetSprintSpeed(float value)
	{
		if (PlayerReflectionCache.PlayerControllerInstance == null)
		{
			PlayerReflectionCache.CachePlayerControllerData();
		}
		if (PlayerReflectionCache.PlayerControllerInstance != null)
		{
			FieldInfo field = PlayerReflectionCache.PlayerControllerType.GetField("SprintSpeed", BindingFlags.Instance | BindingFlags.Public);
			if (field != null)
			{
				field.SetValue(PlayerReflectionCache.PlayerControllerInstance, value);
			}
		}
	}

	public static void MaxHealth()
	{
		if (PlayerReflectionCache.PlayerHealthInstance == null)
		{
			PlayerReflectionCache.CachePlayerControllerData();
		}
		if (PlayerReflectionCache.PlayerHealthInstance == null)
		{
			return;
		}
		MethodInfo method = PlayerReflectionCache.PlayerHealthInstance.GetType().GetMethod("UpdateHealthRPC", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (method != null)
		{
			if (Hax2.infiniteHealthActive)
			{
				method.Invoke(PlayerReflectionCache.PlayerHealthInstance, new object[3] { 999999, 100, true });
			}
			else
			{
				method.Invoke(PlayerReflectionCache.PlayerHealthInstance, new object[3] { 100, 100, true });
			}
		}
	}

	public static void MaxStamina()
	{
		if (PlayerReflectionCache.PlayerControllerInstance == null)
		{
			PlayerReflectionCache.CachePlayerControllerData();
		}
		if (PlayerReflectionCache.PlayerControllerInstance != null && PlayerReflectionCache.EnergyCurrentField != null)
		{
			int num = (Hax2.stamineState ? 999999 : 40);
			PlayerReflectionCache.EnergyCurrentField.SetValue(PlayerReflectionCache.PlayerControllerInstance, num);
		}
	}

	public static void DecreaseStaminaRechargeDelay(float delayMultiplier, float rateMultiplier = 1f)
	{
		if (PlayerReflectionCache.PlayerControllerInstance == null)
		{
			PlayerReflectionCache.CachePlayerControllerData();
		}
		if (PlayerReflectionCache.PlayerControllerInstance != null)
		{
			desiredDelayMultiplier = delayMultiplier;
			desiredRateMultiplier = rateMultiplier;
			FieldInfo sprintRechargeTimeField = PlayerReflectionCache.SprintRechargeTimeField;
			if (sprintRechargeTimeField != null)
			{
				float num = 1f * delayMultiplier;
				sprintRechargeTimeField.SetValue(PlayerReflectionCache.PlayerControllerInstance, num);
			}
			FieldInfo sprintRechargeAmountField = PlayerReflectionCache.SprintRechargeAmountField;
			if (sprintRechargeAmountField != null)
			{
				float num2 = 2f * rateMultiplier;
				sprintRechargeAmountField.SetValue(PlayerReflectionCache.PlayerControllerInstance, num2);
			}
		}
	}

	public static void ReapplyStaminaSettings()
	{
		InitializePlayerController();
		if (playerControllerInstance != null)
		{
			DecreaseStaminaRechargeDelay(desiredDelayMultiplier, desiredRateMultiplier);
		}
	}

	public static void SetFlashlightIntensity(float value)
	{
		if (PlayerReflectionCache.FlashlightControllerInstance == null || PlayerReflectionCache.BaseIntensityField == null)
		{
			PlayerReflectionCache.CachePlayerControllerData();
		}
		if (PlayerReflectionCache.FlashlightControllerInstance != null && PlayerReflectionCache.BaseIntensityField != null)
		{
			PlayerReflectionCache.BaseIntensityField.SetValue(PlayerReflectionCache.FlashlightControllerInstance, value);
		}
	}

	public static void SetCrouchDelay(float value)
	{
		if (PlayerReflectionCache.PlayerControllerInstance != null)
		{
			FieldInfo field = PlayerReflectionCache.PlayerControllerType.GetField("CrouchTimeMin", BindingFlags.Instance | BindingFlags.Public);
			if (field != null)
			{
				field.SetValue(PlayerReflectionCache.PlayerControllerInstance, value);
			}
		}
	}

	public static void SetCrouchSpeed(float value)
	{
		if (PlayerReflectionCache.PlayerControllerInstance != null)
		{
			FieldInfo field = PlayerReflectionCache.PlayerControllerType.GetField("CrouchSpeed", BindingFlags.Instance | BindingFlags.Public);
			if (field != null)
			{
				field.SetValue(PlayerReflectionCache.PlayerControllerInstance, value);
			}
		}
	}

	public static void SetJumpForce(float value)
	{
		if (PlayerReflectionCache.PlayerControllerInstance != null)
		{
			FieldInfo field = PlayerReflectionCache.PlayerControllerType.GetField("JumpForce", BindingFlags.Instance | BindingFlags.Public);
			if (field != null)
			{
				field.SetValue(PlayerReflectionCache.PlayerControllerInstance, value);
			}
		}
	}

	public static void SetExtraJumps(int value)
	{
		if (PlayerReflectionCache.PlayerControllerInstance != null)
		{
			FieldInfo field = PlayerReflectionCache.PlayerControllerType.GetField("JumpExtra", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field != null)
			{
				field.SetValue(PlayerReflectionCache.PlayerControllerInstance, value);
			}
		}
	}

	public static void SetCustomGravity(float value)
	{
		if (PlayerReflectionCache.PlayerControllerInstance != null)
		{
			FieldInfo field = PlayerReflectionCache.PlayerControllerType.GetField("CustomGravity", BindingFlags.Instance | BindingFlags.Public);
			if (field != null)
			{
				field.SetValue(PlayerReflectionCache.PlayerControllerInstance, value);
			}
		}
	}

	public static void SetCrawlDelay(float crawlDelay)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		if (PlayerReflectionCache.PlayerControllerInstance == null)
		{
			return;
		}
		FieldInfo fieldInfo = PlayerReflectionCache.CrouchTimeMinField;
		if (fieldInfo == null)
		{
			fieldInfo = (PlayerReflectionCache.CrouchTimeMinField = PlayerReflectionCache.PlayerControllerType.GetField("CrouchTimeMin", BindingFlags.Instance | BindingFlags.Public));
		}
		if (!(fieldInfo != null))
		{
			return;
		}
		fieldInfo.SetValue(PlayerReflectionCache.PlayerControllerInstance, crawlDelay);
		PhotonView val = null;
		if (PlayerReflectionCache.PhotonViewField != null)
		{
			val = (PhotonView)PlayerReflectionCache.PhotonViewField.GetValue(PlayerReflectionCache.PlayerControllerInstance);
		}
		if ((Object)(object)val == (Object)null)
		{
			FieldInfo field2 = PlayerReflectionCache.PlayerAvatarScriptInstance.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field2 != null)
			{
				val = (PhotonView)field2.GetValue(PlayerReflectionCache.PlayerAvatarScriptInstance);
			}
		}
		if ((Object)(object)val != (Object)null)
		{
			if (PhotonNetwork.IsMasterClient)
			{
				val.RPC("SetCrawlDelayRPC", (RpcTarget)3, new object[1] { crawlDelay });
			}
			else
			{
				val.RPC("SetCrawlDelayRPC", (RpcTarget)2, new object[1] { crawlDelay });
			}
		}
	}

	public static void SetGrabRange(float value)
	{
		if (PlayerReflectionCache.PlayerAvatarScriptInstance == null)
		{
			return;
		}
		FieldInfo field = PlayerReflectionCache.PlayerAvatarScriptInstance.GetType().GetField("physGrabber", BindingFlags.Instance | BindingFlags.Public);
		if (field == null)
		{
			return;
		}
		object value2 = field.GetValue(PlayerReflectionCache.PlayerAvatarScriptInstance);
		if (value2 != null)
		{
			FieldInfo field2 = value2.GetType().GetField("grabRange", BindingFlags.Instance | BindingFlags.Public);
			if (field2 != null)
			{
				field2.SetValue(value2, value);
			}
		}
	}

	public static void SetThrowStrength(float value)
	{
		if (PlayerReflectionCache.PlayerAvatarScriptInstance == null)
		{
			return;
		}
		FieldInfo field = PlayerReflectionCache.PlayerAvatarScriptInstance.GetType().GetField("physGrabber", BindingFlags.Instance | BindingFlags.Public);
		if (field == null)
		{
			return;
		}
		object value2 = field.GetValue(PlayerReflectionCache.PlayerAvatarScriptInstance);
		if (value2 != null)
		{
			FieldInfo field2 = value2.GetType().GetField("throwStrength", BindingFlags.Instance | BindingFlags.Public);
			if (field2 != null)
			{
				field2.SetValue(value2, value);
			}
		}
	}

	public static void SetSlideDecay(float value)
	{
		if (PlayerReflectionCache.PlayerControllerInstance != null)
		{
			FieldInfo field = PlayerReflectionCache.PlayerControllerType.GetField("SlideDecay", BindingFlags.Instance | BindingFlags.Public);
			if (field != null)
			{
				field.SetValue(PlayerReflectionCache.PlayerControllerInstance, value);
			}
		}
	}

	public static int GetCurrentMaxHealth()
	{
		if (PlayerReflectionCache.MaxHealthField == null)
		{
			return 0;
		}
		try
		{
			return (int)PlayerReflectionCache.MaxHealthField.GetValue(PlayerReflectionCache.PlayerHealthInstance);
		}
		catch (Exception)
		{
			return 0;
		}
	}

	public static void SetMaxHealth(int newMaxHealth)
	{
		if (PlayerReflectionCache.PlayerHealthInstance != null)
		{
			MethodInfo method = PlayerReflectionCache.PlayerHealthInstance.GetType().GetMethod("UpdateHealthRPC", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method != null)
			{
				method.Invoke(PlayerReflectionCache.PlayerHealthInstance, new object[3] { newMaxHealth, newMaxHealth, true });
			}
		}
	}

	public static float GetCurrentMaxStamina()
	{
		if (PlayerReflectionCache.EnergyStartField == null)
		{
			return 0f;
		}
		try
		{
			return (float)PlayerReflectionCache.EnergyStartField.GetValue(PlayerReflectionCache.PlayerControllerInstance);
		}
		catch (Exception)
		{
			return 0f;
		}
	}

	public static void SetMaxStamina(float newMaxStamina)
	{
		if (PlayerReflectionCache.PlayerControllerInstance != null)
		{
			if (PlayerReflectionCache.EnergyStartField != null)
			{
				PlayerReflectionCache.EnergyStartField.SetValue(PlayerReflectionCache.PlayerControllerInstance, newMaxStamina);
			}
			if (PlayerReflectionCache.EnergyCurrentField != null)
			{
				PlayerReflectionCache.EnergyCurrentField.SetValue(PlayerReflectionCache.PlayerControllerInstance, newMaxStamina);
			}
		}
	}

	public static void ReapplyAllStats()
	{
		InitializePlayerController();
		if (playerControllerInstance != null)
		{
			_ = PhotonNetwork.LocalPlayer.UserId;
			int currentMaxHealth = GetCurrentMaxHealth();
			SetMaxHealth(Mathf.Max(100, currentMaxHealth));
			float currentMaxStamina = GetCurrentMaxStamina();
			SetMaxStamina(Mathf.Max(40f, currentMaxStamina));
			SetSprintSpeed(5f);
			Strength.MaxStrength();
			MaxStamina();
			ReapplyStaminaSettings();
			SetThrowStrength(Hax2.throwStrength);
			SetGrabRange(5f);
			SetCrouchDelay(Hax2.crouchDelay);
			SetCrouchSpeed(1f);
			SetJumpForce(17f);
			SetExtraJumps(0);
			SetCustomGravity(30f);
			SetSlideDecay(Hax2.slideDecay);
			SetFlashlightIntensity(Hax2.flashlightIntensity);
			if ((Object)(object)FOVEditor.Instance != (Object)null)
			{
				FOVEditor.Instance.SetFOV(Hax2.fieldOfView);
			}
		}
	}

	public static void LoadDefaultStatsIntoHax2()
	{
		InitializePlayerController();
		Type type;
		if (playerControllerInstance != null)
		{
			type = playerControllerType;
			Hax2.crouchSpeed = GetFloat("CrouchSpeed", 1f);
			Hax2.crouchDelay = GetFloat("CrouchTimeMin", 0f);
			Hax2.extraJumps = GetInt("JumpExtra", 0);
			Hax2.flashlightIntensity = 1f;
			Hax2.fieldOfView = 70f;
			Hax2.oldSliderValue = Hax2.sliderValue;
			Hax2.oldSliderValueStrength = Hax2.sliderValueStrength;
		}
		float GetFloat(string fieldName, float fallback)
		{
			FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
			if (field != null)
			{
				object value = field.GetValue(playerControllerInstance);
				if (value is float)
				{
					return (float)value;
				}
			}
			return fallback;
		}
		int GetInt(string fieldName, int fallback)
		{
			FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
			if (field != null)
			{
				object value = field.GetValue(playerControllerInstance);
				if (value is int)
				{
					return (int)value;
				}
			}
			return fallback;
		}
	}

	public static void SetFieldOfView(float fov)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		FOVEditor fOVEditor = Object.FindObjectOfType<FOVEditor>();
		if ((Object)(object)fOVEditor == (Object)null)
		{
			GameObject val = new GameObject("FOVEditor");
			fOVEditor = val.AddComponent<FOVEditor>();
			Object.DontDestroyOnLoad((Object)val);
		}
		fOVEditor.SetFOV(fov);
	}

	public static string GetLocalPlayerSteamID()
	{
		int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
		List<PlayerAvatar> list = SemiFunc.PlayerGetList();
		if (list == null)
		{
			Debug.LogWarning((object)"玩家列表为空！");
			return "";
		}
		foreach (PlayerAvatar item in list)
		{
			if ((Object)(object)item == (Object)null)
			{
				continue;
			}
			try
			{
				FieldInfo field = ((object)item).GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				PhotonView val = field?.GetValue(item) as PhotonView;
				if ((Object)(object)val == (Object)null || val.OwnerActorNr != actorNumber)
				{
					continue;
				}
				FieldInfo field2 = ((object)item).GetType().GetField("steamID", BindingFlags.Instance | BindingFlags.NonPublic);
				return (field2 != null) ? (field2.GetValue(item) as string) : "";
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("从玩家对象获取 SteamID 时出错：" + ex.Message));
			}
		}
		return "";
	}
}
