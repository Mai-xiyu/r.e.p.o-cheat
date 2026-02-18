using System;
using UnityEngine;

public class GameHelper : MonoBehaviour
{
	public static object FindObjectOfType(Type type)
	{
		return Object.FindObjectOfType(type, true);
	}
}
