using System;
using System.Collections.Concurrent;

namespace BlueHeron.Collections.Generic;

/// <summary>
/// Object that is able to create and store a pool of objects.
/// </summary>
/// <typeparam name="T">The type of the objects in the pool</typeparam>
/// <remarks>
/// Creates a new ObjectPool.
/// </remarks>
/// <param name="objectGenerator">The <see cref="Func{T}"/> that creates the object of type <typeparamref name="T"/></param>
/// <param name="maxCapacity">The capacity of the pool</param>
/// <exception cref="ArgumentNullException">Parameter objectGenerator cannot be null</exception>
public sealed class ObjectPool<T>(Func<T> objectGenerator, int maxCapacity = int.MaxValue / 2)
{
    #region Objects and variables

    private readonly ConcurrentBag<T> mObjects = [];
    private readonly Func<T> mObjectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));

    #endregion

    #region Properties

    /// <summary>
    /// Returns the number of objects in the pool.
    /// </summary>
    public int Count => mObjects.Count;

    #endregion

    #region Public methods and functions

    /// <summary>
    /// Removes and returns an object from the pool if it exists. Otherwise a new object is created and returned.
    /// </summary>
    /// <returns>An object of type <typeparamref name="T"/></returns>
    public T Get()
    {
        if (mObjects.TryTake(out var item))
        {
            return item;
        }
        return mObjectGenerator();
    }

    /// <summary>
    /// Stores the given object in the pool.
    /// </summary>
    /// <param name="item">The object to store</param>
    public void Set(T item)
    {
        if (Count < maxCapacity)
        {
            mObjects.Add(item);
        }
    }

    #endregion
}