// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace zxeltor.Types.Lib.Collections;

public sealed class SyncNotifyCollection<T> : ICollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    #region Private Fields

    private int _count;
    private bool _isReadOnly;
    private readonly List<T> _syncList;
    private readonly ReaderWriterLockSlim cacheLock = new(LockRecursionPolicy.SupportsRecursion);

    #endregion

    #region Constructors

    public SyncNotifyCollection()
    {
        this._syncList = new List<T>();
    }

    public SyncNotifyCollection(int count)
    {
        this._syncList = new List<T>(count);
    }

    public SyncNotifyCollection(IEnumerable<T> syncList)
    {
        this._syncList = new List<T>(syncList);
        //OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    #endregion

    #region Public Members

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
            this.cacheLock.EnterReadLock();
            try
            {
                foreach (var item in this._syncList) yield return item;
            }
            finally
            {
                this.cacheLock.ExitReadLock();
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
            this.cacheLock.EnterWriteLock();
            try
            {
                this._syncList.Add(item);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            }
            finally
            {
                this.cacheLock.ExitWriteLock();
            }
    }

    public void AddRange(IEnumerable<T> collection)
    {
            this.cacheLock.EnterWriteLock();
            try
            {
                foreach (var item in collection) this._syncList.Add(item);

                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            finally
            {
                this.cacheLock.ExitWriteLock();
            }
    }

    /// <inheritdoc />
    public void Clear()
    {
            this.cacheLock.EnterWriteLock();
            try
            {
                this._syncList.Clear();
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            finally
            {
                this.cacheLock.ExitWriteLock();
            }
    }

    /// <inheritdoc />
    public bool Contains(T item)
    {
            this.cacheLock.EnterReadLock();
            try
            {
                return this._syncList.Contains(item);
            }
            finally
            {
                this.cacheLock.ExitReadLock();
            }
    }

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex)
    {
            this.cacheLock.EnterWriteLock();
            try
            {
                this._syncList.CopyTo(array, arrayIndex);
            }
            finally
            {
                this.cacheLock.ExitWriteLock();
            }
    }

    /// <inheritdoc />
    public bool Remove(T item)
    {
            this.cacheLock.EnterWriteLock();
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
                this.cacheLock.ExitWriteLock();
            }
    }

    /// <inheritdoc />
    public int Count
    {
        get
        {
                this.cacheLock.EnterReadLock();
                try
                {
                    return this._syncList.Count;
                }
                finally
                {
                    this.cacheLock.ExitReadLock();
                }
        }
    }

    /// <inheritdoc />
    public bool IsReadOnly => false; //this._isReadOnly;

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