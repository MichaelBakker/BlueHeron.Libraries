using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BlueHeron.Collections.Generic;

namespace BlueHeron.Collections
{
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
				return list.ToArray();
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
		public static void Sort<T>(this ObservableCollection<T> collection, Comparison<T> comparison)
		{
			var sortableList = new List<T>(collection);
			sortableList.Sort(comparison);

			for (int i = 0; i < sortableList.Count; i++)
			{
				collection.Move(collection.IndexOf(sortableList[i]), i);
			}
		}

		#endregion
	}
}