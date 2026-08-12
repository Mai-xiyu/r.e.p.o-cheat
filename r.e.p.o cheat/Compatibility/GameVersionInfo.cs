using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace r.e.p.o_cheat.Compatibility
{
	/// <summary>
	/// Compatibility state for the currently loaded game assembly.
	/// Fingerprint = Assembly-CSharp SHA-256 + capability detection, never a version-string compare.
	/// </summary>
	public enum CompatState
	{
		Exact,
		Compatible,
		Partial,
		Unknown
	}

	public static class GameVersionInfo
	{
		/// <summary>SHA-256 of Assembly-CSharp.dll this build was adapted against (v0.4.4.3, 2026-08-13).</summary>
		public const string KnownSha256 = "CE995A182DDC884EA965E87786F1986248D9616300FA825BCC04BCA671EE6526";

		public const string KnownGameVersion = "v0.4.4.3";

		public const string KnownPhotonVersion = "2.52";

		public const string KnownUnityVersion = "2022.3.67f2";

		/// <summary>Game types the cheat (and its modules) depends on at runtime.</summary>
		private static readonly string[] RequiredGameTypes =
		{
			"SemiFunc, Assembly-CSharp",
			"PlayerAvatar, Assembly-CSharp",
			"PhysGrabber, Assembly-CSharp",
			"InputManager, Assembly-CSharp",
			"ItemGun, Assembly-CSharp",
			"StatsManager, Assembly-CSharp",
			"RunManager, Assembly-CSharp",
			"LocalizationManager, Assembly-CSharp",
			"TutorialUI, Assembly-CSharp"
		};

		private static readonly Dictionary<string, bool> _typeCache = new Dictionary<string, bool>(StringComparer.Ordinal);

		private static string _runtimeSha256;

		private static bool _shaComputed;

		/// <summary>SHA-256 of the Assembly-CSharp.dll actually loaded in this process (cached).</summary>
		public static string RuntimeSha256
		{
			get
			{
				if (!_shaComputed)
				{
					_shaComputed = true;
					_runtimeSha256 = ComputeLoadedAssemblySha256();
				}
				return _runtimeSha256;
			}
		}

		/// <summary>Current compatibility state, derived from fingerprint + required symbol detection.</summary>
		public static CompatState State
		{
			get
			{
				string sha = RuntimeSha256;
				if (string.IsNullOrEmpty(sha))
				{
					return CompatState.Unknown;
				}
				if (string.Equals(sha, KnownSha256, StringComparison.OrdinalIgnoreCase))
				{
					return CompatState.Exact;
				}
				int missing = 0;
				foreach (string type in RequiredGameTypes)
				{
					if (!HasGameType(type))
					{
						missing++;
					}
				}
				if (missing == 0)
				{
					return CompatState.Compatible;
				}
				return missing < RequiredGameTypes.Length ? CompatState.Partial : CompatState.Unknown;
			}
		}

		/// <summary>True when the named game type (e.g. "SemiFunc, Assembly-CSharp") is present in the loaded game. Cached.</summary>
		public static bool HasGameType(string assemblyQualifiedName)
		{
			lock (_typeCache)
			{
				if (_typeCache.TryGetValue(assemblyQualifiedName, out bool present))
				{
					return present;
				}
				present = Type.GetType(assemblyQualifiedName, throwOnError: false) != null;
				_typeCache[assemblyQualifiedName] = present;
				return present;
			}
		}

		/// <summary>One-time diagnostics block printed at init.</summary>
		public static string GetDiagnostics()
		{
			return string.Concat(
				"GameVersionInfo: state=", State,
				" gameVersion=", KnownGameVersion,
				" assemblySha256=", RuntimeSha256 ?? "<unavailable>",
				" photon=", KnownPhotonVersion,
				" unity=", KnownUnityVersion);
		}

		private static string ComputeLoadedAssemblySha256()
		{
			try
			{
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					if (assembly == null || assembly.GetName().Name != "Assembly-CSharp")
					{
						continue;
					}
					string location = assembly.Location;
					if (string.IsNullOrEmpty(location) || !File.Exists(location))
					{
						return null;
					}
					using (var stream = new FileStream(location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					using (var sha = SHA256.Create())
					{
						byte[] hash = sha.ComputeHash(stream);
						return BitConverter.ToString(hash).Replace("-", string.Empty);
					}
				}
				return null;
			}
			catch
			{
				return null;
			}
		}
	}
}
