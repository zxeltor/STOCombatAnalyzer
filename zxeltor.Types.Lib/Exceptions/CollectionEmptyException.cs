// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

namespace zxeltor.Types.Lib.Exceptions;

/// <summary>
///     An exception to throw when you can't handle an empty collection.
/// </summary>
public class CollectionEmptyException : Exception
{
    #region Constructors
    /// <inheritdoc />
    public CollectionEmptyException()
    {
    }

    /// <inheritdoc />
    public CollectionEmptyException(string memberName) : base()
    {
        MemberName = memberName;
    }

    /// <inheritdoc />
    public CollectionEmptyException(string message, string memberName) : base(message)
    {
        this.MemberName = memberName;
    }

    /// <inheritdoc />
    public CollectionEmptyException(string message, string memberName, Exception exception) : base(message)
    {
        this.MemberName = memberName;
    }

    /// <inheritdoc />
    public CollectionEmptyException(string memberName, Exception exception) : base(exception.Message, exception)
    {
        this.MemberName = memberName;
    }

    #endregion

    #region Public Properties

    /// <summary>
    ///     The member name of the offending collection.
    /// </summary>
    public string? MemberName { get; set; }

    #endregion
}