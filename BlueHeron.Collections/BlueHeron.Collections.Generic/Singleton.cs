using System;
using System.Collections.Concurrent;

namespace BlueHeron.Collections.Generic;

/// <summary>
/// A simple container for singleton objects.
/// </summary>
/// <typeparam name="T">Yhe <see cref="Type"/> of the object</typeparam>
public static class Singleton<T> where T : new()
{
    #region Objects and variables

    private static readonly ConcurrentDictionary<Type, T> mInstances = new();

    #endregion

    #region Properties

    /// <summary>
    /// Returns the instance of the given type.
    /// </summary>
    public static T Instance => mInstances.GetOrAdd(typeof(T), (t) => new T());

    #endregion
}