using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace BlueHeron.Collections.Generic
{
	/// <summary>
	/// A fast and lean <see cref="List{T}"/>, with direct access to the underlying array and extended with asynchronous enumeration and search.
	/// </summary>
	/// <typeparam name="T">The type of the elements in the list</typeparam>
	[DebuggerDisplay("Count = {Count}"), DebuggerStepThrough()]
	public class FastList<T> : IList<T>, IReadOnlyList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IAsyncEnumerable<T>
	{
		#region Objects and variables

		private const int mDefaultCapacity = 4;
		private static readonly T[] mEmpty = Array.Empty<T>();
		private int mSize;

		#endregion

		#region Construction

		/// <summary>
		/// Creates a new, empty list.
		/// </summary>
		public FastList()
		{
			Items = mEmpty;
		}

		/// <summary>
		/// Creates a new FastList, based on the given <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <param name="collection">The <see cref="IEnumerable{T}"/></param>
		public FastList(IEnumerable<T> collection)
		{
			if (collection is ICollection<T> is2)
			{
				var count = is2.Count;
				Items = new T[count];
				is2.CopyTo(Items, 0);
				mSize = count;
			}
			else
			{
				mSize = 0;
				Items = new T[mDefaultCapacity];
				using var enumerator = collection.GetEnumerator();
				while (enumerator.MoveNext())
				{
					Add(enumerator.Current);
				}
			}
		}

		/// <summary>
		/// Creates a new, empty list with the given capacity.
		/// </summary>
		/// <param name="capacity">The capacity</param>
		public FastList(int capacity)
		{
			Items = new T[capacity];
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the capacity of this list.
		/// </summary>
		public int Capacity
		{
			get => Items.Length;
			set
			{
				if (value != Items.Length)
				{
					if (value > 0)
					{
						var destinationArray = new T[value];
						if (mSize > 0)
						{
							Array.Copy(Items, 0, destinationArray, 0, mSize);
						}
						Items = destinationArray;
					}
					else
					{
						Items = mEmpty;
					}
				}
			}
		}

		/// <summary>
		/// Returns the number of elements in the list.
		/// </summary>
		public int Count => mSize;

		/// <summary>
		/// Gets the items from the internal array. Make sure to get the number of elements in this array using <see cref="Count"/> instead of Array.Length().
		/// </summary>
		public T[] Items { get; private set; }

		/// <summary>
		/// Determines whether this list is read-only. Always <code>false</code>.
		/// </summary>
		bool ICollection<T>.IsReadOnly => false;

		/// <summary>
		/// Indexed accessor.
		/// </summary>
		/// <param name="index">The index of the element</param>
		/// <returns>The element</returns>
		public T this[int index]
		{
			get => Items[index];
			set => Items[index] = value;
		}

		#endregion

		#region Public methods and functions

		/// <summary>
		/// Adds the given item to the list.
		/// </summary>
		/// <param name="item">The item to add</param>
		public void Add(T item)
		{
			if (mSize == Items.Length)
			{
				EnsureCapacity(mSize + 1);
			}
			Items[mSize++] = item;
		}

		/// <summary>
		/// Fast add all from another <see cref="FastList{T}"/>.
		/// </summary>
		/// <param name="list">The list from which to add all items to this list</param>
		public void AddAll(FastList<T> list)
		{
			EnsureCapacity(mSize + list.Count);
			Array.Copy(list.Items, 0, Items, Count, list.Count);
			mSize += list.Count;
		}

		/// <summary>
		/// Adds the given collection of items to this list.
		/// </summary>
		/// <param name="collection">The <see cref="IEnumerable{T}"/> to add</param>
		public void AddRange(IEnumerable<T> collection)
		{
			InsertRange(mSize, collection);
		}

		/// <summary>
		/// Returns this list as a <see cref="ReadOnlyCollection{T}"/>.
		/// </summary>
		/// <returns>A <see cref="ReadOnlyCollection{T}"/></returns>
		public ReadOnlyCollection<T> AsReadOnly()
		{
			return new ReadOnlyCollection<T>(this);
		}

		/// <summary>
		/// Searches the entire list for the given element, using the <see cref="IComparable{T}"/> generic interface implemented by each element of the list and by the specified object.
		/// </summary>
		/// <param name="item">The item to search for</param>
		/// <returns>The index of the specified value in the list, if value is found; otherwise, a negative number.
		/// If value is not found and value is less than one or more elements in the list, the negative number returned is the bitwise complement of the index of the first element that is larger than the value.
		/// If value is not found and value is greater than all elements in array, the negative number returned is the bitwise complement of (the index of the last element plus 1).
		/// If this method is called with a non-sorted list, the return value can be incorrect and a negative number could be returned, even if the value is present in the list</returns>
		public int BinarySearch(T item)
		{
			return BinarySearch(0, Count, item, null);
		}

		/// <summary>
		/// Searches the entire list for the given element using the specified <see cref="IComparer{T}"/> generic interface.
		/// </summary>
		/// <param name="item">The item to search for</param>
		/// <param name="comparer">The <see cref="IComparer{T}"/> implementation to use when comparing elements or null to use the <see cref="IComparable{T}"/> implementation of each element</param>
		/// <returns>The index of the specified value in the mList, if value is found; otherwise, a negative number.
		/// If value is not found and value is less than one or more elements in the list, the negative number returned is the bitwise complement of the index of the first element that is larger than the value.
		/// If value is not found and value is greater than all elements in array, the negative number returned is the bitwise complement of (the index of the last element plus 1).
		/// If this method is called with a non-sorted list, the return value can be incorrect and a negative number could be returned, even if the value is present in the list</returns>
		public int BinarySearch(T item, IComparer<T> comparer)
		{
			return BinarySearch(0, Count, item, comparer);
		}

		/// <summary>
		/// Searches a range of elements in the list for the given element, using the specified <see cref="IComparer{T}"/> generic interface.
		/// </summary>
		/// <param name="index">The starting index of the range to search</param>
		/// <param name="count">The length of the range to search</param>
		/// <param name="item">The item to search for</param>
		/// <param name="comparer">The <see cref="IComparer{T}"/> implementation to use when comparing elements or null to use the <see cref="IComparable{T}"/> implementation of each element</param>
		/// <returns>The index of the specified value in the mList, if value is found; otherwise, a negative number.
		/// If value is not found and value is less than one or more elements in the list, the negative number returned is the bitwise complement of the index of the first element that is larger than the value.
		/// If value is not found and value is greater than all elements in array, the negative number returned is the bitwise complement of (the index of the last element plus 1).
		/// If this method is called with a non-sorted list, the return value can be incorrect and a negative number could be returned, even if the value is present in the list</returns>
		public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
		{
			return Array.BinarySearch(Items, index, count, item, comparer);
		}

		/// <summary>
		/// Clears the list of all items.
		/// </summary>
		public void Clear()
		{
			Clear(false);
		}

		/// <summary>
		/// Clears this list with a fast-clear option.
		/// </summary>
		/// <param name="fastClear">If set to <c>true</c> this method only resets the <see cref="Count"/> number of elements but doesn't clear referenced items already stored in the list</param>
		public void Clear(bool fastClear)
		{
			Resize(0, fastClear);
		}

		/// <summary>
		/// Checks if the list contains the given item.
		/// </summary>
		/// <param name="item">The item to check</param>
		/// <returns>Boolean, true if the item is present in the list</returns>
		public bool Contains(T item)
		{
			if (item == null)
			{
				for (var j = 0; j < mSize; j++)
				{
					if (Items[j] == null)
					{
						return true;
					}
				}
				return false;
			}
			var comparer = EqualityComparer<T>.Default;
			for (var i = 0; i < mSize; i++)
			{
				if (comparer.Equals(Items[i], item))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Copies the items in this list to the given array, starting at the given index.
		/// </summary>
		/// <param name="array">The array to copy to</param>
		/// <param name="arrayIndex">The index in the given array at which to start copying</param>
		public void CopyTo(T[] array, int arrayIndex)
		{
			Array.Copy(Items, 0, array, arrayIndex, mSize);
		}

		/// <summary>
		/// Copies the items in this list to the given array, starting at an index of zero.
		/// </summary>
		/// <param name="array">The array to copy to</param>
		public void CopyTo(T[] array)
		{
			CopyTo(array, 0);
		}

		/// <summary>
		/// Copies a range of elements from the list starting at the specified source index and pastes them into the given <see cref="Array"/> starting at the specified destination index.
		/// The length and the indexes are specified as 64-bit integers.
		/// </summary>
		/// <param name="sourceIndex">The starting index in this list</param>
		/// <param name="array">The destination array</param>
		/// <param name="destinationIndex">The starting index in the given array</param>
		/// <param name="count">The number of elements to copy</param>
		public void CopyTo(int sourceIndex, T[] array, int destinationIndex, int count)
		{
			Array.Copy(Items, sourceIndex, array, destinationIndex, count);
		}

		/// <summary>
		/// Ensures that this list has at least the given capacity.
		/// </summary>
		/// <param name="min">The minimum capacity to ensure</param>
		public void EnsureCapacity(int min)
		{
			if (Items.Length < min)
			{
				var num = (Items.Length == 0) ? mDefaultCapacity : (Items.Length * 2);
				if (num < min)
				{
					num = min;
				}
				Capacity = num;
			}
		}

		/// <summary>
		/// Determines if an item exists in the list that matches the given <see cref="Predicate{T}"/>.
		/// </summary>
		/// <param name="match">The <see cref="Predicate{T}"/> by which to match items</param>
		/// <returns>Boolean, <c>true</c> if an item exists that matches the <see cref="Predicate{T}"/></returns>
		public bool Exists(Predicate<T> match)
		{
			return (FindIndex(match) != -1);
		}

		/// <summary>
		/// Tries to find and returns the first item that matches the given predicate.
		/// If no matching item is found, null is returned.
		/// </summary>
		/// <param name="match">The <see cref="Predicate{T}"/> with which to find the item</param>
		/// <returns>The item if it exists, else null</returns>
		public T Find(Predicate<T> match)
		{
			for (var i = 0; i < mSize; i++)
			{
				if (match(Items[i]))
				{
					return Items[i];
				}
			}
			return default;
		}

		/// <summary>
		/// Returns all items in the list that match the given <see cref="Predicate{T}"/>.
		/// </summary>
		/// <param name="match">The <see cref="Predicate{T}"/> with which to find items</param>
		/// <returns>A <see cref="FastList{T}"/>, containing the matching items</returns>
		public FastList<T> FindAll(Predicate<T> match)
		{
			var list = new FastList<T>();
			for (var i = 0; i < mSize; i++)
			{
				if (match(Items[i]))
				{
					list.Add(Items[i]);
				}
			}
			return list;
		}

		/// <summary>
		/// Tries to find and returns the index of the first item that matches the given predicate.
		/// If no matching item is found, -1 is returned.
		/// </summary>
		/// <param name="match">The <see cref="Predicate{T}"/> with which to find the item</param>
		/// <returns>The index of the item in the list if it exists, else -1</returns>
		public int FindIndex(Predicate<T> match)
		{
			return FindIndex(0, mSize, match);
		}

		/// <summary>
		/// Tries to find and returns the index of the first item that matches the given predicate.
		/// The search is started from the given index forwards.
		/// If no matching item is found, -1 is returned.
		/// </summary>
		/// <param name="startIndex">The index at which to start searching</param>
		/// <param name="match">The <see cref="Predicate{T}"/> with which to find the item</param>
		/// <returns>The index of the item in the list if it exists, else -1</returns>
		public int FindIndex(int startIndex, Predicate<T> match)
		{
			return FindIndex(startIndex, mSize - startIndex, match);
		}

		/// <summary>
		/// Tries to find and returns the index of the first item that matches the given predicate.
		/// The search is started from the given index forwards, searching the given number of items.
		/// If no matching item is found, -1 is returned.
		/// </summary>
		/// <param name="startIndex">The index at which to start searching</param>
		/// <param name="count">The number of items to search through</param>
		/// <param name="match">The <see cref="Predicate{T}"/> with which to find the item</param>
		/// <returns>The index of the item in the list if it exists, else -1</returns>
		public int FindIndex(int startIndex, int count, Predicate<T> match)
		{
			var num = startIndex + count;
			for (var i = startIndex; i < num; i++)
			{
				if (match(Items[i]))
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Tries to find and returns the last item that matches the given predicate.
		/// If no matching item is found, null is returned.
		/// </summary>
		/// <param name="match">The <see cref="Predicate{T}"/> with which to find the item</param>
		/// <returns>The item if it exists, else null</returns>
		public T FindLast(Predicate<T> match)
		{
			for (var i = mSize - 1; i >= 0; i--)
			{
				if (match(Items[i]))
				{
					return Items[i];
				}
			}
			return default;
		}

		/// <summary>
		/// Tries to find and returns the index of the last item that matches the given predicate.
		/// If no matching item is found, -1 is returned.
		/// </summary>
		/// <param name="match">The <see cref="Predicate{T}"/> with which to find the item</param>
		/// <returns>The index of the item in the list if it exists, else -1</returns>
		public int FindLastIndex(Predicate<T> match)
		{
			return FindLastIndex(mSize - 1, mSize, match);
		}

		/// <summary>
		/// Tries to find and returns the index of the last item that matches the given predicate.
		/// The search is started from the given index forwards, searching the given number of items.
		/// If no matching item is found, -1 is returned.
		/// </summary>
		/// <param name="startIndex">The index at which to start searching</param>
		/// <param name="match">The <see cref="Predicate{T}"/> with which to find the item</param>
		/// <returns>The index of the item in the list if it exists, else -1</returns>
		public int FindLastIndex(int startIndex, Predicate<T> match)
		{
			return FindLastIndex(startIndex, startIndex + 1, match);
		}

		/// <summary>
		/// Tries to find and returns the index of the last item that matches the given predicate.
		/// The search is started from the given index forwards, searching the given number of items.
		/// If no matching item is found, -1 is returned.
		/// </summary>
		/// <param name="startIndex">The index at which to start searching</param>
		/// <param name="count">The number of items to search through</param>
		/// <param name="match">The <see cref="Predicate{T}"/> with which to find the item</param>
		/// <returns>The index of the item in the list if it exists, else -1</returns>
		public int FindLastIndex(int startIndex, int count, Predicate<T> match)
		{
			var num = startIndex - count;
			for (var i = startIndex; i > num; i--)
			{
				if (match(Items[i]))
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Iterates over all items in the list and executes the given <see cref="Action{T}"/>.
		/// </summary>
		/// <param name="action">The <see cref="Action{T}"/></param>
		public void ForEach(Action<T> action)
		{
			for (var i = 0; i < mSize; i++)
			{
				action(Items[i]);
			}
		}

		/// <summary>
		/// Returns an <see cref="IAsyncEnumerator{T}"/> for this list.
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns>An <see cref="IAsyncEnumerator{T}"/></returns>
		public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
		{
			return new Enumerator(this, cancellationToken);
		}

		/// <summary>
		/// Returns an <see cref="Enumerator" /> for this list.
		/// </summary>
		/// <returns>An <see cref="Enumerator" /></returns>
		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		/// <summary>
		/// Gets the internal array used to hold data.
		/// </summary>
		/// <returns>An array of items of type <typeparamref name="T"/></returns>
		public T[] GetInternalArray()
		{
			return Items;
		}

		/// <summary>
		/// Returns the given range of items as a <see cref="FastList{T}"/>.
		/// </summary>
		/// <param name="index">The starting index</param>
		/// <param name="count">The number of items to copy</param>
		/// <returns>A <see cref="FastList{T}"/></returns>
		public FastList<T> GetRange(int index, int count)
		{
			var list = new FastList<T>(count);
			Array.Copy(Items, index, list.Items, 0, count);
			list.mSize = count;
			return list;
		}

		/// <summary>
		/// Increases the capacity of this list by the given number.
		/// </summary>
		/// <param name="increase">The increase in number of elements</param>
		public void IncreaseCapacity(int increase)
		{
			EnsureCapacity(mSize + increase);
			mSize += increase;
		}

		/// <summary>
		/// Returns the index in this list of the first instance of the given item.
		/// </summary>
		/// <param name="item">The item to check</param>
		/// <returns>The zero-based index of the first occurrence of this item if found, otherwise -1</returns>
		public int IndexOf(T item)
		{
			return Array.IndexOf(Items, item, 0, mSize);
		}

		/// <summary>
		/// Returns the index in this list of the first instance of the given item.
		/// The search is started at the given index.
		/// </summary>
		/// <param name="item">The item to check</param>
		/// <param name="index">The index at which to start searching</param>
		/// <returns>The zero-based index of the first occurrence of this item if found, otherwise -1</returns>
		public int IndexOf(T item, int index)
		{
			return Array.IndexOf(Items, item, index, mSize - index);
		}

		/// <summary>
		/// Returns the index in this list of the first instance of the given item.
		/// The search is started at the given index and searches the given number of items.
		/// </summary>
		/// <param name="item">The item to check</param>
		/// <param name="index">The index at which to start searching</param>
		/// <param name="count">The number of items to search through</param> 
		/// <returns>The zero-based index of the first occurrence of this item if found, otherwise -1</returns>
		public int IndexOf(T item, int index, int count)
		{
			return Array.IndexOf(Items, item, index, count);
		}

		/// <summary>
		/// Inserts the given item at the given index in the list.
		/// </summary>
		/// <param name="index">The index at which to insert the item</param>
		/// <param name="item">The item to insert</param>
		public void Insert(int index, T item)
		{
			if (mSize == Items.Length)
			{
				EnsureCapacity(mSize + 1);
			}
			if (index < mSize)
			{
				Array.Copy(Items, index, Items, index + 1, mSize - index);
			}
			Items[index] = item;
			mSize++;
		}

		/// <summary>
		/// Inserts the given collection of items, starting at the given index.
		/// </summary>
		/// <param name="index">The index at which to start inserting</param>
		/// <param name="collection">The <see cref="IEnumerable{T}"/> to insert</param>
		public void InsertRange(int index, IEnumerable<T> collection)
		{
			if (collection is ICollection<T> is2)
			{
				var count = is2.Count;
				if (count > 0)
				{
					EnsureCapacity(mSize + count);
					if (index < mSize)
					{
						Array.Copy(Items, index, Items, index + count, mSize - index);
					}
					if (this == is2)
					{
						Array.Copy(Items, 0, Items, index, index);
						Array.Copy(Items, (index + count), Items, (index * 2), (mSize - index));
					}
					else
					{
						is2.CopyTo(Items, index);
					}
					mSize += count;
				}
			}
			else
			{
				using var enumerator = collection.GetEnumerator();
				while (enumerator.MoveNext())
				{
					Insert(index++, enumerator.Current);
				}
			}
		}

		/// <summary>
		/// Resizes this list.
		/// </summary>
		/// <param name="newSize">The new size of the list</param>
		/// <param name="fastClear">If set to <c>true</c> this method only resets the <see cref="Count"/> number of elements but doesn't clear referenced items already stored in the list</param>
		public void Resize(int newSize, bool fastClear)
		{
			if (mSize < newSize)
			{
				EnsureCapacity(newSize);
			}
			else if (!fastClear && mSize - newSize > 0)
			{
				Array.Clear(Items, newSize, mSize - newSize);
			}

			mSize = newSize;
		}

		/// <summary>
		/// Returns the index in this list of the last instance of the given item.
		/// </summary>
		/// <param name="item">The item to check</param>
		/// <returns>The zero-based sourceIndex of the last occurrence of this item if found, otherwise -1</returns>
		public int LastIndexOf(T item)
		{
			if (mSize == 0)
			{
				return -1;
			}
			return LastIndexOf(item, mSize - 1, mSize);
		}

		/// <summary>
		/// Returns the index in this list of the last instance of the given item.
		/// The search is started at the given index.
		/// </summary>
		/// <param name="item">The item to check</param>
		/// <param name="index">The index at which to start searching</param>
		/// <returns>The zero-based index of the last occurrence of this item if found, otherwise -1</returns>
		public int LastIndexOf(T item, int index)
		{
			return LastIndexOf(item, index, index + 1);
		}

		/// <summary>
		/// Returns the index in this list of the last instance of the given item.
		/// The search is started at the given index and searches the given number of items.
		/// </summary>
		/// <param name="item">The item to check</param>
		/// <param name="index">The index at which to start searching</param>
		/// <param name="count">The number of items to search through</param>
		/// <returns>The zero-based sourceIndex of the last occurrence of this item if found, otherwise -1</returns>
		public int LastIndexOf(T item, int index, int count)
		{
			if (mSize == 0)
			{
				return -1;
			}
			return Array.LastIndexOf(Items, item, index, count);
		}

		/// <summary>
		/// Removes the given item from the list.
		/// </summary>
		/// <param name="item">The item to remove</param>
		/// <returns>Boolean, <c>true</c> if the operation was successful</returns>
		public bool Remove(T item)
		{
			var index = IndexOf(item);
			if (index >= 0)
			{
				RemoveAt(index);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Removes all items that match the given <see cref="Predicate{T}"/>.
		/// </summary>
		/// <param name="match">The <see cref="Predicate{T}"/> by which to match items</param>
		/// <returns>The number of removed items</returns>
		public int RemoveAll(Predicate<T> match)
		{
			var index = 0;
			while ((index < mSize) && !match(Items[index]))
			{
				index++;
			}
			if (index >= mSize)
			{
				return 0;
			}
			var num2 = index + 1;
			while (num2 < mSize)
			{
				while ((num2 < mSize) && match(Items[num2]))
				{
					num2++;
				}
				if (num2 < mSize)
				{
					Items[index++] = Items[num2++];
				}
			}
			Array.Clear(Items, index, mSize - index);
			var num3 = mSize - index;
			mSize = index;
			return num3;
		}

		/// <summary>
		/// Removes the item at the given index from the list.
		/// </summary>
		/// <param name="index">The index of the item</param>
		/// <exception cref="ArgumentOutOfRangeException">Index must be larger than or equal to zero and smaller than the size of this list</exception>
		public void RemoveAt(int index)
		{
			if (index < 0 || index >= mSize)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			mSize--;
			if (index < mSize)
			{
				Array.Copy(Items, index + 1, Items, index, mSize - index);
			}
			Items[mSize] = default;
		}

		/// <summary>
		/// Removes a range of items from the list.
		/// </summary>
		/// <param name="index">The index at which to start removing</param>
		/// <param name="count">The number of items to remove</param>
		public void RemoveRange(int index, int count)
		{
			if (count > 0)
			{
				mSize -= count;
				if (index < mSize)
				{
					Array.Copy(Items, index + count, Items, index, mSize - index);
				}
				Array.Clear(Items, mSize, count);
			}
		}

		/// <summary>
		/// Reverses the order of all items in the list.
		/// </summary>
		public void Reverse()
		{
			Reverse(0, Count);
		}

		/// <summary>
		/// Reverses the order of a range of items in the list.
		/// </summary>
		/// <param name="index">The index at which to start reversing</param>
		/// <param name="count">The number of items to reverse</param>
		public void Reverse(int index, int count)
		{
			Array.Reverse(Items, index, count);
		}

		/// <summary>
		/// Sorts the elements in the list using the <see cref="System.IComparable{T}"/> generic interface implementation of each item in the list.
		/// </summary>
		public void Sort()
		{
			Array.Sort(Items, 0, Count);
		}

		/// <summary>
		/// Sorts the elements in the list using the given <see cref="IComparer{T}"/>.
		/// </summary>
		/// <param name="comparer">The <see cref="IComparer{T}"/> to use</param>
		public void Sort(IComparer<T> comparer)
		{
			Sort(0, Count, comparer);
		}

		/// <summary>
		/// Sorts the elements in the list using the given <see cref="IComparer{T}"/>, starting from the given index and sorting the given number of items.
		/// </summary>
		/// <param name="index">The index at which to start sorting</param>
		/// <param name="count">The number of items to sort</param>
		/// <param name="comparer">The <see cref="IComparer{T}"/> to use</param>
		public void Sort(int index, int count, IComparer<T> comparer)
		{
			Array.Sort(Items, index, count, comparer);
		}

		/// <summary>
		/// Returns this list as an array.
		/// </summary>
		/// <returns>An array (T[])</returns>
		public T[] ToArray()
		{
			var destinationArray = new T[mSize];
			Array.Copy(Items, 0, destinationArray, 0, mSize);
			return destinationArray;
		}

		/// <summary>
		/// Sets the capacity of this list to the number of items in the list.
		/// </summary>
		public void TrimExcess()
		{
			if (Count == Capacity)
			{
				return;
			}
			var curr = Items;
			Items = Count == 0 ? mEmpty : new T[Count];
			if (Count > 0)
			{
				Array.Copy(curr, 0, Items, 0, Count);
			}
			Capacity = Count;
		}

		/// <summary>
		/// Returns <c>true</c> if all items in the list match the given <see cref="Predicate{T}"/>.
		/// </summary>
		/// <param name="match">The <see cref="Predicate{T}"/> by which to match items</param>
		/// <returns>True, if all items match the given <see cref="Predicate{T}"/>, else false</returns>
		public bool TrueForAll(Predicate<T> match)
		{
			for (var i = 0; i < mSize; i++)
			{
				if (!match(Items[i]))
				{
					return false;
				}
			}
			return true;
		}

		#endregion

		#region Private methods and functions

		/// <summary>
		/// Returns an <see cref="IEnumerator{T}"/> for this mList.
		/// </summary>
		/// <returns>An <see cref="IEnumerator{T}"/></returns>
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return new Enumerator(this);
		}

		/// <summary>
		/// Returns an <see cref="IEnumerator"/> for this mList.
		/// </summary>
		/// <returns>An <see cref="IEnumerator"/></returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}

		#endregion

		#region Enumerator

		/// <summary>
		/// A custom <see cref="IEnumerator{T}"/>, <see cref="IAsyncEnumerator{T}"/> and <see cref="IEnumerator"/> implementation, optimized for <see cref="FastList{T}"/>s.
		/// </summary>
		[StructLayout(LayoutKind.Sequential), DebuggerStepThrough()]
		public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator, IAsyncEnumerator<T>, IAsyncDisposable
		{
			#region Objects and variables

			private T mCurrent;
			private int mIndex;
			private readonly FastList<T> mList;
			private readonly CancellationToken? mToken;

			#endregion

			#region Construction and destruction

			/// <summary>
			/// Creates a new enumerator for the given <see cref="FastList{T}"/>.
			/// </summary>
			/// <param name="list">The <see cref="FastList{T}"/> for which to return an enumerator</param>
			/// <param name="cancellationToken"></param>
			internal Enumerator(FastList<T> list, CancellationToken cancellationToken = default)
			{
				mList = list;
				mIndex = 0;
				mCurrent = default;
				mToken = cancellationToken;
			}

			/// <summary>
			/// Frees up resources held by this object (none).
			/// </summary>
			public void Dispose() { }

			/// <summary>
			/// Frees up resources held by this object (none).
			/// </summary>
			/// <returns>A <see cref="ValueTask.CompletedTask"/></returns>
			public ValueTask DisposeAsync()
			{
				return ValueTask.CompletedTask;
			}

			#endregion

			#region Properties

			/// <summary>
			/// Gets the current item.
			/// </summary>
			public T Current => mCurrent;

			/// <summary>
			/// Gets the current item.
			/// </summary>
			[Obsolete]
			object IEnumerator.Current => mCurrent;

			#endregion

			#region Public methods and functions

			/// <summary>
			/// Moves to the next item in the list, if present and returns true. If no more items are present, false is returned.
			/// </summary>
			/// <returns>Boolean, true if the enumerator has moved to the next item</returns>
			public bool MoveNext()
			{
				if (mIndex < mList.mSize)
				{
					mCurrent = mList[mIndex];
					mIndex++;
					return true;
				}
				return MoveNextRare();
			}

			/// <summary>
			/// Moves to the next item asynchronously.
			/// </summary>
			/// <returns>A <see cref="ValueTask{Boolean}"/></returns>
			public ValueTask<bool> MoveNextAsync()
			{
				if (mToken.HasValue && mToken.Value.IsCancellationRequested)
				{
					return ValueTask.FromCanceled<bool>(mToken.Value);
				}
				return ValueTask.FromResult(MoveNext());
			}

			#endregion

			#region Private methods and functions

			/// <summary>
			/// Handles the occasion where the enumerator has reached the end of the list.
			/// </summary>
			/// <returns>False</returns>
			private bool MoveNextRare()
			{
				mIndex = mList.mSize + 1;
				mCurrent = default;
				return false;
			}

			/// <summary>
			/// Moves the enumerator to the beginning of the list, setting <see cref="Current"/> to null.
			/// </summary>
			void IEnumerator.Reset()
			{
				mIndex = 0;
				mCurrent = default;
			}
			#endregion
		}
		#endregion
	}
}