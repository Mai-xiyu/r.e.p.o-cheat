using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

internal static class DebugCheats
{
	public class PlayerData
	{
		public object PlayerObject { get; }

		public PhotonView PhotonView { get; }

		public Transform Transform { get; }

		public bool IsAlive { get; set; }

		public string Name { get; set; }

		public PlayerData(object player)
		{
			PlayerObject = player;
			object obj = player.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(player);
			PhotonView = obj as PhotonView;
			object obj2 = player.GetType().GetProperty("transform", BindingFlags.Instance | BindingFlags.Public)?.GetValue(player);
			Transform = obj2 as Transform;
			PlayerAvatar playerAvatar = player as PlayerAvatar;
			Name = ((playerAvatar != null) ? (SemiFunc.PlayerGetName(playerAvatar) ?? "Unknown Player") : "Unknown Player");
			IsAlive = true;
		}
	}

	public class ExtractionPointData
	{
		public ExtractionPoint ExtractionPoint { get; }

		public string CachedState { get; }

		public Vector3 CachedPosition { get; }

		public ExtractionPointData(ExtractionPoint ep, string state, Vector3 position)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			ExtractionPoint = ep;
			CachedState = state;
			CachedPosition = position;
		}
	}

	public static bool drawItemChamsBool;

	public static int minItemValue;

	public static int maxItemValue;

	public static float maxItemEspDistance;

	public static bool showEnemyBox;

	private static int frameCounter;

	public static List<Enemy> enemyList;

	public static List<object> valuableObjects;

	private static List<object> playerList;

	private static float scaleX;

	private static float scaleY;

	public static Texture2D texture2;

	private static float lastUpdateTime;

	private const float updateInterval = 5f;

	private static GameObject localPlayer;

	private static int lastPlayerDataCount;

	private static int lastEnemyCount;

	private static int lastItemCount;

	private static int lastPlayerCount;

	private static int lastExtractionPointCount;

	private static string lastLocalPlayerName;

	private static Vector3 lastExtractionPosition;

	private static List<ExtractionPointData> extractionPointList;

	public static bool drawEspBool;

	public static bool drawItemEspBool;

	public static bool draw3DItemEspBool;

	public static bool drawPlayerEspBool;

	public static bool draw2DPlayerEspBool;

	public static bool draw3DPlayerEspBool;

	public static bool drawExtractionPointEspBool;

	public static GUIStyle nameStyle;

	public static GUIStyle valueStyle;

	public static GUIStyle enemyStyle;

	public static GUIStyle healthStyle;

	public static GUIStyle distanceStyle;

	public static bool showEnemyNames;

	public static bool showEnemyDistance;

	public static bool showEnemyHP;

	public static bool showItemNames;

	public static bool showItemValue;

	public static bool showItemDistance;

	public static bool showPlayerDeathHeads;

	public static bool showExtractionNames;

	public static bool showExtractionDistance;

	public static bool showPlayerNames;

	public static bool showPlayerDistance;

	public static bool showPlayerHP;

	private static Camera cachedCamera;

	private static Material visibleMaterial;

	private static Material hiddenMaterial;

	private static Material itemVisibleMaterial;

	private static Material itemHiddenMaterial;

	private static Dictionary<Renderer, Material[]> enemyOriginalMaterials;

	public static Color enemyVisibleColor;

	public static Color enemyHiddenColor;

	public static Color itemVisibleColor;

	public static Color itemHiddenColor;

	private static Dictionary<Renderer, Material[]> itemOriginalMaterials;

	private static bool cachedOriginalCamera;

	private static float originalFarClipPlane;

	private static DepthTextureMode originalDepthTextureMode;

	private static bool originalOcclusionCulling;

	private static List<PlayerData> playerDataList;

	private static float playerUpdateInterval;

	private static Dictionary<int, int> playerHealthCache;

	private static float lastPlayerUpdateTime;

	public static Dictionary<Enemy, int> enemyHealthCache;

	private const float maxEspDistance = 100f;

	private static FieldInfo _levelAnimationStartedField;

	public static bool drawChamsBool;

	static DebugCheats()
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		drawItemChamsBool = false;
		minItemValue = 0;
		maxItemValue = 50000;
		maxItemEspDistance = 1000f;
		showEnemyBox = true;
		frameCounter = 0;
		enemyList = new List<Enemy>();
		valuableObjects = new List<object>();
		playerList = new List<object>();
		lastUpdateTime = 0f;
		lastPlayerDataCount = 0;
		lastEnemyCount = 0;
		lastItemCount = 0;
		lastPlayerCount = 0;
		lastExtractionPointCount = 0;
		lastLocalPlayerName = "";
		lastExtractionPosition = Vector3.zero;
		extractionPointList = new List<ExtractionPointData>();
		drawEspBool = false;
		drawItemEspBool = false;
		draw3DItemEspBool = false;
		drawPlayerEspBool = false;
		draw2DPlayerEspBool = false;
		draw3DPlayerEspBool = true;
		drawExtractionPointEspBool = false;
		showEnemyNames = true;
		showEnemyDistance = true;
		showEnemyHP = true;
		showItemNames = true;
		showItemValue = true;
		showItemDistance = false;
		showPlayerDeathHeads = true;
		showExtractionNames = true;
		showExtractionDistance = true;
		showPlayerNames = true;
		showPlayerDistance = true;
		showPlayerHP = true;
		enemyOriginalMaterials = new Dictionary<Renderer, Material[]>();
		enemyVisibleColor = new Color(0f, 0.5f, 0.1f, 1f);
		enemyHiddenColor = new Color(0.4f, 0.04f, 0.2f, 0.5f);
		itemVisibleColor = new Color(0.6f, 0.6f, 0f, 0.85f);
		itemHiddenColor = new Color(0.6f, 0.3f, 0f, 0.4f);
		itemOriginalMaterials = new Dictionary<Renderer, Material[]>();
		cachedOriginalCamera = false;
		originalFarClipPlane = 0f;
		originalDepthTextureMode = (DepthTextureMode)0;
		originalOcclusionCulling = false;
		playerDataList = new List<PlayerData>();
		playerUpdateInterval = 1f;
		playerHealthCache = new Dictionary<int, int>();
		lastPlayerUpdateTime = 0f;
		enemyHealthCache = new Dictionary<Enemy, int>();
		_levelAnimationStartedField = typeof(LoadingUI).GetField("levelAnimationStarted", BindingFlags.Instance | BindingFlags.NonPublic);
		drawChamsBool = false;
		cachedCamera = Camera.main;
		if ((Object)(object)cachedCamera != (Object)null)
		{
			scaleX = (float)Screen.width / (float)cachedCamera.pixelWidth;
			scaleY = (float)Screen.height / (float)cachedCamera.pixelHeight;
		}
		UpdateLists();
		UpdateLocalPlayer();
		UpdateExtractionPointList();
		UpdatePlayerDataList();
	}

	private static void UpdatePlayerDataList()
	{
		playerDataList.Clear();
		playerHealthCache.Clear();
		List<PlayerAvatar> list = SemiFunc.PlayerGetList();
		if (list != null)
		{
			foreach (PlayerAvatar item in list)
			{
				if ((Object)(object)item != (Object)null)
				{
					PlayerData playerData = new PlayerData(item);
					if ((Object)(object)playerData.PhotonView != (Object)null && (Object)(object)playerData.Transform != (Object)null)
					{
						playerDataList.Add(playerData);
						int playerHealth = Players.GetPlayerHealth(item);
						playerHealthCache[playerData.PhotonView.ViewID] = playerHealth;
					}
				}
			}
		}
		if (playerDataList.Count != lastPlayerDataCount)
		{
			lastPlayerDataCount = playerDataList.Count;
		}
	}

	private static void UpdateExtractionPointList()
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		extractionPointList.Clear();
		Object[] array = Object.FindObjectsOfType(Type.GetType("ExtractionPoint, Assembly-CSharp"));
		if (array == null)
		{
			return;
		}
		Object[] array2 = array;
		foreach (Object obj in array2)
		{
			ExtractionPoint val = (ExtractionPoint)(object)((obj is ExtractionPoint) ? obj : null);
			if ((Object)(object)val != (Object)null && ((Component)val).gameObject.activeInHierarchy)
			{
				FieldInfo field = ((object)val).GetType().GetField("currentState", BindingFlags.Instance | BindingFlags.NonPublic);
				string state = "Unknown";
				if (field != null)
				{
					state = field.GetValue(val)?.ToString() ?? "Unknown";
				}
				Vector3 position = ((Component)val).transform.position;
				extractionPointList.Add(new ExtractionPointData(val, state, position));
				if (Vector3.Distance(position, lastExtractionPosition) > 0.1f)
				{
					lastExtractionPosition = position;
				}
			}
		}
	}

	private static void UpdateLists()
	{
		UpdateExtractionPointList();
		if (extractionPointList.Count != lastExtractionPointCount)
		{
			lastExtractionPointCount = extractionPointList.Count;
		}
		enemyList.Clear();
		enemyHealthCache.Clear();
		Type type = Type.GetType("EnemyDirector, Assembly-CSharp");
		if (type != null)
		{
			object obj = type.GetField("instance", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
			if (obj != null)
			{
				FieldInfo field = type.GetField("enemiesSpawned", BindingFlags.Instance | BindingFlags.Public);
				if (field != null && field.GetValue(obj) is IEnumerable<object> enumerable)
				{
					foreach (object item in enumerable)
					{
						if (item == null)
						{
							continue;
						}
						FieldInfo fieldInfo = item.GetType().GetField("enemyInstance", BindingFlags.Instance | BindingFlags.NonPublic) ?? item.GetType().GetField("Enemy", BindingFlags.Instance | BindingFlags.NonPublic) ?? item.GetType().GetField("childEnemy", BindingFlags.Instance | BindingFlags.NonPublic);
						if (fieldInfo != null)
						{
							object value = fieldInfo.GetValue(item);
							Enemy val = (Enemy)((value is Enemy) ? value : null);
							if ((Object)(object)val != (Object)null && (Object)(object)((Component)val).gameObject != (Object)null && ((Component)val).gameObject.activeInHierarchy)
							{
								int enemyHealth = Enemies.GetEnemyHealth(val);
								enemyHealthCache[val] = enemyHealth;
								enemyList.Add(val);
							}
						}
					}
				}
			}
		}
		playerList.Clear();
		List<PlayerAvatar> list = SemiFunc.PlayerGetList();
		if (list != null)
		{
			foreach (PlayerAvatar item2 in list)
			{
				if ((Object)(object)item2 != (Object)null)
				{
					playerList.Add(item2);
				}
			}
		}
		lastUpdateTime = Time.time;
		if (enemyList.Count != lastEnemyCount || valuableObjects.Count != lastItemCount || playerList.Count != lastPlayerCount)
		{
			lastEnemyCount = enemyList.Count;
			lastItemCount = valuableObjects.Count;
			lastPlayerCount = playerList.Count;
		}
	}

	private static void UpdateLocalPlayer()
	{
		GameObject val = GetLocalPlayer();
		string text = (((Object)(object)val != (Object)null) ? ((Object)val).name : "null");
		if ((Object)(object)val != (Object)(object)localPlayer || text != lastLocalPlayerName)
		{
			_ = (Object)(object)val != (Object)null;
			lastLocalPlayerName = text;
		}
		localPlayer = val;
	}

	public static bool IsLocalPlayer(object player)
	{
		try
		{
			if ((Object)(object)localPlayer == (Object)null)
			{
				UpdateLocalPlayer();
				if ((Object)(object)localPlayer == (Object)null)
				{
					return false;
				}
			}
			GameObject val = (GameObject)((player is GameObject) ? player : null);
			if (val != null)
			{
				return (Object)(object)val == (Object)(object)localPlayer;
			}
			MonoBehaviour val2 = (MonoBehaviour)((player is MonoBehaviour) ? player : null);
			if (val2 != null)
			{
				return (Object)(object)((Component)val2).gameObject == (Object)(object)localPlayer;
			}
			PropertyInfo property = player.GetType().GetProperty("gameObject");
			if (property != null)
			{
				object value = property.GetValue(player);
				return (Object)((value is GameObject) ? value : null) == (Object)(object)localPlayer;
			}
			return false;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static GameObject GetLocalPlayer()
	{
		if (PhotonNetwork.IsConnected)
		{
			List<PlayerAvatar> list = SemiFunc.PlayerGetList();
			if (list != null)
			{
				foreach (PlayerAvatar item in list)
				{
					FieldInfo field = ((object)item).GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (!(field != null))
					{
						continue;
					}
					object value = field.GetValue(item);
					PhotonView val = (PhotonView)((value is PhotonView) ? value : null);
					if (!((Object)(object)val != (Object)null) || !val.IsMine)
					{
						continue;
					}
					PropertyInfo property = ((object)item).GetType().GetProperty("gameObject", BindingFlags.Instance | BindingFlags.Public);
					if (property != null)
					{
						object value2 = property.GetValue(item);
						object obj = ((value2 is GameObject) ? value2 : null);
						string name = ((Object)obj).name;
						if (name != lastLocalPlayerName)
						{
							lastLocalPlayerName = name;
						}
						return (GameObject)obj;
					}
					string name2 = ((Object)((Component)val).gameObject).name;
					if (name2 != lastLocalPlayerName)
					{
						lastLocalPlayerName = name2;
					}
					return ((Component)val).gameObject;
				}
			}
			if (PhotonNetwork.LocalPlayer != null)
			{
				PhotonView[] array = Object.FindObjectsOfType<PhotonView>();
				foreach (PhotonView val2 in array)
				{
					if (val2.Owner == PhotonNetwork.LocalPlayer && val2.IsMine)
					{
						string name3 = ((Object)((Component)val2).gameObject).name;
						if (name3 != lastLocalPlayerName)
						{
							lastLocalPlayerName = name3;
						}
						return ((Component)val2).gameObject;
					}
				}
			}
			return null;
		}
		List<PlayerAvatar> list2 = SemiFunc.PlayerGetList();
		if (list2 != null && list2.Count > 0)
		{
			PlayerAvatar val3 = list2[0];
			PropertyInfo property2 = ((object)val3).GetType().GetProperty("gameObject", BindingFlags.Instance | BindingFlags.Public);
			if (property2 != null)
			{
				object value3 = property2.GetValue(val3);
				object obj2 = ((value3 is GameObject) ? value3 : null);
				string name4 = ((Object)obj2).name;
				if (name4 != lastLocalPlayerName)
				{
					lastLocalPlayerName = name4;
				}
				return (GameObject)obj2;
			}
		}
		Type type = Type.GetType("PlayerAvatar, Assembly-CSharp");
		if (type != null)
		{
			Object obj3 = Object.FindObjectOfType(type);
			MonoBehaviour val4 = (MonoBehaviour)(object)((obj3 is MonoBehaviour) ? obj3 : null);
			if ((Object)(object)val4 != (Object)null)
			{
				string name5 = ((Object)((Component)val4).gameObject).name;
				if (name5 != lastLocalPlayerName)
				{
					lastLocalPlayerName = name5;
				}
				return ((Component)val4).gameObject;
			}
		}
		GameObject val5 = GameObject.FindWithTag("Player");
		if ((Object)(object)val5 != (Object)null)
		{
			string name6 = ((Object)val5).name;
			if (name6 != lastLocalPlayerName)
			{
				lastLocalPlayerName = name6;
			}
			return val5;
		}
		GameObject[] array2 = Object.FindObjectsOfType<GameObject>();
		foreach (GameObject val6 in array2)
		{
			if (((Object)val6).name.Contains("Player") && val6.activeInHierarchy)
			{
				string name7 = ((Object)val6).name;
				if (name7 != lastLocalPlayerName)
				{
					lastLocalPlayerName = name7;
				}
				return val6;
			}
		}
		return null;
	}

	public static void UpdateEnemyList()
	{
		enemyList.Clear();
		enemyHealthCache.Clear();
		Type type = Type.GetType("EnemyDirector, Assembly-CSharp");
		if (!(type != null))
		{
			return;
		}
		object obj = type.GetField("instance", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
		if (obj == null)
		{
			return;
		}
		FieldInfo field = type.GetField("enemiesSpawned", BindingFlags.Instance | BindingFlags.Public);
		if (!(field != null) || !(field.GetValue(obj) is IEnumerable<object> enumerable))
		{
			return;
		}
		foreach (object item in enumerable)
		{
			if (item == null)
			{
				continue;
			}
			FieldInfo fieldInfo = item.GetType().GetField("enemyInstance", BindingFlags.Instance | BindingFlags.NonPublic) ?? item.GetType().GetField("Enemy", BindingFlags.Instance | BindingFlags.NonPublic) ?? item.GetType().GetField("childEnemy", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fieldInfo != null)
			{
				object value = fieldInfo.GetValue(item);
				Enemy val = (Enemy)((value is Enemy) ? value : null);
				if ((Object)(object)val != (Object)null && (Object)(object)((Component)val).gameObject != (Object)null && ((Component)val).gameObject.activeInHierarchy)
				{
					int enemyHealth = Enemies.GetEnemyHealth(val);
					enemyHealthCache[val] = enemyHealth;
					enemyList.Add(val);
				}
			}
		}
	}

	public static void RectFilled(float x, float y, float width, float height, Texture2D text)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		GUI.DrawTexture(new Rect(x, y, width, height), (Texture)(object)text);
	}

	public static void RectOutlined(float x, float y, float width, float height, Texture2D text, float thickness = 1f)
	{
		RectFilled(x, y, thickness, height, text);
		RectFilled(x + width - thickness, y, thickness, height, text);
		RectFilled(x + thickness, y, width - thickness * 2f, thickness, text);
		RectFilled(x + thickness, y + height - thickness, width - thickness * 2f, thickness, text);
	}

	public static void Box(float x, float y, float width, float height, Texture2D text, float thickness = 2f)
	{
		RectOutlined(x - width / 2f, y - height, width, height, text, thickness);
	}

	public static void InitializeStyles()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_0057: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Expected O, but got Unknown
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Expected O, but got Unknown
		if (nameStyle == null)
		{
			GUIStyle val = new GUIStyle(GUI.skin.label);
			val.normal.textColor = Color.yellow;
			val.alignment = (TextAnchor)4;
			val.fontSize = 14;
			val.fontStyle = (FontStyle)1;
			val.wordWrap = true;
			val.border = new RectOffset(1, 1, 1, 1);
			nameStyle = val;
		}
		if (valueStyle == null)
		{
			GUIStyle val2 = new GUIStyle(GUI.skin.label);
			val2.normal.textColor = Color.green;
			val2.alignment = (TextAnchor)4;
			val2.fontSize = 12;
			val2.fontStyle = (FontStyle)1;
			valueStyle = val2;
		}
		if (enemyStyle == null)
		{
			enemyStyle = new GUIStyle(GUI.skin.label)
			{
				alignment = (TextAnchor)4,
				wordWrap = true,
				fontSize = 12,
				fontStyle = (FontStyle)1
			};
		}
		if (healthStyle == null)
		{
			GUIStyle val3 = new GUIStyle(GUI.skin.label);
			val3.normal.textColor = Color.green;
			val3.alignment = (TextAnchor)4;
			val3.fontSize = 12;
			val3.fontStyle = (FontStyle)1;
			healthStyle = val3;
		}
		if (distanceStyle == null)
		{
			GUIStyle val4 = new GUIStyle(GUI.skin.label);
			val4.normal.textColor = Color.yellow;
			val4.alignment = (TextAnchor)4;
			val4.fontSize = 12;
			val4.fontStyle = (FontStyle)1;
			distanceStyle = val4;
		}
	}

	private static void CreateBoundsEdges(Bounds bounds, Color color)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = (Vector3[])(object)new Vector3[8];
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		array[0] = new Vector3(min.x, min.y, min.z);
		array[1] = new Vector3(max.x, min.y, min.z);
		array[2] = new Vector3(max.x, min.y, max.z);
		array[3] = new Vector3(min.x, min.y, max.z);
		array[4] = new Vector3(min.x, max.y, min.z);
		array[5] = new Vector3(max.x, max.y, min.z);
		array[6] = new Vector3(max.x, max.y, max.z);
		array[7] = new Vector3(min.x, max.y, max.z);
		Vector2[] array2 = (Vector2[])(object)new Vector2[8];
		bool flag = false;
		for (int i = 0; i < 8; i++)
		{
			Vector3 val = cachedCamera.WorldToScreenPoint(array[i]);
			if (val.z > 0f)
			{
				flag = true;
			}
			array2[i] = new Vector2(val.x * scaleX, (float)Screen.height - val.y * scaleY);
		}
		if (flag)
		{
			DrawLine(array2[0], array2[1], color);
			DrawLine(array2[1], array2[2], color);
			DrawLine(array2[2], array2[3], color);
			DrawLine(array2[3], array2[0], color);
			DrawLine(array2[4], array2[5], color);
			DrawLine(array2[5], array2[6], color);
			DrawLine(array2[6], array2[7], color);
			DrawLine(array2[7], array2[4], color);
			DrawLine(array2[0], array2[4], color);
			DrawLine(array2[1], array2[5], color);
			DrawLine(array2[2], array2[6], color);
			DrawLine(array2[3], array2[7], color);
		}
	}

	private static void DrawLine(Vector2 start, Vector2 end, Color color)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)texture2 == (Object)null))
		{
			float num = Vector2.Distance(start, end);
			float num2 = Mathf.Atan2(end.y - start.y, end.x - start.x) * 57.29578f;
			GUI.color = color;
			Matrix4x4 matrix = GUI.matrix;
			GUIUtility.RotateAroundPivot(num2, start);
			GUI.DrawTexture(new Rect(start.x, start.y, num, 1f), (Texture)(object)texture2);
			GUI.matrix = matrix;
			GUI.color = Color.white;
		}
	}

	private static Bounds GetActiveColliderBounds(GameObject obj)
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		Collider[] componentsInChildren = obj.GetComponentsInChildren<Collider>(true);
		List<Collider> list = new List<Collider>();
		Collider[] array = componentsInChildren;
		foreach (Collider val in array)
		{
			if (val.enabled && ((Component)val).gameObject.activeInHierarchy)
			{
				list.Add(val);
			}
		}
		if (list.Count == 0)
		{
			Renderer[] componentsInChildren2 = obj.GetComponentsInChildren<Renderer>(true);
			if (componentsInChildren2.Length != 0)
			{
				Bounds bounds = componentsInChildren2[0].bounds;
				for (int j = 1; j < componentsInChildren2.Length; j++)
				{
					if (componentsInChildren2[j].enabled && ((Component)componentsInChildren2[j]).gameObject.activeInHierarchy)
					{
						bounds.Encapsulate(componentsInChildren2[j].bounds);
					}
				}
				return bounds;
			}
			return new Bounds(obj.transform.position, Vector3.one * 0.5f);
		}
		Bounds bounds2 = list[0].bounds;
		for (int k = 1; k < list.Count; k++)
		{
			bounds2.Encapsulate(list[k].bounds);
		}
		bounds2.Expand(0.1f);
		return bounds2;
	}

	public static void DrawESP()
	{
		//IL_0eed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ef4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ef6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f07: Unknown result type (might be due to invalid IL or missing references)
		//IL_113a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f18: Unknown result type (might be due to invalid IL or missing references)
		//IL_113f: Unknown result type (might be due to invalid IL or missing references)
		//IL_114b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f2a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0733: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f3b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1177: Unknown result type (might be due to invalid IL or missing references)
		//IL_117c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f4d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f62: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f7c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f81: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fb1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f98: Unknown result type (might be due to invalid IL or missing references)
		//IL_11d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_11e5: Expected O, but got Unknown
		//IL_11ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_11fa: Expected O, but got Unknown
		//IL_1234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e38: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e3d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1348: Unknown result type (might be due to invalid IL or missing references)
		//IL_134d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1352: Unknown result type (might be due to invalid IL or missing references)
		//IL_1354: Unknown result type (might be due to invalid IL or missing references)
		//IL_12a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_128f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1294: Unknown result type (might be due to invalid IL or missing references)
		//IL_1261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0947: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e56: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e5b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1365: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Expected O, but got Unknown
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Expected O, but got Unknown
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0886: Unknown result type (might be due to invalid IL or missing references)
		//IL_088b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1376: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_15fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1600: Unknown result type (might be due to invalid IL or missing references)
		//IL_1388: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Expected O, but got Unknown
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Expected O, but got Unknown
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1620: Unknown result type (might be due to invalid IL or missing references)
		//IL_1625: Unknown result type (might be due to invalid IL or missing references)
		//IL_1399: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_13ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_13c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_163a: Unknown result type (might be due to invalid IL or missing references)
		//IL_163c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1645: Unknown result type (might be due to invalid IL or missing references)
		//IL_1647: Unknown result type (might be due to invalid IL or missing references)
		//IL_164e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1653: Unknown result type (might be due to invalid IL or missing references)
		//IL_1658: Unknown result type (might be due to invalid IL or missing references)
		//IL_165f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1661: Unknown result type (might be due to invalid IL or missing references)
		//IL_1666: Unknown result type (might be due to invalid IL or missing references)
		//IL_166d: Unknown result type (might be due to invalid IL or missing references)
		//IL_166f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1674: Unknown result type (might be due to invalid IL or missing references)
		//IL_1676: Unknown result type (might be due to invalid IL or missing references)
		//IL_1684: Unknown result type (might be due to invalid IL or missing references)
		//IL_143e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1443: Unknown result type (might be due to invalid IL or missing references)
		//IL_1417: Unknown result type (might be due to invalid IL or missing references)
		//IL_141e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a06: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a0b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a10: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a17: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a19: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a1e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a25: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a27: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a2c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a2e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1483: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a3f: Unknown result type (might be due to invalid IL or missing references)
		//IL_169a: Unknown result type (might be due to invalid IL or missing references)
		//IL_16af: Unknown result type (might be due to invalid IL or missing references)
		//IL_16c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_16e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_147c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1475: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a50: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a65: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a79: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a9b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aab: Unknown result type (might be due to invalid IL or missing references)
		//IL_1749: Unknown result type (might be due to invalid IL or missing references)
		//IL_174e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1780: Unknown result type (might be due to invalid IL or missing references)
		//IL_1785: Unknown result type (might be due to invalid IL or missing references)
		//IL_1791: Unknown result type (might be due to invalid IL or missing references)
		//IL_14cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_14d7: Expected O, but got Unknown
		//IL_1521: Unknown result type (might be due to invalid IL or missing references)
		//IL_1506: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c9e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0caa: Expected O, but got Unknown
		//IL_0c4b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c57: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cd4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ce0: Expected O, but got Unknown
		//IL_0d16: Unknown result type (might be due to invalid IL or missing references)
		//IL_184c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1858: Expected O, but got Unknown
		//IL_1861: Unknown result type (might be due to invalid IL or missing references)
		//IL_186d: Expected O, but got Unknown
		//IL_0c1c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c15: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c0e: Unknown result type (might be due to invalid IL or missing references)
		//IL_18b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_18fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_18df: Unknown result type (might be due to invalid IL or missing references)
		bool flag = _levelAnimationStartedField != null && (bool)_levelAnimationStartedField.GetValue(LoadingUI.instance);
		if (((Object)(object)RunManager.instance.levelCurrent != (Object)null && !Hax2.levelsToSearchItems.Contains(((Object)RunManager.instance.levelCurrent).name)) || (!Hax2.levelsToSearchItems.Contains(((Object)RunManager.instance.levelCurrent).name) && flag))
		{
			return;
		}
		InitializeStyles();
		if (!drawEspBool && !drawItemEspBool && !drawExtractionPointEspBool && !drawPlayerEspBool && !draw2DPlayerEspBool && !draw3DPlayerEspBool && !draw3DItemEspBool && !drawChamsBool)
		{
			return;
		}
		if ((Object)(object)localPlayer == (Object)null)
		{
			UpdateLocalPlayer();
		}
		float time = Time.time;
		if (time - lastPlayerUpdateTime > playerUpdateInterval)
		{
			UpdatePlayerDataList();
			lastPlayerUpdateTime = time;
		}
		if (time - lastUpdateTime > 5f)
		{
			if (drawEspBool || drawItemEspBool || drawExtractionPointEspBool || drawPlayerEspBool || draw2DPlayerEspBool || draw3DPlayerEspBool || draw3DItemEspBool)
			{
				UpdateLists();
			}
			lastUpdateTime = time;
		}
		if ((Object)(object)cachedCamera == (Object)null || (Object)(object)cachedCamera != (Object)(object)Camera.main)
		{
			cachedCamera = Camera.main;
			if ((Object)(object)cachedCamera == (Object)null)
			{
				return;
			}
		}
		scaleX = (float)Screen.width / (float)cachedCamera.pixelWidth;
		scaleY = (float)Screen.height / (float)cachedCamera.pixelHeight;
		if ((Object)(object)visibleMaterial == (Object)null || (Object)(object)hiddenMaterial == (Object)null)
		{
			Shader val = Shader.Find("Hidden/Internal-Colored");
			if ((Object)(object)val != (Object)null)
			{
				hiddenMaterial = new Material(val);
				hiddenMaterial.SetInt("_SrcBlend", 5);
				hiddenMaterial.SetInt("_DstBlend", 10);
				hiddenMaterial.SetInt("_Cull", 0);
				hiddenMaterial.SetInt("_ZTest", 8);
				hiddenMaterial.SetInt("_ZWrite", 0);
				hiddenMaterial.SetColor("_Color", enemyHiddenColor);
				hiddenMaterial.renderQueue = 4000;
				hiddenMaterial.globalIlluminationFlags = (MaterialGlobalIlluminationFlags)1;
				hiddenMaterial.EnableKeyword("_EMISSION");
				visibleMaterial = new Material(val);
				visibleMaterial.SetInt("_SrcBlend", 5);
				visibleMaterial.SetInt("_DstBlend", 10);
				visibleMaterial.SetInt("_Cull", 0);
				visibleMaterial.SetInt("_ZTest", 4);
				visibleMaterial.SetInt("_ZWrite", 0);
				visibleMaterial.SetColor("_Color", enemyVisibleColor);
				visibleMaterial.renderQueue = 4001;
				visibleMaterial.globalIlluminationFlags = (MaterialGlobalIlluminationFlags)1;
				visibleMaterial.EnableKeyword("_EMISSION");
			}
		}
		else
		{
			if ((Object)(object)hiddenMaterial != (Object)null)
			{
				hiddenMaterial.SetColor("_Color", enemyHiddenColor);
			}
			if ((Object)(object)visibleMaterial != (Object)null)
			{
				visibleMaterial.SetColor("_Color", enemyVisibleColor);
			}
		}
		if ((Object)(object)itemVisibleMaterial == (Object)null || (Object)(object)itemHiddenMaterial == (Object)null)
		{
			Shader val2 = Shader.Find("Hidden/Internal-Colored");
			if ((Object)(object)val2 != (Object)null)
			{
				itemHiddenMaterial = new Material(val2);
				itemHiddenMaterial.SetInt("_SrcBlend", 5);
				itemHiddenMaterial.SetInt("_DstBlend", 10);
				itemHiddenMaterial.SetInt("_Cull", 0);
				itemHiddenMaterial.SetInt("_ZTest", 8);
				itemHiddenMaterial.SetInt("_ZWrite", 0);
				itemHiddenMaterial.SetColor("_Color", itemHiddenColor);
				itemHiddenMaterial.renderQueue = 4000;
				itemVisibleMaterial = new Material(val2);
				itemVisibleMaterial.SetInt("_SrcBlend", 5);
				itemVisibleMaterial.SetInt("_DstBlend", 10);
				itemVisibleMaterial.SetInt("_Cull", 0);
				itemVisibleMaterial.SetInt("_ZTest", 4);
				itemVisibleMaterial.SetInt("_ZWrite", 0);
				itemVisibleMaterial.SetColor("_Color", itemVisibleColor);
				itemVisibleMaterial.renderQueue = 4001;
			}
		}
		else
		{
			if ((Object)(object)itemHiddenMaterial != (Object)null)
			{
				itemHiddenMaterial.SetColor("_Color", itemHiddenColor);
			}
			if ((Object)(object)itemVisibleMaterial != (Object)null)
			{
				itemVisibleMaterial.SetColor("_Color", itemVisibleColor);
			}
		}
		if (drawEspBool && drawChamsBool)
		{
			Camera main = Camera.main;
			if ((Object)(object)main != (Object)null)
			{
				if (!cachedOriginalCamera)
				{
					originalFarClipPlane = main.farClipPlane;
					originalDepthTextureMode = main.depthTextureMode;
					originalOcclusionCulling = main.useOcclusionCulling;
					cachedOriginalCamera = true;
				}
				main.farClipPlane = 500f;
				main.depthTextureMode = (DepthTextureMode)0;
				main.useOcclusionCulling = false;
			}
			enemyList.RemoveAll((Enemy e) => (Object)(object)e == (Object)null || !((Component)e).gameObject.activeInHierarchy || (Object)(object)e.CenterTransform == (Object)null);
			foreach (Enemy enemy in enemyList)
			{
				if ((Object)(object)enemy == (Object)null || !((Component)enemy).gameObject.activeInHierarchy || (Object)(object)enemy.CenterTransform == (Object)null)
				{
					continue;
				}
				Component componentInParent = ((Component)enemy).GetComponentInParent(Type.GetType("EnemyParent, Assembly-CSharp"));
				if ((Object)(object)componentInParent == (Object)null)
				{
					continue;
				}
				Renderer[] componentsInChildren = componentInParent.GetComponentsInChildren<Renderer>(true);
				foreach (Renderer val3 in componentsInChildren)
				{
					if ((Object)(object)val3 == (Object)null || !((Component)val3).gameObject.activeInHierarchy)
					{
						continue;
					}
					if (!enemyOriginalMaterials.ContainsKey(val3))
					{
						try
						{
							enemyOriginalMaterials[val3] = val3.materials;
						}
						catch
						{
							continue;
						}
					}
					try
					{
						val3.materials = (Material[])(object)new Material[2] { hiddenMaterial, visibleMaterial };
					}
					catch (Exception ex)
					{
						Debug.LogWarning((object)("[EnemyChams] Failed to apply chams to " + ((Object)val3).name + ": " + ex.Message));
					}
				}
			}
		}
		else
		{
			foreach (KeyValuePair<Renderer, Material[]> item in enemyOriginalMaterials.ToList())
			{
				if ((Object)(object)item.Key == (Object)null)
				{
					enemyOriginalMaterials.Remove(item.Key);
					continue;
				}
				try
				{
					item.Key.materials = item.Value;
				}
				catch (Exception ex2)
				{
					Debug.LogWarning((object)("[EnemyChams] Failed to restore materials: " + ex2.Message));
				}
			}
			enemyOriginalMaterials.Clear();
			if (cachedOriginalCamera)
			{
				Camera main2 = Camera.main;
				if ((Object)(object)main2 != (Object)null)
				{
					main2.farClipPlane = originalFarClipPlane;
					main2.depthTextureMode = originalDepthTextureMode;
					main2.useOcclusionCulling = originalOcclusionCulling;
				}
				cachedOriginalCamera = false;
			}
		}
		if (drawItemEspBool && drawItemChamsBool)
		{
			foreach (object valuableObject in valuableObjects)
			{
				if (valuableObject == null)
				{
					continue;
				}
				object obj2 = valuableObject.GetType().GetProperty("transform", BindingFlags.Instance | BindingFlags.Public)?.GetValue(valuableObject);
				Transform val4 = (Transform)((obj2 is Transform) ? obj2 : null);
				if ((Object)(object)val4 == (Object)null || !((Component)val4).gameObject.activeInHierarchy)
				{
					continue;
				}
				Renderer[] componentsInChildren = ((Component)val4).GetComponentsInChildren<Renderer>(true);
				foreach (Renderer val5 in componentsInChildren)
				{
					if (!((Object)(object)val5 == (Object)null) && ((Component)val5).gameObject.activeInHierarchy)
					{
						if (!itemOriginalMaterials.ContainsKey(val5))
						{
							itemOriginalMaterials[val5] = val5.materials;
						}
						val5.materials = (Material[])(object)new Material[2] { itemHiddenMaterial, itemVisibleMaterial };
					}
				}
			}
			Camera main3 = Camera.main;
			if ((Object)(object)main3 != (Object)null)
			{
				if (!cachedOriginalCamera)
				{
					originalFarClipPlane = main3.farClipPlane;
					originalDepthTextureMode = main3.depthTextureMode;
					originalOcclusionCulling = main3.useOcclusionCulling;
					cachedOriginalCamera = true;
				}
				main3.farClipPlane = 500f;
				main3.depthTextureMode = (DepthTextureMode)0;
				main3.useOcclusionCulling = false;
			}
		}
		else
		{
			foreach (KeyValuePair<Renderer, Material[]> itemOriginalMaterial in itemOriginalMaterials)
			{
				if ((Object)(object)itemOriginalMaterial.Key != (Object)null)
				{
					itemOriginalMaterial.Key.materials = itemOriginalMaterial.Value;
				}
			}
			itemOriginalMaterials.Clear();
			if (cachedOriginalCamera && (Object)(object)cachedCamera != (Object)null)
			{
				cachedCamera.farClipPlane = originalFarClipPlane;
				cachedCamera.depthTextureMode = originalDepthTextureMode;
				cachedCamera.useOcclusionCulling = originalOcclusionCulling;
			}
		}
		if (drawEspBool)
		{
			enemyList.RemoveAll(delegate(Enemy enemyInstance)
			{
				try
				{
					return (Object)(object)enemyInstance == (Object)null || (Object)(object)((Component)enemyInstance).gameObject == (Object)null || (Object)(object)enemyInstance.CenterTransform == (Object)null;
				}
				catch
				{
					return true;
				}
			});
			foreach (Enemy enemy2 in enemyList)
			{
				if ((Object)(object)enemy2 == (Object)null || !((Component)enemy2).gameObject.activeInHierarchy || (Object)(object)enemy2.CenterTransform == (Object)null)
				{
					continue;
				}
				Vector3 position = ((Component)enemy2).transform.position;
				float num2 = 2f;
				Vector3 val6 = ((Component)enemy2).transform.position + Vector3.up * num2;
				Vector3 val7 = cachedCamera.WorldToScreenPoint(position);
				Vector3 val8 = cachedCamera.WorldToScreenPoint(val6);
				if (val7.z > 0f && val8.z > 0f)
				{
					float num3 = val7.x * scaleX;
					float num4 = (float)Screen.height - val7.y * scaleY;
					float num5 = (float)Screen.height - val8.y * scaleY;
					float num6 = Mathf.Abs(num4 - num5);
					float num7 = ((Component)enemy2).transform.localScale.y * 200f;
					float z = val7.z;
					float num8 = num7 / z * scaleX;
					num8 = Mathf.Clamp(num8, 30f, num6 * 1.2f);
					num6 = Mathf.Clamp(num6, 40f, 400f);
					float num9 = num3;
					float num10 = num4;
					if (showEnemyBox)
					{
						Box(num9, num10, num8, num6, texture2, 1f);
					}
					float num11 = 200f;
					float num12 = num9 - num11 / 2f;
					Component componentInParent2 = ((Component)enemy2).GetComponentInParent(Type.GetType("EnemyParent, Assembly-CSharp"));
					string text = "敌人";
					if ((Object)(object)componentInParent2 != (Object)null)
					{
						text = (((object)componentInParent2).GetType().GetField("enemyName", BindingFlags.Instance | BindingFlags.Public)?.GetValue(componentInParent2) as string) ?? "敌人";
					}
					string text2 = "";
					if (showEnemyHP && enemyHealthCache.ContainsKey(enemy2))
					{
						int enemyHealth = Enemies.GetEnemyHealth(enemy2);
						enemyHealthCache[enemy2] = enemyHealth;
						int enemyMaxHealth = Enemies.GetEnemyMaxHealth(enemy2);
						float num13 = ((enemyMaxHealth > 0) ? ((float)enemyHealth / (float)enemyMaxHealth) : 0f);
						text2 = ((enemyHealth >= 0) ? $" HP: {enemyHealth}/{enemyMaxHealth}" : "");
						healthStyle.normal.textColor = ((num13 > 0.66f) ? Color.green : ((num13 > 0.33f) ? Color.yellow : Color.red));
					}
					string text3 = "";
					if (showEnemyDistance && (Object)(object)localPlayer != (Object)null)
					{
						float num14 = Vector3.Distance(localPlayer.transform.position, ((Component)enemy2).transform.position);
						text3 = $" [{num14:F1}m]";
					}
					string text4 = "";
					if (showEnemyNames)
					{
						text4 = (text = text + " " + text3);
					}
					float num15 = enemyStyle.CalcHeight(new GUIContent(text4), num11);
					float num16 = num10 - num6 - num15;
					float num17 = 0f;
					if (showEnemyHP && !string.IsNullOrEmpty(text2))
					{
						num17 = healthStyle.CalcHeight(new GUIContent(text2), num11);
					}
					GUI.Label(new Rect(num12, num16, num11, num15), text4, enemyStyle);
					if (showEnemyHP && !string.IsNullOrEmpty(text2))
					{
						GUI.Label(new Rect(num12, num16 - num17, num11, num17), text2, healthStyle);
					}
				}
			}
		}
		if (drawItemEspBool)
		{
			valuableObjects.RemoveAll(delegate(object item)
			{
				try
				{
					int result;
					if (item != null)
					{
						Object val16 = (Object)((item is Object) ? item : null);
						result = ((val16 != null && val16 == (Object)null) ? 1 : 0);
					}
					else
					{
						result = 1;
					}
					return (byte)result != 0;
				}
				catch
				{
					return true;
				}
			});
			foreach (object valuableObject2 in valuableObjects)
			{
				if (valuableObject2 == null)
				{
					continue;
				}
				bool flag2 = valuableObject2.GetType().Name == "PlayerDeathHead";
				if (!showPlayerDeathHeads && flag2)
				{
					continue;
				}
				Transform val9 = null;
				try
				{
					object obj3 = valuableObject2?.GetType()?.GetProperty("transform", BindingFlags.Instance | BindingFlags.Public)?.GetValue(valuableObject2);
					val9 = (Transform)((obj3 is Transform) ? obj3 : null);
				}
				catch (Exception ex3)
				{
					Debug.LogWarning((object)("[ESP] Failed to get transform from valuableObject: " + ex3.Message));
					continue;
				}
				if ((Object)(object)val9 == (Object)null || !((Component)val9).gameObject.activeInHierarchy)
				{
					continue;
				}
				Vector3 position2 = val9.position;
				if ((Object)(object)localPlayer != (Object)null && Vector3.Distance(localPlayer.transform.position, position2) > maxItemEspDistance)
				{
					continue;
				}
				if (!flag2 && showItemValue)
				{
					FieldInfo fieldInfo = valuableObject2.GetType().GetField("dollarValueCurrent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? valuableObject2.GetType().GetField("dollarValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (fieldInfo != null)
					{
						try
						{
							if (Convert.ToInt32(fieldInfo.GetValue(valuableObject2)) < minItemValue)
							{
								continue;
							}
						}
						catch (Exception ex4)
						{
							Debug.Log((object)("Error reading 'dollarValueCurrent': " + ex4.Message + ". Defaulting to 0."));
						}
					}
				}
				Vector3 val10 = cachedCamera.WorldToScreenPoint(position2);
				if (!(val10.z > 0f) || !(val10.x > 0f) || !(val10.x < (float)Screen.width) || !(val10.y > 0f) || !(val10.y < (float)Screen.height))
				{
					continue;
				}
				float num18 = val10.x * scaleX;
				float num19 = (float)Screen.height - val10.y * scaleY;
				Color textColor = nameStyle.normal.textColor;
				string text5;
				if (flag2)
				{
					text5 = "死亡玩家头颅";
					nameStyle.normal.textColor = Color.red;
				}
				else
				{
					nameStyle.normal.textColor = Color.yellow;
					try
					{
						text5 = valuableObject2.GetType().GetProperty("name", BindingFlags.Instance | BindingFlags.Public)?.GetValue(valuableObject2) as string;
						if (string.IsNullOrEmpty(text5))
						{
							object obj4 = ((valuableObject2 is Object) ? valuableObject2 : null);
							text5 = ((obj4 != null) ? ((Object)obj4).name : null) ?? "未知";
						}
					}
					catch (Exception ex5)
					{
						object obj5 = ((valuableObject2 is Object) ? valuableObject2 : null);
						text5 = ((obj5 != null) ? ((Object)obj5).name : null) ?? "未知";
						Debug.Log((object)("Error accessing item 'name': " + ex5.Message + ". Using GameObject name: " + text5));
					}
					if (text5.StartsWith("Valuable", StringComparison.OrdinalIgnoreCase))
					{
						text5 = text5.Substring("Valuable".Length).Trim();
					}
					if (text5.EndsWith("(Clone)", StringComparison.OrdinalIgnoreCase))
					{
						text5 = text5.Substring(0, text5.Length - "(Clone)".Length).Trim();
					}
				}
				int num20 = 0;
				if (!flag2)
				{
					FieldInfo fieldInfo2 = valuableObject2.GetType().GetField("dollarValueCurrent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? valuableObject2.GetType().GetField("dollarValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (fieldInfo2 != null)
					{
						try
						{
							num20 = Convert.ToInt32(fieldInfo2.GetValue(valuableObject2));
						}
						catch (Exception ex6)
						{
							Debug.Log((object)("Error reading 'dollarValueCurrent' for '" + text5 + "': " + ex6.Message + ". Defaulting to 0."));
						}
					}
				}
				Color textColor2 = (flag2 ? Color.red : Color.yellow);
				nameStyle.normal.textColor = textColor2;
				string text6 = "";
				if (showItemDistance && (Object)(object)localPlayer != (Object)null)
				{
					float num21 = Vector3.Distance(localPlayer.transform.position, position2);
					text6 = $" [{num21:F1}m]";
				}
				string text7 = (showItemNames ? text5 : "");
				if (showItemDistance)
				{
					text7 += text6;
				}
				float num22 = 200f;
				float num23 = valueStyle.CalcHeight(new GUIContent(num20 + "$"), num22);
				float num24 = nameStyle.CalcHeight(new GUIContent(text7), num22);
				float num25 = num24 + num23 + 5f;
				float num26 = num18 - num22 / 2f;
				float num27 = num19 - num25 - 5f;
				if (!string.IsNullOrEmpty(text7))
				{
					GUI.Label(new Rect(num26, num27, num22, num24), text7, nameStyle);
				}
				if (showItemValue && !flag2)
				{
					GUI.Label(new Rect(num26, num27 + num24 + 2f, num22, num23), num20 + "$", valueStyle);
				}
				if (draw3DItemEspBool)
				{
					CreateBoundsEdges(GetActiveColliderBounds(((Component)val9).gameObject), Color.yellow);
				}
				nameStyle.normal.textColor = textColor;
			}
		}
		if (drawExtractionPointEspBool)
		{
			extractionPointList.RemoveAll(delegate(ExtractionPointData ep)
			{
				try
				{
					int result;
					if (ep != null && !((Object)(object)ep.ExtractionPoint == (Object)null))
					{
						Object extractionPoint = (Object)(object)ep.ExtractionPoint;
						if (extractionPoint == null || !(extractionPoint == (Object)null))
						{
							result = ((!((Component)ep.ExtractionPoint).gameObject.activeInHierarchy) ? 1 : 0);
							goto IL_003a;
						}
					}
					result = 1;
					goto IL_003a;
					IL_003a:
					return (byte)result != 0;
				}
				catch
				{
					return true;
				}
			});
			foreach (ExtractionPointData extractionPoint2 in extractionPointList)
			{
				if ((Object)(object)extractionPoint2.ExtractionPoint == (Object)null || !((Component)extractionPoint2.ExtractionPoint).gameObject.activeInHierarchy)
				{
					continue;
				}
				Vector3 val11 = cachedCamera.WorldToScreenPoint(extractionPoint2.CachedPosition);
				if (val11.z > 0f && val11.x > 0f && val11.x < (float)Screen.width && val11.y > 0f && val11.y < (float)Screen.height)
				{
					float num28 = val11.x * scaleX;
					float num29 = (float)Screen.height - val11.y * scaleY;
					string text8 = "Extraction Point";
					string text9 = " (" + extractionPoint2.CachedState + ")";
					string text10 = ((showExtractionDistance && (Object)(object)localPlayer != (Object)null) ? $"{Vector3.Distance(localPlayer.transform.position, extractionPoint2.CachedPosition):F1}m" : "");
					Color textColor3 = nameStyle.normal.textColor;
					nameStyle.normal.textColor = ((extractionPoint2.CachedState == "Active") ? Color.green : ((extractionPoint2.CachedState == "Idle") ? Color.red : Color.cyan));
					string text11 = (showExtractionNames ? (text8 + text9) : "");
					if (showExtractionDistance)
					{
						text11 = text11 + " " + text10;
					}
					float num30 = 150f;
					float num31 = nameStyle.CalcHeight(new GUIContent(text11), num30);
					float num32 = num31;
					float num33 = num28 - num30 / 2f;
					float num34 = num29 - num32 - 5f;
					if (!string.IsNullOrEmpty(text11))
					{
						GUI.Label(new Rect(num33, num34, num30, num31), text11, nameStyle);
					}
					nameStyle.normal.textColor = textColor3;
				}
			}
		}
		if (!drawPlayerEspBool)
		{
			return;
		}
		playerDataList.RemoveAll(delegate(PlayerData player)
		{
			try
			{
				return player == null || (Object)(object)player.Transform == (Object)null || (Object)(object)((Component)player.Transform).gameObject == (Object)null || !((Component)player.Transform).gameObject.activeInHierarchy;
			}
			catch
			{
				return true;
			}
		});
		foreach (PlayerData playerData in playerDataList)
		{
			bool flag3 = false;
			if (!PhotonNetwork.IsConnected && (Object)(object)localPlayer != (Object)null && (Object)(object)((Component)playerData.Transform).gameObject == (Object)(object)localPlayer)
			{
				flag3 = true;
			}
			if ((Object)(object)playerData.PhotonView == (Object)null || (playerData.PhotonView.IsMine && PhotonNetwork.IsConnected) || flag3)
			{
				continue;
			}
			Vector3 position3 = playerData.Transform.position;
			float num35 = (((Object)(object)localPlayer != (Object)null) ? Vector3.Distance(localPlayer.transform.position, position3) : float.MaxValue);
			if (num35 > 100f)
			{
				continue;
			}
			Vector3 val12 = position3;
			float num36 = 2f;
			Vector3 val13 = position3 + Vector3.up * num36;
			Vector3 val14 = cachedCamera.WorldToScreenPoint(val12);
			Vector3 val15 = cachedCamera.WorldToScreenPoint(val13);
			if (val14.z > 0f && val15.z > 0f)
			{
				float num37 = val14.x * scaleX;
				float num38 = (float)Screen.height - val14.y * scaleY;
				float num39 = (float)Screen.height - val15.y * scaleY;
				float num40 = Mathf.Abs(num38 - num39);
				float num41 = playerData.Transform.localScale.y * 200f / (num35 + 1f) * scaleX;
				num41 = Mathf.Clamp(num41, 30f, num40 * 1.2f);
				num40 = Mathf.Clamp(num40, 40f, 400f);
				float x = num37;
				float y = num38;
				if (draw3DPlayerEspBool)
				{
					CreateBoundsEdges(GetActiveColliderBounds(((Component)playerData.Transform).gameObject), Color.red);
				}
				if (draw2DPlayerEspBool)
				{
					Box(x, y, num41, num40, texture2);
				}
				Color textColor4 = nameStyle.normal.textColor;
				nameStyle.normal.textColor = Color.white;
				int num42 = (playerHealthCache.ContainsKey(playerData.PhotonView.ViewID) ? playerHealthCache[playerData.PhotonView.ViewID] : 100);
				string text12 = $"HP: {num42}";
				string text13 = ((showPlayerDistance && (Object)(object)localPlayer != (Object)null) ? $"{num35:F1}m" : "");
				string text14 = (showPlayerNames ? playerData.Name : "");
				if (showPlayerDistance)
				{
					text14 = text14 + " " + text13;
				}
				float num43 = 150f;
				float num44 = nameStyle.CalcHeight(new GUIContent(text14), num43);
				float num45 = healthStyle.CalcHeight(new GUIContent(text12), num43);
				float num46 = num44 + (showPlayerHP ? (num45 + 2f) : 0f);
				float num47 = num37 - num43 / 2f;
				float num48 = num38 - num40 - num46 - 10f;
				if (!string.IsNullOrEmpty(text14))
				{
					GUI.Label(new Rect(num47, num48, num43, num44), text14, nameStyle);
				}
				if (showPlayerHP)
				{
					GUI.Label(new Rect(num47, num48 + num44 + 2f, num43, num45), text12, healthStyle);
				}
				nameStyle.normal.textColor = textColor4;
			}
		}
	}
}
