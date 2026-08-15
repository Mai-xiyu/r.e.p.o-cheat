using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace r.e.p.o_cheat
{
	/// <summary>
	/// Cosmetics go through MetaManager / PlayerCosmetics (equip, save, SetupCosmeticsRPC / SetupColorsRPC).
	/// Rainbow cycles body tint via SetupColors — not the removed SetColorRPC / playerColor path.
	/// </summary>
	public static class CosmeticFeatures
	{
		public static bool RainbowMode;

		public static float RainbowSpeed = 0.35f;

		public static bool LiveRandom;

		public static float LiveRandomInterval = 2.5f;

		public static string LastStatus = string.Empty;

		private static float _nextColorSync;

		private static float _nextOutfit;

		private static readonly BindingFlags InstAll = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		private static FieldInfo _unlocksField;

		private static FieldInfo _equippedField;

		private static FieldInfo _colorsField;

		private static FieldInfo _tokensField;

		private static FieldInfo _materialsField;

		private static FieldInfo _deathHeadField;

		private static FieldInfo _menuCosmeticsField;

		private static int _albedoId;

		private static int _emissionId;

		private static int _fresnelId;

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
				int after = GetUnlocked(meta).Count;
				LastStatus = changed
					? "Unlocked " + (after - before) + " cosmetics (" + after + "/" + meta.cosmeticAssets.Count + ")"
					: "Already unlocked (" + before + "/" + meta.cosmeticAssets.Count + ")";
				RefreshTokenUi();
				ApplyLocalCosmetics(sync: true, forced: true);
				return true;
			}
			catch (Exception ex)
			{
				LastStatus = "UnlockAll failed: " + ex.Message;
				return false;
			}
		}

		public static bool RandomizeOutfit(bool randomizeColors, bool persistSave = true)
		{
			try
			{
				MetaManager meta = MetaManager.instance;
				if (meta == null)
				{
					LastStatus = "MetaManager not ready";
					return false;
				}
				if (GetUnlocked(meta).Count == 0)
				{
					meta.CosmeticUnlockAll();
				}
				List<int> unlocked = GetUnlocked(meta);
				if (unlocked.Count == 0)
				{
					LastStatus = "No cosmetics in MetaManager";
					return false;
				}

				foreach (int index in new List<int>(GetEquipped(meta)))
				{
					if (index >= 0 && index < meta.cosmeticAssets.Count && meta.cosmeticAssets[index] != null)
					{
						meta.CosmeticUnequip(meta.cosmeticAssets[index], _isPreview: false);
					}
				}

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

				int equippedCount = 0;
				foreach (var pair in byType)
				{
					CosmeticTypeAsset typeAsset = (int)pair.Key < meta.cosmeticTypeAssets.Count ? meta.cosmeticTypeAssets[(int)pair.Key] : null;
					if (typeAsset != null && typeAsset.meshSwitch)
					{
						continue;
					}
					bool multi = typeAsset != null && typeAsset.canEquipMultiple;
					int rolls = multi ? 2 : 1;
					for (int r = 0; r < rolls; r++)
					{
						CosmeticAsset pick = pair.Value[UnityEngine.Random.Range(0, pair.Value.Count)];
						if (meta.CosmeticEquip(pick, _isPreview: false))
						{
							equippedCount++;
						}
					}
				}

				int[] colors = EnsureColorArray(meta);
				if (randomizeColors && meta.colors != null && meta.colors.Count > 0)
				{
					for (int i = 0; i < colors.Length; i++)
					{
						colors[i] = UnityEngine.Random.Range(0, meta.colors.Count);
					}
					SetMetaColors(meta, colors);
				}

				ApplyLocalCosmetics(sync: true, forced: true);
				if (persistSave)
				{
					meta.Save();
				}
				LastStatus = "Random outfit: " + equippedCount + " pieces" + (randomizeColors ? " + colors" : string.Empty);
				return equippedCount > 0;
			}
			catch (Exception ex)
			{
				LastStatus = "RandomizeOutfit failed: " + ex.Message;
				return false;
			}
		}

		public static void TickLiveRandom()
		{
			if (!LiveRandom)
			{
				return;
			}
			if (Time.time < _nextOutfit)
			{
				return;
			}
			_nextOutfit = Time.time + Mathf.Max(0.45f, LiveRandomInterval);
			RandomizeOutfit(randomizeColors: !RainbowMode, persistSave: false);
		}

		public static void TickRainbow()
		{
			if (!RainbowMode)
			{
				return;
			}
			MetaManager meta = MetaManager.instance;
			if (meta == null || meta.colors == null || meta.colors.Count == 0)
			{
				return;
			}
			float hue = Mathf.Repeat(Time.time * Mathf.Max(0.05f, RainbowSpeed), 1f);
			Color rgb = Color.HSVToRGB(hue, 1f, 1f);
			int palette = NearestPaletteIndex(meta, rgb);
			if (Time.time >= _nextColorSync)
			{
				_nextColorSync = Time.time + 0.45f;
				ApplyPaletteColor(palette, sync: true);
			}
			foreach (PlayerCosmetics pc in EnumerateLocalCosmetics())
			{
				TintMaterials(pc, rgb);
			}
		}

		public static bool ApplyPaletteColor(int colorId, bool sync)
		{
			try
			{
				MetaManager meta = MetaManager.instance;
				if (meta == null || meta.colors == null || meta.colors.Count == 0)
				{
					LastStatus = "color palette not ready";
					return false;
				}
				colorId = Mathf.Clamp(colorId, 0, meta.colors.Count - 1);
				int[] colors = EnsureColorArray(meta);
				for (int i = 0; i < colors.Length; i++)
				{
					colors[i] = colorId;
				}
				SetMetaColors(meta, colors);
				bool applied = false;
				foreach (PlayerCosmetics pc in EnumerateLocalCosmetics())
				{
					pc.SetupColors(sync, colors);
					applied = true;
				}
				LastStatus = applied ? ("Color " + colorId + " (" + ColorName(meta, colorId) + ")") : "no local PlayerCosmetics";
				return applied;
			}
			catch (Exception ex)
			{
				LastStatus = "color: " + ex.Message;
				return false;
			}
		}

		public static Dictionary<int, string> GetPaletteNames()
		{
			var map = new Dictionary<int, string>();
			try
			{
				MetaManager meta = MetaManager.instance;
				if (meta != null && meta.colors != null && meta.colors.Count > 0)
				{
					for (int i = 0; i < meta.colors.Count; i++)
					{
						map[i] = ColorName(meta, i);
					}
					return map;
				}
			}
			catch
			{
			}
			for (int i = 0; i <= 35; i++)
			{
				map[i] = LanguageManager.GetColorName(i);
			}
			return map;
		}

		public static int AddTokens(int count)
		{
			try
			{
				MetaManager meta = MetaManager.instance;
				if (meta == null)
				{
					LastStatus = "MetaManager missing";
					return 0;
				}
				if (_tokensField == null)
				{
					_tokensField = typeof(MetaManager).GetField("cosmeticTokens", InstAll);
				}
				List<int> tokens = _tokensField?.GetValue(meta) as List<int>;
				if (tokens == null)
				{
					LastStatus = "no token list";
					return 0;
				}
				count = Mathf.Clamp(count, 1, 20);
				SemiFunc.Rarity[] rarities =
				{
					SemiFunc.Rarity.Common,
					SemiFunc.Rarity.Uncommon,
					SemiFunc.Rarity.Rare,
					SemiFunc.Rarity.UltraRare
				};
				for (int i = 0; i < count; i++)
				{
					tokens.Add((int)rarities[i % rarities.Length]);
				}
				meta.Save();
				RefreshTokenUi();
				LastStatus = "tokens +" + count + " (now " + tokens.Count + ")";
				return count;
			}
			catch (Exception ex)
			{
				LastStatus = "tokens: " + ex.Message;
				return 0;
			}
		}

		public static PlayerCosmetics GetLocalPlayerCosmetics()
		{
			foreach (PlayerCosmetics pc in EnumerateLocalCosmetics())
			{
				if (pc != null)
				{
					return pc;
				}
			}
			return null;
		}

		private static IEnumerable<PlayerCosmetics> EnumerateLocalCosmetics()
		{
			PlayerAvatar avatar = null;
			try { avatar = SemiFunc.PlayerAvatarLocal(); } catch { }
			if (avatar == null)
			{
				try { avatar = SemiFunc.PlayerGetLocal(); } catch { }
			}
			if (avatar != null)
			{
				if (avatar.playerCosmetics != null)
				{
					yield return avatar.playerCosmetics;
				}
				if (_deathHeadField == null)
				{
					_deathHeadField = typeof(PlayerAvatar).GetField("playerDeathHead", InstAll);
				}
				PlayerDeathHead head = _deathHeadField?.GetValue(avatar) as PlayerDeathHead;
				if (head != null && head.playerCosmetics != null)
				{
					yield return head.playerCosmetics;
				}
			}
			if (PlayerAvatarMenu.instance != null)
			{
				if (_menuCosmeticsField == null)
				{
					_menuCosmeticsField = typeof(PlayerAvatarMenu).GetField("playerCosmetics", InstAll);
				}
				PlayerCosmetics menuPc = _menuCosmeticsField?.GetValue(PlayerAvatarMenu.instance) as PlayerCosmetics;
				if (menuPc != null)
				{
					yield return menuPc;
				}
			}
		}

		private static void ApplyLocalCosmetics(bool sync, bool forced)
		{
			try
			{
				if (GameplayManager.instance != null)
				{
					SetMember(GameplayManager.instance, "cosmetics", true);
				}
			}
			catch
			{
			}
			List<int> equipped = null;
			try
			{
				equipped = new List<int>(GetEquipped(MetaManager.instance));
			}
			catch
			{
			}
			try
			{
				MetaManager.instance?.CosmeticPlayerUpdateLocal(sync, forced);
			}
			catch
			{
			}
			foreach (PlayerCosmetics pc in EnumerateLocalCosmetics())
			{
				if (equipped != null)
				{
					pc.SetupCosmetics(sync, forced, equipped);
				}
				else
				{
					pc.SetupCosmetics(sync, forced);
				}
				int[] colors = GetColors(MetaManager.instance);
				if (colors != null)
				{
					pc.SetupColors(sync, colors);
				}
			}
		}

		private static void SetMember(object instance, string name, object value)
		{
			if (instance == null)
			{
				return;
			}
			FieldInfo field = instance.GetType().GetField(name, InstAll);
			if (field != null)
			{
				field.SetValue(instance, value);
			}
		}

		private static void TintMaterials(PlayerCosmetics pc, Color rgb)
		{
			if (pc == null)
			{
				return;
			}
			if (_materialsField == null)
			{
				_materialsField = typeof(PlayerCosmetics).GetField("playerMaterials", InstAll);
			}
			List<PlayerMaterial> mats = _materialsField?.GetValue(pc) as List<PlayerMaterial>;
			if (mats == null)
			{
				return;
			}
			if (_albedoId == 0)
			{
				_albedoId = Shader.PropertyToID("_AlbedoColor");
				_emissionId = Shader.PropertyToID("_EmissionColor");
				_fresnelId = Shader.PropertyToID("_FresnelColor");
			}
			foreach (PlayerMaterial mat in mats)
			{
				if (mat == null || !mat.tintable)
				{
					continue;
				}
				try
				{
					mat.Setup();
				}
				catch
				{
				}
				Renderer renderer = mat.GetComponent<Renderer>();
				if (renderer == null)
				{
					continue;
				}
				Material material = renderer.material;
				if (material == null)
				{
					continue;
				}
				Color a = material.GetColor(_albedoId);
				material.SetColor(_albedoId, new Color(rgb.r, rgb.g, rgb.b, a.a));
				Color e = material.GetColor(_emissionId);
				material.SetColor(_emissionId, new Color(rgb.r, rgb.g, rgb.b, e.a));
				if (mat.tintFresnel)
				{
					material.SetColor(_fresnelId, new Color(rgb.r, rgb.g, rgb.b, 1f));
				}
			}
		}

		private static int NearestPaletteIndex(MetaManager meta, Color rgb)
		{
			int best = 0;
			float bestD = float.MaxValue;
			for (int i = 0; i < meta.colors.Count; i++)
			{
				if (meta.colors[i] == null)
				{
					continue;
				}
				Color c = meta.colors[i].color;
				float d = (c.r - rgb.r) * (c.r - rgb.r) + (c.g - rgb.g) * (c.g - rgb.g) + (c.b - rgb.b) * (c.b - rgb.b);
				if (d < bestD)
				{
					bestD = d;
					best = i;
				}
			}
			return best;
		}

		private static string ColorName(MetaManager meta, int index)
		{
			try
			{
				if (index >= 0 && index < meta.colors.Count && meta.colors[index] != null)
				{
					string n = meta.colors[index].colorName;
					if (!string.IsNullOrEmpty(n))
					{
						return n;
					}
				}
			}
			catch
			{
			}
			return LanguageManager.GetColorName(index);
		}

		private static List<int> GetUnlocked(MetaManager meta)
		{
			if (_unlocksField == null)
			{
				_unlocksField = typeof(MetaManager).GetField("cosmeticUnlocks", InstAll);
			}
			return _unlocksField?.GetValue(meta) as List<int> ?? new List<int>();
		}

		private static List<int> GetEquipped(MetaManager meta)
		{
			if (_equippedField == null)
			{
				_equippedField = typeof(MetaManager).GetField("cosmeticEquipped", InstAll);
			}
			return _equippedField?.GetValue(meta) as List<int> ?? new List<int>();
		}

		private static int[] GetColors(MetaManager meta)
		{
			if (_colorsField == null)
			{
				_colorsField = typeof(MetaManager).GetField("colorsEquipped", InstAll);
			}
			return _colorsField?.GetValue(meta) as int[];
		}

		private static int[] EnsureColorArray(MetaManager meta)
		{
			int[] colors = GetColors(meta);
			int needed = Enum.GetValues(typeof(SemiFunc.CosmeticType)).Length;
			if (colors == null || colors.Length < needed)
			{
				int[] next = new int[needed];
				if (colors != null)
				{
					Array.Copy(colors, next, Math.Min(colors.Length, needed));
				}
				colors = next;
				_colorsField?.SetValue(meta, colors);
			}
			return colors;
		}

		private static void SetMetaColors(MetaManager meta, int[] colors)
		{
			for (int i = 0; i < colors.Length; i++)
			{
				meta.CosmeticColorSet(i, colors[i]);
			}
		}

		private static void RefreshTokenUi()
		{
			try
			{
				if (CosmeticTokenUI.instance != null)
				{
					CosmeticTokenUI.instance.Setup();
				}
			}
			catch
			{
			}
		}
	}
}
