// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using zxeltor.StoCombat.Analyzer.Properties;
using zxeltor.StoCombat.Lib.Attributes;
using zxeltor.StoCombat.Lib.Model.CombatMap;
using zxeltor.StoCombat.Lib.Parser;
using zxeltor.Types.Lib.Helpers;

namespace zxeltor.StoCombat.Analyzer.Classes;

/// <summary>
///     Main application settings
/// </summary>
public sealed class StoCombatAnalyzerSettings : INotifyPropertyChanged, IDisposable
{
    #region Static Fields and Constants

    private static StoCombatAnalyzerSettings? _instance;
    private static readonly object Padlock = new();

    #endregion

    #region Private Fields

    private string? _combatControlGridDisplayList;
    private string? _combatEventTypeGridContext;
    private int _howLongToKeepLogsInDays = 7;
    private bool _isDebugLoggingEnabled;
    private bool _isDetectionsSettingsTabEnabled;
    private bool _isDisplayDevTestTools;
    private bool _isDisplayParseResults;
    private bool _isDisplayPlotMagnitudeBase;
    private bool _isDisplayPlotPlayerInactive;
    private bool _isEnableAnalysisTools;
    private bool _isIncludeNonPlayerEntities;
    private CombatLogParseSettings? _parserSettings;
    private bool _purgeCombatLogs;

    #endregion

    #region Constructors

    /// <summary>
    ///     A private constructor to block creating instances outside the singleton pattern and serialization
    /// </summary>
    private StoCombatAnalyzerSettings()
    {
    }

    #endregion

    #region Public Properties

    public bool IsDebugLoggingEnabled
    {
        get => this._isDebugLoggingEnabled;
        set => this.SetField(ref this._isDebugLoggingEnabled, value);
    }

    public bool IsDetectionsSettingsTabEnabled
    {
        get => this._isDetectionsSettingsTabEnabled;
        set => this.SetField(ref this._isDetectionsSettingsTabEnabled, value);
    }

    public bool IsDisplayDevTestTools
    {
        get => this._isDisplayDevTestTools;
        set => this.SetField(ref this._isDisplayDevTestTools, value);
    }

    public bool IsDisplayParseResults
    {
        get => this._isDisplayParseResults;
        set => this.SetField(ref this._isDisplayParseResults, value);
    }

    public bool IsDisplayPlotMagnitudeBase
    {
        get => this._isDisplayPlotMagnitudeBase;
        set => this.SetField(ref this._isDisplayPlotMagnitudeBase, value);
    }

    public bool IsDisplayPlotPlayerInactive
    {
        get => this._isDisplayPlotPlayerInactive;
        set => this.SetField(ref this._isDisplayPlotPlayerInactive, value);
    }

    public bool IsEnableAnalysisTools
    {
        get => this._isEnableAnalysisTools;
        set => this.SetField(ref this._isEnableAnalysisTools, value);
    }

    public bool IsIncludeNonPlayerEntities
    {
        get => this._isIncludeNonPlayerEntities;
        set => this.SetField(ref this._isIncludeNonPlayerEntities, value);
    }

    public bool IsSelectLatestCombatOnParseLogs => true;

    [PropertySetting(label: "Purge STO Combat Logs At Startup",
        description: "Enable combat log folder purge at application startup.",
        note: "<Bold>Note:</Bold> If only one combat log exists, it won't be purged, regardless of how old it is.")]
    public bool PurgeCombatLogs
    {
        get => this._purgeCombatLogs;
        set => this.SetField(ref this._purgeCombatLogs, value);
    }

    public CombatLogParseSettings? ParserSettings
    {
        get => this._parserSettings;
        set => this.SetField(ref this._parserSettings, value);
    }

    public int HowLongToKeepLogsInDays
    {
        get => this._howLongToKeepLogsInDays;
        set => this.SetField(ref this._howLongToKeepLogsInDays, value);
    }

    /// <summary>
    ///     A singleton instance of <see cref="StoCombatAnalyzerSettings" /> for the main application.
    /// </summary>
    [JsonIgnore]
    public static StoCombatAnalyzerSettings Instance
    {
        get
        {
            lock (Padlock)
            {
                return _instance ??= FromAppConfig();
            }
        }
    }

    public string? CombatControlGridDisplayList
    {
        get => this._combatControlGridDisplayList;
        set => this.SetField(ref this._combatControlGridDisplayList, value);
    }

