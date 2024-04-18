// Copyright (c) 2024, zxeltor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using Newtonsoft.Json;
using zxeltor.ConfigUtilsHelpers.Helpers;

namespace zxeltor.ConfigUtilsHelpers.Config;

public class ConfigFile
{
    public ConfigFile()
    {
    }

    public ConfigFile(IEnumerable<ConfigSetting> appConfigSettings)
    {
        this.ConfigSettings.AddRange(appConfigSettings.ToList());
    }

    [JsonProperty(Required = Required.Always, Order = 0)]
    public string ConfigVersion { get; set; } = "1.0.0";

    [JsonProperty(Required = Required.AllowNull, Order = 1)]
    public string AppName { get; set; } = AssemblyInfoHelper.GetApplicationNameFromAssemblyOrDefault();

    [JsonIgnore]
    public string? AppNameWithVersion { get; set; } =
        $"{AssemblyInfoHelper.GetApplicationNameFromAssemblyOrDefault()} v{AssemblyInfoHelper.GetApplicationNameFromAssemblyOrDefault()}";

    //[JsonProperty(Required = Required.Always, Order = 2)]
    //public List<ConfigGroup> ConfigGroups { get; set; } = new();

    [JsonProperty(Required = Required.Always, Order = 3)]
    public List<ConfigSetting> ConfigSettings { get; set; } = [];
}