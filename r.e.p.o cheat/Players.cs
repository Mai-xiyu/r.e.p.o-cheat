using System;
using System.Collections.Generic;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

internal static class Players
{
	public static object playerHealthInstance;

	public static object playerMaxHealthInstance;

	public static void HealPlayer(object targetPlayer, int healAmount, string playerName)
	{
		if (targetPlayer == null)
		{
			return;
		}
		try
		{
			FieldInfo field = targetPlayer.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				return;
			}
			object value = field.GetValue(targetPlayer);
			PhotonView val = (PhotonView)((value is PhotonView) ? value : null);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			FieldInfo field2 = targetPlayer.GetType().GetField("playerHealth", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field2 == null)
			{
				return;
			}
			object value2 = field2.GetValue(targetPlayer);
			if (value2 == null)
			{
				return;
			}
			Type type = value2.GetType();
			MethodInfo method = type.GetMethod("Heal", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method != null)
			{
				method.Invoke(value2, new object[2] { healAmount, true });
			}
			if (!PhotonNetwork.IsConnected || !((Object)(object)val != (Object)null))
			{
				return;
			}
			FieldInfo field3 = type.GetField("currentHealth", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field3 != null)
			{
				_ = (int)field3.GetValue(value2);
			}
			int playerMaxHealth = GetPlayerMaxHealth(value2);
			val.RPC("UpdateHealthRPC", (RpcTarget)3, new object[3] { playerMaxHealth, playerMaxHealth, true });
			try
			{
				val.RPC("HealRPC", (RpcTarget)3, new object[2] { healAmount, true });
			}
			catch
			{
			}
		}
		catch (Exception)
		{
		}
	}

	public static void DamagePlayer(object targetPlayer, int damageAmount, string playerName)
	{
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		if (targetPlayer == null)
		{
			return;
		}
		try
		{
			FieldInfo field = targetPlayer.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				return;
			}
			object value = field.GetValue(targetPlayer);
			PhotonView val = (PhotonView)((value is PhotonView) ? value : null);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			FieldInfo field2 = targetPlayer.GetType().GetField("playerHealth", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field2 == null)
			{
				return;
			}
			object value2 = field2.GetValue(targetPlayer);
			if (value2 == null)
			{
				return;
			}
			MethodInfo method = value2.GetType().GetMethod("Hurt", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method != null)
			{
				method.Invoke(value2, new object[3] { damageAmount, true, -1 });
			}
			if (!PhotonNetwork.IsConnected || !((Object)(object)val != (Object)null))
			{
				return;
			}
			GetPlayerMaxHealth(value2);
			val.RPC("HurtOtherRPC", (RpcTarget)3, new object[4]
			{
				damageAmount,
				Vector3.zero,
				false,
				-1
			});
			try
			{
				val.RPC("HurtRPC", (RpcTarget)3, new object[3] { damageAmount, true, -1 });
			}
			catch
			{
			}
		}
		catch (Exception)
		{
		}
	}

	internal static void ReviveSelectedPlayer(int selectedPlayerIndex, List<object> playerList, List<string> playerNames)
	{
		if (selectedPlayerIndex >= 0 && selectedPlayerIndex < playerList.Count && selectedPlayerIndex < playerNames.Count)
		{
			object obj = playerList[selectedPlayerIndex];
			string playerName = playerNames[selectedPlayerIndex];
			if (obj != null)
			{
				ReviveSelectedPlayer(obj, playerList, playerName);
			}
		}
	}

	public static void ReviveSelectedPlayer(object selectedPlayer, List<object> playerList, string playerName)
	{
		if (selectedPlayer == null)
		{
			return;
		}
		try
		{
			FieldInfo field = selectedPlayer.GetType().GetField("playerDeathHead", BindingFlags.Instance | BindingFlags.Public);
			if (field != null)
			{
				object value = field.GetValue(selectedPlayer);
				if (value != null)
				{
					FieldInfo field2 = value.GetType().GetField("inExtractionPoint", BindingFlags.Instance | BindingFlags.NonPublic);
					MethodInfo method = value.GetType().GetMethod("Revive", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (field2 != null)
					{
						field2.SetValue(value, true);
					}
					if (method != null)
					{
						method.Invoke(value, null);
					}
				}
			}
			FieldInfo field3 = selectedPlayer.GetType().GetField("playerHealth", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (!(field3 != null))
			{
				return;
			}
			object value2 = field3.GetValue(selectedPlayer);
			if (value2 != null)
			{
				FieldInfo field4 = value2.GetType().GetField("health", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				int playerMaxHealth = GetPlayerMaxHealth(value2);
				if (field4 != null)
				{
					field4.SetValue(value2, playerMaxHealth);
				}
				else
				{
					HealPlayer(selectedPlayer, playerMaxHealth, playerName);
				}
				GetPlayerHealth(selectedPlayer);
			}
		}
		catch (Exception)
		{
		}
	}

	internal static void KillSelectedPlayer(int selectedPlayerIndex, List<object> playerList, List<string> playerNames)
	{
		if (selectedPlayerIndex >= 0 && selectedPlayerIndex < playerList.Count && selectedPlayerIndex < playerNames.Count)
		{
			object obj = playerList[selectedPlayerIndex];
			string playerName = playerNames[selectedPlayerIndex];
			if (obj != null)
			{
				KillSelectedPlayer(obj, playerList, playerName);
			}
		}
	}

	public static void KillSelectedPlayer(object selectedPlayer, List<object> playerList, string playerName)
	{
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		if (selectedPlayer == null)
		{
			return;
		}
		try
		{
			FieldInfo field = selectedPlayer.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				return;
			}
			object value = field.GetValue(selectedPlayer);
			PhotonView val = (PhotonView)((value is PhotonView) ? value : null);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			FieldInfo field2 = selectedPlayer.GetType().GetField("playerHealth", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field2 == null)
			{
				return;
			}
			object value2 = field2.GetValue(selectedPlayer);
			if (value2 == null)
			{
				return;
			}
			Type type = value2.GetType();
			MethodInfo method = type.GetMethod("Death", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method == null)
			{
				return;
			}
			method.Invoke(value2, null);
			FieldInfo field3 = type.GetField("playerAvatar", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field3 != null)
			{
				object value3 = field3.GetValue(value2);
				if (value3 != null)
				{
					MethodInfo method2 = value3.GetType().GetMethod("PlayerDeath", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (method2 != null)
					{
						method2.Invoke(value3, new object[1] { -1 });
					}
				}
			}
			if (PhotonNetwork.IsConnected && (Object)(object)val != (Object)null)
			{
				int playerMaxHealth = GetPlayerMaxHealth(value2);
				val.RPC("UpdateHealthRPC", (RpcTarget)3, new object[3] { 0, playerMaxHealth, true });
				try
				{
					val.RPC("PlayerDeathRPC", (RpcTarget)3, new object[1] { -1 });
				}
				catch
				{
				}
				val.RPC("HurtOtherRPC", (RpcTarget)3, new object[4]
				{
					9999,
					Vector3.zero,
					false,
					-1
				});
			}
		}
		catch (Exception)
		{
		}
	}

	public static void ForcePlayerTumble(float duration = 10f)
	{
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		if (Hax2.selectedPlayerIndex < 0 || Hax2.selectedPlayerIndex >= Hax2.playerList.Count)
		{
			Debug.Log((object)"玩家索引无效！");
			return;
		}
		object obj = Hax2.playerList[Hax2.selectedPlayerIndex];
		if (obj == null)
		{
			Debug.Log((object)"所选玩家为空！");
			return;
		}
		try
		{
			Debug.Log((object)$"正在让 {Hax2.playerNames[Hax2.selectedPlayerIndex]} 翻滚 {duration} 秒。");
			FieldInfo field = obj.GetType().GetField("tumble", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				Debug.Log((object)"未找到 PlayerTumble 字段！");
				return;
			}
			object value = field.GetValue(obj);
			PlayerTumble val = (PlayerTumble)((value is PlayerTumble) ? value : null);
			if ((Object)(object)val == (Object)null)
			{
				Debug.Log((object)"PlayerTumble 实例为 null！");
				return;
			}
			FieldInfo field2 = ((object)val).GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field2 == null)
			{
				Debug.Log((object)"在 PlayerTumble 上未找到 PhotonView 字段！");
				return;
			}
			object value2 = field2.GetValue(val);
			PhotonView val2 = (PhotonView)((value2 is PhotonView) ? value2 : null);
			if ((Object)(object)val2 == (Object)null)
			{
				Debug.Log((object)"PhotonView 无效！");
				return;
			}
			val2.RPC("TumbleSetRPC", (RpcTarget)0, new object[2] { true, false });
			val2.RPC("TumbleOverrideTimeRPC", (RpcTarget)0, new object[1] { duration });
			val2.RPC("TumbleForceRPC", (RpcTarget)0, new object[1] { (object)new Vector3(10f, 50f, 0f) });
			val2.RPC("TumbleTorqueRPC", (RpcTarget)0, new object[1] { (object)new Vector3(0f, 0f, 2000f) });
			Debug.Log((object)$"已让 {Hax2.playerNames[Hax2.selectedPlayerIndex]} 翻滚 {duration} 秒。");
		}
		catch (Exception ex)
		{
			Debug.Log((object)("让 " + Hax2.playerNames[Hax2.selectedPlayerIndex] + " 翻滚时出错：" + ex.Message));
		}
	}

	public static int GetPlayerHealth(object player)
	{
		try
		{
			FieldInfo field = player.GetType().GetField("playerHealth", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				return 100;
			}
			object value = field.GetValue(player);
			if (value == null)
			{
				return 100;
			}
			FieldInfo field2 = value.GetType().GetField("health", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field2 == null)
			{
				return 100;
			}
			return (int)field2.GetValue(value);
		}
		catch (Exception)
		{
			return 100;
		}
	}

	public static int GetPlayerMaxHealth(object playerHealthInstance)
	{
		if (playerHealthInstance == null)
		{
			return 100;
		}
		FieldInfo field = playerHealthInstance.GetType().GetField("maxHealth", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (!(field != null))
		{
			return 100;
		}
		return (int)field.GetValue(playerHealthInstance);
	}

	/// <summary>
	/// 复活自己 - 无需主机权限
	/// 通过RPC同步确保所有客户端看到复活效果
	/// </summary>
	public static void ReviveSelf()
	{
		try
		{
			// 找到本地玩家的 PlayerAvatar
			List<PlayerAvatar> players = SemiFunc.PlayerGetList();
			if (players == null || players.Count == 0)
			{
				Debug.Log((object)"[ReviveSelf] 没有找到玩家列表");
				return;
			}

			object localPlayer = null;
			foreach (PlayerAvatar avatar in players)
			{
				FieldInfo pvField = ((object)avatar).GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (pvField == null) continue;
				object pvObj = pvField.GetValue(avatar);
				PhotonView pv = (PhotonView)((pvObj is PhotonView) ? pvObj : null);
				if ((Object)(object)pv != (Object)null && pv.IsMine)
				{
					localPlayer = avatar;
					break;
				}
			}

			if (localPlayer == null)
			{
				Debug.Log((object)"[ReviveSelf] 无法找到本地玩家");
				return;
			}

			// 获取 playerDeathHead
			FieldInfo deathHeadField = localPlayer.GetType().GetField("playerDeathHead", BindingFlags.Instance | BindingFlags.Public);
			if (deathHeadField != null)
			{
				object deathHead = deathHeadField.GetValue(localPlayer);
				if (deathHead != null)
				{
					// 设置 inExtractionPoint = true（跳过撤离点检查）
					FieldInfo extractField = deathHead.GetType().GetField("inExtractionPoint", BindingFlags.Instance | BindingFlags.NonPublic);
					if (extractField != null)
					{
						extractField.SetValue(deathHead, true);
					}

					// 调用 Revive() 方法
					MethodInfo reviveMethod = deathHead.GetType().GetMethod("Revive", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (reviveMethod != null)
					{
						reviveMethod.Invoke(deathHead, null);
					}

					// 通过 RPC 同步 Revive（让其他客户端也看到）
					FieldInfo deathHeadPvField = deathHead.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (deathHeadPvField != null)
					{
						object dpvObj = deathHeadPvField.GetValue(deathHead);
						PhotonView dpv = (PhotonView)((dpvObj is PhotonView) ? dpvObj : null);
						if ((Object)(object)dpv != (Object)null)
						{
							try { dpv.RPC("ReviveRPC", (RpcTarget)0, Array.Empty<object>()); } catch { }
						}
					}
				}
			}

			// 恢复血量
			FieldInfo healthField = localPlayer.GetType().GetField("playerHealth", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (healthField != null)
			{
				object healthInstance = healthField.GetValue(localPlayer);
				if (healthInstance != null)
				{
					int maxHP = GetPlayerMaxHealth(healthInstance);
					// 设置本地血量
					FieldInfo hpField = healthInstance.GetType().GetField("health", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (hpField != null)
					{
						hpField.SetValue(healthInstance, maxHP);
					}
					// RPC 同步血量
					FieldInfo pvField = localPlayer.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (pvField != null)
					{
						object pvObj = pvField.GetValue(localPlayer);
						PhotonView pv = (PhotonView)((pvObj is PhotonView) ? pvObj : null);
						if ((Object)(object)pv != (Object)null && PhotonNetwork.IsConnected)
						{
							try { pv.RPC("UpdateHealthRPC", (RpcTarget)0, new object[] { maxHP, maxHP, true }); } catch { }
							try { pv.RPC("HealRPC", (RpcTarget)0, new object[] { maxHP, true }); } catch { }
						}
					}
				}
			}

			Debug.Log((object)"[ReviveSelf] 已复活自己");
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("[ReviveSelf] 出错: " + ex.Message));
		}
	}

	/// <summary>
	/// 治疗复活队友 - 通过RPC同步，不需要主机权限
	/// 先恢复血量（通过HealPlayer），然后尝试复活死亡头/玩家
	/// </summary>
	public static void HealRevivePlayer(object targetPlayer, string playerName)
	{
		if (targetPlayer == null) return;

		try
		{
			// 1. 先尝试复活（如果已死亡）
			FieldInfo deathHeadField = targetPlayer.GetType().GetField("playerDeathHead", BindingFlags.Instance | BindingFlags.Public);
			if (deathHeadField != null)
			{
				object deathHead = deathHeadField.GetValue(targetPlayer);
				if (deathHead != null)
				{
					// 设置 inExtractionPoint = true
					FieldInfo extractField = deathHead.GetType().GetField("inExtractionPoint", BindingFlags.Instance | BindingFlags.NonPublic);
					if (extractField != null)
					{
						extractField.SetValue(deathHead, true);
					}
					// 调用 Revive()
					MethodInfo reviveMethod = deathHead.GetType().GetMethod("Revive", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (reviveMethod != null)
					{
						reviveMethod.Invoke(deathHead, null);
					}
					// RPC 同步
					FieldInfo dhPvField = deathHead.GetType().GetField("photonView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (dhPvField != null)
					{
						object dpvObj = dhPvField.GetValue(deathHead);
						PhotonView dpv = (PhotonView)((dpvObj is PhotonView) ? dpvObj : null);
						if ((Object)(object)dpv != (Object)null)
						{
							try { dpv.RPC("ReviveRPC", (RpcTarget)0, Array.Empty<object>()); } catch { }
						}
					}
				}
			}

			// 2. 恢复满血 (通过已有的 HealPlayer，自带 RPC 同步)
			FieldInfo healthField = targetPlayer.GetType().GetField("playerHealth", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (healthField != null)
			{
				object healthInstance = healthField.GetValue(targetPlayer);
				if (healthInstance != null)
				{
					int maxHP = GetPlayerMaxHealth(healthInstance);
					HealPlayer(targetPlayer, maxHP, playerName);
				}
			}

			Debug.Log((object)("[HealRevive] 已治疗复活: " + playerName));
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("[HealRevive] 出错: " + ex.Message));
		}
	}
}
