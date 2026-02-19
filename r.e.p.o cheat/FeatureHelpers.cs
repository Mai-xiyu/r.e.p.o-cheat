using System;
using System.Reflection;
using Photon.Pun;
using UnityEngine;

namespace r.e.p.o_cheat;

/// <summary>
/// 一键全升级工具
/// </summary>
public static class UpgradeHelper
{
    public static void MaxAllUpgrades()
    {
        try
        {
            string steamID = PlayerController.GetLocalPlayerSteamID();
            if (string.IsNullOrEmpty(steamID)) return;

            PhotonView pv = ((Component)UnityEngine.Object.FindObjectOfType<PunManager>()).GetComponent<PhotonView>();
            if (pv == null) return;

            pv.RPC("UpgradePlayerGrabStrengthRPC", (RpcTarget)3, new object[] { steamID, 30 });
            pv.RPC("UpgradePlayerThrowStrengthRPC", (RpcTarget)3, new object[] { steamID, 30 });
            pv.RPC("UpgradePlayerSprintSpeedRPC", (RpcTarget)3, new object[] { steamID, 30 });
            pv.RPC("UpgradePlayerGrabRangeRPC", (RpcTarget)3, new object[] { steamID, 30 });
            pv.RPC("UpgradePlayerExtraJumpRPC", (RpcTarget)3, new object[] { steamID, 30 });
            pv.RPC("UpgradePlayerTumbleLaunchRPC", (RpcTarget)3, new object[] { steamID, 20 });

            // Update UI slider values
            Hax2.sliderValueStrength = 30f;
            Hax2.oldSliderValue = 30f;
            Hax2.sliderValue = 30f;
            Hax2.grabRange = 30f;
            Hax2.throwStrength = 30f;
            Hax2.extraJumps = 30;
            Hax2.tumbleLaunch = 20f;

            Debug.Log("[UpgradeHelper] All upgrades maxed!");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[UpgradeHelper] Error: " + ex.Message);
        }
    }
}

