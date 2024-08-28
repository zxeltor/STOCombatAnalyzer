// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace zxeltor.ConfigUtilsHelpers.Helpers;

/// <summary>
///     A collection of static helpers used to manage logging for the application.
/// </summary>
public class LoggingHelper
{
    #region Static Fields and Constants

    private static readonly ILog Log = LogManager.GetLogger(typeof(LoggingHelper));

    #endregion

    #region Public Members

    /// <summary>
    ///     Configure log4net by setting a log4net.config from the root application folder, if it's available.
    ///     <para>
    ///         If running in a development environment, this logic will look for another version of log4net.config named
    ///         log4net.Development.config to use while debugging. If the development version
    ///         isn't available, it will default to log4net.config, if it can.
    ///     </para>
    /// </summary>
    public static void ConfigureLog4NetLogging()
    {
#if DEBUG
        var isDevelopment = true;
#else
        var isDevelopment = false;
#endif

        var debugLogSettingsAreBeingUsed = false;

        if (isDevelopment)
        {
            var devVersionOfSettingsFilePath = Path.Combine(AssemblyInfoHelper.GetMainApplicationRootFolder(),
                "Log4Net.Development.config");

            if (File.Exists(devVersionOfSettingsFilePath))
            {
                XmlConfigurator.ConfigureAndWatch(new FileInfo(devVersionOfSettingsFilePath));
                debugLogSettingsAreBeingUsed = true;
            }
        }

        if (!debugLogSettingsAreBeingUsed)
        {
            var settingsFilePath =
                Path.Combine(AssemblyInfoHelper.GetMainApplicationRootFolder(), "Log4Net.config");
            if (File.Exists(settingsFilePath)) XmlConfigurator.ConfigureAndWatch(new FileInfo(settingsFilePath));
        }
    }

    /// <summary>
    ///     Attempt to change the log4net logging level programmatically.
    /// </summary>
    /// <param name="enableDebugLogging">True to enable debug logging. False otherwise.</param>
    /// <returns>True of successful. False otherwise.</returns>
    public static bool TrySettingLog4NetLogLevel(bool enableDebugLogging)
    {
        try
        {
            LogManager.GetAllRepositories().ToList().ForEach(repository =>
            {
                var hier = (Hierarchy)repository;
                hier.GetCurrentLoggers().ToList().ForEach(logger =>
                {
                    var tmpLogger = (Logger)logger;
                    tmpLogger.Level = enableDebugLogging ? Level.Debug : Level.Warn;
                });
            });

            return true;
        }
        catch (Exception e)
        {
            Log.Error("Failed to set application log level", e);
        }

        return false;
    }

    #endregion
}