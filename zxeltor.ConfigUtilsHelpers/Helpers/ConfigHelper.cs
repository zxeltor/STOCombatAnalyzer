using log4net;
using zxeltor.ConfigUtilsHelpers.Config;

namespace zxeltor.ConfigUtilsHelpers.Helpers;

public static class ConfigHelper
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(ConfigHelper));

    /// <summary>
    ///     Attempt to load the config settings from ConfigSettings.json
    /// </summary>
    /// <returns>True if successful. False otherwise</returns>
    public static bool TryLoadConfig(string configFilePath)
    {
        try
        {
            if (!File.Exists(configFilePath))
                throw new FileNotFoundException("Can't find file", configFilePath);

            using (var sr = new StreamReader(configFilePath))
            {
                var configSettings = SerializationHelper.Deserialize<ConfigFile>(sr.ReadToEnd());
                ConfigManager.SetConfigData(configSettings.ConfigSettings);
            }

            return true;
        }
        catch (Exception e)
        {
            Log.Error($"Failed to load ConfigSettings: \"{configFilePath}\"", e);
        }

        return false;
    }

    public static bool TrySaveConfig(ConfigFile configFile, string configFilePath)
    {
        try
        {
            using (var sw = new StreamWriter(configFilePath))
            {
                var appConfigSerialized = SerializationHelper.Serialize(configFile);
                sw.Write(appConfigSerialized);
                sw.Flush();
                sw.Close();
            }
        }
        catch (Exception e)
        {
            Log.Error($"Failed to write ConfigSettings: \"{configFilePath}\"", e);
        }

        return false;
    }
}