using System;
using System.Collections.Generic;
using System.Text;

namespace r.e.p.o_cheat.Localization
{
	/// <summary>
	/// Pure C# validation of translation pairs. A translation is only accepted when every
	/// dynamic token of the source survives verbatim:
	///   {smart}        - Unity.Localization SmartFormat placeholders, incl. nested {list:{}|...|...}
	///   [keybind]      - R.E.P.O. keybinding tags, replaced at display time by InputManager
	///   &lt;color&gt; etc. - TMP rich text tags
	///   %s %d %n       - printf-style placeholders
	///   \n             - newline escapes
	/// </summary>
	public static class TranslationValidator
	{
		/// <summary>Extract balanced {placeholder} tokens, outermost first, nested braces kept inside the token.</summary>
		public static List<string> ExtractSmartTokens(string text)
		{
			var tokens = new List<string>();
			if (string.IsNullOrEmpty(text))
			{
				return tokens;
			}
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] != '{')
				{
					continue;
				}
				int depth = 0;
				int end = -1;
				for (int j = i; j < text.Length; j++)
				{
					if (text[j] == '{')
					{
						depth++;
					}
					else if (text[j] == '}')
					{
						depth--;
						if (depth == 0)
						{
							end = j;
							break;
						}
					}
				}
				if (end >= 0)
				{
					tokens.Add(text.Substring(i, end - i + 1));
					i = end;
				}
			}
			return tokens;
		}

		/// <summary>Extract [keybind] tokens.</summary>
		public static List<string> ExtractKeybindTokens(string text)
		{
			var tokens = new List<string>();
			if (string.IsNullOrEmpty(text))
			{
				return tokens;
			}
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] != '[')
				{
					continue;
				}
				int end = text.IndexOf(']', i + 1);
				if (end > i)
				{
					tokens.Add(text.Substring(i, end - i + 1));
					i = end;
				}
			}
			return tokens;
		}

		/// <summary>Extract printf-style %s/%d/%n/%f tokens (longest match first: %2d etc.).</summary>
		public static List<string> ExtractPercentTokens(string text)
		{
			var tokens = new List<string>();
			if (string.IsNullOrEmpty(text))
			{
				return tokens;
			}
			for (int i = 0; i < text.Length - 1; i++)
			{
				if (text[i] != '%')
				{
					continue;
				}
				char next = text[i + 1];
				if ((next >= 'a' && next <= 'z') || (next >= 'A' && next <= 'Z') || next == '%')
				{
					// include optional width/precision digits immediately before the letter
					int start = i + 1;
					while (start < text.Length && char.IsDigit(text[start]))
					{
						start++;
					}
					int end = start + 1;
					tokens.Add(text.Substring(i, Math.Min(end, text.Length) - i));
					i = Math.Min(end, text.Length) - 1;
				}
			}
			return tokens;
		}

		public static int CountNewlines(string text)
		{
			int count = 0;
			if (string.IsNullOrEmpty(text))
			{
				return count;
			}
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '\n')
				{
					count++;
				}
			}
			return count;
		}

		/// <summary>
		/// True when source and translation contain the same multiset of every dynamic token
		/// (smart braces, keybind brackets, printf percents, newline count) and TMP tag parity holds.
		/// </summary>
		public static bool ValidatePair(string source, string translation, out string error)
		{
			error = null;
			if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(translation))
			{
				error = "empty source or translation";
				return false;
			}

			if (!SameMultiset(ExtractSmartTokens(source), ExtractSmartTokens(translation), out error, "placeholder"))
			{
				return false;
			}
			if (!SameMultiset(ExtractKeybindTokens(source), ExtractKeybindTokens(translation), out error, "keybind tag"))
			{
				return false;
			}
			if (!SameMultiset(ExtractPercentTokens(source), ExtractPercentTokens(translation), out error, "printf token"))
			{
				return false;
			}
			if (CountNewlines(source) != CountNewlines(translation))
			{
				error = "newline count mismatch";
				return false;
			}
			if (!RichTextPreserver.ValidateTagParity(source, translation, out error))
			{
				return false;
			}
			return true;
		}

		private static bool SameMultiset(List<string> a, List<string> b, out string error, string kind)
		{
			error = null;
			if (a.Count != b.Count)
			{
				error = string.Concat(kind, " count mismatch (", a.Count, " vs ", b.Count, ")");
				return false;
			}
			var counts = new Dictionary<string, int>(StringComparer.Ordinal);
			foreach (string token in a)
			{
				counts.TryGetValue(token, out int c);
				counts[token] = c + 1;
			}
			foreach (string token in b)
			{
				if (!counts.TryGetValue(token, out int c) || c == 0)
				{
					error = string.Concat(kind, " '", token, "' missing from translation");
					return false;
				}
				counts[token] = c - 1;
			}
			return true;
		}
	}
}
