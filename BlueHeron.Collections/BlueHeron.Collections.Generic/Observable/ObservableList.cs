﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace BlueHeron.Collections.Generic;

/// <summary>
/// An <see cref="IList{T}"/> that implements <see cref="IObservableCollection{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the items in the list</typeparam>
public class ObservableList<T> : IList<T>, IObservableCollection<T>
{
	#region Objects and variables

	private readonly FastList<T> mList;

#nullable enable
	/// <summary>
	/// Event is fired when the collection or one or more items in the collection changed.
	/// </summary>
	public event NotifyCollectionChangedEventHandler<T>? CollectionChanged;
#nullable disable

	#endregion

	#region Construction

	/// <summary>
	/// Creates a new, empty list.
	/// </summary>
	public ObservableList()
	{
		mList = new FastList<T>();
	}

	/// <summary>
	/// Creates a new list, populated with the given collection of <typeparamref name="T"/>s.
	/// </summary>
	/// <param name="collection">A collection of <typeparamref name="T"/>s</param>
	public ObservableList(IEnumerable<T> collection)
	{
		mList = new FastList<T>(collection);
	}

	/// <summary>
	/// Creates a new, empty list using the given <see cref="System.Runtime.Serialization.StreamingContext"/>.
	/// </summary>
	/// <param name="serializationInfo">The <see cref="System.Runtime.Serialization.SerializationInfo"/></param>
	/// <param name="streamingContext">The <see cref="System.Runtime.Serialization.StreamingContext"/></param>
	public ObservableList(SerializationInfo serializationInfo, StreamingContext streamingContext)
	{
		SerializationInfo = serializationInfo;
		StreamingContext = streamingContext;
	}

	#endregion

	#region Properties

	/// <summary>
	/// Returns the number of items in this list.
	/// </summary>
	public int Count
	{
		get
		{
			lock (SyncRoot)
			{
				return mList.Count;
			}
		}
	}

	/// <summary>
	/// Returns always <code>false</code>.
	/// </summary>
	public bool IsReadOnly => false;

	/// <summary>
	/// Returns the last change that occurred.
	/// </summary>
	public NotifyCollectionChangedEventArgs<T> LastChange { get; private set; }

	/// <summary>
	/// Needed to ensure thread-safety.
	/// </summary>
	public object SyncRoot { get; } = new object();

	/// <summary>
	/// Gets or sets the item at the given index.
	/// </summary>
	/// <param name="index">The index of the item</param>
	/// <returns>The item</returns>
	public T this[int index]
	{
		get
		{
			lock (SyncRoot)
			{
				return mList[index];
			}
		}
		set
		{
			lock (SyncRoot)
			{
				var oldValue = mList[index];
				mList[index] = value;
				LastChange = NotifyCollectionChangedEventArgs<T>.Replace(value, oldValue);
				OnChanged();
			}
		}
	}

	/// <summary>
	/// The current <see cref="System.Runtime.Serialization.SerializationInfo"/>
	/// </summary>
	public SerializationInfo SerializationInfo { get; }

	/// <summary>
	/// The current <see cref="System.Runtime.Serialization.StreamingContext"/>.
	/// </summary>
	public StreamingContext StreamingContext { get; }

	#endregion

	#region Public methods and functions

	/// <summary>
	/// Adds the given item to the list.
	/// </summary>
	/// <param name="item">The item</param>
	public void Add(T item)
	{
		lock (SyncRoot)
		{
			LastChange = NotifyCollectionChangedEventArgs<T>.Add(item);
			mList.Add(item);
			OnChanged();
		}
	}

	/// <summary>
	/// Returns this list as an <see cref="IAsyncEnumerable{T}"/>.
	/// </summary>
	/// <returns>An <see cref="IAsyncEnumerable{T}"/></returns>
	public IAsyncEnumerable<T> AsAsyncEnumerable()
	{
		return mList.ToAsyncEnumerable<T>();
	}

	/// <summary>
	/// Clears the list of all items.
	/// </summary>
	public void Clear()
	{
		lock (SyncRoot)
		{
			mList.Clear();
			LastChange = NotifyCollectionChangedEventArgs<T>.Reset();
			OnChanged();
		}
	}

	/// <summary>
	/// Checks if the list contains the given item.
	/// </summary>
	/// <param name="item">An item of type <typeparamref name="T"/></param>
	/// <returns>Boolean, true if the item is present in the list</returns>
	public bool Contains(T item)
	{
		lock (SyncRoot)
		{
			return mList.Contains(item);
		}
	}

	/// <summary>
	/// Copies the items in this list to the given array, starting at the given index.
	/// </summary>
	/// <param name="array">The array to copy the items to</param>
	/// <param name="arrayIndex">The index at which to start</param>
	public void CopyTo(T[] array, int arrayIndex)
	{
		lock (SyncRoot)
		{
			mList.CopyTo(array, arrayIndex);
		}
	}

	/// <summary>
	/// Returns the enumerator for this collection.
	/// </summary>
	/// <returns>An <see cref="IEnumerator{T}"/></returns>
	public IEnumerator<T> GetEnumerator()
	{
		lock (SyncRoot)
		{
			foreach (var item in mList)
			{
				yield return item;
			}
		}
	}

	/// <summary>
	/// Returns the index in this list of the first instance of the given item.
	/// </summary>
	/// <param name="item">The item</param>
	/// <returns>The zero-based index of the first occurrence of this item if found, otherwise -1</returns>
	public int IndexOf(T item)
	{
		return mList.IndexOf(item);
	}

	/// <summary>
	/// Inserts the given item at the given index in the list.
	/// </summary>
	/// <param name="index">The index</param>
	/// <param name="item">The item</param>
	public void Insert(int index, T item)
	{
		mList.Insert(index, item);
	}

	/// <summary>
	/// Removes the given item from the list.
	/// </summary>
	/// <param name="item">The item</param>
	/// <returns>Boolean, true if the operation was successful</returns>
	public bool Remove(T item)
	{
		lock (SyncRoot)
		{
			if (mList.Remove(item))
			{
				LastChange = NotifyCollectionChangedEventArgs<T>.Remove(item);
				OnChanged();
				return true;
			}
			return false;
		}
	}

	/// <summary>
	/// Removes the item at the given index from the list.
	/// </summary>
	/// <param name="index">The index of the item</param>
	/// <returns>Boolean, true if the operation was successful</returns>
	public void RemoveAt(int index)
	{
		mList.RemoveAt(index);
	}

	/// <summary>
	/// Replaces the given item with a new item.
	/// </summary>
	/// <param name="oldItem">The item to replace</param>
	/// <param name="newItem">The new item</param>
	/// <returns>Boolean, true if the operation was successful</returns>
	public bool Replace(T oldItem, T newItem)
	{
		lock (SyncRoot)
		{
			if (mList.Remove(oldItem))
			{
				mList.Add(newItem);
				LastChange = NotifyCollectionChangedEventArgs<T>.Replace(newItem, oldItem);
				OnChanged();
				return true;
			}
			return false;
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