// Copyright (c) 2024, zxeltor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Collections.ObjectModel;
using System.Globalization;
using zxeltor.ConfigUtilsHelpers.Helpers;

namespace zxeltor.ConfigUtilsHelpers.Config;

public static class ConfigManager
{
    private static readonly object _lock = new();

    private static volatile Dictionary<int, object> _settingsDefault;

    private static Dictionary<int, object> Settings { get; } = new();

    private static Dictionary<int, object> SettingsDefault => _settingsDefault;

    public static ObservableCollection<ConfigSetting> CurrentConfigSettings { get; } = new();

    /// <summary>
    ///     Get a Global Config setting by int key
    /// </summary>
    /// <param name="key">int key for the setting</param>
    /// <returns>dynamic Global Config setting</returns>
    public static T GetValue<T>(int key)
    {
        try
        {
            var result = (T)Settings[key];
            //if (result.Equals(default(T))) return (T)SettingsDefault[key];

            return result;
        }
        catch (Exception)
        {
            //// TODO: Log this in 4.5
            //try
            //{
            //    return (T)SettingsDefault[key];
            //}
            //catch (Exception)
            //{
            // TODO: Log this in 4.5
            return default;
            //}
        }
    }

    private static void ApplyObservableSettingsToSettings()
    {
        SetConfigData(CurrentConfigSettings.ToList());
    }

    /// <summary>
    ///     Parse and Load the Settings Dictionary
    /// </summary>
    /// <param name="configData">List of ConfigSetting to parse and load into the Settings Dictionary</param>
    /// <exception cref="System.Exception">Thrown when an error occurs parsing settings</exception>
    /// <exception cref="AggregateException">If any of the configuration is invalid it will throw this..</exception>
    public static void SetConfigData(List<ConfigSetting> configData)
    {
        var exceptions = new List<Exception>();
        Settings.Clear();
        CurrentConfigSettings.Clear();

        // Use reflection to create Deserialize method for POCO types
        var deserializeMethod = typeof(SerializationHelper).GetMethod("Deserialize");

        // Parse config data
        foreach (var setting in configData)
        {
            string valueToUse;

            // Use Value, Default Value, Emptry String in that order
            if (!string.IsNullOrEmpty(setting.Value))
                valueToUse = setting.Value;
            else if (!string.IsNullOrEmpty(setting.DefaultValue))
                valueToUse = setting.DefaultValue;
            else
                valueToUse = string.Empty;

            object settingToStore = null;
            try
            {
                // Handle NULL TypeName; string value
                if (string.IsNullOrEmpty(setting.TypeName))
                {
                    settingToStore = valueToUse;
                }
                else
                {
                    var t = Type.GetType(setting.TypeName);

                    // value types
                    if (t.IsValueType)
                    {
                        settingToStore = Convert.ChangeType(valueToUse, t, new CultureInfo("en-US"));
                    }
                    else if (t == typeof(string))
                    {
                        settingToStore = valueToUse;
                    }
                    else
                    {
                        // Use reflection to call generic Deserialize
                        var deserializeMethodWithType = deserializeMethod.MakeGenericMethod(t);
                        var result = deserializeMethodWithType.Invoke(null, new object[] { valueToUse });

                        settingToStore = result;
                    }
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(new Exception(string.Format("ID={1}, Setting Name={0}, Error={2}", setting.Name,
                    setting.Id, ex)));
            }
            finally
            {
                Settings.Add(setting.Id, settingToStore ?? valueToUse);
            }
        }

        // If Exceptions occurred, throw
        if (exceptions.Any()) throw new AggregateException(exceptions);

        configData.ForEach(conf => CurrentConfigSettings.Add(conf));
    }

    /// <summary>
    ///     Attempt to load the config settings from ConfigSettings.json
    /// </summary>
    /// <returns>True if successful. False otherwise</returns>
    public static void LoadConfigFromFile(string configFilePath)
    {
        if (!File.Exists(configFilePath))
            throw new FileNotFoundException("Can't find file", configFilePath);

        using (var sr = new StreamReader(configFilePath))
        {
            var configFile = SerializationHelper.Deserialize<ConfigFile>(sr.ReadToEnd());
            SetConfigData(configFile.ConfigSettings);
        }
    }

    private static void SaveConfigToFile(List<ConfigSetting> configSettings, string configFilePath)
    {
        using (var sw = new StreamWriter(configFilePath))
        {
            var configFile = new ConfigFile(configSettings);
            var appConfigSerialized = SerializationHelper.Serialize(configFile);
            sw.Write(appConfigSerialized);
            sw.Flush();
            sw.Close();
        }
    }

    public static void ResetSettingsToDefaults(string stringFilePath)
    {
        CurrentConfigSettings.ToList().ForEach(conf => conf.Value = null);
        SaveSettingsToFile(stringFilePath);
    }

    public static void SaveSettingsToFile(string stringFilePath)
    {
        ApplyObservableSettingsToSettings();
        SaveConfigToFile(CurrentConfigSettings.ToList(), stringFilePath);
    }
}