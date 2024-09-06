// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Text.RegularExpressions;

namespace zxeltor.Types.Lib.Extensions;

public static class StringExtensions
{
    #region Static Fields and Constants

    /// <summary>
    ///     Special characters to remove from a string. Vertical tab, tab, end of line, and carriage return.
    /// </summary>
    public const string SpecialCharsToRemove = @"\v|\t|\n|\r";

    /// <summary>
    ///     Used to replace special characters defined in <see cref="SpecialCharsToRemove" />
    /// </summary>
    public const string ReplacementCharForSpecialCharsToRemove = "";

    #endregion

    #region Public Members

    /// <summary>
    ///     Removes special characters defined in <see cref="SpecialCharsToRemove" /> with
    ///     <see cref="ReplacementCharForSpecialCharsToRemove" />
    /// </summary>
    /// <param name="str">The string to parse.</param>
    /// <returns>The cleaned up string</returns>
    public static string RemoveSpecialChars(this string str)
    {
        return Regex.Replace(str, SpecialCharsToRemove, ReplacementCharForSpecialCharsToRemove);
    }

    #endregion
}