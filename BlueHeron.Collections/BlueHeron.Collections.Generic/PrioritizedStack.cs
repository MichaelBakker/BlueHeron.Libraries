using System.Collections.Generic;
using System.Linq;

namespace BlueHeron.Collections.Generic;

/// <summary>
/// A stack that pops items, based on their priority.
/// </summary>
/// <typeparam name="TKey">The type of the key by which to sort items.</typeparam>
/// <typeparam name="TValue">The type of the objects in the stack</typeparam>
public class PrioritizedStack<TKey, TValue> : IPrioritizedCollection<TKey, TValue> where TKey : notnull where TValue : notnull
{
    #region Objects and variables

    private readonly SortedDictionary<TKey, Stack<TValue>> mStacks = [];

    #endregion

    #region Properties

    /// <summary>
    /// Returns the number of <see cref="Stack{T}"/>s in this collection.
    /// </summary>
    public int Count => mStacks.Count;

    /// <summary>
    /// Determines whether there are one or more elements in the stack.
    /// </summary>
    public bool HasElements => mStacks.Count != 0;

    #endregion

    #region Public methods and functions

    /// <summary>
    /// Adds an element of type <typeparamref name="TValue"/> to the stack.
    /// </summary>
    /// <param name="priority">The priority of the element</param>
    /// <param name="value">The element</param>
    public void Add(TKey priority, TValue value)
    {
        Stack<TValue> stack;
        if (mStacks.TryGetValue(priority, out var value1))
        {
            stack = value1;
        }
        else
        {
            stack = new Stack<TValue>();
            mStacks.Add(priority, stack);
        }
        stack.Push(value);
    }

    /// <summary>
    /// Pops the next element of type <typeparamref name="TValue"/> from the stack.
    /// </summary>
    /// <returns>A <typeparamref name="TValue"/></returns>
    public TValue Get()
    {
        if (!HasElements)
        {
            return default!;
        }

        var pair = mStacks.First();
        var stack = pair.Value;
        var value = stack.Pop();
        if (stack.Count == 0)
        {
            mStacks.Remove(pair.Key);
        }

        return value;
    }

    #endregion
}