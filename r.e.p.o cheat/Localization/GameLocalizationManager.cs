using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;

namespace r.e.p.o_cheat.Localization
{
	/// <summary>
	/// R.E.P.O. game-body Simplified Chinese localization. A FEATURE, OFF BY DEFAULT.
	///
	/// Primary hook: patches the game's own Unity.Localization StringTables (HUD/Menu/Game)
	/// for the selected locale AND the project (fallback) locale - the same tables the game's
	/// official TSV override mechanism mutates. Re-applied on every locale change and scene
	/// load. Revert restores saved originals, so disable is clean.
	///
	/// Secondary hook: <see cref="TextInterceptor"/> handles rare hardcoded English strings.
	/// Font: <see cref="ChineseFontProvider"/> attaches a CJK fallback; the original game font
	/// keeps rendering English/numbers/icons.
	///
	/// Purely local: never touches Photon state, RPCs, player data or lobby state.
	/// </summary>
	public class GameLocalizationManager : MonoBehaviour
	{
		public const string KeyEnabled = "EnableGameChineseLocalization";

		public const string KeyMode = "GameChineseMode";

		public const string KeyDebugLog = "LocalizationDebugLog";

		public enum LanguageMode
		{
			Auto = 0,
			ZhCn = 1,
			English = 2
		}

		public static GameLocalizationManager Instance { get; private set; }

		public TranslationDatabase Database { get; private set; }

		public LanguageMode Mode { get; private set; }

		public bool Enabled { get; private set; }

		/// <summary>True when text should currently render translated.</summary>
		public bool IsTranslating { get; private set; }

		public string LastStatus { get; private set; }

		private static readonly string[] TableNames = { TranslationDatabase.TableHud, TranslationDatabase.TableMenu, TranslationDatabase.TableGame };

		/// <summary>table::localeCode::key -> original value before our first patch (null = key added by us).</summary>
		private readonly Dictionary<string, string> _originals = new Dictionary<string, string>(StringComparer.Ordinal);

		private readonly HashSet<string> _addedKeys = new HashSet<string>(StringComparer.Ordinal);

		private Action _localeChangedCallback;

		private int _applyAttempts;

		private bool _patchApplied;

		private bool _errorLogged;

		private string _fontStatus;

		public static string ExternalOverridePath
		{
			get { return TranslationDatabase.GetExternalOverridePath(); }
		}

		private void Awake()
		{
			// tolerate repeated injection: one manager only
			if (Instance != null && Instance != this)
			{
				UnityEngine.Object.Destroy(this);
				return;
			}
			Instance = this;
			LoadConfig();
			Database = TranslationDatabase.Load();
		}

		private void Start()
		{
			StartCoroutine(InitRoutine());
		}

		private void OnDestroy()
		{
			// revert all table patches so unload leaves the game in its original state
			CleanupForUnload();
			if (Instance == this)
			{
				Instance = null;
			}
		}

		private void OnApplicationQuit()
		{
			FlushMissingTranslations();
		}

		private IEnumerator InitRoutine()
		{
			// the game's LocalizationManager awakes in the main menu; wait for it
			Type managerType = Type.GetType("LocalizationManager, Assembly-CSharp");
			while (managerType == null && _applyAttempts < 30)
			{
				_applyAttempts++;
				yield return new WaitForSeconds(1f);
				managerType = Type.GetType("LocalizationManager, Assembly-CSharp");
			}
			if (managerType != null)
			{
				while (UnityEngine.Object.FindObjectOfType(managerType) == null && _applyAttempts < 90)
				{
					_applyAttempts++;
					yield return new WaitForSeconds(1f);
				}
			}
			LastStatus = "ready";
			if (Enabled && Mode != LanguageMode.English)
			{
				ApplyAll();
			}
			LogStatus();
		}

		/// <summary>Enables/disables the feature. Persists config. Safe to call any time.</summary>
		public void SetEnabled(bool enabled)
		{
			if (Enabled == enabled)
			{
				return;
			}
			Enabled = enabled;
			ConfigManager.SaveToggle(KeyEnabled, enabled);
			if (enabled && Mode != LanguageMode.English)
			{
				ApplyAll();
			}
			else
			{
				RevertAll();
			}
			LogStatus();
		}

		/// <summary>Sets language mode (Auto/ZhCn/English). Persists config.</summary>
		public void SetMode(LanguageMode mode)
		{
			if (Mode == mode)
			{
				return;
			}
			Mode = mode;
			ConfigManager.SaveInt(KeyMode, (int)mode);
			if (Enabled && Mode != LanguageMode.English)
			{
				ApplyAll();
			}
			else
			{
				RevertAll();
			}
		}

