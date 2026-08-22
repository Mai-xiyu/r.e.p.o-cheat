using System;
using System.Collections.Generic;
using System.Reflection;
using IEnumerator = System.Collections.IEnumerator;
using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// Per-player eye effect. The local owner drives PlayerHealth.EyeMaterialOverride,
/// which is a vanilla owner-authenticated RPC and is therefore visible to clients
/// without this injection. Custom player properties retain the exact RGB selection
/// for injected peers; vanilla clients see the nearest built-in eye preset.
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
	private static readonly FieldInfo NativeOverrideStateField = AccessTools.Field(typeof(PlayerHealth), "overrideEyeState");
	private static readonly FieldInfo NativeOverridePriorityField = AccessTools.Field(typeof(PlayerHealth), "overrideEyePriority");
	private static readonly FieldInfo MaterialEffectField = AccessTools.Field(typeof(PlayerHealth), "materialEffect");
	private static readonly FieldInfo HealthField = AccessTools.Field(typeof(PlayerHealth), "health");
	private static readonly Dictionary<int, InstanceState> States = new Dictionary<int, InstanceState>();
	private static readonly HashSet<int> NativeRelayActors = new HashSet<int>();
	private static readonly int OverlayColor = Shader.PropertyToID("_ColorOverlay");
	private static readonly int OverlayAmount = Shader.PropertyToID("_ColorOverlayAmount");
	private const int NativeEyePriority = 0;
	private const float NativeEyeHoldSeconds = 0.75f;
	private const float NativeEyeRefreshSeconds = 0.25f;
	private static readonly float[] NativeRelaySchedule = { 0.25f, 1f, 2.5f };

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
	private static bool _nativeVisualActive;
	private static PlayerHealth.EyeOverrideState _nativeVisualState;
	private static float _nextNativeVisualRefresh;

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
			DriveNativeEyeVisual(BuildLocalState());
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
		DriveNativeEyeVisual(state);
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
		if (!TryGetOwnerState(health, out SyncState state) || state.Mode == EyeMode.Off)
		{
			Release(health, eye, pupil, instance, nativeOwnsFrame);
			return;
		}
		// The vanilla-compatible state is intentionally applied through the game's
		// native override.  When that override matches this owner's selected state,
		// injected peers may still render the precise RGB overlay while unmodified
		// peers keep the nearest native preset. A different native override remains
		// authoritative (hurt, death, healing, enemy effects, etc.).
		bool customNativeReplica = nativeOwnsFrame && IsCustomNativeReplica(health, state);
		if (nativeOwnsFrame && !customNativeReplica)
		{
			Release(health, eye, pupil, instance, nativeOwnsFrame: true);
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

	private static bool IsCustomNativeReplica(PlayerHealth health, SyncState state)
	{
		try
		{
			if (MaterialEffectField?.GetValue(health) is bool materialEffect && materialEffect)
			{
				return false;
			}
			if (HealthField?.GetValue(health) is int currentHealth && currentHealth <= 0)
			{
				return false;
			}
			if (!TryReadNativeOverride(health, out bool active, out PlayerHealth.EyeOverrideState nativeState, out int priority) ||
				!active || nativeState != ResolveNativeEyeState(state))
			{
				return false;
			}

			// The owner can validate its priority. A remote vanilla RPC does not carry
			// priority, so matching the selected state is the strongest safe signal it
			// can observe locally.
			PlayerAvatar localAvatar = PlayerAvatar.instance;
			return localAvatar == null || localAvatar.playerHealth != health || priority == NativeEyePriority;
		}
		catch
		{
			return false;
		}
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

	internal static void HandlePlayerEntered(Player newPlayer)
	{
		if (newPlayer == null || newPlayer.IsLocal || !PhotonNetwork.InRoom || Mode == EyeMode.Off)
		{
			return;
		}

		int actor = newPlayer.ActorNumber;
		if (actor <= 0 || !NativeRelayActors.Add(actor))
		{
			return;
		}
		Loader.RunCoroutine(RelayNativeEyeVisualToJoiner(actor));
	}

	private static IEnumerator RelayNativeEyeVisualToJoiner(int actor)
	{
		float started = Time.unscaledTime;
		try
		{
			for (int i = 0; i < NativeRelaySchedule.Length; i++)
			{
				while (PhotonNetwork.InRoom && Time.unscaledTime - started < NativeRelaySchedule[i])
				{
					yield return null;
				}
				if (!PhotonNetwork.InRoom || Mode == EyeMode.Off || PhotonNetwork.CurrentRoom == null)
				{
					yield break;
				}

				Player target = PhotonNetwork.CurrentRoom.GetPlayer(actor);
				if (target == null)
				{
					yield break;
				}
				if (SendNativeEyeVisualTo(target))
				{
					Debug.Log("[EyeSync] native eye relay target=" + actor + " attempt=" + (i + 1) + "/" +
						NativeRelaySchedule.Length + " state=" + _nativeVisualState);
				}
			}
		}
		finally
		{
			NativeRelayActors.Remove(actor);
		}
	}

	private static void DriveNativeEyeVisual(SyncState state)
	{
		if (state.Mode == EyeMode.Off)
		{
			_nativeVisualActive = false;
			_nextNativeVisualRefresh = 0f;
			return;
		}

		PlayerAvatar avatar = PlayerAvatar.instance;
		PlayerHealth health = avatar != null ? avatar.playerHealth : null;
		PhotonView view = avatar != null ? avatar.photonView : null;
		if (avatar == null || health == null || (PhotonNetwork.InRoom && (view == null || !view.IsMine)))
		{
			_nativeVisualActive = false;
			return;
		}

		PlayerHealth.EyeOverrideState desired = ResolveNativeEyeState(state);
		if (NativeVisualBlocked(health, desired))
		{
			_nativeVisualActive = false;
			return;
		}

		if (_nativeVisualActive && _nativeVisualState == desired &&
			Time.unscaledTime < _nextNativeVisualRefresh)
		{
			return;
		}

		try
		{
			health.EyeMaterialOverride(desired, NativeEyeHoldSeconds, NativeEyePriority);
			_nativeVisualActive = true;
			_nativeVisualState = desired;
			_nextNativeVisualRefresh = Time.unscaledTime + NativeEyeRefreshSeconds;
		}
		catch (Exception ex)
		{
			_nativeVisualActive = false;
			_nextNativeVisualRefresh = Time.unscaledTime + 1f;
			Debug.LogWarning("[EyeSync] native eye drive failed: " + ex.GetType().Name);
		}
	}

	private static bool SendNativeEyeVisualTo(Player target)
	{
		if (target == null || target.IsLocal || Mode == EyeMode.Off)
		{
			return false;
		}

		SyncState state = BuildLocalState();
		DriveNativeEyeVisual(state);
		if (!_nativeVisualActive)
		{
			return false;
		}

		PlayerAvatar avatar = PlayerAvatar.instance;
		PlayerHealth health = avatar != null ? avatar.playerHealth : null;
		PhotonView view = avatar != null ? avatar.photonView : null;
		if (avatar == null || health == null || view == null || !view.IsMine ||
			!TryReadNativeOverride(health, out bool active, out PlayerHealth.EyeOverrideState current, out int priority) ||
			!active || current != _nativeVisualState || priority != NativeEyePriority)
		{
			return false;
		}

		try
		{
			// EyeMaterialOverrideRPC is the game's own owner-only RPC.  The sender is
			// this avatar's owner, so unmodified receivers execute it normally.
			view.RPC("EyeMaterialOverrideRPC", target, _nativeVisualState, true);
			PhotonNetwork.SendAllOutgoingCommands();
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[EyeSync] native eye relay failed target=" + target.ActorNumber + ": " +
				ex.GetType().Name);
			return false;
		}
	}

	private static bool NativeVisualBlocked(PlayerHealth health, PlayerHealth.EyeOverrideState desired)
	{
		try
		{
			if (MaterialEffectField?.GetValue(health) is bool materialEffect && materialEffect)
			{
				return true;
			}
			if (HealthField?.GetValue(health) is int currentHealth && currentHealth <= 0)
			{
				return true;
			}
			if (!TryReadNativeOverride(health, out bool active, out PlayerHealth.EyeOverrideState current, out int priority))
			{
				return false;
			}
			if (active)
			{
				// A state we just installed may be refreshed. Any different active state
				// belongs to the game (hurt/heal/eye enemy/etc.) and must finish first.
				return !_nativeVisualActive || current != desired || priority != NativeEyePriority;
			}
			return NativeOverrideLerpField?.GetValue(health) is float lerp && lerp > 0.001f;
		}
		catch
		{
			return true;
		}
	}

	private static bool TryReadNativeOverride(PlayerHealth health, out bool active,
		out PlayerHealth.EyeOverrideState state, out int priority)
	{
		active = false;
		state = PlayerHealth.EyeOverrideState.None;
		priority = int.MinValue;
		try
		{
			if (!(NativeOverrideField?.GetValue(health) is bool currentActive))
			{
				return false;
			}
			if (!(NativeOverrideStateField?.GetValue(health) is PlayerHealth.EyeOverrideState currentState))
			{
				return false;
			}
			if (NativeOverridePriorityField?.GetValue(health) == null)
			{
				return false;
			}

			active = currentActive;
			state = currentState;
			priority = Convert.ToInt32(NativeOverridePriorityField.GetValue(health));
			return true;
		}
		catch
		{
			return false;
		}
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

	private static PlayerHealth.EyeOverrideState ResolveNativeEyeState(SyncState state)
	{
		switch (state.Mode)
		{
			case EyeMode.Fixed:
				return NearestNativeEyeState(new Color(state.Red / 255f, state.Green / 255f, state.Blue / 255f, 1f));
			case EyeMode.Random:
				return NearestNativeEyeState(ResolveColor(state));
			case EyeMode.Rainbow:
			{
				uint elapsed = unchecked((uint)(ClockMilliseconds() - state.StartedAt));
				float phase = elapsed * 0.001f * state.RainbowSpeed;
				int index = Mathf.FloorToInt(Mathf.Repeat(phase, 1f) * 5f);
				switch (Mathf.Clamp(index, 0, 4))
				{
					case 0:
						return PlayerHealth.EyeOverrideState.Red;
					case 1:
						return PlayerHealth.EyeOverrideState.Love;
					case 2:
						return PlayerHealth.EyeOverrideState.CeilingEye;
					case 3:
						return PlayerHealth.EyeOverrideState.Green;
					default:
						return PlayerHealth.EyeOverrideState.Inverted;
				}
			}
			default:
				return PlayerHealth.EyeOverrideState.Red;
		}
	}

	private static PlayerHealth.EyeOverrideState NearestNativeEyeState(Color color)
	{
		PlayerHealth.EyeOverrideState bestState = PlayerHealth.EyeOverrideState.Red;
		float bestDistance = ColorDistanceSquared(color, Color.red);
		ConsiderNativeEyeState(color, new Color(0f, 1f, 0f, 1f), PlayerHealth.EyeOverrideState.Green,
			ref bestState, ref bestDistance);
		ConsiderNativeEyeState(color, new Color(1f, 0f, 0.5f, 1f), PlayerHealth.EyeOverrideState.Love,
			ref bestState, ref bestDistance);
		ConsiderNativeEyeState(color, new Color(1f, 0.4f, 0f, 1f), PlayerHealth.EyeOverrideState.CeilingEye,
			ref bestState, ref bestDistance);
		ConsiderNativeEyeState(color, Color.black, PlayerHealth.EyeOverrideState.Inverted,
			ref bestState, ref bestDistance);
		return bestState;
	}

	private static void ConsiderNativeEyeState(Color source, Color candidate,
		PlayerHealth.EyeOverrideState candidateState, ref PlayerHealth.EyeOverrideState bestState, ref float bestDistance)
	{
		float distance = ColorDistanceSquared(source, candidate);
		if (distance < bestDistance)
		{
			bestState = candidateState;
			bestDistance = distance;
		}
	}

	private static float ColorDistanceSquared(Color left, Color right)
	{
		float red = left.r - right.r;
		float green = left.g - right.g;
		float blue = left.b - right.b;
		return red * red + green * green + blue * blue;
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
