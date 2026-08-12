using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace r.e.p.o_cheat.Localization
{
	/// <summary>
	/// Controlled TMP fallback for the small number of game UI strings that bypass the
	/// Unity.Localization string tables entirely (hardcoded .text = writes).
	///
	/// The patch is ALWAYS installed but is a no-op unless localization is enabled, so the
	/// feature stays off by default. Translation happens only when:
	///   - the exact string (or its placeholder-normalized template) exists in the database
	///   - the text component is a UI component AND not player/user content (chat, names,
	///     lobby names, input fields) and not cheat-menu UI
	/// Missing translations fall back to the original English and are optionally recorded
	/// once for the diagnostics scanner.
	/// </summary>
	[HarmonyPatch(typeof(TMP_Text), "set_text")]
	public static class TextInterceptor
	{
		private static readonly string[] ExcludedNameMarkers =
		{
			"chat", "playername", "player name", "inputfield", "input field",
			"console", "debug", "watermark", "esp", "radar", "cheat", "toast", "fps"
		};

		private static readonly Dictionary<TMP_Text, bool> _allowedCache = new Dictionary<TMP_Text, bool>();

		private static readonly HashSet<string> _recordedMissing = new HashSet<string>(StringComparer.Ordinal);

		[ThreadStatic]
		private static bool _applying;

		private static int _cacheTicks;

		public static int MissingCount
		{
			get { return _recordedMissing.Count; }
		}

		public static IEnumerable<string> MissingStrings
		{
			get { return _recordedMissing; }
		}

		public static void ClearMissing()
		{
			_recordedMissing.Clear();
		}

		public static bool RecordMissing { get; set; }

		private static void Postfix(TMP_Text __instance)
		{
			if (_applying)
			{
				return;
			}
			GameLocalizationManager manager = GameLocalizationManager.Instance;
			if (manager == null || !manager.IsTranslating)
			{
				return;
			}
			TranslationDatabase db = manager.Database;
			if (db == null || __instance == null)
			{
				return;
			}
			try
			{
				if (!( __instance is TextMeshProUGUI))
				{
					return;
				}
				if (!IsAllowed(__instance))
				{
					return;
				}
				string source = __instance.text;
				if (string.IsNullOrEmpty(source) || !db.TryGetDirect(source, out string translated) || translated == source)
				{
					MaybeRecordMissing(db, source);
					return;
				}
				_applying = true;
				try
				{
					__instance.text = translated;
				}
				finally
				{
					_applying = false;
				}
			}
			catch
			{
				// interceptor must never break the game; on error stay on the safe English path
			}
		}

		private static bool IsAllowed(TMP_Text text)
		{
			if (_allowedCache.TryGetValue(text, out bool allowed))
			{
				return allowed;
			}
			if (_cacheTicks++ > 512)
			{
				PurgeDestroyed();
			}
			allowed = ComputeAllowed(text);
			_allowedCache[text] = allowed;
			return allowed;
		}

		private static bool ComputeAllowed(TMP_Text text)
		{
			Transform t = text.transform;
			while (t != null)
			{
				string name = t.gameObject.name;
				foreach (string marker in ExcludedNameMarkers)
				{
					if (name.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return false;
					}
				}
				t = t.parent;
			}
			return true;
		}

		private static void PurgeDestroyed()
		{
			_cacheTicks = 0;
			var dead = new List<TMP_Text>();
			foreach (var kvp in _allowedCache)
			{
				if (kvp.Key == null)
				{
					dead.Add(kvp.Key);
				}
			}
			foreach (TMP_Text key in dead)
			{
				_allowedCache.Remove(key);
			}
		}

		private static void MaybeRecordMissing(TranslationDatabase db, string source)
		{
			if (!RecordMissing || string.IsNullOrEmpty(source))
			{
				return;
			}
			if (source.Length > 200 || _recordedMissing.Contains(source))
			{
				return;
			}
			// game-authored text heuristic: has letters, at least one word boundary,
			// not markup-only, not a path/URL, not user input noise
			int letters = 0;
			int spaces = 0;
			bool markupOnly = true;
			foreach (char c in source)
			{
				if (char.IsLetter(c))
				{
					letters++;
					markupOnly = false;
				}
				else if (c == ' ')
				{
					spaces++;
				}
			}
			if (letters < 3 || spaces == 0 || markupOnly)
			{
				return;
			}
			if (source.IndexOf("://", StringComparison.Ordinal) >= 0)
			{
				return;
			}
			_recordedMissing.Add(source);
		}
	}
}