		/// <summary>Toggles the diagnostics scanner (writes localization-missing.txt).</summary>
		public void SetDebugLog(bool on)
		{
			ConfigManager.SaveToggle(KeyDebugLog, on);
			TextInterceptor.RecordMissing = on;
		}

		public bool DebugLog
		{
			get { return TextInterceptor.RecordMissing; }
		}

		private void LoadConfig()
		{
			Enabled = ConfigManager.LoadToggle(KeyEnabled, defaultValue: false);
			Mode = (LanguageMode)ConfigManager.LoadInt(KeyMode, 0);
			TextInterceptor.RecordMissing = ConfigManager.LoadToggle(KeyDebugLog);
		}

		// ------------------------------------------------------------------ table patch

		private void ApplyAll()
		{
			if (Database == null)
			{
				return;
			}
			try
			{
				var locales = CollectLocales();
				int applied = 0;
				foreach (string tableName in TableNames)
				{
					if (!Database.Tables.TryGetValue(tableName, out var translations))
					{
						continue;
					}
					foreach (Locale locale in locales)
					{
						if (locale == null || string.IsNullOrEmpty(locale.Identifier.Code))
						{
							continue;
						}
						applied += PatchTable(tableName, locale, translations);
					}
				}
				_patchApplied = applied > 0;
				IsTranslating = Enabled && Mode != LanguageMode.English;
				SubscribeCallbacks();
				_fontStatus = ChineseFontProvider.EnsureCjkFallback();
			}
			catch (Exception ex)
			{
				LogOnce("apply failed: " + ex);
			}
		}

		private int PatchTable(string tableName, Locale locale, Dictionary<string, string> translations)
		{
			int applied = 0;
			try
			{
				StringTable table = LocalizationSettings.StringDatabase.GetTable(tableName, locale);
				if (table == null)
				{
					return 0;
				}
				foreach (var kvp in translations)
				{
					string storageKey = tableName + "::" + locale.Identifier.Code + "::" + kvp.Key;
					StringTableEntry entry = table.GetEntry(kvp.Key);
					if (entry == null)
					{
						table.AddEntry(kvp.Key, kvp.Value);
						_originals[storageKey] = null;
						_addedKeys.Add(storageKey);
						applied++;
						continue;
					}
					if (!_originals.ContainsKey(storageKey))
					{
						_originals[storageKey] = entry.Value;
					}
					entry.Value = kvp.Value;
					applied++;
				}
			}
			catch (Exception ex)
			{
				LogOnce("table patch failed for " + tableName + "/" + (locale != null ? locale.Identifier.Code : "?") + ": " + ex.Message);
			}
			return applied;
		}

		private void RevertAll()
		{
			IsTranslating = false;
			UnsubscribeCallbacks();
			ChineseFontProvider.Revert();
			try
			{
				foreach (var kvp in _originals)
				{
					// storageKey = table::localeCode::key
					string[] parts = kvp.Key.Split(new[] { "::" }, StringSplitOptions.None);
					if (parts.Length != 3)
					{
						continue;
					}
					Locale locale = FindLocaleByCode(parts[1]);
					if (locale == null)
					{
						continue;
					}
					StringTable table = LocalizationSettings.StringDatabase.GetTable(parts[0], locale);
					if (table == null)
					{
						continue;
					}
					if (_addedKeys.Contains(kvp.Key))
					{
						table.RemoveEntry(parts[2]);
						continue;
					}
					StringTableEntry entry = table.GetEntry(parts[2]);
					if (entry != null)
					{
						entry.Value = kvp.Value ?? string.Empty;
					}
				}
			}
			catch (Exception ex)
			{
				LogOnce("revert failed: " + ex);
			}
			_originals.Clear();
			_addedKeys.Clear();
			_patchApplied = false;
		}

		private List<Locale> CollectLocales()
		{
			var locales = new List<Locale>();
			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			try
			{
				AddLocale(LocalizationSettings.SelectedLocale, locales, seen);
			}
			catch
			{
			}
			try
			{
				AddLocale(LocalizationSettings.ProjectLocale, locales, seen);
			}
			catch
			{
			}
			return locales;
		}

		private static void AddLocale(Locale locale, List<Locale> list, HashSet<string> seen)
		{
			if (locale == null || string.IsNullOrEmpty(locale.Identifier.Code))
			{
				return;
			}
			if (seen.Add(locale.Identifier.Code))
			{
				list.Add(locale);
			}
		}

