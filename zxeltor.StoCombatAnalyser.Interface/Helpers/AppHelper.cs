// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.IO;
using Microsoft.Win32;

namespace zxeltor.StoCombatAnalyzer.Interface.Helpers;

public static class AppHelper
{
    /// <summary>
    ///     Attempt to get the STO log folder using a Windows registry key.
    /// </summary>
    /// <param name="stoLogFolderPath">The STO log folder path.</param>
    /// <returns>True is successful. False otherwise.</returns>
    public static bool TryGetStoBaseLogFolder(out string? stoLogFolderPath)
    {
        // Attempt to get a windows registry key with STO install location i.e. "HKEY_CURRENT_USER\Software\Cryptic\Star Trek Online"
        var crypticInstallLocation = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Cryptic\Star Trek Online",
            "InstallLocation", null);

        if (crypticInstallLocation != null)
        {
            // Append the log folder to the root folder string.
            var logPath = Path.Combine(crypticInstallLocation.ToString()!.Replace("/", "\\"),
                @"Star Trek Online\Live\logs\GameClient");
            // Confirm the folder exists
            if (Directory.Exists(logPath))
            {
                // If we get here we succeeded. Set the folder path and return
                stoLogFolderPath = logPath;
                return true;
            }
        }

        stoLogFolderPath = null;
        return false;
    }
}