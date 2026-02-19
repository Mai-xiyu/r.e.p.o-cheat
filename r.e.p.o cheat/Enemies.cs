using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

namespace r.e.p.o_cheat;

public static class Enemies
{
	private static Dictionary<Enemy, int> enemyMaxHealthCache = new Dictionary<Enemy, int>();

	public static List<Enemy> enemyList = new List<Enemy>();

	public static void KillSelectedEnemy(int selectedEnemyIndex, List<Enemy> enemyList, List<string> enemyNames)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		if (selectedEnemyIndex < 0 || selectedEnemyIndex >= enemyList.Count)
		{
			return;
		}
		Enemy val = enemyList[selectedEnemyIndex];
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		try
		{
			FieldInfo field = ((object)val).GetType().GetField("Health", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				object value = field.GetValue(val);
				if (value != null)
				{
					MethodInfo method = value.GetType().GetMethod("Hurt", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (method != null)
					{
						method.Invoke(value, new object[2]
						{
							9999,
							Vector3.zero
						});
					}
				}
			}
			DebugCheats.UpdateEnemyList();
		}
		catch (Exception)
		{
		}
	}

	public static void KillAllEnemies()
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		List<Enemy> list = DebugCheats.enemyList;
		if (list == null || list.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (Enemy item in new List<Enemy>(list))
		{
			if ((Object)(object)item == (Object)null)
			{
				continue;
			}
			try
			{
				FieldInfo field = ((object)item).GetType().GetField("Health", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (!(field != null))
				{
					continue;
				}
				object value = field.GetValue(item);
				if (value != null)
				{
					MethodInfo method = value.GetType().GetMethod("Hurt", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (method != null)
					{
						method.Invoke(value, new object[2]
						{
							9999,
							Vector3.zero
						});
						num++;
					}
				}
			}
			catch (Exception)
			{
			}
		}
		DebugCheats.UpdateEnemyList();
	}

	public static void TeleportEnemyToMe(int selectedEnemyIndex, List<Enemy> enemyList, List<string> enemyNames)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		if (selectedEnemyIndex < 0 || selectedEnemyIndex >= enemyList.Count)
		{
			return;
		}
		Enemy val = enemyList[selectedEnemyIndex];
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		try
		{
			GameObject localPlayer = DebugCheats.GetLocalPlayer();
			if ((Object)(object)localPlayer == (Object)null)
			{
				return;
			}
			Vector3 forward = localPlayer.transform.forward;
			Vector3 val2 = localPlayer.transform.position + forward * 1f + Vector3.up * 1.5f;
			PhotonView component = ((Component)val).GetComponent<PhotonView>();
			if (PhotonNetwork.IsConnected && (Object)(object)component != (Object)null && !component.IsMine)
			{
				component.RequestOwnership();
			}
			FieldInfo field = ((object)val).GetType().GetField("NavMeshAgent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			object obj = null;
			if (field != null)
			{
				obj = field.GetValue(val);
				if (obj != null)
				{
					PropertyInfo property = obj.GetType().GetProperty("enabled", BindingFlags.Instance | BindingFlags.Public);
					if (property != null)
					{
						property.SetValue(obj, false);
					}
				}
			}
			((Component)val).transform.position = val2;
			_ = ((Component)val).transform.position;
			if (PhotonNetwork.IsConnected && (Object)(object)component != (Object)null)
			{
				MethodInfo method = ((object)val).GetType().GetMethod("EnemyTeleported", BindingFlags.Instance | BindingFlags.Public);
				if (method != null)
				{
					method.Invoke(val, new object[1] { val2 });
				}
			}
			if (obj != null)
			{
				MonoBehaviour val3 = (MonoBehaviour)(object)val;
				if ((Object)(object)val3 != (Object)null)
				{
					val3.StartCoroutine(ReEnableNavMeshAgent(obj, 2f));
				}
			}
			GameObject val4 = ((val != null) ? ((Component)val).gameObject : null);
			if ((Object)(object)val4 != (Object)null && val4.activeInHierarchy)
			{
				val4.SetActive(false);
				val4.SetActive(true);
			}
		}
		catch (Exception)
		{
		}
	}

	public static void TeleportEnemyToPlayer(int selectedEnemyIndex, List<Enemy> enemyList, List<string> enemyNames, int targetPlayerIndex, List<object> playerList, List<string> playerNames)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		if (selectedEnemyIndex < 0 || selectedEnemyIndex >= enemyList.Count)
		{
			return;
		}
		Enemy val = enemyList[selectedEnemyIndex];
		if ((Object)(object)val == (Object)null || targetPlayerIndex < 0 || targetPlayerIndex >= playerList.Count)
		{
			return;
		}
		object obj = playerList[targetPlayerIndex];
		if (obj == null)
		{
			return;
		}
		try
		{
			GameObject val2 = (GameObject)((obj is GameObject) ? obj : null);
			Vector3 val3;
			if (val2 != null)
			{
				val3 = val2.transform.position + Vector3.up * 1.5f;
			}
			else
			{
				MonoBehaviour val4 = (MonoBehaviour)((obj is MonoBehaviour) ? obj : null);
				if (val4 != null)
				{
					val3 = ((Component)val4).transform.position + Vector3.up * 1.5f;
				}
				else
				{
					FieldInfo field = obj.GetType().GetField("transform", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (!(field != null))
					{
						return;
					}
					object value = field.GetValue(obj);
					Transform val5 = (Transform)((value is Transform) ? value : null);
					if (!((Object)(object)val5 != (Object)null))
					{
						return;
					}
					val3 = val5.position + Vector3.up * 1.5f;
				}
			}
			PhotonView component = ((Component)val).GetComponent<PhotonView>();
			if (PhotonNetwork.IsConnected && (Object)(object)component != (Object)null && !component.IsMine)
			{
				component.RequestOwnership();
			}
			FieldInfo field2 = ((object)val).GetType().GetField("NavMeshAgent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			object obj2 = null;
			if (field2 != null)
			{
				obj2 = field2.GetValue(val);
				if (obj2 != null)
				{
					PropertyInfo property = obj2.GetType().GetProperty("enabled", BindingFlags.Instance | BindingFlags.Public);
					if (property != null)
					{
						property.SetValue(obj2, false);
					}
				}
			}
			((Component)val).transform.position = val3;
			if (PhotonNetwork.IsConnected && (Object)(object)component != (Object)null)
			{
				MethodInfo method = ((object)val).GetType().GetMethod("EnemyTeleported", BindingFlags.Instance | BindingFlags.Public);
				if (method != null)
				{
					method.Invoke(val, new object[1] { val3 });
				}
			}
			if (obj2 != null)
			{
				MonoBehaviour val6 = (MonoBehaviour)(object)val;
				if ((Object)(object)val6 != (Object)null)
				{
					val6.StartCoroutine(ReEnableNavMeshAgent(obj2, 2f));
				}
			}
			GameObject val7 = ((val != null) ? ((Component)val).gameObject : null);
			if ((Object)(object)val7 != (Object)null && val7.activeInHierarchy)
			{
				val7.SetActive(false);
				val7.SetActive(true);
			}
		}
		catch (Exception)
		{
		}
	}

	private static IEnumerator ReEnableNavMeshAgent(object navMeshAgent, float delay)
	{
		yield return (object)new WaitForSeconds(delay);
		PropertyInfo property = navMeshAgent.GetType().GetProperty("enabled", BindingFlags.Instance | BindingFlags.Public);
		if (property != null)
		{
			property.SetValue(navMeshAgent, true);
		}
	}

	public static int GetEnemyHealth(Enemy enemy)
	{
		try
		{
			FieldInfo field = ((object)enemy).GetType().GetField("Health", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				return -1;
			}
			object value = field.GetValue(enemy);
			if (value == null)
			{
				return -1;
			}
			FieldInfo field2 = value.GetType().GetField("healthCurrent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field2 == null)
			{
				return -1;
			}
			return (int)field2.GetValue(value);
		}
		catch (Exception)
		{
			return -1;
		}
	}

	public static int GetEnemyMaxHealth(Enemy enemy)
	{
		if (enemyMaxHealthCache.TryGetValue(enemy, out var value))
		{
			return value;
		}
		int enemyHealth = GetEnemyHealth(enemy);
		int num = ((enemyHealth > 0) ? enemyHealth : 100);
		enemyMaxHealthCache[enemy] = num;
		return num;
	}

	public static float GetEnemyHealthPercentage(Enemy enemy)
	{
		int enemyHealth = GetEnemyHealth(enemy);
		int enemyMaxHealth = GetEnemyMaxHealth(enemy);
		if (enemyHealth < 0 || enemyMaxHealth <= 0)
		{
			return -1f;
		}
		return (float)enemyHealth / (float)enemyMaxHealth;
	}

	// === 冻结所有敌人 ===
	public static bool freezeAllEnemies = false;
	private static Dictionary<Enemy, bool> savedNavMeshState = new Dictionary<Enemy, bool>();

	public static void FreezeAllEnemies()
	{
		freezeAllEnemies = true;
		try
		{
			List<EnemyParent> list = EnemyDirector.instance?.enemiesSpawned;
			if (list == null) return;
			foreach (var ep in list)
			{
				if ((Object)(object)ep == (Object)null) continue;
				Enemy enemy = ((Component)ep).GetComponentInChildren<Enemy>();
				if ((Object)(object)enemy == (Object)null) continue;
				try
				{
					NavMeshAgent agent = ((Component)enemy).GetComponent<NavMeshAgent>();
					if (agent != null && agent.enabled)
					{
						savedNavMeshState[enemy] = true;
						agent.enabled = false;
					}
					// Also try to freeze Rigidbody
					Rigidbody rb = ((Component)enemy).GetComponent<Rigidbody>();
					if (rb != null) rb.isKinematic = true;
				}
				catch { }
			}
		}
		catch (Exception ex) { Debug.LogWarning("[Enemies] FreezeAll: " + ex.Message); }
	}

	public static void UnfreezeAllEnemies()
	{
		freezeAllEnemies = false;
		try
		{
			foreach (var kvp in savedNavMeshState)
			{
				if ((Object)(object)kvp.Key != (Object)null)
				{
					try
					{
						NavMeshAgent agent = ((Component)kvp.Key).GetComponent<NavMeshAgent>();
						if (agent != null) agent.enabled = true;
						Rigidbody rb = ((Component)kvp.Key).GetComponent<Rigidbody>();
						if (rb != null) rb.isKinematic = false;
					}
					catch { }
				}
			}
			savedNavMeshState.Clear();
		}
		catch (Exception ex) { Debug.LogWarning("[Enemies] UnfreezeAll: " + ex.Message); }
	}

	/// <summary>
	/// 每帧调用: 持续冻结新刷出的敌人
	/// </summary>
	public static void UpdateFreeze()
	{
		if (!freezeAllEnemies) return;
		try
		{
			List<EnemyParent> list = EnemyDirector.instance?.enemiesSpawned;
			if (list == null) return;
			foreach (var ep in list)
			{
				if ((Object)(object)ep == (Object)null) continue;
				Enemy enemy = ((Component)ep).GetComponentInChildren<Enemy>();
				if ((Object)(object)enemy == (Object)null) continue;
				if (savedNavMeshState.ContainsKey(enemy)) continue;
				try
				{
					NavMeshAgent agent = ((Component)enemy).GetComponent<NavMeshAgent>();
					if (agent != null && agent.enabled)
					{
						savedNavMeshState[enemy] = true;
						agent.enabled = false;
					}
					Rigidbody rb = ((Component)enemy).GetComponent<Rigidbody>();
					if (rb != null) rb.isKinematic = true;
				}
				catch { }
			}
		}
		catch { }
	}
}
