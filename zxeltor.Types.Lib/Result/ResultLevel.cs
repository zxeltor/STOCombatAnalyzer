// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

namespace zxeltor.Types.Lib.Result;

public enum ResultLevel
{
    /// <summary>
    ///     Debug information. This will set Success to true in our Result.
    /// </summary>
    Debug,
    /// <summary>
    ///     Information. This will set Success to true in our Result.
    /// </summary>
    Info,
    /// <summary>
    ///     Warning. This will set Success to true in our Result.
    /// </summary>
    Warning,
    /// <summary>
    ///     Error. This will set Success to false in the Result.
    /// </summary>
    Error,
    /// <summary>
    ///     Halt. This will set Success to false in the Result.
    ///     <para>This would signify the current process should be stopped.</para>
    /// </summary>
    Halt
    //,
    ///// <summary>
    /////     Fatal. This will set Success to false in the Result.
    /////     <para>This would signify the application should be stopped.</para>
    ///// </summary>
    //Fatal
}