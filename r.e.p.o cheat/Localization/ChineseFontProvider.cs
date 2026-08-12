using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace r.e.p.o_cheat.Localization
{
	/// <summary>
	/// Attaches a CJK-capable font as FALLBACK (never replaces the original font), so English,
	/// numbers and icons keep rendering with the game's LiberationSans and only Chinese glyphs
	/// come from the CJK asset. Priority:
	///   1. Game's own Noto Sans SC/JP SDF asset (already shipped in sharedassets0.assets)
	///   2. Any loaded TMP_FontAsset with CJK glyph coverage
	///   3. Runtime dynamic font from the OS (Microsoft YaHei UI / Microsoft YaHei) - not redistributed
	/// The chosen asset is cached globally; never one font asset per text object.
	/// </summary>
	public static class ChineseFontProvider
	{
		private const string CjkProbe = "中文游戏"; // 中文游戏

		private static readonly string[] NameMarkers = { "Noto", "SC", "CJK", "Fallback", "YaHei", "Chinese" };

		private static readonly List<TMP_FontAsset> _attachedFallbacks = new List<TMP_FontAsset>();

		/// <summary>Fonts this provider added to TMP_Settings.fallbackFontAssets (revert removes only these).</summary>
		private static readonly List<TMP_FontAsset> _addedGlobalFallbacks = new List<TMP_FontAsset>();

		private static TMP_FontAsset _cjkFont;

		private static bool _searched;

		private static bool _fallbackAttached;

		/// <summary>True once a CJK fallback is attached to the default font (idempotent).</summary>
		public static bool IsActive
		{
			get { return _fallbackAttached; }
		}

		/// <summary>Ensures the default TMP font can render CJK. Idempotent. Never throws.</summary>
		public static string EnsureCjkFallback()
		{
			try
			{
				if (_fallbackAttached && _cjkFont != null)
				{
					return "already-active:" + _cjkFont.name;
				}

				TMP_FontAsset cjk = FindOrCreateCjkFont();
				if (cjk == null)
				{
					return "no-cjk-font-found";
				}

				TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
				if (defaultFont != null)
				{
					if (defaultFont.fallbackFontAssetTable == null)
					{
						defaultFont.fallbackFontAssetTable = new List<TMP_FontAsset>();
					}
					if (!defaultFont.fallbackFontAssetTable.Contains(cjk))
					{
						defaultFont.fallbackFontAssetTable.Add(cjk);
						_attachedFallbacks.Add(cjk);
					}
					if (defaultFont.fallbackFontAssetTable.Contains(cjk))
					{
						_fallbackAttached = true;
					}
				}

				// also register globally so any font created later inherits the fallback
				// (track the add so Revert can remove exactly what we added - the game may
				// ship its own CJK font in this list and that entry must survive disable)
				List<TMP_FontAsset> globalFallbacks = TMP_Settings.fallbackFontAssets;
				if (globalFallbacks != null && !globalFallbacks.Contains(cjk))
				{
					globalFallbacks.Add(cjk);
					_addedGlobalFallbacks.Add(cjk);
				}

				return "attached:" + cjk.name + (defaultFont == null ? " (default font missing)" : string.Empty);
			}
			catch (Exception ex)
			{
				return "error:" + ex.Message;
			}
		}

		/// <summary>Removes fallbacks this provider attached (used on disable/unload).</summary>
		public static void Revert()
		{
			try
			{
				TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
				if (defaultFont != null && defaultFont.fallbackFontAssetTable != null)
				{
					foreach (TMP_FontAsset font in _attachedFallbacks)
					{
						if (font != null)
						{
							defaultFont.fallbackFontAssetTable.Remove(font);
						}
					}
				}
				List<TMP_FontAsset> globalFallbacks = TMP_Settings.fallbackFontAssets;
				if (globalFallbacks != null)
				{
					foreach (TMP_FontAsset font in _addedGlobalFallbacks)
					{
						if (font != null)
						{
							globalFallbacks.Remove(font);
						}
					}
				}
			}
			catch
			{
			}
			finally
			{
				_attachedFallbacks.Clear();
				_addedGlobalFallbacks.Clear();
				_fallbackAttached = false;
			}
		}

		private static TMP_FontAsset FindOrCreateCjkFont()
		{
			if (_searched && _cjkFont != null)
			{
				return _cjkFont;
			}
			_searched = true;

			// 1. already in the global fallback list (game may ship Noto fonts there)
			List<TMP_FontAsset> globalFallbacks = TMP_Settings.fallbackFontAssets;
			if (globalFallbacks != null)
			{
				foreach (TMP_FontAsset font in globalFallbacks)
				{
					if (font != null && HasCjk(font))
					{
						_cjkFont = font;
						return _cjkFont;
					}
				}
			}

			// 2. default font's own fallback table
			TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
			if (defaultFont != null && defaultFont.fallbackFontAssetTable != null)
			{
				foreach (TMP_FontAsset font in defaultFont.fallbackFontAssetTable)
				{
					if (font != null && HasCjk(font))
					{
						_cjkFont = font;
						return _cjkFont;
					}
				}
			}

			// 3. any loaded font asset with CJK coverage
			try
			{
				TMP_FontAsset[] loaded = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
				foreach (TMP_FontAsset font in loaded)
				{
					if (font == null || _cjkFont == font)
					{
						continue;
					}
					if (HasCjk(font))
					{
						_cjkFont = font;
						return _cjkFont;
					}
				}
			}
			catch
			{
			}

			// 4. runtime dynamic font from the OS (never redistributed with the repo)
			try
			{
				Font osFont = Font.CreateDynamicFontFromOSFont("Microsoft YaHei UI", 48);
				if (osFont == null)
				{
					osFont = Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 48);
				}
				if (osFont != null)
				{
					_cjkFont = TMP_FontAsset.CreateFontAsset(osFont);
					if (_cjkFont != null)
					{
						_cjkFont.name = "ChineseOSFallback";
					}
					return _cjkFont;
				}
			}
			catch
			{
			}

			return null;
		}

		private static bool HasCjk(TMP_FontAsset font)
		{
			if (font == null)
			{
				return false;
			}
			// static SDF atlases must actually cover CJK
			try
			{
				if (font.HasCharacters(CjkProbe, out List<char> _))
				{
					return true;
				}
			}
			catch
			{
			}
			// dynamic-population fonts don't have the glyphs in their table yet -
			// fall back to a name heuristic (game's Noto assets, OS-created fallback)
			foreach (string marker in NameMarkers)
			{
				if (font.name.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}
			}
			return false;
		}
	}
}
