using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace BlueHeron.Collections.Generic
{
	/// <summary>
	/// Strongly-typed <see cref="Hashtable">Hashtable</see>.
	/// </summary>
	/// <typeparam name="TKey">The type of the key by which objects are accessed</typeparam>
	/// <typeparam name="TValue">The type of the object to store</typeparam>
	[Serializable]
	public class StateTable<TKey, TValue> : ObservableDictionary<TKey, TValue>, ICloneable, IEquatable<StateTable<TKey, TValue>>
	{
		#region  Public methods and functions 

		/// <summary>
		/// Returns the items in this table as an enumerable of <see cref="KeyValuePair{TKey, TValue}"/> objects.
		/// </summary>
		/// <returns>The contents of this table as an enumerable of <see cref="KeyValuePair{TKey, TValue}"/> objects</returns>
		[DebuggerStepThrough()]
		public IEnumerable<KeyValuePair<TKey, TValue>> AsEnumerable()
		{
			foreach (var kv in this)
			{
				yield return kv;
			}
		}

		/// <summary>
		/// Returns this object as a <see cref="Dictionary{TKey, TValue}"/>.
		/// </summary>
		/// <returns>A <see cref="Dictionary{TKey, TValue}"/></returns>
		[DebuggerStepThrough()]
		public Dictionary<TKey, TValue> AsDictionary() { return this.Cast<DictionaryEntry>().ToDictionary(d => (TKey)d.Key, d => (TValue)d.Value); }

		/// <summary>
		/// Returns the items in this table as an enumerable of <see cref="KeyValuePair{TKey, TValue}"/> objects, sorted by key.
		/// </summary>
		/// <returns>The contents of this table as an enumerable of <see cref="KeyValuePair{TKey, TValue}"/> objects</returns>
		[DebuggerStepThrough()]
		public IEnumerable<KeyValuePair<TKey, TValue>> AsSortedEnumerable()
		{
			foreach (var kv in this.OrderBy(S => S.Key))
			{
				yield return kv;
			}
		}

		/// <summary>
		/// Returns this object as a <see cref="Dictionary{TKey, TValue}"/>, sorted by key.
		/// </summary>
		/// <returns>A <see cref="Dictionary{TKey, TValue}"/></returns>
		public Dictionary<TKey, TValue> AsSortedDictionary()
		{
			return this.OrderBy(s => s.Key).Cast<DictionaryEntry>().ToDictionary(d => (TKey)d.Key, d => (TValue)d.Value);
		}

		/// <summary>
		/// Returns a deep clone of this object.
		/// </summary>
		public object Clone() {	return new StateTable<TKey, TValue>(AsEnumerable()); }

		/// <summary>
		/// Compares the contents of this StateTable to the contents of the given StateTable.
		/// The order of the keys is not important.
		/// </summary>
		/// <param name="other">The StateTable to compare</param>
		/// <returns>Boolean, true if both objects have the same keys and values</returns>
		public bool Equals(StateTable<TKey, TValue> other)
		{
			if (Count != other.Count)
			{
				return false;
			}

			var pairs = AsEnumerable().OrderBy(s => s.Key).ToList(); // enumerate once
			var otherPairs = other.AsEnumerable().OrderBy(s => s.Key).ToList(); // enumerate once

			for (var i = 0; i < Count; i++) // don't use hashcode for equality!
			{
				if (!pairs[i].Key.Equals(otherPairs[i].Key) || !pairs[i].Value.Equals(otherPairs[i].Value))
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Overridden to return the result of a call to <see cref="Equals(StateTable{TKey, TValue})"/>.
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>Boolean, true if both objects have the same keys and values, in the same order</returns>
		public override bool Equals(object obj)
		{
			if (obj is null || obj is not StateTable<TKey, TValue>)
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			return Equals((StateTable<TKey, TValue>)obj);
		}

		/// <summary>
		/// Compares the contents of two StateTables for equality.
		/// The order of the keys is not important.
		/// <returns>Boolean, true if both objects have the same keys and values</returns>
		/// </summary>
		/// <param name="state1">The first StateTable</param>
		/// <param name="state2">The second StateTable</param>
		public static bool operator ==(StateTable<TKey, TValue> state1, StateTable<TKey, TValue> state2)
		{
			return state1.Equals(state2);
		}

		/// <summary>
		/// Compares the contents of two StateTables for equality.
		/// The order of the keys is not important.
		/// <returns>Boolean, true if both objects have the different keys and/or values</returns>
		/// </summary>
		/// <param name="state1">The first StateTable</param>
		/// <param name="state2">The second StateTable</param>
		public static bool operator !=(StateTable<TKey, TValue> state1, StateTable<TKey, TValue> state2)
		{
			return !state1.Equals(state2);
		}

		#endregion

		#region  Construction 

		/// <summary>
		/// Initializes a new, mEmpty instance of a StateTable.
		/// </summary>
		public StateTable() : base() { }

		/// <summary>
		/// Initializes a new instance of a StateTable and tries to populate it using the given <see cref="KeyValuePair{TKey, TValue}"/>s.
		/// </summary>
		/// <param name="stateValues">A collection of <see cref="KeyValuePair{TKey, TValue}"/>s</param>
		public StateTable(IEnumerable<KeyValuePair<TKey, TValue>> stateValues) : base(stateValues) { }

		/// <summary>Initializes a new, mEmpty instance of the StateTable, that is serializable using the given <see cref="SerializationInfo"/> and <see cref="System.Runtime.Serialization.StreamingContext"/> objects.</summary>
		/// <param name="serializationInfo">The <see cref="SerializationInfo"/> to use</param>
		/// <param name="streamingContext">The <see cref="StreamingContext"/> to use</param>
		protected StateTable(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }

		#endregion
	}
}