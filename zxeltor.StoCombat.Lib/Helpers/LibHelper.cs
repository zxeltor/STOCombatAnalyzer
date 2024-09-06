// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.IO;
using Microsoft.Win32;

namespace zxeltor.StoCombat.Lib.Helpers;

public static class LibHelper
{
    #region Static Fields and Constants

    /// <summary>
    ///     A string for the STO combat log folder.
    /// </summary>
    public const string StoCombatLogSubFolder = @"Star Trek Online\Live\logs\GameClient";

    #endregion

    #region Public Members

    /// <summary>
    ///     Attempt to get the STO base folder using a Windows registry key.
    /// </summary>
    /// <param name="stoBaseFolderPath">The STO base folder path.</param>
    /// <returns>True is successful. False otherwise.</returns>
    public static bool TryGetStoBaseFolder(out string stoBaseFolderPath)
    {
        // Attempt to get a windows registry key with STO install location i.e. "HKEY_CURRENT_USER\Software\Cryptic\Star Trek Online"
        var crypticInstallLocation = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Cryptic\Star Trek Online",
            "InstallLocation", null);

        if (crypticInstallLocation != null)
        {
            var stoBaseFolder = crypticInstallLocation.ToString()!.Replace("/", "\\");

            // Confirm the folder exists
            if (Directory.Exists(stoBaseFolder))
            {
                // If we get here we succeeded. Set the folder path and return
                stoBaseFolderPath = stoBaseFolder;
                return true;
            }
        }

        stoBaseFolderPath = string.Empty;
        return false;
    }

    /// <summary>
    ///     Attempt to get the STO log folder using a Windows registry key.
    /// </summary>
    /// <param name="stoLogFolderPath">The STO log folder path.</param>
    /// <returns>True is successful. False otherwise.</returns>
    public static bool TryGetStoBaseLogFolder(out string stoLogFolderPath)
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

        stoLogFolderPath = string.Empty;
        return false;
    }

    #endregion
}