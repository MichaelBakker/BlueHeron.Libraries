using BlueHeron.Collections.Generic;
using System.Collections.Generic;
using System.Linq;

namespace BlueHeron.Collections
{
	/// <summary>
	/// Extension functions for collections.
	/// </summary>
	public static class Extensions
	{
		#region Collections

		/// <summary>
		/// Gets the internal array of a <see cref="List{T}"/>.
		/// </summary>
		/// <typeparam name="T">The type of the elements.</typeparam>
		/// <param name="list">The respective mList.</param>
		/// <returns>The internal array of the mList.</returns>
		public static T[] GetArray<T>(this List<T> list)
		{
			return list.ToArray();
		}

		/// <summary>
		/// Returns the mList as a typed array.
		/// </summary>
		/// <typeparam name="T">The type of the lements in the mList</typeparam>
		/// <param name="list">An <see cref="IList{T}"/></param>
		/// <returns>A array of elements of type T</returns>
		public static T[] GetArrayByType<T>(this IList<T> list)
		{
			T[] array;
			if (list is T[] t)
			{
				array = t;
			}
			else if (list is FastList<T> f)
			{
				array = f.Items;
			}
			else
			{
				array = list.ToArray();
			}
			return array;
		}

		/// <summary>
		/// Tries to get a value from a <see cref="IDictionary{K,V}"/>.
		/// </summary>
		/// <typeparam name="K">The type of the key</typeparam>
		/// <typeparam name="V">The type of the value</typeparam>
		/// <param name="dict">The respective dictionary</param>
		/// <param name="key">The respective key</param>
		/// <returns>The value if it exists, else <c>null</c>.</returns>
		public static V Get<K, V>(this IDictionary<K, V> dict, K key)
		{
			if (dict.TryGetValue(key, out var val))
			{
				return val;
			}
			return default;
		}

		#endregion
	}
}
