using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace r.e.p.o_cheat;

public static class AntiCrashProtection
{
	private static Dictionary<string, List<float>> rpcTimestamps = new Dictionary<string, List<float>>();

	private static Dictionary<string, bool> blockedRpcs = new Dictionary<string, bool>();

	private static float blockDuration = 30f;

	private static int rpcThreshold = 15;

	private static float timeWindow = 3f;

	/// <summary>
	/// 关卡生成期间必须通过的 RPC，使用更高的阈值以免误封
	/// </summary>
	private static readonly HashSet<string> _levelGenRpcs = new HashSet<string>
	{
		"ItemSetup", "NavMeshSetupRPC"
	};

	private static int levelGenThreshold = 500;

	public static bool ShouldBlockRpc(string rpcName)
	{
		if (blockedRpcs.TryGetValue(rpcName, out var value) && value)
		{
			Debug.Log((object)("Blocked malicious RPC: " + rpcName));
			return true;
		}
		float currentTime = Time.time;
		if (!rpcTimestamps.ContainsKey(rpcName))
		{
			rpcTimestamps[rpcName] = new List<float>();
		}
		rpcTimestamps[rpcName].Add(currentTime);
		rpcTimestamps[rpcName].RemoveAll((float timestamp) => currentTime - timestamp > timeWindow);
		int threshold = _levelGenRpcs.Contains(rpcName) ? levelGenThreshold : rpcThreshold;
		if (rpcTimestamps[rpcName].Count >= threshold)
		{
			Debug.LogWarning((object)$"RPC spam detected for {rpcName}! Blocking for {blockDuration} seconds.");
			blockedRpcs[rpcName] = true;
			MonoBehaviour val = Object.FindObjectOfType<MonoBehaviour>();
			if ((Object)(object)val != (Object)null)
			{
				val.StartCoroutine(UnblockRpcAfterDelay(rpcName, blockDuration));
			}
			return true;
		}
		return false;
	}

	private static IEnumerator UnblockRpcAfterDelay(string rpcName, float delay)
	{
		yield return (object)new WaitForSeconds(delay);
		blockedRpcs[rpcName] = false;
		Debug.Log((object)("Unblocked RPC: " + rpcName));
	}
}
