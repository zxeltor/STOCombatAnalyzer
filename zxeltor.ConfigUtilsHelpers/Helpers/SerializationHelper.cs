// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using Newtonsoft.Json;

namespace zxeltor.ConfigUtilsHelpers.Helpers;

/// <summary>
///     A collection of helpers to serialize or deserialize object to or from JSON.
///     <para>The helpers in this class require <see cref="Newtonsoft" /></para>
/// </summary>
public static class SerializationHelper
{
    /// <summary>
    ///     Deserialize a JSON string as a chosen type.
    /// </summary>
    /// <typeparam name="T">The chosen type</typeparam>
    /// <param name="data">The JSON string to deserialize.</param>
    /// <returns>An object of the chosen type.</returns>
    public static T Deserialize<T>(string data)
    {
        return JsonConvert.DeserializeObject<T>(data);
    }

    /// <summary>
    ///     Serialize a chosen type to a JSON string.
    /// </summary>
    /// <param name="data">The data object to serialize.</param>
    /// <returns>A JSON string representation of the data object.</returns>
    public static string Serialize(object data)
    {
        return JsonConvert.SerializeObject(data);
    }

    /// <summary>
    ///     Try to deserialize a JSON string as a chosen type.
    /// </summary>
    /// <typeparam name="T">The chosen type</typeparam>
    /// <param name="data">The JSON string to deserialize.</param>
    /// <param name="output">An object of the chosen type.</param>
    /// <returns>True if successful. False otherwise.</returns>
    public static bool TryDeserializeString<T>(string data, out T output)
        where T : class
    {
        output = null;

        if (data == null)
            return false;

        try
        {
            output = JsonConvert.DeserializeObject<T>(data);
            return true;
        }
        catch
        {
            output = default;
        }

        return false;
    }

    /// <summary>
    ///     Try to serialize a chosen type to a JSON string.
    /// </summary>
    /// <param name="data">The data object to serialize.</param>
    /// <param name="output">A JSON string representation of the data object</param>
    /// <returns>True if successful. False otherwise.</returns>
    public static bool TrySerialize(object data, out string output)
    {
        try
        {
            output = JsonConvert.SerializeObject(data);
            return true;
        }
        catch
        {
            output = default;
        }

        return false;
    }
}