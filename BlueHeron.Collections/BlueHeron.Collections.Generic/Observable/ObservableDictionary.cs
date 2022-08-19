using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace BlueHeron.Collections.Generic
{
	/// <summary>
	/// A <see cref="IDictionary{TKey, TValue}"/> that implements <see cref="IObservableCollection{T}"/>.
	/// </summary>
	/// <typeparam name="TKey">The type of the key</typeparam>
	/// <typeparam name="TValue">The type of the value</typeparam>
	public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IObservableCollection<KeyValuePair<TKey, TValue>> where TKey : notnull
	{
		#region Objects and variables

		private readonly Dictionary<TKey, TValue> mDictionary;

#nullable enable
		/// <summary>
		/// Event is fired when the collection or one or more items in the collection changed.
		/// </summary>
		public event NotifyCollectionChangedEventHandler<KeyValuePair<TKey, TValue>>? CollectionChanged;
#nullable disable

		#endregion

		#region Construction

		/// <summary>
		/// Creates a new, mEmpty dictionary.
		/// </summary>
		public ObservableDictionary()
		{
			mDictionary = new Dictionary<TKey, TValue>();
		}

		/// <summary>
		/// Creates a new dictionary, populated with the given collection of <see cref="KeyValuePair{TKey, TValue}"/>s
		/// </summary>
		/// <param name="collection">A collection of <see cref="KeyValuePair{TKey, TValue}"/>s</param>
		public ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
		{
			mDictionary = new Dictionary<TKey, TValue>(collection);
		}

		/// <summary>
		/// Creates a new, mEmpty dictionary using the given <see cref="StreamingContext"/>.
		/// </summary>
		/// <param name="serializationInfo">The <see cref="System.Runtime.Serialization.SerializationInfo"/></param>
		/// <param name="streamingContext">The <see cref="System.Runtime.Serialization.StreamingContext"/></param>
		public ObservableDictionary(SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			SerializationInfo = serializationInfo;
			StreamingContext = streamingContext;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Returns the number items in this dictionary.
		/// </summary>
		public int Count
		{
			get
			{
				lock (SyncRoot)
				{
					return mDictionary.Count;
				}
			}
		}

		/// <summary>
		/// Returns always <code>false</code>.
		/// </summary>
		public bool IsReadOnly => false;

		/// <summary>Returns a collection of keys present in this dictionary.
		/// </summary>
		public IEnumerable<TKey> Keys
		{
			get
			{
				lock (SyncRoot)
				{
					return mDictionary.Keys;
				}
			}
		}

		/// <summary>Returns a collection of keys present in this dictionary.
		/// </summary>
		ICollection<TKey> IDictionary<TKey, TValue>.Keys
		{
			get
			{
				lock (SyncRoot)
				{
					return mDictionary.Keys;
				}
			}
		}

		/// <summary>
		/// Returns the last change that occurred.
		/// </summary>
		public NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>> LastChange { get; private set; }

		/// <summary>
		/// Needed to ensure thread-safety.
		/// </summary>
		public object SyncRoot { get; } = new object();

		/// <summary>
		/// Gets or sets the <typeparamref name="TValue"/> with the given key.
		/// </summary>
		/// <param name="key">The key of type <typeparamref name="TKey"/></param>
		/// <returns>The value of type <typeparamref name="TValue"/></returns>
		public TValue this[TKey key]
		{
			get
			{
				lock (SyncRoot)
				{
					return mDictionary[key];
				}
			}
			set
			{
				lock (SyncRoot)
				{
					if (mDictionary.TryGetValue(key, out var oldValue))
					{
						mDictionary[key] = value;
						LastChange = NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>>.Replace(
							new KeyValuePair<TKey, TValue>(key, value),
							new KeyValuePair<TKey, TValue>(key, oldValue));
						OnChanged();
					}
					else
					{
						Add(key, value);
					}
				}
			}
		}

		/// <summary>Returns a collection of values present in this dictionary.
		/// </summary>
		public IEnumerable<TValue> Values
		{
			get
			{
				lock (SyncRoot)
				{
					return mDictionary.Values;
				}
			}
		}

		/// <summary>Returns a collection of values present in this dictionary.
		/// </summary>
		ICollection<TValue> IDictionary<TKey, TValue>.Values
		{
			get
			{
				lock (SyncRoot)
				{
					return mDictionary.Values;
				}
			}
		}

		/// <summary>
		/// The current <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// </summary>
		public SerializationInfo SerializationInfo
		{
			get;
		}

		/// <summary>
		/// The current <see cref="System.Runtime.Serialization.StreamingContext"/>.
		/// </summary>
		public StreamingContext StreamingContext
		{
			get;
		}

		#endregion

		#region Public methods and functions

		/// <summary>
		/// Adds an item to the dictionary.
		/// </summary>
		/// <param name="key">The key</param>
		/// <param name="value">The value</param>
		public void Add(TKey key, TValue value)
		{
			lock (SyncRoot)
			{
				LastChange = NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>>.Add(new KeyValuePair<TKey, TValue>(key, value));
				mDictionary.Add(key, value);
				OnChanged();
			}
		}

		/// <summary>
		/// Adds an item to the dictionary.
		/// </summary>
		/// <param name="item">A <see cref="KeyValuePair{TKey, TValue}"/></param>
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			Add(item.Key, item.Value);
		}

		/// <summary>
		/// Removes all items from the dictionary.
		/// </summary>
		public void Clear()
		{
			lock (SyncRoot)
			{
				mDictionary.Clear();
				LastChange = NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>>.Reset();
				OnChanged();
			}
		}

		/// <summary>
		/// Determines whether the dictionary contains a specific item.
		/// </summary>
		/// <param name="item">A <see cref="KeyValuePair{TKey, TValue}"/></param>
		/// <returns>Boolean, true if the item exists in the dictionary</returns>
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			lock (SyncRoot)
			{
				return ((ICollection<KeyValuePair<TKey, TValue>>)mDictionary).Contains(item);
			}
		}

		/// <summary>
		/// Determines whether the dictionary contains an item with the given key.
		/// </summary>
		/// <param name="key">The key to look up</param>
		/// <returns>Boolean, true if the item exists in the dictionary</returns>
		public bool ContainsKey(TKey key)
		{
			lock (SyncRoot)
			{
				return ((IDictionary<TKey, TValue>)mDictionary).ContainsKey(key);
			}
		}

		/// <summary>
		/// Copies the items of the dictionary to the given array, starting at the given index
		/// </summary>
		/// <param name="array">The array to copy the items to</param>
		/// <param name="arrayIndex">The index at which to start</param>
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			lock (SyncRoot)
			{
				((ICollection<KeyValuePair<TKey, TValue>>)mDictionary).CopyTo(array, arrayIndex);
			}
		}

		/// <summary>
		/// Returns the enumerator for this collection.
		/// </summary>
		/// <returns>An <see cref="IEnumerator{T}"/></returns>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			lock (SyncRoot)
			{
				foreach (var item in mDictionary)
				{
					yield return item;
				}
			}
		}

		/// <summary>
		/// Removes the item with the given key from the dictionary.
		/// </summary>
		/// <param name="key">The key</param>
		/// <returns>Boolean, true if the operation was successful</returns>
		public bool Remove(TKey key)
		{
			lock (SyncRoot)
			{
				if (mDictionary.Remove(key, out var value))
				{
					LastChange = NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>>.Remove(new KeyValuePair<TKey, TValue>(key, value));
					OnChanged();
					return true;
				}
				return false;
			}
		}

		/// <summary>
		/// Removes the given key-value pair from the dictionary.
		/// </summary>
		/// <param name="item">The <see cref="KeyValuePair{TKey, TValue}"/> to remove</param>
		/// <returns>Boolean, true if the operation was successful</returns>
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			lock (SyncRoot)
			{
				if (mDictionary.TryGetValue(item.Key, out var value))
				{
					if (EqualityComparer<TValue>.Default.Equals(value, item.Value))
					{
						if (mDictionary.Remove(item.Key, out var value2))
						{
							LastChange = NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>>.Remove(new KeyValuePair<TKey, TValue>(item.Key, value2));
							OnChanged();
							return true;
						}
					}
				}
				return false;
			}
		}

		/// <summary>
		/// Replace the old key-value pair with a new key-value pair.
		/// </summary>
		/// <param name="oldKey">The old key</param>
		/// <param name="oldValue">The old value</param>
		/// <param name="newKey">The new key</param>
		/// <param name="newValue">The new value</param>
		/// <returns>Bollean, true if the operation was successful</returns>
		public bool Replace(TKey oldKey, TValue oldValue, TKey newKey, TValue newValue)
		{
			lock (SyncRoot)
			{
				if (mDictionary.Remove(oldKey))
				{
					mDictionary.Add(newKey, newValue);
					LastChange = NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>>.Replace(
							new KeyValuePair<TKey, TValue>(newKey, newValue),
							new KeyValuePair<TKey, TValue>(oldKey, oldValue));
					OnChanged();
					return true;
				}				
				return false;
			}
		}

		/// <summary>
		/// Gets the value associated with the given key.
		/// </summary>
		/// <param name="key">The key to look up</param>
		/// <param name="value">The value, if an item with the given key exists, else null</param>
		/// <returns>Boolean, true if the operation was successful</returns>
		public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
		{
			lock (SyncRoot)
			{
				return mDictionary.TryGetValue(key, out value);
			}
		}

		#endregion

		#region Private methods and functions

		/// <summary>
		/// Returns an enumerator for this collection.
		/// </summary>
		/// <returns>An <see cref="IEnumerator"/></returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Invokes the <see cref="CollectionChanged"/> event.
		/// </summary>
		private void OnChanged()
		{
			CollectionChanged?.Invoke(LastChange);
		}

		#endregion
	}
}