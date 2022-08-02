using System.Collections.Generic;

namespace BlueHeron.Collections.Generic;

/// <summary>
/// Event handler for change notifications.
/// </summary>
/// <typeparam name="T">The type of the items in the collection</typeparam>
/// <param name="e">A <see cref="NotifyCollectionChangedEventArgs{T}"/> implementation</param>
public delegate void NotifyCollectionChangedEventHandler<T>(in NotifyCollectionChangedEventArgs<T> e);

/// <summary>
/// Interface definition for readonly observable collections.
/// Inherits from <see cref="IReadOnlyCollection{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the object in the collection</typeparam>
public interface IObservableCollection<T> : IReadOnlyCollection<T>
{
#nullable enable
    /// <summary>
    /// Event is fired when the collection has changed or when an item in the collection has changed.
    /// </summary>
    event NotifyCollectionChangedEventHandler<T>? CollectionChanged;
#nullable disable

    /// <summary>
    /// Object, needed to ensure thread safety.
    /// </summary>
    object SyncRoot
    {
        get;
    }
}