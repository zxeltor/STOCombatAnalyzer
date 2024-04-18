// Copyright (c) 2024, zxeltor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

namespace zxeltor.ConfigUtilsHelpers.Helpers;

/// <summary>
///     Class to assist with Enum operations
/// </summary>
/// <remarks>
///     Initially created to support string to int enum conversion as an endpoint parameter
/// </remarks>
public static class EnumHelper
{
    #region Public Methods and Operators

    /// <summary>
    ///     Convert a string for an enum value to the integer value of the enum.
    /// </summary>
    /// <remarks>
    ///     Will perform a case insensitive comparison of the enumTitle to enum value.
    /// </remarks>
    /// <typeparam name="T">Enum type to convert.</typeparam>
    /// <param name="enumTitle">Title to convert and get the enum value for.</param>
    /// <returns>Matched enum value</returns>
    public static T ConvertStringToEnum<T>(string enumTitle)
        where T : Enum
    {
        return (T)Enum.Parse(typeof(T), enumTitle, true);
    }

    /// <summary>
    ///     Try to convert a string for an enum value to the integer value of the enum.
    /// </summary>
    /// <remarks>
    ///     Will perform a case insensitive comparison of the enumTitle to enum value.
    /// </remarks>
    /// <typeparam name="T">Enum type to convert.</typeparam>
    /// <param name="enumTitle">Title to convert and get the enum value for.</param>
    /// <param name="outputEnum">Matched enum value.</param>
    /// <returns>True if successful. False otherwise.</returns>
    public static bool TryConvertStringToEnum<T>(string enumTitle, out T? outputEnum)
        where T : Enum
    {
        outputEnum = default;

        try
        {
            outputEnum = ConvertStringToEnum<T>(enumTitle);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}