/// <summary>
/// 全物品价值膨胀工具
/// </summary>
public static class ItemInflater
{
    public static void InflateAll(float targetValue = 99999f)
    {
        try
        {
            var items = DebugCheats.valuableObjects;
            if (items == null || items.Count == 0) return;

            int count = 0;
            foreach (object item in items)
            {
                if (item == null) continue;
                try
                {
                    // Check if it's a Unity object that was destroyed
                    UnityEngine.Object unityObj = item as UnityEngine.Object;
                    if (unityObj != null && unityObj == null) continue;

                    PhotonView pv = null;
                    if (item is Component comp)
                        pv = comp.GetComponent<PhotonView>();
                    else
                    {
                        var pvField = item.GetType().GetField("photonView",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (pvField != null)
                            pv = pvField.GetValue(item) as PhotonView;
                    }

                    if (pv != null)
                    {
                        pv.RPC("DollarValueSetRPC", (RpcTarget)0, new object[] { targetValue });
                        count++;
                    }
                }
                catch { }
            }
            Debug.Log($"[ItemInflater] Inflated {count} items to ${targetValue}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[ItemInflater] Error: " + ex.Message);
        }
    }

    public static void MultiplyAll(float multiplier = 10f)
    {
        try
        {
            var items = DebugCheats.valuableObjects;
            if (items == null || items.Count == 0) return;

            foreach (object item in items)
            {
                if (item == null) continue;
                try
                {
                    UnityEngine.Object unityObj = item as UnityEngine.Object;
                    if (unityObj != null && unityObj == null) continue;

                    FieldInfo valueField = item.GetType().GetField("dollarValueCurrent",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        ?? item.GetType().GetField("dollarValue",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    float currentValue = 0f;
                    if (valueField != null)
                        currentValue = Convert.ToSingle(valueField.GetValue(item));

                    float newValue = currentValue * multiplier;

                    PhotonView pv = null;
                    if (item is Component comp)
                        pv = comp.GetComponent<PhotonView>();
                    else
                    {
                        var pvField = item.GetType().GetField("photonView",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (pvField != null)
                            pv = pvField.GetValue(item) as PhotonView;
                    }

                    if (pv != null)
                        pv.RPC("DollarValueSetRPC", (RpcTarget)0, new object[] { newValue });
                }
                catch { }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[ItemInflater] Multiply error: " + ex.Message);
        }
    }
}

/// <summary>
/// 搬运目标归零工具
/// </summary>
public static class HaulGoalZero
{
    public static string statusMessage = "";

    public static void ZeroHaulGoal()
    {
        try
        {
            RoundDirector instance = RoundDirector.instance;
            if (instance == null)
            {
                statusMessage = L.T("haul.no_round");
                return;
            }

            bool found = false;
            // Search for haul/goal related fields via reflection
            string[] fieldNames = { "haulGoal", "extractionHaulGoal", "goalAmount",
                "shopHaulGoal", "currentHaulGoal", "totalHaulGoal", "requiredHaul" };

            foreach (string name in fieldNames)
            {
                FieldInfo field = typeof(RoundDirector).GetField(name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    if (field.FieldType == typeof(int))
                        field.SetValue(instance, 0);
                    else if (field.FieldType == typeof(float))
                        field.SetValue(instance, 0f);
                    found = true;
                    Debug.Log("[HaulGoal] Set " + name + " = 0");
                }
            }

            // Also try property search
            if (!found)
            {
                var allFields = typeof(RoundDirector).GetFields(
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (var f in allFields)
                {
                    string nameLower = f.Name.ToLower();
                    if (nameLower.Contains("haul") || nameLower.Contains("goal") ||
                        nameLower.Contains("target") || nameLower.Contains("quota"))
                    {
                        try
                        {
                            if (f.FieldType == typeof(int))
                            { f.SetValue(instance, 0); found = true; }
                            else if (f.FieldType == typeof(float))
                            { f.SetValue(instance, 0f); found = true; }
                            Debug.Log("[HaulGoal] Found and zeroed: " + f.Name + " (" + f.FieldType + ")");
                        }
                        catch { }
                    }
                }
            }

            statusMessage = found ? L.T("haul.zeroed") : L.T("haul.no_field");
        }
        catch (Exception ex)
        {
            statusMessage = L.T("haul.error", ex.Message);
            Debug.LogWarning("[HaulGoal] Error: " + ex.Message);
        }
    }
}

/// <summary>
/// 物品复制机
/// </summary>
public static class ItemDuplicator
{
    public static bool DuplicateHeldItem()
    {
        try
        {
            object pc = PlayerReflectionCache.PlayerControllerInstance;
            if (pc == null) return false;

            FieldInfo avatarField = pc.GetType().GetField("playerAvatarScript",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (avatarField == null) return false;
            object avatar = avatarField.GetValue(pc);
            if (avatar == null) return false;

            FieldInfo grabberField = avatar.GetType().GetField("physGrabber",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (grabberField == null) return false;
            object grabber = grabberField.GetValue(avatar);
            if (grabber == null) return false;

            FieldInfo grabbedField = grabber.GetType().GetField("grabbed",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (grabbedField == null || !(bool)grabbedField.GetValue(grabber)) return false;

            FieldInfo transformField = grabber.GetType().GetField("grabbedObjectTransform",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (transformField == null) return false;
            Transform grabbedTransform = transformField.GetValue(grabber) as Transform;
            if (grabbedTransform == null) return false;

            string objName = grabbedTransform.gameObject.name;
            if (objName.EndsWith("(Clone)"))
                objName = objName.Substring(0, objName.Length - "(Clone)".Length).Trim();

            GameObject localPlayer = DebugCheats.GetLocalPlayer();
            if (localPlayer == null) return false;
            Vector3 spawnPos = localPlayer.transform.position + localPlayer.transform.forward * 1.5f + Vector3.up;

            // Try spawning via ItemSpawner
            ItemSpawner.SpawnItem(objName, spawnPos);
            Debug.Log("[ItemDuplicator] Duplicated: " + objName);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[ItemDuplicator] Error: " + ex.Message);
            return false;
        }
    }
}

/// <summary>
/// 反作弊隐身模式
/// </summary>
public static class StealthMode
{
    public static bool isEnabled = false;

    public static void Apply()
    {
        try
        {
            GameObject localPlayer = DebugCheats.GetLocalPlayer();
            if (localPlayer == null) return;

            PhotonView pv = localPlayer.GetComponent<PhotonView>();
            if (pv == null)
            {
                // Try getting from PlayerAvatar
                var avatar = localPlayer.GetComponent<PlayerAvatar>();
                if (avatar != null)
                {
                    var pvField = avatar.GetType().GetField("photonView",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (pvField != null)
                        pv = pvField.GetValue(avatar) as PhotonView;
                }
            }

            if (pv == null) return;

            if (isEnabled)
            {
                pv.Synchronization = (ViewSynchronization)0; // Off
                Debug.Log("[Stealth] Network sync DISABLED — you are invisible");
            }
            else
            {
                pv.Synchronization = (ViewSynchronization)3; // UnreliableOnChange
                Debug.Log("[Stealth] Network sync RESTORED");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[Stealth] Error: " + ex.Message);
        }
    }
}

/// <summary>
/// 自动完成回合
/// </summary>
public static class AutoCompleteRound
{
    public static void Execute()
    {
        try
        {
            // Step 1: Activate all extraction points
            MiscFeatures.ForceActivateAllExtractionPoints();

            // Step 2: Teleport all items to nearest extraction point
            ExtractionPoint[] points = UnityEngine.Object.FindObjectsOfType<ExtractionPoint>();
            if (points.Length == 0)
            {
                Debug.LogWarning("[AutoComplete] No extraction points found");
                return;
            }

            GameObject localPlayer = DebugCheats.GetLocalPlayer();
            Vector3 refPos = localPlayer != null ? localPlayer.transform.position : Vector3.zero;

            // Find nearest extraction point
            ExtractionPoint nearest = null;
            float nearestDist = float.MaxValue;
            foreach (var ep in points)
            {
                float dist = Vector3.Distance(refPos, ((Component)ep).transform.position);
                if (dist < nearestDist) { nearestDist = dist; nearest = ep; }
            }

            if (nearest == null) return;
            Vector3 sellPos = ((Component)nearest).transform.position + Vector3.up * 0.5f;

            // Move all valuable objects to extraction
            var items = DebugCheats.valuableObjects;
            if (items != null)
            {
                foreach (object item in items)
                {
                    if (item == null) continue;
                    try
                    {
                        UnityEngine.Object unityObj = item as UnityEngine.Object;
                        if (unityObj != null && unityObj == null) continue;

                        Transform t = null;
                        if (item is Component comp)
                            t = comp.transform;
                        else
                        {
                            var prop = item.GetType().GetProperty("transform",
                                BindingFlags.Instance | BindingFlags.Public);
                            if (prop != null)
                                t = prop.GetValue(item) as Transform;
                        }
                        if (t != null)
                            t.position = sellPos;
                    }
                    catch { }
                }
            }

            // Step 3: Zero haul goal
            HaulGoalZero.ZeroHaulGoal();

            Debug.Log("[AutoComplete] Round auto-completed!");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[AutoComplete] Error: " + ex.Message);
        }
    }
}

/// <summary>
/// 门/陷阱禁用器
/// </summary>
public static class TrapDisabler
{
    public static int DisableAllTraps()
    {
        int count = 0;
        try
        {
            // Known trap types
            string[] trapTypeNames = { "ClownTrap", "TrapDoor", "TrapController",
                "TrapBear", "TrapFloor", "TrapCeiling", "TrapSpike",
                "Trap", "DoorController", "DoorLock" };

            var assembly = typeof(RunManager).Assembly;

            foreach (string typeName in trapTypeNames)
            {
                Type trapType = assembly.GetType(typeName);
                if (trapType == null) continue;

                var objects = UnityEngine.Object.FindObjectsOfType(trapType);
                foreach (var obj in objects)
                {
                    try
                    {
                        if (obj is Component comp && comp.gameObject.activeInHierarchy)
                        {
                            comp.gameObject.SetActive(false);
                            count++;
                        }
                    }
                    catch { }
                }
            }

            // Also search for any MonoBehaviour with "Trap" in name
            var allBehaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
            foreach (var mb in allBehaviours)
            {
                string typeName = mb.GetType().Name;
                if (typeName.Contains("Trap") && mb.gameObject.activeInHierarchy)
                {
                    try
                    {
                        mb.gameObject.SetActive(false);
                        count++;
                    }
                    catch { }
                }
            }

            Debug.Log($"[TrapDisabler] Disabled {count} traps/doors");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[TrapDisabler] Error: " + ex.Message);
        }
        return count;
    }
}
