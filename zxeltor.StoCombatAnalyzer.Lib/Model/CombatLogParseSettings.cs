﻿// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using System.Runtime.CompilerServices;
using log4net;
using zxeltor.ConfigUtilsHelpers.Helpers;
using zxeltor.StoCombatAnalyzer.Lib.Model.CombatMap;

namespace zxeltor.StoCombatAnalyzer.Lib.Model;

public class CombatLogParseSettings : INotifyPropertyChanged, IDisposable
{
    #region Private Fields

    private readonly ApplicationSettingsBase? _applicationSettingsBase;
    private readonly List<PropertyInfo>? _applicationSettingsBaseProperties;

    private readonly ILog _log = LogManager.GetLogger(typeof(CombatLogParseSettings));
    private readonly List<PropertyInfo>? _propertyInfoList;

    private string? _combatLogPath;
    private string _combatLogPathFilePattern = "combatlog*.log";

    private string? _defaultCombatDetectionSettings;

    private int _howFarBackForCombatInHours = 24;
    private int _howLongBeforeNewCombatInSeconds = 20;
    private int _howLongToKeepLogsInDays = 7;
    private bool _isCombinePets = true;
    private bool _isEnableInactiveTimeCalculations = true;

    private CombatMapDetectionSettings? _mapDetectionSettings;
    private int _minInActiveInSeconds = 4;
    private string? _userCombatDetectionSettings;

    #endregion

    #region Constructors

    /// <summary>
    ///     Default constructor.
    /// </summary>
    public CombatLogParseSettings()
    {
        this._propertyInfoList = typeof(CombatLogParseSettings).GetProperties().ToList();
    }

    /// <summary>
    ///     Used to init with <see cref="ApplicationSettingsBase" />
    /// </summary>
    /// <param name="applicationSettingsBase">Application settings</param>
    public CombatLogParseSettings(ApplicationSettingsBase? applicationSettingsBase) : this()
    {
        if (applicationSettingsBase == null) return;
        if (this._propertyInfoList == null || this._propertyInfoList.Count == 0) return;

        this._applicationSettingsBase = applicationSettingsBase;

        this._applicationSettingsBaseProperties = this._applicationSettingsBase.GetType().GetProperties().ToList();

        this._propertyInfoList.ForEach(combatParserProp =>
        {
            var appSettingProperty =
                this._applicationSettingsBaseProperties.FirstOrDefault(appSetting =>
                    appSetting.Name.Equals(combatParserProp.Name));

            if (appSettingProperty != null)
                combatParserProp.SetValue(this, appSettingProperty.GetValue(applicationSettingsBase));
        });

        this._applicationSettingsBase.PropertyChanged += this.ApplicationSettingsBaseOnPropertyChanged;
    }

    /// <summary>
    ///     Create a new instance with the provided parameters.
    /// </summary>
    /// <param name="combatCombatLogPath">The folder to the STO combat log files.</param>
    /// <param name="combatLogPathFilePattern">A file pattern used to search for combat log files.</param>
    /// <param name="howFarBackForCombatInHours">A timespan in hours to retrieve combat log data.</param>
    /// <param name="howLongBeforeNewCombatInSeconds">A timespan in seconds defining a new combat instance boundary in time.</param>
    /// <param name="howLongToKeepLogsInDays">How long to keep STO combat files around in days.</param>
    /// <param name="combatMapDetectionSettings">Used in map/event detection when parsing the STO combat logs.</param>
    public CombatLogParseSettings(string combatCombatLogPath, string? combatLogPathFilePattern = null,
        int? howFarBackForCombatInHours = null, int? howLongBeforeNewCombatInSeconds = null,
        int? howLongToKeepLogsInDays = null,
        CombatMapDetectionSettings? combatMapDetectionSettings = null)
    {
        this._combatLogPath = combatCombatLogPath;
        if (combatLogPathFilePattern != null) this._combatLogPathFilePattern = combatLogPathFilePattern;
        if (howFarBackForCombatInHours.HasValue) this._howFarBackForCombatInHours = howFarBackForCombatInHours.Value;
        if (howLongBeforeNewCombatInSeconds.HasValue)
            this._howLongBeforeNewCombatInSeconds = howLongBeforeNewCombatInSeconds.Value;
        if (howLongToKeepLogsInDays.HasValue) this._howLongToKeepLogsInDays = howLongToKeepLogsInDays.Value;
        if (combatMapDetectionSettings != null)
            this._mapDetectionSettings = combatMapDetectionSettings;
    }

    #endregion

    #region Public Properties

    public bool IsEnableInactiveTimeCalculations
    {
        get => this._isEnableInactiveTimeCalculations;
        set => this.SetField(ref this._isEnableInactiveTimeCalculations, value);
    }

