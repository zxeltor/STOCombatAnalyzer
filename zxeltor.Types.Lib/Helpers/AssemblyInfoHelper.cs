// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Reflection;

namespace zxeltor.Types.Lib.Helpers;

/// <summary>
///     A collection of static helpers used to retrieve assembly information from the entry assembly of the current
///     application.
/// </summary>
public static class AssemblyInfoHelper
{
    /// <summary>
    ///     Get an application name based on the file name of the entry assembly (without file extension) for the current
    ///     application.
    ///     <para>If assembly info is not available, a default of <see cref="zxeltor.ConfigUtilsHelpers.Constants.ApplicationNameDefault" /> is returned.</para>
    /// </summary>
    /// <returns>The application name.</returns>
    public static string GetApplicationNameFromAssemblyOrDefault()
    {
        var assemblyName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()?.Location);
        return assemblyName ?? Constants.ApplicationNameDefault;
    }

    /// <summary>
    ///     Get assembly information version from the entry assembly for the current application.
    ///     <para>
    ///         If assembly info is not available, a default of <see cref="zxeltor.ConfigUtilsHelpers.Constants.ApplicationInfoVersionDefault" /> is
    ///         returned.
    ///     </para>
    /// </summary>
    /// <returns>The application version</returns>
    public static string GetApplicationInfoVersionFromAssemblyOrDefault()
    {
        var assemblyVersion = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        return assemblyVersion ?? Constants.ApplicationInfoVersionDefault;
    }

    /// <summary>
    ///     Get assembly version from the entry assembly for the current application.
    /// </summary>
    /// <returns>The application version</returns>
    public static Version GetApplicationVersionFromAssembly()
    {
        var assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version;
        return assemblyVersion == null ? new Version() : assemblyVersion;
    }

    /// <summary>
    ///     Get the root folder from the location of the entry assembly for the current application.
    ///     <para>If assembly info is not available, a default is taken from <see cref="Environment.CurrentDirectory" /></para>
    /// </summary>
    /// <returns>The application root folder.</returns>
    public static string GetMainApplicationRootFolder()
    {
        var rootFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? Environment.CurrentDirectory;
        return rootFolder;
    }
}