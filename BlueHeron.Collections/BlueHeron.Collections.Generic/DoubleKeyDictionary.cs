using System;
using System.Collections;
using System.Collections.Generic;

namespace BlueHeron.Collections.Generic;

/// <summary>
/// A double key dictionary.
/// </summary>
/// <typeparam name="K1">The type of the first key</typeparam>
/// <typeparam name="K2">The type of the second key</typeparam>
/// <typeparam name="TValue">The value type</typeparam>
public sealed class DoubleKeyDictionary<K1, K2, TValue> : IEnumerable<DoubleKeyPairValue<K1, K2, TValue>>, IEquatable<DoubleKeyDictionary<K1, K2, TValue>> where K1 : notnull where K2 : notnull
{
	#region Properties

	/// <summary>
	/// Gets or sets the value with the specified keys.
	/// </summary>
	/// <value>The value, which is either added or updated</value>
	public TValue this[K1 key1, K2 key2]
	{
		get => OuterDictionary[key1][key2];
		set => Add(key1, key2, value);
	}

	/// <summary>
	/// Gets the outer dictionary.
	/// </summary>
	private Dictionary<K1, Dictionary<K2, TValue>> OuterDictionary { get; } = [];

	#endregion

	#region Public methods and functions

	/// <summary>
	/// Adds the specified value under the specified keys.
	/// </summary>
	/// <param name="key1">The first key</param>
	/// <param name="key2">The second key</param>
	/// <param name="value">The value</param>
	public void Add(K1 key1, K2 key2, TValue value)
	{
		if (OuterDictionary.TryGetValue(key1, out var inner))
		{
			if (!inner.TryAdd(key2, value))
			{
				inner[key2] = value;
			}
        }
        else
		{
			inner = new Dictionary<K2, TValue> {{ key2, value }};
			OuterDictionary.Add(key1, inner);
		}
	}

	/// <summary>
	/// Clears this dictionary of all items.
	/// </summary>
	public void Clear()
	{
		foreach (var dict in OuterDictionary.Values)
		{
			dict?.Clear();
		}
		OuterDictionary.Clear();
	}

	/// <summary>
	/// Determines whether this dictionary contains the given keys.
	/// </summary>
	/// <param name="index1">The first key</param>
	/// <param name="index2">The second key</param>
	/// <returns>Returns <c>true</c> if the specified keys are both present; otherwise, <c>false</c></returns>
	public bool ContainsKey(K1 index1, K2 index2)
	{
		if (!OuterDictionary.TryGetValue(index1, out var value))
		{
			return false;
		}
		if (!value.ContainsKey(index2))
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Returns true, if both dictionaries are equal.
	/// </summary>
	/// <param name="other">The other <see cref="DoubleKeyDictionary{K1, K2, TValue}"/></param>
	/// <returns>True, if both dictionaries are equal</returns>
	public bool Equals(DoubleKeyDictionary<K1, K2, TValue>? other)
	{
		if (other == null || OuterDictionary.Keys.Count != other.OuterDictionary.Keys.Count)
		{
			return false;
		}

		var isEqual = true;

		foreach (var innerItems in OuterDictionary)
		{
			if (!other.OuterDictionary.ContainsKey(innerItems.Key))
			{
				isEqual = false;
			}
			if (!isEqual)
			{
				break;
			}

			// here we can be sure that the key is in both lists, but we need to check the contents of the inner dictionary
			var otherInnerDictionary = other.OuterDictionary[innerItems.Key];
			foreach (var innerValue in innerItems.Value)
			{
				if (!otherInnerDictionary.ContainsValue(innerValue.Value))
				{
					isEqual = false;
				}
				if (!otherInnerDictionary.ContainsKey(innerValue.Key))
				{
					isEqual = false;
				}
			}
			if (!isEqual)
			{
				break;
			}
		}
		return isEqual;
	}

	/// <summary>
	/// Returns true, if both dictionaries are equal.
	/// </summary>
	/// <param name="obj">The object to compare to</param>
	/// <returns>True, if both dictionaries are equal</returns>
	public override bool Equals(object? obj)
	{
		if (obj is null)
		{
			return false;
		}
		if (obj is DoubleKeyDictionary<K1, K2, TValue>)
		{
			return Equals(obj as DoubleKeyDictionary<K1, K2, TValue>);
		}
		return false;
	}

	/// <summary>
	/// Returns the <see cref="IEnumerator{T}"/>.
	/// </summary>
	public IEnumerator<DoubleKeyPairValue<K1, K2, TValue>> GetEnumerator()
	{
		foreach (var outer in OuterDictionary)
		{
			foreach (var inner in outer.Value)
			{
				yield return new DoubleKeyPairValue<K1, K2, TValue>(outer.Key, inner.Key, inner.Value);
			}
		}
	}

	/// <summary>
	/// Returns an enumerator that iterates through the collection.
	/// </summary>
	/// <returns>
	/// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
	/// </returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>
	/// Removes the item with the specified keys.
	/// </summary>
	/// <param name="key1">The first key</param>
	/// <param name="key2">The second key</param>
	public void Remove(K1 key1, K2 key2)
	{
		OuterDictionary[key1].Remove(key2);
		if (OuterDictionary[key1].Count == 0)
		{
			OuterDictionary.Remove(key1);
		}
	}

	/// <summary>
	/// Tries to retrieve the value with the specified keys.
	/// </summary>
	/// <param name="key1">The first key</param>
	/// <param name="key2">The second key</param>
	/// <param name="obj">The value of type <typeparamref name="TValue"/>, if present, else null</param>
	/// <returns></returns>
	public bool TryGetValue(K1 key1, K2 key2, out TValue? obj)
	{
		if (OuterDictionary.TryGetValue(key1, out var inner) && inner.TryGetValue(key2, out obj))
		{
			return true;
		}
		else
		{
			obj = default!;
			return false;
		}
	}

	/// <summary>
	/// Gets all values.
	/// </summary>
	public IEnumerable<TValue> Values
	{
		get
		{
			foreach (var dict in OuterDictionary.Values)
			{
				if (dict != null)
				{
					foreach(var item in dict.Values)
					{
						yield return item;
					}
				}
			}
		}
	}

	#endregion
}

/// <summary>
/// A key-value pair that is represented by two keys and a value.
/// </summary>
/// <typeparam name="K1">The type of the first key</typeparam>
/// <typeparam name="K2">The type of the second key</typeparam>
/// <typeparam name="TValue">The type of the value</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="DoubleKeyPairValue{K1,K2,TValue}"/> class.
/// </remarks>
/// <param name="key1">The first key</param>
/// <param name="key2">The second key</param>
/// <param name="value">The value</param>
public sealed class DoubleKeyPairValue<K1, K2, TValue>(K1 key1, K2 key2, TValue value)
{
	#region Objects and variables

	private const string fmtToString = "{0} - {1} | {2}";

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the first key.
    /// </summary>
    /// <value>The key</value>
    public K1 Key1 { get; set; } = key1;

    /// <summary>
    /// Gets or sets the second key.
    /// </summary>
    /// <value>The key</value>
    public K2 Key2 { get; set; } = key2;

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>The value</value>
    public TValue Value { get; set; } = value;

    #endregion

    #region Public methods and functions

    /// <summary>
    /// Returns a <see cref="string"/> that represents this instance.
    /// </summary>
    public override string ToString()
	{
		return string.Format(fmtToString, Key1, Key2, Value);
	}

	#endregion
}