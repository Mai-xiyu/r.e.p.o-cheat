using System;
using System.Collections.Generic;
using System.Reflection;
using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// Per-player eye renderer effect. Each injected client publishes only its own
/// selected state through its Photon player custom properties. Receivers render
/// the state for the owner of each avatar; no master-client relay is involved.
/// Native hurt/death/expression overlays always take priority.
/// </summary>
public static class EyeColorEffects
{
	public enum EyeMode
	{
		Off,
		Fixed,
		Random,
		Rainbow
	}

	private const string PropertyMode = "rpo.eye.mode";
	private const string PropertyRed = "rpo.eye.r";
	private const string PropertyGreen = "rpo.eye.g";
	private const string PropertyBlue = "rpo.eye.b";
	private const string PropertyRandomInterval = "rpo.eye.interval";
	private const string PropertyRainbowSpeed = "rpo.eye.speed";
	private const string PropertySeed = "rpo.eye.seed";
	private const string PropertyStartedAt = "rpo.eye.started";

	private sealed class InstanceState
	{
		public bool Applied;
	}

	private struct SyncState
	{
		public EyeMode Mode;
		public byte Red;
		public byte Green;
		public byte Blue;
		public float RandomInterval;
		public float RainbowSpeed;
		public int Seed;
		public int StartedAt;
	}

	private static readonly FieldInfo EyeMaterialField = AccessTools.Field(typeof(PlayerHealth), "eyeMaterial");
	private static readonly FieldInfo PupilMaterialField = AccessTools.Field(typeof(PlayerHealth), "pupilMaterial");
	private static readonly FieldInfo NativeOverrideField = AccessTools.Field(typeof(PlayerHealth), "overrideEyeActive");
	private static readonly FieldInfo NativeOverrideLerpField = AccessTools.Field(typeof(PlayerHealth), "overrideEyeMaterialLerp");
	private static readonly FieldInfo MaterialEffectField = AccessTools.Field(typeof(PlayerHealth), "materialEffect");
	private static readonly FieldInfo HealthField = AccessTools.Field(typeof(PlayerHealth), "health");
	private static readonly Dictionary<int, InstanceState> States = new Dictionary<int, InstanceState>();
	private static readonly int OverlayColor = Shader.PropertyToID("_ColorOverlay");
	private static readonly int OverlayAmount = Shader.PropertyToID("_ColorOverlayAmount");

	private static bool _timelineInitialized;
	private static EyeMode _timelineMode;
	private static float _timelineInterval;
	private static float _timelineSpeed;
	private static int _timelineStartedAt;
	private static int _randomSeed = 0x5A1D37;
	private static string _publishedRoom;
	private static int _publishedActor;
	private static bool _hasPublished;
	private static SyncState _publishedState;
	private static float _nextPublishAttempt;

	public static EyeMode Mode = EyeMode.Off;
	public static Color FixedColor = new Color(0.1f, 0.8f, 1f, 1f);
	public static float RandomInterval = 1.25f;
	public static float RainbowSpeed = 0.2f;

	public static void RandomizeFixedColor()
	{
		FixedColor = NewRandomColor();
	}

	public static void RerollRandomColors()
	{
		_randomSeed = NewRandomSeed();
		ResetTimeline();
	}

