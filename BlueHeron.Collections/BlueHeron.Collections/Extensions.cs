using System.Collections.Generic;
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
	}
}