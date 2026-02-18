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

	private static void InitializePlayerController()
	{
		if (playerControllerType == null)
		{
			Debug.LogError((object)"未找到 PlayerController 类型。");
		}
		else if (playerControllerInstance == null)
		{
			playerControllerInstance = Object.FindObjectOfType(playerControllerType);
			if (playerControllerInstance == null)
			{
				Debug.LogError((object)"当前场景未找到 PlayerController 实例。");
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
		if (playerControllerInstance == null)
		{
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
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!noclipActive || playerControllerInstance == null)
			{
				if (playerControllerInstance == null)
				{
					Debug.Log((object)"PlayerControllerInstance 为 null");
				}
				else
				{
					Debug.Log((object)"noclipActive 为 false");
				}
				return;
			}
			if (jumpAction == null || crouchAction == null || sprintAction == null)
			{
				Debug.Log((object)"Jump/Crouch/Sprint 为空，正在初始化输入动作。");
				InitializeInputActions();
			}
			Camera main = Camera.main;
			if ((Object)(object)main == (Object)null)
			{
				Debug.LogError((object)"Camera.main 为空！");
				noclipActive = false;
				return;
			}
			Vector3 val = Vector3.zero;
			Transform transform = ((Component)main).transform;
			Transform transform2 = ((Component)(MonoBehaviour)playerControllerInstance).transform;
			if ((Object)(object)transform2 == (Object)null)
			{
				Debug.LogError((object)"玩家 Transform 为空！");
				noclipActive = false;
				return;
			}
			Debug.Log((object)"处理按键输入前。");
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
			Debug.Log((object)"处理按键输入后。");
			Debug.Log((object)"处理跳跃/下蹲前。");
			if (jumpAction != null && jumpAction.IsPressed())
			{
				val += Vector3.up;
			}
			if (crouchAction != null && crouchAction.IsPressed())
			{
				val -= Vector3.up;
			}
			Debug.Log((object)"处理跳跃/下蹲后。");
			float num = ((sprintAction != null && sprintAction.IsPressed()) ? 20f : 10f);
			val = val.normalized * num * Time.deltaTime;
			transform2.position += val;
			Debug.Log((object)"更新玩家位置后。");
			if (sprintAction != null && !sprintAction.IsPressed() && rbField != null)
			{
				object value = rbField.GetValue(playerControllerInstance);
				Rigidbody val2 = (Rigidbody)((value is Rigidbody) ? value : null);
				if ((Object)(object)val2 != (Object)null)
				{
					val2.velocity = Vector3.zero;
					return;
				}
				Debug.LogError((object)"Rigidbody（rb）为空！");
				noclipActive = false;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("NoclipController.UpdateMovement 出错：" + ex.Message));
			noclipActive = false;
		}
	}
}
