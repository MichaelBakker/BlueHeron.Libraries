using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace BlueHeron.Collections.Generic
{
	/// <summary>
	/// Strongly-typed <see cref="Hashtable">Hashtable</see>.
	/// </summary>
	public class StateTable : ObservableDictionary<string, StateTableEntry>, ICloneable, IEquatable<StateTable>
	{
		#region  Public methods and functions 

		/// <summary>
		/// Adds the given value to the StateTable using the given key.
		/// </summary>
		/// <typeparam name="T">The type of the value</typeparam>
		/// <param name="key">The key under which to store the value</param>
		/// <param name="value">The value to store</param>
		/// <param name="isPublic">If true, mark this value as public. Default: true</param>
		public void Add<T>(string key, T value, bool isPublic = true)
		{
			if (value is StateTableEntry)
			{
				base.Add(key, value as StateTableEntry);
			}
			else
			{
				base.Add(key, new StateTableEntry(key, value, isPublic));
			}
		}

		/// <summary>
		/// Returns the items in this table as an enumerable of <see cref="KeyValuePair{String, StateTableEntry}"/> objects.
		/// </summary>
		/// <returns>The contents of this table as an enumerable of <see cref="KeyValuePair{String, TValue}"/> objects</returns>
		[DebuggerStepThrough()]
		public IEnumerable<KeyValuePair<string, StateTableEntry>> AsEnumerable()
		{
			foreach (var kv in this)
			{
				yield return kv;
			}
		}

		/// <summary>
		/// Returns this object as a <see cref="Dictionary{String, StateTableEntry}"/>.
		/// </summary>
		/// <returns>A <see cref="Dictionary{String, StateTableEntry}"/></returns>
		[DebuggerStepThrough()]
		public Dictionary<string, StateTableEntry> AsDictionary() { return Count == 0? new Dictionary<string, StateTableEntry>() : this.Cast<DictionaryEntry>().ToDictionary(d => (string)d.Key, d => (StateTableEntry)d.Value); }

		/// <summary>
		/// Returns the items in this table as an enumerable of <see cref="KeyValuePair{String, StateTableEntry}"/> objects, sorted by key.
		/// </summary>
		/// <returns>The contents of this table as an enumerable of <see cref="KeyValuePair{String, StateTableEntry}"/> objects</returns>
		[DebuggerStepThrough()]
		public IEnumerable<KeyValuePair<string, StateTableEntry>> AsSortedEnumerable()
		{
			foreach (var kv in this.OrderBy(S => S.Key))
			{
				yield return kv;
			}
		}

		/// <summary>
		/// Returns this object as a <see cref="Dictionary{String, StateTableEntry}"/>, sorted by key.
		/// </summary>
		/// <returns>A <see cref="Dictionary{String, StateTableEntry}"/></returns>
		public Dictionary<string, StateTableEntry> AsSortedDictionary()
		{
			return Count == 0 ? new Dictionary<string, StateTableEntry>() : this.OrderBy(s => s.Key).Cast<DictionaryEntry>().ToDictionary(d => (string)d.Key, d => (StateTableEntry)d.Value);
		}

		/// <summary>
		/// Returns a deep clone of this object.
		/// </summary>
		public object Clone() {	return Count == 0? new StateTable() : new StateTable(Values.Select(e => (StateTableEntry)e.Clone())); }

		/// <summary>
		/// Compares the contents of this StateTable to the contents of the given StateTable.
		/// The order of the keys is not important.
		/// </summary>
		/// <param name="other">The StateTable to compare</param>
		/// <returns>Boolean, true if both objects have the same keys and values</returns>
		public bool Equals(StateTable other)
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
		/// Overridden to return the result of a call to <see cref="Equals(StateTable)"/>.
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>Boolean, true if both objects have the same keys and values, in the same order</returns>
		public override bool Equals(object obj)
		{
			if (obj is null || obj is not StateTable)
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			return Equals((StateTable)obj);
		}

		/// <summary>
		/// Returns the typed value of the <see cref="StateTableEntry"/> with the given key.
		/// </summary>
		/// <typeparam name="T">The type of the value stored in the <see cref="StateTableEntry"/></typeparam>
		/// <param name="key">The key under which the item was stored</param>
		/// <returns>The value with type <typeparamref name="T"/></returns>
		public T GetValue<T>(string key)
		{
			return (T)this[key].Value;
		}

		/// <summary>
		/// Tries to get the typed value of the <see cref="StateTableEntry"/> with the given key.
		/// </summary>
		/// <typeparam name="T">The type of the value stored in the <see cref="StateTableEntry"/></typeparam>
		/// <param name="key">The key under which the item was stored</param>
		/// <param name="validateType">Determines whether the stored type should match the requested type</param>
		/// <param name="value">The value with type <typeparamref name="T"/>, if the operation was successful</param>
		/// <returns>Boolean, true if the typed value could be retrieved</returns>
		public bool TryGetValue<T>(string key, bool validateType, [MaybeNullWhen(false)] out T value)
		{
			if (ContainsKey(key))
			{
				var entry = this[key];
				if (validateType)
				{
					if (Type.GetType(entry.ValueTypeName).IsAssignableTo(typeof(T)))
					{
						value = (T)entry.Value;
						return true;
					}
				}
				else
				{
					value = (T)entry.Value;
					return true;
				}
			}
			value = default(T);
			return false;
		}

		/// <summary>
		/// Compares the contents of two StateTables for equality.
		/// The order of the keys is not important.
		/// <returns>Boolean, true if both objects have the same keys and values</returns>
		/// </summary>
		/// <param name="state1">The first StateTable</param>
		/// <param name="state2">The second StateTable</param>
		public static bool operator ==(StateTable state1, StateTable state2)
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
		public static bool operator !=(StateTable state1, StateTable state2)
		{
			return !state1.Equals(state2);
		}

		#endregion

		#region  Construction 

		/// <summary>
		/// Initializes a new, empty instance of a StateTable.
		/// </summary>
		public StateTable() : base() { }

		/// <summary>
		/// Initializes a new instance of a StateTable and tries to populate it using the given <see cref="IEnumerable{StateTableEntry}"/>s.
		/// </summary>
		/// <param name="stateEntries">A collection of <see cref="StateTableEntry"/> objects</param>
		public StateTable(IEnumerable<StateTableEntry> stateEntries) {
			
			if(stateEntries == null || !stateEntries.Any())
			{
				throw new ArgumentNullException(nameof(stateEntries));
			}
			var enu = stateEntries.GetEnumerator();
			while (enu.MoveNext())
			{
				base.Add(enu.Current.Key, enu.Current);
			}

		}

		/// <summary>
		/// Initializes a new instance of a StateTable and tries to populate it using the given <see cref="KeyValuePair{String, StateTableEntry}"/>s.
		/// </summary>
		/// <param name="stateValues">A collection of <see cref="KeyValuePair{String, StateTableEntry}"/>s</param>
		public StateTable(IEnumerable<KeyValuePair<string, StateTableEntry>> stateValues) : base(stateValues) { }

		/// <summary>
		/// Initializes a new instance of a StateTable and tries to populate it using the given <see cref="KeyValuePair{String, StateTableEntry}"/>s.
		/// </summary>
		/// <param name="stateValues">A collection of <see cref="KeyValuePair{String, StateTableEntry}"/>s</param>
		public StateTable(IEnumerable<KeyValuePair<string, object>> stateValues)
		{
			if (stateValues == null || !stateValues.Any())
			{
				throw new ArgumentNullException(nameof(stateValues));
			}
			var enu = stateValues.GetEnumerator();
			while (enu.MoveNext())
			{
				if (enu.Current.Value is StateTableEntry)
				{
					base.Add(enu.Current.Key, (StateTableEntry)enu.Current.Value);
				}
				else
				{
					base.Add(enu.Current.Key, new StateTableEntry(enu.Current.Key, enu.Current.Value));
				}
			}
		}

		/// <summary>Initializes a new, empty instance of the StateTable, that is serializable using the given <see cref="SerializationInfo"/> and <see cref="StreamingContext"/> objects.</summary>
		/// <param name="serializationInfo">The <see cref="SerializationInfo"/> to use</param>
		/// <param name="streamingContext">The <see cref="StreamingContext"/> to use</param>
		protected StateTable(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }

		#endregion
	}

	/// <summary>
	/// Container for an item in the <see cref="StateTable" />.
	/// </summary>
	[JsonObject("e"), DebuggerDisplay("{Key}: {Value}")]
	public class StateTableEntry : IEquatable<StateTableEntry>, ICloneable
	{
		#region Objects and variables

		private string mType;

		#endregion

		#region Properties

		/// <summary>
		/// The key of the entry.
		/// </summary>
		[JsonProperty(PropertyName = "k")]
		public string Key { get; set; }

		/// <summary>
		/// If true, mark this item as public.
		/// </summary>
		[JsonProperty(PropertyName = "p")]
		public bool IsPublic { get; set; }

		/// <summary>
		/// The type of the value, represented by this object.
		/// </summary>
		[JsonIgnore()]
		public string ValueTypeName
		{
			get
			{
				if(string.IsNullOrEmpty(mType))
				{
					mType = Value.GetType().AssemblyQualifiedName;
				}
				return mType;
			} 
		}

		/// <summary>
		/// The value of the object.
		/// </summary>
		[JsonProperty(PropertyName = "v")]
		public object Value { get; set; }

		#endregion

		#region  Public methods and functions 

		/// <summary>
		/// Returns a deep copy of this object.
		/// </summary>
		/// <returns>A new <see cref="StateTableEntry"/> </returns>
		public object Clone()
		{
			return new StateTableEntry
			{
				Key = Key,
				Value = Value,
				IsPublic = IsPublic,
				mType = ValueTypeName
			};
		}

		/// <summary>
		/// Returns the <see cref="Value"/>.
		/// </summary>
		[DebuggerStepThrough()]
		public override string ToString()
		{
			return Value.ToString();
		}

		#endregion

		#region Equality

		/// <summary>
		/// Determines if both entries have the same key (case-sensitive) and value.
		/// </summary>
		/// <param name="other">The other <see cref="StateTableEntry"/></param>
		/// <returns>Boolean, true if both objects have the same key and value</returns>
		public bool Equals(StateTableEntry other)
		{
			return Key.Equals(other.Key, StringComparison.InvariantCulture) && Value.ToString().Equals(other.Value.ToString(), StringComparison.InvariantCulture);
		}

		/// <summary>
		/// Overridden to return the result of a call to <see cref="Equals(StateTableEntry)"/>.
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>Boolean, true if both objects have the same key and value</returns>
		public override bool Equals(object obj)
		{
			if (obj is null)
			{
				return false;
			}
			if (obj is not StateTableEntry)
			{
				return Value.Equals(obj);
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			return Equals((StateTable)obj);
		}

		/// <summary>
		/// Determines if both entries have the same key (case-sensitive) and value.
		/// <returns>Boolean, true if both objects have the same key and value</returns>
		/// </summary>
		/// <param name="entry1">The first StateTableEntry</param>
		/// <param name="entry2">The second StateTableEntry</param>
		public static bool operator ==(StateTableEntry entry1, StateTableEntry entry2)
		{
			return entry1.Equals(entry2);
		}

		/// <summary>
		/// Determines if both objects have the same value. The key is ignored here.
		/// <returns>Boolean, true if both objects have the value</returns>
		/// </summary>
		/// <param name="entry1">The first StateTableEntry</param>
		/// <param name="entry2">The second object</param>
		public static bool operator ==(StateTableEntry entry1, object entry2)
		{
			return entry1.Equals(entry2);
		}

		/// <summary>
		/// Determines if both entries have the same key (case-sensitive) and value.
		/// <returns>Boolean, true if the objects have a different key and/or value</returns>
		/// </summary>
		/// <param name="entry1">The first StateTableEntry</param>
		/// <param name="entry2">The second StateTableEntry</param>
		public static bool operator !=(StateTableEntry entry1, StateTableEntry entry2)
		{
			return !entry1.Equals(entry2);
		}

		/// <summary>
		/// Determines if both entries have the same value. The key is ignored here.
		/// <returns>Boolean, true if the objects have a different value</returns>
		/// </summary>
		/// <param name="entry1">The first StateTableEntry</param>
		/// <param name="entry2">The second object</param>
		public static bool operator !=(StateTableEntry entry1, object entry2)
		{
			return !entry1.Equals(entry2);
		}

		#endregion

		#region Construction

		/// <summary>
		/// Creates a new, empty entry.
		/// </summary>
		public StateTableEntry() { }
		
		/// <summary>
		/// Creates a new, public entry.
		/// </summary>
		/// <param name="key">The key, by which this item is accessed.</param>
		/// <param name="value">The value of the item</param>
		public StateTableEntry(string key, object value) 
		{
			Key = key;
			Value = value;
			IsPublic = true;
		}

		/// <summary>
		/// Creates a new entry.
		/// </summary>
		/// <param name="key">The key, by which this item is accessed.</param>
		/// <param name="value">The value of the item</param>
		/// <param name="isPublic">If <see langword="true"/>, mark this item as public</param>
		public StateTableEntry(string key, object value, bool isPublic)
		{
			Key = key;
			Value = value;
			IsPublic = isPublic;
		}

		#endregion
	}
}