	/// <summary>
	/// Called once per frame by the persistent menu component. Player properties
	/// are server-cached by Photon, so late injected clients can read the latest
	/// eye state without a buffered RPC or a host-side forwarding path.
	/// </summary>
	internal static void Tick()
	{
		if (!PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null || PhotonNetwork.CurrentRoom == null)
		{
			_publishedRoom = null;
			_publishedActor = 0;
			_hasPublished = false;
			_nextPublishAttempt = 0f;
			return;
		}

		Player local = PhotonNetwork.LocalPlayer;
		string roomName = PhotonNetwork.CurrentRoom.Name ?? string.Empty;
		if (_publishedRoom != roomName || _publishedActor != local.ActorNumber)
		{
			_publishedRoom = roomName;
			_publishedActor = local.ActorNumber;
			_hasPublished = false;
			_nextPublishAttempt = 0f;
			ResetTimeline();
		}

		SyncState state = BuildLocalState();
		if (_hasPublished && StatesEqual(state, _publishedState))
		{
			return;
		}
		if (Time.unscaledTime < _nextPublishAttempt)
		{
			return;
		}

		try
		{
			Hashtable properties = new Hashtable
			{
				{ PropertyMode, (byte)state.Mode },
				{ PropertyRed, state.Red },
				{ PropertyGreen, state.Green },
				{ PropertyBlue, state.Blue },
				{ PropertyRandomInterval, state.RandomInterval },
				{ PropertyRainbowSpeed, state.RainbowSpeed },
				{ PropertySeed, state.Seed },
				{ PropertyStartedAt, state.StartedAt }
			};
			if (PhotonNetwork.LocalPlayer.SetCustomProperties(properties))
			{
				_publishedState = state;
				_hasPublished = true;
				_nextPublishAttempt = 0f;
			}
			else
			{
				_nextPublishAttempt = Time.unscaledTime + 1f;
			}
		}
		catch (Exception ex)
		{
			_nextPublishAttempt = Time.unscaledTime + 1f;
			Debug.LogWarning("[EyeSync] local property publish failed: " + ex.GetType().Name);
		}
	}

	internal static void Apply(PlayerHealth health)
	{
		if (health == null)
		{
			return;
		}

		int id = health.GetInstanceID();
		if (!States.TryGetValue(id, out InstanceState instance))
		{
			instance = new InstanceState();
			States[id] = instance;
		}

		Material eye = EyeMaterialField?.GetValue(health) as Material;
		Material pupil = PupilMaterialField?.GetValue(health) as Material;
		if (eye == null || pupil == null)
		{
			instance.Applied = false;
			return;
		}

		bool nativeOwnsFrame = NativeEffectActive(health);
		if (!TryGetOwnerState(health, out SyncState state) || state.Mode == EyeMode.Off || nativeOwnsFrame)
		{
			Release(health, eye, pupil, instance, nativeOwnsFrame);
			return;
		}

		Color color = ResolveColor(state);
		color.a = 1f;
		SetOverlay(eye, color, 1f);
		SetOverlay(pupil, Color.white, 1f);

		Light light = health.eyeLight;
		if (light != null)
		{
			if (!light.gameObject.activeSelf)
			{
				light.gameObject.SetActive(true);
			}
			light.color = color;
			light.intensity = 4f;
		}
		instance.Applied = true;
	}

	private static bool TryGetOwnerState(PlayerHealth health, out SyncState state)
	{
		PlayerAvatar avatar = health.GetComponentInParent<PlayerAvatar>();
		PhotonView view = health.GetComponent<PhotonView>();
		if (view == null && avatar != null)
		{
			view = avatar.photonView;
		}
		if (view == null)
		{
			view = health.GetComponentInParent<PhotonView>();
		}

		if (!PhotonNetwork.InRoom)
		{
			if (avatar == null || avatar != PlayerAvatar.instance)
			{
				state = default;
				return false;
			}
			state = BuildLocalState();
			return true;
		}

		if (view != null && view.IsMine)
		{
			state = BuildLocalState();
			return true;
		}

		Player owner = view != null ? view.Owner : null;
		if (owner != null)
		{
			return TryReadState(owner.CustomProperties, out state);
		}

		state = default;
		return false;
	}

	private static SyncState BuildLocalState()
	{
		EnsureTimeline();
		Color fixedColor = FixedColor;
		return new SyncState
		{
			Mode = Mode,
			Red = ToByte(fixedColor.r),
			Green = ToByte(fixedColor.g),
			Blue = ToByte(fixedColor.b),
			RandomInterval = Mathf.Clamp(RandomInterval, 0.1f, 10f),
			RainbowSpeed = Mathf.Clamp(RainbowSpeed, 0.02f, 1.5f),
			Seed = _randomSeed,
			StartedAt = _timelineStartedAt
		};
	}

