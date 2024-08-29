// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace zxeltor.Types.Lib.Collections;

/// <summary>
///     This class is an implementation of <see cref="ObservableCollection{T}" />.
///     <para>Note: This class provides an AddRange method to add multiple items at once.</para>
/// </summary>
/// <typeparam name="T"></typeparam>
public class LargeObservableCollection<T> : ObservableCollection<T>
{
    #region Constructors

    /// <summary>
    ///     An implementation to initialize the base class constructor.
    ///     <see cref="ObservableCollection{T}" />
    /// </summary>
    public LargeObservableCollection()
    {
    }

    /// <summary>
    ///     An implementation to initialize the base class constructor.
    ///     <see cref="ObservableCollection{T}" />
    /// </summary>
    public LargeObservableCollection(IEnumerable<T> enumerable) : base(enumerable)
    {
    }

    /// <summary>
    ///     An implementation to initialize the base class constructor.
    ///     <see cref="ObservableCollection{T}()" />
    /// </summary>
    public LargeObservableCollection(List<T> list) : base(list)
    {
    }

    #endregion

    #region Public Members

    /// <summary>
    ///     Add a collection of <see cref="IEnumerable{T}" /> to the end of the existing collection
    ///     <para>
    ///         Note: The method doesn't trigger the <see cref="INotifyCollectionChanged" /> event until after all items have
    ///         been
    ///         added.
    ///     </para>
    /// </summary>
    /// <param name="collection">The collection of items to add.</param>
    public void AddRange(IEnumerable<T> collection)
    {
        foreach (var i in collection) this.Items.Add(i);
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    #endregion
}