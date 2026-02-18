using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace r.e.p.o_cheat;

public static class TextureLoader
{
	public static Texture2D LoadEmbeddedTexture(string resourceName)
	{
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Expected O, but got Unknown
		try
		{
			Assembly assembly = typeof(TextureLoader).Assembly;
			string name = assembly.GetName().Name;
			string[] source = new string[4]
			{
				resourceName,
				name + "." + resourceName,
				name + ".images." + resourceName.Split('.').Last(),
				"r.e.p.o_cheat.images." + resourceName.Split('.').Last()
			};
			Stream stream = null;
			string text = "";
			foreach (string item in source.Distinct())
			{
				stream = assembly.GetManifestResourceStream(item);
				if (stream != null)
				{
					text = item;
					break;
				}
			}
			if (stream == null)
			{
				Debug.LogError((object)("Embedded resource not found! Tried: " + string.Join(", ", source.Distinct())));
				return CreateFallbackTexture(resourceName);
			}
			using (stream)
			{
				byte[] array = new byte[stream.Length];
				stream.Read(array, 0, array.Length);
				Texture2D val = new Texture2D(2, 2, (TextureFormat)5, false);
				if (!ImageConversion.LoadImage(val, array))
				{
					Debug.LogError((object)("Failed to load image data for: " + text));
					return CreateFallbackTexture(resourceName);
				}
				((Texture)val).filterMode = (FilterMode)1;
				((Texture)val).wrapMode = (TextureWrapMode)1;
				return val;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Exception while loading embedded texture: " + ex));
			return Texture2D.whiteTexture;
		}
	}

	private static Texture2D CreateFallbackTexture(string context)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(2, 2, (TextureFormat)5, false);
		Color val2 = Color.magenta;
		if (context.Contains("bg"))
		{
			val2 = new Color(0.2f, 0.2f, 0.2f, 1f);
		}
		else if (context.Contains("On"))
		{
			val2 = Color.green;
		}
		else if (context.Contains("Off"))
		{
			val2 = Color.red;
		}
		Color[] array = (Color[])(object)new Color[4];
		for (int i = 0; i < 4; i++)
		{
			array[i] = val2;
		}
		val.SetPixels(array);
		val.Apply();
		return val;
	}
}
