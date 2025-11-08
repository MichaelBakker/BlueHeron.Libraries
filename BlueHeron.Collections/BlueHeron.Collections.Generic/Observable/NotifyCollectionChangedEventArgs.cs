using System;
using System.Collections.Specialized;
using System.Runtime.InteropServices;

namespace BlueHeron.Collections.Generic;

/// <summary>
/// Event arguments for the <see cref="ObservableDictionary{TKey, TValue}"/>
/// </summary>
/// <typeparam name="T">The type of the object in the collection</typeparam>
/// <remarks>
/// Creates a new <see cref="NotifyCollectionChangedEventArgs"/>.
/// </remarks>
/// <param name="action">The <see cref="NotifyCollectionChangedAction"/> that was performed</param>
/// <param name="newItem">The new item</param>
/// <param name="oldItem">The old item</param>
[StructLayout(LayoutKind.Auto)]
public readonly struct NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction action, T? newItem = default, T? oldItem = default)
{
	/// <summary>
	/// The <see cref="NotifyCollectionChangedAction"/> that was performed.
	/// </summary>
	public readonly NotifyCollectionChangedAction Action = action;
	/// <summary>
	/// The new item.
	/// </summary>
	public readonly T? NewItem = newItem;
	/// <summary>
	/// The old item.
	/// </summary>
	public readonly T? OldItem = oldItem;

    /// <summary>
    /// Converts this object to a <see cref="NotifyCollectionChangedEventArgs"/> object.
    /// </summary>
    /// <returns>A <see cref="NotifyCollectionChangedEventArgs"/> object</returns>
    /// <exception cref="ArgumentOutOfRangeException">Invalid <see cref="NotifyCollectionChangedAction"/> value (i.e. <see cref="NotifyCollectionChangedAction.Move"/>)</exception>
    public NotifyCollectionChangedEventArgs ToStandardEventArgs()
	{
		return Action switch
		{
			NotifyCollectionChangedAction.Add => new NotifyCollectionChangedEventArgs(Action, NewItem),
			NotifyCollectionChangedAction.Remove => new NotifyCollectionChangedEventArgs(Action, OldItem),
			NotifyCollectionChangedAction.Replace => new NotifyCollectionChangedEventArgs(Action, NewItem, OldItem),
			NotifyCollectionChangedAction.Reset => new NotifyCollectionChangedEventArgs(Action),
			_ => throw new ArgumentOutOfRangeException(),
		};
	}

	/// <summary>
	/// Creates a new <see cref="NotifyCollectionChangedEventArgs{T}"/> that represents an <see cref="NotifyCollectionChangedAction.Add"/> operation.
	/// </summary>
	/// <param name="newItem">The added item</param>
	/// <returns>A <see cref="NotifyCollectionChangedEventArgs{T}"/></returns>
	public static NotifyCollectionChangedEventArgs<T> Add(T newItem)
	{
		return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Add, newItem);
	}

	/// <summary>
	/// Creates a new <see cref="NotifyCollectionChangedEventArgs{T}"/> that represents a <see cref="NotifyCollectionChangedAction.Remove"/> operation.
	/// </summary>
	/// <param name="oldItem">The removed item</param>
	/// <returns>A <see cref="NotifyCollectionChangedEventArgs{T}"/></returns>
	public static NotifyCollectionChangedEventArgs<T> Remove(T oldItem)
	{
		return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Remove, oldItem: oldItem);
	}

	/// <summary>
	/// Creates a new <see cref="NotifyCollectionChangedEventArgs{T}"/> that represents a <see cref="NotifyCollectionChangedAction.Replace"/> operation.
	/// </summary>
	/// <param name="oldItem">The old item</param>
	/// <param name="newItem">The new item</param>
	/// <returns>A <see cref="NotifyCollectionChangedEventArgs{T}"/></returns>
	public static NotifyCollectionChangedEventArgs<T> Replace(T newItem, T oldItem)
	{
		return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Replace, newItem, oldItem);
	}

	/// <summary>
	/// Creates a new <see cref="NotifyCollectionChangedEventArgs{T}"/> that represents a <see cref="NotifyCollectionChangedAction.Reset"/> operation.
	/// </summary>
	/// <returns>A <see cref="NotifyCollectionChangedEventArgs{T}"/></returns>
	public static NotifyCollectionChangedEventArgs<T> Reset()
	{
		return new NotifyCollectionChangedEventArgs<T>(NotifyCollectionChangedAction.Reset);
	}
}