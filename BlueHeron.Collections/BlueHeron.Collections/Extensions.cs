using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BlueHeron.Collections.Generic;

namespace BlueHeron.Collections;

/// <summary>
/// Extension functions for collections.
/// </summary>
public static class Extensions
{
	#region Collections

	/// <summary>
	/// Returns the <see cref="IList{T}"/> as a typed array, optimized for the possibility that the <see cref="IList{T}"/> may be a <see cref="FastList{T}"/> or a <typeparamref name="T"/>[].
	/// </summary>
	/// <typeparam name="T">The type of the elements in the list</typeparam>
	/// <param name="list">An <see cref="IList{T}"/></param>
	/// <returns>A array of elements of type T</returns>
	public static T[] AsTypedArray<T>(this IList<T> list)
	{
		if (list is T[] t)
		{
			return t;
		}
		else if (list is FastList<T> f)
		{
			return f.Items;
		}
		else
		{
			return [.. list];
		}
	}

	#endregion

	#region ObservableCollections

	/// <summary>
	/// Sorts this <see cref="ObservableCollection{T}"/> without breaking any bindings.
	/// </summary>
	/// <typeparam name="T">The <see cref="Type"/> of the element in the collection</typeparam>
	/// <param name="collection">The <see cref="ObservableCollection{T}"/> to sort</param>
	/// <param name="comparison">The <see cref="Comparison{T}"/> to use when sorting the items in the collection</param>
	/// <param name="reBind">Optionally rebind items in the collection where reordering does not update the UI (e.g. TreeViewItem.ItemsSource does not respond to <see cref="ObservableCollection{T}.Move(int, int)"/> actions)</param>
	public static void Sort<T>(this ObservableCollection<T> collection, Comparison<T> comparison, bool reBind = false)
	{
		var sortableList = new List<T>(collection);
		Action<int> action = reBind ? (int i) => collection.Add(sortableList[i]) : (int i) => collection.Move(collection.IndexOf(sortableList[i]), i);

		sortableList.Sort(comparison);

		if (reBind)
		{
			collection.Clear();
		}
		for (var i = 0; i < sortableList.Count; i++)
		{
			action(i);
		}
	}

	#endregion
}