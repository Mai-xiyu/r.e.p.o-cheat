using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace r.e.p.o_cheat.Localization
{
	/// <summary>
	/// Pure C# translation store. Loaded ONCE at module init, then O(1) lookups only.
	/// Sources (merged in order, later wins):
	///   1. Embedded resource i18n\game\zh-CN.json
	///   2. External override: &lt;game root&gt;\REPOChinese\zh-CN.json (user-editable)
	/// Lookup channels:
	///   Tables["HUD"]["BIG_MESSAGE.EXTRACTION.ACTIVATED"]  - game StringTable keys (primary)
	///   Direct["Use [move] to move."]                      - hardcoded English fallback (interceptor)
	/// Entries failing placeholder/rich-text validation are rejected at load time.
	/// Missing translations always fall back to the original English (never blank).
	/// </summary>
	public class TranslationDatabase
	{
		public const string TableHud = "HUD";

		public const string TableMenu = "Menu";

		public const string TableGame = "Game";

		private const string EmbeddedResourceName = "r.e.p.o_cheat.i18n.game.zh-CN.json";

		private readonly Dictionary<string, Dictionary<string, string>> _tables =
			new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

		private readonly Dictionary<string, string> _direct = new Dictionary<string, string>(StringComparer.Ordinal);

		private readonly List<DirectTemplate> _templates = new List<DirectTemplate>();

		public int TableEntryCount { get; private set; }

		public int DirectCount { get; private set; }

		public int RejectedCount { get; private set; }

		public bool ExternalOverrideLoaded { get; private set; }

		/// <summary>Loads embedded + external translations. Returns null on catastrophic failure.</summary>
		public static TranslationDatabase Load()
		{
			try
			{
				var db = new TranslationDatabase();
				string embedded = ReadEmbedded();
				if (embedded != null)
				{
					db.MergeJson(embedded);
				}
				string externalPath = GetExternalOverridePath();
				if (externalPath != null && File.Exists(externalPath))
				{
					string external = File.ReadAllText(externalPath, Encoding.UTF8);
					db.MergeJson(external);
					db.ExternalOverrideLoaded = true;
				}
				return db;
			}
			catch (Exception ex)
			{
				Debug.LogError("[GameLocalization] TranslationDatabase.Load failed: " + ex);
				return null;
			}
		}

		/// <summary>Test seam: builds a database from raw JSON without touching Unity (no embedded resource, no file IO, no Debug).</summary>
		internal static TranslationDatabase FromJson(string json)
		{
			var db = new TranslationDatabase();
			db.MergeJson(json);
			return db;
		}

		/// <summary>Game string-table lookup. Falls back to source English; never returns null when source is provided.</summary>
		public bool TryGetTable(string table, string key, out string translation)
		{
			translation = null;
			if (_tables.TryGetValue(table, out var keys) && keys.TryGetValue(key, out translation))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Hardcoded-English lookup: exact match first, then template match. Template matching
		/// splits the English key at its {placeholder}/[keybind] tokens into static segments and
		/// matches the RUNTIME text (which already has the placeholders filled in by the game,
		/// e.g. "Press F to interact") against those segments, then substitutes the captured
		/// arguments back into the translation ("按 F 互动").
		/// </summary>
		public bool TryGetDirect(string english, out string translation)
		{
			if (!string.IsNullOrEmpty(english) && _direct.TryGetValue(english, out translation))
			{
				return true;
			}
			if (!string.IsNullOrEmpty(english))
			{
				foreach (DirectTemplate template in _templates)
				{
					if (template.TryMatch(english, out translation))
					{
						return true;
					}
				}
			}
			translation = null;
			return false;
		}

		public IReadOnlyDictionary<string, Dictionary<string, string>> Tables
		{
			get { return _tables; }
		}

		internal void MergeJson(string json)
		{
			Dictionary<string, object> root;
			try
			{
				root = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
			}
			catch (Exception ex)
			{
				// a malformed external override file must never take the module down
				try
				{
					Debug.LogWarning("[GameLocalization] invalid translation JSON ignored: " + ex.Message);
				}
				catch
				{
				}
				return;
			}
			if (root == null)
			{
				return;
			}
			foreach (var kvp in root)
			{
				if (string.Equals(kvp.Key, "direct", StringComparison.OrdinalIgnoreCase))
				{
					MergeDictionary(_direct, kvp.Value as JObject ?? (kvp.Value as JToken)?.ToObject<JObject>(), requireValidation: true);
					continue;
				}
				// table section: {"HUD": {...}}
				if (!_tables.TryGetValue(kvp.Key, out var table))
				{
					table = new Dictionary<string, string>(StringComparer.Ordinal);
					_tables[kvp.Key] = table;
				}
				MergeDictionary(table, kvp.Value as JObject ?? (kvp.Value as JToken)?.ToObject<JObject>(), requireValidation: false);
			}
			RebuildCounts();
		}

		private void MergeDictionary(Dictionary<string, string> target, JObject source, bool requireValidation)
		{
			if (source == null)
			{
				return;
			}
			foreach (var kvp in source)
			{
				string value = kvp.Value?.ToString();
				if (string.IsNullOrEmpty(value))
				{
					continue;
				}
				// direct entries are validated against the ENGLISH source (the key itself)
				if (requireValidation && !TranslationValidator.ValidatePair(kvp.Key, value, out string error))
				{
					RejectedCount++;
					try
					{
						Debug.LogWarning("[GameLocalization] rejected direct entry '" + kvp.Key + "': " + error);
					}
					catch
					{
						// logging must never break loading (also keeps this path hostable in unit tests)
					}
					continue;
				}
				target[kvp.Key] = value;
			}
		}

		private void RebuildCounts()
		{
			TableEntryCount = 0;
			foreach (var table in _tables.Values)
			{
				TableEntryCount += table.Count;
			}
			DirectCount = _direct.Count;
			_templates.Clear();
			foreach (var kvp in _direct)
			{
				DirectTemplate template = DirectTemplate.TryCreate(kvp.Key, kvp.Value);
				if (template != null)
				{
					_templates.Add(template);
				}
			}
		}

		/// <summary>
		/// A direct-entry template: the English key split at its dynamic tokens into static
		/// segments. Matching runtime text consumes the segments in order and captures the
		/// filled-in arguments between them.
		/// </summary>
		private sealed class DirectTemplate
		{
			public string[] Segments;

			/// <summary>Tokens in the order they appear in the English key.</summary>
			public string[] Tokens;

			public string Translation;

			public static DirectTemplate TryCreate(string key, string translation)
			{
				var segments = new List<string>();
				var tokens = new List<string>();
				int start = 0;
				for (int i = 0; i < key.Length; i++)
				{
					int tokenEnd = -1;
					if (key[i] == '{')
					{
						int depth = 0;
						for (int j = i; j < key.Length; j++)
						{
							if (key[j] == '{')
							{
								depth++;
							}
							else if (key[j] == '}')
							{
								depth--;
								if (depth == 0)
								{
									tokenEnd = j;
									break;
								}
							}
						}
					}
					else if (key[i] == '[')
					{
						tokenEnd = key.IndexOf(']', i + 1);
					}
					if (tokenEnd < 0)
					{
						continue;
					}
					segments.Add(key.Substring(start, i - start));
					tokens.Add(key.Substring(i, tokenEnd - i + 1));
					i = tokenEnd;
					start = tokenEnd + 1;
				}
				if (tokens.Count == 0)
				{
					return null; // exact-match only
				}
				segments.Add(key.Substring(start));
				return new DirectTemplate
				{
					Segments = segments.ToArray(),
					Tokens = tokens.ToArray(),
					Translation = translation
				};
			}

			public bool TryMatch(string text, out string translation)
			{
				translation = null;
				if (string.IsNullOrEmpty(text))
				{
					return false;
				}
				var captures = new List<string>();
				int pos = 0;
				for (int s = 0; s < Segments.Length; s++)
				{
					string segment = Segments[s];
					if (segment.Length == 0)
					{
						continue;
					}
					if (s == 0)
					{
						if (!text.StartsWith(segment, StringComparison.Ordinal))
						{
							return false;
						}
						pos = segment.Length;
						continue;
					}
					int found = text.IndexOf(segment, pos, StringComparison.Ordinal);
					if (found <= pos)
					{
						return false; // segment missing, or empty capture (not a real fill-in)
					}
					captures.Add(text.Substring(pos, found - pos));
					pos = found + segment.Length;
				}
				// key ends with a token: the remaining text is the final capture
				if (Segments[Segments.Length - 1].Length == 0)
				{
					if (pos >= text.Length)
					{
						return false;
					}
					captures.Add(text.Substring(pos));
					pos = text.Length;
				}
				if (pos != text.Length || captures.Count != Tokens.Length)
				{
					return false;
				}
				translation = Substitute(Translation, captures);
				return translation != null;
			}

			private string Substitute(string template, List<string> captures)
			{
				// captures[i] fills the i-th token occurrence of the translation (validator
				// guarantees the translation carries the same token multiset as the key)
				var sb = new StringBuilder(template.Length + 16);
				int captureIndex = 0;
				for (int i = 0; i < template.Length; i++)
				{
					int tokenEnd = -1;
					if (template[i] == '{')
					{
						int depth = 0;
						for (int j = i; j < template.Length; j++)
						{
							if (template[j] == '{')
							{
								depth++;
							}
							else if (template[j] == '}')
							{
								depth--;
								if (depth == 0)
								{
									tokenEnd = j;
									break;
								}
							}
						}
					}
					else if (template[i] == '[')
					{
						tokenEnd = template.IndexOf(']', i + 1);
					}
					if (tokenEnd >= 0)
					{
						if (captureIndex < captures.Count)
						{
							sb.Append(captures[captureIndex++]);
						}
						else
						{
							sb.Append(template.Substring(i, tokenEnd - i + 1)); // safety: leave verbatim, never blank
						}
						i = tokenEnd;
						continue;
					}
					sb.Append(template[i]);
				}
				return sb.ToString();
			}
		}

		private static string ReadEmbedded()
		{
			try
			{
				var assembly = typeof(TranslationDatabase).Assembly;
				using (Stream stream = assembly.GetManifestResourceStream(EmbeddedResourceName))
				{
					if (stream == null)
					{
						Debug.LogWarning("[GameLocalization] embedded resource not found: " + EmbeddedResourceName);
						return null;
					}
					using (var reader = new StreamReader(stream, Encoding.UTF8))
					{
						return reader.ReadToEnd();
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[GameLocalization] embedded resource read failed: " + ex.Message);
				return null;
			}
		}

		/// <summary>&lt;game root&gt;\REPOChinese\zh-CN.json (never inside REPO_Data\Managed).</summary>
		public static string GetExternalOverridePath()
		{
			try
			{
				string dataPath = Application.dataPath;
				if (string.IsNullOrEmpty(dataPath))
				{
					return null;
				}
				return Path.Combine(Path.GetDirectoryName(dataPath), "REPOChinese", "zh-CN.json");
			}
			catch
			{
				return null;
			}
		}
	}
}
