using System.Collections.Generic;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// Composable, auto-rolling-back timelines on top of existing Fun modules.
/// Birthday / robot / haunted stay on LocalOnly or SelfOwned game APIs.
/// </summary>
public static class EffectDirector
{
	public enum ActionKind
	{
		Toast,
		Nod,
		Shake,
		GyroOn,
		GyroOff,
		ScaleOn,
		ScaleOff,
		RainbowOn,
		RainbowOff,
		Color,
		Bounce,
		Tumble,
		ScareLocal,
		ScareAmbience,
		Expression,
		ExpressionReset,
		Restore
	}

	public struct Step
	{
		public float At;
		public ActionKind Action;
		public float Duration;
		public float Value;
		public int IntValue;
		public string Text;
	}

	public class Preset
	{
		public string Id;
		public string NameKey;
		public string HintKey;
		public EffectScope Scope;
		public float Length;
		public Step[] Steps;
	}

	private struct Snapshot
	{
		public bool Gyro;
		public float GyroSpeed;
		public bool Scale;
		public float ScaleValue;
		public bool Rainbow;
		public int ExpressionIndex;
	}

	private static readonly List<Preset> Presets = BuildPresets();
	private static Preset _playing;
	private static float _startedAt;
	private static int _nextStep;
	private static Snapshot _snap;
	private static int _heldExpression = -1;
	private static float _nextExpressionPulse;
	private static string _levelName;

	public static bool IsPlaying => _playing != null;
	public static string PlayingId => _playing != null ? _playing.Id : "";
	public static string LastStatus = "";

	public static IReadOnlyList<Preset> AllPresets => Presets;

	public static void Tick()
	{
		string level = null;
		try
		{
			if (RunManager.instance != null && RunManager.instance.levelCurrent != null)
			{
				level = RunManager.instance.levelCurrent.name;
			}
		}
		catch
		{
		}
		if (level != _levelName)
		{
			_levelName = level;
			if (IsPlaying)
			{
				StopAll();
			}
		}

		if (_playing == null)
		{
			return;
		}

		float elapsed = Time.unscaledTime - _startedAt;
		while (_nextStep < _playing.Steps.Length && _playing.Steps[_nextStep].At <= elapsed)
		{
			Fire(_playing.Steps[_nextStep]);
			_nextStep++;
		}

		if (_heldExpression >= 0 && Time.unscaledTime >= _nextExpressionPulse)
		{
			ApplyExpression(_heldExpression, 1f);
			_nextExpressionPulse = Time.unscaledTime + 0.15f;
		}

		if (elapsed >= _playing.Length)
		{
			StopAll();
		}
	}

	public static bool Play(string id)
	{
		Preset preset = Find(id);
		if (preset == null)
		{
			LastStatus = "unknown preset";
			return false;
		}
		if (!CanStart(preset, out string reason))
		{
			LastStatus = reason;
			ToastNotification.Show(L.T("director.title"), reason, new Color(1f, 0.5f, 0.2f));
			return false;
		}

		StopAll();
		_snap = Capture();
		_playing = preset;
		_startedAt = Time.unscaledTime;
		_nextStep = 0;
		_heldExpression = -1;
		LastStatus = L.T(preset.NameKey);
		ToastNotification.Show(L.T("director.title"), L.T(preset.NameKey));
		return true;
	}

	public static void StopAll()
	{
		if (_playing == null)
		{
			_heldExpression = -1;
			return;
		}
		_playing = null;
		_nextStep = 0;
		_heldExpression = -1;
		Restore(_snap);
		LastStatus = L.T("director.stopped");
	}

	public static void Unload()
	{
		if (_playing != null)
		{
			StopAll();
		}
		LastStatus = "";
	}

	private static bool CanStart(Preset preset, out string reason)
	{
		reason = "";
		if (preset.Scope == EffectScope.HostPrivate && NativeGameApi.IsGuest())
		{
			reason = L.T("role.host_only_tag");
			return false;
		}
		if (preset.Scope == EffectScope.SelfOwned || preset.Scope == EffectScope.HostPrivate)
		{
			try
			{
				if (SemiFunc.PlayerAvatarLocal() == null)
				{
					reason = L.T("director.no_avatar");
					return false;
				}
			}
			catch
			{
				reason = L.T("director.no_avatar");
				return false;
			}
		}
		return true;
	}

	private static void Fire(Step step)
	{
		switch (step.Action)
		{
			case ActionKind.Toast:
				ToastNotification.Show(L.T("director.title"), string.IsNullOrEmpty(step.Text) ? LastStatus : L.T(step.Text));
				break;
			case ActionKind.Nod:
				HeadGesture.StartNod();
				break;
			case ActionKind.Shake:
				HeadGesture.StartShake();
				break;
			case ActionKind.GyroOn:
				GyroSpin.spinSpeed = step.Value > 1f ? step.Value : 220f;
				GyroSpin.isEnabled = true;
				break;
			case ActionKind.GyroOff:
				GyroSpin.isEnabled = false;
				break;
			case ActionKind.ScaleOn:
				ScaleSync.targetScale = step.Value > 0.05f ? step.Value : 1.4f;
				ScaleSync.isEnabled = true;
				break;
			case ActionKind.ScaleOff:
				ScaleSync.isEnabled = false;
				ScaleSync.Restore();
				break;
			case ActionKind.RainbowOn:
				CosmeticFeatures.RainbowMode = true;
				break;
			case ActionKind.RainbowOff:
				CosmeticFeatures.RainbowMode = false;
				break;
			case ActionKind.Color:
				CosmeticFeatures.ApplyPaletteColor(step.IntValue, sync: true);
				break;
			case ActionKind.Bounce:
				NativeGameApi.Bounce();
				break;
			case ActionKind.Tumble:
				NativeGameApi.SelfTumble();
				break;
			case ActionKind.ScareLocal:
				ScareSound.PlayLocalImpact(soft: step.IntValue == 0);
				break;
			case ActionKind.ScareAmbience:
				ScareSound.PlayHostAmbience();
				break;
			case ActionKind.Expression:
				_heldExpression = step.IntValue;
				ApplyExpression(_heldExpression, 1f);
				_nextExpressionPulse = Time.unscaledTime + 0.15f;
				break;
			case ActionKind.ExpressionReset:
				_heldExpression = -1;
				ResetExpression();
				break;
			case ActionKind.Restore:
				Restore(_snap);
				_heldExpression = -1;
				break;
		}
	}

