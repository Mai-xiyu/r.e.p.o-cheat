using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace r.e.p.o_cheat;

public class Hax2 : MonoBehaviour
{
	private enum MenuCategory
	{
		Self,
		ESP,
		Combat,
		Misc,
		Enemies,
		Items,
		Hotkeys
	}

	private enum SortMode
	{
		None,
		RegionAZ,
		RegionZA,
		MostPlayers,
		LeastPlayers
	}

	private enum SortOption
	{
		None,
		RegionAsc,
		RegionDesc,
		MostPlayers,
		LeastPlayers
	}

	public class LobbyCoroutineHost : MonoBehaviour
	{
	}

	private static List<EnemySetup> cachedFilteredEnemySetups = null;

	private static List<string> cachedEnemySetupNames = null;

	private static float lastEnemyCacheTime = 0f;

	public string spawnCountText = "1";

	public int spawnEnemyIndex;

	public bool showSpawnDropdown;

	public Vector2 spawnDropdownScrollPosition = Vector2.zero;

	public static bool ChatDropdownVisible = false;

	public static string ChatDropdownVisibleName = L.T("common.all");

	public static float fieldOfView = 70f;

	public static float oldFieldOfView = 70f;

	private float nextUpdateTime;

	private const float updateInterval = 10f;

	private string[] availableLevels = new string[6] { "Level - Wizard", "Level - Shop", "Level - Manor", "Level - Arctic", "Level - Lobby", "Level - Recording" };

	private Vector2 chatDropdownScroll = Vector2.zero;

	private bool showChatDropdown;

	private int ChatSelectedPlayerIndex;

	private string chatMessageText = "Mai_xiyu CHEATS :3";

	private int selectedLevelIndex;

	private bool showLevelDropdown;

	private Vector2 levelDropdownScroll = Vector2.zero;

	public static bool hasInitializedDefaults = false;

	private bool sliderDragging;

	private bool dragTargetIsMin;

	private Vector2 sourceDropdownScrollPosition = Vector2.zero;

	private Vector2 destDropdownScrollPosition = Vector2.zero;

	private Vector2 enemyTeleportDropdownScrollPosition = Vector2.zero;

	private float levelCheckTimer;

	private const float LEVEL_CHECK_INTERVAL = 5f;

	private string previousLevelName = "";

	private bool pendingLevelUpdate;

	private float levelChangeDetectedTime;

	private const float LEVEL_UPDATE_DELAY = 3f;

	public static int selectedPlayerIndex = 0;

	public static List<string> playerNames = new List<string>();

	public static List<object> playerList = new List<object>();

	private int selectedEnemyIndex;

	private List<string> enemyNames = new List<string>();

	private List<Enemy> enemyList = new List<Enemy>();

	public static float offsetESp = 0f;

	public static bool showMenu = false;

	// Dynamic Island Animation
	private float _animProgress = 0f; // 0 = Collapsed pill, 1 = Full menu
	private bool _isExpanding = false;
	private bool _uiStylesInited = false;
	private const float PILL_WIDTH = 220f;
	private const float PILL_HEIGHT = 44f;
	private const float MENU_WIDTH = 860f;
	private const float MENU_HEIGHT = 600f;
	private const float ANIM_SPEED = 8f;
	// Pill always-visible + drag support
	private bool _pillAlwaysShow = true;
	private Vector2 _menuDragOffset = Vector2.zero;
	private bool _hasDragged = false;

	public static bool godModeActive = false;

	public static bool debounce = false;

	public static bool infiniteHealthActive = false;

	public static bool stamineState = false;

	public static bool unlimitedBatteryActive = false;

	public static UnlimitedBattery unlimitedBatteryComponent;

	public static bool blindEnemies = false;

	private Vector2 playerScrollPosition = Vector2.zero;

	private Vector2 enemyScrollPosition = Vector2.zero;

	private int teleportPlayerSourceIndex;

	private int teleportPlayerDestIndex;

	private string[] teleportPlayerSourceOptions;

	private string[] teleportPlayerDestOptions;

	private bool showTeleportUI;

	private bool showSourceDropdown;

	private bool showDestDropdown;

	private bool showEnemyTeleportUI;

	private bool showEnemyTeleportDropdown;

	private int enemyTeleportDestIndex;

	private string[] enemyTeleportDestOptions;

	private float enemyTeleportLabelWidth = 70f;

	private float enemyTeleportToWidth = 20f;

	private float enemyTeleportDropdownWidth = 200f;

	private float enemyTeleportTotalWidth;

	private float enemyTeleportStartX;

	public static bool showTotalValue = true;

	public static bool showPlayerStatus = true;

	private int totalValuableValue;

	public static string[] levelsToSearchItems = new string[6] { "Level - Manor", "Level - Wizard", "Level - Arctic", "Level - Shop", "Level - Lobby", "Level - Recording" };

	private GUIStyle menuStyle;

	private bool initialized;

	private static Dictionary<Color, Texture2D> solidTextures = new Dictionary<Color, Texture2D>();

	private bool showColorPicker;

	private int selectedColorOption;

	private MenuCategory currentCategory;

	public static float staminaRechargeDelay = 1f;

	public static float staminaRechargeRate = 1f;

	public static float oldStaminaRechargeDelay = 1f;

	public static float oldStaminaRechargeRate = 1f;

	private bool spoofNameEnabled;

	private string spoofTargetVisibleName = L.T("common.all");

	private bool spoofDropdownVisible;

	public static string spoofedNameText = "Text";

	public static string persistentNameText = "Text";

	private string originalSteamName = SteamClient.Name;

	public static bool spoofNameActive = false;

	private float lastSpoofTime;

	private const float NAME_SPOOF_DELAY = 4f;

	public static bool hasAlreadySpoofed = false;

	private string colorTargetVisibleName = L.T("common.all");

	private bool colorDropdownVisible;

	private string colorIndexText = "1";

	private bool showColorIndexDropdown;

	private Vector2 colorIndexScrollPosition = Vector2.zero;

	private System.Random rainbowRandom = new System.Random();

	private Dictionary<int, bool> playerRainbowStates = new Dictionary<int, bool>();

	private Dictionary<int, float> lastRainbowTimes = new Dictionary<int, float>();

	private const float PLAYER_RAINBOW_DELAY = 0.1f;

	private Dictionary<int, string> colorNameMapping = null;

	private Dictionary<int, string> GetColorNames()
	{
		if (colorNameMapping == null)
		{
			colorNameMapping = new Dictionary<int, string>();
			for (int i = 0; i <= 35; i++)
				colorNameMapping[i] = LanguageManager.GetColorName(i);
		}
		return colorNameMapping;
	}

	private int previousItemCount;

	public static float sliderValue = 1f;

	public static float sliderValueStrength = 1f;

	public static float jumpForce = 1f;

	public static float customGravity = 1f;

	public static int extraJumps = 0;

	public static float flashlightIntensity = 0f;

	public static float crouchDelay = 0f;

	public static float crouchSpeed = 0f;

	public static float grabRange = 1f;

	public static float tumbleLaunch = 0f;

	public static float throwStrength = 0f;

	public static float slideDecay = 0f;

	public static float oldSliderValue = 1f;

	public static float oldSliderValueStrength = 1f;

	public static float OldjumpForce = 1f;

	public static float OldcustomGravity = 1f;

	public static float OldextraJumps = 0f;

	public static float OldflashlightIntensity = 0f;

	public static float OldcrouchDelay = 0f;

	public static float OldcrouchSpeed = 0f;

	public static float OldgrabRange = 1f;

	public static float OldtumbleLaunch = 0f;

	public static float OldthrowStrength = 0f;

	public static float OldslideDecay = 0f;

	private List<ItemTeleport.GameItem> itemList = new List<ItemTeleport.GameItem>();

	private int selectedItemIndex;

	private Vector2 itemScrollPosition = Vector2.zero;

	private List<string> availableItemsList = new List<string>();

	private int selectedItemToSpawnIndex;

	private Vector2 itemSpawnerScrollPosition = Vector2.zero;

	private int itemSpawnValue = 45000;

	private bool isChangingItemValue;

	private float itemValueSliderPos = 4f;

	private bool showItemSpawner;

	private bool isHost;

	private HotkeyManager hotkeyManager;

	public static bool showWatermark = true;

	private float menuX = 100f;

	private Rect menuRect = new Rect(100f, 100f, 640f, 500f);

	private Vector2 scrollPos;

	private string[] tabs = LanguageManager.GetTabNames();

	private Vector2 waypointScrollPos = Vector2.zero;
	private int espPresetIndex = 0;

	private int currentTab;

	// Theme system
	private ThemeType currentTheme = ThemeType.Classic;
	private ThemeData activeTheme;

	public static Texture2D toggleBackground;

	public string configstatus = L.T("common.status_wait");

	private float toggleAnimationSpeed = 8f;

	private Dictionary<string, float> toggleAnimations = new Dictionary<string, float>();

	private bool isResizing;

	private Vector2 resizeStartMousePos;

	private Vector2 resizeStartSize;

	private GUIStyle titleStyle;

	private GUIStyle subtitleStyle;

	private GUIStyle tabStyle;

	private GUIStyle horizontalSliderStyle;

	private GUIStyle horizontalThumbStyle;

	private GUIStyle scrollbarStyle;

	private GUIStyle scrollbarThumbStyle;

	private GUIStyle tabSelectedStyle;

	private GUIStyle sectionHeaderStyle;

	private GUIStyle buttonStyle;

	private GUIStyle labelStyle;

	private GUIStyle backgroundStyle;

	private GUIStyle warningStyle;

	private GUIStyle boxStyle;

	private GUIStyle textFieldStyle;

	private GUIStyle verticalSliderStyle;

	private GUIStyle verticalThumbStyle;

	private Vector2 itemScroll;

	private Vector2 itemSpawnerScroll;

	private string itemSpawnSearch = "";

	private static bool showChamsWindow = false;

	private static Rect chamsWindowRect = new Rect(100f, 100f, 220f, 400f);

	public Texture2D toggleBgTexture;

	public Texture2D toggleKnobOffTexture;

	public Texture2D toggleKnobOnTexture;

	public static bool useModernESP = false;

	private static bool showingActionSelector = false;

	private static Rect featureSelectorRect = new Rect(200f, 200f, 400f, 400f);

	private static Vector2 actionScroll;

	private bool hideFullLobbies;

	private Vector2 memberWindowScroll;

	private bool showMemberWindow;

	private static Rect lobbyMemberWindowRect = new Rect(100f, 100f, 320f, 240f);

	private static SteamId selectedLobbyId = default(SteamId);

	public static Dictionary<SteamId, string> LobbyHostCache = new Dictionary<SteamId, string>();

	public static Dictionary<SteamId, List<string>> LobbyMemberCache = new Dictionary<SteamId, List<string>>();

	private string lobbySearchTerm = "";

	private SortMode sortMode;

	// 区域筛选
	private int regionFilterIndex = 0;
	private bool showRegionDropdown = false;
	private static readonly string[] regionFilterOptions = new string[]
	{
		"全部区域", "亚洲", "日本", "欧洲", "北美", "南美", "大洋洲", "非洲", "中东"
	};
	private static readonly string[][] regionFilterKeywords = new string[][]
	{
		new string[0], // 全部
		new string[] { "asia", "cn", "hk", "tw", "sg", "kr", "in" },
		new string[] { "jp", "japan" },
		new string[] { "eu", "europe", "uk", "de", "fr", "ru" },
		new string[] { "us", "na", "usw", "use", "ca" },
		new string[] { "sa", "br", "south america" },
		new string[] { "au", "oceania", "nz" },
		new string[] { "af", "africa" },
		new string[] { "me", "middle east", "uae" }
	};

	// 快速创建房间
	private string createRoomName = "";
	private int createRoomMaxPlayers = 6;
	private bool showCreateRoom = false;

	private static Vector2 serverListScroll;

	public static LobbyCoroutineHost CoroutineHost;

	// GUI 美化
	private static Texture2D tabHighlightTex;
	private static Texture2D sectionAccentTex;
	private static Texture2D sectionLineTex;

	private Rect previousWindowRect;

	private bool hasStoredPreviousSize;

	private bool wasInServerTab;

	private static Texture2D rowBgNormal;

	private static Texture2D rowBgHover;

	private static Texture2D rowBgSelected;

	private static Texture2D enemyBgSelected;
	private static Texture2D enemyBgNormal;
	private static Texture2D itemBgSelected;
	private static Texture2D itemBgNormal;
	private static GUIStyle cachedPlayerListStyle;
	private static GUIStyle cachedEnemyListStyle;
	private static GUIStyle cachedItemListStyle;
	private static GUIStyle cachedServerRowStyle;
	private static bool listStylesInitialized;
	private static List<Lobby> cachedSortedLobbies;
	private static SortMode cachedSortMode = (SortMode)(-1);
	private static int cachedLobbyCount = -1;

	public static bool grabThroughWallsEnabled = false;

	private bool showTextEditorPopup;

	private string largeTextBoxContent = "";

	private Rect editorPopupRect = new Rect(200f, 200f, 400f, 300f);

	private string activeTextFieldId;

	private static Vector2 textboxscroll;

	// 菜单设置页 fields
	private float menuBgR = 15f;
	private float menuBgG = 18f;
	private float menuBgB = 25f;
	private float menuBgA = 230f;
	private string customBgImagePath = "";
	private Texture2D customBgTexture;
	private bool confirmUnload = false;
	private bool confirmQuit = false;
	private float confirmTimer = 0f;

	// 语言下拉 fields
	private bool showLangDropdown = false;
	private Vector2 langDropdownScroll;

	// ===== Admin 标签页 fields =====
	private int adminTargetIndex = 0;
	private string[] adminTargetOptions = new string[0];
	private bool showAdminTargetDropdown = false;
	private Vector2 adminTargetScroll;
	private float adminGrabStrength = 0f;
	private float adminThrowStrength = 0f;
	private float adminSprintSpeed = 0f;
	private float adminGrabRange = 0f;
	private float adminExtraJump = 0f;
	private float adminTumbleLaunch = 0f;
	private bool adminInvincibilityAura = false;
	private int adminAuraTarget = -1;

	// ===== 假玩家 fields =====
	private string fakePlayerName = "Bot";
	private float fakePlayerCount = 1f;
	private bool fakePlayerUse3D = false;

	// ===== 商店 fields =====
	private bool shopFreeEnabled = false;
	private float moneySpawnAmount = 45000f;

	// ===== 反踢 fields =====
	private bool antiKickEnabled = false;

	// ===== 娱乐 Tab fields =====
	private int asciiArtIndex = 0;
	private bool showAsciiDropdown = false;
	private string asciiTargetName = "All";
	private bool asciiTargetDropdown = false;
	private int levelAdjustIndex = 0;
	private bool showLevelAdjustDropdown = false;
	private Vector2 levelAdjustScroll;

	// ===== 敌人速度 fields =====
	private float enemySpeedMult = 1.0f;

	// ===== 升级强制同步 =====
	private float _upgradeEnforceTimer = 0f;
	private const float UPGRADE_ENFORCE_INTERVAL = 8f; // 每8秒强制重新应用本地升级

	private void CheckIfHost()
	{
		isHost = !SemiFunc.IsMultiplayer() || PhotonNetwork.IsMasterClient;
	}

	private void UpdateTeleportOptions()
	{
		List<string> list = new List<string>();
		list.Add(L.T("common.all_players"));
		list.AddRange(playerNames);
		teleportPlayerSourceOptions = list.ToArray();
		List<string> list2 = new List<string>();
		list2.AddRange(playerNames);
		list2.Add(L.T("common.void"));
		teleportPlayerDestOptions = list2.ToArray();
		teleportPlayerSourceIndex = 0;
		teleportPlayerDestIndex = teleportPlayerDestOptions.Length - 1;
	}

	private void UpdateEnemyTeleportOptions()
	{
		List<string> list = new List<string>();
		list.AddRange(playerNames);
		enemyTeleportDestOptions = list.ToArray();
		enemyTeleportDestIndex = 0;
		float num = menuX + 300f;
		enemyTeleportTotalWidth = enemyTeleportLabelWidth + 10f + enemyTeleportToWidth + 10f + enemyTeleportDropdownWidth;
		enemyTeleportStartX = num - enemyTeleportTotalWidth / 2f;
	}

	private void CheckForLevelChange()
	{
		_ = Time.time;
		string text = (((Object)(object)RunManager.instance.levelCurrent != (Object)null) ? ((Object)RunManager.instance.levelCurrent).name : "");
		if (text != previousLevelName && !string.IsNullOrEmpty(text) && !pendingLevelUpdate)
		{
			previousLevelName = text;
			pendingLevelUpdate = true;
			levelChangeDetectedTime = Time.time;
			showSourceDropdown = false;
			showDestDropdown = false;
			showEnemyTeleportDropdown = false;
		}
		if (pendingLevelUpdate && Time.time >= levelChangeDetectedTime + 3f)
		{
			pendingLevelUpdate = false;
			PerformDelayedLevelUpdate();
		}
	}

	private void PerformDelayedLevelUpdate()
	{
		PlayerReflectionCache.CachePlayerControllerData();
		UpdatePlayerList();
		UpdateEnemyList();
		if (showTeleportUI)
		{
			UpdateTeleportOptions();
		}
		if (showEnemyTeleportUI)
		{
			UpdateEnemyTeleportOptions();
		}
		if (!hasInitializedDefaults)
		{
			PlayerController.LoadDefaultStatsIntoHax2();
			hasInitializedDefaults = true;
		}
		PlayerController.ReapplyAllStats();
	}

