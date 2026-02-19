using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public static class NoclipController
{
	public static bool noclipActive = false;

	private static float previousGravityValue;

	private static object playerControllerInstance;

	private static Type playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");

	private static InputAction jumpAction;

	private static InputAction crouchAction;

	private static InputAction sprintAction;

	private static FieldInfo rbField;

	private static FieldInfo customGravityField;

	private static MethodInfo antiGravityMethod;

	/// <summary>
	/// 检测 playerControllerInstance 是否真正有效（包括Unity对象已销毁的情况）
	/// </summary>
	private static bool IsPlayerControllerValid()
	{
		if (playerControllerInstance == null) return false;
		// 对于Unity对象，需要用Unity的null检查来检测已销毁的对象
		if (playerControllerInstance is UnityEngine.Object unityObj)
		{
			return (Object)(object)unityObj != (Object)null;
		}
		return true;
	}

	/// <summary>
	/// 重置所有缓存的引用，下次使用时会重新查找
	/// </summary>
	public static void ResetState()
	{
		playerControllerInstance = null;
		rbField = null;
		customGravityField = null;
		antiGravityMethod = null;
		jumpAction = null;
		crouchAction = null;
		sprintAction = null;
	}

	private static void InitializePlayerController()
	{
		if (playerControllerType == null)
		{
			Debug.LogError((object)"未找到 PlayerController 类型。");
			return;
		}
		// 使用 Unity-safe 检查，处理对象已被销毁的情况
		if (!IsPlayerControllerValid())
		{
			playerControllerInstance = null;
			rbField = null;
			customGravityField = null;
			antiGravityMethod = null;

			playerControllerInstance = Object.FindObjectOfType(playerControllerType);
			if (playerControllerInstance == null)
			{
				return;
			}
			rbField = playerControllerType.GetField("rb", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			customGravityField = playerControllerType.GetField("CustomGravity", BindingFlags.Instance | BindingFlags.Public);
			antiGravityMethod = playerControllerType.GetMethod("AntiGravity", BindingFlags.Instance | BindingFlags.Public);
		}
	}

	private static void InitializeInputActions()
	{
		if (jumpAction != null && crouchAction != null && sprintAction != null)
		{
			return;
		}
		Type typeFromHandle = typeof(InputManager);
		if (typeFromHandle == null)
		{
			Debug.LogError((object)"未找到 InputManager 类型。");
			return;
		}
		FieldInfo field = typeFromHandle.GetField("instance", BindingFlags.Static | BindingFlags.Public);
		if (field == null)
		{
			Debug.LogError((object)"未找到 InputManager.instance 字段。");
			return;
		}
		object value = field.GetValue(null);
		if (value == null)
		{
			Debug.LogError((object)"InputManager.instance 为 null。");
			return;
		}
		FieldInfo field2 = typeFromHandle.GetField("inputActions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (field2 == null)
		{
			Debug.LogError((object)"在 InputManager 中未找到字段 'inputActions'。");
			return;
		}
		object value2 = field2.GetValue(value);
		if (value2 == null)
		{
			Debug.LogError((object)"InputManager.inputActions 为 null。");
			return;
		}
		if (!(value2 is IDictionary dictionary))
		{
			Debug.LogError((object)"InputManager.inputActions 不是字典类型。");
			return;
		}
		foreach (DictionaryEntry item in dictionary)
		{
			object value3 = item.Value;
			InputAction val = (InputAction)((value3 is InputAction) ? value3 : null);
			if (val != null)
			{
				if (val.name == "Jump")
				{
					Debug.Log((object)("找到 Jump 动作：" + (object)val));
					jumpAction = val;
				}
				else if (val.name == "Crouch")
				{
					Debug.Log((object)("找到 Crouch 动作：" + (object)val));
					crouchAction = val;
				}
				else if (val.name == "Sprint")
				{
					Debug.Log((object)("找到 Sprint 动作：" + (object)val));
					sprintAction = val;
				}
			}
		}
	}

	public static void ToggleNoclip()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		InitializePlayerController();
		if (!IsPlayerControllerValid())
		{
			Debug.Log((object)"[Noclip] PlayerController 未找到，重置状态等待重试");
			ResetState();
			return;
		}
		if (rbField != null)
		{
			object value = rbField.GetValue(playerControllerInstance);
			Rigidbody val = (Rigidbody)((value is Rigidbody) ? value : null);
			if ((Object)(object)val != (Object)null)
			{
				val.useGravity = !noclipActive;
				val.isKinematic = noclipActive;
				if (noclipActive)
				{
					if (antiGravityMethod != null)
					{
						antiGravityMethod.Invoke(playerControllerInstance, new object[1] { 100000000f });
					}
					else
					{
						Debug.Log((object)"在 PlayerController 中未找到方法 AntiGravity。");
					}
					val.velocity = Vector3.zero;
				}
				else if (antiGravityMethod != null)
				{
					antiGravityMethod.Invoke(playerControllerInstance, new object[1] { 0 });
				}
				else
				{
					Debug.Log((object)"在 PlayerController 中未找到方法 AntiGravity。");
				}
				Collider[] componentsInChildren = ((Component)(MonoBehaviour)playerControllerInstance).GetComponentsInChildren<Collider>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].isTrigger = noclipActive;
				}
				Debug.Log((object)("穿墙" + ((!noclipActive) ? "已关闭" : "已开启") + "。"));
			}
			else
			{
				Debug.Log((object)"未在 PlayerController 中找到 Rigidbody。");
			}
		}
		else
		{
			Debug.Log((object)"未找到 PlayerController 的 rb 字段。");
		}
		if (customGravityField != null)
		{
			if (noclipActive)
			{
				previousGravityValue = (float)customGravityField.GetValue(playerControllerInstance);
				customGravityField.SetValue(playerControllerInstance, 0f);
			}
			else
			{
				float num = ((previousGravityValue > 0f) ? previousGravityValue : 30f);
				customGravityField.SetValue(playerControllerInstance, num);
			}
		}
		else
		{
			Debug.Log((object)"未找到 PlayerController 的 CustomGravity 字段。");
		}
	}

	public static void UpdateMovement()
	{
		try
		{
			if (!noclipActive) return;

			// 检测PlayerController是否有效（死亡/进商店后可能被销毁）
			if (!IsPlayerControllerValid())
			{
				ResetState();
				InitializePlayerController();
				if (!IsPlayerControllerValid()) return; // 等待下一帧重试
				// 重新找到PlayerController，重新激活noclip
				ToggleNoclip();
				return;
			}

			// === 每帧强制清零物理状态，防止向上飘移 ===
			if (rbField != null)
			{
				object rbObj = rbField.GetValue(playerControllerInstance);
				Rigidbody rb = (Rigidbody)((rbObj is Rigidbody) ? rbObj : null);
				if ((Object)(object)rb != (Object)null)
				{
					rb.velocity = Vector3.zero;
					rb.angularVelocity = Vector3.zero;
				}
			}
			// 每帧强制 CustomGravity=0 防止游戏系统恢复重力
			if (customGravityField != null)
			{
				customGravityField.SetValue(playerControllerInstance, 0f);
			}

			// === 翻滚状态下仍可穿墙移动：强制停止翻滚物理 ===
			CancelTumblePhysics();

			if (jumpAction == null || crouchAction == null || sprintAction == null)
			{
				InitializeInputActions();
			}
			Camera main = Camera.main;
			if ((Object)(object)main == (Object)null)
			{
				return; // 等待相机恢复，不关闭noclip
			}
			Vector3 val = Vector3.zero;
			Transform transform = ((Component)main).transform;
			Transform transform2 = ((Component)(MonoBehaviour)playerControllerInstance).transform;
			if ((Object)(object)transform2 == (Object)null)
			{
				ResetState();
				return; // 等待下一帧重试
			}
			if (Input.GetKey((KeyCode)119))
			{
				val += transform.forward;
			}
			if (Input.GetKey((KeyCode)115))
			{
				val -= transform.forward;
			}
			if (Input.GetKey((KeyCode)97))
			{
				val -= transform.right;
			}
			if (Input.GetKey((KeyCode)100))
			{
				val += transform.right;
			}
			if (jumpAction != null && jumpAction.IsPressed())
			{
				val += Vector3.up;
			}
			if (crouchAction != null && crouchAction.IsPressed())
			{
				val -= Vector3.up;
			}
			float num = ((sprintAction != null && sprintAction.IsPressed()) ? 20f : 10f);
			val = val.normalized * num * Time.deltaTime;
			transform2.position += val;
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("NoclipController.UpdateMovement 出错：" + ex.Message));
			ResetState();
			// 不关闭 noclipActive，允许下一帧重试
		}
	}

	/// <summary>
	/// 翻滚状态下强制停止翻滚物理，使穿墙仍然有效
	/// </summary>
	private static void CancelTumblePhysics()
	{
		try
		{
			if (!IsPlayerControllerValid()) return;

			Type tumbleType = Type.GetType("PlayerTumble, Assembly-CSharp");
			if (tumbleType == null) return;

			Component tumble = ((Component)(MonoBehaviour)playerControllerInstance).GetComponentInChildren(tumbleType);
			if (tumble == null) return;

			// 获取 tumbleActive 字段并设为 false
			FieldInfo tumbleActiveField = tumbleType.GetField("tumbleActive", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (tumbleActiveField != null)
			{
				bool isTumbling = (bool)tumbleActiveField.GetValue(tumble);
				if (isTumbling)
				{
					tumbleActiveField.SetValue(tumble, false);
				}
			}

			// 同时尝试设置 tumbleTimer 为 0
			FieldInfo tumbleTimerField = tumbleType.GetField("tumbleTimer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (tumbleTimerField != null)
			{
				tumbleTimerField.SetValue(tumble, 0f);
			}
		}
		catch { }
	}
}
