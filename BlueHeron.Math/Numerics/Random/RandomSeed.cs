﻿using System;
using System.Security.Cryptography;

namespace BlueHeron.Math.Numerics.Random;

/// <summary>
/// Random seed value provider.
/// </summary>
public static class RandomSeed
{
	static readonly object Lock = new object();
	static readonly RandomNumberGenerator MasterRng = RandomNumberGenerator.Create();

	/// <summary>
	/// Provides a time-dependent seed value, matching the default behavior of System.Random.
	/// WARNING: There is no randomness in this seed and quick repeated calls can cause the same seed value.
	/// Do not use for cryptography!
	/// </summary>
	public static int Time()
	{
		return Environment.TickCount;
	}

	/// <summary>
	/// Provides a seed based on time and unique GUIDs.
	/// WARNING: There is only low randomness in this seed, but at least quick repeated calls will result in different seed values.
	/// Do not use for cryptography!
	/// </summary>
	public static int Guid()
	{
		return Environment.TickCount ^ System.Guid.NewGuid().GetHashCode();
	}

	/// <summary>
	/// Provides a seed based on an internal random number generator (crypto if available), time and unique GUIDs.
	/// WARNING: There is only medium randomness in this seed, but quick repeated calls will result in different seed values.
	/// Do not use for cryptography!
	/// </summary>
	public static int Robust()
	{
		lock (Lock)
		{
			var bytes = new byte[4];
			MasterRng.GetBytes(bytes);
			return BitConverter.ToInt32(bytes, 0);
		}
	}
}