	public void Start()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		hotkeyManager = HotkeyManager.Instance;
		availableItemsList = ItemSpawner.GetAvailableItems();
		if ((Object)(object)unlimitedBatteryComponent == (Object)null)
		{
			GameObject val = new GameObject("BatteryManager");
			unlimitedBatteryComponent = val.AddComponent<UnlimitedBattery>();
			Object.DontDestroyOnLoad((Object)val);
		}
		DebugCheats.texture2 = new Texture2D(2, 2, (TextureFormat)5, false);
		DebugCheats.texture2.SetPixels((Color[])(object)new Color[4]
		{
			Color.red,
			Color.red,
			Color.red,
			Color.red
		});
		DebugCheats.texture2.Apply();
		Type type = Type.GetType("PlayerHealth, Assembly-CSharp");
		if (type != null)
		{
			Players.playerHealthInstance = Object.FindObjectOfType(type);
		}
		Type type2 = Type.GetType("ItemUpgradePlayerHealth, Assembly-CSharp");
		if (type2 != null)
		{
			Players.playerMaxHealthInstance = Object.FindObjectOfType(type2);
		}
		// Toggle textures are now generated programmatically in DrawCustomToggle
		// No external PNG files needed
		ConfigManager.LoadAllToggles();
		GameObject val2 = new GameObject("LobbyCoroutineHost");
		Object.DontDestroyOnLoad((Object)val2);
		CoroutineHost = val2.AddComponent<LobbyCoroutineHost>();
	}

	public void Update()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		CheckIfHost();
		levelCheckTimer += Time.deltaTime;
		if (levelCheckTimer >= 5f)
		{
			levelCheckTimer = 0f;
			CheckForLevelChange();
		}
		if (Input.GetKeyDown(hotkeyManager.MenuToggleKey))
		{
			_isExpanding = !_isExpanding;
			if (_isExpanding)
			{
				showMenu = true;
				CursorController.cheatMenuOpen = true;
			}
			UpdateCursorState();
		}

		// Smooth ease-out animation
		float target = _isExpanding ? 1f : 0f;
		_animProgress = Mathf.MoveTowards(_animProgress, target, Time.unscaledDeltaTime * ANIM_SPEED);
		// Snap to endpoints
		if (Mathf.Abs(_animProgress - target) < 0.005f) _animProgress = target;

		if (!_isExpanding && _animProgress <= 0f)
		{
			if (showMenu)
			{
				showMenu = false;
				CursorController.cheatMenuOpen = false;
				TryUnlockCamera();
				UpdateCursorState();
			}
		}
		
		if (Input.GetKeyDown(hotkeyManager.ReloadKey))
		{
			Start();
		}
		if (Input.GetKeyDown(hotkeyManager.UnloadKey))
		{
			showMenu = false;
			CursorController.cheatMenuOpen = showMenu;
			CursorController.UpdateCursorState();
			TryUnlockCamera();
			UpdateCursorState();
			Loader.UnloadCheat();
		}
		if (hotkeyManager.ConfiguringHotkey)
		{
			foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
			{
				if (Input.GetKeyDown(value))
				{
					hotkeyManager.ProcessHotkeyConfiguration(value);
					break;
				}
			}
		}
		else if (hotkeyManager.ConfiguringSystemKey)
		{
			foreach (KeyCode value2 in Enum.GetValues(typeof(KeyCode)))
			{
				if (Input.GetKeyDown(value2))
				{
					hotkeyManager.ProcessSystemKeyConfiguration(value2);
					break;
				}
			}
		}
		if ((Object)(object)RunManager.instance != (Object)null && (Object)(object)RunManager.instance.levelCurrent != (Object)null && levelsToSearchItems.Contains(((Object)RunManager.instance.levelCurrent).name))
		{
			if (Time.time >= nextUpdateTime)
			{
				UpdateEnemyList();
				UpdateItemList();
				itemList = ItemTeleport.GetItemList();
				nextUpdateTime = Time.time + 3f;
			}
			if (playerColor.isRandomizing)
			{
				playerColor.colorRandomizer();
			}
			hotkeyManager.CheckAndExecuteHotkeys();
			if (showMenu)
			{
				TryLockCamera();
			}
			if (NoclipController.noclipActive)
			{
				NoclipController.UpdateMovement();
			}
			AutoPickup.UpdateAutoPickup();
			AutoDodge.UpdateAutoDodge();
			CreativeMode.UpdateCreativeFlight();
			PlayerPossession.UpdatePossession();
			Enemies.UpdateFreeze();
			Enemies.UpdateSpeedModify();
			PlayerAura.Update();
			Aimbot.UpdateAimbot();
			AutoPilot.UpdateAutoPilot();
			ScaleSync.Update();
		}
		// 定期强制重新应用升级值（防止主机覆盖）
		_upgradeEnforceTimer += Time.deltaTime;
		if (_upgradeEnforceTimer >= UPGRADE_ENFORCE_INTERVAL)
		{
			_upgradeEnforceTimer = 0f;
			try
			{
				UpgradeHelper.ApplyLocalPhysicsUpgrades(
					Mathf.RoundToInt(sliderValueStrength),
					Mathf.RoundToInt(sliderValue),
					extraJumps);
			}
			catch { }
		}
		foreach (KeyValuePair<int, bool> playerRainbowState in playerRainbowStates)
		{
			int key = playerRainbowState.Key;
			if (playerRainbowState.Value)
			{
				int colorIndex = rainbowRandom.Next(0, 36);
				string targetName = colorTargetVisibleName;
				if (!lastRainbowTimes.ContainsKey(key))
				{
					lastRainbowTimes[key] = Time.time;
				}
				if (Time.time - lastRainbowTimes[key] >= 0.1f)
				{
					ChatHijack.ChangePlayerColor(colorIndex, targetName, playerList, playerNames);
					lastRainbowTimes[key] = Time.time;
				}
			}
		}
	}

	public void LateUpdate()
	{
		ThirdPersonCamera.LateUpdateCamera();
		PlayerPossession.LateUpdatePossession();
	}

	private void TryLockCamera()
	{
		if (!((Object)(object)InputManager.instance != (Object)null))
		{
			return;
		}
		FieldInfo field = typeof(InputManager).GetField("disableAimingTimer", BindingFlags.Instance | BindingFlags.NonPublic);
		if (field != null)
		{
			float num = (float)field.GetValue(InputManager.instance);
			if (num < 2f || num > 10f)
			{
				float num2 = Mathf.Clamp(num, 2f, 10f);
				field.SetValue(InputManager.instance, num2);
			}
		}
	}

	private void TryUnlockCamera()
	{
		if ((Object)(object)InputManager.instance != (Object)null)
		{
			FieldInfo field = typeof(InputManager).GetField("disableAimingTimer", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field != null && (float)field.GetValue(InputManager.instance) > 0f)
			{
				field.SetValue(InputManager.instance, 0f);
			}
		}
	}

	private void UpdateCursorState()
	{
		Cursor.visible = showMenu;
		CursorController.cheatMenuOpen = showMenu;
		CursorController.UpdateCursorState();
	}

	private void UpdateItemList()
	{
		DebugCheats.valuableObjects.Clear();
		totalValuableValue = 0;
		Object[] array = Object.FindObjectsOfType(Type.GetType("ValuableObject, Assembly-CSharp"));
		if (array != null)
		{
			Object[] array2 = array;
			foreach (Object val in array2)
			{
				DebugCheats.valuableObjects.Add(val);
				FieldInfo fieldInfo = ((object)val).GetType().GetField("dollarValueCurrent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? ((object)val).GetType().GetField("dollarValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (fieldInfo != null)
				{
					object value = fieldInfo.GetValue(val);
					if (value is int num)
					{
						totalValuableValue += num;
					}
					else if (value is float num2)
					{
						totalValuableValue += Mathf.RoundToInt(num2);
					}
					else
					{
						Debug.LogWarning((object)("[Valuable] Unknown value type: " + value?.GetType()?.Name));
					}
				}
			}
		}
		Object[] array3 = Object.FindObjectsOfType(Type.GetType("PlayerDeathHead, Assembly-CSharp"));
		if (array3 != null)
		{
			DebugCheats.valuableObjects.AddRange(array3);
		}
		itemList = ItemTeleport.GetItemList();
		if (itemList.Count != previousItemCount)
		{
			previousItemCount = itemList.Count;
		}
	}

	private void UpdateEnemyList()
	{
		enemyNames.Clear();
		enemyList.Clear();
		DebugCheats.UpdateEnemyList();
		enemyList = DebugCheats.enemyList;
		foreach (Enemy enemy in enemyList)
		{
			if ((Object)(object)enemy != (Object)null && ((Component)enemy).gameObject.activeInHierarchy)
			{
				string text = L.T("common.enemy");
				Component componentInParent = ((Component)enemy).GetComponentInParent(Type.GetType("EnemyParent, Assembly-CSharp"));
				if ((Object)(object)componentInParent != (Object)null)
				{
					text = (((object)componentInParent).GetType().GetField("enemyName", BindingFlags.Instance | BindingFlags.Public)?.GetValue(componentInParent) as string) ?? L.T("common.enemy");
				}
				int enemyHealth = Enemies.GetEnemyHealth(enemy);
				DebugCheats.enemyHealthCache[enemy] = enemyHealth;
				int enemyMaxHealth = Enemies.GetEnemyMaxHealth(enemy);
				float num = ((enemyMaxHealth > 0) ? ((float)enemyHealth / (float)enemyMaxHealth) : 0f);
				string arg = ((num > 0.66f) ? "<color=green>" : ((num > 0.33f) ? "<color=yellow>" : "<color=red>"));
				string text2 = ((enemyHealth >= 0) ? $"{arg}HP: {enemyHealth}/{enemyMaxHealth}</color>" : "<color=gray>" + L.T("common.hp_unknown") + "</color>");
				enemyNames.Add(text + " [" + text2 + "]");
			}
		}
		if (enemyNames.Count == 0)
		{
			enemyNames.Add(L.T("common.no_enemies"));
		}
	}

	private void UpdatePlayerList()
	{
		playerNames.Clear();
		playerList.Clear();
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			playerList.Add(item);
			string text = SemiFunc.PlayerGetName(item) ?? L.T("common.unknown_player");
			string text2 = (IsPlayerAlive(item, text) ? "<color=green>" + L.T("common.alive") + "</color> " : "<color=red>" + L.T("common.dead") + "</color> ");
			playerNames.Add(text2 + text);
		}
		// 添加假人条目 (使用 FakePlayerManager 的列表)
		for (int num = 0; num < FakePlayerManager.fakePlayerNames.Count; num++)
		{
			playerNames.Add("<color=yellow>[BOT]</color> " + FakePlayerManager.fakePlayerNames[num]);
			playerList.Add(null);
		}
		if (playerNames.Count == 0)
		{
			playerNames.Add(L.T("common.no_players"));
		}
	}

	private bool IsPlayerAlive(object player, string playerName)
	{
		int playerHealth = Players.GetPlayerHealth(player);
		if (playerHealth < 0)
		{
			return true;
		}
		return playerHealth > 0;
	}

	private void OnGUI()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Expected O, but got Unknown
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Expected O, but got Unknown
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Expected O, but got Unknown
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_044a: Unknown result type (might be due to invalid IL or missing references)
		if (activeTheme == null) activeTheme = ThemeData.GetTheme(currentTheme);
		if (!_uiStylesInited) { UIStyles.Init(activeTheme); _uiStylesInited = true; }
		InitStyles();

		// Dynamic Island: smoothstep interpolation
		float t = _animProgress * _animProgress * (3f - 2f * _animProgress);
		float curW = Mathf.Lerp(PILL_WIDTH, MENU_WIDTH, t);
		float curH = Mathf.Lerp(PILL_HEIGHT, MENU_HEIGHT, t);
		// Calculate centered position, then apply drag offset
		float centeredX = (Screen.width - curW) / 2f;
		float centeredY = 20f;
		if (!_hasDragged)
		{
			menuRect.x = centeredX;
			menuRect.y = centeredY;
		}
		else
		{
			// When collapsing back to pill, smoothly return to center
			if (_animProgress < 0.5f)
			{
				menuRect.x = Mathf.Lerp(centeredX, menuRect.x, _animProgress * 2f);
				menuRect.y = Mathf.Lerp(centeredY, menuRect.y, _animProgress * 2f);
				if (_animProgress <= 0f) _hasDragged = false;
			}
		}
		menuRect.width = curW;
		menuRect.height = curH;

		// === Always-visible pill when menu is closed ===
		if (_pillAlwaysShow && _animProgress <= 0f && !_isExpanding)
		{
			Rect pillRect = new Rect((Screen.width - PILL_WIDTH) / 2f, 20f, PILL_WIDTH, PILL_HEIGHT);
			GUIStyle pillStyle = UIStyles.GetPillStyle() ?? GUI.skin.box;
			GUI.Box(pillRect, GUIContent.none, pillStyle);
			// Title
			GUIStyle pillLabel = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 13, fontStyle = FontStyle.Bold };
			pillLabel.normal.textColor = activeTheme != null ? activeTheme.pillText : Color.white;
			GUI.Label(new Rect(pillRect.x, pillRect.y, pillRect.width * 0.5f, pillRect.height), "R.E.P.O", pillLabel);
			// Active cheat dots
			var activeNames = new System.Collections.Generic.List<string>();
			if (godModeActive) activeNames.Add("GOD");
			if (stamineState) activeNames.Add("STA");
			if (DebugCheats.drawEspBool) activeNames.Add("ESP");
			if (blindEnemies) activeNames.Add("BLD");
			if (NoclipController.noclipActive) activeNames.Add("FLY");
			if (activeNames.Count > 0)
			{
				GUIStyle dotStyle = new GUIStyle(GUI.skin.label) { fontSize = 9, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
				dotStyle.normal.textColor = activeTheme != null ? activeTheme.pillDotColor : new Color(0.04f, 0.52f, 1f);
				string dotText = string.Join(" \u2022 ", activeNames.ToArray());
				GUI.Label(new Rect(pillRect.x + pillRect.width * 0.45f, pillRect.y, pillRect.width * 0.55f, pillRect.height), dotText, dotStyle);
			}
			// Click pill to expand
			if (Event.current.type == EventType.MouseDown && pillRect.Contains(Event.current.mousePosition))
			{
				_isExpanding = true;
				showMenu = true;
				CursorController.cheatMenuOpen = true;
				UpdateCursorState();
				Event.current.Use();
			}
		}

		if (_animProgress > 0f || _isExpanding)
		{
			var originalColor = GUI.color;
			// Use pill style when mostly collapsed, window style when mostly open
			GUIStyle winStyle = (_animProgress < 0.5f) ? UIStyles.GetPillStyle() : UIStyles.GetWindowStyle();
			var prevRect = menuRect;
			menuRect = GUI.Window(0, menuRect, new WindowFunction(DrawMenuWindow), "", winStyle);
			// Track if user has dragged the window
			if (Mathf.Abs(menuRect.x - prevRect.x) > 1f || Mathf.Abs(menuRect.y - prevRect.y) > 1f)
				_hasDragged = true;

			HandleResize();
			if (showChamsWindow)
			{
				chamsWindowRect = GUI.Window(10001, chamsWindowRect, new WindowFunction(DrawChamsColorWindow), "", backgroundStyle);
			}
			if (showingActionSelector)
			{
				GUI.Box(featureSelectorRect, "", boxStyle);
				featureSelectorRect = GUI.Window(9999, featureSelectorRect, new WindowFunction(DrawFeatureSelectionWindow), "", boxStyle);
			}
			if (showMemberWindow && selectedLobbyId.Value != 0uL)
			{
				lobbyMemberWindowRect = GUI.Window(1299, lobbyMemberWindowRect, new WindowFunction(DrawLobbyMemberWindow), GUIContent.none, backgroundStyle);
			}
			if (showTextEditorPopup)
			{
				editorPopupRect = GUI.Window(9989, editorPopupRect, new WindowFunction(DrawTextEditorPopup), "", backgroundStyle);
			}
			GUI.color = originalColor;
		}
		if (useModernESP)
		{
			if (DebugCheats.drawEspBool || DebugCheats.drawItemEspBool || DebugCheats.drawExtractionPointEspBool || DebugCheats.drawPlayerEspBool)
			{
				try { ModernESP.Render(); } catch (System.Exception ex) { Debug.LogWarning((object)("[ESP] ModernESP error: " + ex.Message)); }
			}
		}
		else if (DebugCheats.drawEspBool || DebugCheats.drawItemEspBool || DebugCheats.drawExtractionPointEspBool || DebugCheats.drawPlayerEspBool)
		{
			try { DebugCheats.DrawESP(); } catch (System.Exception ex) { Debug.LogWarning((object)("[ESP] DrawESP error: " + ex.Message)); }
			ModernESP.ClearItemLabels();
			ModernESP.ClearEnemyLabels();
		}
		// ESP增强：追踪线
		try { ESPEnhancements.DrawTraceLines(); } catch (System.Exception ex) { Debug.LogWarning((object)("[ESP] TraceLine error: " + ex.Message)); }
		// 小地图雷达
		MiniRadar.DrawRadar();
		GUIStyle val = new GUIStyle(GUI.skin.label)
		{
			wordWrap = false
		};
		if (showWatermark)
		{
			GUIContent val2 = new GUIContent(L.T("watermark.main_fmt", hotkeyManager.MenuToggleKey));
			Vector2 val3 = val.CalcSize(val2);
			GUI.Label(new Rect(10f, 10f, val3.x, val3.y), val2, val);
			GUI.Label(new Rect(10f + val3.x + 10f, 10f, 300f, val3.y), L.T("watermark.credit"), val);
		}
		GUIStyle val4 = new GUIStyle(GUI.skin.label);
		val4.fontSize = 16;
		val4.fontStyle = (FontStyle)1;
		val4.normal.textColor = Color.white;
		float num = 20f;
		float num2 = 250f;
		float num3 = 24f;
		int num4 = 0;
		if (DebugCheats.drawPlayerEspBool && showPlayerStatus)
		{
			List<PlayerAvatar> list = SemiFunc.PlayerGetList();
			if (list == null)
			{
				return;
			}
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			foreach (PlayerAvatar item in list)
			{
				string text = SemiFunc.PlayerGetName(item) ?? L.T("server.unknown");
				FieldInfo field = ((object)item).GetType().GetField("isDisabled", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field != null)
				{
					if ((bool)field.GetValue(item))
					{
						list3.Add(text);
					}
					else
					{
						list2.Add(text);
					}
				}
				else
				{
					Debug.LogWarning((object)("[DeathCheck] 'isDisabled' field not found on player: " + text));
				}
			}
			GUI.color = Color.green;
			GUI.Label(new Rect(num, num2 + (float)num4++ * num3, 400f, num3), L.T("common.alive_list"), val4);
			foreach (string item2 in list2)
			{
				string text2 = "- " + item2;
				GUI.Label(new Rect(num, num2 + (float)num4++ * num3, 400f, num3), text2);
			}
			num4++;
			GUI.color = Color.red;
			GUI.Label(new Rect(num, num2 + (float)num4++ * num3, 400f, num3), L.T("common.dead_list"), val4);
			foreach (string item3 in list3)
			{
				string text3 = "- " + item3;
				GUI.Label(new Rect(num, num2 + (float)num4++ * num3, 400f, num3), text3);
			}
			GUI.color = Color.white;
		}
		num4++;
		num4++;
		num4++;
		if (DebugCheats.drawItemEspBool && showTotalValue)
		{
			GUI.color = Color.yellow;
			GUI.Label(new Rect(num, num2 + (float)num4++ * num3, 400f, num3), L.T("common.map_value_fmt", totalValuableValue), val4);
			GUI.color = Color.white;
			num4++;
		}
		// ─── Toast 通知系统渲染 ───
		ToastNotification.DrawToasts();
	}

	private bool DrawCustomToggle(string id, bool state)
	{
		float trackW = 44f;
		float trackH = 24f;
		float knobSize = 20f;
		float pad = (trackH - knobSize) / 2f;
		GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GUILayout.Width(trackW),
			GUILayout.Height(trackH)
		});
		Rect rect = GUILayoutUtility.GetRect(trackW, trackH);
		GUILayout.EndVertical();
		if (!toggleAnimations.TryGetValue(id, out var anim))
		{
			anim = (state ? 1f : 0f);
			toggleAnimations[id] = anim;
		}
		float target = (state ? 1f : 0f);
		if ((int)Event.current.type == 7 && !Mathf.Approximately(anim, target))
		{
			anim = Mathf.MoveTowards(anim, target, Time.deltaTime * toggleAnimationSpeed);
			toggleAnimations[id] = anim;
		}
		Color oldColor = GUI.color;
		// Draw rounded track using pre-generated textures
		if (UIStyles.ToggleTrackOff != null && UIStyles.ToggleTrackOn != null)
		{
			GUI.color = Color.white;
			// Draw off track as base
			GUI.DrawTexture(new Rect(rect.x, rect.y, trackW, trackH), UIStyles.ToggleTrackOff, ScaleMode.StretchToFill);
			// Blend on track
			GUI.color = new Color(1f, 1f, 1f, anim);
			GUI.DrawTexture(new Rect(rect.x, rect.y, trackW, trackH), UIStyles.ToggleTrackOn, ScaleMode.StretchToFill);
		}
		else
		{
			Color trackOff = activeTheme != null ? activeTheme.toggleTrackOff : new Color(0.22f, 0.22f, 0.25f, 1f);
			Color trackOn = activeTheme != null ? activeTheme.toggleTrackOn : new Color(0.1f, 0.55f, 0.2f, 1f);
			GUI.color = Color.Lerp(trackOff, trackOn, anim);
			GUI.DrawTexture(new Rect(rect.x, rect.y, trackW, trackH), Texture2D.whiteTexture);
		}
		// Draw rounded knob with shadow effect
		float travel = trackW - knobSize - pad * 2f;
		float knobX = rect.x + pad + travel * anim;
		GUI.color = new Color(0f, 0f, 0f, 0.25f);
		Texture knobTex = (UIStyles.ToggleKnob != null) ? (Texture)UIStyles.ToggleKnob : (Texture)Texture2D.whiteTexture;
		GUI.DrawTexture(new Rect(knobX + 0.5f, rect.y + pad + 1f, knobSize, knobSize), knobTex);
		GUI.color = activeTheme != null ? activeTheme.toggleKnob : Color.white;
		GUI.DrawTexture(new Rect(knobX, rect.y + pad, knobSize, knobSize), knobTex);
		GUI.color = oldColor;
		if ((int)Event.current.type == 0 && rect.Contains(Event.current.mousePosition))
		{
			Event.current.Use();
			return !state;
		}
		return state;
	}

	private void DrawSectionHeader(string text)
	{
		if (sectionAccentTex == null)
			sectionAccentTex = MakeSolidBackground(activeTheme != null ? activeTheme.sectionAccentColor : new Color(1f, 0.55f, 0f, 1f));
		if (sectionLineTex == null)
			sectionLineTex = MakeSolidBackground(activeTheme != null ? activeTheme.sectionLineColor : new Color(1f, 0.55f, 0f, 0.3f));

		GUILayout.Space(10f);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		// Left accent bar (rounded via texture)
		GUILayout.Box("", (GUILayoutOption[])(object)new GUILayoutOption[2] { GUILayout.Width(3f), GUILayout.Height(18f) });
		Rect lastBox = GUILayoutUtility.GetLastRect();
		GUI.DrawTexture(lastBox, sectionAccentTex);
		GUILayout.Space(8f);
		// Section title - uppercase for macOS feel
		GUIStyle headerStyle = new GUIStyle(sectionHeaderStyle);
		headerStyle.fontSize = 12;
		headerStyle.fontStyle = FontStyle.Bold;
		GUILayout.Label(text.ToUpper(), headerStyle, Array.Empty<GUILayoutOption>());
		GUILayout.EndHorizontal();
		GUILayout.Space(2f);
		// Subtle separator line
		Rect lineRect = GUILayoutUtility.GetLastRect();
		GUI.DrawTexture(new Rect(lineRect.x, lineRect.yMax, lineRect.width, 1f), sectionLineTex);
		GUILayout.Space(6f);
	}

	private bool ToggleLogic(string id, string label, ref bool value, Action onToggle = null)
	{
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(label, labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(250f) });
		bool flag = DrawCustomToggle(id, value);
		if (flag != value)
		{
			value = flag;
			ToastNotification.ShowToggle(label, flag);
			onToggle?.Invoke();
		}
		GUILayout.EndHorizontal();
		return value;
	}

	private void DrawMenuWindow(int windowID)
	{
		// === Dynamic Island: Collapsed Pill State ===
		if (_animProgress < 0.82f)
		{
			float pillT = _animProgress / 0.82f; // 0..1 within pill phase
			
			// Title text - centered, scales slightly with expansion
			GUIStyle pillLabel = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleCenter,
				fontSize = (int)Mathf.Lerp(14f, 20f, pillT),
				fontStyle = FontStyle.Bold
			};
			pillLabel.normal.textColor = activeTheme != null ? activeTheme.pillText : Color.white;

			// Draw title in upper portion
			float titleH = Mathf.Min(PILL_HEIGHT, menuRect.height * 0.5f);
			GUI.Label(new Rect(0, 0, menuRect.width, titleH), "R.E.P.O", pillLabel);

			// Status indicators with rounded dots (only when pill has some height)
			if (menuRect.height > PILL_HEIGHT + 8f)
			{
				Color dotColor = activeTheme != null ? activeTheme.pillDotColor : new Color(0.04f, 0.52f, 1f);
				var statusItems = new System.Collections.Generic.List<string>();
				if (godModeActive) statusItems.Add("GOD");
				if (stamineState) statusItems.Add("STA");
				if (DebugCheats.drawEspBool) statusItems.Add("ESP");
				if (blindEnemies) statusItems.Add("BLD");

				if (statusItems.Count > 0)
				{
					GUIStyle statusStyle = new GUIStyle(GUI.skin.label)
					{
						fontSize = 10,
						alignment = TextAnchor.MiddleCenter,
						fontStyle = FontStyle.Bold
					};
					statusStyle.normal.textColor = dotColor;
					string statusText = string.Join("  \u2022  ", statusItems.ToArray());
					GUI.Label(new Rect(0, titleH, menuRect.width, 20f), statusText, statusStyle);
				}
			}
			else
			{
				// Tiny dots on the pill itself
				int dots = 0;
				if (godModeActive) dots++;
				if (stamineState) dots++;
				if (DebugCheats.drawEspBool) dots++;
				if (dots > 0 && UIStyles.ToggleKnob != null)
				{
					float ds = 5f, sp = 9f;
					float sx = (menuRect.width - dots * sp) / 2f;
					var pc = GUI.color;
					GUI.color = activeTheme != null ? activeTheme.pillDotColor : new Color(0.04f, 0.52f, 1f);
					for (int d = 0; d < dots; d++)
						GUI.DrawTexture(new Rect(sx + d * sp, menuRect.height - 10f, ds, ds), UIStyles.ToggleKnob);
					GUI.color = pc;
				}
			}

			GUI.DragWindow();
			return;
		}

		// === Transition: fade in full menu content ===
		float contentAlpha = Mathf.Clamp01((_animProgress - 0.82f) / 0.18f);
		contentAlpha = contentAlpha * contentAlpha; // ease-in for smooth fade
		var _savedColor = GUI.color;
		GUI.color = new Color(_savedColor.r, _savedColor.g, _savedColor.b, _savedColor.a * contentAlpha);

		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (currentTab == 9 && !wasInServerTab)
		{
			if (!hasStoredPreviousSize)
			{
				previousWindowRect = menuRect;
				hasStoredPreviousSize = true;
			}
			menuRect.width = 1200f;
			menuRect.height = 765f;
			wasInServerTab = true;
		}
		else if (currentTab != 9 && wasInServerTab)
		{
			if (hasStoredPreviousSize)
			{
				menuRect = previousWindowRect;
			}
			wasInServerTab = false;
		}
		GUI.DragWindow(new Rect(0f, 0f, menuRect.width, 30f));
		float sidebarW = 140f;
		float gap = 12f;
		float contentW = Mathf.Max(200f, menuRect.width - sidebarW - gap * 3f);

		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		GUILayout.Space(8f);

		// Title bar - centered, clean
		GUILayout.Label(L.T("menu.title"), titleStyle, Array.Empty<GUILayoutOption>());
		// QQ 群信息
		if (subtitleStyle == null)
		{
			subtitleStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = 11,
				alignment = TextAnchor.MiddleCenter,
				fontStyle = FontStyle.Italic
			};
			subtitleStyle.normal.textColor = new Color(0.6f, 0.75f, 1f, 0.85f);
		}
		GUILayout.Label(L.T("menu.qq_group"), subtitleStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Space(4f);

		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Space(gap);

		// === Sidebar ===
		GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(sidebarW) });
		for (int i = 0; i < tabs.Length; i++)
		{
			GUIStyle tStyle = (i == currentTab) ? tabSelectedStyle : tabStyle;
			if (GUILayout.Button(tabs[i], tStyle, Array.Empty<GUILayoutOption>()))
			{
				currentTab = i;
			}
			// Selected indicator - left accent bar instead of bottom line (macOS sidebar style)
			if (i == currentTab)
			{
				Rect lastRect = GUILayoutUtility.GetLastRect();
				if (tabHighlightTex == null)
				{
					tabHighlightTex = MakeSolidBackground(activeTheme != null ? activeTheme.tabHighlightColor : new Color(1f, 0.5f, 0f, 1f));
				}
				// Left accent bar (3px wide, pill-like)
				GUI.DrawTexture(new Rect(lastRect.x, lastRect.y + 4f, 3f, lastRect.height - 8f), tabHighlightTex);
			}
		}
		GUILayout.EndVertical();

		GUILayout.Space(gap);

		// === Content area ===
		GUILayout.BeginVertical(boxStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(contentW) });
		scrollPos = GUILayout.BeginScrollView(scrollPos, Array.Empty<GUILayoutOption>());
		switch (currentTab)
		{
		case 0:
			DrawSelfTab();
			break;
		case 1:
			DrawVisualsTab();
			break;
		case 2:
			DrawCombatTab();
			break;
		case 3:
			DrawFunTab();
			break;
		case 4:
			DrawEnemiesTab();
			break;
		case 5:
			DrawItemsTab();
			break;
		case 6:
			DrawHotkeysTab();
			break;
		case 7:
			DrawRoomTab();
			break;
		case 8:
			DrawConfigTab();
			break;
		case 9:
			DrawServersTab();
			break;
		case 10:
			DrawTeleportTab();
			break;
		case 11:
			DrawMenuSettingsTab();
			break;
		case 12:
			DrawAdminTab();
			break;
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();

		// Restore GUI.color after content fade
		GUI.color = _savedColor;
	}

	private void DrawSelfTab()
	{
		//IL_0c4d: Unknown result type (might be due to invalid IL or missing references)
		DrawSectionHeader(L.T("self.health"));
		ToggleLogic("god_mode", L.T("self.god_mode"), ref godModeActive, () => PlayerController.SetGodMode(godModeActive));
		GUILayout.Space(5f);
		ToggleLogic("inf_health", L.T("self.inf_health"), ref infiniteHealthActive, PlayerController.MaxHealth);
		GUILayout.Space(5f);
		if (GUILayout.Button(L.T("self.revive_self"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			Players.ReviveSelf();
		}
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("self.movement"));
		ToggleLogic("No_Clip", L.T("self.noclip"), ref NoclipController.noclipActive, NoclipController.ToggleNoclip);
		GUILayout.Space(5f);
		ToggleLogic("inf_stam", L.T("self.inf_stamina"), ref stamineState, PlayerController.MaxStamina);
		GUILayout.Space(5f);
		ToggleLogic("auto_dodge", L.T("self.auto_dodge") + " [传送]", ref AutoDodge.isAutoDodgeEnabled);
		if (AutoDodge.isAutoDodgeEnabled)
		{
			GUILayout.Label(L.T("self.dodge_dist_fmt", Mathf.RoundToInt(AutoDodge.dodgeDistance)) + " (Animator检测)", labelStyle, Array.Empty<GUILayoutOption>());
			AutoDodge.dodgeDistance = GUILayout.HorizontalSlider(AutoDodge.dodgeDistance, 3f, 20f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
			GUILayout.Label($"传送距离: {AutoDodge.teleportDistance:F0}m", labelStyle, Array.Empty<GUILayoutOption>());
			AutoDodge.teleportDistance = GUILayout.HorizontalSlider(AutoDodge.teleportDistance, 2f, 15f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
			if (AutoDodge.dodgeCount > 0)
				GUILayout.Label($"闪避次数: {AutoDodge.dodgeCount} | {AutoDodge.lastDodgeInfo}", labelStyle, Array.Empty<GUILayoutOption>());
		}
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("self.misc"));
		ToggleLogic("rgb_player", L.T("self.rgb_player"), ref playerColor.isRandomizing,
			() => { if (!playerColor.isRandomizing) playerColor.RestoreOriginalColor(); });
		GUILayout.Space(5f);
		ToggleLogic("No_Fog", L.T("self.no_fog"), ref MiscFeatures.NoFogEnabled, MiscFeatures.ToggleNoFog);
		GUILayout.Space(5f);
		ToggleLogic("WaterMark_Toggle", L.T("self.watermark"), ref showWatermark);
		GUILayout.Space(5f);
		ToggleLogic("Grab_Guard", L.T("self.anti_grab"), ref debounce);
		GUILayout.Space(5f);
		if (GUILayout.Button(L.T("self.give_crown"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			string localPlayerSteamID = PlayerController.GetLocalPlayerSteamID();
			Object.FindObjectOfType<SessionManager>();
			PhotonView component = ((Component)Object.FindObjectOfType<PunManager>()).GetComponent<PhotonView>();
			if ((Object)(object)component != (Object)null)
			{
				component.RPC("CrownPlayerRPC", (RpcTarget)3, new object[1] { localPlayerSteamID });
				Debug.Log((object)"Gave self crown!");
			}
			else
			{
				Debug.LogError((object)"PhotonView not found on PunManager GameObject!");
			}
		}
		ToggleLogic("no_weapon_recoil", L.T("self.no_recoil"), ref Patches.NoWeaponRecoil._isEnabledForConfig, delegate
		{
			ConfigManager.SaveToggle("no_weapon_recoil", Patches.NoWeaponRecoil._isEnabledForConfig);
			PlayerPrefs.Save();
			Debug.Log((object)$"[Self Tab Toggle] Set No Recoil Enabled to: {Patches.NoWeaponRecoil._isEnabledForConfig}");
		});
		GUILayout.Space(10f);
		ToggleLogic("no_weapon_cooldown", L.T("self.no_cooldown"), ref ConfigManager.NoWeaponCooldownEnabled, delegate
		{
			ConfigManager.SaveToggle("no_weapon_cooldown", ConfigManager.NoWeaponCooldownEnabled);
			PlayerPrefs.Save();
			Debug.Log((object)$"[GUI Toggle] Set No Cooldown Enabled to: {ConfigManager.NoWeaponCooldownEnabled}");
		});
		GUILayout.Space(10f);
		GUILayout.Label(L.T("self.spread_fmt", ConfigManager.CurrentSpreadMultiplier, ((ConfigManager.CurrentSpreadMultiplier <= 0.01f) ? L.T("self.spread_none") : (Mathf.Approximately(ConfigManager.CurrentSpreadMultiplier, 1f) ? L.T("self.spread_normal") : $"{ConfigManager.CurrentSpreadMultiplier * 100f:F0}%"))), labelStyle, Array.Empty<GUILayoutOption>());
		float num = GUILayout.HorizontalSlider(ConfigManager.CurrentSpreadMultiplier, 0f, 2f, Array.Empty<GUILayoutOption>());
		if (num != ConfigManager.CurrentSpreadMultiplier)
		{
			ConfigManager.CurrentSpreadMultiplier = num;
			ConfigManager.SaveFloat("weapon_spread_multiplier", num);
			PlayerPrefs.Save();
			Debug.Log((object)$"[GUI Slider] Set Spread Multiplier to: {num}");
		}
		grabThroughWallsEnabled = ToggleLogic("grab_through_walls", L.T("self.grab_walls"), ref grabThroughWallsEnabled, delegate
		{
			Patches.ToggleGrabThroughWalls(grabThroughWallsEnabled);
			ConfigManager.SaveToggle("grab_through_walls", grabThroughWallsEnabled);
			PlayerPrefs.Save();
		});
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("self.stats"));
		GUILayout.Label(L.T("self.strength_fmt", Mathf.RoundToInt(sliderValueStrength)), labelStyle, Array.Empty<GUILayoutOption>());
		sliderValueStrength = GUILayout.HorizontalSlider(sliderValueStrength, 1f, 30f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (sliderValueStrength != oldSliderValueStrength)
		{
			int num2 = Mathf.RoundToInt(sliderValueStrength);
			string localPlayerSteamID2 = PlayerController.GetLocalPlayerSteamID();
			PhotonView component2 = ((Component)Object.FindObjectOfType<PunManager>()).GetComponent<PhotonView>();
			if ((Object)(object)component2 != (Object)null)
			{
				component2.RPC("UpgradePlayerGrabStrengthRPC", (RpcTarget)3, new object[2] { localPlayerSteamID2, num2 });
			}
			else
			{
				Debug.LogError((object)"PhotonView not found on PunManager GameObject!");
			}
			oldSliderValueStrength = sliderValueStrength;
		}
		GUILayout.Label(L.T("self.throw_fmt", Mathf.RoundToInt(throwStrength)), labelStyle, Array.Empty<GUILayoutOption>());
		throwStrength = GUILayout.HorizontalSlider(throwStrength, 0f, 30f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (throwStrength != OldthrowStrength)
		{
			int num3 = Mathf.RoundToInt(throwStrength);
			string localPlayerSteamID3 = PlayerController.GetLocalPlayerSteamID();
			PhotonView component3 = ((Component)Object.FindObjectOfType<PunManager>()).GetComponent<PhotonView>();
			if ((Object)(object)component3 != (Object)null)
			{
				component3.RPC("UpgradePlayerThrowStrengthRPC", (RpcTarget)3, new object[2] { localPlayerSteamID3, num3 });
			}
			else
			{
				Debug.LogError((object)"PhotonView not found on PunManager GameObject!");
			}
			OldthrowStrength = throwStrength;
		}
		GUILayout.Label(L.T("self.speed_fmt", Mathf.RoundToInt(sliderValue)), labelStyle, Array.Empty<GUILayoutOption>());
		sliderValue = GUILayout.HorizontalSlider(sliderValue, 1f, 30f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (sliderValue != oldSliderValue)
		{
			int num4 = Mathf.RoundToInt(sliderValue);
			string localPlayerSteamID4 = PlayerController.GetLocalPlayerSteamID();
			PhotonView component4 = ((Component)Object.FindObjectOfType<PunManager>()).GetComponent<PhotonView>();
			if ((Object)(object)component4 != (Object)null)
			{
				component4.RPC("UpgradePlayerSprintSpeedRPC", (RpcTarget)3, new object[2] { localPlayerSteamID4, num4 });
			}
			else
			{
				Debug.LogError((object)"PhotonView not found on PunManager GameObject!");
			}
			oldSliderValue = sliderValue;
		}
		GUILayout.Label(L.T("self.grab_range_fmt", Mathf.RoundToInt(grabRange)), labelStyle, Array.Empty<GUILayoutOption>());
		grabRange = GUILayout.HorizontalSlider(grabRange, 1f, 30f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (grabRange != OldgrabRange)
		{
			int num5 = Mathf.RoundToInt(grabRange);
			string localPlayerSteamID5 = PlayerController.GetLocalPlayerSteamID();
			PhotonView component5 = ((Component)Object.FindObjectOfType<PunManager>()).GetComponent<PhotonView>();
			if ((Object)(object)component5 != (Object)null)
			{
				component5.RPC("UpgradePlayerGrabRangeRPC", (RpcTarget)3, new object[2] { localPlayerSteamID5, num5 });
			}
			else
			{
				Debug.LogError((object)"PhotonView not found on PunManager GameObject!");
			}
			OldgrabRange = grabRange;
		}
		GUILayout.Label(L.T("self.stam_delay_fmt", Mathf.RoundToInt(staminaRechargeDelay)), labelStyle, Array.Empty<GUILayoutOption>());
		staminaRechargeDelay = GUILayout.HorizontalSlider(staminaRechargeDelay, 1f, 30f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (staminaRechargeDelay != oldStaminaRechargeDelay)
		{
			oldStaminaRechargeDelay = staminaRechargeDelay;
			Debug.Log((object)("Stamina Recharge Delay to: " + staminaRechargeDelay));
		}
		GUILayout.Label(L.T("self.stam_rate_fmt", Mathf.RoundToInt(staminaRechargeRate)), labelStyle, Array.Empty<GUILayoutOption>());
		staminaRechargeRate = GUILayout.HorizontalSlider(staminaRechargeRate, 1f, 30f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (staminaRechargeDelay != oldStaminaRechargeDelay || staminaRechargeRate != oldStaminaRechargeRate)
		{
			PlayerController.DecreaseStaminaRechargeDelay(staminaRechargeDelay, staminaRechargeRate);
			Debug.Log((object)$"Stamina recharge updated: Delay={staminaRechargeDelay}x, Rate={staminaRechargeRate}x");
			oldStaminaRechargeDelay = staminaRechargeDelay;
			oldStaminaRechargeRate = staminaRechargeRate;
		}
		GUILayout.Label(L.T("self.extra_jump_fmt", Mathf.RoundToInt((float)extraJumps)), labelStyle, Array.Empty<GUILayoutOption>());
		extraJumps = (int)GUILayout.HorizontalSlider((float)extraJumps, 0f, 30f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if ((float)extraJumps != OldextraJumps)
		{
			int num6 = Mathf.RoundToInt((float)extraJumps);
			string localPlayerSteamID6 = PlayerController.GetLocalPlayerSteamID();
			PhotonView component6 = ((Component)Object.FindObjectOfType<PunManager>()).GetComponent<PhotonView>();
			if ((Object)(object)component6 != (Object)null)
			{
				component6.RPC("UpgradePlayerExtraJumpRPC", (RpcTarget)3, new object[2] { localPlayerSteamID6, num6 });
			}
			else
			{
				Debug.LogError((object)"PhotonView not found on PunManager GameObject!");
			}
			OldextraJumps = extraJumps;
		}
		GUILayout.Label(L.T("self.tumble_fmt", Mathf.RoundToInt(tumbleLaunch)), labelStyle, Array.Empty<GUILayoutOption>());
		tumbleLaunch = (int)GUILayout.HorizontalSlider(tumbleLaunch, 0f, 20f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (tumbleLaunch != OldtumbleLaunch)
		{
			int num7 = Mathf.RoundToInt(tumbleLaunch);
			string localPlayerSteamID7 = PlayerController.GetLocalPlayerSteamID();
			PhotonView component7 = ((Component)Object.FindObjectOfType<PunManager>()).GetComponent<PhotonView>();
			if ((Object)(object)component7 != (Object)null)
			{
				component7.RPC("UpgradePlayerTumbleLaunchRPC", (RpcTarget)3, new object[2] { localPlayerSteamID7, num7 });
			}
			else
			{
				Debug.LogError((object)"PhotonView not found on PunManager GameObject!");
			}
			OldtumbleLaunch = tumbleLaunch;
		}

		// ===================== 一键全升级 =====================
		GUILayout.Space(10f);
		if (GUILayout.Button(L.T("self.max_all_upgrades"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			UpgradeHelper.MaxAllUpgrades();
		}

		// ===================== 游戏速度 =====================
		GUILayout.Label(L.T("self.game_speed_fmt", Time.timeScale.ToString("F1")), labelStyle, Array.Empty<GUILayoutOption>());
		float newTimeScale = GUILayout.HorizontalSlider(Time.timeScale, 0.1f, 5f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (Mathf.Abs(newTimeScale - Time.timeScale) > 0.01f)
		{
			Time.timeScale = newTimeScale;
		}
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("0.5x", buttonStyle, Array.Empty<GUILayoutOption>())) Time.timeScale = 0.5f;
		if (GUILayout.Button("1x", buttonStyle, Array.Empty<GUILayoutOption>())) Time.timeScale = 1f;
		if (GUILayout.Button("2x", buttonStyle, Array.Empty<GUILayoutOption>())) Time.timeScale = 2f;
		if (GUILayout.Button("3x", buttonStyle, Array.Empty<GUILayoutOption>())) Time.timeScale = 3f;
		GUILayout.EndHorizontal();

		// ===================== 幽灵模式 =====================
		GUILayout.Space(5f);
		ToggleLogic("stealth_mode", "👻 " + L.T("self.stealth_mode"), ref StealthMode.isEnabled, StealthMode.Apply);
		if (StealthMode.isEnabled && !string.IsNullOrEmpty(StealthMode.statusMessage))
			GUILayout.Label(StealthMode.statusMessage, labelStyle, Array.Empty<GUILayoutOption>());

		GUILayout.Label(L.T("self.jump_fmt", Mathf.RoundToInt(jumpForce)), labelStyle, Array.Empty<GUILayoutOption>());
		jumpForce = GUILayout.HorizontalSlider(jumpForce, 1f, 30f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (jumpForce != OldjumpForce)
		{
			PlayerController.SetJumpForce(17f + jumpForce);
			OldjumpForce = jumpForce;
		}
		GUILayout.Label(L.T("self.gravity_fmt", Mathf.RoundToInt(customGravity)), labelStyle, Array.Empty<GUILayoutOption>());
		customGravity = GUILayout.HorizontalSlider(customGravity, 1f, 30f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (customGravity != OldcustomGravity)
		{
			PlayerController.SetCustomGravity(30f + customGravity);
			OldcustomGravity = customGravity;
		}
		GUILayout.Label(L.T("self.crouch_delay_fmt", Mathf.RoundToInt(crouchDelay)), labelStyle, Array.Empty<GUILayoutOption>());
		crouchDelay = GUILayout.HorizontalSlider(crouchDelay, 0f, 30f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (crouchDelay != OldcrouchDelay)
		{
			PlayerController.SetCrouchDelay(crouchDelay);
			OldcrouchDelay = crouchDelay;
		}
		GUILayout.Label(L.T("self.crouch_speed_fmt", Mathf.RoundToInt(crouchSpeed)), labelStyle, Array.Empty<GUILayoutOption>());
		crouchSpeed = GUILayout.HorizontalSlider(crouchSpeed, 1f, 30f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (crouchSpeed != OldcrouchSpeed)
		{
			PlayerController.SetCrouchSpeed(crouchSpeed);
			OldcrouchSpeed = crouchSpeed;
		}
		GUILayout.Label(L.T("self.slide_fmt", Mathf.RoundToInt(slideDecay)), labelStyle, Array.Empty<GUILayoutOption>());
		slideDecay = GUILayout.HorizontalSlider(slideDecay, 0f, 20f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (slideDecay != OldslideDecay)
		{
			PlayerController.SetSlideDecay(slideDecay);
			OldslideDecay = slideDecay;
		}
		GUILayout.Label(L.T("self.flashlight_fmt", Mathf.RoundToInt(flashlightIntensity)), labelStyle, Array.Empty<GUILayoutOption>());
		flashlightIntensity = GUILayout.HorizontalSlider(flashlightIntensity, 1f, 20f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (flashlightIntensity != OldflashlightIntensity)
		{
			PlayerController.SetFlashlightIntensity(flashlightIntensity);
			OldflashlightIntensity = flashlightIntensity;
		}
		if ((Object)(object)FOVEditor.Instance == (Object)null)
		{
			new GameObject("FOVEditor").AddComponent<FOVEditor>();
		}
		if ((Object)(object)FOVEditor.Instance != (Object)null)
		{
			float fOV = FOVEditor.Instance.GetFOV();
			GUILayout.Label(L.T("self.fov_fmt", Mathf.RoundToInt(fOV)), labelStyle, Array.Empty<GUILayoutOption>());
			float num8 = GUILayout.HorizontalSlider(fOV, 60f, 120f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
			if (num8 != fOV)
			{
				FOVEditor.Instance.SetFOV(num8);
				fieldOfView = num8;
			}
		}
		else
		{
			GUILayout.Label(L.T("self.fov_loading"), labelStyle, Array.Empty<GUILayoutOption>());
		}

		// ===================== 第三人称相机 =====================
		GUILayout.Space(10f);
		ToggleLogic("third_person", L.T("self.third_person"), ref ThirdPersonCamera.isEnabled,
			() => { if (ThirdPersonCamera.isEnabled) ThirdPersonCamera.Enable(); else ThirdPersonCamera.Disable(); });
		if (ThirdPersonCamera.isEnabled)
		{
			GUILayout.Label(L.T("self.tp_cam_dist_fmt", ThirdPersonCamera.cameraDistance), labelStyle, Array.Empty<GUILayoutOption>());
			ThirdPersonCamera.cameraDistance = GUILayout.HorizontalSlider(ThirdPersonCamera.cameraDistance, 1f, 15f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
			GUILayout.Label(L.T("self.tp_cam_height_fmt", ThirdPersonCamera.cameraHeight), labelStyle, Array.Empty<GUILayoutOption>());
			ThirdPersonCamera.cameraHeight = GUILayout.HorizontalSlider(ThirdPersonCamera.cameraHeight, 0f, 5f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		}

		// ===================== 创造模式 (内嵌到自身页) =====================
		GUILayout.Space(15f);
		DrawSectionHeader(L.T("creative.title"));
		GUILayout.Space(5f);
		GUILayout.Label(L.T("creative.desc"), labelStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Space(5f);
		ToggleLogic("creative_mode", L.T("creative.toggle"), ref CreativeMode.isCreativeMode, CreativeMode.ToggleCreativeMode);
		GUILayout.Space(5f);
		if (CreativeMode.isCreativeMode)
		{
			GUILayout.Label(L.T("creative.status_on"), warningStyle, Array.Empty<GUILayoutOption>());
			DrawSectionHeader(L.T("creative.controls"));
			GUILayout.Label(L.T("creative.wasd"), labelStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label(L.T("creative.shift"), labelStyle, Array.Empty<GUILayoutOption>());
		}
		else
		{
			GUILayout.Label(L.T("creative.status_off"), labelStyle, Array.Empty<GUILayoutOption>());
		}

		// ===================== 反踢保护 (从杂项迁移) =====================
		GUILayout.Space(15f);
		DrawSectionHeader(L.T("misc.protection"));
		ToggleLogic("anti_kick", L.T("misc.anti_kick"), ref antiKickEnabled, () => {
			if (antiKickEnabled) AntiKick.Enable(); else AntiKick.Disable();
		});

		// ===================== 智能寻路 (从杂项迁移) =====================
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("autopilot.title"));
		if (GUILayout.Button(AutoPilot.isActive ? L.T("autopilot.stop") : L.T("autopilot.start"), 
			AutoPilot.isActive ? tabSelectedStyle : buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			AutoPilot.Toggle();
		}
		if (AutoPilot.isActive && !string.IsNullOrEmpty(AutoPilot.statusText))
		{
			GUILayout.Label(AutoPilot.statusText, warningStyle, Array.Empty<GUILayoutOption>());
		}
	}

	private void DrawVisualsTab()
	{
		GUILayout.Space(5f);
		ToggleLogic("modern_esp", L.T("vis.modern_esp"), ref useModernESP);
		DrawSectionHeader(L.T("vis.enemy_esp"));
		ToggleLogic("enable_en_esp", L.T("vis.enemy_toggle"), ref DebugCheats.drawEspBool);
		if (DebugCheats.drawEspBool)
		{
			GUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
			GUILayout.Space(5f);
			ToggleLogic("show_2d_box_enemy", L.T("vis.show_2d_box"), ref DebugCheats.showEnemyBox);
			GUILayout.Space(5f);
			bool value = DebugCheats.drawChamsBool;
			ToggleLogic("show_chams_enemy", L.T("vis.show_chams"), ref value);
			DebugCheats.drawChamsBool = value;
			GUILayout.Space(5f);
			ToggleLogic("show_names_enemy", L.T("vis.show_name"), ref DebugCheats.showEnemyNames);
			GUILayout.Space(5f);
			ToggleLogic("show_distance_enemy", L.T("vis.show_distance"), ref DebugCheats.showEnemyDistance);
			GUILayout.Space(5f);
			ToggleLogic("show_health_enemy", L.T("vis.show_health"), ref DebugCheats.showEnemyHP);
			GUILayout.EndVertical();
		}
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("vis.item_esp"));
		ToggleLogic("enable_item_esp", L.T("vis.item_toggle"), ref DebugCheats.drawItemEspBool);
		if (DebugCheats.drawItemEspBool)
		{
			GUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
			GUILayout.Space(5f);
			ToggleLogic("show_3d_box_item", L.T("vis.show_3d_box"), ref DebugCheats.draw3DItemEspBool);
			GUILayout.Space(5f);
			bool value2 = DebugCheats.drawItemChamsBool;
			ToggleLogic("show_chams_item", L.T("vis.item_chams"), ref value2);
			DebugCheats.drawItemChamsBool = value2;
			if (DebugCheats.drawChamsBool || DebugCheats.drawItemChamsBool)
			{
				GUILayout.Space(10f);
				if (GUILayout.Button(L.T("vis.chams_color"), buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(30f) }))
				{
					showChamsWindow = !showChamsWindow;
				}
			}
			GUILayout.Space(5f);
			ToggleLogic("show_names_item", L.T("vis.item_name"), ref DebugCheats.showItemNames);
			GUILayout.Space(5f);
			ToggleLogic("show_distance_item", L.T("vis.item_distance"), ref DebugCheats.showItemDistance);
			if (DebugCheats.showItemDistance)
			{
				GUILayout.Label(L.T("vis.max_dist_fmt", DebugCheats.maxItemEspDistance), labelStyle, Array.Empty<GUILayoutOption>());
				DebugCheats.maxItemEspDistance = GUILayout.HorizontalSlider(DebugCheats.maxItemEspDistance, 0f, 1000f, Array.Empty<GUILayoutOption>());
			}
			GUILayout.Space(5f);
			ToggleLogic("show_value_item", L.T("vis.show_value"), ref DebugCheats.showItemValue);
			if (DebugCheats.showItemValue)
			{
				GUILayout.Label(L.T("vis.min_value_fmt", DebugCheats.minItemValue), labelStyle, Array.Empty<GUILayoutOption>());
				DebugCheats.minItemValue = Mathf.RoundToInt(GUILayout.HorizontalSlider((float)DebugCheats.minItemValue, 0f, 50000f, Array.Empty<GUILayoutOption>()));
			}
			GUILayout.Space(5f);
			ToggleLogic("show_dead_heads", L.T("vis.show_skulls"), ref DebugCheats.showPlayerDeathHeads);
			GUILayout.EndVertical();
		}
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("vis.extract_esp"));
		ToggleLogic("enable_extract_esp", L.T("vis.extract_toggle"), ref DebugCheats.drawExtractionPointEspBool);
		if (DebugCheats.drawExtractionPointEspBool)
		{
			GUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
			GUILayout.Space(5f);
			ToggleLogic("show_extract_names", L.T("vis.show_name_status"), ref DebugCheats.showExtractionNames);
			GUILayout.Space(5f);
			ToggleLogic("show_extract_distance", L.T("vis.extract_dist"), ref DebugCheats.showExtractionDistance);
			GUILayout.EndVertical();
		}
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("vis.player_esp"));
		ToggleLogic("enable_player_esp", L.T("vis.player_toggle"), ref DebugCheats.drawPlayerEspBool);
		if (DebugCheats.drawPlayerEspBool)
		{
			GUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
			GUILayout.Space(5f);
			ToggleLogic("show_2d_box_player", L.T("vis.player_2d"), ref DebugCheats.draw2DPlayerEspBool);
			GUILayout.Space(5f);
			ToggleLogic("show_3d_box_player", L.T("vis.player_3d"), ref DebugCheats.draw3DPlayerEspBool);
			GUILayout.Space(5f);
			ToggleLogic("show_names_player", L.T("vis.player_name"), ref DebugCheats.showPlayerNames);
			GUILayout.Space(5f);
			ToggleLogic("show_distance_player", L.T("vis.player_dist"), ref DebugCheats.showPlayerDistance);
			GUILayout.Space(5f);
			ToggleLogic("show_health_player", L.T("vis.player_hp"), ref DebugCheats.showPlayerHP);
			GUILayout.Space(5f);
			ToggleLogic("show_alive_dead_list", L.T("vis.player_list"), ref showPlayerStatus);
			GUILayout.EndVertical();
		}
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("vis.trace"));
		ToggleLogic("trace_enemy", L.T("vis.trace_enemy"), ref ESPEnhancements.showTraceLinesEnemy);
		GUILayout.Space(5f);
		ToggleLogic("trace_item", L.T("vis.trace_item"), ref ESPEnhancements.showTraceLinesItem);
		GUILayout.Space(5f);
		ToggleLogic("trace_player", L.T("vis.trace_player"), ref ESPEnhancements.showTraceLinesPlayer);
		GUILayout.Space(5f);
		ToggleLogic("skeleton_esp", L.T("vis.skeleton_esp"), ref SkeletonESP.enabled);
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("vis.esp_presets"));
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		var presetDisplayNames = ESPEnhancements.GetPresetNames();
		for (int pi = 0; pi < presetDisplayNames.Length; pi++)
		{
			GUIStyle presetBtnStyle = ((int)ESPEnhancements.currentPreset == pi) ? tabSelectedStyle : buttonStyle;
			if (GUILayout.Button(presetDisplayNames[pi], presetBtnStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(85f) }))
			{
				ESPEnhancements.ApplyPreset((ESPEnhancements.ESPPreset)pi);
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("vis.radar"));
		ToggleLogic("mini_radar", L.T("vis.radar_toggle"), ref MiniRadar.isRadarEnabled);
		if (MiniRadar.isRadarEnabled)
		{
			GUILayout.Label(L.T("vis.radar_range_fmt", Mathf.RoundToInt(MiniRadar.radarRange)), labelStyle, Array.Empty<GUILayoutOption>());
			MiniRadar.radarRange = GUILayout.HorizontalSlider(MiniRadar.radarRange, 10f, 200f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		}
	}

	private void DrawCombatTab()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		UpdatePlayerList();
		EnsureListStylesInitialized();
		DrawSectionHeader(L.T("combat.select"));
		for (int i = 0; i < playerNames.Count; i++)
		{
			if (i == selectedPlayerIndex)
			{
				cachedPlayerListStyle.normal.background = rowBgSelected;
				cachedPlayerListStyle.normal.textColor = (Color)(new Color32(byte.MaxValue, (byte)165, (byte)0, byte.MaxValue));
			}
			else
			{
				cachedPlayerListStyle.normal.background = rowBgNormal;
				cachedPlayerListStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
				cachedPlayerListStyle.hover.background = rowBgHover;
				cachedPlayerListStyle.hover.textColor = Color.white;
			}
			if (GUILayout.Button(playerNames[i], cachedPlayerListStyle, Array.Empty<GUILayoutOption>()))
			{
				selectedPlayerIndex = i;
			}
		}
		GUILayout.Space(40f);
		if (GUILayout.Button(L.T("combat.damage"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			if (selectedPlayerIndex >= 0 && selectedPlayerIndex < playerList.Count)
			{
				Players.DamagePlayer(playerList[selectedPlayerIndex], 1, playerNames[selectedPlayerIndex]);
				Debug.Log((object)("Player " + playerNames[selectedPlayerIndex] + " damaged."));
			}
			else
			{
				Debug.Log((object)"No valid player selected to damage!");
			}
		}
		if (GUILayout.Button(L.T("combat.heal"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			if (selectedPlayerIndex >= 0 && selectedPlayerIndex < playerList.Count)
			{
				Players.HealPlayer(playerList[selectedPlayerIndex], 50, playerNames[selectedPlayerIndex]);
				Debug.Log((object)("Player " + playerNames[selectedPlayerIndex] + " healed."));
			}
			else
			{
				Debug.Log((object)"No valid player selected to heal!");
			}
		}
		if (GUILayout.Button(L.T("combat.kill"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			Players.KillSelectedPlayer(selectedPlayerIndex, playerList, playerNames);
			Debug.Log((object)("Player killed: " + playerNames[selectedPlayerIndex]));
		}
		if (GUILayout.Button(L.T("combat.revive"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			Players.ReviveSelectedPlayer(selectedPlayerIndex, playerList, playerNames);
			Debug.Log((object)("Player revived: " + playerNames[selectedPlayerIndex]));
		}
		if (GUILayout.Button(L.T("combat.heal_revive"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			if (selectedPlayerIndex >= 0 && selectedPlayerIndex < playerList.Count)
			{
				string name = (selectedPlayerIndex < playerNames.Count) ? playerNames[selectedPlayerIndex] : "Unknown";
				Players.HealRevivePlayer(playerList[selectedPlayerIndex], name);
			}
		}
		if (GUILayout.Button(L.T("combat.tumble"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			Players.ForcePlayerTumble();
			Debug.Log((object)("Player tumbled: " + playerNames[selectedPlayerIndex]));
		}
		if (GUILayout.Button(showTeleportUI ? L.T("combat.hide_tp") : L.T("combat.show_tp"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			showTeleportUI = !showTeleportUI;
			if (showTeleportUI)
			{
				UpdateTeleportOptions();
			}
		}
		if (showTeleportUI)
		{
			DrawTeleportOptions();
		}
		GUILayout.Space(15f);
		// 附身功能
		if (PlayerPossession.isPossessing)
		{
			GUILayout.Label(L.T("combat.possess_active", PlayerPossession.possessTargetName), warningStyle, Array.Empty<GUILayoutOption>());
			if (GUILayout.Button(L.T("combat.unpossess"), buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				PlayerPossession.StopPossession();
			}
		}
		else
		{
			if (GUILayout.Button(L.T("combat.possess"), buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				PlayerPossession.StartPossession(playerList, playerNames, selectedPlayerIndex);
			}
		}

		// ===================== 自动瞄准 =====================
		GUILayout.Space(15f);
		DrawSectionHeader(L.T("combat.aimbot_title"));
		ToggleLogic("aimbot_enabled", L.T("combat.aimbot_toggle"), ref Aimbot.isEnabled);
		if (Aimbot.isEnabled)
		{
			GUILayout.Label(L.T("combat.aimbot_smooth_fmt", Aimbot.smoothness.ToString("F0")), labelStyle, Array.Empty<GUILayoutOption>());
			Aimbot.smoothness = GUILayout.HorizontalSlider(Aimbot.smoothness, 1f, 30f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
			GUILayout.Label(L.T("combat.aimbot_range_fmt", Aimbot.maxDistance.ToString("F0")), labelStyle, Array.Empty<GUILayoutOption>());
			Aimbot.maxDistance = GUILayout.HorizontalSlider(Aimbot.maxDistance, 10f, 200f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		}
	}

	private void DrawFunTab()
	{
		UpdatePlayerList();
		EnsureListStylesInitialized();

		// ===================== 🌀 陀螺旋转 =====================
		DrawSectionHeader(L.T("fun.title"));
		GUILayout.Space(5f);
		ToggleLogic("gyro_spin", L.T("fun.gyro_spin"), ref GyroSpin.isEnabled);
		if (GyroSpin.isEnabled)
		{
			GUILayout.Label(L.T("fun.gyro_speed_fmt", Mathf.RoundToInt(GyroSpin.spinSpeed).ToString()), labelStyle, Array.Empty<GUILayoutOption>());
			GyroSpin.spinSpeed = GUILayout.HorizontalSlider(GyroSpin.spinSpeed, 10f, 720f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
			GUILayout.Label(L.T("fun.gyro_desc"), labelStyle, Array.Empty<GUILayoutOption>());
		}

		// ===================== 💃 表情动作 =====================
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("fun.gesture"));
		GUILayout.Label(L.T("fun.gesture_desc"), labelStyle, Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button(L.T("fun.gesture_shake"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			HeadGesture.StartShake();
		}
		if (GUILayout.Button(L.T("fun.gesture_nod"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			HeadGesture.StartNod();
		}
		GUILayout.EndHorizontal();

		// ===================== 🔮 大小变换 =====================
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("fun.scale"));
		GUILayout.Label(L.T("fun.scale_desc"), labelStyle, Array.Empty<GUILayoutOption>());
		ToggleLogic("scale_sync", L.T("fun.scale"), ref ScaleSync.isEnabled, () => {
			if (!ScaleSync.isEnabled) ScaleSync.Restore();
		});
		if (ScaleSync.isEnabled)
		{
			GUILayout.Label(L.T("fun.scale_size_fmt", ScaleSync.targetScale.ToString("F1")), labelStyle, Array.Empty<GUILayoutOption>());
			ScaleSync.targetScale = GUILayout.HorizontalSlider(ScaleSync.targetScale, 0.2f, 3f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		}

		// ===================== 伪造名字 (从杂项迁移) =====================
		GUILayout.Space(15f);
		DrawSectionHeader(L.T("misc.spoof_name"));
		DrawSectionHeader(L.T("misc.select"));
		for (int i = 0; i < playerNames.Count; i++)
		{
			if (i == selectedPlayerIndex)
			{
				cachedPlayerListStyle.normal.background = rowBgSelected;
				cachedPlayerListStyle.normal.textColor = (Color)(new Color32(byte.MaxValue, (byte)165, (byte)0, byte.MaxValue));
			}
			else
			{
				cachedPlayerListStyle.normal.background = rowBgNormal;
				cachedPlayerListStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
				cachedPlayerListStyle.hover.background = rowBgHover;
				cachedPlayerListStyle.hover.textColor = Color.white;
			}
			if (GUILayout.Button(playerNames[i], cachedPlayerListStyle, Array.Empty<GUILayoutOption>()))
			{
				selectedPlayerIndex = i;
			}
		}
		GUILayout.Space(10f);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button(L.T("misc.spoof_btn"), buttonStyle, Array.Empty<GUILayoutOption>()) && !string.IsNullOrEmpty(spoofedNameText))
		{
			ChatHijack.ToggleNameSpoofing(enable: true, spoofedNameText, spoofTargetVisibleName, playerList, playerNames);
		}
		if (GUILayout.Button(spoofTargetVisibleName, buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			spoofDropdownVisible = !spoofDropdownVisible;
		}
		string text = "SpoofNameField";
		GUI.SetNextControlName(text);
		spoofedNameText = GUILayout.TextField(spoofedNameText, textFieldStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(110f) });
		if (GUI.GetNameOfFocusedControl() == text && !showTextEditorPopup)
		{
			activeTextFieldId = text;
			largeTextBoxContent = spoofedNameText;
			showTextEditorPopup = true;
			GUI.FocusControl((string)null);
		}
		GUILayout.EndHorizontal();
		if (spoofDropdownVisible)
		{
			for (int k = 0; k < playerNames.Count + 1; k++)
			{
				string text2 = ((k == 0) ? L.T("common.all") : playerNames[k - 1]);
				if (GUILayout.Button(text2, buttonStyle, Array.Empty<GUILayoutOption>()))
				{
					spoofTargetVisibleName = text2;
					spoofDropdownVisible = false;
					if (!string.IsNullOrEmpty(spoofedNameText))
					{
						ChatHijack.ToggleNameSpoofing(enable: true, spoofedNameText, spoofTargetVisibleName, playerList, playerNames);
					}
				}
			}
		}
		if (GUILayout.Button(L.T("misc.reset_spoof"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ChatHijack.ToggleNameSpoofing(enable: false, "", spoofTargetVisibleName, playerList, playerNames);
			spoofedNameText = "Text";
		}
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button(L.T("misc.random_player_name"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			try
			{
				GameObject localP = DebugCheats.GetLocalPlayer();
				string localName = (localP != null) ? ((UnityEngine.Object)localP).name : "";
				List<string> otherNames = new List<string>();
				for (int ri = 0; ri < playerNames.Count; ri++)
				{
					string stripped = System.Text.RegularExpressions.Regex.Replace(playerNames[ri], "<.*?>", "").Trim();
					string clean = System.Text.RegularExpressions.Regex.Replace(stripped, "\\[(LIVE|DEAD|\u5b58\u6d3b|\u6b7b\u4ea1)\\]\\s*", "").Trim();
					if (!string.IsNullOrEmpty(clean) && !clean.Contains("FakePlayer") && !clean.Contains(localName))
					{
						otherNames.Add(clean);
					}
				}
				if (otherNames.Count > 0)
				{
					string randomName = otherNames[new System.Random().Next(otherNames.Count)];
					ChatHijack.SpoofLocalPlayerName(randomName);
				}
			}
			catch { }
		}
		if (GUILayout.Button(L.T("misc.random_string"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			try
			{
				const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
				System.Random rng = new System.Random();
				char[] buf = new char[8];
				for (int ci = 0; ci < 8; ci++) buf[ci] = chars[rng.Next(chars.Length)];
				ChatHijack.SpoofLocalPlayerName(new string(buf));
			}
			catch { }
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		ToggleLogic("persistent_spoof_name", L.T("misc.persist_spoof"), ref spoofNameActive);
		if (spoofNameActive)
		{
			text = "PersistentSpoofNameField";
			GUI.SetNextControlName(text);
			persistentNameText = GUILayout.TextField(persistentNameText, textFieldStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(210f) });
			if (GUI.GetNameOfFocusedControl() == text && !showTextEditorPopup)
			{
				activeTextFieldId = text;
				largeTextBoxContent = persistentNameText;
				showTextEditorPopup = true;
				GUI.FocusControl((string)null);
			}
		}

		// ===================== 伪造颜色 (从杂项迁移) =====================
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("misc.spoof_color"));
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button(L.T("misc.spoof_color"), buttonStyle, Array.Empty<GUILayoutOption>()) && int.TryParse(colorIndexText, out var result) && GetColorNames().ContainsKey(result))
		{
			ChatHijack.ChangePlayerColor(result, colorTargetVisibleName, playerList, playerNames);
		}
		if (GUILayout.Button(colorTargetVisibleName, buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			colorDropdownVisible = !colorDropdownVisible;
		}
		if (GUILayout.Button(GetColorNames().TryGetValue(int.Parse(colorIndexText), out var value) ? value : L.T("misc.select_color"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			showColorIndexDropdown = !showColorIndexDropdown;
		}
		GUILayout.EndHorizontal();
		if (!playerRainbowStates.ContainsKey(selectedPlayerIndex))
		{
			playerRainbowStates[selectedPlayerIndex] = false;
		}
		bool rainbowState = playerRainbowStates[selectedPlayerIndex];
		ToggleLogic("rainbow_spoof_" + selectedPlayerIndex, L.T("misc.rainbow"), ref rainbowState, delegate
		{
			if (rainbowState)
			{
				lastRainbowTimes[selectedPlayerIndex] = Time.time;
			}
		});
		playerRainbowStates[selectedPlayerIndex] = rainbowState;
		GUILayout.Space(10f);
		if (colorDropdownVisible)
		{
			for (int num = 0; num < playerNames.Count + 1; num++)
			{
				string text3 = ((num == 0) ? L.T("common.all") : playerNames[num - 1]);
				if (GUILayout.Button(text3, buttonStyle, Array.Empty<GUILayoutOption>()))
				{
					colorTargetVisibleName = text3;
					colorDropdownVisible = false;
				}
			}
		}
		if (showColorIndexDropdown)
		{
			colorIndexScrollPosition = GUILayout.BeginScrollView(colorIndexScrollPosition, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(150f) });
			foreach (KeyValuePair<int, string> item in GetColorNames())
			{
				if (GUILayout.Button(item.Value, buttonStyle, Array.Empty<GUILayoutOption>()))
				{
					colorIndexText = item.Key.ToString();
					showColorIndexDropdown = false;
				}
			}
			GUILayout.EndScrollView();
		}

		// ===================== 聊天刷屏 (从杂项迁移) =====================
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("misc.chat_spam"));
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button(L.T("misc.send"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ChatHijack.MakeChat(chatMessageText, ChatDropdownVisibleName, playerList, playerNames);
		}
		if (GUILayout.Button(ChatDropdownVisibleName, buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ChatDropdownVisible = !ChatDropdownVisible;
		}
		text = "chatmessageField";
		GUI.SetNextControlName(text);
		chatMessageText = GUILayout.TextField(chatMessageText, textFieldStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(110f) });
		if (GUI.GetNameOfFocusedControl() == text && !showTextEditorPopup)
		{
			activeTextFieldId = text;
			largeTextBoxContent = chatMessageText;
			showTextEditorPopup = true;
			GUI.FocusControl((string)null);
		}
		GUILayout.EndHorizontal();
		if (ChatDropdownVisible)
		{
			for (int num2 = 0; num2 < playerNames.Count + 1; num2++)
			{
				string text4 = ((num2 == 0) ? L.T("common.all") : playerNames[num2 - 1]);
				if (GUILayout.Button(text4, buttonStyle, Array.Empty<GUILayoutOption>()))
				{
					ChatDropdownVisibleName = text4;
					ChatDropdownVisible = false;
				}
			}
		}

		// ===================== 🎨 ASCII 艺术刷屏 =====================
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("fun.ascii_art"));
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(L.T("fun.ascii_select"), labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(80f) });
		string[] artNames = AsciiArtSpam.GetArtNames();
		if (asciiArtIndex < artNames.Length)
		{
			if (GUILayout.Button(artNames[asciiArtIndex], buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				showAsciiDropdown = !showAsciiDropdown;
			}
		}
		GUILayout.EndHorizontal();
		if (showAsciiDropdown)
		{
			for (int ai = 0; ai < artNames.Length; ai++)
			{
				if (GUILayout.Button(artNames[ai], buttonStyle, Array.Empty<GUILayoutOption>()))
				{
					asciiArtIndex = ai;
					AsciiArtSpam.selectedArtIndex = ai;
					showAsciiDropdown = false;
				}
			}
		}
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button(L.T("fun.ascii_send"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			AsciiArtSpam.selectedArtIndex = asciiArtIndex;
			AsciiArtSpam.Send(asciiTargetName, playerList, playerNames);
		}
		if (GUILayout.Button(asciiTargetName, buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			asciiTargetDropdown = !asciiTargetDropdown;
		}
		GUILayout.EndHorizontal();
		if (asciiTargetDropdown)
		{
			for (int ti = 0; ti < playerNames.Count + 1; ti++)
			{
				string tname = (ti == 0) ? L.T("common.all") : playerNames[ti - 1];
				if (GUILayout.Button(tname, buttonStyle, Array.Empty<GUILayoutOption>()))
				{
					asciiTargetName = tname;
					asciiTargetDropdown = false;
				}
			}
		}
		if (!string.IsNullOrEmpty(AsciiArtSpam.statusMessage))
		{
			GUILayout.Label(AsciiArtSpam.statusMessage, warningStyle, Array.Empty<GUILayoutOption>());
		}

		// ===================== 👻 随机恐吓音效 =====================
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("fun.scare"));
		GUILayout.Label(L.T("fun.scare_desc"), labelStyle, Array.Empty<GUILayoutOption>());
		if (GUILayout.Button(L.T("fun.scare_btn"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ScareSound.TriggerRandomScare();
		}
		if (!string.IsNullOrEmpty(ScareSound.statusMessage))
		{
			GUILayout.Label(ScareSound.statusMessage, warningStyle, Array.Empty<GUILayoutOption>());
		}

		// ===================== 聊天命令 (从杂项迁移) =====================
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("misc.chat_commands"));
		GUILayout.Label(L.T("misc.chat_commands_desc"), labelStyle, Array.Empty<GUILayoutOption>());
		if (!string.IsNullOrEmpty(ChatCommands.GetFeedback()))
		{
			GUILayout.Label(ChatCommands.GetFeedback(), warningStyle, Array.Empty<GUILayoutOption>());
		}
	}

	private void DrawEnemiesTab()
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Expected O, but got Unknown
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Expected O, but got Unknown
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Expected O, but got Unknown
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Expected O, but got Unknown
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_0519: Unknown result type (might be due to invalid IL or missing references)
		//IL_0531: Unknown result type (might be due to invalid IL or missing references)
		//IL_0536: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			// 允许重新缓存：如果为空且超过30秒重试
			if (cachedFilteredEnemySetups != null && cachedFilteredEnemySetups.Count == 0 && Time.time - lastEnemyCacheTime > 30f)
			{
				cachedFilteredEnemySetups = null;
				cachedEnemySetupNames = null;
			}
			if (cachedFilteredEnemySetups == null || cachedEnemySetupNames == null)
			{
				lastEnemyCacheTime = Time.time;
				List<EnemySetup> list = new List<EnemySetup>();
				// 通过反射调用EnemySpawner（当前游戏版本可能已移除该类型）
				var enemySpawnerType = typeof(RunManager).Assembly.GetType("EnemySpawner");
				if (enemySpawnerType != null)
				{
					var tryGetMethod = enemySpawnerType.GetMethod("TryGetEnemyLists", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
					if (tryGetMethod != null)
					{
						var args = new object[] { null, null, null };
						bool gotLists = (bool)tryGetMethod.Invoke(null, args);
						if (gotLists)
						{
							list.AddRange((List<EnemySetup>)args[0]);
							list.AddRange((List<EnemySetup>)args[1]);
							list.AddRange((List<EnemySetup>)args[2]);
						}
					}
				}
				// Fallback: 如果反射获取为空，使用 Resources 搜索所有 EnemySetup 资源
				if (list.Count == 0)
				{
					var allSetups = Resources.FindObjectsOfTypeAll<EnemySetup>();
					if (allSetups != null && allSetups.Length > 0)
					{
						list.AddRange(allSetups);
						Debug.Log("[Hax2] EnemySpawner fallback: found " + allSetups.Length + " EnemySetup via Resources");
					}
				}
				cachedFilteredEnemySetups = new List<EnemySetup>();
				cachedEnemySetupNames = new List<string>();
				if (list != null)
				{
					foreach (EnemySetup item2 in list)
					{
						if ((Object)(object)item2 != (Object)null && !((Object)item2).name.Contains("Enemy Group"))
						{
							string displayName = LanguageManager.GetEnemyName(((Object)item2).name);
							cachedFilteredEnemySetups.Add(item2);
							cachedEnemySetupNames.Add(displayName);
						}
					}
				}
			}
			UpdateEnemyList();
			DrawSectionHeader(L.T("enemies.select"));
			EnsureListStylesInitialized();
			enemyScrollPosition = GUILayout.BeginScrollView(enemyScrollPosition, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height((float)Mathf.Min(200, enemyNames.Count * 35)) });
			for (int i = 0; i < enemyNames.Count; i++)
			{
				if (i == selectedEnemyIndex)
				{
					cachedEnemyListStyle.normal.background = enemyBgSelected;
					cachedEnemyListStyle.normal.textColor = new Color(1f, 0.55f, 0.1f);
				}
				else
				{
					cachedEnemyListStyle.normal.background = enemyBgNormal;
					cachedEnemyListStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
				}
				if (GUILayout.Button(enemyNames[i], cachedEnemyListStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(30f) }))
				{
					selectedEnemyIndex = i;
				}
			}
			GUILayout.EndScrollView();
			GUI.color = Color.white;
			GUILayout.Space(40f);
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (GUILayout.Button(L.T("enemies.spawn"), buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				TrySpawnEnemy();
			}
			spawnCountText = GUILayout.TextField(spawnCountText, textFieldStyle, Array.Empty<GUILayoutOption>());
			if (GUILayout.Button((spawnEnemyIndex >= 0 && spawnEnemyIndex < cachedEnemySetupNames.Count) ? cachedEnemySetupNames[spawnEnemyIndex] : L.T("enemies.select_fallback"), buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				showSpawnDropdown = !showSpawnDropdown;
			}
			GUILayout.EndHorizontal();
			if (showSpawnDropdown)
			{
				spawnDropdownScrollPosition = GUILayout.BeginScrollView(spawnDropdownScrollPosition, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(150f) });
				for (int j = 0; j < cachedEnemySetupNames.Count; j++)
				{
					if (GUILayout.Button(cachedEnemySetupNames[j], buttonStyle, Array.Empty<GUILayoutOption>()))
					{
						spawnEnemyIndex = j;
						showSpawnDropdown = false;
					}
				}
				GUILayout.EndScrollView();
			}
			GUILayout.Space(10f);
			if (GUILayout.Button(L.T("enemies.kill"), buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				Enemies.KillSelectedEnemy(selectedEnemyIndex, enemyList, enemyNames);
			}
			if (GUILayout.Button(L.T("enemies.kill_all"), buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				Enemies.KillAllEnemies();
			}

			// ===================== 冻结所有敌人 =====================
			GUILayout.Space(5f);
			if (GUILayout.Button(Enemies.freezeAllEnemies ? L.T("enemies.unfreeze") : L.T("enemies.freeze"), buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				if (Enemies.freezeAllEnemies) Enemies.UnfreezeAllEnemies();
				else Enemies.FreezeAllEnemies();
			}

			// ===================== 禁用陷阱 =====================
			if (GUILayout.Button(L.T("enemies.disable_traps"), buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				int count = TrapDisabler.DisableAllTraps();
				Debug.Log("[Hax2] Disabled " + count + " traps");
			}

			ToggleLogic("blind_enemies", L.T("enemies.blind"), ref blindEnemies, delegate
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Expected O, but got Unknown
				Hashtable val2 = new Hashtable();
				val2[(object)"isBlindEnabled"] = blindEnemies;
				PhotonNetwork.LocalPlayer.SetCustomProperties(val2, (Hashtable)null, (WebFlags)null);
				ConfigManager.SaveToggle("blind_enemies", blindEnemies);
			});
			if (GUILayout.Button(showEnemyTeleportUI ? L.T("enemies.hide_tp") : L.T("enemies.show_tp"), buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				showEnemyTeleportUI = !showEnemyTeleportUI;
				if (showEnemyTeleportUI)
				{
					UpdateEnemyTeleportOptions();
				}
			}
			if (showEnemyTeleportUI)
			{
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				GUILayout.Label(L.T("enemies.tp_to"), labelStyle, Array.Empty<GUILayoutOption>());
				if (GUILayout.Button((enemyTeleportDestIndex >= 0 && enemyTeleportDestIndex < enemyTeleportDestOptions.Length) ? enemyTeleportDestOptions[enemyTeleportDestIndex] : L.T("enemies.no_player"), buttonStyle, Array.Empty<GUILayoutOption>()))
				{
					showEnemyTeleportDropdown = !showEnemyTeleportDropdown;
				}
				GUILayout.EndHorizontal();
				if (showEnemyTeleportDropdown)
				{
					enemyTeleportDropdownScrollPosition = GUILayout.BeginScrollView(enemyTeleportDropdownScrollPosition, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(150f) });
					for (int num = 0; num < enemyTeleportDestOptions.Length; num++)
					{
						if (num != enemyTeleportDestIndex && GUILayout.Button(enemyTeleportDestOptions[num], Array.Empty<GUILayoutOption>()))
						{
							enemyTeleportDestIndex = num;
						}
					}
					GUILayout.EndScrollView();
				}
				GUILayout.Space(10f);
				if (GUILayout.Button(L.T("enemies.execute_tp"), buttonStyle, Array.Empty<GUILayoutOption>()))
				{
					int num2 = enemyTeleportDestIndex;
					if (num2 >= 0 && num2 < playerList.Count)
					{
						if (DebugCheats.IsLocalPlayer(playerList[num2]))
						{
							Enemies.TeleportEnemyToMe(selectedEnemyIndex, enemyList, enemyNames);
						}
						else
						{
							Enemies.TeleportEnemyToPlayer(selectedEnemyIndex, enemyList, enemyNames, num2, playerList, playerNames);
						}
						UpdateEnemyList();
					}
				}
			}

			// ===================== 敌人速度 =====================
			GUILayout.Space(10f);
			DrawSectionHeader(L.T("enemies.speed_section"));
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			GUILayout.Label(L.T("enemies.speed_mult", enemySpeedMult.ToString("F1")), labelStyle, new GUILayoutOption[] { GUILayout.Width(250f) });
			float newSpeed = GUILayout.HorizontalSlider(enemySpeedMult, 0.1f, 5.0f, new GUILayoutOption[] { GUILayout.Width(200f) });
			GUILayout.EndHorizontal();
			if (Mathf.Abs(newSpeed - enemySpeedMult) > 0.01f)
			{
				enemySpeedMult = newSpeed;
				Enemies.SetSpeedMultiplier(enemySpeedMult);
			}
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (GUILayout.Button(L.T("enemies.speed_freeze"), buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				enemySpeedMult = 0.1f;
				Enemies.SetSpeedMultiplier(0.1f);
			}
			if (GUILayout.Button(L.T("enemies.speed_normal"), buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				enemySpeedMult = 1.0f;
				Enemies.SetSpeedMultiplier(1.0f);
			}
			if (GUILayout.Button(L.T("enemies.speed_fast"), buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				enemySpeedMult = 3.0f;
				Enemies.SetSpeedMultiplier(3.0f);
			}
			GUILayout.EndHorizontal();
		}
		catch (Exception arg)
		{
			Debug.LogError((object)$"[EnemiesTab] GUI Exception: {arg}");
		}
	}

	private void DrawItemsTab()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Expected O, but got Unknown
		//IL_042a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0434: Expected O, but got Unknown
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Expected O, but got Unknown
		//IL_044a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0454: Expected O, but got Unknown
		//IL_04b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0470: Unknown result type (might be due to invalid IL or missing references)
		//IL_0475: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0678: Unknown result type (might be due to invalid IL or missing references)
		//IL_0684: Unknown result type (might be due to invalid IL or missing references)
		//IL_068e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0693: Unknown result type (might be due to invalid IL or missing references)
		//IL_0698: Unknown result type (might be due to invalid IL or missing references)
		//IL_069d: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bd: Unknown result type (might be due to invalid IL or missing references)
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		DrawSectionHeader(L.T("items.auto"));
		ToggleLogic("auto_pickup", L.T("items.auto_pickup"), ref AutoPickup.isAutoPickupEnabled);
		if (AutoPickup.isAutoPickupEnabled)
		{
			GUILayout.Label(L.T("items.pickup_range_fmt", Mathf.RoundToInt(AutoPickup.pickupRadius)), labelStyle, Array.Empty<GUILayoutOption>());
			AutoPickup.pickupRadius = GUILayout.HorizontalSlider(AutoPickup.pickupRadius, 5f, 100f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
			GUILayout.Label(L.T("items.min_value_fmt", AutoPickup.minPickupValue), labelStyle, Array.Empty<GUILayoutOption>());
			AutoPickup.minPickupValue = (int)GUILayout.HorizontalSlider((float)AutoPickup.minPickupValue, 0f, 5000f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		}
		GUILayout.Space(5f);
		ToggleLogic("auto_sell", L.T("items.auto_sell"), ref AutoPickup.isAutoSellEnabled);
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("items.select"));
		List<ItemTeleport.GameItem> list = itemList.OrderByDescending((ItemTeleport.GameItem item) => item.Value).ToList();
		itemScroll = GUILayout.BeginScrollView(itemScroll, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(200f) });
		EnsureListStylesInitialized();
		for (int num = 0; num < list.Count; num++)
		{
			if (num == selectedItemIndex)
			{
				cachedItemListStyle.normal.background = itemBgSelected;
				cachedItemListStyle.normal.textColor = new Color(1f, 0.55f, 0.1f);
			}
			else
			{
				cachedItemListStyle.normal.background = itemBgNormal;
				cachedItemListStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
			}
			if (GUILayout.Button($"{list[num].Name}   {L.T("items.value_fmt", list[num].Value)}", cachedItemListStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(30f) }))
			{
				selectedItemIndex = num;
			}
		}
		GUILayout.EndScrollView();
		GUI.color = Color.white;
		if (GUILayout.Button(L.T("items.tp_to_me"), buttonStyle, Array.Empty<GUILayoutOption>()) && selectedItemIndex >= 0 && selectedItemIndex < list.Count)
		{
			ItemTeleport.TeleportItemToMe(list[selectedItemIndex]);
		}
		if (GUILayout.Button(L.T("items.tp_all"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ItemTeleport.TeleportAllItemsToMe();
		}
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("items.modify_value"));
		int num2 = (int)Mathf.Pow(10f, itemValueSliderPos);
		GUILayout.Label($"${num2:N0}", labelStyle, Array.Empty<GUILayoutOption>());
		itemValueSliderPos = GUILayout.HorizontalSlider(itemValueSliderPos, 3f, 9f, Array.Empty<GUILayoutOption>());
		if (GUILayout.Button(L.T("items.apply_value"), buttonStyle, Array.Empty<GUILayoutOption>()) && selectedItemIndex >= 0 && selectedItemIndex < list.Count)
		{
			ItemTeleport.SetItemValue(list[selectedItemIndex], num2);
		}

		// ===================== 物品价值膨胀 =====================
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("items.inflate_title"));
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button(L.T("items.inflate_10x"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ItemInflater.MultiplyAll(10f);
		}
		if (GUILayout.Button(L.T("items.inflate_100x"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ItemInflater.MultiplyAll(100f);
		}
		if (GUILayout.Button(L.T("items.inflate_max"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ItemInflater.InflateAll(99999f);
		}
		GUILayout.EndHorizontal();

		// ===================== 复制手持物品 =====================
		GUILayout.Space(5f);
		if (GUILayout.Button(L.T("items.duplicate"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ItemDuplicator.DuplicateHeldItem();
		}

		if (GUILayout.Button(showItemSpawner ? L.T("items.hide_spawner") : L.T("items.show_spawner"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			showItemSpawner = !showItemSpawner;
			if (showItemSpawner && availableItemsList.Count == 0)
			{
				availableItemsList = ItemSpawner.GetAvailableItems();
			}
		}
		if (showItemSpawner)
		{
			GUILayout.Space(10f);
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			GUILayout.Label(L.T("items.select_spawn"), labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(180f) });
			itemSpawnSearch = GUILayout.TextField(itemSpawnSearch, textFieldStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
			GUILayout.EndHorizontal();
			List<string> list2 = (string.IsNullOrWhiteSpace(itemSpawnSearch) ? availableItemsList : availableItemsList.Where((string item) => item.ToLower().Contains(itemSpawnSearch.ToLower())).ToList());
			itemSpawnerScroll = GUILayout.BeginScrollView(itemSpawnerScroll, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(150f) });
			for (int num3 = 0; num3 < list2.Count; num3++)
			{
				if (num3 == selectedItemToSpawnIndex)
				{
					cachedItemListStyle.normal.background = itemBgSelected;
					cachedItemListStyle.normal.textColor = new Color(1f, 0.55f, 0.1f);
				}
				else
				{
					cachedItemListStyle.normal.background = itemBgNormal;
					cachedItemListStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
				}
				if (GUILayout.Button(list2[num3], cachedItemListStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(30f) }))
				{
					selectedItemToSpawnIndex = availableItemsList.IndexOf(list2[num3]);
				}
			}
			GUILayout.EndScrollView();
			bool flag = availableItemsList.Count > 0 && selectedItemToSpawnIndex < availableItemsList.Count && availableItemsList[selectedItemToSpawnIndex].Contains("Valuable");
			if (flag)
			{
				GUILayout.Label($"Item Value: ${itemSpawnValue:n0}", labelStyle, Array.Empty<GUILayoutOption>());
				float num4 = Mathf.Log10((float)itemSpawnValue / 1000f) / 6f;
				float num5 = GUILayout.HorizontalSlider(num4, 0f, 1f, Array.Empty<GUILayoutOption>());
				if (num5 != num4 && isHost)
				{
					itemSpawnValue = Mathf.Clamp((int)(Mathf.Pow(10f, num5 * 6f) * 1000f), 1000, 1000000000);
				}
			}
			GUI.enabled = availableItemsList.Count > 0 && selectedItemToSpawnIndex < availableItemsList.Count;
			if (GUILayout.Button(L.T("items.spawn"), buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				GameObject localPlayer = DebugCheats.GetLocalPlayer();
				if ((Object)(object)localPlayer != (Object)null)
				{
					Vector3 position = localPlayer.transform.position + localPlayer.transform.forward * 1.5f + Vector3.up;
					string itemName = availableItemsList[selectedItemToSpawnIndex];
					if (flag)
					{
						ItemSpawner.SpawnItem(itemName, position, itemSpawnValue);
					}
					else
					{
						ItemSpawner.SpawnItem(itemName, position);
					}
				}
			}
			if (GUILayout.Button(L.T("items.spawn_50"), buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				ItemSpawner.SpawnSelectedItemMultiple(50, availableItemsList, selectedItemToSpawnIndex, itemSpawnValue);
			}
			GUI.enabled = true;
		}

		// ===================== 商店工具 =====================
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("items.shop_section"));
		if (GUILayout.Button(L.T("items.shop_free"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ShopHack.SetAllShopItemsFree();
		}
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(L.T("items.money_amount", ((int)moneySpawnAmount).ToString()), labelStyle, new GUILayoutOption[] { GUILayout.Width(250f) });
		moneySpawnAmount = GUILayout.HorizontalSlider(moneySpawnAmount, 1000f, 100000f, new GUILayoutOption[] { GUILayout.Width(200f) });
		GUILayout.EndHorizontal();
		if (GUILayout.Button(L.T("items.spawn_money"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ShopHack.SpawnMoneyAtPlayer((int)moneySpawnAmount);
		}
		if (GUILayout.Button(L.T("items.add_money"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ShopHack.AddMoney((int)moneySpawnAmount);
		}

		GUILayout.EndVertical();
	}

	private unsafe void DrawHotkeysTab()
	{
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		if (!string.IsNullOrEmpty(hotkeyManager.KeyAssignmentError) && Time.time - hotkeyManager.ErrorMessageTime < 3f)
		{
			GUIStyle val = new GUIStyle(GUI.skin.label)
			{
				fontSize = 14,
				fontStyle = (FontStyle)1
			};
			val.normal.textColor = Color.red;
			val.alignment = (TextAnchor)4;
			GUIStyle val2 = val;
			GUILayout.Label(hotkeyManager.KeyAssignmentError, val2, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(25f) });
		}
		DrawSectionHeader(L.T("hotkeys.config"));
		GUILayout.Space(5f);
		GUILayout.Label(L.T("hotkeys.how_to"), labelStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Label(L.T("hotkeys.step1"), labelStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Label(L.T("hotkeys.step2"), labelStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Space(10f);
		GUILayout.Label(L.T("hotkeys.warning"), warningStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("hotkeys.system"));
		DrawHotkeyField(L.T("hotkeys.menu_toggle"), hotkeyManager.MenuToggleKey, delegate
		{
			hotkeyManager.StartConfigureSystemKey(0);
		}, 0);
		DrawHotkeyField(L.T("hotkeys.reload"), hotkeyManager.ReloadKey, delegate
		{
			hotkeyManager.StartConfigureSystemKey(1);
		}, 1);
		DrawHotkeyField(L.T("hotkeys.unload"), hotkeyManager.UnloadKey, delegate
		{
			hotkeyManager.StartConfigureSystemKey(2);
		}, 2);
		GUILayout.Space(20f);
		DrawSectionHeader(L.T("hotkeys.action"));
		for (int num = 0; num < 12; num++)
		{
			KeyCode hotkeyForSlot = hotkeyManager.GetHotkeyForSlot(num);
			string obj = ((hotkeyManager.SelectedHotkeySlot == num && hotkeyManager.ConfiguringHotkey) ? L.T("hotkeys.press_key") : (((int)hotkeyForSlot == 0) ? L.T("hotkeys.not_set") : hotkeyForSlot.ToString()));
			string actionNameForKey = hotkeyManager.GetActionNameForKey(hotkeyForSlot);
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (GUILayout.Button(obj, buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				hotkeyManager.StartHotkeyConfiguration(num);
			}
			if (GUILayout.Button(actionNameForKey, buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				if ((int)hotkeyForSlot != 0)
				{
					showingActionSelector = true;
					hotkeyManager.ShowActionSelector(num, hotkeyForSlot);
				}
				else
				{
					Debug.Log((object)L.T("hotkeys.need_key"));
				}
			}
			if (GUILayout.Button(L.T("hotkeys.clear"), buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				hotkeyManager.ClearHotkeyBinding(num);
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.Space(10f);
		if (GUILayout.Button(L.T("hotkeys.save"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			hotkeyManager.SaveHotkeySettings();
			Debug.Log((object)L.T("hotkeys.saved"));
		}
		GUILayout.EndVertical();
	}

	private void DrawRoomTab()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_042e: Unknown result type (might be due to invalid IL or missing references)
		UpdatePlayerList();
		EnsureListStylesInitialized();

		// ===================== 强制夺取主机 =====================
		DrawSectionHeader(L.T("room.force_host"));
		GUILayout.Space(3f);
		if (PhotonNetwork.InRoom)
		{
			Photon.Realtime.Player master = PhotonNetwork.MasterClient;
			string masterName = (master != null) ? master.NickName : L.T("server.unknown");
			bool iAmHost = PhotonNetwork.IsMasterClient;
			GUILayout.Label(L.T("room.current_host") + " " + masterName + (iAmHost ? " (★)" : ""), labelStyle, Array.Empty<GUILayoutOption>());

			// 状态显示
			if (!string.IsNullOrEmpty(ForceHost.statusMessage))
			{
				GUILayout.Label(ForceHost.statusMessage, labelStyle, Array.Empty<GUILayoutOption>());
			}
			GUILayout.Space(3f);

			// 禁用状态下不允许点击
			bool disabled = ForceHost.IsProcessing;

			// 全自动按钮
			if (GUILayout.Button(disabled ? L.T("room.forcing") : L.T("room.auto_force"), buttonStyle, Array.Empty<GUILayoutOption>()) && !disabled)
			{
				ForceHost.Instance.StartCoroutine(ForceHost.Instance.Method_AutoAll());
			}
			GUILayout.Space(3f);

			GUILayout.Label(L.T("room.pick_method"), labelStyle, Array.Empty<GUILayoutOption>());
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (GUILayout.Button("1: SetMaster", buttonStyle, Array.Empty<GUILayoutOption>()) && !disabled)
			{
				ForceHost.Instance.StartCoroutine(ForceHost.Instance.Method_SetMasterClient());
			}
			if (GUILayout.Button("2: LowLevel", buttonStyle, Array.Empty<GUILayoutOption>()) && !disabled)
			{
				ForceHost.Instance.StartCoroutine(ForceHost.Instance.Method_LowLevelOp());
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (GUILayout.Button("3: ActorSpoof", buttonStyle, Array.Empty<GUILayoutOption>()) && !disabled)
			{
				ForceHost.Instance.StartCoroutine(ForceHost.Instance.Method_ActorNumberSpoof());
			}
			if (GUILayout.Button("4: " + L.T("room.crash_host"), buttonStyle, Array.Empty<GUILayoutOption>()) && !disabled)
			{
				ForceHost.Instance.StartCoroutine(ForceHost.Instance.Method_CrashHost());
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (GUILayout.Button("5: " + L.T("room.local_fake"), buttonStyle, Array.Empty<GUILayoutOption>()) && !disabled)
			{
				ForceHost.Method_LocalMasterFake();
			}
			GUILayout.EndHorizontal();
		}
		else
		{
			GUILayout.Label(L.T("room.not_in_room"), labelStyle, Array.Empty<GUILayoutOption>());
		}
		GUILayout.Space(15f);

		// ===================== 权限利用工具 =====================
		DrawSectionHeader("Authority Tools");
		GUILayout.Space(3f);
		if (PhotonNetwork.InRoom)
		{
			// ── Shadow Host ──
			ToggleLogic("shadow_host", "👻 Shadow Host", ref ShadowHostMode.isEnabled);
			if (ShadowHostMode.isEnabled || !string.IsNullOrEmpty(ShadowHostMode.statusMessage))
			{
				GUILayout.Label(ShadowHostMode.GetDiagnostics(), labelStyle, Array.Empty<GUILayoutOption>());
			}
			GUILayout.Space(3f);

			// ── Ownership 统计 ──
			var stats = AuthoritySpoofing.GetOwnershipStats();
			GUILayout.Label($"PhotonView: {stats}", labelStyle, Array.Empty<GUILayoutOption>());

			if (!string.IsNullOrEmpty(AuthoritySpoofing.statusMessage))
				GUILayout.Label(AuthoritySpoofing.statusMessage, labelStyle, Array.Empty<GUILayoutOption>());

			GUILayout.Space(3f);

			// ── 批量夺权按钮 ──
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (GUILayout.Button("夺取敌人", buttonStyle, Array.Empty<GUILayoutOption>()))
				AuthoritySpoofing.TakeOverEnemies();
			if (GUILayout.Button("夺取物品", buttonStyle, Array.Empty<GUILayoutOption>()))
				AuthoritySpoofing.TakeOverItems();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (GUILayout.Button("夺取全部", buttonStyle, Array.Empty<GUILayoutOption>()))
				AuthoritySpoofing.TakeOverAll();
			if (GUILayout.Button("夺取玩家", buttonStyle, Array.Empty<GUILayoutOption>()))
				AuthoritySpoofing.TakeOverPlayers();
			GUILayout.EndHorizontal();

			GUILayout.Space(5f);

			// ── RPC 注入器 ──
			DrawSectionHeader("RPC Injector");
			GUILayout.Space(3f);

			if (!string.IsNullOrEmpty(RpcInjector.statusMessage))
				GUILayout.Label(RpcInjector.statusMessage, labelStyle, Array.Empty<GUILayoutOption>());

			if (GUILayout.Button("扫描游戏 RPC", buttonStyle, Array.Empty<GUILayoutOption>()))
				RpcInjector.ScanAllRPCs(true);

			var hvRpcs = RpcInjector.GetAvailableHighValueRPCs();
			if (hvRpcs.Count > 0)
			{
				GUILayout.Label($"可用高价值 RPC: {hvRpcs.Count}", labelStyle, Array.Empty<GUILayoutOption>());

				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				if (GUILayout.Button("激活提取点", buttonStyle, Array.Empty<GUILayoutOption>()))
					RpcInjector.ActivateExtractionPoint();
				if (GUILayout.Button("设置金钱 99999", buttonStyle, Array.Empty<GUILayoutOption>()))
					RpcInjector.SetDollarValue(99999);
				GUILayout.EndHorizontal();
			}

			// ── RPC 队列状态 ──
			if (RpcInjector.QueuedCount > 0)
			{
				GUILayout.Space(3f);
				GUILayout.Label($"RPC 队列: {RpcInjector.QueuedCount} 个待执行", labelStyle, Array.Empty<GUILayoutOption>());
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				if (GUILayout.Button("立即执行队列", buttonStyle, Array.Empty<GUILayoutOption>()))
					RpcInjector.FlushRpcQueue();
				if (GUILayout.Button("清空队列", buttonStyle, Array.Empty<GUILayoutOption>()))
					RpcInjector.ClearQueue();
				GUILayout.EndHorizontal();
			}
		}
		else
		{
			GUILayout.Label(L.T("room.not_in_room"), labelStyle, Array.Empty<GUILayoutOption>());
		}
		GUILayout.Space(15f);

		// ===================== 世界工具 (从杂项迁移) =====================
		DrawSectionHeader(L.T("room.world_tools"));
		GUILayout.Space(5f);
		if (GUILayout.Button(L.T("misc.activate_extraction"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			MiscFeatures.ForceActivateAllExtractionPoints();
		}
		GUILayout.Space(5f);
		DrawSectionHeader(L.T("misc.map_tweaks"));
		if (GUILayout.Button(L.T("misc.disable_overlay"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			MapTools.changeOverlayStatus(status: true);
		}
		if (GUILayout.Button(L.T("misc.discover_valuables"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			MapTools.DiscoveryMapValuables();
		}
		GUILayout.Space(5f);
		DrawSectionHeader(L.T("misc.round_tools"));
		if (GUILayout.Button(L.T("misc.zero_haul"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			HaulGoalZero.ZeroHaulGoal();
		}
		if (!string.IsNullOrEmpty(HaulGoalZero.statusMessage))
		{
			GUILayout.Label(HaulGoalZero.statusMessage, labelStyle, Array.Empty<GUILayoutOption>());
		}
		if (GUILayout.Button(L.T("misc.auto_complete"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			AutoCompleteRound.Execute();
		}
		GUILayout.Space(5f);
		DrawSectionHeader(L.T("room.level_adjust"));
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button(L.T("room.level_adjust"), buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(100f) }))
		{
			LevelAdjust.RefreshLevels();
			showLevelAdjustDropdown = !showLevelAdjustDropdown;
		}
		if (levelAdjustIndex < LevelAdjust.availableLevels.Length)
		{
			GUILayout.Label(LevelAdjust.availableLevels[levelAdjustIndex], labelStyle, Array.Empty<GUILayoutOption>());
		}
		GUILayout.EndHorizontal();
		if (showLevelAdjustDropdown && LevelAdjust.availableLevels.Length > 0)
		{
			levelAdjustScroll = GUILayout.BeginScrollView(levelAdjustScroll, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(150f) });
			for (int lvl = 0; lvl < LevelAdjust.availableLevels.Length; lvl++)
			{
				if (GUILayout.Button(LevelAdjust.availableLevels[lvl], buttonStyle, Array.Empty<GUILayoutOption>()))
				{
					levelAdjustIndex = lvl;
					showLevelAdjustDropdown = false;
				}
			}
			GUILayout.EndScrollView();
		}
		if (GUILayout.Button(L.T("room.set_next_level"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			if (levelAdjustIndex < LevelAdjust.availableLevels.Length)
			{
				LevelAdjust.selectedLevelIndex = levelAdjustIndex;
				LevelAdjust.SetNextLevel();
			}
		}
		if (!string.IsNullOrEmpty(LevelAdjust.statusMessage))
		{
			GUILayout.Label(LevelAdjust.statusMessage, warningStyle, Array.Empty<GUILayoutOption>());
		}

		GUILayout.Space(15f);

		// ===================== 恶搞功能 =====================
		DrawSectionHeader(L.T("room.trolling"));
		GUILayout.Space(5f);
		DrawSectionHeader(L.T("room.select"));
		for (int i = 0; i < playerNames.Count; i++)
		{
			if (i == selectedPlayerIndex)
			{
				cachedPlayerListStyle.normal.background = rowBgSelected;
				cachedPlayerListStyle.normal.textColor = (Color)(new Color32(byte.MaxValue, (byte)165, (byte)0, byte.MaxValue));
			}
			else
			{
				cachedPlayerListStyle.normal.background = rowBgNormal;
				cachedPlayerListStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
				cachedPlayerListStyle.hover.background = rowBgHover;
				cachedPlayerListStyle.hover.textColor = Color.white;
			}
			if (GUILayout.Button(playerNames[i], cachedPlayerListStyle, Array.Empty<GUILayoutOption>()))
			{
				selectedPlayerIndex = i;
			}
		}
		GUILayout.Space(40f);
		if (GUILayout.Button(L.T("trolling.bubble"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ToiletFun[] array = Object.FindObjectsOfType<ToiletFun>();
			Cauldron[] array2 = Object.FindObjectsOfType<Cauldron>();
			ToiletFun[] array3 = array;
			for (int j = 0; j < array3.Length; j++)
			{
				((Component)array3[j]).GetComponent<PhotonView>().RPC("FlushStartRPC", (RpcTarget)0, Array.Empty<object>());
			}
			Cauldron[] array4 = array2;
			for (int j = 0; j < array4.Length; j++)
			{
				((Component)array4[j]).GetComponent<PhotonView>().RPC("CookStartRPC", (RpcTarget)0, Array.Empty<object>());
			}
		}
		GUILayout.Space(5f);
		if (GUILayout.Button(L.T("trolling.clown"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ClownTrap[] array5 = Object.FindObjectsOfType<ClownTrap>();
			for (int j = 0; j < array5.Length; j++)
			{
				PhotonView component = ((Component)array5[j]).GetComponent<PhotonView>();
				if ((Object)(object)component != (Object)null)
				{
					component.RPC("TouchNoseRPC", (RpcTarget)0, Array.Empty<object>());
				}
			}
		}
		GUILayout.Space(5f);
		if (GUILayout.Button(L.T("trolling.glitch"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			Troll.ForcePlayerGlitch();
		}
		GUILayout.Space(5f);
		if (GUILayout.Button(L.T("trolling.unmute"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			MiscFeatures.ForcePlayerMicVolume(100);
		}
		GUILayout.Space(5f);
		if (GUILayout.Button(L.T("trolling.mute"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			MiscFeatures.ForcePlayerMicVolume(-1);
		}
		GUILayout.Space(5f);
		if (GUILayout.Button(L.T("trolling.inf_tumble"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			Players.ForcePlayerTumble(9999999f);
		}
		GUILayout.Space(5f);
		if (GUILayout.Button(L.T("trolling.inf_loading"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			Troll.InfiniteLoadingSelectedPlayer();
		}
		GUILayout.Space(5f);
		if (GUILayout.Button(L.T("trolling.remove_loading"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			Troll.SceneRecovery();
		}
		GUILayout.Space(5f);
		if (GUILayout.Button(L.T("trolling.crash_player"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			MiscFeatures.CrashSelectedPlayerNew();
		}
		GUILayout.Space(5f);
		if (GUILayout.Button(L.T("trolling.detonate"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			Object.FindObjectOfType<ItemMine>();
			typeof(ItemGrenade).GetMethod("TickStartRPC", BindingFlags.Instance | BindingFlags.NonPublic);
			MiscFeatures.ExlploadAll();
			Debug.Log((object)"Detonated All Grenades/Mines");
		}
		GUILayout.Space(5f);
		if (GUILayout.Button(L.T("trolling.crash_lobby"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			GameObject localPlayer = DebugCheats.GetLocalPlayer();
			if ((Object)(object)localPlayer == (Object)null)
			{
				return;
			}
			Vector3 position = localPlayer.transform.position + Vector3.up * 1.5f;
			((Component)this).transform.position = position;
			CrashLobby.Crash(position);
		}
		GUILayout.Space(5f);

		// ===================== 假玩家 =====================
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("trolling.fake_players"));
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(L.T("trolling.fake_name"), labelStyle, new GUILayoutOption[] { GUILayout.Width(120f) });
		fakePlayerName = GUILayout.TextField(fakePlayerName, textFieldStyle, new GUILayoutOption[] { GUILayout.Width(200f) });
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(L.T("trolling.fake_count", ((int)fakePlayerCount).ToString()), labelStyle, new GUILayoutOption[] { GUILayout.Width(250f) });
		fakePlayerCount = GUILayout.HorizontalSlider(fakePlayerCount, 1f, 10f, new GUILayoutOption[] { GUILayout.Width(200f) });
		GUILayout.EndHorizontal();
		ToggleLogic("fake_3d", L.T("trolling.fake_3d_mode"), ref fakePlayerUse3D);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button(L.T("trolling.add_fakes") + (fakePlayerUse3D ? " (" + L.T("trolling.fake_host_only") + ")" : ""), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			FakePlayerManager.customFakeName = fakePlayerName;
			if (fakePlayerUse3D)
			{
				GameObject lp = DebugCheats.GetLocalPlayer();
				Vector3 spawnPos = (lp != null) ? lp.transform.position + Vector3.forward * 2f : Vector3.zero;
				FakePlayerManager.Add3DPlayers(fakePlayerName, (int)fakePlayerCount, spawnPos);
			}
			else
			{
				FakePlayerManager.AddGhostPlayers(fakePlayerName, (int)fakePlayerCount);
			}
		}
		GUILayout.EndHorizontal();
		// 当前假人状态
		if (FakePlayerManager.fakePlayerNames.Count > 0)
		{
			GUILayout.Label(L.T("trolling.fake_status", FakePlayerManager.fakePlayerNames.Count.ToString()), labelStyle, Array.Empty<GUILayoutOption>());
		}
		if (GUILayout.Button(L.T("trolling.remove_fakes"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			FakePlayerManager.RemoveAll();
		}
	}

	private void DrawConfigTab()
	{
		DrawSectionHeader(L.T("config.title"));
		GUILayout.Label(configstatus, titleStyle, Array.Empty<GUILayoutOption>());
		DrawSectionHeader(L.T("config.prefs"));
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button(L.T("config.save"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ConfigManager.SaveAllToggles();
			configstatus = L.T("config.saved");
		}
		if (GUILayout.Button(L.T("config.load"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ConfigManager.LoadAllToggles();
			configstatus = L.T("config.loaded");
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("config.json"));
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button(L.T("config.save_json"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			JsonConfig.SaveToFile();
			configstatus = L.T("config.json_saved_fmt", JsonConfig.GetConfigPath());
		}
		if (GUILayout.Button(L.T("config.load_json"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			JsonConfig.LoadFromFile();
			configstatus = L.T("config.json_loaded");
		}
		GUILayout.EndHorizontal();
		GUILayout.Label(L.T("config.json_path_fmt", JsonConfig.GetConfigPath()), labelStyle, Array.Empty<GUILayoutOption>());
	}

	private void DrawFeatureSelectionWindow(int id)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		DrawSectionHeader(L.T("common.select_feature"));
		actionScroll = GUILayout.BeginScrollView(actionScroll, false, true, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GUILayout.Width(380f),
			GUILayout.Height(320f)
		});
		List<HotkeyManager.HotkeyAction> availableActions = HotkeyManager.Instance.GetAvailableActions();
		for (int i = 0; i < availableActions.Count; i++)
		{
			if (GUILayout.Button(availableActions[i].Name, buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				HotkeyManager.Instance.AssignActionToHotkey(i);
				showingActionSelector = false;
			}
		}
		GUILayout.EndScrollView();
		GUILayout.Space(5f);
		if (GUILayout.Button(L.T("common.cancel"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			showingActionSelector = false;
		}
		GUI.DragWindow(new Rect(0f, 0f, 10000f, 30f));
	}

	private unsafe void DrawHotkeyField(string label, KeyCode key, Action configureCallback, int index)
	{
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(label, buttonStyle, Array.Empty<GUILayoutOption>());
		if (GUILayout.Button((hotkeyManager.ConfiguringSystemKey && hotkeyManager.SystemKeyConfigIndex == index && hotkeyManager.WaitingForAnyKey) ? L.T("hotkeys.press_key") : key.ToString(), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			configureCallback();
		}
		GUILayout.EndHorizontal();
	}

	private void DrawChamsColorWindow(int windowID)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		GUI.DragWindow(new Rect(0f, 0f, chamsWindowRect.width, 25f));
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		GUILayout.Space(5f);
		GUILayout.Label(L.T("common.chams_picker"), titleStyle, Array.Empty<GUILayoutOption>());
		string[] array = new string[4] { "Enemy Visible", "Enemy Hidden", "Item Visible", "Item Hidden" };
		for (int i = 0; i < array.Length; i++)
		{
			Color val = Color.white;
			switch (i)
			{
			case 0:
				val = DebugCheats.enemyVisibleColor;
				break;
			case 1:
				val = DebugCheats.enemyHiddenColor;
				break;
			case 2:
				val = DebugCheats.itemVisibleColor;
				break;
			case 3:
				val = DebugCheats.itemHiddenColor;
				break;
			}
			GUIStyle val2 = new GUIStyle(GUI.skin.button);
			val2.normal.background = colorpicker(val);
			val2.normal.textColor = GetContrastColor(val);
			val2.fontStyle = (FontStyle)1;
			val2.alignment = (TextAnchor)4;
			if (GUILayout.Button(array[i], val2, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(25f) }))
			{
				selectedColorOption = i;
			}
		}
		GUILayout.Space(10f);
		Color val3 = Color.white;
		if (selectedColorOption == 0)
		{
			val3 = DebugCheats.enemyVisibleColor;
		}
		else if (selectedColorOption == 1)
		{
			val3 = DebugCheats.enemyHiddenColor;
		}
		else if (selectedColorOption == 2)
		{
			val3 = DebugCheats.itemVisibleColor;
		}
		else if (selectedColorOption == 3)
		{
			val3 = DebugCheats.itemHiddenColor;
		}
		float num = 200f;
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.FlexibleSpace();
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		GUILayout.Label("R", labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(20f) });
		GUILayout.Space(5f);
		float num2 = GUILayout.VerticalSlider(val3.r, 1f, 0f, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GUILayout.Height(num),
			GUILayout.Width(20f)
		});
		GUILayout.EndVertical();
		GUILayout.Space(20f);
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		GUILayout.Label("G", labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(20f) });
		GUILayout.Space(5f);
		float num3 = GUILayout.VerticalSlider(val3.g, 1f, 0f, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GUILayout.Height(num),
			GUILayout.Width(20f)
		});
		GUILayout.EndVertical();
		GUILayout.Space(20f);
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		GUILayout.Label("B", labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(20f) });
		GUILayout.Space(5f);
		float num4 = GUILayout.VerticalSlider(val3.b, 1f, 0f, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GUILayout.Height(num),
			GUILayout.Width(20f)
		});
		GUILayout.EndVertical();
		GUILayout.Space(20f);
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		GUILayout.Label("A", labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(20f) });
		GUILayout.Space(5f);
		float num5 = GUILayout.VerticalSlider(val3.a, 1f, 0f, (GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GUILayout.Height(num),
			GUILayout.Width(20f)
		});
		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		Color val4 = new Color(num2, num3, num4, num5);
		if (val4 != val3)
		{
			if (selectedColorOption == 0)
			{
				DebugCheats.enemyVisibleColor = val4;
			}
			else if (selectedColorOption == 1)
			{
				DebugCheats.enemyHiddenColor = val4;
			}
			else if (selectedColorOption == 2)
			{
				DebugCheats.itemVisibleColor = val4;
			}
			else if (selectedColorOption == 3)
			{
				DebugCheats.itemHiddenColor = val4;
			}
		}
		GUILayout.EndVertical();
	}

	private unsafe void DrawServersTab()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0435: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0827: Unknown result type (might be due to invalid IL or missing references)
		//IL_082c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0840: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_08cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_08fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_085e: Unknown result type (might be due to invalid IL or missing references)
		//IL_046e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_04df: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0505: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0514: Expected O, but got Unknown
		//IL_0514: Unknown result type (might be due to invalid IL or missing references)
		//IL_0519: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Expected O, but got Unknown
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_0528: Unknown result type (might be due to invalid IL or missing references)
		//IL_0532: Expected O, but got Unknown
		//IL_0534: Expected O, but got Unknown
		//IL_0536: Unknown result type (might be due to invalid IL or missing references)
		//IL_0540: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0574: Unknown result type (might be due to invalid IL or missing references)
		//IL_0579: Unknown result type (might be due to invalid IL or missing references)
		//IL_0697: Unknown result type (might be due to invalid IL or missing references)
		//IL_069c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rowBgNormal == (Object)null)
		{
			rowBgNormal = MakeSolidBackground((Color)(new Color32((byte)30, (byte)30, (byte)30, byte.MaxValue)));
			rowBgHover = MakeSolidBackground((Color)(new Color32((byte)40, (byte)40, (byte)50, byte.MaxValue)));
			rowBgSelected = MakeSolidBackground((Color)(new Color32((byte)50, (byte)50, (byte)70, byte.MaxValue)));
		}
		EnsureListStylesInitialized();
		GUILayout.Label(L.T("server.title"), titleStyle, Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button(L.T("server.refresh"), buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(160f) }))
		{
			LobbyHostCache.Clear();
			LobbyMemberCache.Clear();
			LobbyFinder.AlreadyTriedLobbies.Clear();
			LobbyFinder.RefreshLobbies();
		}
		ToggleLogic("hide_full_lobbies", L.T("server.hide_full"), ref hideFullLobbies);
		ToggleLogic("show_lobby_members", L.T("server.show_members"), ref showMemberWindow);
		GUILayout.EndHorizontal();

		// ===== 区域筛选下拉 =====
		GUILayout.Space(5f);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(L.T("server.region_filter"), labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(80f) });
		string regionBtnText = (regionFilterIndex >= 0 && regionFilterIndex < regionFilterOptions.Length) ? regionFilterOptions[regionFilterIndex] : regionFilterOptions[0];
		if (GUILayout.Button(regionBtnText, buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(120f) }))
		{
			showRegionDropdown = !showRegionDropdown;
		}
		if (showRegionDropdown)
		{
			for (int ri = 0; ri < regionFilterOptions.Length; ri++)
			{
				if (ri != regionFilterIndex && GUILayout.Button(regionFilterOptions[ri], buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(100f) }))
				{
					regionFilterIndex = ri;
					showRegionDropdown = false;
					cachedSortedLobbies = null; // 重新排序
				}
			}
		}

		// ===== 快速创建房间 =====
		GUILayout.Space(20f);
		if (GUILayout.Button(showCreateRoom ? L.T("server.hide_create") : L.T("server.show_create"), buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(120f) }))
		{
			showCreateRoom = !showCreateRoom;
		}
		GUILayout.EndHorizontal();

		if (showCreateRoom)
		{
			GUILayout.Space(5f);
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			GUILayout.Label(L.T("server.room_name"), labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(80f) });
			createRoomName = GUILayout.TextField(createRoomName, textFieldStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
			GUILayout.Space(10f);
			GUILayout.Label(L.T("server.max_players", createRoomMaxPlayers), labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(120f) });
			createRoomMaxPlayers = (int)GUILayout.HorizontalSlider(createRoomMaxPlayers, 2f, 20f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(150f) });
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (GUILayout.Button(RoomCreator.IsCreating ? L.T("room.creating") : L.T("server.create_room"), buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(160f) }))
			{
				if (!RoomCreator.IsCreating)
					RoomCreator.CreateRoom(createRoomName, createRoomMaxPlayers);
			}
			if (!string.IsNullOrEmpty(RoomCreator.StatusText))
			{
				GUILayout.Label(RoomCreator.StatusText, labelStyle, Array.Empty<GUILayoutOption>());
			}
			GUILayout.EndHorizontal();
		}

		GUILayout.Space(10f);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(L.T("server.search"), labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(70f) });
		lobbySearchTerm = GUILayout.TextField(lobbySearchTerm, textFieldStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(260f) });
		if (GUILayout.Button(L.T("server.sort_az"), buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(100f) }))
		{
			sortMode = SortMode.RegionAZ;
		}
		if (GUILayout.Button(L.T("server.sort_za"), buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(100f) }))
		{
			sortMode = SortMode.RegionZA;
		}
		if (GUILayout.Button(L.T("server.sort_most"), buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(110f) }))
		{
			sortMode = SortMode.MostPlayers;
		}
		if (GUILayout.Button(L.T("server.sort_least"), buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(120f) }))
		{
			sortMode = SortMode.LeastPlayers;
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(L.T("server.col_name"), labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(280f) });
		GUILayout.Space(10f);
		GUILayout.Label(L.T("server.col_players"), labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(80f) });
		GUILayout.Space(10f);
		GUILayout.Label(L.T("server.col_region"), labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(80f) });
		GUILayout.Space(50f);
		GUILayout.Label(L.T("server.col_host"), labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(true) });
		GUILayout.EndHorizontal();
		GUILayout.Space(6f);
		serverListScroll = GUILayout.BeginScrollView(serverListScroll, boxStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(500f) });
		int currentLobbyCount = (LobbyFinder.FoundLobbies != null) ? LobbyFinder.FoundLobbies.Count : 0;
		if (cachedSortedLobbies == null || cachedSortMode != sortMode || cachedLobbyCount != currentLobbyCount)
		{
			cachedSortedLobbies = new List<Lobby>(LobbyFinder.FoundLobbies);
			switch (sortMode)
			{
			case SortMode.RegionAZ:
				cachedSortedLobbies.Sort((Lobby a, Lobby b) => string.Compare(a.GetData("Region"), b.GetData("Region")));
				break;
			case SortMode.RegionZA:
				cachedSortedLobbies.Sort((Lobby a, Lobby b) => string.Compare(b.GetData("Region"), a.GetData("Region")));
				break;
			case SortMode.MostPlayers:
				cachedSortedLobbies.Sort((Lobby a, Lobby b) => b.MemberCount.CompareTo(a.MemberCount));
				break;
			case SortMode.LeastPlayers:
				cachedSortedLobbies.Sort((Lobby a, Lobby b) => a.MemberCount.CompareTo(b.MemberCount));
				break;
			}
			cachedSortMode = sortMode;
			cachedLobbyCount = currentLobbyCount;
		}
		foreach (Lobby item in cachedSortedLobbies)
		{
			Lobby current = item;
			if ((hideFullLobbies && current.MemberCount >= current.MaxMembers) || !LobbyHostCache.ContainsKey(current.Id) || current.MemberCount < 3)
			{
				continue;
			}
			// 区域筛选
			if (regionFilterIndex > 0 && regionFilterIndex < regionFilterKeywords.Length)
			{
				string lobbyRegion = (current.GetData("Region") ?? "").ToLower();
				bool regionMatch = false;
				foreach (string kw in regionFilterKeywords[regionFilterIndex])
				{
					if (lobbyRegion.Contains(kw)) { regionMatch = true; break; }
				}
				if (!regionMatch) continue;
			}
			string value;
			string text = (LobbyHostCache.TryGetValue(current.Id, out value) ? value : L.T("server.fetching"));
			if (!text.Contains("Failed (0)") && (string.IsNullOrWhiteSpace(lobbySearchTerm) || ((object)current.Id/*cast due to .constrained prefix*/).ToString().IndexOf(lobbySearchTerm, StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf(lobbySearchTerm, StringComparison.OrdinalIgnoreCase) >= 0 || (LobbyMemberCache.TryGetValue(current.Id, out var value2) && value2.Exists((string m) => m.IndexOf(lobbySearchTerm, StringComparison.OrdinalIgnoreCase) >= 0))))
			{
				if (current.Id.Value == selectedLobbyId.Value)
				{
					cachedServerRowStyle.normal.background = rowBgSelected;
					cachedServerRowStyle.normal.textColor = (Color)(new Color32(byte.MaxValue, (byte)165, (byte)0, byte.MaxValue));
				}
				else
				{
					cachedServerRowStyle.normal.background = rowBgNormal;
					cachedServerRowStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
					cachedServerRowStyle.hover.background = rowBgHover;
					cachedServerRowStyle.hover.textColor = Color.white;
				}
				GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				string text2 = (text.Contains("(") ? text.Substring(0, text.IndexOf("(")).Trim() : text);
				string text3 = L.T("server.room_fmt", string.IsNullOrWhiteSpace(text2) ? L.T("server.unknown") : text2);
				if (current.MaxMembers > 6)
				{
					text3 += " <color=red>" + L.T("server.modded") + "</color>";
				}
				string data = current.GetData("Region");
				int num = Mathf.Max(0, current.MemberCount - 1);
				int maxMembers = current.MaxMembers;
				if (GUILayout.Button(text3, cachedServerRowStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(280f) }))
				{
					selectedLobbyId = current.Id;
				}
				GUILayout.Space(10f);
				GUILayout.Label(num + "/" + maxMembers, labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(80f) });
				GUILayout.Space(10f);
				GUILayout.Label(data, labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(80f) });
				GUILayout.Space(10f);
				GUILayout.Label(text, labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(260f) });
				GUILayout.EndHorizontal();
				GUILayout.Space(6f);
				GUILayout.EndVertical();
				GUILayout.Space(4f);
			}
		}
		GUILayout.EndScrollView();
		GUILayout.Space(10f);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button(L.T("server.join"), buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(120f) }))
		{
			Lobby lobby = LobbyFinder.FoundLobbies.Find((Lobby l) => l.Id.Value == selectedLobbyId.Value);
			if (lobby.Id.Value != 0uL)
			{
				LobbyFinder.JoinLobbyAndPlay(lobby);
			}
		}
		GUILayout.Space(12f);
		if (selectedLobbyId.Value != 0uL)
		{
			Lobby val2 = LobbyFinder.FoundLobbies.Find((Lobby l) => l.Id.Value == selectedLobbyId.Value);
			string data2 = val2.GetData("Region");
			string value3;
			string arg = (LobbyHostCache.TryGetValue(selectedLobbyId, out value3) ? value3 : L.T("server.unknown"));
			GUILayout.Label(L.T("server.selected_fmt", selectedLobbyId, arg, data2), labelStyle, Array.Empty<GUILayoutOption>());
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button(L.T("server.invite"), buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(150f) }))
		{
			Lobby val3 = LobbyFinder.FoundLobbies.Find((Lobby l) => l.Id.Value == selectedLobbyId.Value);
			Friend owner = val3.Owner;
			string arg2 = owner.Id.ToString();
			string text4 = (GUIUtility.systemCopyBuffer = $"steam://joinlobby/3241660/{val3.Id}/{arg2}");
			Debug.Log((object)("[InviteLink] 已复制：" + text4));
		}
		GUILayout.EndHorizontal();
	}

	private void DrawTextEditorPopup(int id)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		GUILayout.BeginVertical(boxStyle, Array.Empty<GUILayoutOption>());
		textboxscroll = GUILayout.BeginScrollView(textboxscroll, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(235f) });
		largeTextBoxContent = GUILayout.TextArea(largeTextBoxContent, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandHeight(true) });
		GUILayout.EndScrollView();
		if (activeTextFieldId == "SpoofNameField")
		{
			spoofedNameText = largeTextBoxContent;
		}
		else if (activeTextFieldId == "PersistentSpoofNameField")
		{
			persistentNameText = largeTextBoxContent;
		}
		else if (activeTextFieldId == "chatmessageField")
		{
			chatMessageText = largeTextBoxContent;
		}
		GUILayout.Space(10f);
		if (GUILayout.Button(L.T("common.close"), buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(25f) }))
		{
			showTextEditorPopup = false;
			activeTextFieldId = null;
		}
		GUILayout.EndVertical();
		GUI.DragWindow();
	}

	private void DrawLobbyMemberWindow(int id)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		GUILayout.BeginVertical(boxStyle, Array.Empty<GUILayoutOption>());
		DrawSectionHeader(L.T("server.members"));
		GUILayout.Space(4f);
		memberWindowScroll = GUILayout.BeginScrollView(memberWindowScroll, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(150f) });
		if (LobbyMemberCache.TryGetValue(selectedLobbyId, out var value))
		{
			foreach (string item in value)
			{
				if (!item.Contains(SteamClient.Name))
				{
					GUILayout.Label("•" + item, labelStyle, Array.Empty<GUILayoutOption>());
					GUILayout.Space(5f);
				}
			}
		}
		else
		{
			GUILayout.Label(L.T("common.fetching_players"), warningStyle, Array.Empty<GUILayoutOption>());
		}
		GUILayout.EndScrollView();
		GUILayout.Space(8f);
		if (GUILayout.Button(L.T("common.close"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			showMemberWindow = false;
		}
		GUILayout.EndVertical();
		GUI.DragWindow();
	}

	private void InitStyles()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00af: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Expected O, but got Unknown
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Expected O, but got Unknown
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Expected O, but got Unknown
		//IL_028a: Expected O, but got Unknown
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Expected O, but got Unknown
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Expected O, but got Unknown
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Expected O, but got Unknown
		//IL_02d7: Expected O, but got Unknown
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Expected O, but got Unknown
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Expected O, but got Unknown
		//IL_03a6: Expected O, but got Unknown
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0455: Expected O, but got Unknown
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0479: Expected O, but got Unknown
		//IL_0490: Unknown result type (might be due to invalid IL or missing references)
		//IL_0495: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0515: Unknown result type (might be due to invalid IL or missing references)
		//IL_051f: Expected O, but got Unknown
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_0543: Expected O, but got Unknown
		//IL_055a: Unknown result type (might be due to invalid IL or missing references)
		//IL_055f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0585: Unknown result type (might be due to invalid IL or missing references)
		//IL_058a: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05df: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e9: Expected O, but got Unknown
		//IL_0613: Unknown result type (might be due to invalid IL or missing references)
		//IL_061d: Expected O, but got Unknown
		//IL_0627: Unknown result type (might be due to invalid IL or missing references)
		//IL_0631: Expected O, but got Unknown
		//IL_063b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0645: Expected O, but got Unknown
		//IL_065c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0661: Unknown result type (might be due to invalid IL or missing references)
		//IL_068a: Unknown result type (might be due to invalid IL or missing references)
		//IL_068f: Unknown result type (might be due to invalid IL or missing references)
		//IL_06be: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f7: Expected O, but got Unknown
		//IL_0721: Unknown result type (might be due to invalid IL or missing references)
		//IL_072b: Expected O, but got Unknown
		//IL_0739: Unknown result type (might be due to invalid IL or missing references)
		//IL_0743: Expected O, but got Unknown
		//IL_074d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0757: Expected O, but got Unknown
		//IL_076e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0773: Unknown result type (might be due to invalid IL or missing references)
		//IL_079c: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0804: Unknown result type (might be due to invalid IL or missing references)
		//IL_080c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0813: Unknown result type (might be due to invalid IL or missing references)
		//IL_081a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0821: Unknown result type (might be due to invalid IL or missing references)
		//IL_082b: Expected O, but got Unknown
		//IL_082b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0830: Unknown result type (might be due to invalid IL or missing references)
		//IL_083a: Expected O, but got Unknown
		//IL_083a: Unknown result type (might be due to invalid IL or missing references)
		//IL_083f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0849: Expected O, but got Unknown
		//IL_084e: Expected O, but got Unknown
		//IL_08c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_08de: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_090f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0911: Unknown result type (might be due to invalid IL or missing references)
		//IL_0927: Unknown result type (might be due to invalid IL or missing references)
		//IL_0928: Unknown result type (might be due to invalid IL or missing references)
		//IL_0942: Unknown result type (might be due to invalid IL or missing references)
		//IL_0944: Unknown result type (might be due to invalid IL or missing references)
		//IL_0959: Unknown result type (might be due to invalid IL or missing references)
		//IL_095e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0966: Unknown result type (might be due to invalid IL or missing references)
		//IL_096d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0973: Unknown result type (might be due to invalid IL or missing references)
		//IL_0982: Expected O, but got Unknown
		//IL_0995: Unknown result type (might be due to invalid IL or missing references)
		//IL_099f: Expected O, but got Unknown
		//IL_09aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_09bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_09cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_09de: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ef: Expected O, but got Unknown
		//IL_09ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fe: Expected O, but got Unknown
		//IL_09fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a03: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a0d: Expected O, but got Unknown
		//IL_0a0d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a14: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a1b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a2d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a32: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a41: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a5b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a60: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a6a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a7c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a81: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a90: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a96: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ac6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0acc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ae8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0afc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b02: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b11: Expected O, but got Unknown
		//IL_0b1c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b26: Expected O, but got Unknown
		//IL_0b32: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b37: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b95: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b9f: Expected O, but got Unknown
		//IL_0ba9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bb3: Expected O, but got Unknown
		//IL_0bbd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc7: Expected O, but got Unknown
		//IL_0bf2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bfc: Expected O, but got Unknown
		//IL_0c13: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c18: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c52: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c5c: Expected O, but got Unknown
		//IL_0c73: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c78: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c95: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c9f: Expected O, but got Unknown
		// Check if textures were destroyed by Unity scene transition
		if (titleStyle != null && buttonStyle != null)
		{
			if ((Object)(object)buttonStyle.normal.background == (Object)null ||
			    (Object)(object)tabStyle?.normal?.background == (Object)null)
			{
				titleStyle = null;
				listStylesInitialized = false;
			}
		}
		if (titleStyle == null)
		{
			activeTheme = ThemeData.GetTheme(currentTheme);
			GUIStyle val = new GUIStyle(GUI.skin.label)
			{
				fontSize = 20,
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)4
			};
			val.normal.textColor = activeTheme.titleColor;
			titleStyle = val;
			tabStyle = new GUIStyle(GUI.skin.button)
			{
				fontSize = 14,
				fontStyle = (FontStyle)1,
				fixedHeight = 27f,
				alignment = (TextAnchor)4,
				padding = new RectOffset(8, 8, 4, 4),
				margin = new RectOffset(4, 4, 2, 2)
			};
			tabStyle.normal.textColor = activeTheme.tabNormalText;
			tabStyle.normal.background = MakeSolidBackground(activeTheme.tabNormalBg);
			tabStyle.hover.textColor = activeTheme.tabHoverText;
			tabStyle.hover.background = MakeSolidBackground(activeTheme.tabHoverBg);
			tabStyle.active.textColor = Color.white;
			tabStyle.active.background = MakeSolidBackground(activeTheme.tabActiveBg);
			tabSelectedStyle = new GUIStyle(tabStyle);
			tabSelectedStyle.normal.background = MakeSolidBackground(activeTheme.tabSelectedBg);
			tabSelectedStyle.normal.textColor = activeTheme.tabSelectedText;
			tabSelectedStyle.fontStyle = (FontStyle)1;
			GUIStyle val2 = new GUIStyle(GUI.skin.label)
			{
				fontSize = 16,
				fontStyle = (FontStyle)1,
				padding = new RectOffset(8, 0, 2, 2)
			};
			val2.normal.textColor = activeTheme.sectionHeaderText;
			sectionHeaderStyle = val2;
			GUIStyle val3 = new GUIStyle(GUI.skin.box);
			val3.normal.background = MakeSolidBackground(activeTheme.boxBg);
			val3.padding = new RectOffset(10, 10, 10, 10);
			boxStyle = val3;
			scrollbarStyle = new GUIStyle(GUI.skin.verticalScrollbar)
			{
				fixedWidth = 12f,
				border = new RectOffset(4, 4, 4, 4),
				margin = new RectOffset(2, 2, 2, 2),
				padding = new RectOffset(0, 0, 0, 0)
			};
			scrollbarStyle.normal.background = MakeSolidBackground((Color)(new Color32((byte)25, (byte)25, (byte)25, byte.MaxValue)));
			scrollbarStyle.hover.background = MakeSolidBackground((Color)(new Color32((byte)35, (byte)35, (byte)35, byte.MaxValue)));
			scrollbarStyle.active.background = MakeSolidBackground((Color)(new Color32((byte)45, (byte)45, (byte)45, byte.MaxValue)));
			GUI.skin.verticalScrollbar = scrollbarStyle;
			scrollbarThumbStyle = new GUIStyle(GUI.skin.verticalScrollbarThumb)
			{
				fixedWidth = 8f,
				margin = new RectOffset(2, 2, 2, 2),
				border = new RectOffset(4, 4, 4, 4)
			};
			scrollbarThumbStyle.normal.background = MakeSolidBackground((Color)(new Color32((byte)90, (byte)90, (byte)90, byte.MaxValue)));
			scrollbarThumbStyle.hover.background = MakeSolidBackground((Color)(new Color32((byte)110, (byte)110, (byte)110, byte.MaxValue)));
			scrollbarThumbStyle.active.background = MakeSolidBackground((Color)(new Color32((byte)130, (byte)130, (byte)130, byte.MaxValue)));
			GUI.skin.verticalScrollbarThumb = scrollbarThumbStyle;
			horizontalSliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
			horizontalSliderStyle.fixedHeight = 12f;
			horizontalSliderStyle.margin = new RectOffset(4, 4, 6, 6);
			horizontalSliderStyle.normal.background = MakeSolidBackground((Color)(new Color32((byte)40, (byte)40, (byte)40, byte.MaxValue)));
			horizontalSliderStyle.hover.background = MakeSolidBackground((Color)(new Color32((byte)50, (byte)50, (byte)50, byte.MaxValue)));
			horizontalSliderStyle.active.background = MakeSolidBackground((Color)(new Color32((byte)60, (byte)60, (byte)60, byte.MaxValue)));
			GUI.skin.horizontalSlider = horizontalSliderStyle;
			verticalSliderStyle = new GUIStyle(GUI.skin.verticalSlider);
			verticalSliderStyle.fixedHeight = 190f;
			verticalSliderStyle.margin = new RectOffset(4, 4, 6, 6);
			verticalSliderStyle.normal.background = MakeSolidBackground((Color)(new Color32((byte)40, (byte)40, (byte)40, byte.MaxValue)));
			verticalSliderStyle.hover.background = MakeSolidBackground((Color)(new Color32((byte)50, (byte)50, (byte)50, byte.MaxValue)));
			verticalSliderStyle.active.background = MakeSolidBackground((Color)(new Color32((byte)60, (byte)60, (byte)60, byte.MaxValue)));
			GUI.skin.verticalSlider = verticalSliderStyle;
			horizontalThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
			horizontalThumbStyle.fixedWidth = 16f;
			horizontalThumbStyle.fixedHeight = 16f;
			horizontalThumbStyle.border = new RectOffset(4, 4, 4, 4);
			horizontalThumbStyle.margin = new RectOffset(0, 0, 0, 0);
			horizontalThumbStyle.padding = new RectOffset(0, 0, 0, 0);
			horizontalThumbStyle.normal.background = MakeSolidBackground(activeTheme.sliderThumbNormal);
			horizontalThumbStyle.hover.background = MakeSolidBackground(activeTheme.sliderThumbHover);
			horizontalThumbStyle.active.background = MakeSolidBackground(activeTheme.sliderThumbActive);
			GUI.skin.horizontalSliderThumb = horizontalThumbStyle;
			verticalThumbStyle = new GUIStyle(GUI.skin.verticalSliderThumb);
			verticalThumbStyle.fixedWidth = 16f;
			verticalThumbStyle.fixedHeight = 16f;
			verticalThumbStyle.border = new RectOffset(4, 4, 4, 4);
			verticalThumbStyle.margin = new RectOffset(-2, -2, -2, -2);
			verticalThumbStyle.padding = new RectOffset(0, 0, 0, 0);
			verticalThumbStyle.normal.background = MakeSolidBackground(activeTheme.sliderThumbNormal);
			verticalThumbStyle.hover.background = MakeSolidBackground(activeTheme.sliderThumbHover);
			verticalThumbStyle.active.background = MakeSolidBackground(activeTheme.sliderThumbActive);
			GUI.skin.verticalSliderThumb = verticalThumbStyle;
			buttonStyle = new GUIStyle(GUI.skin.button)
			{
				fontSize = 14,
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)4,
				padding = new RectOffset(12, 12, 6, 6),
				margin = new RectOffset(4, 4, 4, 4),
				border = new RectOffset(6, 6, 6, 6)
			};
			buttonStyle.normal.background = MakeSolidBackground(activeTheme.buttonNormalBg);
			buttonStyle.normal.textColor = activeTheme.buttonNormalText;
			buttonStyle.hover.background = MakeSolidBackground(activeTheme.buttonHoverBg);
			buttonStyle.hover.textColor = activeTheme.buttonHoverText;
			buttonStyle.active.background = MakeSolidBackground(activeTheme.buttonActiveBg);
			buttonStyle.active.textColor = activeTheme.buttonActiveText;
			GUIStyle val9 = new GUIStyle(GUI.skin.label)
			{
				fontSize = 14,
				fontStyle = (FontStyle)1
			};
			val9.normal.textColor = Color.white;
			labelStyle = val9;
			labelStyle.richText = true;
			warningStyle = new GUIStyle(labelStyle);
			warningStyle.normal.textColor = Color.yellow;
			GUIStyle val10 = new GUIStyle(GUI.skin.textField)
			{
				fontSize = 14,
				alignment = (TextAnchor)3,
				fixedHeight = 28f,
				padding = new RectOffset(10, 10, 6, 6),
				margin = new RectOffset(4, 4, 4, 4),
				border = new RectOffset(4, 4, 4, 4),
				wordWrap = false,
				clipping = (TextClipping)1
			};
			val10.normal.background = MakeSolidBackground((Color)(new Color32((byte)50, (byte)50, (byte)60, byte.MaxValue)));
			val10.normal.textColor = (Color)(new Color32((byte)230, (byte)230, (byte)230, byte.MaxValue));
			val10.focused.background = MakeSolidBackground((Color)(new Color32((byte)60, (byte)60, (byte)80, byte.MaxValue)));
			val10.focused.textColor = Color.white;
			val10.hover.background = MakeSolidBackground((Color)(new Color32((byte)55, (byte)55, (byte)70, byte.MaxValue)));
			val10.hover.textColor = Color.white;
			val10.active.background = MakeSolidBackground((Color)(new Color32((byte)70, (byte)70, (byte)90, byte.MaxValue)));
			val10.active.textColor = Color.white;
			textFieldStyle = val10;
			backgroundStyle = new GUIStyle(GUI.skin.window);
			Texture2D background = MakeSolidBackground(activeTheme.windowBg);
			backgroundStyle.normal.background = background;
			backgroundStyle.onNormal.background = background;
			backgroundStyle.focused.background = background;
			backgroundStyle.onFocused.background = background;
			backgroundStyle.border = new RectOffset(0, 0, 0, 0);
			backgroundStyle.margin = new RectOffset(0, 0, 0, 0);
			backgroundStyle.padding = new RectOffset(0, 0, 0, 0);
		}
		if (backgroundStyle == null || (Object)(object)backgroundStyle.normal.background == (Object)null)
		{
			if (activeTheme == null) activeTheme = ThemeData.GetTheme(currentTheme);
			backgroundStyle = new GUIStyle(GUI.skin.window);
			backgroundStyle.normal.background = MakeSolidBackground(activeTheme.windowBg);
		}
		if (boxStyle == null || (Object)(object)boxStyle.normal.background == (Object)null)
		{
			if (activeTheme == null) activeTheme = ThemeData.GetTheme(currentTheme);
			boxStyle = new GUIStyle(GUI.skin.box);
			boxStyle.normal.background = MakeSolidBackground(activeTheme.boxBg);
			boxStyle.padding = new RectOffset(10, 10, 10, 10);
		}
	}

	private void HandleResize()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Invalid comparison between Unknown and I4
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Invalid comparison between Unknown and I4
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Invalid comparison between Unknown and I4
		float num = 16f;
		Rect val = new Rect(menuRect.xMax - num, menuRect.yMax - num, num, num);
		bool num2 = val.Contains(Event.current.mousePosition);
		Color val2 = new Color(1f, 0.7f, 0.2f, 1f);
		Color val3 = new Color(1f, 0.9f, 0.4f, 1f);
		GUI.color = (num2 ? val3 : val2);
		GUI.DrawTexture(val, (Texture)(object)Texture2D.whiteTexture);
		GUI.color = Color.white;
		GUIStyle val4 = new GUIStyle(GUI.skin.label)
		{
			alignment = (TextAnchor)4,
			fontSize = 10,
			fontStyle = (FontStyle)1
		};
		val4.normal.textColor = Color.white;
		GUIStyle val5 = val4;
		GUI.Label(val, "⧫", val5);
		if ((int)Event.current.type == 0 && val.Contains(Event.current.mousePosition))
		{
			isResizing = true;
			resizeStartMousePos = Event.current.mousePosition;
			resizeStartSize = new Vector2(menuRect.width, menuRect.height);
			Event.current.Use();
		}
		if (isResizing)
		{
			if ((int)Event.current.type == 3)
			{
				Vector2 val6 = Event.current.mousePosition - resizeStartMousePos;
				menuRect.width = Mathf.Clamp(resizeStartSize.x + val6.x, 400f, 1200f);
				menuRect.height = Mathf.Clamp(resizeStartSize.y + val6.y, 300f, 1000f);
				Event.current.Use();
			}
			else if ((int)Event.current.type == 1 || (int)Event.current.rawType == 1)
			{
				isResizing = false;
				Event.current.Use();
			}
		}
	}

	private void TrySpawnEnemy()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)

		// ── 主机权限检查 ──────────────────────────────────
		if (SemiFunc.IsMultiplayer() && !PhotonNetwork.IsMasterClient)
		{
			// 先尝试本地伪装为主机，使 PhotonNetwork.Instantiate 等调用通过
			bool faked = ForceHost.Method_LocalMasterFake();
			if (!faked || !PhotonNetwork.IsMasterClient)
			{
				Debug.LogWarning("[SpawnEnemy] 非主机，尝试自动夺取主机权限…");
				ForceHost.Instance.StartCoroutine(ForceHost.Instance.Method_AutoAll());
				ForceHost.statusMessage = "[SpawnEnemy] 正在尝试获取主机权限，请稍后重试";
				return;
			}
		}

		LevelGenerator val = Object.FindObjectOfType<LevelGenerator>();
		if ((Object)(object)val == (Object)null)
		{
			Debug.Log((object)"LevelGenerator instance not found!");
			return;
		}
		GameObject localPlayer = DebugCheats.GetLocalPlayer();
		if ((Object)(object)localPlayer == (Object)null)
		{
			Debug.Log((object)"Local player not found!");
			return;
		}
		Vector3 position = localPlayer.transform.position + Vector3.up * 1.5f;
		spawnCountText = Regex.Replace(spawnCountText, "[^0-9]", "");
		if (spawnCountText.Length > 2)
		{
			spawnCountText = spawnCountText.Substring(0, 2);
		}
		int result = 1;
		if (!int.TryParse(spawnCountText, out result))
		{
			result = 1;
		}
		result = Mathf.Clamp(result, 1, 10);
		if (spawnEnemyIndex >= 0 && spawnEnemyIndex < cachedFilteredEnemySetups.Count)
		{
			var esType = typeof(RunManager).Assembly.GetType("EnemySpawner");
			bool spawned = false;
			for (int i = 0; i < result; i++)
			{
				if (esType != null)
				{
					// 尝试多种方法签名（游戏更新可能改变参数）
					bool ok = false;
					// 签名1: (LevelGenerator, EnemySetup, Vector3)
					var m3 = esType.GetMethod("SpawnSpecificEnemy", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
						null, new Type[] { typeof(LevelGenerator), typeof(EnemySetup), typeof(Vector3) }, null);
					if (m3 != null)
					{
						try { m3.Invoke(null, new object[] { val, cachedFilteredEnemySetups[spawnEnemyIndex], position }); ok = true; }
						catch (Exception ex) { Debug.LogWarning("[SpawnEnemy] 3-param failed: " + ex.InnerException?.Message); }
					}
					if (!ok)
					{
						// 签名2: (LevelGenerator, EnemySetup, Vector3, bool) — 新版可能有forced参数
						var m4 = esType.GetMethod("SpawnSpecificEnemy", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
							null, new Type[] { typeof(LevelGenerator), typeof(EnemySetup), typeof(Vector3), typeof(bool) }, null);
						if (m4 != null)
						{
							try { m4.Invoke(null, new object[] { val, cachedFilteredEnemySetups[spawnEnemyIndex], position, true }); ok = true; }
							catch (Exception ex) { Debug.LogWarning("[SpawnEnemy] 4-param failed: " + ex.InnerException?.Message); }
						}
					}
					if (!ok)
					{
						// 签名3: 尝试任何名为SpawnSpecificEnemy的静态方法
						var mAny = esType.GetMethod("SpawnSpecificEnemy", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
						if (mAny != null)
						{
							try
							{
								var pars = mAny.GetParameters();
								object[] args;
								if (pars.Length == 3) args = new object[] { val, cachedFilteredEnemySetups[spawnEnemyIndex], position };
								else if (pars.Length == 4) args = new object[] { val, cachedFilteredEnemySetups[spawnEnemyIndex], position, true };
								else args = new object[] { val, cachedFilteredEnemySetups[spawnEnemyIndex], position };
								mAny.Invoke(null, args); ok = true;
							}
							catch (Exception ex) { Debug.LogWarning("[SpawnEnemy] Generic invoke failed: " + ex.InnerException?.Message); }
						}
						else
						{
							Debug.LogError("[SpawnEnemy] 未找到 EnemySpawner.SpawnSpecificEnemy 方法");
						}
					}
					if (ok) spawned = true;
				}
				else
				{
					Debug.LogError("[SpawnEnemy] 未找到 EnemySpawner 类型");
				}
			}
			if (spawned)
				Debug.Log((object)$"Spawned {result}x {cachedEnemySetupNames[spawnEnemyIndex]}");
		}
		else
		{
			Debug.Log((object)"Invalid enemy selection.");
		}
	}

	private void DrawTeleportOptions()
	{
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		int num = 25;
		int num2 = 6;
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("common.tp_options"));
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button((teleportPlayerSourceIndex >= 0 && teleportPlayerSourceIndex < teleportPlayerSourceOptions.Length) ? teleportPlayerSourceOptions[teleportPlayerSourceIndex] : L.T("enemies.no_player"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			showSourceDropdown = !showSourceDropdown;
		}
		GUILayout.Label(L.T("common.to"), labelStyle, Array.Empty<GUILayoutOption>());
		if (GUILayout.Button((teleportPlayerDestIndex >= 0 && teleportPlayerDestIndex < teleportPlayerDestOptions.Length) ? teleportPlayerDestOptions[teleportPlayerDestIndex] : L.T("enemies.no_player"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			showDestDropdown = !showDestDropdown;
		}
		GUILayout.EndHorizontal();
		if (showSourceDropdown)
		{
			float num3 = Mathf.Min(teleportPlayerSourceOptions.Length, num2) * num;
			sourceDropdownScrollPosition = GUILayout.BeginScrollView(sourceDropdownScrollPosition, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(num3) });
			for (int i = 0; i < teleportPlayerSourceOptions.Length; i++)
			{
				if (i != teleportPlayerSourceIndex && GUILayout.Button(teleportPlayerSourceOptions[i], buttonStyle, Array.Empty<GUILayoutOption>()))
				{
					teleportPlayerSourceIndex = i;
					showSourceDropdown = false;
				}
			}
			GUILayout.EndScrollView();
		}
		if (showDestDropdown)
		{
			float num4 = Mathf.Min(teleportPlayerDestOptions.Length, num2) * num;
			destDropdownScrollPosition = GUILayout.BeginScrollView(destDropdownScrollPosition, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(num4) });
			for (int j = 0; j < teleportPlayerDestOptions.Length; j++)
			{
				if (j != teleportPlayerDestIndex && GUILayout.Button(teleportPlayerDestOptions[j], buttonStyle, Array.Empty<GUILayoutOption>()))
				{
					teleportPlayerDestIndex = j;
					showDestDropdown = false;
				}
			}
			GUILayout.EndScrollView();
		}
		GUILayout.Space(10f);
		if (GUILayout.Button(L.T("common.execute_tp"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			Teleport.ExecuteTeleportWithSeparateOptions(teleportPlayerSourceIndex, teleportPlayerDestIndex, teleportPlayerSourceOptions, teleportPlayerDestOptions, playerList);
			Debug.Log((object)"Teleport executed successfully");
			showSourceDropdown = false;
			showDestDropdown = false;
		}
	}

	private void EnsureListStylesInitialized()
	{
		// Also check if textures were destroyed by Unity
		if (listStylesInitialized && (Object)(object)rowBgNormal != (Object)null)
			return;
		listStylesInitialized = false;
		if ((Object)(object)rowBgNormal == (Object)null)
		{
			rowBgNormal = MakeSolidBackground((Color)(new Color32((byte)30, (byte)30, (byte)30, byte.MaxValue)));
			rowBgHover = MakeSolidBackground((Color)(new Color32((byte)40, (byte)40, (byte)50, byte.MaxValue)));
			rowBgSelected = MakeSolidBackground((Color)(new Color32((byte)50, (byte)50, (byte)70, byte.MaxValue)));
		}
		enemyBgSelected = MakeSolidBackground((Color)(new Color32((byte)60, (byte)30, (byte)10, (byte)200)));
		enemyBgNormal = MakeSolidBackground((Color)(new Color32((byte)30, (byte)30, (byte)35, (byte)180)));
		itemBgSelected = MakeSolidBackground((Color)(new Color32((byte)45, (byte)25, (byte)5, (byte)220)));
		itemBgNormal = MakeSolidBackground((Color)(new Color32((byte)28, (byte)28, (byte)30, (byte)180)));
		cachedPlayerListStyle = new GUIStyle(GUI.skin.button)
		{
			alignment = (TextAnchor)4,
			fontSize = 14,
			fontStyle = (FontStyle)1,
			fixedHeight = 28f,
			margin = new RectOffset(2, 2, 2, 2),
			padding = new RectOffset(8, 8, 4, 4),
			border = new RectOffset(4, 4, 4, 4)
		};
		cachedEnemyListStyle = new GUIStyle(GUI.skin.button)
		{
			fontSize = 13,
			fontStyle = (FontStyle)1,
			alignment = (TextAnchor)4,
			margin = new RectOffset(4, 4, 2, 2),
			padding = new RectOffset(6, 6, 4, 4),
			border = new RectOffset(4, 4, 4, 4)
		};
		cachedItemListStyle = new GUIStyle(GUI.skin.button)
		{
			fontSize = 13,
			fontStyle = (FontStyle)1,
			alignment = (TextAnchor)4,
			margin = new RectOffset(4, 4, 2, 2),
			padding = new RectOffset(6, 6, 4, 4),
			border = new RectOffset(4, 4, 4, 4)
		};
		cachedServerRowStyle = new GUIStyle(GUI.skin.button)
		{
			alignment = (TextAnchor)3,
			fontSize = 14,
			fontStyle = (FontStyle)1,
			fixedHeight = 32f,
			margin = new RectOffset(2, 2, 2, 2),
			padding = new RectOffset(8, 8, 4, 4),
			border = new RectOffset(4, 4, 4, 4)
		};
		listStylesInitialized = true;
	}

	private Texture2D MakeSolidBackground(Color color)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(4, 4);
		Color[] array = (Color[])(object)new Color[16];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = color;
		}
		val.SetPixels(array);
		val.Apply();
		((Object)val).hideFlags = (HideFlags)61;
		return val;
	}

	public static Texture2D colorpicker(Color color, float alpha = 1f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		Texture2D val = new Texture2D(1, 1);
		val.SetPixel(0, 0, new Color(color.r, color.g, color.b, alpha));
		val.Apply();
		return val;
	}

	private Color GetContrastColor(Color backgroundColor)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (!(0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b < 0.5f))
		{
			return Color.black;
		}
		return Color.white;
	}

	// =====================================================
	// 菜单设置 Tab
	// =====================================================
	private void ApplyTheme(ThemeType theme)
	{
		currentTheme = theme;
		activeTheme = ThemeData.GetTheme(theme);
		titleStyle = null;
		listStylesInitialized = false;
		tabHighlightTex = null;
		sectionAccentTex = null;
		sectionLineTex = null;
		_uiStylesInited = false; // Force UIStyles to re-init with new theme color
		configstatus = L.T("menupage.theme_applied");
	}

	private void DrawMenuSettingsTab()
	{
		DrawSectionHeader(L.T("menupage.title"));
		GUILayout.Space(10f);

		// 主题切换
		DrawSectionHeader(L.T("menupage.theme"));
		GUILayout.Label(L.T("menupage.theme_current") + " " + L.T("theme." + currentTheme.ToString().ToLower()), labelStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Space(5f);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		foreach (ThemeType t in System.Enum.GetValues(typeof(ThemeType)))
		{
			GUIStyle style = (t == currentTheme) ? tabSelectedStyle : buttonStyle;
			if (GUILayout.Button(L.T("theme." + t.ToString().ToLower()), style, Array.Empty<GUILayoutOption>()))
			{
				ApplyTheme(t);
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(15f);

		// 背景颜色设置
		DrawSectionHeader(L.T("menupage.bg_color"));
		GUILayout.Space(5f);
		GUILayout.Label(L.T("menupage.bg_r") + " " + Mathf.RoundToInt(menuBgR), labelStyle, Array.Empty<GUILayoutOption>());
		menuBgR = GUILayout.HorizontalSlider(menuBgR, 0f, 255f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		GUILayout.Label(L.T("menupage.bg_g") + " " + Mathf.RoundToInt(menuBgG), labelStyle, Array.Empty<GUILayoutOption>());
		menuBgG = GUILayout.HorizontalSlider(menuBgG, 0f, 255f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		GUILayout.Label(L.T("menupage.bg_b") + " " + Mathf.RoundToInt(menuBgB), labelStyle, Array.Empty<GUILayoutOption>());
		menuBgB = GUILayout.HorizontalSlider(menuBgB, 0f, 255f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		GUILayout.Label(L.T("menupage.bg_a") + " " + Mathf.RoundToInt(menuBgA), labelStyle, Array.Empty<GUILayoutOption>());
		menuBgA = GUILayout.HorizontalSlider(menuBgA, 0f, 255f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		GUILayout.Space(5f);
		// 颜色预览
		Color previewColor = new Color(menuBgR / 255f, menuBgG / 255f, menuBgB / 255f, menuBgA / 255f);
		Color oldGUIColor = GUI.color;
		GUI.color = previewColor;
		GUILayout.Box("", (GUILayoutOption[])(object)new GUILayoutOption[2] { GUILayout.Width(200f), GUILayout.Height(30f) });
		GUI.color = oldGUIColor;
		GUILayout.Space(5f);
		if (GUILayout.Button(L.T("menupage.apply_color"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			Texture2D newBg = MakeSolidBackground(new Color(menuBgR / 255f, menuBgG / 255f, menuBgB / 255f, menuBgA / 255f));
			backgroundStyle.normal.background = newBg;
			backgroundStyle.onNormal.background = newBg;
			backgroundStyle.focused.background = newBg;
			backgroundStyle.onFocused.background = newBg;
			customBgTexture = null;
		}
		GUILayout.Space(15f);

		// 自定义背景图片
		DrawSectionHeader(L.T("menupage.bg_image"));
		GUILayout.Space(5f);
		GUILayout.Label(L.T("menupage.image_path"), labelStyle, Array.Empty<GUILayoutOption>());
		customBgImagePath = GUILayout.TextField(customBgImagePath, textFieldStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Space(5f);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button(L.T("menupage.load_image"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			try
			{
				if (System.IO.File.Exists(customBgImagePath))
				{
					byte[] imgData = System.IO.File.ReadAllBytes(customBgImagePath);
					Texture2D tex = new Texture2D(2, 2, (TextureFormat)5, false);
					if (ImageConversion.LoadImage(tex, imgData))
					{
						customBgTexture = tex;
						backgroundStyle.normal.background = tex;
						backgroundStyle.onNormal.background = tex;
						backgroundStyle.focused.background = tex;
						backgroundStyle.onFocused.background = tex;
						configstatus = L.T("menupage.image_loaded");
					}
					else
					{
						configstatus = L.T("menupage.image_failed");
					}
				}
				else
				{
					configstatus = L.T("menupage.image_failed");
				}
			}
			catch
			{
				configstatus = L.T("menupage.image_failed");
			}
		}
		if (GUILayout.Button(L.T("menupage.clear_image"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			customBgTexture = null;
			Texture2D newBg = MakeSolidBackground(new Color(menuBgR / 255f, menuBgG / 255f, menuBgB / 255f, menuBgA / 255f));
			backgroundStyle.normal.background = newBg;
			backgroundStyle.onNormal.background = newBg;
			backgroundStyle.focused.background = newBg;
			backgroundStyle.onFocused.background = newBg;
			configstatus = L.T("menupage.image_cleared");
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(20f);

		// 语言切换
		DrawSectionHeader(L.T("menupage.language"));
		GUILayout.Label(L.T("menupage.lang_current") + " " + LanguageManager.GetLanguageDisplayName(LanguageManager.currentLanguage), labelStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Space(5f);
		string[] availLangs = LanguageManager.GetAvailableLanguages();
		if (GUILayout.Button(showLangDropdown ? "▲ " + LanguageManager.GetLanguageDisplayName(LanguageManager.currentLanguage) : "▼ " + LanguageManager.GetLanguageDisplayName(LanguageManager.currentLanguage), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			showLangDropdown = !showLangDropdown;
		}
		if (showLangDropdown)
		{
			GUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
			langDropdownScroll = GUILayout.BeginScrollView(langDropdownScroll, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(Mathf.Min(150f, availLangs.Length * 30f)) });
			foreach (string lang in availLangs)
			{
				string displayName = LanguageManager.GetLanguageDisplayName(lang);
				GUIStyle style = (lang == LanguageManager.currentLanguage) ? tabSelectedStyle : buttonStyle;
				if (GUILayout.Button(displayName + " (" + lang + ")", style, Array.Empty<GUILayoutOption>()))
				{
					LanguageManager.SetLanguage(lang);
					tabs = LanguageManager.GetTabNames();
					colorNameMapping = null;
					configstatus = L.T("menupage.lang_switched");
					showLangDropdown = false;
				}
			}
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
		}
		GUILayout.Space(20f);

		// 菜单控制
		DrawSectionHeader(L.T("menupage.controls"));
		GUILayout.Space(5f);
		if (GUILayout.Button(L.T("menupage.reload"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			titleStyle = null;
			listStylesInitialized = false;
			configstatus = L.T("config.loaded");
		}
		GUILayout.Space(5f);
		// 卸载菜单（二次确认）
		if (confirmUnload && Time.time - confirmTimer < 3f)
		{
			if (GUILayout.Button(L.T("menupage.confirm_unload"), warningStyle, Array.Empty<GUILayoutOption>()))
			{
				showMenu = false;
				Object.Destroy(((Component)this).gameObject);
			}
		}
		else
		{
			confirmUnload = false;
			if (GUILayout.Button(L.T("menupage.unload"), buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				confirmUnload = true;
				confirmTimer = Time.time;
			}
		}
		GUILayout.Space(5f);
		if (GUILayout.Button(L.T("menupage.main_menu"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			showMenu = false;
			UnityEngine.SceneManagement.SceneManager.LoadScene(0);
		}
		GUILayout.Space(5f);
		// 退出游戏（二次确认）
		if (confirmQuit && Time.time - confirmTimer < 3f)
		{
			if (GUILayout.Button(L.T("menupage.confirm_quit"), warningStyle, Array.Empty<GUILayoutOption>()))
			{
				Application.Quit();
			}
		}
		else
		{
			confirmQuit = false;
			if (GUILayout.Button(L.T("menupage.quit_game"), buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				confirmQuit = true;
				confirmTimer = Time.time;
			}
		}
	}

	// =====================================================
	// 传送 Tab
	// =====================================================
	private void DrawTeleportTab()
	{
		DrawSectionHeader(L.T("tp.quick"));
		GUILayout.Space(5f);

		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button(L.T("tp.crosshair"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			TeleportPlus.TeleportToCrosshair();
		}
		if (GUILayout.Button(L.T("tp.extraction"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			TeleportPlus.TeleportToExtraction();
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(5f);

		if (GUILayout.Button(L.T("tp.random"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			TeleportPlus.TeleportRandom(50f);
		}

		GUILayout.Space(15f);
		DrawSectionHeader(L.T("tp.waypoints"));
		GUILayout.Space(5f);

		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(L.T("tp.name"), labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(55f) });
		TeleportPlus.newWaypointName = GUILayout.TextField(TeleportPlus.newWaypointName, textFieldStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(160f) });
		if (GUILayout.Button(L.T("tp.save"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			TeleportPlus.SaveCurrentPosition(TeleportPlus.newWaypointName);
			TeleportPlus.newWaypointName = "";
		}
		GUILayout.EndHorizontal();

		GUILayout.Space(5f);
		if (TeleportPlus.savedWaypoints.Count == 0)
		{
			GUILayout.Label(L.T("tp.no_waypoints"), labelStyle, Array.Empty<GUILayoutOption>());
		}
		else
		{
			waypointScrollPos = GUILayout.BeginScrollView(waypointScrollPos, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height((float)Mathf.Min(200, TeleportPlus.savedWaypoints.Count * 30)) });
			for (int wp = 0; wp < TeleportPlus.savedWaypoints.Count; wp++)
			{
				var waypoint = TeleportPlus.savedWaypoints[wp];
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());

				GUIStyle wpStyle = (wp == TeleportPlus.selectedWaypointIndex) ? tabSelectedStyle : buttonStyle;
				if (GUILayout.Button(waypoint.Name, wpStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(150f) }))
				{
					TeleportPlus.selectedWaypointIndex = wp;
				}
				GUILayout.Label($"({waypoint.Position.x:F0}, {waypoint.Position.y:F0}, {waypoint.Position.z:F0})", labelStyle, Array.Empty<GUILayoutOption>());
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();

			GUILayout.Space(5f);
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (GUILayout.Button(L.T("tp.goto"), buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				TeleportPlus.TeleportToWaypoint(TeleportPlus.selectedWaypointIndex);
			}
			if (GUILayout.Button(L.T("tp.delete"), buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				TeleportPlus.RemoveWaypoint(TeleportPlus.selectedWaypointIndex);
			}
			GUILayout.EndHorizontal();
		}

		// ===================== 全队传送到撤离点 (从杂项迁移) =====================
		GUILayout.Space(15f);
		DrawSectionHeader(L.T("misc.team_tools"));
		if (GUILayout.Button(L.T("misc.tp_all_extraction"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			TeamTeleport.TeleportAllToExtraction();
		}
	}

	private void DrawAdminTab()
	{
		DrawSectionHeader(L.T("admin.title"));

		// ===================== 目标选择 =====================
		GUILayout.Space(5f);
		GUILayout.Label(L.T("admin.select_target"), labelStyle, Array.Empty<GUILayoutOption>());

		// 更新目标列表
		if (adminTargetOptions.Length != playerNames.Count + 1)
		{
			var opts = new List<string> { L.T("common.all_players") };
			opts.AddRange(playerNames);
			adminTargetOptions = opts.ToArray();
			if (adminTargetIndex >= adminTargetOptions.Length) adminTargetIndex = 0;
		}

		string currentTarget = (adminTargetIndex >= 0 && adminTargetIndex < adminTargetOptions.Length)
			? adminTargetOptions[adminTargetIndex] : "---";
		if (GUILayout.Button(currentTarget, buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			showAdminTargetDropdown = !showAdminTargetDropdown;
		}
		if (showAdminTargetDropdown)
		{
			adminTargetScroll = GUILayout.BeginScrollView(adminTargetScroll, new GUILayoutOption[] { GUILayout.Height(120f) });
			for (int i = 0; i < adminTargetOptions.Length; i++)
			{
				if (i != adminTargetIndex && GUILayout.Button(adminTargetOptions[i], Array.Empty<GUILayoutOption>()))
				{
					adminTargetIndex = i;
					showAdminTargetDropdown = false;
				}
			}
			GUILayout.EndScrollView();
		}

		// ===================== 升级滑块 =====================
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("admin.upgrades"));

		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(L.T("admin.grab_str", ((int)adminGrabStrength).ToString()), labelStyle, new GUILayoutOption[] { GUILayout.Width(250f) });
		adminGrabStrength = GUILayout.HorizontalSlider(adminGrabStrength, 0f, 30f, new GUILayoutOption[] { GUILayout.Width(200f) });
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(L.T("admin.throw_str", ((int)adminThrowStrength).ToString()), labelStyle, new GUILayoutOption[] { GUILayout.Width(250f) });
		adminThrowStrength = GUILayout.HorizontalSlider(adminThrowStrength, 0f, 30f, new GUILayoutOption[] { GUILayout.Width(200f) });
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(L.T("admin.sprint_spd", ((int)adminSprintSpeed).ToString()), labelStyle, new GUILayoutOption[] { GUILayout.Width(250f) });
		adminSprintSpeed = GUILayout.HorizontalSlider(adminSprintSpeed, 0f, 30f, new GUILayoutOption[] { GUILayout.Width(200f) });
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(L.T("admin.grab_range", ((int)adminGrabRange).ToString()), labelStyle, new GUILayoutOption[] { GUILayout.Width(250f) });
		adminGrabRange = GUILayout.HorizontalSlider(adminGrabRange, 0f, 30f, new GUILayoutOption[] { GUILayout.Width(200f) });
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(L.T("admin.extra_jump", ((int)adminExtraJump).ToString()), labelStyle, new GUILayoutOption[] { GUILayout.Width(250f) });
		adminExtraJump = GUILayout.HorizontalSlider(adminExtraJump, 0f, 30f, new GUILayoutOption[] { GUILayout.Width(200f) });
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(L.T("admin.tumble_launch", ((int)adminTumbleLaunch).ToString()), labelStyle, new GUILayoutOption[] { GUILayout.Width(250f) });
		adminTumbleLaunch = GUILayout.HorizontalSlider(adminTumbleLaunch, 0f, 20f, new GUILayoutOption[] { GUILayout.Width(200f) });
		GUILayout.EndHorizontal();

		GUILayout.Space(5f);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button(L.T("admin.apply"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ApplyAdminUpgrades();
		}
		if (GUILayout.Button(L.T("admin.max_all"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			adminGrabStrength = 30f; adminThrowStrength = 30f; adminSprintSpeed = 30f;
			adminGrabRange = 30f; adminExtraJump = 30f; adminTumbleLaunch = 20f;
			ApplyAdminUpgrades();
		}
		GUILayout.EndHorizontal();

		// ===================== 无敌光环 =====================
		GUILayout.Space(10f);
		DrawSectionHeader(L.T("admin.aura_section"));
		ToggleLogic("invincibility_aura", L.T("admin.aura_toggle"), ref adminInvincibilityAura, () => {
			PlayerAura.isEnabled = adminInvincibilityAura;
			if (adminInvincibilityAura)
			{
				// 设置光环目标：0=全部, 其他=对应玩家
				if (adminTargetIndex == 0)
					PlayerAura.targetActorNumber = -1;
				else if (adminTargetIndex - 1 < playerList.Count)
				{
					var pv = ((Component)playerList[adminTargetIndex - 1]).GetComponent<PhotonView>();
					PlayerAura.targetActorNumber = (pv != null) ? pv.OwnerActorNr : -1;
				}
			}
		});
		if (adminInvincibilityAura)
		{
			string auraTargetText = (PlayerAura.targetActorNumber == -1) ? L.T("common.all_players") : L.T("admin.aura_actor", PlayerAura.targetActorNumber.ToString());
			GUILayout.Label(L.T("admin.aura_status", auraTargetText), labelStyle, Array.Empty<GUILayoutOption>());
		}
	}

	private void ApplyAdminUpgrades()
	{
		try
		{
			if (adminTargetIndex == 0)
			{
				// 为所有玩家应用
				foreach (var player in playerList)
				{
					string sid = PlayerController.GetSteamIDFromAvatar(player);
					if (!string.IsNullOrEmpty(sid))
						UpgradeForPlayer.ApplyUpgrades(sid, (int)adminGrabStrength, (int)adminThrowStrength, (int)adminSprintSpeed, (int)adminGrabRange, (int)adminExtraJump, (int)adminTumbleLaunch);
				}
			}
			else
			{
				int idx = adminTargetIndex - 1;
				if (idx >= 0 && idx < playerList.Count)
				{
					string sid = PlayerController.GetSteamIDFromAvatar(playerList[idx]);
					if (!string.IsNullOrEmpty(sid))
						UpgradeForPlayer.ApplyUpgrades(sid, (int)adminGrabStrength, (int)adminThrowStrength, (int)adminSprintSpeed, (int)adminGrabRange, (int)adminExtraJump, (int)adminTumbleLaunch);
				}
			}
		}
		catch (Exception ex) { Debug.LogWarning("[Admin] Upgrade error: " + ex.Message); }
	}
}
