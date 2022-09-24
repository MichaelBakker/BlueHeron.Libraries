using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueHeron.Collections.Generic
{
    /// <summary>
    /// Object that maps indexes to names and vice versa, and maps indexes to values.
    /// </summary>
    /// <typeparam name="TIndex">The type of the mIndex</typeparam>
    /// <typeparam name="TName">The type of the name</typeparam>
    /// <typeparam name="TValue">The type of the value</typeparam>
    public sealed class MappingCollection<TIndex, TName, TValue>
    {
        #region Objects and variables

        private readonly Dictionary<TIndex, TName> indexNameMapping = new();
        private readonly Dictionary<TName, TIndex> nameIndexMapping = new();
        private readonly Dictionary<TIndex, TValue> indexDataMapping = new();

        private const string errDuplicateIndex = "Cannot add duplicate mIndex.";
        private const string errDuplicateName = "Cannot add duplicate name.";

        #endregion

        #region Properties

        /// <summary>
        /// Returns the number of mappings in the collection (i.e. indexes to names).
        /// </summary>
        public int Count => indexNameMapping.Count;

        /// <summary>
        /// Returns all values in the collection.
        /// </summary>
        public IEnumerable<TValue> Values => indexDataMapping.Values;

        /// <summary>
        /// Returns all indexes in the collection.
        /// </summary>
        public IEnumerable<TIndex> Keys => indexNameMapping.Keys;

        /// <summary>
        /// Returns the mIndex to name mappings as array of <see cref="KeyValuePair{TIndex, TValue}"/>s.
        /// </summary>
        public KeyValuePair<TIndex, TValue>[] MappingArray { get; private set; } = Array.Empty<KeyValuePair<TIndex, TValue>>();

        /// <summary>
        /// Returns the value with the given key (i.e. mIndex).
        /// </summary>
        /// <param name="key">The <typeparamref name="TIndex"/> of the value</param>
        /// <returns>A <typeparamref name="TValue"/></returns>
        public TValue this[TIndex key] => indexDataMapping[key];

        /// <summary>
        /// Returns the <typeparamref name="TIndex"/> of the given <typeparamref name="TName"/>.
        /// </summary>
        /// <param name="name">The <typeparamref name="TName"/></param>
        /// <returns>The <typeparamref name="TIndex"/></returns>
        public TIndex this[TName name] => nameIndexMapping[name];

        #endregion

        #region Public methods and functions

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="index">The <typeparamref name="TIndex"/></param>
        /// <param name="name">The <typeparamref name="TName"/></param>
        /// <param name="item">The <typeparamref name="TValue"/></param>
        public void Add(TIndex index, TName name, TValue item)
        {
            if (nameIndexMapping.ContainsKey(name))
            {
                throw new ArgumentException(errDuplicateName);
            }
            if (indexNameMapping.ContainsKey(index))
            {
                throw new ArgumentException(errDuplicateIndex);
            }
            indexNameMapping.Add(index, name);
            nameIndexMapping.Add(name, index);
            indexDataMapping.Add(index, item);
            MappingArray = indexDataMapping.ToArray();
        }

        /// <summary>
        /// Clears the collection of all mappings.
        /// </summary>
        public void Clear()
        {
            nameIndexMapping.Clear();
            indexNameMapping.Clear();
            indexDataMapping.Clear();
            MappingArray = Array.Empty<KeyValuePair<TIndex, TValue>>();
        }

        /// <summary>
        /// Determines if a mapping exists for the given <typeparamref name="TIndex"/>.
        /// </summary>
        /// <param name="id">The <typeparamref name="TIndex"/></param>
        /// <returns>True if a mapping exists for the given <typeparamref name="TIndex"/>, else false</returns>
        public bool HasItem(TIndex id)
        {
            return indexNameMapping.ContainsKey(id);
        }

        /// <summary>
        /// Determines if a mapping exists for the given <typeparamref name="TName"/>.
        /// </summary>
        /// <param name="name">The <typeparamref name="TName"/></param>
        /// <returns>True if a mapping exists for the given <typeparamref name="TName"/>, else false</returns>
        public bool HasItem(TName name)
        {
            return nameIndexMapping.ContainsKey(name);
        }

        /// <summary>
        /// Removes the item with the given <typeparamref name="TIndex"/>.
        /// </summary>
        /// <param name="index">The <typeparamref name="TIndex"/></param>
        /// <returns>Boolean, true if the operation was successful</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1853:Unnecessary call to 'Dictionary.ContainsKey(key)'", Justification = "By design")]
        public bool Remove(TIndex index)
        {
            if (indexNameMapping.ContainsKey(index))
            {
                nameIndexMapping.Remove(indexNameMapping[index]);
                indexNameMapping.Remove(index);
                indexDataMapping.Remove(index);
                MappingArray = indexDataMapping.ToArray();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes the item with the given <typeparamref name="TName"/>.
        /// </summary>
        /// <param name="name">The <typeparamref name="TName"/></param>
        /// <returns>Boolean, true if the operation was successful</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1853:Unnecessary call to 'Dictionary.ContainsKey(key)'", Justification = "By design")]
        public bool Remove(TName name)
        {
            if (nameIndexMapping.ContainsKey(name))
            {
                indexNameMapping.Remove(nameIndexMapping[name]);
                indexDataMapping.Remove(nameIndexMapping[name]);
                nameIndexMapping.Remove(name);
                MappingArray = indexDataMapping.ToArray();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to get the <typeparamref name="TValue"/> with the given <typeparamref name="TIndex"/>.
        /// </summary>
        /// <param name="id">The <typeparamref name="TIndex"/></param>
        /// <param name="data">The <typeparamref name="TValue"/> if it exists, else null</param>
        /// <returns>True if the operation was successful</returns>
        public bool TryGetItem(TIndex id, out TValue data)
        {
            return indexDataMapping.TryGetValue(id, out data);
        }

        /// <summary>
        /// Tries to get the <typeparamref name="TValue"/> with the given <typeparamref name="TName"/>.
        /// </summary>
        /// <param name="name">The <typeparamref name="TName"/></param>
        /// <param name="data">The <typeparamref name="TValue"/> if it exists, else null</param>
        /// <returns>True if the operation was successful</returns>
        public bool TryGetItem(TName name, out TValue data)
        {
            if (nameIndexMapping.TryGetValue(name, out var idx) && indexDataMapping.TryGetValue(idx, out data))
            {
                return true;
            }
            else
            {
                data = default;
                return false;
            }
        }

        /// <summary>
        /// Tries to get the <typeparamref name="TName"/> of the mapping with the given <typeparamref name="TIndex"/>.
        /// </summary>
        /// <param name="key">The <typeparamref name="TIndex"/></param>
        /// <param name="name">The <typeparamref name="TName"/></param>
        /// <returns>True if the operation was successful</returns>
        public bool TryGetName(TIndex key, out TName name)
        {
            return indexNameMapping.TryGetValue(key, out name);
        }

        /// <summary>
        /// Tries to get the <typeparamref name="TIndex"/> of the mapping with the given <typeparamref name="TName"/>.
        /// </summary>
        /// <param name="name">The <typeparamref name="TName"/></param>
        /// <param name="key">The <typeparamref name="TIndex"/></param>
        /// <returns>True if the operation was successful</returns>
        public bool TryGetSlot(TName name, out TIndex key)
        {
            return nameIndexMapping.TryGetValue(name, out key);
        }

        #endregion
    }
}