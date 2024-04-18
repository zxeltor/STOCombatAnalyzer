// Copyright (c) 2024, zxeltor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using Newtonsoft.Json;

namespace zxeltor.ConfigUtilsHelpers.Config;

public class ConfigSetting
{
    public ConfigSetting(int id, string name, string description, string? value, string? defaultValue, string typeName,
        int groupId = 0)
    {
        this.Id = id;
        this.Name = name;
        this.Description = description;
        this.Value = value;
        this.DefaultValue = defaultValue;
        this.TypeName = typeName;
        this.GroupId = groupId;
    }

    /// <summary>
    ///     The default value for this config
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string? DefaultValue { get; set; }

    /// <summary>
    ///     A description for this config
    /// </summary>
    [JsonProperty("desc", Order = 3, Required = Required.Always)]
    public string Description { get; set; }

    /// <summary>
    ///     Used for grouping types of
    /// </summary>
    public int? GroupId { get; set; }

    /// <summary>
    ///     A unique id for our config.
    /// </summary>
    [JsonProperty(Order = 0, Required = Required.Always)]
    public int Id { get; set; }

    /// <summary>
    ///     The name for our config
    /// </summary>
    [JsonProperty(Order = 2, Required = Required.Always)]
    public string Name { get; set; }

    /// <summary>
    ///     The .NET type this config should be deserialized as.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string TypeName { get; set; }

    /// <summary>
    ///     A user generated value for the config
    /// </summary>
    public string? Value { get; set; }

    public override string ToString()
    {
        return $"Id={this.Id}, Name={this.Name}, Value={this.Value ?? this.DefaultValue}";
    }
}