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

	public string spawnCountText = "1";

	public int spawnEnemyIndex;

	public bool showSpawnDropdown;

	public Vector2 spawnDropdownScrollPosition = Vector2.zero;

	public static bool ChatDropdownVisible = false;

	public static string ChatDropdownVisibleName = "全部";

	public static float fieldOfView = 70f;

	public static float oldFieldOfView = 70f;

	private float nextUpdateTime;

	private const float updateInterval = 10f;

	private string[] availableLevels = new string[6] { "Level - Wizard", "Level - Shop", "Level - Manor", "Level - Arctic", "Level - Lobby", "Level - Recording" };

	private Vector2 chatDropdownScroll = Vector2.zero;

	private bool showChatDropdown;

	private int ChatSelectedPlayerIndex;

	private string chatMessageText = "D4RK CHEATS :3";

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

	public static bool showMenu = true;

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

	private string spoofTargetVisibleName = "全部";

	private bool spoofDropdownVisible;

	public static string spoofedNameText = "Text";

	public static string persistentNameText = "Text";

	private string originalSteamName = SteamClient.Name;

	public static bool spoofNameActive = false;

	private float lastSpoofTime;

	private const float NAME_SPOOF_DELAY = 4f;

	public static bool hasAlreadySpoofed = false;

	private string colorTargetVisibleName = "全部";

	private bool colorDropdownVisible;

	private string colorIndexText = "1";

	private bool showColorIndexDropdown;

	private Vector2 colorIndexScrollPosition = Vector2.zero;

	private System.Random rainbowRandom = new System.Random();

	private Dictionary<int, bool> playerRainbowStates = new Dictionary<int, bool>();

	private Dictionary<int, float> lastRainbowTimes = new Dictionary<int, float>();

	private const float PLAYER_RAINBOW_DELAY = 0.1f;

	private Dictionary<int, string> colorNameMapping = new Dictionary<int, string>
	{
		{ 0, "白色" },
		{ 1, "灰色" },
		{ 2, "黑色" },
		{ 3, "浅红" },
		{ 4, "红色" },
		{ 5, "深红1" },
		{ 6, "深红2" },
		{ 7, "亮粉1" },
		{ 8, "亮粉2" },
		{ 9, "亮紫" },
		{ 10, "浅紫1" },
		{ 11, "浅紫2" },
		{ 12, "紫色" },
		{ 13, "深紫1" },
		{ 14, "深紫2" },
		{ 15, "深蓝" },
		{ 16, "蓝色" },
		{ 17, "浅蓝1" },
		{ 18, "浅蓝2" },
		{ 19, "青色" },
		{ 20, "浅绿1" },
		{ 21, "浅绿2" },
		{ 22, "浅绿3" },
		{ 23, "绿色" },
		{ 24, "绿色2" },
		{ 25, "深绿1" },
		{ 26, "深绿2" },
		{ 27, "深绿3" },
		{ 28, "浅黄" },
		{ 29, "黄色" },
		{ 30, "深黄" },
		{ 31, "橙色" },
		{ 32, "深橙" },
		{ 33, "棕色" },
		{ 34, "橄榄" },
		{ 35, "肤色" }
	};

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

	private string[] tabs = new string[12] { "自身", "视觉", "战斗", "杂项", "敌人", "物品", "热键", "恶搞", "配置", "服务器", "创造", "传送" };

	private Vector2 waypointScrollPos = Vector2.zero;
	private int espPresetIndex = 0;

	private int currentTab;

	public static Texture2D toggleBackground;

	public string configstatus = "等待操作...";

	private float toggleAnimationSpeed = 8f;

	private Dictionary<string, float> toggleAnimations = new Dictionary<string, float>();

	private bool isResizing;

	private Vector2 resizeStartMousePos;

	private Vector2 resizeStartSize;

	private GUIStyle titleStyle;

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

	private static Vector2 serverListScroll;

	public static LobbyCoroutineHost CoroutineHost;

	private Rect previousWindowRect;

	private bool hasStoredPreviousSize;

	private bool wasInServerTab;

	private static Texture2D rowBgNormal;

	private static Texture2D rowBgHover;

	private static Texture2D rowBgSelected;

	public static bool grabThroughWallsEnabled = false;

	private bool showTextEditorPopup;

	private string largeTextBoxContent = "";

	private Rect editorPopupRect = new Rect(200f, 200f, 400f, 300f);

	private string activeTextFieldId;

	private static Vector2 textboxscroll;

	private void CheckIfHost()
	{
		isHost = !SemiFunc.IsMultiplayer() || PhotonNetwork.IsMasterClient;
	}

	private void UpdateTeleportOptions()
	{
		List<string> list = new List<string>();
		list.Add("所有玩家");
		list.AddRange(playerNames);
		teleportPlayerSourceOptions = list.ToArray();
		List<string> list2 = new List<string>();
		list2.AddRange(playerNames);
		list2.Add("虚空");
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
		toggleBgTexture = TextureLoader.LoadEmbeddedTexture("r.e.p.o_cheat.images.toggle_bg.png");
		toggleKnobOffTexture = TextureLoader.LoadEmbeddedTexture("r.e.p.o_cheat.images.toggle_knobOff.png");
		toggleKnobOnTexture = TextureLoader.LoadEmbeddedTexture("r.e.p.o_cheat.images.toggle_knobOn.png");
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
			showMenu = !showMenu;
			CursorController.cheatMenuOpen = showMenu;
			CursorController.UpdateCursorState();
			if (!showMenu)
			{
				TryUnlockCamera();
			}
			UpdateCursorState();
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
		if ((Object)(object)RunManager.instance.levelCurrent != (Object)null && levelsToSearchItems.Contains(((Object)RunManager.instance.levelCurrent).name))
		{
			if (Time.time >= nextUpdateTime)
			{
				UpdateEnemyList();
				UpdateItemList();
				itemList = ItemTeleport.GetItemList();
				nextUpdateTime = Time.time + 10f;
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
				string text = "敌人";
				Component componentInParent = ((Component)enemy).GetComponentInParent(Type.GetType("EnemyParent, Assembly-CSharp"));
				if ((Object)(object)componentInParent != (Object)null)
				{
					text = (((object)componentInParent).GetType().GetField("enemyName", BindingFlags.Instance | BindingFlags.Public)?.GetValue(componentInParent) as string) ?? "敌人";
				}
				int enemyHealth = Enemies.GetEnemyHealth(enemy);
				DebugCheats.enemyHealthCache[enemy] = enemyHealth;
				int enemyMaxHealth = Enemies.GetEnemyMaxHealth(enemy);
				float num = ((enemyMaxHealth > 0) ? ((float)enemyHealth / (float)enemyMaxHealth) : 0f);
				string arg = ((num > 0.66f) ? "<color=green>" : ((num > 0.33f) ? "<color=yellow>" : "<color=red>"));
				string text2 = ((enemyHealth >= 0) ? $"{arg}HP: {enemyHealth}/{enemyMaxHealth}</color>" : "<color=gray>HP: 未知</color>");
				enemyNames.Add(text + " [" + text2 + "]");
			}
		}
		if (enemyNames.Count == 0)
		{
			enemyNames.Add("未发现敌人");
		}
	}

	private void UpdatePlayerList()
	{
		List<string> list = playerNames.Where((string name) => name.Contains("FakePlayer")).ToList();
		int count = list.Count;
		playerNames.Clear();
		playerList.Clear();
		foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
		{
			playerList.Add(item);
			string text = SemiFunc.PlayerGetName(item) ?? "未知玩家";
			string text2 = (IsPlayerAlive(item, text) ? "<color=green>[存活]</color> " : "<color=red>[死亡]</color> ");
			playerNames.Add(text2 + text);
		}
		for (int num = 0; num < count; num++)
		{
			playerNames.Add(list[num]);
			playerList.Add(null);
		}
		if (playerNames.Count == 0)
		{
			playerNames.Add("未发现玩家");
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
		InitStyles();
		if (showMenu)
		{
			menuRect = GUI.Window(0, menuRect, new WindowFunction(DrawMenuWindow), "", backgroundStyle);
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
		}
		if (useModernESP)
		{
			if (DebugCheats.drawEspBool || DebugCheats.drawItemEspBool || DebugCheats.drawExtractionPointEspBool || DebugCheats.drawPlayerEspBool)
			{
				ModernESP.Render();
			}
		}
		else if (DebugCheats.drawEspBool || DebugCheats.drawItemEspBool || DebugCheats.drawExtractionPointEspBool || DebugCheats.drawPlayerEspBool)
		{
			DebugCheats.DrawESP();
			ModernESP.ClearItemLabels();
			ModernESP.ClearEnemyLabels();
		}
		// ESP增强：追踪线
		ESPEnhancements.DrawTraceLines();
		// 小地图雷达
		MiniRadar.DrawRadar();
		GUIStyle val = new GUIStyle(GUI.skin.label)
		{
			wordWrap = false
		};
		if (showWatermark)
		{
			GUIContent val2 = new GUIContent($"DARK CHEAT | {hotkeyManager.MenuToggleKey} - MENU");
			Vector2 val3 = val.CalcSize(val2);
			GUI.Label(new Rect(10f, 10f, val3.x, val3.y), val2, val);
			GUI.Label(new Rect(10f + val3.x + 10f, 10f, 200f, val3.y), "@Github/D4rkks (+collabs)", val);
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
				string text = SemiFunc.PlayerGetName(item) ?? "未知";
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
			GUI.Label(new Rect(num, num2 + (float)num4++ * num3, 400f, num3), "存活玩家:", val4);
			foreach (string item2 in list2)
			{
				string text2 = "- " + item2;
				GUI.Label(new Rect(num, num2 + (float)num4++ * num3, 400f, num3), text2);
			}
			num4++;
			GUI.color = Color.red;
			GUI.Label(new Rect(num, num2 + (float)num4++ * num3, 400f, num3), "死亡玩家:", val4);
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
			GUI.Label(new Rect(num, num2 + (float)num4++ * num3, 400f, num3), $"地图总价值 ${totalValuableValue}", val4);
			GUI.color = Color.white;
			num4++;
		}
	}

	private bool DrawCustomToggle(string id, bool state)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Invalid comparison between Unknown and I4
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		float num = 46f;
		float num2 = 22f;
		float num3 = 18f;
		float num4 = (num2 - num3) / 2f;
		GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[2]
		{
			GUILayout.Width(num),
			GUILayout.Height(num2)
		});
		Rect rect = GUILayoutUtility.GetRect(num, num2);
		GUILayout.EndVertical();
		if (!toggleAnimations.TryGetValue(id, out var value))
		{
			value = (state ? 1f : 0f);
			toggleAnimations[id] = value;
		}
		float num5 = (state ? 1f : 0f);
		if ((int)Event.current.type == 7 && !Mathf.Approximately(value, num5))
		{
			value = Mathf.MoveTowards(value, num5, Time.deltaTime * toggleAnimationSpeed);
			toggleAnimations[id] = value;
		}
		GUI.color = Color.white;
		GUI.DrawTexture(new Rect(rect.x, rect.y, num, num2), (Texture)(object)toggleBgTexture);
		float num6 = num - num3 - num4 * 2f;
		float num7 = rect.x + num4 + num6 * value;
		GUI.DrawTexture(new Rect(num7, rect.y + num4, num3, num3), (Texture)(object)(state ? toggleKnobOnTexture : toggleKnobOffTexture));
		if ((int)Event.current.type == 0 && rect.Contains(Event.current.mousePosition))
		{
			Event.current.Use();
			return !state;
		}
		return state;
	}

	private bool ToggleLogic(string id, string label, ref bool value, Action onToggle = null)
	{
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(label, labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		bool flag = DrawCustomToggle(id, value);
		if (flag != value)
		{
			value = flag;
			onToggle?.Invoke();
		}
		GUILayout.EndHorizontal();
		return value;
	}

	private void DrawMenuWindow(int windowID)
	{
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
		GUI.DragWindow(new Rect(0f, 0f, menuRect.width, 25f));
		float num = 120f;
		float num2 = 20f;
		float num3 = Mathf.Max(200f, menuRect.width - num - num2 * 2f);
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		GUILayout.Space(5f);
		GUILayout.Label("DARK MENU 1.3 (汉化版", titleStyle, Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(num) });
		for (int i = 0; i < tabs.Length; i++)
		{
			if (GUILayout.Button(tabs[i], (i == currentTab) ? tabSelectedStyle : tabStyle, Array.Empty<GUILayoutOption>()))
			{
				currentTab = i;
			}
		}
		GUILayout.EndVertical();
		GUILayout.BeginVertical(boxStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(num3) });
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
			DrawMiscTab();
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
			DrawTrollingTab();
			break;
		case 8:
			DrawConfigTab();
			break;
		case 9:
			DrawServersTab();
			break;
		case 10:
			DrawCreativeTab();
			break;
		case 11:
			DrawTeleportTab();
			break;
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
	}

	private void DrawSelfTab()
	{
		//IL_0c4d: Unknown result type (might be due to invalid IL or missing references)
		GUILayout.Label("生命值", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		ToggleLogic("god_mode", " 上帝模式", ref godModeActive, PlayerController.GodMode);
		GUILayout.Space(5f);
		ToggleLogic("inf_health", " 无限生命", ref infiniteHealthActive, PlayerController.MaxHealth);
		GUILayout.Space(10f);
		GUILayout.Label("移动", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		ToggleLogic("No_Clip", " 穿墙模式", ref NoclipController.noclipActive, NoclipController.ToggleNoclip);
		GUILayout.Space(5f);
		ToggleLogic("inf_stam", " 无限体力", ref stamineState, PlayerController.MaxStamina);
		GUILayout.Space(5f);
		ToggleLogic("auto_dodge", " 自动闪避", ref AutoDodge.isAutoDodgeEnabled);
		if (AutoDodge.isAutoDodgeEnabled)
		{
			GUILayout.Label("闪避距离: " + Mathf.RoundToInt(AutoDodge.dodgeDistance) + "m", labelStyle, Array.Empty<GUILayoutOption>());
			AutoDodge.dodgeDistance = GUILayout.HorizontalSlider(AutoDodge.dodgeDistance, 3f, 20f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		}
		GUILayout.Space(10f);
		GUILayout.Label("杂项", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		ToggleLogic("rgb_player", " 玩家RGB颜色", ref playerColor.isRandomizing);
		GUILayout.Space(5f);
		ToggleLogic("No_Fog", " 去除迷雾", ref MiscFeatures.NoFogEnabled, MiscFeatures.ToggleNoFog);
		GUILayout.Space(5f);
		ToggleLogic("WaterMark_Toggle", " 显示水印", ref showWatermark);
		GUILayout.Space(5f);
		ToggleLogic("Grab_Guard", " 防抓取保护", ref debounce);
		GUILayout.Space(5f);
		if (GUILayout.Button("给予皇冠", buttonStyle, Array.Empty<GUILayoutOption>()))
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
		ToggleLogic("no_weapon_recoil", " 无武器后坐力", ref Patches.NoWeaponRecoil._isEnabledForConfig, delegate
		{
			ConfigManager.SaveToggle("no_weapon_recoil", Patches.NoWeaponRecoil._isEnabledForConfig);
			PlayerPrefs.Save();
			Debug.Log((object)$"[Self Tab Toggle] Set No Recoil Enabled to: {Patches.NoWeaponRecoil._isEnabledForConfig}");
		});
		GUILayout.Space(10f);
		ToggleLogic("no_weapon_cooldown", " 无武器冷却", ref ConfigManager.NoWeaponCooldownEnabled, delegate
		{
			ConfigManager.SaveToggle("no_weapon_cooldown", ConfigManager.NoWeaponCooldownEnabled);
			PlayerPrefs.Save();
			Debug.Log((object)$"[GUI Toggle] Set No Cooldown Enabled to: {ConfigManager.NoWeaponCooldownEnabled}");
		});
		GUILayout.Space(10f);
		GUILayout.Label($"扩散倍率: {ConfigManager.CurrentSpreadMultiplier:F2}x " + "(" + ((ConfigManager.CurrentSpreadMultiplier <= 0.01f) ? "无扩散" : (Mathf.Approximately(ConfigManager.CurrentSpreadMultiplier, 1f) ? "正常" : $"{ConfigManager.CurrentSpreadMultiplier * 100f:F0}%")) + ")", labelStyle, Array.Empty<GUILayoutOption>());
		float num = GUILayout.HorizontalSlider(ConfigManager.CurrentSpreadMultiplier, 0f, 2f, Array.Empty<GUILayoutOption>());
		if (num != ConfigManager.CurrentSpreadMultiplier)
		{
			ConfigManager.CurrentSpreadMultiplier = num;
			ConfigManager.SaveFloat("weapon_spread_multiplier", num);
			PlayerPrefs.Save();
			Debug.Log((object)$"[GUI Slider] Set Spread Multiplier to: {num}");
		}
		grabThroughWallsEnabled = ToggleLogic("grab_through_walls", " 穿墙抓取", ref grabThroughWallsEnabled, delegate
		{
			Patches.ToggleGrabThroughWalls(grabThroughWallsEnabled);
			ConfigManager.SaveToggle("grab_through_walls", grabThroughWallsEnabled);
			PlayerPrefs.Save();
		});
		GUILayout.Space(10f);
		GUILayout.Label("玩家属性", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Label("力量: " + Mathf.RoundToInt(sliderValueStrength), labelStyle, Array.Empty<GUILayoutOption>());
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
		GUILayout.Label("投掷力量: " + Mathf.RoundToInt(throwStrength), labelStyle, Array.Empty<GUILayoutOption>());
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
		GUILayout.Label("速度: " + Mathf.RoundToInt(sliderValue), labelStyle, Array.Empty<GUILayoutOption>());
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
		GUILayout.Label("抓取范围: " + Mathf.RoundToInt(grabRange), labelStyle, Array.Empty<GUILayoutOption>());
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
		GUILayout.Label("体力恢复延迟: " + Mathf.RoundToInt(staminaRechargeDelay), labelStyle, Array.Empty<GUILayoutOption>());
		staminaRechargeDelay = GUILayout.HorizontalSlider(staminaRechargeDelay, 1f, 30f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (staminaRechargeDelay != oldStaminaRechargeDelay)
		{
			oldStaminaRechargeDelay = staminaRechargeDelay;
			Debug.Log((object)("Stamina Recharge Delay to: " + staminaRechargeDelay));
		}
		GUILayout.Label("体力恢复速率: " + Mathf.RoundToInt(staminaRechargeRate), labelStyle, Array.Empty<GUILayoutOption>());
		staminaRechargeRate = GUILayout.HorizontalSlider(staminaRechargeRate, 1f, 30f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (staminaRechargeDelay != oldStaminaRechargeDelay || staminaRechargeRate != oldStaminaRechargeRate)
		{
			PlayerController.DecreaseStaminaRechargeDelay(staminaRechargeDelay, staminaRechargeRate);
			Debug.Log((object)$"Stamina recharge updated: Delay={staminaRechargeDelay}x, Rate={staminaRechargeRate}x");
			oldStaminaRechargeDelay = staminaRechargeDelay;
			oldStaminaRechargeRate = staminaRechargeRate;
		}
		GUILayout.Label("额外跳跃: " + Mathf.RoundToInt((float)extraJumps), labelStyle, Array.Empty<GUILayoutOption>());
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
		GUILayout.Label("翻滚发射力度: " + Mathf.RoundToInt(tumbleLaunch), labelStyle, Array.Empty<GUILayoutOption>());
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
		GUILayout.Label("跳跃力度: " + Mathf.RoundToInt(jumpForce), labelStyle, Array.Empty<GUILayoutOption>());
		jumpForce = GUILayout.HorizontalSlider(jumpForce, 1f, 30f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (jumpForce != OldjumpForce)
		{
			PlayerController.SetJumpForce(17f + jumpForce);
			OldjumpForce = jumpForce;
		}
		GUILayout.Label("重力: " + Mathf.RoundToInt(customGravity), labelStyle, Array.Empty<GUILayoutOption>());
		customGravity = GUILayout.HorizontalSlider(customGravity, 1f, 30f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (customGravity != OldcustomGravity)
		{
			PlayerController.SetCustomGravity(30f + customGravity);
			OldcustomGravity = customGravity;
		}
		GUILayout.Label("蹲下延迟: " + Mathf.RoundToInt(crouchDelay), labelStyle, Array.Empty<GUILayoutOption>());
		crouchDelay = GUILayout.HorizontalSlider(crouchDelay, 0f, 30f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (crouchDelay != OldcrouchDelay)
		{
			PlayerController.SetCrouchDelay(crouchDelay);
			OldcrouchDelay = crouchDelay;
		}
		GUILayout.Label("蹲下速度: " + Mathf.RoundToInt(crouchSpeed), labelStyle, Array.Empty<GUILayoutOption>());
		crouchSpeed = GUILayout.HorizontalSlider(crouchSpeed, 1f, 30f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (crouchSpeed != OldcrouchSpeed)
		{
			PlayerController.SetCrouchSpeed(crouchSpeed);
			OldcrouchSpeed = crouchSpeed;
		}
		GUILayout.Label("滑行减速 " + Mathf.RoundToInt(slideDecay), labelStyle, Array.Empty<GUILayoutOption>());
		slideDecay = GUILayout.HorizontalSlider(slideDecay, 0f, 20f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		if (slideDecay != OldslideDecay)
		{
			PlayerController.SetSlideDecay(slideDecay);
			OldslideDecay = slideDecay;
		}
		GUILayout.Label("手电筒亮度 " + Mathf.RoundToInt(flashlightIntensity), labelStyle, Array.Empty<GUILayoutOption>());
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
			GUILayout.Label("视野FOV: " + Mathf.RoundToInt(fOV), labelStyle, Array.Empty<GUILayoutOption>());
			float num8 = GUILayout.HorizontalSlider(fOV, 60f, 120f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
			if (num8 != fOV)
			{
				FOVEditor.Instance.SetFOV(num8);
				fieldOfView = num8;
			}
		}
		else
		{
			GUILayout.Label("加载FOV编辑器中...", labelStyle, Array.Empty<GUILayoutOption>());
		}
	}

	private void DrawVisualsTab()
	{
		GUILayout.Space(5f);
		ToggleLogic("modern_esp", " 使用现代ESP", ref useModernESP);
		GUILayout.Label("敌人透视", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		ToggleLogic("enable_en_esp", " 启用敌人透视", ref DebugCheats.drawEspBool);
		if (DebugCheats.drawEspBool)
		{
			GUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
			GUILayout.Space(5f);
			ToggleLogic("show_2d_box_enemy", " 显示2D方框", ref DebugCheats.showEnemyBox);
			GUILayout.Space(5f);
			bool value = DebugCheats.drawChamsBool;
			ToggleLogic("show_chams_enemy", " 显示发光模型", ref value);
			DebugCheats.drawChamsBool = value;
			GUILayout.Space(5f);
			ToggleLogic("show_names_enemy", " 显示名称", ref DebugCheats.showEnemyNames);
			GUILayout.Space(5f);
			ToggleLogic("show_distance_enemy", " 显示距离", ref DebugCheats.showEnemyDistance);
			GUILayout.Space(5f);
			ToggleLogic("show_health_enemy", " 显示血量", ref DebugCheats.showEnemyHP);
			GUILayout.EndVertical();
		}
		GUILayout.Space(10f);
		GUILayout.Label("物品透视", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		ToggleLogic("enable_item_esp", " 启用物品透视", ref DebugCheats.drawItemEspBool);
		if (DebugCheats.drawItemEspBool)
		{
			GUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
			GUILayout.Space(5f);
			ToggleLogic("show_3d_box_item", " 显示3D方框", ref DebugCheats.draw3DItemEspBool);
			GUILayout.Space(5f);
			bool value2 = DebugCheats.drawItemChamsBool;
			ToggleLogic("show_chams_item", " 显示发光模型", ref value2);
			DebugCheats.drawItemChamsBool = value2;
			if (DebugCheats.drawChamsBool || DebugCheats.drawItemChamsBool)
			{
				GUILayout.Space(10f);
				if (GUILayout.Button("发光颜色设置", buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(30f) }))
				{
					showChamsWindow = !showChamsWindow;
				}
			}
			GUILayout.Space(5f);
			ToggleLogic("show_names_item", " 显示名称", ref DebugCheats.showItemNames);
			GUILayout.Space(5f);
			ToggleLogic("show_distance_item", " 显示距离", ref DebugCheats.showItemDistance);
			if (DebugCheats.showItemDistance)
			{
				GUILayout.Label($"最大距离 {DebugCheats.maxItemEspDistance:F0}米", labelStyle, Array.Empty<GUILayoutOption>());
				DebugCheats.maxItemEspDistance = GUILayout.HorizontalSlider(DebugCheats.maxItemEspDistance, 0f, 1000f, Array.Empty<GUILayoutOption>());
			}
			GUILayout.Space(5f);
			ToggleLogic("show_value_item", " 显示价值", ref DebugCheats.showItemValue);
			if (DebugCheats.showItemValue)
			{
				GUILayout.Label($"最小价值 ${DebugCheats.minItemValue}", labelStyle, Array.Empty<GUILayoutOption>());
				DebugCheats.minItemValue = Mathf.RoundToInt(GUILayout.HorizontalSlider((float)DebugCheats.minItemValue, 0f, 50000f, Array.Empty<GUILayoutOption>()));
			}
			GUILayout.Space(5f);
			ToggleLogic("show_dead_heads", " 显示死亡玩家头颅", ref DebugCheats.showPlayerDeathHeads);
			GUILayout.EndVertical();
		}
		GUILayout.Space(10f);
		GUILayout.Label("撤离点透视", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		ToggleLogic("enable_extract_esp", " 启用撤离点透视", ref DebugCheats.drawExtractionPointEspBool);
		if (DebugCheats.drawExtractionPointEspBool)
		{
			GUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
			GUILayout.Space(5f);
			ToggleLogic("show_extract_names", " 显示名称/状态", ref DebugCheats.showExtractionNames);
			GUILayout.Space(5f);
			ToggleLogic("show_extract_distance", " 显示距离", ref DebugCheats.showExtractionDistance);
			GUILayout.EndVertical();
		}
		GUILayout.Space(10f);
		GUILayout.Label("玩家透视", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		ToggleLogic("enable_player_esp", " 启用玩家透视", ref DebugCheats.drawPlayerEspBool);
		if (DebugCheats.drawPlayerEspBool)
		{
			GUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
			GUILayout.Space(5f);
			ToggleLogic("show_2d_box_player", " 显示2D方框", ref DebugCheats.draw2DPlayerEspBool);
			GUILayout.Space(5f);
			ToggleLogic("show_3d_box_player", " 显示3D方框", ref DebugCheats.draw3DPlayerEspBool);
			GUILayout.Space(5f);
			ToggleLogic("show_names_player", " 显示名称", ref DebugCheats.showPlayerNames);
			GUILayout.Space(5f);
			ToggleLogic("show_distance_player", " 显示距离", ref DebugCheats.showPlayerDistance);
			GUILayout.Space(5f);
			ToggleLogic("show_health_player", " 显示血量", ref DebugCheats.showPlayerHP);
			GUILayout.Space(5f);
			ToggleLogic("show_alive_dead_list", " 显示存活/死亡列表", ref showPlayerStatus);
			GUILayout.EndVertical();
		}
		GUILayout.Space(10f);
		GUILayout.Label("追踪线", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		ToggleLogic("trace_enemy", " 敌人追踪线", ref ESPEnhancements.showTraceLinesEnemy);
		GUILayout.Space(5f);
		ToggleLogic("trace_item", " 物品追踪线", ref ESPEnhancements.showTraceLinesItem);
		GUILayout.Space(5f);
		ToggleLogic("trace_player", " 玩家追踪线", ref ESPEnhancements.showTraceLinesPlayer);
		GUILayout.Space(10f);
		GUILayout.Label("ESP预设", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		for (int pi = 0; pi < ESPEnhancements.presetNames.Length; pi++)
		{
			GUIStyle presetBtnStyle = ((int)ESPEnhancements.currentPreset == pi) ? tabSelectedStyle : buttonStyle;
			if (GUILayout.Button(ESPEnhancements.presetNames[pi], presetBtnStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(70f) }))
			{
				ESPEnhancements.ApplyPreset((ESPEnhancements.ESPPreset)pi);
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.Label("雷达", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		ToggleLogic("mini_radar", " 小地图雷达", ref MiniRadar.isRadarEnabled);
		if (MiniRadar.isRadarEnabled)
		{
			GUILayout.Label("扫描范围: " + Mathf.RoundToInt(MiniRadar.radarRange), labelStyle, Array.Empty<GUILayoutOption>());
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
		GUILayout.Label("选择玩家:", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		for (int i = 0; i < playerNames.Count; i++)
		{
			GUIStyle val = new GUIStyle(GUI.skin.button);
			val.alignment = (TextAnchor)4;
			val.fontSize = 14;
			val.fontStyle = (FontStyle)1;
			val.fixedHeight = 28f;
			val.margin = new RectOffset(2, 2, 2, 2);
			val.padding = new RectOffset(8, 8, 4, 4);
			val.border = new RectOffset(4, 4, 4, 4);
			if (i == selectedPlayerIndex)
			{
				val.normal.background = MakeSolidBackground((Color)(new Color32((byte)50, (byte)50, (byte)70, byte.MaxValue)));
				val.normal.textColor = (Color)(new Color32(byte.MaxValue, (byte)165, (byte)0, byte.MaxValue));
			}
			else
			{
				val.normal.background = MakeSolidBackground((Color)(new Color32((byte)30, (byte)30, (byte)30, byte.MaxValue)));
				val.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
				val.hover.background = MakeSolidBackground((Color)(new Color32((byte)40, (byte)40, (byte)50, byte.MaxValue)));
				val.hover.textColor = Color.white;
			}
			if (GUILayout.Button(playerNames[i], val, Array.Empty<GUILayoutOption>()))
			{
				selectedPlayerIndex = i;
			}
		}
		GUILayout.Space(40f);
		if (GUILayout.Button("-1 伤害", buttonStyle, Array.Empty<GUILayoutOption>()))
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
		if (GUILayout.Button("最大治疗", buttonStyle, Array.Empty<GUILayoutOption>()))
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
		if (GUILayout.Button("击杀", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			Players.KillSelectedPlayer(selectedPlayerIndex, playerList, playerNames);
			Debug.Log((object)("Player killed: " + playerNames[selectedPlayerIndex]));
		}
		if (GUILayout.Button("复活", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			Players.ReviveSelectedPlayer(selectedPlayerIndex, playerList, playerNames);
			Debug.Log((object)("Player revived: " + playerNames[selectedPlayerIndex]));
		}
		if (GUILayout.Button("翻滚 (10秒", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			Players.ForcePlayerTumble();
			Debug.Log((object)("Player tumbled: " + playerNames[selectedPlayerIndex]));
		}
		if (GUILayout.Button(showTeleportUI ? "隐藏传送选项" : "显示传送选项", buttonStyle, Array.Empty<GUILayoutOption>()))
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
	}

	private void DrawMiscTab()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d1: Unknown result type (might be due to invalid IL or missing references)
		UpdatePlayerList();
		GUILayout.Label("选择玩家:", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		for (int i = 0; i < playerNames.Count; i++)
		{
			GUIStyle val = new GUIStyle(GUI.skin.button);
			val.alignment = (TextAnchor)4;
			val.fontSize = 14;
			val.fontStyle = (FontStyle)1;
			val.fixedHeight = 28f;
			val.margin = new RectOffset(2, 2, 2, 2);
			val.padding = new RectOffset(8, 8, 4, 4);
			val.border = new RectOffset(4, 4, 4, 4);
			if (i == selectedPlayerIndex)
			{
				val.normal.background = MakeSolidBackground((Color)(new Color32((byte)50, (byte)50, (byte)70, byte.MaxValue)));
				val.normal.textColor = (Color)(new Color32(byte.MaxValue, (byte)165, (byte)0, byte.MaxValue));
			}
			else
			{
				val.normal.background = MakeSolidBackground((Color)(new Color32((byte)30, (byte)30, (byte)30, byte.MaxValue)));
				val.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
				val.hover.background = MakeSolidBackground((Color)(new Color32((byte)40, (byte)40, (byte)50, byte.MaxValue)));
				val.hover.textColor = Color.white;
			}
			if (GUILayout.Button(playerNames[i], val, Array.Empty<GUILayoutOption>()))
			{
				selectedPlayerIndex = i;
			}
		}
		GUILayout.Space(40f);
		if (GUILayout.Button("[主机] 生成金钱", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			GameObject localPlayer = DebugCheats.GetLocalPlayer();
			if ((Object)(object)localPlayer != (Object)null)
			{
				ItemSpawner.SpawnMoney(localPlayer.transform.position + Vector3.up * 1.5f);
				Debug.Log((object)"Money spawned.");
			}
		}
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.EndHorizontal();
		if (showLevelDropdown)
		{
			levelDropdownScroll = GUILayout.BeginScrollView(levelDropdownScroll, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(150f) });
			for (int j = 0; j < availableLevels.Length; j++)
			{
				if (GUILayout.Button(availableLevels[j], buttonStyle, Array.Empty<GUILayoutOption>()))
				{
					selectedLevelIndex = j;
					showLevelDropdown = false;
				}
			}
			GUILayout.EndScrollView();
		}
		GUILayout.Space(10f);
		GUILayout.Label("伪造名字", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("伪造", buttonStyle, Array.Empty<GUILayoutOption>()) && !string.IsNullOrEmpty(spoofedNameText))
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
				string text2 = ((k == 0) ? "All" : playerNames[k - 1]);
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
		if (GUILayout.Button("重置伪造名称", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ChatHijack.ToggleNameSpoofing(enable: false, "", spoofTargetVisibleName, playerList, playerNames);
			spoofedNameText = "Text";
		}
		GUILayout.Space(10f);
		ToggleLogic("persistent_spoof_name", " 持久伪造名称", ref spoofNameActive);
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
		GUILayout.Space(10f);
		GUILayout.Label("伪造颜色", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("伪造颜色", buttonStyle, Array.Empty<GUILayoutOption>()) && int.TryParse(colorIndexText, out var result) && colorNameMapping.ContainsKey(result))
		{
			ChatHijack.ChangePlayerColor(result, colorTargetVisibleName, playerList, playerNames);
		}
		if (GUILayout.Button(colorTargetVisibleName, buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			colorDropdownVisible = !colorDropdownVisible;
		}
		if (GUILayout.Button(colorNameMapping.TryGetValue(int.Parse(colorIndexText), out var value) ? value : "选择颜色", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			showColorIndexDropdown = !showColorIndexDropdown;
		}
		GUILayout.EndHorizontal();
		if (!playerRainbowStates.ContainsKey(selectedPlayerIndex))
		{
			playerRainbowStates[selectedPlayerIndex] = false;
		}
		bool rainbowState = playerRainbowStates[selectedPlayerIndex];
		ToggleLogic("rainbow_spoof_" + selectedPlayerIndex, " 彩虹模式", ref rainbowState, delegate
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
				string text3 = ((num == 0) ? "All" : playerNames[num - 1]);
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
			foreach (KeyValuePair<int, string> item in colorNameMapping)
			{
				if (GUILayout.Button(item.Value, buttonStyle, Array.Empty<GUILayoutOption>()))
				{
					colorIndexText = item.Key.ToString();
					showColorIndexDropdown = false;
				}
			}
			GUILayout.EndScrollView();
		}
		GUILayout.Space(10f);
		GUILayout.Label("聊天刷屏", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("发送", buttonStyle, Array.Empty<GUILayoutOption>()))
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
				string text4 = ((num2 == 0) ? "All" : playerNames[num2 - 1]);
				if (GUILayout.Button(text4, buttonStyle, Array.Empty<GUILayoutOption>()))
				{
					ChatDropdownVisibleName = text4;
					ChatDropdownVisible = false;
				}
			}
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("Activate All Extraction Points", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			MiscFeatures.ForceActivateAllExtractionPoints();
		}
		GUILayout.Space(10f);
		GUILayout.Label("Map Tweaks (can't be undone in level):", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("Disable '?' Overlay", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			MapTools.changeOverlayStatus(status: true);
		}
		if (GUILayout.Button("Discover Map Valuables", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			MapTools.DiscoveryMapValuables();
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
			if (cachedFilteredEnemySetups == null || cachedEnemySetupNames == null)
			{
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
				cachedFilteredEnemySetups = new List<EnemySetup>();
				cachedEnemySetupNames = new List<string>();
				if (list != null)
				{
					foreach (EnemySetup item2 in list)
					{
						if ((Object)(object)item2 != (Object)null && !((Object)item2).name.Contains("Enemy Group"))
						{
							string item = (((Object)item2).name.StartsWith("Enemy -") ? ((Object)item2).name.Substring("Enemy -".Length).Trim() : ((Object)item2).name);
							cachedFilteredEnemySetups.Add(item2);
							cachedEnemySetupNames.Add(item);
						}
					}
				}
			}
			UpdateEnemyList();
			scrollPos = GUILayout.BeginScrollView(scrollPos, Array.Empty<GUILayoutOption>());
			GUILayout.Label("选择敌人:", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
			enemyScrollPosition = GUILayout.BeginScrollView(enemyScrollPosition, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height((float)Mathf.Min(200, enemyNames.Count * 35)) });
			for (int i = 0; i < enemyNames.Count; i++)
			{
				GUIStyle val = new GUIStyle(GUI.skin.button);
				val.fontSize = 13;
				val.fontStyle = (FontStyle)1;
				val.alignment = (TextAnchor)4;
				val.margin = new RectOffset(4, 4, 2, 2);
				val.padding = new RectOffset(6, 6, 4, 4);
				val.border = new RectOffset(4, 4, 4, 4);
				if (i == selectedEnemyIndex)
				{
					val.normal.background = MakeSolidBackground((Color)(new Color32((byte)60, (byte)30, (byte)10, (byte)200)));
					val.normal.textColor = new Color(1f, 0.55f, 0.1f);
				}
				else
				{
					val.normal.background = MakeSolidBackground((Color)(new Color32((byte)30, (byte)30, (byte)35, (byte)180)));
					val.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
				}
				if (GUILayout.Button(enemyNames[i], val, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(30f) }))
				{
					selectedEnemyIndex = i;
				}
			}
			GUILayout.EndScrollView();
			GUI.color = Color.white;
			GUILayout.Space(40f);
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (GUILayout.Button("[主机] 生成", buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				TrySpawnEnemy();
			}
			spawnCountText = GUILayout.TextField(spawnCountText, textFieldStyle, Array.Empty<GUILayoutOption>());
			if (GUILayout.Button((spawnEnemyIndex >= 0 && spawnEnemyIndex < cachedEnemySetupNames.Count) ? cachedEnemySetupNames[spawnEnemyIndex] : "选择敌人", buttonStyle, Array.Empty<GUILayoutOption>()))
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
			if (GUILayout.Button("击杀敌人", buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				Enemies.KillSelectedEnemy(selectedEnemyIndex, enemyList, enemyNames);
			}
			if (GUILayout.Button("击杀所有敌人", buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				Enemies.KillAllEnemies();
			}
			ToggleLogic("blind_enemies", " 致盲敌人", ref blindEnemies, delegate
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Expected O, but got Unknown
				Hashtable val2 = new Hashtable();
				val2[(object)"isBlindEnabled"] = blindEnemies;
				PhotonNetwork.LocalPlayer.SetCustomProperties(val2, (Hashtable)null, (WebFlags)null);
				ConfigManager.SaveToggle("blind_enemies", blindEnemies);
			});
			if (GUILayout.Button(showEnemyTeleportUI ? "隐藏传送选项" : "显示传送选项", buttonStyle, Array.Empty<GUILayoutOption>()))
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
				GUILayout.Label("传送敌人到 ↓", labelStyle, Array.Empty<GUILayoutOption>());
				if (GUILayout.Button((enemyTeleportDestIndex >= 0 && enemyTeleportDestIndex < enemyTeleportDestOptions.Length) ? enemyTeleportDestOptions[enemyTeleportDestIndex] : "无玩家可用", buttonStyle, Array.Empty<GUILayoutOption>()))
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
				if (GUILayout.Button("执行传送", buttonStyle, Array.Empty<GUILayoutOption>()))
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
			GUILayout.EndScrollView();
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
		GUILayout.Label("自动功能", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		ToggleLogic("auto_pickup", " 自动拾取", ref AutoPickup.isAutoPickupEnabled);
		if (AutoPickup.isAutoPickupEnabled)
		{
			GUILayout.Label("拾取范围: " + Mathf.RoundToInt(AutoPickup.pickupRadius) + "m", labelStyle, Array.Empty<GUILayoutOption>());
			AutoPickup.pickupRadius = GUILayout.HorizontalSlider(AutoPickup.pickupRadius, 5f, 100f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
			GUILayout.Label("最小价值: " + AutoPickup.minPickupValue, labelStyle, Array.Empty<GUILayoutOption>());
			AutoPickup.minPickupValue = (int)GUILayout.HorizontalSlider((float)AutoPickup.minPickupValue, 0f, 5000f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
		}
		GUILayout.Space(5f);
		ToggleLogic("auto_sell", " 自动卖出（传送到撤离点）", ref AutoPickup.isAutoSellEnabled);
		GUILayout.Space(10f);
		GUILayout.Label("选择物品:", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		List<ItemTeleport.GameItem> list = itemList.OrderByDescending((ItemTeleport.GameItem item) => item.Value).ToList();
		itemScroll = GUILayout.BeginScrollView(itemScroll, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(200f) });
		for (int num = 0; num < list.Count; num++)
		{
			GUIStyle val = new GUIStyle(GUI.skin.button);
			val.fontSize = 13;
			val.fontStyle = (FontStyle)1;
			val.alignment = (TextAnchor)4;
			val.margin = new RectOffset(4, 4, 2, 2);
			val.padding = new RectOffset(6, 6, 4, 4);
			val.border = new RectOffset(4, 4, 4, 4);
			if (num == selectedItemIndex)
			{
				val.normal.background = MakeSolidBackground((Color)(new Color32((byte)45, (byte)25, (byte)5, (byte)220)));
				val.normal.textColor = new Color(1f, 0.55f, 0.1f);
			}
			else
			{
				val.normal.background = MakeSolidBackground((Color)(new Color32((byte)28, (byte)28, (byte)30, (byte)180)));
				val.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
			}
			if (GUILayout.Button($"{list[num].Name}   [价值 ${list[num].Value}]", val, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(30f) }))
			{
				selectedItemIndex = num;
			}
		}
		GUILayout.EndScrollView();
		GUI.color = Color.white;
		if (GUILayout.Button("将物品传送到我", buttonStyle, Array.Empty<GUILayoutOption>()) && selectedItemIndex >= 0 && selectedItemIndex < list.Count)
		{
			ItemTeleport.TeleportItemToMe(list[selectedItemIndex]);
		}
		if (GUILayout.Button("将所有物品传送到我", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ItemTeleport.TeleportAllItemsToMe();
		}
		GUILayout.Space(10f);
		GUILayout.Label("修改物品价值", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		int num2 = (int)Mathf.Pow(10f, itemValueSliderPos);
		GUILayout.Label($"${num2:N0}", labelStyle, Array.Empty<GUILayoutOption>());
		itemValueSliderPos = GUILayout.HorizontalSlider(itemValueSliderPos, 3f, 9f, Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("应用价值修改", buttonStyle, Array.Empty<GUILayoutOption>()) && selectedItemIndex >= 0 && selectedItemIndex < list.Count)
		{
			ItemTeleport.SetItemValue(list[selectedItemIndex], num2);
		}
		if (GUILayout.Button(showItemSpawner ? "隐藏物品生成器" : "显示物品生成器", buttonStyle, Array.Empty<GUILayoutOption>()))
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
			GUILayout.Label("选择要生成的物品:", labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(160f) });
			itemSpawnSearch = GUILayout.TextField(itemSpawnSearch, textFieldStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(200f) });
			GUILayout.EndHorizontal();
			List<string> list2 = (string.IsNullOrWhiteSpace(itemSpawnSearch) ? availableItemsList : availableItemsList.Where((string item) => item.ToLower().Contains(itemSpawnSearch.ToLower())).ToList());
			itemSpawnerScroll = GUILayout.BeginScrollView(itemSpawnerScroll, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(150f) });
			for (int num3 = 0; num3 < list2.Count; num3++)
			{
				GUIStyle val2 = new GUIStyle(GUI.skin.button);
				val2.fontSize = 13;
				val2.fontStyle = (FontStyle)1;
				val2.alignment = (TextAnchor)4;
				val2.margin = new RectOffset(4, 4, 2, 2);
				val2.padding = new RectOffset(6, 6, 4, 4);
				val2.border = new RectOffset(4, 4, 4, 4);
				if (num3 == selectedItemToSpawnIndex)
				{
					val2.normal.background = MakeSolidBackground((Color)(new Color32((byte)45, (byte)25, (byte)5, (byte)220)));
					val2.normal.textColor = new Color(1f, 0.55f, 0.1f);
				}
				else
				{
					val2.normal.background = MakeSolidBackground((Color)(new Color32((byte)28, (byte)28, (byte)30, (byte)180)));
					val2.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
				}
				if (GUILayout.Button(list2[num3], val2, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(30f) }))
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
			if (GUILayout.Button("生成选中物品", buttonStyle, Array.Empty<GUILayoutOption>()))
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
			if (GUILayout.Button("Spawn 50 of Selected Item", buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				ItemSpawner.SpawnSelectedItemMultiple(50, availableItemsList, selectedItemToSpawnIndex, itemSpawnValue);
			}
			GUI.enabled = true;
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
		GUILayout.Label("热键配置", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Space(5f);
		GUILayout.Label("如何设置热键:", labelStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Label("1. 点击按键字段 →按下想要的键", labelStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Label("2. 点击动作字段 →选择功能", labelStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Space(10f);
		GUILayout.Label("警告: 确保每个按键只分配给一个动作", warningStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Space(10f);
		GUILayout.Label("系统按键", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		DrawHotkeyField("菜单开关", hotkeyManager.MenuToggleKey, delegate
		{
			hotkeyManager.StartConfigureSystemKey(0);
		}, 0);
		DrawHotkeyField("重载:", hotkeyManager.ReloadKey, delegate
		{
			hotkeyManager.StartConfigureSystemKey(1);
		}, 1);
		DrawHotkeyField("卸载:", hotkeyManager.UnloadKey, delegate
		{
			hotkeyManager.StartConfigureSystemKey(2);
		}, 2);
		GUILayout.Space(20f);
		GUILayout.Label("动作热键", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		for (int num = 0; num < 12; num++)
		{
			KeyCode hotkeyForSlot = hotkeyManager.GetHotkeyForSlot(num);
			string obj = ((hotkeyManager.SelectedHotkeySlot == num && hotkeyManager.ConfiguringHotkey) ? "按下任意键..." : (((int)hotkeyForSlot == 0) ? "未设置" : hotkeyForSlot.ToString()));
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
					Debug.Log((object)"请先为此槽位分配一个按键");
				}
			}
			if (GUILayout.Button("清除", buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				hotkeyManager.ClearHotkeyBinding(num);
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.Space(10f);
		if (GUILayout.Button("保存热键设置", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			hotkeyManager.SaveHotkeySettings();
			Debug.Log((object)"热键设置已保存");
		}
		GUILayout.EndVertical();
	}

	private void DrawTrollingTab()
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
		GUILayout.Label("Select a player:", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		for (int i = 0; i < playerNames.Count; i++)
		{
			GUIStyle val = new GUIStyle(GUI.skin.button);
			val.alignment = (TextAnchor)4;
			val.fontSize = 14;
			val.fontStyle = (FontStyle)1;
			val.fixedHeight = 28f;
			val.margin = new RectOffset(2, 2, 2, 2);
			val.padding = new RectOffset(8, 8, 4, 4);
			val.border = new RectOffset(4, 4, 4, 4);
			if (i == selectedPlayerIndex)
			{
				val.normal.background = MakeSolidBackground((Color)(new Color32((byte)50, (byte)50, (byte)70, byte.MaxValue)));
				val.normal.textColor = (Color)(new Color32(byte.MaxValue, (byte)165, (byte)0, byte.MaxValue));
			}
			else
			{
				val.normal.background = MakeSolidBackground((Color)(new Color32((byte)30, (byte)30, (byte)30, byte.MaxValue)));
				val.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
				val.hover.background = MakeSolidBackground((Color)(new Color32((byte)40, (byte)40, (byte)50, byte.MaxValue)));
				val.hover.textColor = Color.white;
			}
			if (GUILayout.Button(playerNames[i], val, Array.Empty<GUILayoutOption>()))
			{
				selectedPlayerIndex = i;
			}
		}
		GUILayout.Space(40f);
		if (GUILayout.Button("激活冲气泡泡", buttonStyle, Array.Empty<GUILayoutOption>()))
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
		if (GUILayout.Button("触摸小丑鼻子", buttonStyle, Array.Empty<GUILayoutOption>()))
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
		if (GUILayout.Button("强制故障", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			Troll.ForcePlayerGlitch();
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("强制取消静音", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			MiscFeatures.ForcePlayerMicVolume(100);
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("强制静音", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			MiscFeatures.ForcePlayerMicVolume(-1);
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("强制无限翻滚", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			Players.ForcePlayerTumble(9999999f);
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("无限加载屏幕", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			Troll.InfiniteLoadingSelectedPlayer();
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("移除无限加载屏幕", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			Troll.SceneRecovery();
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("崩溃选中玩家", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			MiscFeatures.CrashSelectedPlayerNew();
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("引爆所有爆炸物", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			Object.FindObjectOfType<ItemMine>();
			typeof(ItemGrenade).GetMethod("TickStartRPC", BindingFlags.Instance | BindingFlags.NonPublic);
			MiscFeatures.ExlploadAll();
			Debug.Log((object)"Detonated All Grenades/Mines");
		}
		GUILayout.Space(5f);
		if (GUILayout.Button("崩溃大厅", buttonStyle, Array.Empty<GUILayoutOption>()))
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
	}

	private void DrawConfigTab()
	{
		GUILayout.Label("配置", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Label(configstatus, titleStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Label("PlayerPrefs 配置", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("保存配置", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ConfigManager.SaveAllToggles();
			configstatus = "配置已保存";
		}
		if (GUILayout.Button("加载配置", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			ConfigManager.LoadAllToggles();
			configstatus = "配置已加载";
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.Label("JSON 配置文件", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("保存为JSON文件", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			JsonConfig.SaveToFile();
			configstatus = "JSON已保存: " + JsonConfig.GetConfigPath();
		}
		if (GUILayout.Button("从JSON文件加载", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			JsonConfig.LoadFromFile();
			configstatus = "JSON已加载";
		}
		GUILayout.EndHorizontal();
		GUILayout.Label("路径: " + JsonConfig.GetConfigPath(), labelStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Space(15f);
		GUILayout.Label("语言/Language", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		if (GUILayout.Button(LanguageManager.Get("switch_lang"), buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			LanguageManager.ToggleLanguage();
			tabs = LanguageManager.GetTabNames();
			configstatus = "语言已切换 / Language switched";
		}
	}

	private void DrawFeatureSelectionWindow(int id)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		GUILayout.Label("选择要绑定的功能", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
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
		if (GUILayout.Button("取消", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			showingActionSelector = false;
		}
		GUI.DragWindow(new Rect(0f, 0f, 10000f, 30f));
	}

	private unsafe void DrawHotkeyField(string label, KeyCode key, Action configureCallback, int index)
	{
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label(label, buttonStyle, Array.Empty<GUILayoutOption>());
		if (GUILayout.Button((hotkeyManager.ConfiguringSystemKey && hotkeyManager.SystemKeyConfigIndex == index && hotkeyManager.WaitingForAnyKey) ? "Press any key..." : key.ToString(), buttonStyle, Array.Empty<GUILayoutOption>()))
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
		GUILayout.Label("Chams 颜色选择器", titleStyle, Array.Empty<GUILayoutOption>());
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
		GUILayout.Label("服务器浏览器", titleStyle, Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("刷新房间", buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(160f) }))
		{
			LobbyHostCache.Clear();
			LobbyMemberCache.Clear();
			LobbyFinder.AlreadyTriedLobbies.Clear();
			LobbyFinder.RefreshLobbies();
		}
		ToggleLogic("hide_full_lobbies", " 隐藏满员房间", ref hideFullLobbies);
		ToggleLogic("show_lobby_members", " 显示房间成员", ref showMemberWindow);
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label("搜索:", labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(60f) });
		lobbySearchTerm = GUILayout.TextField(lobbySearchTerm, textFieldStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(260f) });
		if (GUILayout.Button("区域 A-Z", buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(100f) }))
		{
			sortMode = SortMode.RegionAZ;
		}
		if (GUILayout.Button("区域 Z-A", buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(100f) }))
		{
			sortMode = SortMode.RegionZA;
		}
		if (GUILayout.Button("最多玩家", buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(110f) }))
		{
			sortMode = SortMode.MostPlayers;
		}
		if (GUILayout.Button("最少玩家", buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(120f) }))
		{
			sortMode = SortMode.LeastPlayers;
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label("房间名称", labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(280f) });
		GUILayout.Space(10f);
		GUILayout.Label("玩家数", labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(80f) });
		GUILayout.Space(10f);
		GUILayout.Label("区域", labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(80f) });
		GUILayout.Space(50f);
		GUILayout.Label("主机", labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandWidth(true) });
		GUILayout.EndHorizontal();
		GUILayout.Space(6f);
		serverListScroll = GUILayout.BeginScrollView(serverListScroll, boxStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(500f) });
		List<Lobby> list = new List<Lobby>(LobbyFinder.FoundLobbies);
		switch (sortMode)
		{
		case SortMode.RegionAZ:
			list.Sort((Lobby a, Lobby b) => string.Compare(a.GetData("Region"), b.GetData("Region")));
			break;
		case SortMode.RegionZA:
			list.Sort((Lobby a, Lobby b) => string.Compare(b.GetData("Region"), a.GetData("Region")));
			break;
		case SortMode.MostPlayers:
			list.Sort((Lobby a, Lobby b) => b.MemberCount.CompareTo(a.MemberCount));
			break;
		case SortMode.LeastPlayers:
			list.Sort((Lobby a, Lobby b) => a.MemberCount.CompareTo(b.MemberCount));
			break;
		}
		foreach (Lobby item in list)
		{
			Lobby current = item;
			if ((hideFullLobbies && current.MemberCount >= current.MaxMembers) || !LobbyHostCache.ContainsKey(current.Id) || current.MemberCount < 3)
			{
				continue;
			}
			string value;
			string text = (LobbyHostCache.TryGetValue(current.Id, out value) ? value : "获取中..");
			if (!text.Contains("Failed (0)") && (string.IsNullOrWhiteSpace(lobbySearchTerm) || ((object)current.Id/*cast due to .constrained prefix*/).ToString().IndexOf(lobbySearchTerm, StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf(lobbySearchTerm, StringComparison.OrdinalIgnoreCase) >= 0 || (LobbyMemberCache.TryGetValue(current.Id, out var value2) && value2.Exists((string m) => m.IndexOf(lobbySearchTerm, StringComparison.OrdinalIgnoreCase) >= 0))))
			{
				GUIStyle val = new GUIStyle(GUI.skin.button)
				{
					alignment = (TextAnchor)3,
					fontSize = 14,
					fontStyle = (FontStyle)1,
					fixedHeight = 32f,
					margin = new RectOffset(2, 2, 2, 2),
					padding = new RectOffset(8, 8, 4, 4),
					border = new RectOffset(4, 4, 4, 4)
				};
				if (current.Id.Value == selectedLobbyId.Value)
				{
					val.normal.background = rowBgSelected;
					val.normal.textColor = (Color)(new Color32(byte.MaxValue, (byte)165, (byte)0, byte.MaxValue));
				}
				else
				{
					val.normal.background = rowBgNormal;
					val.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
					val.hover.background = rowBgHover;
					val.hover.textColor = Color.white;
				}
				GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				string text2 = (text.Contains("(") ? text.Substring(0, text.IndexOf("(")).Trim() : text);
				string text3 = "房间: " + (string.IsNullOrWhiteSpace(text2) ? "未知" : text2);
				if (current.MaxMembers > 6)
				{
					text3 += " <color=red>(模组)</color>";
				}
				string data = current.GetData("Region");
				int num = Mathf.Max(0, current.MemberCount - 1);
				int maxMembers = current.MaxMembers;
				if (GUILayout.Button(text3, val, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(280f) }))
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
		if (GUILayout.Button("加入房间", buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(120f) }))
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
			string arg = (LobbyHostCache.TryGetValue(selectedLobbyId, out value3) ? value3 : "未知");
			GUILayout.Label($"选中: {selectedLobbyId} | 主机: {arg} | 区域: {data2}", labelStyle, Array.Empty<GUILayoutOption>());
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("获取邀请链接", buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(150f) }))
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
		if (GUILayout.Button("关闭", buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Height(25f) }))
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
		GUILayout.Label("房间成员", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
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
			GUILayout.Label("Fetching players...", warningStyle, Array.Empty<GUILayoutOption>());
		}
		GUILayout.EndScrollView();
		GUILayout.Space(8f);
		if (GUILayout.Button("Close", buttonStyle, Array.Empty<GUILayoutOption>()))
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
		if (titleStyle == null)
		{
			GUIStyle val = new GUIStyle(GUI.skin.label)
			{
				fontSize = 20,
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)4
			};
			val.normal.textColor = (Color)(new Color32(byte.MaxValue, (byte)165, (byte)0, byte.MaxValue));
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
			tabStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
			tabStyle.normal.background = MakeSolidBackground((Color)(new Color32((byte)40, (byte)40, (byte)40, byte.MaxValue)));
			tabStyle.hover.textColor = (Color)(new Color32(byte.MaxValue, (byte)165, (byte)0, byte.MaxValue));
			tabStyle.hover.background = MakeSolidBackground((Color)(new Color32((byte)50, (byte)50, (byte)50, byte.MaxValue)));
			tabStyle.active.textColor = Color.white;
			tabStyle.active.background = MakeSolidBackground((Color)(new Color32((byte)20, (byte)20, (byte)20, byte.MaxValue)));
			tabSelectedStyle = new GUIStyle(tabStyle);
			tabSelectedStyle.normal.background = MakeSolidBackground((Color)(new Color32((byte)30, (byte)30, (byte)30, byte.MaxValue)));
			tabSelectedStyle.normal.textColor = (Color)(new Color32(byte.MaxValue, (byte)165, (byte)0, byte.MaxValue));
			GUIStyle val2 = new GUIStyle(GUI.skin.label)
			{
				fontSize = 16,
				fontStyle = (FontStyle)1
			};
			val2.normal.textColor = new Color(1f, 0.5f, 0f);
			sectionHeaderStyle = val2;
			GUIStyle val3 = new GUIStyle(GUI.skin.box);
			val3.normal.background = MakeSolidBackground((Color)(new Color32((byte)25, (byte)28, (byte)35, (byte)220)));
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
			horizontalThumbStyle.normal.background = MakeSolidBackground((Color)(new Color32((byte)90, (byte)90, (byte)120, byte.MaxValue)));
			horizontalThumbStyle.hover.background = MakeSolidBackground((Color)(new Color32((byte)110, (byte)110, (byte)140, byte.MaxValue)));
			horizontalThumbStyle.active.background = MakeSolidBackground((Color)(new Color32((byte)130, (byte)130, (byte)160, byte.MaxValue)));
			GUI.skin.horizontalSliderThumb = horizontalThumbStyle;
			verticalThumbStyle = new GUIStyle(GUI.skin.verticalSliderThumb);
			verticalThumbStyle.fixedWidth = 16f;
			verticalThumbStyle.fixedHeight = 16f;
			verticalThumbStyle.border = new RectOffset(4, 4, 4, 4);
			verticalThumbStyle.margin = new RectOffset(-2, -2, -2, -2);
			verticalThumbStyle.padding = new RectOffset(0, 0, 0, 0);
			verticalThumbStyle.normal.background = MakeSolidBackground((Color)(new Color32((byte)90, (byte)90, (byte)120, byte.MaxValue)));
			verticalThumbStyle.hover.background = MakeSolidBackground((Color)(new Color32((byte)110, (byte)110, (byte)140, byte.MaxValue)));
			verticalThumbStyle.active.background = MakeSolidBackground((Color)(new Color32((byte)130, (byte)130, (byte)160, byte.MaxValue)));
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
			Color32 val4 = new Color32((byte)45, (byte)45, (byte)55, byte.MaxValue);
			Color32 val5 = new Color32((byte)60, (byte)50, (byte)35, byte.MaxValue);
			Color32 val6 = new Color32((byte)90, (byte)60, (byte)30, byte.MaxValue);
			Color32 val7 = new Color32((byte)240, (byte)240, (byte)240, byte.MaxValue);
			Color32 val8 = new Color32(byte.MaxValue, (byte)165, (byte)0, byte.MaxValue);
			buttonStyle.normal.background = MakeSolidBackground((Color)(val4));
			buttonStyle.normal.textColor = (Color)(val7);
			buttonStyle.hover.background = MakeSolidBackground((Color)(val5));
			buttonStyle.hover.textColor = (Color)(val8);
			buttonStyle.active.background = MakeSolidBackground((Color)(val6));
			buttonStyle.active.textColor = (Color)(val8);
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
			Texture2D background = MakeSolidBackground((Color)(new Color32((byte)15, (byte)18, (byte)25, (byte)230)));
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
			backgroundStyle = new GUIStyle(GUI.skin.window);
			backgroundStyle.normal.background = MakeSolidBackground((Color)(new Color32((byte)15, (byte)18, (byte)25, (byte)230)));
		}
		if (boxStyle == null || (Object)(object)boxStyle.normal.background == (Object)null)
		{
			boxStyle = new GUIStyle(GUI.skin.box);
			boxStyle.normal.background = MakeSolidBackground((Color)(new Color32((byte)25, (byte)28, (byte)35, (byte)220)));
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
			for (int i = 0; i < result; i++)
			{
				// 通过反射调用SpawnSpecificEnemy
				var esType = typeof(RunManager).Assembly.GetType("EnemySpawner");
				if (esType != null)
				{
					var spawnMethod = esType.GetMethod("SpawnSpecificEnemy", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
					spawnMethod?.Invoke(null, new object[] { val, cachedFilteredEnemySetups[spawnEnemyIndex], position });
				}
			}
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
		GUILayout.Label("Teleport Options", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button((teleportPlayerSourceIndex >= 0 && teleportPlayerSourceIndex < teleportPlayerSourceOptions.Length) ? teleportPlayerSourceOptions[teleportPlayerSourceIndex] : "No source available", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			showSourceDropdown = !showSourceDropdown;
		}
		GUILayout.Label("to", labelStyle, Array.Empty<GUILayoutOption>());
		if (GUILayout.Button((teleportPlayerDestIndex >= 0 && teleportPlayerDestIndex < teleportPlayerDestOptions.Length) ? teleportPlayerDestOptions[teleportPlayerDestIndex] : "No destination available", buttonStyle, Array.Empty<GUILayoutOption>()))
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
		if (GUILayout.Button("Execute Teleport", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			Teleport.ExecuteTeleportWithSeparateOptions(teleportPlayerSourceIndex, teleportPlayerDestIndex, teleportPlayerSourceOptions, teleportPlayerDestOptions, playerList);
			Debug.Log((object)"Teleport executed successfully");
			showSourceDropdown = false;
			showDestDropdown = false;
		}
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
	// 创造模式 Tab
	// =====================================================
	private void DrawCreativeTab()
	{
		GUILayout.Label("创造模式", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Space(5f);
		GUILayout.Label("类似Minecraft创造模式:", labelStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Label("• 无敌 + 飞行 + 无限体力", labelStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Label("• 穿墙抓取 + 无武器冷却", labelStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Label("• 去除迷雾", labelStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Space(10f);

		bool prevCreative = CreativeMode.isCreativeMode;
		ToggleLogic("creative_mode", " 创造模式", ref CreativeMode.isCreativeMode, CreativeMode.ToggleCreativeMode);

		GUILayout.Space(10f);
		if (CreativeMode.isCreativeMode)
		{
			GUILayout.Label("状态: 创造模式已启用", warningStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Space(10f);
			GUILayout.Label("飞行控制", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label("WASD - 移动    Space - 上升    Ctrl - 下降", labelStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label("Shift - 加速飞行", labelStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Space(10f);
			GUILayout.Label("已激活功能:", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label("  ✓ 上帝模式", labelStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label("  ✓ 无限生命", labelStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label("  ✓ 穿墙飞行", labelStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label("  ✓ 无限体力", labelStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label("  ✓ 穿墙抓取", labelStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label("  ✓ 无武器冷却", labelStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label("  ✓ 去除迷雾", labelStyle, Array.Empty<GUILayoutOption>());
		}
		else
		{
			GUILayout.Label("状态: 生存模式", labelStyle, Array.Empty<GUILayoutOption>());
		}
	}

	// =====================================================
	// 传送 Tab
	// =====================================================
	private void DrawTeleportTab()
	{
		GUILayout.Label("快速传送", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Space(5f);

		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("传送到准心位置", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			TeleportPlus.TeleportToCrosshair();
		}
		if (GUILayout.Button("传送到最近撤离点", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			TeleportPlus.TeleportToExtraction();
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(5f);

		if (GUILayout.Button("随机传送 (50m范围)", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			TeleportPlus.TeleportRandom(50f);
		}

		GUILayout.Space(15f);
		GUILayout.Label("传送点管理", sectionHeaderStyle, Array.Empty<GUILayoutOption>());
		GUILayout.Space(5f);

		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label("名称:", labelStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(40f) });
		TeleportPlus.newWaypointName = GUILayout.TextField(TeleportPlus.newWaypointName, textFieldStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(160f) });
		if (GUILayout.Button("保存当前位置", buttonStyle, Array.Empty<GUILayoutOption>()))
		{
			TeleportPlus.SaveCurrentPosition(TeleportPlus.newWaypointName);
			TeleportPlus.newWaypointName = "";
		}
		GUILayout.EndHorizontal();

		GUILayout.Space(5f);
		if (TeleportPlus.savedWaypoints.Count == 0)
		{
			GUILayout.Label("暂无保存的传送点", labelStyle, Array.Empty<GUILayoutOption>());
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
			if (GUILayout.Button("传送到选中点", buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				TeleportPlus.TeleportToWaypoint(TeleportPlus.selectedWaypointIndex);
			}
			if (GUILayout.Button("删除选中点", buttonStyle, Array.Empty<GUILayoutOption>()))
			{
				TeleportPlus.RemoveWaypoint(TeleportPlus.selectedWaypointIndex);
			}
			GUILayout.EndHorizontal();
		}
	}
}
