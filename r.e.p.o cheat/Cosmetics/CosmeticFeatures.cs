using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace r.e.p.o_cheat
{
	/// <summary>
	/// Cosmetics: unlock everything, random outfits, and a rainbow cycle - all through the
	/// game's own MetaManager / PlayerCosmetics API (equip checks, type conflicts, saves and
	/// the SetupCosmeticsRPC/SetupColorsRPC sync are handled by the game itself).
	/// Replaces the old playerColor RGB hack, which no longer maps to the v0.4.x color system.
	/// </summary>
	public static class CosmeticFeatures
	{
		public static bool RainbowMode;

		public static float RainbowIntervalSeconds = 4f;

		public static string LastStatus = string.Empty;

		private static float _nextRainbowTime;

		// MetaManager internals (internal in the game assembly -> cached reflection, read per use)
		private static FieldInfo _unlocksField;

		private static FieldInfo _equippedField;

		private static FieldInfo _colorsField;

		/// <summary>Unlocks every cosmetic via the game's own API (persists through MetaSave).</summary>
		public static bool UnlockAll()
		{
			try
			{
				MetaManager meta = MetaManager.instance;
				if (meta == null)
				{
					LastStatus = "MetaManager not ready";
					return false;
				}
				List<int> unlocked = GetUnlocked(meta);
				int before = unlocked.Count;
				bool changed = meta.CosmeticUnlockAll();
				LastStatus = changed
					? "Unlocked " + (GetUnlocked(meta).Count - before) + " new cosmetics (" + GetUnlocked(meta).Count + "/" + meta.cosmeticAssets.Count + " total)"
					: "All cosmetics already unlocked (" + before + "/" + meta.cosmeticAssets.Count + ")";
				if (changed)
				{
					RefreshTokenUi();
				}
				return true;
			}
			catch (Exception ex)
			{
				LastStatus = "UnlockAll failed: " + ex.Message;
				return false;
			}
		}

		/// <summary>Equips a random unlocked cosmetic per body slot (and optionally random colors).</summary>
		public static bool RandomizeOutfit(bool randomizeColors)
		{
			try
			{
				MetaManager meta = MetaManager.instance;
				PlayerCosmetics pc = GetLocalPlayerCosmetics();
				if (meta == null)
				{
					LastStatus = "MetaManager not ready";
					return false;
				}
				List<int> unlocked = GetUnlocked(meta);
				if (unlocked.Count == 0)
				{
					LastStatus = "No cosmetics unlocked - run Unlock All first";
					return false;
				}

				// unequip everything currently equipped (copy: the game's method mutates the list)
				foreach (int index in new List<int>(GetEquipped(meta)))
				{
					if (index >= 0 && index < meta.cosmeticAssets.Count && meta.cosmeticAssets[index] != null)
					{
						meta.CosmeticUnequip(meta.cosmeticAssets[index], _isPreview: false);
					}
				}

				// group unlocked assets by slot type
				var byType = new Dictionary<SemiFunc.CosmeticType, List<CosmeticAsset>>();
				foreach (int index in unlocked)
				{
					if (index < 0 || index >= meta.cosmeticAssets.Count)
					{
						continue;
					}
					CosmeticAsset asset = meta.cosmeticAssets[index];
					if (asset == null)
					{
						continue;
					}
					if (!byType.TryGetValue(asset.type, out var list))
					{
						list = new List<CosmeticAsset>();
						byType[asset.type] = list;
					}
					list.Add(asset);
				}

				// one random item per type (extra rolls for multi-equip slots)
				int equippedCount = 0;
				foreach (var pair in byType)
				{
					if (UnityEngine.Random.value > 0.65f)
					{
						continue; // leave some slots empty for variety
					}
					CosmeticTypeAsset typeAsset = pair.Key < (SemiFunc.CosmeticType)meta.cosmeticTypeAssets.Count ? meta.cosmeticTypeAssets[(int)pair.Key] : null;
					bool multi = typeAsset != null && typeAsset.canEquipMultiple;
					int rolls = multi && UnityEngine.Random.value > 0.5f ? 2 : 1;
					for (int r = 0; r < rolls; r++)
					{
						CosmeticAsset pick = pair.Value[UnityEngine.Random.Range(0, pair.Value.Count)];
						if (meta.CosmeticEquip(pick, _isPreview: false))
						{
							equippedCount++;
						}
					}
				}

				// random color per slot
				int[] colors = GetColors(meta);
				if (randomizeColors && meta.colors != null && meta.colors.Count > 0)
				{
					for (int i = 0; i < colors.Length; i++)
					{
						colors[i] = UnityEngine.Random.Range(0, meta.colors.Count);
					}
				}

				// apply locally + broadcast to the lobby (game's own RPCs)
				if (pc != null)
				{
					pc.SetupCosmetics(_synced: true, _forced: false, null);
					pc.SetupColors(_synced: true, colors);
				}
				meta.Save();
				LastStatus = "Random outfit: " + equippedCount + " pieces" + (randomizeColors ? " + random colors" : string.Empty);
				return true;
			}
			catch (Exception ex)
			{
				LastStatus = "RandomizeOutfit failed: " + ex.Message;
				return false;
			}
		}

		/// <summary>Call from Hax2.Update: cycles the outfit while RainbowMode is on.</summary>
		public static void TickRainbow()
		{
			if (!RainbowMode)
			{
				return;
			}
			if (Time.time < _nextRainbowTime)
			{
				return;
			}
			_nextRainbowTime = Time.time + Mathf.Max(1f, RainbowIntervalSeconds);
			RandomizeOutfit(randomizeColors: true);
		}

		public static PlayerCosmetics GetLocalPlayerCosmetics()
		{
			foreach (PlayerAvatar candidate in UnityEngine.Object.FindObjectsOfType<PlayerAvatar>())
			{
				if (candidate != null && candidate.photonView != null && candidate.photonView.IsMine)
				{
					return candidate.playerCosmetics;
				}
			}
			// singleplayer fallback: the only avatar is ours
			PlayerAvatar avatar = UnityEngine.Object.FindObjectOfType<PlayerAvatar>();
			return avatar != null ? avatar.playerCosmetics : null;
		}

		private static List<int> GetUnlocked(MetaManager meta)
		{
			if (_unlocksField == null)
			{
				_unlocksField = typeof(MetaManager).GetField("cosmeticUnlocks", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			}
			return _unlocksField?.GetValue(meta) as List<int> ?? new List<int>();
		}

		private static List<int> GetEquipped(MetaManager meta)
		{
			if (_equippedField == null)
			{
				_equippedField = typeof(MetaManager).GetField("cosmeticEquipped", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			}
			return _equippedField?.GetValue(meta) as List<int> ?? new List<int>();
		}

		private static int[] GetColors(MetaManager meta)
		{
			if (_colorsField == null)
			{
				_colorsField = typeof(MetaManager).GetField("colorsEquipped", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			}
			return _colorsField?.GetValue(meta) as int[] ?? new int[0];
		}

		private static void RefreshTokenUi()
		{
			try
			{
				// token UI mirrors the unlock list; refresh it if present
				UnityEngine.Object tokenUi = UnityEngine.Object.FindObjectOfType(Type.GetType("CosmeticTokenUI, Assembly-CSharp"));
				if (tokenUi != null)
				{
					var setup = tokenUi.GetType().GetMethod("Setup", BindingFlags.Instance | BindingFlags.Public);
					setup?.Invoke(tokenUi, null);
				}
			}
			catch
			{
			}
		}
	}
}
