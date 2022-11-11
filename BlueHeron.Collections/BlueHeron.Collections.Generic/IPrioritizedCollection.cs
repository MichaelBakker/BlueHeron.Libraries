
namespace BlueHeron.Collections.Generic;

/// <summary>
/// A collection that returns items based on their priority.
/// </summary>
/// <typeparam name="P">The type of the key</typeparam>
/// <typeparam name="V">The type of the value</typeparam>
public interface IPrioritizedCollection<P, V>
{
    /// <summary>
    /// Returns the number of elements in the collection.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Determines whether there are one or more elements in the collection.
    /// </summary>
    bool HasElements { get; }

    /// <summary>
    /// Adds an element to the collection.
    /// </summary>
    /// <param name="priority">The priority</param>
    /// <param name="value">The element</param>
    void Add(P priority, V value);

    /// <summary>
    /// Retrieves the next item from the collection.
    /// </summary>
    V Get();
}