using System;
using System.Collections.ObjectModel;

namespace BlueHeron.Collections.Generic;

/// <summary>
/// An <see cref="ObservableCollection{T}"/> with a key property, useful in grouping operations.
/// </summary>
/// <typeparam name="T">The <see cref="System.Type"/> of the elements in the collection</typeparam>
public class GroupInfoCollection<TKey, TValue>(TKey key) : ObservableCollection<TValue>
{
    /// <summary>
    /// The key of this collection.
    /// </summary>
    public TKey Key { get; set; } = key ?? throw new ArgumentNullException(nameof(key));
}