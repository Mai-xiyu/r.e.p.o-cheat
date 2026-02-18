using System;
using System.Collections.Generic;
using UnityEngine;

namespace r.e.p.o_cheat;

public class HotkeyManager
{
	public class HotkeyAction
	{
		public string Name { get; set; }

		public Action Action { get; set; }

		public string Description { get; set; }

		public HotkeyAction(string name, Action action, string description = "")
		{
			Name = name;
			Action = action;
			Description = description;
		}
	}

	private Dictionary<KeyCode, Action> hotkeyBindings = new Dictionary<KeyCode, Action>();

	private List<HotkeyAction> availableActions = new List<HotkeyAction>();

	private KeyCode[] defaultHotkeys = (KeyCode[])(object)new KeyCode[12];

	private string keyAssignmentError = "";

	private float errorMessageTime;

	public const float ERROR_MESSAGE_DURATION = 3f;

	private KeyCode _menuToggleKey = (KeyCode)127;

	private KeyCode _reloadKey = (KeyCode)286;

	private KeyCode _unloadKey = (KeyCode)291;

	private static HotkeyManager _instance;

	public KeyCode MenuToggleKey => _menuToggleKey;

	public KeyCode ReloadKey => _reloadKey;

	public KeyCode UnloadKey => _unloadKey;

	public bool ConfiguringHotkey { get; private set; }

	public bool ConfiguringSystemKey { get; private set; }

	public bool WaitingForAnyKey { get; private set; }

	public int SystemKeyConfigIndex { get; private set; } = -1;

	public int SelectedHotkeySlot { get; private set; }

	public KeyCode CurrentHotkeyKey { get; private set; }

	public string KeyAssignmentError => keyAssignmentError;

	public float ErrorMessageTime => errorMessageTime;

