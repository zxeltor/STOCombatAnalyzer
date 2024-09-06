// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Diagnostics;

namespace zxeltor.Types.Lib.Helpers;

/// <summary>
///     A collection of Url related helper methods.
/// </summary>
public static class UrlHelper
{
    #region Public Methods and Operators

    /// <summary>
    ///     Attempt to combine string segments using forward slashes between each segment to make a valid Url.
    ///     <para>
    ///         Note: This is a simple string join, using a forward slash between each segment. If trying to construct an
    ///         absolute Url with http protocol,
    ///         host, and port (i.e. http://localhost:90), they all need to be in included in the first segment.
    ///     </para>
    /// </summary>
    /// <param name="segments">A collection of Url segments</param>
    /// <returns>A string of combined url segments.</returns>
    public static string CombineUriSegments(params string[] segments)
    {
        var segmentsResult = string.Join("/", segments.Select(seg => seg.Trim().Trim('/')));
        return segmentsResult;
    }

    /// <summary>
    ///     Attempt to launch the OS default browser with the supplied url.
    /// </summary>
    /// <param name="url">The URL to navigate too.</param>
    public static void LaunchUrlInDefaultBrowser(string url)
    {
        using var process = Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    #endregion
}