		private Locale FindLocaleByCode(string code)
		{
			foreach (Locale locale in CollectLocales())
			{
				if (string.Equals(locale.Identifier.Code, code, StringComparison.OrdinalIgnoreCase))
				{
					return locale;
				}
			}
			// a locale patched while it was selected may no longer be SelectedLocale/ProjectLocale;
			// revert must still reach its tables, so also sweep every available locale
			try
			{
				var available = LocalizationSettings.AvailableLocales;
				if (available != null && available.Locales != null)
				{
					foreach (Locale locale in available.Locales)
					{
						if (locale != null && string.Equals(locale.Identifier.Code, code, StringComparison.OrdinalIgnoreCase))
						{
							return locale;
						}
					}
				}
			}
			catch
			{
			}
			return null;
		}

		private void SubscribeCallbacks()
		{
			if (_localeChangedCallback != null)
			{
				return;
			}
			_localeChangedCallback = OnGameLocaleChanged;
			try
			{
				UnityEngine.Object manager = UnityEngine.Object.FindObjectOfType(Type.GetType("LocalizationManager, Assembly-CSharp"));
				if (manager != null)
				{
					var add = manager.GetType().GetMethod("AddLocalizationChangedCallback", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
					add?.Invoke(manager, new object[] { _localeChangedCallback });
				}
			}
			catch
			{
			}
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		private void UnsubscribeCallbacks()
		{
			if (_localeChangedCallback != null)
			{
				try
				{
					UnityEngine.Object manager = UnityEngine.Object.FindObjectOfType(Type.GetType("LocalizationManager, Assembly-CSharp"));
					if (manager != null)
					{
						// the game overloads RemoveLocalizationChangedEvent(LocalizationChangedEvent) and
						// RemoveLocalizationChangedEvent(Action); GetMethod by name alone throws
						// AmbiguousMatchException and the unsubscribe would silently fail, leaving the
						// cheat's callback attached to the game's multicast delegate after unload.
						var remove = manager.GetType().GetMethod("RemoveLocalizationChangedEvent", new[] { typeof(Action) });
						remove?.Invoke(manager, new object[] { _localeChangedCallback });
					}
				}
				catch
				{
				}
				_localeChangedCallback = null;
			}
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		private void OnGameLocaleChanged()
		{
			if (Enabled && Mode != LanguageMode.English && Database != null)
			{
				ApplyAll();
			}
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (Enabled && Mode != LanguageMode.English && Database != null)
			{
				ApplyAll();
			}
		}

		// ------------------------------------------------------------------ diagnostics

		public void LogStatus()
		{
			string state = r.e.p.o_cheat.Compatibility.GameVersionInfo.State.ToString();
			Debug.Log("[GameLocalization] state=" + state
				+ " enabled=" + Enabled
				+ " mode=" + Mode
				+ " translating=" + IsTranslating
				+ " tables=" + (Database != null ? Database.TableEntryCount.ToString() : "0")
				+ " direct=" + (Database != null ? Database.DirectCount.ToString() : "0")
				+ " rejected=" + (Database != null ? Database.RejectedCount.ToString() : "0")
				+ " font=" + (_fontStatus ?? "not-applied")
				+ " external=" + (Database != null && Database.ExternalOverrideLoaded ? ExternalOverridePath : "embedded-only")
				+ " missing=" + TextInterceptor.MissingCount);
		}

		private void LogOnce(string message)
		{
			if (_errorLogged)
			{
				return;
			}
			_errorLogged = true;
			Debug.LogWarning("[GameLocalization] " + message);
		}

		public void FlushMissingTranslations()
		{
			if (TextInterceptor.MissingCount == 0)
			{
				return;
			}
			try
			{
				string root = Path.GetDirectoryName(Application.dataPath);
				if (string.IsNullOrEmpty(root))
				{
					return;
				}
				var sb = new StringBuilder();
				foreach (string text in TextInterceptor.MissingStrings)
				{
					sb.Append(text.Replace("\n", "\\n")).Append('\n');
				}
				File.WriteAllText(Path.Combine(root, "localization-missing.txt"), sb.ToString(), new UTF8Encoding(false));
				Debug.Log("[GameLocalization] missing translations written to localization-missing.txt (" + TextInterceptor.MissingCount + ")");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[GameLocalization] failed to write missing translations: " + ex.Message);
			}
		}

		/// <summary>Full cleanup on cheat unload: revert table patches, detach font, drop callbacks.</summary>
		public void CleanupForUnload()
		{
			RevertAll();
			FlushMissingTranslations();
		}
	}
}