    public int MinInActiveInSeconds
    {
        get => this._minInActiveInSeconds;
        set => this.SetField(ref this._minInActiveInSeconds, value);
    }

    public bool IsCombinePets
    {
        get => this._isCombinePets;
        set => this.SetField(ref this._isCombinePets, value);
    }

    /// <summary>
    ///     Application default <see cref="CombatMapDetectionSettings" />
    /// </summary>
    public string? DefaultCombatDetectionSettings
    {
        get => this._defaultCombatDetectionSettings;
        set => this.SetField(ref this._defaultCombatDetectionSettings, value);
    }

    /// <summary>
    ///     User level <see cref="CombatMapDetectionSettings" />
    /// </summary>
    public string? UserCombatDetectionSettings
    {
        get => this._userCombatDetectionSettings;
        set => this.SetField(ref this._userCombatDetectionSettings, value);
    }

    /// <summary>
    ///     The folder to the STO combat log files.
    /// </summary>
    public string? CombatLogPath
    {
        get => this._combatLogPath;
        set => this.SetField(ref this._combatLogPath, value);
    }

    /// <summary>
    ///     How long to keep STO combat files around in days.
    /// </summary>
    public int HowLongToKeepLogsInDays
    {
        get => this._howLongToKeepLogsInDays;
        set => this.SetField(ref this._howLongToKeepLogsInDays, value);
    }

    /// <summary>
    ///     A file pattern used to search for combat log files.
    ///     <para>Wildcards can be used to return multiple files.</para>
    /// </summary>
    public string CombatLogPathFilePattern
    {
        get => this._combatLogPathFilePattern;
        set => this.SetField(ref this._combatLogPathFilePattern, value);
    }

    /// <summary>
    ///     A timespan in hours to retrieve combat log data.
    /// </summary>
    public int HowFarBackForCombatInHours
    {
        get => this._howFarBackForCombatInHours;
        set => this.SetField(ref this._howFarBackForCombatInHours, value);
    }

    /// <summary>
    ///     A timespan in seconds defining a new combat instance boundary in time.
    /// </summary>
    public int HowLongBeforeNewCombatInSeconds
    {
        get => this._howLongBeforeNewCombatInSeconds;
        set => this.SetField(ref this._howLongBeforeNewCombatInSeconds, value);
    }

    /// <summary>
    ///     Used in map/event detection when parsing the STO combat logs.
    /// </summary>
    public CombatMapDetectionSettings? MapDetectionSettings
    {
        get
        {
            if (this._mapDetectionSettings == null)
            {
                if (!string.IsNullOrWhiteSpace(this._userCombatDetectionSettings))
                    this._mapDetectionSettings =
                        SerializationHelper.Deserialize<CombatMapDetectionSettings>(this._userCombatDetectionSettings);
                else if (!string.IsNullOrWhiteSpace(this._defaultCombatDetectionSettings))
                    this._mapDetectionSettings =
                        SerializationHelper.Deserialize<CombatMapDetectionSettings>(
                            this._defaultCombatDetectionSettings);
            }

            return this._mapDetectionSettings;
        }
        set => this.SetField(ref this._mapDetectionSettings, value);
    }

    #endregion

    #region Public Members

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Other Members

    private void ApplicationSettingsBaseOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.PropertyName)) return;
        if (this._propertyInfoList == null || this._propertyInfoList.Count == 0) return;
        if (this._applicationSettingsBase == null || this._applicationSettingsBaseProperties == null ||
            this._applicationSettingsBaseProperties.Count == 0) return;

        var appPropertyInfo =
            this._applicationSettingsBaseProperties.FirstOrDefault(setting => setting.Name.Equals(e.PropertyName));

        var thisPropertyInfo = this._propertyInfoList.FirstOrDefault(prop => prop.Name.Equals(e.PropertyName));

        if (appPropertyInfo == null || thisPropertyInfo == null) return;

        thisPropertyInfo.SetValue(this, appPropertyInfo.GetValue(this._applicationSettingsBase));
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        this.OnPropertyChanged(propertyName);
        return true;
    }

    #endregion

    #region Overrides of Object

    /// <inheritdoc />
    public override string ToString()
    {
        return
            $"Path={this.CombatLogPath}, File={this.CombatLogPathFilePattern}, HowFar={this.HowFarBackForCombatInHours}, HowLong={this.HowLongBeforeNewCombatInSeconds}";
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (this._applicationSettingsBase != null)
            this._applicationSettingsBase.PropertyChanged -= this.ApplicationSettingsBaseOnPropertyChanged;
    }

    #endregion
}