	private static Snapshot Capture()
	{
		return new Snapshot
		{
			Gyro = GyroSpin.isEnabled,
			GyroSpeed = GyroSpin.spinSpeed,
			Scale = ScaleSync.isEnabled,
			ScaleValue = ScaleSync.targetScale,
			Rainbow = CosmeticFeatures.RainbowMode,
			ExpressionIndex = -1
		};
	}

	private static void Restore(Snapshot snap)
	{
		GyroSpin.isEnabled = snap.Gyro;
		GyroSpin.spinSpeed = snap.GyroSpeed > 1f ? snap.GyroSpeed : 45f;
		ScaleSync.targetScale = snap.ScaleValue > 0.05f ? snap.ScaleValue : 1f;
		ScaleSync.isEnabled = snap.Scale;
		if (!snap.Scale)
		{
			ScaleSync.Restore();
		}
		CosmeticFeatures.RainbowMode = snap.Rainbow;
		HeadGesture.Stop();
		ResetExpression();
	}

	private static void ApplyExpression(int index, float percent)
	{
		try
		{
			PlayerAvatar local = SemiFunc.PlayerAvatarLocal();
			if (local == null || local.photonView == null || !local.photonView.IsMine)
			{
				return;
			}
			local.PlayerExpressionSet(index, percent);
		}
		catch
		{
		}
	}

	private static void ResetExpression()
	{
		try
		{
			PlayerAvatar local = SemiFunc.PlayerAvatarLocal();
			if (local == null || local.photonView == null || !local.photonView.IsMine)
			{
				return;
			}
			local.PlayerExpressionReset();
		}
		catch
		{
		}
	}

	private static Preset Find(string id)
	{
		for (int i = 0; i < Presets.Count; i++)
		{
			if (Presets[i].Id == id)
			{
				return Presets[i];
			}
		}
		return null;
	}

	private static List<Preset> BuildPresets()
	{
		return new List<Preset>
		{
			new Preset
			{
				Id = "birthday",
				NameKey = "director.birthday",
				HintKey = "director.birthday_hint",
				Scope = EffectScope.SelfOwned,
				Length = 8.5f,
				Steps = new[]
				{
					new Step { At = 0f, Action = ActionKind.Toast, Text = "director.birthday" },
					new Step { At = 0f, Action = ActionKind.Expression, IntValue = 1 },
					new Step { At = 0.4f, Action = ActionKind.Bounce },
					new Step { At = 1f, Action = ActionKind.ScareLocal, IntValue = 0 },
					new Step { At = 1.2f, Action = ActionKind.RainbowOn },
					new Step { At = 2.2f, Action = ActionKind.Color, IntValue = 3 },
					new Step { At = 3.4f, Action = ActionKind.Color, IntValue = 7 },
					new Step { At = 5f, Action = ActionKind.Nod },
					new Step { At = 7.5f, Action = ActionKind.RainbowOff },
					new Step { At = 7.6f, Action = ActionKind.ExpressionReset },
					new Step { At = 8f, Action = ActionKind.Restore }
				}
			},
			new Preset
			{
				Id = "robot",
				NameKey = "director.robot",
				HintKey = "director.robot_hint",
				Scope = EffectScope.SelfOwned,
				Length = 8.5f,
				Steps = new[]
				{
					new Step { At = 0f, Action = ActionKind.Nod },
					new Step { At = 1.4f, Action = ActionKind.Shake },
					new Step { At = 2.8f, Action = ActionKind.GyroOn, Value = 280f },
					new Step { At = 4.6f, Action = ActionKind.Tumble },
					new Step { At = 5.4f, Action = ActionKind.Color, IntValue = 2 },
					new Step { At = 6.4f, Action = ActionKind.GyroOff },
					new Step { At = 7.8f, Action = ActionKind.Restore }
				}
			},
			new Preset
			{
				Id = "haunted",
				NameKey = "director.haunted",
				HintKey = "director.haunted_hint",
				Scope = EffectScope.LocalOnly,
				Length = 3.6f,
				Steps = new[]
				{
					new Step { At = 0f, Action = ActionKind.ScareLocal, IntValue = 1 },
					new Step { At = 0.15f, Action = ActionKind.Toast, Text = "director.haunted" },
					new Step { At = 3f, Action = ActionKind.Toast, Text = "director.prank_over" }
				}
			}
		};
	}
}
