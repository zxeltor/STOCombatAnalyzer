// Copyright since “2018” by Avail Technologies INC.  All rights reserved.  
// Filename: ConfigManager.cs
// Description: Global Configuration Manager.  Parses and loads ConfigData and provies
//     a Dictionary of strongly-typed settings.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Availtec.MyAvail.Portable.Config.ConfigTypes;
using Availtec.MyAvail.Portable.Enums;

namespace Availtec.MyAvail.Portable.Config
{
    public static class ConfigManager
    {
        #region Static Fields

        private static readonly object _lock = new object();

        private static volatile ConfigDictionary _settingsDefault;

        #endregion

        #region Properties

        private static ConfigDictionary Settings { get; set; }

        private static ConfigDictionary SettingsDefault
        {
            get
            {
                if (_settingsDefault == null)
                {
                    lock (_lock)
                    {
                        if (_settingsDefault == null)
                        {
                            _settingsDefault = GetDefaultSettings();
                        }
                    }
                }

                return _settingsDefault;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Returns the value in the appropriate Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static T GetConfigDataValue<T>(ConfigData setting)
        {
            // Use reflection to create Deserialize method for POCO types
            var deserializeMethod = typeof(ConfigDeserializer).GetMethod("Deserialize");

            string valueToUse;

            // Use Value, Default Value, Emptry String in that order
            if (!string.IsNullOrEmpty(setting.Value))
            {
                valueToUse = setting.Value;
            }
            else if (!string.IsNullOrEmpty(setting.DefaultValue))
            {
                valueToUse = setting.DefaultValue;
            }
            else
            {
                valueToUse = string.Empty;
            }

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
                    // special case for myavail core array type, this way they can get deserialized too
                    if (setting.TypeName.Equals("MyAvailCore.Models.ConfigGlobal.ConfigTypes.StringArray"))
                    {
                        setting.TypeName = "Availtec.MyAvail.Portable.Config.ConfigTypes.StringArray";
                    }


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

                        // Unwrap Array types
                        if (t == typeof(StringArray))
                        {
                            settingToStore = (result as StringArray).Items;
                        }
                        else
                        {
                            settingToStore = result;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return default(T);
            }

            return (T)(settingToStore ?? valueToUse);
        }

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
                if (result.Equals(default(T)))
                {
                    return (T)SettingsDefault[key];
                }

                return result;
            }
            catch (Exception)
            {
                // TODO: Log this in 4.5
                try
                {
                    return (T)SettingsDefault[key];
                }
                catch (Exception)
                {
                    // TODO: Log this in 4.5
                    return default(T);
                }
            }
        }

        /// <summary>
        ///     Get a Global Config setting by ConfigType
        /// </summary>
        /// <param name="key">ConfigType enum</param>
        /// <returns>dynamic Global Config setting</returns>
        public static T GetValue<T>(ConfigType key)
        {
            return GetValue<T>((int)key);
        }

        /// <summary>
        ///     Parse and Load the Settings Dictionary
        /// </summary>
        /// <param name="configData">List of ConfigData to parse and load into the Settings Dictionary</param>
        /// <exception cref="System.Exception">Thrown when an error occurs parsing settings</exception>
        /// <exception cref="AggregateException">If any of the configuration is invalid it will throw this..</exception>
        public static void SetConfigData(List<ConfigData> configData)
        {
            var exceptions = new List<Exception>();
            Settings = new ConfigDictionary();
            Settings.Clear();

            // Use reflection to create Deserialize method for POCO types
            var deserializeMethod = typeof(ConfigDeserializer).GetMethod("Deserialize");

            // Parse config data
            foreach (var setting in configData)
            {
                string valueToUse;

                // Use Value, Default Value, Emptry String in that order
                if (!string.IsNullOrEmpty(setting.Value))
                {
                    valueToUse = setting.Value;
                }
                else if (!string.IsNullOrEmpty(setting.DefaultValue))
                {
                    valueToUse = setting.DefaultValue;
                }
                else
                {
                    valueToUse = string.Empty;
                }

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
                        // special case for myavail core array type, this way they can get deserialized too
                        if (setting.TypeName.Equals("MyAvailCore.Models.ConfigGlobal.ConfigTypes.StringArray"))
                        {
                            setting.TypeName = "Availtec.MyAvail.Portable.Config.ConfigTypes.StringArray";
                        }
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

                            // Unwrap Array types
                            if (t == typeof(StringArray))
                            {
                                settingToStore = (result as StringArray).Items;
                            }
                            else
                            {
                                settingToStore = result;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //_log.Error("Error loading config setting " + setting.Name, ex);
                    exceptions.Add(new Exception(string.Format("ID={1}, Setting Name={0}, Error={2}", setting.Name, setting.Id, ex)));
                }
                finally
                {
                    Settings.Add(setting.Id, settingToStore ?? valueToUse);
                }
            }

            // If Exceptions occurred, throw
            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

#endregion

#region Methods

        /// <summary>
        ///     Set default values for config settings
        /// </summary>
        /// <returns>ConfigDictionary with default settings populated</returns>
        private static ConfigDictionary GetDefaultSettings()
        {
            var dict = new ConfigDictionary();
            dict.Add((int)ConfigType.EnableAlerts, true);

            //dict.Add((int)ConfigType.TidsCacheExpirationInSeconds, 1200);

            return dict;
        }

#endregion
    }
}