	private static bool TryReadState(Hashtable properties, out SyncState state)
	{
		state = default;
		if (properties == null || !TryReadInt(properties, PropertyMode, out int rawMode) ||
			rawMode < (int)EyeMode.Off || rawMode > (int)EyeMode.Rainbow)
		{
			return false;
		}

		state.Mode = (EyeMode)rawMode;
		if (state.Mode == EyeMode.Off)
		{
			return true;
		}

		if (!TryReadInt(properties, PropertyRed, out int red) ||
			!TryReadInt(properties, PropertyGreen, out int green) ||
			!TryReadInt(properties, PropertyBlue, out int blue) ||
			!TryReadFloat(properties, PropertyRandomInterval, out float randomInterval) ||
			!TryReadFloat(properties, PropertyRainbowSpeed, out float rainbowSpeed) ||
			!TryReadInt(properties, PropertySeed, out int seed) ||
			!TryReadInt(properties, PropertyStartedAt, out int startedAt))
		{
			return false;
		}

		state.Red = (byte)Mathf.Clamp(red, 0, 255);
		state.Green = (byte)Mathf.Clamp(green, 0, 255);
		state.Blue = (byte)Mathf.Clamp(blue, 0, 255);
		state.RandomInterval = Mathf.Clamp(randomInterval, 0.1f, 10f);
		state.RainbowSpeed = Mathf.Clamp(rainbowSpeed, 0.02f, 1.5f);
		state.Seed = seed;
		state.StartedAt = startedAt;
		return true;
	}

