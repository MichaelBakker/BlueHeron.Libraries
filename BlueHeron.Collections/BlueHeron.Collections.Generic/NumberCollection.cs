using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace BlueHeron.Collections.Generic;

/// <summary>
/// A <see cref="FastList{T}"/> implementation where <typeparamref name="T"/> is an <see cref="INumber{TSelf}"/> implementation.
/// </summary>
/// <typeparam name="T">The type of the <see cref="INumber{TSelf}"/></typeparam>
public sealed class NumberCollection<T> : FastList<T> where T : INumber<T>
{
    #region Objects and variables

    private const char _SPACE = ' ';

    #endregion

    #region Construction

    /// <summary>
    /// Creates a new, empty NumberCollection.
    /// </summary>
    public NumberCollection() { }

    /// <summary>
    /// Creates a new, empty NumberCollection with the given capacity.
    /// </summary>
    /// <param name="capacity">The capacity of the list</param>
    public NumberCollection(int capacity) : base(capacity) { }

    /// <summary>
    /// Creates a new NumberCollection, based on the given collection of values.
    /// </summary>
    /// <param name="items">The <see cref="IEnumerable{T}"/> on which to base the collection</param>
    public NumberCollection(IEnumerable<T> items) : base(items) { }

    #endregion

    #region Public methods and functions

    /// <summary>
    /// Parses the given string into this collection.
    /// </summary>
    /// <param name="source">A separated string of values</param>
    /// <returns>An <see cref="NumberCollection{T}"/></returns>
    public static NumberCollection<T> Parse(string source)
    {
        IFormatProvider formatProvider = CultureInfo.InvariantCulture;
        var th = new StringTokenizer(source, formatProvider);
        var resource = new NumberCollection<T>();
        while (th.NextToken())
        {
            var value = T.Parse(th.GetCurrentToken(), formatProvider);
            resource.Add(value);
        }
        return resource;
    }

    /// <summary>
    /// Parses this collection into a formatted string of values.
    /// </summary>
    /// <param name="format">The format in which to parse values</param>
    /// <param name="provider">The <see cref="IFormatProvider"/> to use</param>
    /// <returns>A <see cref="string"/></returns>
    public new string ToString(string format, IFormatProvider? provider)
    {
        if (Count == 0)
        {
            return string.Empty;
        }

        var str = new StringBuilder();
        for (var i = 0; i < Count; i++)
        {
            str.AppendFormat(provider, $"{{0:{format}}}", this[i]);
            if (i != Count - 1)
            {
                str.Append(_SPACE);
            }
        }
        return str.ToString();
    }

    #endregion
}