using System;
using System.Collections.Generic;

namespace r.e.p.o_cheat.Localization
{
	/// <summary>
	/// Pure C# TMP rich-text tag handling. A translation must carry the same tag structure
	/// as its source so &lt;color&gt;/&lt;b&gt;/&lt;sprite&gt; markup keeps rendering.
	/// </summary>
	public static class RichTextPreserver
	{
		private static readonly HashSet<string> VoidTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"sprite", "br", "space"
		};

		/// <summary>Tag name -> (open count, close count).</summary>
		public static Dictionary<string, Pair> CountTags(string text)
		{
			var counts = new Dictionary<string, Pair>(StringComparer.OrdinalIgnoreCase);
			if (string.IsNullOrEmpty(text))
			{
				return counts;
			}
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] != '<' || i + 1 >= text.Length)
				{
					continue;
				}
				int end = text.IndexOf('>', i + 1);
				if (end < 0)
				{
					break;
				}
				string inner = text.Substring(i + 1, end - i - 1).Trim();
				if (inner.Length == 0 || inner[0] == '!' || inner[0] == '/')
				{
					// comment or closing handled below; closing tag:
					if (inner.Length > 1 && inner[0] == '/')
					{
						// closing: name may be trimmed, no attributes
						int space = inner.IndexOfAny(new[] { ' ', '\t' });
						string closeName = (space > 0 ? inner.Substring(1, space - 1) : inner.Substring(1)).Trim();
						if (closeName.Length > 0 && !VoidTags.Contains(closeName))
						{
							Increment(counts, closeName, open: false);
						}
					}
					i = end;
					continue;
				}
				// opening tag: name then optional attributes
				int nameEnd = inner.Length;
				for (int j = 0; j < inner.Length; j++)
				{
					char c = inner[j];
					if (c == ' ' || c == '\t' || c == '=' || c == '/')
					{
						nameEnd = j;
						break;
					}
				}
				string name = inner.Substring(0, nameEnd);
				if (name.Length > 0)
				{
					Increment(counts, name, open: true);
				}
				i = end;
			}
			return counts;
		}

		/// <summary>True when source and translation have identical tag name counts (open and close).</summary>
		public static bool ValidateTagParity(string source, string translation, out string error)
		{
			error = null;
			Dictionary<string, Pair> s = CountTags(source);
			Dictionary<string, Pair> t = CountTags(translation);
			var names = new HashSet<string>(s.Keys, StringComparer.OrdinalIgnoreCase);
			names.UnionWith(t.Keys);
			foreach (string name in names)
			{
				s.TryGetValue(name, out Pair sp);
				t.TryGetValue(name, out Pair tp);
				if (sp.Open != tp.Open || sp.Close != tp.Close)
				{
					error = string.Concat("TMP tag '<", name, ">' parity mismatch: source ", sp, " vs translation ", tp);
					return false;
				}
				// every non-void opening tag needs its closing tag in BOTH texts
				if (!VoidTags.Contains(name) && (sp.Open != sp.Close || tp.Open != tp.Close))
				{
					error = string.Concat("TMP tag '<", name, ">' unbalanced (open ", sp, ", close ", tp, ")");
					return false;
				}
			}
			return true;
		}

		private static void Increment(Dictionary<string, Pair> counts, string name, bool open)
		{
			counts.TryGetValue(name, out Pair pair);
			pair = new Pair { Open = pair.Open + (open ? 1 : 0), Close = pair.Close + (open ? 0 : 1) };
			counts[name] = pair;
		}

		public struct Pair
		{
			public int Open;

			public int Close;

			public override string ToString()
			{
				return string.Concat(Open.ToString(), "/", Close.ToString());
			}
		}
	}
}
