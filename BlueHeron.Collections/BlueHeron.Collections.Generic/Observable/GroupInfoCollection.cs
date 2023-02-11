using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BlueHeron.Collections.Generic;

/// <summary>
/// An <see cref="ObservableCollection{T}"/> with a key property, useful in grouping operations.
/// </summary>
/// <typeparam name="T">The <see cref="System.Type"/> of the elements in the collection</typeparam>
public class GroupInfoCollection<T> : ObservableCollection<T>
{
    /// <summary>
    /// The key of this collection.
    /// </summary>
    public object Key { get; set; }

    /// <summary>
    /// Returns the <see cref="IEnumerator{T}"/> for this collection.
    /// </summary>
    /// <returns>An <see cref="IEnumerator{T}"/></returns>
    public new IEnumerator<T> GetEnumerator() => (IEnumerator<T>)base.GetEnumerator();
}