	private static bool TryReadInt(Hashtable properties, string key, out int value)
	{
		value = 0;
		try
		{
			if (!properties.TryGetValue(key, out object raw) || raw == null)
			{
				return false;
			}
			value = Convert.ToInt32(raw);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static bool TryReadFloat(Hashtable properties, string key, out float value)
	{
		value = 0f;
		try
		{
			if (!properties.TryGetValue(key, out object raw) || raw == null)
			{
				return false;
			}
			value = Convert.ToSingle(raw);
			return !float.IsNaN(value) && !float.IsInfinity(value);
		}
		catch
		{
			return false;
		}
	}

	private static void EnsureTimeline()
	{
		float interval = Mathf.Clamp(RandomInterval, 0.1f, 10f);
		float speed = Mathf.Clamp(RainbowSpeed, 0.02f, 1.5f);
		if (_timelineInitialized && _timelineMode == Mode &&
			Mathf.Abs(_timelineInterval - interval) < 0.0001f &&
			Mathf.Abs(_timelineSpeed - speed) < 0.0001f)
		{
			return;
		}

		_timelineInitialized = true;
		_timelineMode = Mode;
		_timelineInterval = interval;
		_timelineSpeed = speed;
		_timelineStartedAt = ClockMilliseconds();
	}

	private static void ResetTimeline()
	{
		_timelineInitialized = false;
		EnsureTimeline();
	}

	private static int ClockMilliseconds()
	{
		return PhotonNetwork.InRoom ? PhotonNetwork.ServerTimestamp : Mathf.RoundToInt(Time.unscaledTime * 1000f);
	}

	private static Color ResolveColor(SyncState state)
	{
		switch (state.Mode)
		{
			case EyeMode.Fixed:
				return new Color(state.Red / 255f, state.Green / 255f, state.Blue / 255f, 1f);
			case EyeMode.Random:
			{
				uint elapsed = unchecked((uint)(ClockMilliseconds() - state.StartedAt));
				uint interval = (uint)Mathf.Max(100, Mathf.RoundToInt(state.RandomInterval * 1000f));
				uint bucket = elapsed / interval;
				uint seed = unchecked((uint)state.Seed) ^ unchecked(bucket * 0x9E3779B9u);
				return ColorFromSeed(seed);
			}
			case EyeMode.Rainbow:
			{
				uint elapsed = unchecked((uint)(ClockMilliseconds() - state.StartedAt));
				float phase = elapsed * 0.001f * state.RainbowSpeed;
				return Color.HSVToRGB(Mathf.Repeat(phase, 1f), 0.9f, 1f);
			}
			default:
				return Color.white;
		}
	}

	private static bool NativeEffectActive(PlayerHealth health)
	{
		try
		{
			if (NativeOverrideField?.GetValue(health) is bool active && active)
			{
				return true;
			}
			if (NativeOverrideLerpField?.GetValue(health) is float lerp && lerp > 0.001f)
			{
				return true;
			}
			if (MaterialEffectField?.GetValue(health) is bool materialEffect && materialEffect)
			{
				return true;
			}
			if (HealthField?.GetValue(health) is int currentHealth && currentHealth <= 0)
			{
				return true;
			}
		}
		catch
		{
			return true;
		}
		return false;
	}

	private static bool StatesEqual(SyncState left, SyncState right)
	{
		return left.Mode == right.Mode && left.Red == right.Red && left.Green == right.Green && left.Blue == right.Blue &&
			left.Seed == right.Seed && left.StartedAt == right.StartedAt &&
			Mathf.Abs(left.RandomInterval - right.RandomInterval) < 0.0001f &&
			Mathf.Abs(left.RainbowSpeed - right.RainbowSpeed) < 0.0001f;
	}

	private static byte ToByte(float value)
	{
		return (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Clamp01(value) * 255f), 0, 255);
	}

	private static Color NewRandomColor()
	{
		return Color.HSVToRGB(UnityEngine.Random.value, UnityEngine.Random.Range(0.65f, 1f),
			UnityEngine.Random.Range(0.8f, 1f));
	}

	private static int NewRandomSeed()
	{
		return unchecked(UnityEngine.Random.Range(1, int.MaxValue) ^ Environment.TickCount);
	}

	private static Color ColorFromSeed(uint seed)
	{
		uint hueBits = Mix(seed);
		uint saturationBits = Mix(hueBits);
		uint valueBits = Mix(saturationBits);
		float hue = (hueBits & 0x00FFFFFFu) / 16777216f;
		float saturation = 0.65f + (saturationBits & 0x0000FFFFu) / 65535f * 0.35f;
		float value = 0.8f + (valueBits & 0x0000FFFFu) / 65535f * 0.2f;
		return Color.HSVToRGB(hue, saturation, value);
	}

	private static uint Mix(uint value)
	{
		value ^= value >> 16;
		value *= 0x7FEB352Du;
		value ^= value >> 15;
		value *= 0x846CA68Bu;
		value ^= value >> 16;
		return value;
	}

	private static void SetOverlay(Material material, Color color, float amount)
	{
		if (material.HasProperty(OverlayColor))
		{
			material.SetColor(OverlayColor, color);
		}
		if (material.HasProperty(OverlayAmount))
		{
			material.SetFloat(OverlayAmount, amount);
		}
	}

	private static void Release(PlayerHealth health, Material eye, Material pupil,
		InstanceState state, bool nativeOwnsFrame)
	{
		if (!state.Applied)
		{
			return;
		}

		// PlayerHealth.Update ran immediately before this postfix. If a native
		// effect is active, its values are already authoritative and must be kept.
		if (!nativeOwnsFrame)
		{
			SetOverlay(eye, Color.white, 0f);
			SetOverlay(pupil, Color.black, 0f);
			Light light = health.eyeLight;
			if (light != null && light.gameObject.activeSelf)
			{
				light.gameObject.SetActive(false);
			}
		}
		state.Applied = false;
	}
}

[HarmonyPatch(typeof(PlayerHealth), "Update")]
public static class EyeColorEffectsPlayerHealthPatch
{
	private static void Postfix(PlayerHealth __instance)
	{
		EyeColorEffects.Apply(__instance);
	}
}