	public static HotkeyManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new HotkeyManager();
			}
			return _instance;
		}
	}

	private HotkeyManager()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		availableActions.Clear();
		InitializeHotkeyActions();
		LoadHotkeySettings();
	}

	private void InitializeHotkeyActions()
	{
		availableActions.Add(new HotkeyAction("God Mode", delegate
		{
			bool godModeActive = !Hax2.godModeActive;
			PlayerController.GodMode();
			Hax2.godModeActive = godModeActive;
		}, "toggles god mode on/off"));
		availableActions.Add(new HotkeyAction("Noclip", delegate
		{
			bool noclipActive = !NoclipController.noclipActive;
			NoclipController.ToggleNoclip();
			NoclipController.noclipActive = noclipActive;
		}, "toggles noclip on/off"));
		availableActions.Add(new HotkeyAction("Tumble Guard", delegate
		{
			Hax2.debounce = !Hax2.debounce;
		}, "toggles tumble guard on/off"));
		availableActions.Add(new HotkeyAction("Infinite Health", delegate
		{
			Hax2.infiniteHealthActive = !Hax2.infiniteHealthActive;
			PlayerController.MaxHealth();
		}, "toggles infinite health on/off"));
		availableActions.Add(new HotkeyAction("Infinite Stamina", delegate
		{
			Hax2.stamineState = !Hax2.stamineState;
			PlayerController.MaxStamina();
		}, "toggles infinite stamina on/off"));
		availableActions.Add(new HotkeyAction("RGB Player", delegate
		{
			playerColor.isRandomizing = !playerColor.isRandomizing;
		}, "toggles rgb player effect"));
		availableActions.Add(new HotkeyAction("Spawn Money", delegate
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			GameObject localPlayer = DebugCheats.GetLocalPlayer();
			if ((Object)(object)localPlayer != (Object)null)
			{
				ItemSpawner.SpawnMoney(localPlayer.transform.position + Vector3.up * 1.5f);
			}
		}, "spawns money at your position"));
		availableActions.Add(new HotkeyAction("Kill All Enemies", delegate
		{
			Enemies.KillAllEnemies();
		}, "kills all enemies on the map"));
		availableActions.Add(new HotkeyAction("Enemy ESP Toggle", delegate
		{
			DebugCheats.drawEspBool = !DebugCheats.drawEspBool;
		}, "toggles enemy esp on/off"));
		availableActions.Add(new HotkeyAction("Item ESP Toggle", delegate
		{
			DebugCheats.drawItemEspBool = !DebugCheats.drawItemEspBool;
		}, "toggles item esp on/off"));
		availableActions.Add(new HotkeyAction("Player ESP Toggle", delegate
		{
			DebugCheats.drawPlayerEspBool = !DebugCheats.drawPlayerEspBool;
		}, "toggles player esp on/off"));
		availableActions.Add(new HotkeyAction("Heal Self", delegate
		{
			GameObject localPlayer = DebugCheats.GetLocalPlayer();
			if ((Object)(object)localPlayer != (Object)null)
			{
				Players.HealPlayer(localPlayer, 100, "Self");
			}
		}, "heals yourself by 100 hp"));
		availableActions.Add(new HotkeyAction("Max Speed", delegate
		{
			Hax2.sliderValueStrength = 30f;
			PlayerController.SetSprintSpeed(Hax2.sliderValueStrength);
		}, "sets speed to maximum value"));
		availableActions.Add(new HotkeyAction("Normal Speed", delegate
		{
			Hax2.sliderValueStrength = 5f;
			PlayerController.SetSprintSpeed(Hax2.sliderValueStrength);
		}, "sets speed to normal value"));
		availableActions.Add(new HotkeyAction("Unlimited Battery", delegate
		{
			Hax2.unlimitedBatteryActive = !Hax2.unlimitedBatteryActive;
			if ((Object)(object)Hax2.unlimitedBatteryComponent != (Object)null)
			{
				Hax2.unlimitedBatteryComponent.unlimitedBatteryEnabled = Hax2.unlimitedBatteryActive;
			}
		}, "toggles unlimited battery on/off"));
		for (int num = 0; num < defaultHotkeys.Length; num++)
		{
			defaultHotkeys[num] = (KeyCode)0;
		}
	}

	public void SaveHotkeySettings()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected I4, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected I4, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected I4, but got Unknown
		PlayerPrefs.SetInt("MenuToggleKey", (int)_menuToggleKey);
		PlayerPrefs.SetInt("ReloadKey", (int)_reloadKey);
		PlayerPrefs.SetInt("UnloadKey", (int)_unloadKey);
		for (int i = 0; i < defaultHotkeys.Length; i++)
		{
			PlayerPrefs.SetInt($"HotkeyKey_{i}", (int)defaultHotkeys[i]);
			int num = -1;
			if (defaultHotkeys[i] != KeyCode.None && hotkeyBindings.ContainsKey(defaultHotkeys[i]))
			{
				Action action = hotkeyBindings[defaultHotkeys[i]];
				if (action != null)
				{
					for (int j = 0; j < availableActions.Count; j++)
					{
						if (availableActions[j].Action == action)
						{
							num = j;
							break;
						}
					}
				}
			}
			PlayerPrefs.SetInt($"HotkeyAction_{i}", num);
		}
		PlayerPrefs.Save();
	}

	private void LoadHotkeySettings()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		_menuToggleKey = (KeyCode)PlayerPrefs.GetInt("MenuToggleKey", 127);
		_reloadKey = (KeyCode)PlayerPrefs.GetInt("ReloadKey", 286);
		_unloadKey = (KeyCode)PlayerPrefs.GetInt("UnloadKey", 291);
		hotkeyBindings.Clear();
		for (int i = 0; i < defaultHotkeys.Length; i++)
		{
			defaultHotkeys[i] = (KeyCode)PlayerPrefs.GetInt($"HotkeyKey_{i}", 0);
			int num = PlayerPrefs.GetInt($"HotkeyAction_{i}", -1);
			if (defaultHotkeys[i] != KeyCode.None && num >= 0 && num < availableActions.Count)
			{
				hotkeyBindings[defaultHotkeys[i]] = availableActions[num].Action;
			}
		}
	}

	public void StartConfigureSystemKey(int index)
	{
		ConfiguringSystemKey = true;
		SystemKeyConfigIndex = index;
		WaitingForAnyKey = true;
	}

	public string GetSystemKeyName(int index)
	{
		return index switch
		{
			0 => "Menu Toggle", 
			1 => "Reload", 
			2 => "Unload", 
			_ => "Unknown", 
		};
	}

	public void ShowActionSelector(int slotIndex, KeyCode key)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		CurrentHotkeyKey = key;
	}

	public void AssignActionToHotkey(int actionIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if ((int)CurrentHotkeyKey != 0 && actionIndex >= 0 && actionIndex < availableActions.Count)
		{
			hotkeyBindings[CurrentHotkeyKey] = availableActions[actionIndex].Action;
			SaveHotkeySettings();
		}
	}

	public void ClearHotkeyBinding(int slotIndex)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (slotIndex >= 0 && slotIndex < defaultHotkeys.Length)
		{
			KeyCode val = defaultHotkeys[slotIndex];
			if ((int)val != 0 && hotkeyBindings.ContainsKey(val))
			{
				hotkeyBindings.Remove(val);
			}
			defaultHotkeys[slotIndex] = (KeyCode)0;
			SaveHotkeySettings();
		}
	}

	public void ProcessHotkeyConfiguration(KeyCode key)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected I4, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		if ((int)key != 27)
		{
			if (key == _menuToggleKey || key == _reloadKey || key == _unloadKey)
			{
				keyAssignmentError = $"Cannot assign {key} as hotkey - it's already used as a system key!";
				errorMessageTime = Time.time;
				ConfiguringHotkey = false;
				return;
			}
			if (hotkeyBindings.ContainsKey(key))
			{
				keyAssignmentError = $"Cannot assign {key} as hotkey - it's already used for another action!";
				errorMessageTime = Time.time;
				ConfiguringHotkey = false;
				return;
			}
			KeyCode val = defaultHotkeys[SelectedHotkeySlot];
			if ((int)val != 0 && hotkeyBindings.ContainsKey(val))
			{
				Action value = hotkeyBindings[val];
				hotkeyBindings.Remove(val);
				hotkeyBindings[key] = value;
			}
			else
			{
				hotkeyBindings[key] = null;
			}
			defaultHotkeys[SelectedHotkeySlot] = (KeyCode)(int)key;
			ConfiguringHotkey = false;
		}
		else
		{
			ConfiguringHotkey = false;
		}
	}

	public void ProcessSystemKeyConfiguration(KeyCode key)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		if ((int)key != 27)
		{
			bool flag = false;
			string arg = "";
			WaitingForAnyKey = false;
			if (key == _menuToggleKey && SystemKeyConfigIndex != 0)
			{
				flag = true;
				arg = "Menu Toggle";
			}
			else if (key == _reloadKey && SystemKeyConfigIndex != 1)
			{
				flag = true;
				arg = "Reload";
			}
			else if (key == _unloadKey && SystemKeyConfigIndex != 2)
			{
				flag = true;
				arg = "Unload";
			}
			else if (hotkeyBindings.ContainsKey(key))
			{
				flag = true;
				arg = "action hotkey";
			}
			if (flag)
			{
				keyAssignmentError = $"Cannot assign {key} - already used as {arg}!";
				errorMessageTime = Time.time;
				ConfiguringSystemKey = false;
				return;
			}
			switch (SystemKeyConfigIndex)
			{
			case 0:
				_menuToggleKey = key;
				break;
			case 1:
				_reloadKey = key;
				break;
			case 2:
				_unloadKey = key;
				break;
			}
			ConfiguringSystemKey = false;
			SaveHotkeySettings();
		}
		else
		{
			ConfiguringSystemKey = false;
		}
	}

	public void StartHotkeyConfiguration(int slotIndex)
	{
		SelectedHotkeySlot = slotIndex;
		ConfiguringHotkey = true;
	}

	public void CheckAndExecuteHotkeys()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<KeyCode, Action> hotkeyBinding in hotkeyBindings)
		{
			if (Input.GetKeyDown(hotkeyBinding.Key) && hotkeyBinding.Value != null)
			{
				hotkeyBinding.Value();
				break;
			}
		}
	}

	public KeyCode GetHotkeyForSlot(int slotIndex)
	{
		if (slotIndex >= 0 && slotIndex < defaultHotkeys.Length)
		{
			return defaultHotkeys[slotIndex];
		}
		return (KeyCode)0;
	}

	public string GetActionNameForKey(KeyCode key)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if ((int)key != 0 && hotkeyBindings.ContainsKey(key))
		{
			Action action = hotkeyBindings[key];
			if (action != null)
			{
				for (int i = 0; i < availableActions.Count; i++)
				{
					if (availableActions[i].Action == action)
					{
						return availableActions[i].Name;
					}
				}
			}
		}
		return "Not assigned";
	}

	public List<HotkeyAction> GetAvailableActions()
	{
		return availableActions;
	}
}
