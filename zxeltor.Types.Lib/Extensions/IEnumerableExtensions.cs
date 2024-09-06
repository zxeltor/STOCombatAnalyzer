// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using zxeltor.Types.Lib.Collections;

namespace zxeltor.Types.Lib.Extensions;

public static class IEnumerableExtensions
{
    #region Public Members

    /// <summary>
    ///    An extension method to convert an IEnumerable to a <see cref="SyncNotifyCollection{T}"/>
    /// </summary>
    /// <returns><see cref="SyncNotifyCollection{T}"/></returns>
    public static SyncNotifyCollection<TSource> ToSyncNotifyCollection<TSource>(this IEnumerable<TSource> collection)
    {
        return new SyncNotifyCollection<TSource>(collection.ToList());
    }

    #endregion
}