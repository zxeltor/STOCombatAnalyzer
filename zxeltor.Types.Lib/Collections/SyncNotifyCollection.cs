// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace zxeltor.Types.Lib.Collections;

/// <summary>
///     An implementation of <see cref="ICollection{T}" />. This collection uses an instance of
///     <see cref="ReaderWriterLockSlim" />, to
///     synchronize all methods and properties which read or modify the internal collection. This collection also
///     implements
///     <see cref="INotifyPropertyChanged" /> and <see cref="INotifyCollectionChanged" />
///     <para>Note: This class is meant to be a thread safe alternative to <see cref="ObservableCollection{T}" /></para>
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class SyncNotifyCollection<T> : ICollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    #region Private Fields

    /// <summary>
    ///     Used to sync all reading and writing to the internal collection.
    /// </summary>
    private readonly ReaderWriterLockSlim _cacheLock;

    /// <summary>
    ///     The internal collection.
    /// </summary>
    private readonly List<T> _syncList;

    #endregion

    #region Constructors

    /// <summary>
    ///     Initializes a new instance of the <see cref="SyncNotifyCollection{T}" />> class that is empty and has the default
    ///     initial capacity.
    /// </summary>
    public SyncNotifyCollection()
    {
        this._cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        this._syncList = new List<T>();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SyncNotifyCollection{T}" />> class that is empty and has the specified
    ///     initial capacity.
    /// </summary>
    /// <param name="count"></param>
    public SyncNotifyCollection(int count)
    {
        this._cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        this._syncList = new List<T>(count);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SyncNotifyCollection{T}" />> class that contains elements copied from
    ///     the specified collection and has sufficient capacity to accommodate the number of elements copied.
    /// </summary>
    /// <param name="syncList"></param>
    public SyncNotifyCollection(IEnumerable<T> syncList)
    {
        this._cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        this._syncList = new List<T>(syncList);
    }

    #endregion

    #region Public Members

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Other Members

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        this.OnPropertyChanged(propertyName);
        return true;
    }

    #endregion

    #region Implementation of IEnumerable

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator()
    {
        this._cacheLock.EnterReadLock();
        try
        {
            foreach (var item in this._syncList) yield return item;
        }
        finally
        {
            this._cacheLock.ExitReadLock();
        }
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    #endregion

    #region Implementation of ICollection<T>

    /// <inheritdoc />
    public void Add(T item)
    {
        this._cacheLock.EnterWriteLock();
        try
        {
            this._syncList.Add(item);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }
        finally
        {
            this._cacheLock.ExitWriteLock();
        }
    }

    public void AddRange(IEnumerable<T> collection)
    {
        this._cacheLock.EnterWriteLock();
        try
        {
            foreach (var item in collection) this._syncList.Add(item);

            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        finally
        {
            this._cacheLock.ExitWriteLock();
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        this._cacheLock.EnterWriteLock();
        try
        {
            this._syncList.Clear();
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        finally
        {
            this._cacheLock.ExitWriteLock();
        }
    }

    /// <inheritdoc />
    public bool Contains(T item)
    {
        this._cacheLock.EnterReadLock();
        try
        {
            return this._syncList.Contains(item);
        }
        finally
        {
            this._cacheLock.ExitReadLock();
        }
    }

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex)
    {
        this._cacheLock.EnterWriteLock();
        try
        {
            this._syncList.CopyTo(array, arrayIndex);
        }
        finally
        {
            this._cacheLock.ExitWriteLock();
        }
    }

    /// <inheritdoc />
    public bool Remove(T item)
    {
        this._cacheLock.EnterWriteLock();
        try
        {
            var result = this._syncList.Remove(item);
            if (result)
                this.OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));

            return this._syncList.Remove(item);
        }
        finally
        {
            this._cacheLock.ExitWriteLock();
        }
    }

    /// <inheritdoc />
    public int Count
    {
        get
        {
            this._cacheLock.EnterReadLock();
            try
            {
                return this._syncList.Count;
            }
            finally
            {
                this._cacheLock.ExitReadLock();
            }
        }
    }

    /// <inheritdoc />
    public bool IsReadOnly => false;

    #endregion

    #region Implementation of INotifyCollectionChanged

    /// <inheritdoc />
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    private void OnCollectionChanged(NotifyCollectionChangedEventArgs arg)
    {
        this.CollectionChanged?.Invoke(this, arg);
    }

    #endregion
}