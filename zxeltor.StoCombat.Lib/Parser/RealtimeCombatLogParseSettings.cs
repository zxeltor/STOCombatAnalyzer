// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using System.Runtime.CompilerServices;
using log4net;

namespace zxeltor.StoCombat.Lib.Parser;

public class RealtimeCombatLogParseSettings : INotifyPropertyChanged, IDisposable
{
    #region Private Fields

    private int _announcementPlaybackVolumePercentage;

    private readonly ApplicationSettingsBase? _applicationSettingsBase;
    private readonly List<PropertyInfo>? _applicationSettingsBaseProperties;
    private int _combatDurationPercentage = 1;
    private string? _combatLogPath;
    private string _combatLogPathFilePattern = "combatlog*.log";
    private int _howLongBeforeNewCombatInSeconds = 20;
    private int _howOftenParseLogsInSeconds = 3;
    private int _howOftenPullDataFromLogFilesSeconds = 4;
    private bool _isCombinePets;
    private bool _isEnableInactiveTimeCalculations;
    private bool _isIncludeAssistedKillsInAchievements;
    private bool _isKeepPlayersInListAfterCombat;
    private bool _isProcessKillingSpreeAnnouncements;
    private bool _isProcessMultiKillAnnouncements;
    private bool _isRemoveEntityOutliersNonPlayers;
    private bool _isRemoveEntityOutliersPlayers;
    private bool _isUnrealAnnouncementsEnabled;
    private readonly ILog _log = LogManager.GetLogger(typeof(CombatLogParseSettings));
    private int _minInActiveInSeconds = 4;
    private int _multiKillWaitInSeconds;
    private string? _myCharacter;
    private readonly List<PropertyInfo>? _propertyInfoList;

    #endregion

    #region Constructors

    /// <summary>
    ///     Default constructor.
    /// </summary>
    public RealtimeCombatLogParseSettings()
    {
        this._propertyInfoList = typeof(RealtimeCombatLogParseSettings).GetProperties().ToList();
    }

    public RealtimeCombatLogParseSettings(ApplicationSettingsBase? applicationSettingsBase) : this()
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

    #endregion

    #region Public Properties

    public bool IsCombinePets
    {
        get => this._isCombinePets;
        set => this.SetField(ref this._isCombinePets, value);
    }

    public bool IsEnableInactiveTimeCalculations
    {
        get => this._isEnableInactiveTimeCalculations;
        set => this.SetField(ref this._isEnableInactiveTimeCalculations, value);
    }

    public bool IsIncludeAssistedKillsInAchievements
    {
        get => this._isIncludeAssistedKillsInAchievements;
        set => this.SetField(ref this._isIncludeAssistedKillsInAchievements, value);
    }

    public bool IsKeepPlayersInListAfterCombat
    {
        get => this._isKeepPlayersInListAfterCombat;
        set => this.SetField(ref this._isKeepPlayersInListAfterCombat, value);
    }

    public bool IsProcessKillingSpreeAnnouncements
    {
        get => this._isProcessKillingSpreeAnnouncements;
        set => this.SetField(ref this._isProcessKillingSpreeAnnouncements, value);
    }

    public bool IsProcessMultiKillAnnouncements
    {
        get => this._isProcessMultiKillAnnouncements;
        set => this.SetField(ref this._isProcessMultiKillAnnouncements, value);
    }

    public bool IsRemoveEntityOutliersNonPlayers
    {
        get => this._isRemoveEntityOutliersNonPlayers;
        set => this.SetField(ref this._isRemoveEntityOutliersNonPlayers, value);
    }

    public bool IsRemoveEntityOutliersPlayers
    {
        get => this._isRemoveEntityOutliersPlayers;
        set => this.SetField(ref this._isRemoveEntityOutliersPlayers, value);
    }

    public bool IsUnrealAnnouncementsEnabled
    {
        get => this._isUnrealAnnouncementsEnabled;
        set => this.SetField(ref this._isUnrealAnnouncementsEnabled, value);
    }

    public int AnnouncementPlaybackVolumePercentage
    {
        get => this._announcementPlaybackVolumePercentage;
        set => this.SetField(ref this._announcementPlaybackVolumePercentage, value);
    }

    public int CombatDurationPercentage
    {
        get => this._combatDurationPercentage;
        set => this.SetField(ref this._combatDurationPercentage, value);
    }

    /// <summary>
    ///     A timespan in seconds defining a new combat instance boundary in time.
    /// </summary>
    public int HowLongBeforeNewCombatInSeconds
    {
        get => this._howLongBeforeNewCombatInSeconds;
        set => this.SetField(ref this._howLongBeforeNewCombatInSeconds, value);
    }

    public int HowOftenParseLogsInSeconds
    {
        get => this._howOftenParseLogsInSeconds;
        set => this.SetField(ref this._howOftenParseLogsInSeconds, value);
    }

    public int HowOftenPullDataFromLogFilesSeconds
    {
        get => this._howOftenPullDataFromLogFilesSeconds;
        set => this.SetField(ref this._howOftenPullDataFromLogFilesSeconds, value);
    }

    public int MinInActiveInSeconds
    {
        get => this._minInActiveInSeconds;
        set => this.SetField(ref this._minInActiveInSeconds, value);
    }

    public int MultiKillWaitInSeconds
    {
        get => this._multiKillWaitInSeconds;
        set => this.SetField(ref this._multiKillWaitInSeconds, value);
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
    ///     The folder to the STO combat log files.
    /// </summary>
    public string? CombatLogPath
    {
        get => this._combatLogPath;
        set => this.SetField(ref this._combatLogPath, value);
    }

    public string? MyCharacter
    {
        get => this._myCharacter;
        set => this.SetField(ref this._myCharacter, value);
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
            $"Path={this.CombatLogPath}, File={this.CombatLogPathFilePattern}, HowLong={this.HowLongBeforeNewCombatInSeconds}";
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (this._applicationSettingsBase != null)
            this._applicationSettingsBase.PropertyChanged -= this.ApplicationSettingsBaseOnPropertyChanged;
    }

    #endregion
}