    public string? CombatEventTypeGridContext
    {
        get => this._combatEventTypeGridContext;
        set => this.SetField(ref this._combatEventTypeGridContext, value);
    }

    #endregion

    #region Public Members

    #region IDisposable

    /// <inheritdoc />
    public void Dispose()
    {
        this.ParserSettings.PropertyChanged -= this.ParserSettingsOnPropertyChanged;
    }

    #endregion

    public static StoCombatAnalyzerSettings FromAppConfig()
    {
        if (string.IsNullOrWhiteSpace(Settings.Default.StoCombatAnalyzerSettings) ||
            !SerializationHelper.TryDeserializeString<StoCombatAnalyzerSettings>(
                Settings.Default.StoCombatAnalyzerSettings, out var settings) || settings == null)
        {
            var context = new StoCombatAnalyzerSettings();
            context.Init();
            context.SaveToAppConfig();
            return context;
        }

        settings.Init();
        return settings;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void ResetMapDetectionSettingsFromAppDefault()
    {
        if (!string.IsNullOrWhiteSpace(Settings.Default.DefaultCombatDetectionSettings) &&
            SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(
                Settings.Default.DefaultCombatDetectionSettings, out var defaultSettings))
            if (defaultSettings != null)
            {
                this.ParserSettings.MapDetectionSettings = defaultSettings;
                Settings.Default.UserCombatDetectionSettings = Settings.Default.DefaultCombatDetectionSettings;
                Settings.Default.Save();
                return;
            }

        this.ParserSettings.MapDetectionSettings = new CombatMapDetectionSettings();
        Settings.Default.UserCombatDetectionSettings =
            SerializationHelper.Serialize(this.ParserSettings.MapDetectionSettings);
        Settings.Default.Save();
    }

    public void SaveToAppConfig()
    {
        if (this.ParserSettings?.MapDetectionSettings != null)
            this.SaveMapDetectionSettings();

        var thisAsString = SerializationHelper.Serialize(this);
        Settings.Default.StoCombatAnalyzerSettings = thisAsString;
        Settings.Default.Save();
    }

    public void SetMapDetectionSettings(CombatMapDetectionSettings combatMapDetectionSettings)
    {
        this.ParserSettings.MapDetectionSettings = combatMapDetectionSettings;
        this.SaveMapDetectionSettings();
    }

    #endregion

    #region Other Members

    private static CombatMapDetectionSettings GetMapDetectionSettings()
    {
        if (!string.IsNullOrWhiteSpace(Settings.Default.UserCombatDetectionSettings) &&
            SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(
                Settings.Default.UserCombatDetectionSettings,
                out var userSettings))
        {
            if (userSettings != null) return userSettings;
        }
        else if (!string.IsNullOrWhiteSpace(Settings.Default.DefaultCombatDetectionSettings) &&
                 SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(
                     Settings.Default.DefaultCombatDetectionSettings, out var defaultSettings))
        {
            if (defaultSettings != null)
            {
                Settings.Default.UserCombatDetectionSettings = Settings.Default.DefaultCombatDetectionSettings;
                Settings.Default.Save();
                return defaultSettings;
            }
        }

        var detectionSettings = new CombatMapDetectionSettings();
        Settings.Default.UserCombatDetectionSettings = SerializationHelper.Serialize(detectionSettings);
        Settings.Default.Save();

        return detectionSettings;
    }

    /// <summary>
    ///     This method pulls map detection settings from the application config, and
    ///     attaches various property changed event handlers.
    /// </summary>
    private void Init()
    {
        if (ParserSettings == null)
            ParserSettings = new CombatLogParseSettings();
        
        var mapDetectionSettings = GetMapDetectionSettings();
        this.ParserSettings.PropertyChanged += this.ParserSettingsOnPropertyChanged;
        this.ParserSettings.MapDetectionSettings = mapDetectionSettings;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        this.SaveToAppConfig();
    }

    private void ParserSettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        this.SaveToAppConfig();
    }

    private void SaveMapDetectionSettings()
    {
        if (this.ParserSettings.MapDetectionSettings != null)
        {
            Settings.Default.UserCombatDetectionSettings =
                SerializationHelper.Serialize(this._parserSettings.MapDetectionSettings);
            Settings.Default.Save();
        }
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        this.OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}