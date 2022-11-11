using System.Collections.Generic;
using System.Linq;

namespace BlueHeron.Collections.Generic;

/// <summary>
/// A queue that pops items, based on their priority.
/// </summary>
/// <typeparam name="TKey">The type of the key by which to sort items.</typeparam>
/// <typeparam name="TValue">The type of the objects in the stack</typeparam>
public class PrioritizedQueue<TKey, TValue> : IPrioritizedCollection<TKey, TValue>
{
    #region Objects and variables

    private readonly SortedDictionary<TKey, Queue<TValue>> mQueues = new();

    #endregion

    #region Properties

    /// <summary>
    /// Returns the number of <see cref="Queue{T}"/>s in this collection.
    /// </summary>
    public int Count => mQueues.Count;

    /// <summary>
    /// Determines whether there are one or more elements in the queue.
    /// </summary>
    public bool HasElements => mQueues.Any();

    #endregion

    #region Public methods and functions

    /// <summary>
    /// Adds an element of type <typeparamref name="TValue"/> to the queue.
    /// </summary>
    /// <param name="priority">The priority of the element</param>
    /// <param name="value">The element</param>
    public void Add(TKey priority, TValue value)
    {
        Queue<TValue> queue;
        if (mQueues.ContainsKey(priority))
        {
            queue = mQueues[priority];
        }
        else
        {
            queue = new Queue<TValue>();
            mQueues.Add(priority, queue);
        }
        queue.Enqueue(value);
    }

    /// <summary>
    /// Pops the next element of type <typeparamref name="TValue"/> from the queue.
    /// </summary>
    /// <returns>A <typeparamref name="TValue"/></returns>
    public TValue Get()
    {
        if (!HasElements)
        {
            return default;
        }

        var pair = mQueues.First();
        var queue = pair.Value;
        var value = queue.Dequeue();
        if (queue.Count == 0)
        {
            mQueues.Remove(pair.Key);
        }

        return value;
    }

    #endregion
}