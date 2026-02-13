using System.Collections;
using System.Reflection;

namespace Genocs.Common.Collections;

/// <summary>
/// A shortcut for <see cref="TypeList{TBaseType}"/> to use object as base type.
/// </summary>
public class TypeList : TypeList<object>, ITypeList;

/// <summary>
/// Extends <see cref="List{Type}"/> to add restriction a specific base type.
/// </summary>
/// <typeparam name="TBaseType">Base Type of <see cref="Type"/>s in this list.</typeparam>
public class TypeList<TBaseType> : ITypeList<TBaseType>
{
    /// <summary>
    /// Gets the count.
    /// </summary>
    /// <value>The count.</value>
    public int Count
    {
        get
        {
            return _typeList.Count;
        }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is read only.
    /// </summary>
    /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
    public bool IsReadOnly
    {
        get { return false; }
    }

    /// <summary>
    /// Gets or sets the <see cref="Type"/> at the specified index.
    /// </summary>
    /// <param name="index">Index.</param>
    public Type this[int index]
    {
        get
        {
            return _typeList[index];
        }

        set
        {
            CheckType(value);
            _typeList[index] = value;
        }
    }

    private readonly List<Type> _typeList;

    /// <summary>
    /// Creates a new <see cref="TypeList{T}"/> object.
    /// </summary>
    public TypeList()
    {
        _typeList = [];
    }

    /// <inheritdoc/>
    public void Add<T>()
        where T : TBaseType
    {
        _typeList.Add(typeof(T));
    }

    /// <inheritdoc/>
    public void Add(Type item)
    {
        CheckType(item);
        _typeList.Add(item);
    }

    /// <inheritdoc/>
    public void Insert(int index, Type item)
    {
        _typeList.Insert(index, item);
    }

    /// <inheritdoc/>
    public int IndexOf(Type item)
    {
        return _typeList.IndexOf(item);
    }

    /// <inheritdoc/>
    public bool Contains<T>()
        where T : TBaseType
    {
        return Contains(typeof(T));
    }

    /// <inheritdoc/>
    public bool Contains(Type item)
    {
        return _typeList.Contains(item);
    }

    /// <inheritdoc/>
    public void Remove<T>()
        where T : TBaseType
    {
        _typeList.Remove(typeof(T));
    }

    /// <summary>
    /// Removes the specified type from the collection.
    /// </summary>
    /// <remarks>This method modifies the underlying collection and may affect the state of the collection. It
    /// is important to ensure that the item being removed is not null.</remarks>
    /// <param name="item">The type to remove from the collection. If the type is not found, no changes are made.</param>
    /// <returns>true if the type was successfully removed; otherwise, false.</returns>
    public bool Remove(Type item)
    {
        return _typeList.Remove(item);
    }

    /// <summary>
    /// Removes the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    public void RemoveAt(int index)
    {
        _typeList.RemoveAt(index);
    }

    /// <summary>
    /// Clears all elements from the list.
    /// </summary>
    public void Clear()
    {
        _typeList.Clear();
    }

    /// <summary>
    /// Copies the elements of the collection to the specified one-dimensional array, starting at the specified index of
    /// the target array.
    /// </summary>
    /// <remarks>Throws an ArgumentNullException if the array is null. Throws an ArgumentOutOfRangeException
    /// if arrayIndex is less than zero or greater than the length of the array. Throws an ArgumentException if the
    /// number of elements in the source collection is greater than the available space from arrayIndex to the end of
    /// the destination array.</remarks>
    /// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection. The array must
    /// have zero-based indexing and sufficient space to accommodate the copied elements.</param>
    /// <param name="arrayIndex">The zero-based index in the destination array at which copying begins. Must be within the bounds of the array.</param>
    public void CopyTo(Type[] array, int arrayIndex)
    {
        _typeList.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns>An enumerator for the list of types.</returns>
    public IEnumerator<Type> GetEnumerator()
    {
        return _typeList.GetEnumerator();
    }

    /// <summary>
    /// Gets the enumerator for the list of types.
    /// </summary>
    /// <returns>An enumerator for the list of types.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _typeList.GetEnumerator();
    }

    private static void CheckType(Type item)
    {
        if (!typeof(TBaseType).GetTypeInfo().IsAssignableFrom(item))
        {
            throw new ArgumentException($"Given item is not type of {typeof(TBaseType).AssemblyQualifiedName}", nameof(item));
        }
    }
}
