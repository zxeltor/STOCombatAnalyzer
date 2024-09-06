// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using log4net;
using zxeltor.ConfigUtilsHelpers.Helpers;
using zxeltor.StoCombat.Analyzer.Classes.UI;
using zxeltor.StoCombat.Analyzer.Properties;
using zxeltor.StoCombat.Lib.Model.CombatLog;
using zxeltor.StoCombat.Lib.Model.CombatMap;
using zxeltor.StoCombat.Lib.Parser;

namespace zxeltor.StoCombat.Analyzer.Classes;

/// <summary>
///     Used as the primary application DataContext, and handles all the heavy lifting with the combat log parsing.
/// </summary>
public class CombatLogManager : INotifyPropertyChanged
{
    #region Public Delegates

    /// <summary>
    ///     An event used to send status updates back to the main window
    /// </summary>
    public delegate void StatusChangeEventHandler(object sender, CombatManagerStatusEventArgs e);

    #endregion

    #region Static Fields and Constants

    private static readonly ILog _log = LogManager.GetLogger(typeof(CombatLogManager));

    #endregion

    #region Private Fields

    private CombatLogParserResult? _combatLogParserResult;

    private CombatMapDetectionSettings? _combatMapDetectionSettings = new();

    private string? _combatMapDetectionSettingsBeforeSave;
    private string? _dataGridSearchString;

    private CombatEventTypeSelector _eventTypeDisplayFilter = new("OVERALL");
    private bool _isDisplayPlotPlayerInactive = true;

    private bool _isExecutingBackgroundProcess;
    private bool _isPlotDisplayMagnitude = true;
    private bool _isPlotDisplayMagnitudeBase;
    private Combat? _selectedCombat;

    private CombatEntity? _selectedCombatEntity;

    private CombatEventType? _selectedCombatEventType;

    #endregion

    #region Constructors

    public CombatLogManager()
    {
        // Pull our map detection settings from the config
        if (!string.IsNullOrWhiteSpace(Settings.Default.UserCombatDetectionSettings) &&
            SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(
                Settings.Default.UserCombatDetectionSettings,
                out var combatMapSettingsUser))
            this.CombatMapDetectionSettings = combatMapSettingsUser;
        else if (!string.IsNullOrWhiteSpace(Settings.Default.DefaultCombatDetectionSettings) &&
                 SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(
                     Settings.Default.DefaultCombatDetectionSettings, out var combatMapSettingsDefault))
            this.CombatMapDetectionSettings = combatMapSettingsDefault;

        if (this.CombatMapDetectionSettings != null)
            this.CombatMapDetectionSettings.SetChange(false);

        Settings.Default.PropertyChanged += this.DefaultOnPropertyChanged;
    }

    #endregion

    #region Public Properties

    /// <summary>
    ///     Display Magnitude in the main Plot control
    /// </summary>
    public bool IsDisplayPlotMagnitude
    {
        get => this._isPlotDisplayMagnitude;
        set => this.SetField(ref this._isPlotDisplayMagnitude, value);
    }

    /// <summary>
    ///     Display BaseMagnitude in the main Plot control
    /// </summary>
    public bool IsDisplayPlotMagnitudeBase
    {
        get => this._isPlotDisplayMagnitudeBase = Settings.Default.IsDisplayPlotMagnitudeBase;
        set
        {
            this.SetField(ref this._isPlotDisplayMagnitudeBase, value);
            if (Settings.Default.IsDisplayPlotMagnitudeBase != value)
                Settings.Default.IsDisplayPlotMagnitudeBase = value;
            //Settings.Default.Save();
        }
    }

    public bool IsDisplayPlotPlayerInactive
    {
        get => this._isDisplayPlotPlayerInactive = Settings.Default.IsDisplayPlotPlayerInactive;
        set
        {
            this.SetField(ref this._isDisplayPlotPlayerInactive, value);
            if (Settings.Default.IsDisplayPlotPlayerInactive != value)
                Settings.Default.IsDisplayPlotPlayerInactive = value;
            //Settings.Default.Save();
        }
    }

    /// <summary>
    ///     A databind to disable the main UI while an expensive background process is running
    /// </summary>
    public bool IsExecutingBackgroundProcess
    {
        get => this._isExecutingBackgroundProcess;
        set => this.SetField(ref this._isExecutingBackgroundProcess, value);
    }

    /// <summary>
    ///     The currently selected Combat instance from <see cref="Combats" />
    /// </summary>
    public Combat? SelectedCombat
    {
        get => this._selectedCombat;
        set => this.SetField(ref this._selectedCombat, value);
    }

    /// <summary>
    ///     The currently selected combat entity in the main ui
    /// </summary>
    public CombatEntity? SelectedCombatEntity
    {
        get => this._selectedCombatEntity;
        set => this.SetField(ref this._selectedCombatEntity, value);
    }

    /// <summary>
    ///     The currently selected type for the current entity.
    /// </summary>
    public CombatEventType? SelectedCombatEventType
    {
        get => this._selectedCombatEventType;
        set => this.SetField(ref this._selectedCombatEventType, value);
    }

    /// <summary>
    ///     The filter string for the
    /// </summary>
    public CombatEventTypeSelector EventTypeDisplayFilter
    {
        get => this._eventTypeDisplayFilter;
        set
        {
            this.SetField(ref this._eventTypeDisplayFilter, value ?? new CombatEventTypeSelector("OVERALL"));
            this.OnPropertyChanged(nameof(this.FilteredSelectedEntityCombatEventList));
        }
    }

    public CombatLogParserResult? CombatLogParserResult
    {
        get => this._combatLogParserResult;
        set => this.SetField(ref this._combatLogParserResult, value);
    }

    /// <summary>
    ///     Combat map detection settings.
    /// </summary>
    public CombatMapDetectionSettings? CombatMapDetectionSettings
    {
        get => this._combatMapDetectionSettings;
        set => this.SetField(ref this._combatMapDetectionSettings, value);
    }

    /// <summary>
    ///     MaxDamageHit damage for the selected event type
    /// </summary>
    public double? SelectedCombatEntityEventTypeMaxDamage
    {
        get
        {
            if (this.SelectedCombatEntity?.CombatEventTypeListForEntity == null ||
                this.SelectedCombatEntity.CombatEventTypeListForEntity.Count == 0) return null;

            return this.SelectedCombatEntity?.CombatEventTypeListForEntity.Max(ev => ev.MaxDamageHit);
        }
    }

    /// <summary>
    ///     Total damage for the selected event type
    /// </summary>
    public double? SelectedCombatEntityEventTypeTotalDamage
    {
        get
        {
            if (this.SelectedCombatEntity?.CombatEventTypeListForEntity == null ||
                this.SelectedCombatEntity.CombatEventTypeListForEntity.Count == 0) return null;

            return this.SelectedCombatEntity.CombatEventTypeListForEntity.Sum(ev => ev.Damage);
        }
    }

    /// <summary>
    ///     MaxDamageHit damage for the selected pet event type
    /// </summary>
    public double? SelectedCombatEntityPetEventTypeMaxDamage
    {
        get
        {
            if (this.SelectedCombatEntity?.CombatEventTypeListForEntityPets == null ||
                this.SelectedCombatEntity.CombatEventTypeListForEntityPets.Count == 0) return null;

            return this.SelectedCombatEntity?.CombatEventTypeListForEntityPets.Max(ev =>
                ev.CombatEventTypes.Max(evt => evt.MaxDamageHit));
        }
    }

    /// <summary>
    ///     Total damage for the selected pet event type
    /// </summary>
    public double? SelectedCombatEntityPetEventTypeTotalDamage
    {
        get
        {
            if (this.SelectedCombatEntity?.CombatEventTypeListForEntityPets == null ||
                this.SelectedCombatEntity.CombatEventTypeListForEntityPets.Count == 0) return null;

            return this.SelectedCombatEntity?.CombatEventTypeListForEntityPets.Sum(ev =>
                ev.CombatEventTypes.Sum(evt => evt.Damage));
        }
    }

    /// <summary>
    ///     A list of <see cref="Combat" /> displayed in the main UI.
    /// </summary>
    public ObservableCollection<Combat> Combats { get; set; } = new();

    /// <summary>
    ///     A list of <see cref="CombatEvent" /> for <see cref="SelectedCombat" />
    /// </summary>
    //public ObservableCollection<CombatEvent>? SelectedEntityCombatEventList { get; set; } = new();

    public ObservableCollection<CombatEvent>? FilteredSelectedEntityCombatEventList
    {
        get
        {
            ObservableCollection<CombatEvent> results;

            // If we don't have analysis tool turned on, we don't want to take the time to return this list.
            if (!Settings.Default.IsEnableAnalysisTools)
                return null;

            if (this.SelectedCombatEntity == null)
                return null;

            // Return a list of all player events, with pet events grouped together.
            if (this.EventTypeDisplayFilter == null || this.EventTypeDisplayFilter.EventTypeId.Equals("OVERALL"))
            {
                results = new ObservableCollection<CombatEvent>(this.SelectedCombatEntity.CombatEventsList);
            }
            // Return a list of all Player Pet events
            else if (this.EventTypeDisplayFilter.EventTypeId.Equals("PETS OVERALL"))
            {
                results = new ObservableCollection<CombatEvent>(
                    this.SelectedCombatEntity.CombatEventsList?.Where(evt => evt.IsOwnerPetEvent) ??
                    Array.Empty<CombatEvent>());
            }
            // Return a list of events specific to a Pet
            else if (this.EventTypeDisplayFilter.IsPetEvent)
            {
                if (this.SelectedEntityPetCombatEventTypeList == null ||
                    this.SelectedEntityPetCombatEventTypeList.Count == 0)
                {
                    results = new ObservableCollection<CombatEvent>();
                }
                else
                {
                    var petEvtList = new List<CombatEvent>();

                    this.SelectedEntityPetCombatEventTypeList.ToList().ForEach(petevt =>
                    {
                        petevt.CombatEventTypes.ForEach(evt =>
                        {
                            if (this.EventTypeDisplayFilter.EventTypeId.Equals(evt.EventTypeId))
                                petEvtList.AddRange(evt.CombatEvents);
                        });
                    });

                    results = new ObservableCollection<CombatEvent>(petEvtList);
                }
            }
            else
            {
                // Return a list of events for a specific non-pet event.
                results = new ObservableCollection<CombatEvent>(
                    this.SelectedCombatEntity.CombatEventsList?.Where(evt => !evt.IsOwnerPetEvent &&
                                                                             evt.EventInternal.Equals(
                                                                                 this.EventTypeDisplayFilter
                                                                                     .EventTypeId,
                                                                                 StringComparison
                                                                                     .CurrentCultureIgnoreCase)) ??
                    Array.Empty<CombatEvent>());
            }

            // Filter our final result set, if we have a filter string set.
            if (results != null && results.Count > 0 && !string.IsNullOrWhiteSpace(this.DataGridSearchString))
                results = new ObservableCollection<CombatEvent>(results.Where(ev =>
                    ev.OwnerInternal.Contains(this.DataGridSearchString, StringComparison.CurrentCultureIgnoreCase)
                    || ev.OwnerDisplay.Contains(this.DataGridSearchString, StringComparison.CurrentCultureIgnoreCase)
                    || ev.TargetInternal.Contains(this.DataGridSearchString, StringComparison.CurrentCultureIgnoreCase)
                    || ev.TargetDisplay.Contains(this.DataGridSearchString, StringComparison.CurrentCultureIgnoreCase)
                    || ev.SourceInternal.Contains(this.DataGridSearchString, StringComparison.CurrentCultureIgnoreCase)
                    || ev.SourceDisplay.Contains(this.DataGridSearchString, StringComparison.CurrentCultureIgnoreCase)
                    || ev.EventDisplay.Contains(this.DataGridSearchString, StringComparison.CurrentCultureIgnoreCase)
                    || ev.EventInternal.Contains(this.DataGridSearchString, StringComparison.CurrentCultureIgnoreCase)
                    || ev.Type.Contains(this.DataGridSearchString, StringComparison.CurrentCultureIgnoreCase)
                    || ev.Flags.Contains(this.DataGridSearchString, StringComparison.CurrentCultureIgnoreCase)
                ).ToList());

            return results;
        }
    }

    /// <summary>
    ///     A list of <see cref="CombatEventType" /> for the Selected CombatEntity
    /// </summary>
    public ObservableCollection<CombatEventType>? SelectedEntityCombatEventTypeList { get; set; } = new();

    /// <summary>
    ///     A list of available combat event type metrics to display.
    /// </summary>
    public ObservableCollection<CombatEventTypeMetric> CombatEventTypeMetrics =>
        CombatEventTypeMetric.CombatEventTypeMetricList;

    public ObservableCollection<CombatEventTypeSelector> SelectedEntityCombatEventTypeListDisplayedFilterOptions
    {
        get
        {
            var resultCollection = new ObservableCollection<CombatEventTypeSelector>
            {
                new("OVERALL"), // Return all events
                new("PETS OVERALL") // Return player pet events.
            };

            // Add player events to the list
            if (this.SelectedEntityCombatEventTypeList != null && this.SelectedEntityCombatEventTypeList.Count > 0)
                this.SelectedEntityCombatEventTypeList.OrderBy(evt => evt.EventTypeLabel).ToList().ForEach(evt =>
                    resultCollection.Add(new CombatEventTypeSelector(evt)));

            // Add player pet events to the list
            if (this.SelectedEntityPetCombatEventTypeList != null &&
                this.SelectedEntityPetCombatEventTypeList.Count > 0)
                this.SelectedEntityPetCombatEventTypeList.OrderBy(evt => evt.PetLabel).ToList().ForEach(pevt =>
                {
                    if (pevt.CombatEventTypes != null && pevt.CombatEventTypes.Count > 0)
                        pevt.CombatEventTypes.OrderBy(evt => evt.EventTypeLabel).ToList().ForEach(evt =>
                        {
                            resultCollection.Add(new CombatEventTypeSelector(evt, true));
                        });
                });

            return resultCollection;
        }
    }

    /// <summary>
    ///     A list of Pet <see cref="CombatEventType" /> for the Selected CombatEntity
    /// </summary>
    public ObservableCollection<CombatPetEventType>? SelectedEntityPetCombatEventTypeList { get; set; } = new();

    /// <summary>
    ///     Assembly version string for the application.
    /// </summary>
    public string ApplicationVersionInfoString
    {
        get
        {
            var version = AssemblyInfoHelper.GetApplicationVersionFromAssembly();
            return $"{version.Major}.{version.Minor}.{version.Revision}";
        }
    }

    /// <summary>
    ///     The title for main UI.
    /// </summary>
    public string MainWindowTitle => $"{Resources.ApplicationName}: {this.ApplicationVersionInfoString}";

    /// <summary>
    ///     A backed up serialized version of our CombatMapDetectionSettings
    /// </summary>
    public string? CombatMapDetectionSettingsBeforeSave
    {
        get => this._combatMapDetectionSettingsBeforeSave;
        set => this.SetField(ref this._combatMapDetectionSettingsBeforeSave, value);
    }

    /// <summary>
    ///     Used to filter the result set for the data grid
    /// </summary>
    public string? DataGridSearchString
    {
        get => this._dataGridSearchString;
        set
        {
            this.SetField(ref this._dataGridSearchString, value);
            this.OnPropertyChanged(nameof(this.FilteredSelectedEntityCombatEventList));
        }
    }

    #endregion

    #region Public Members

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Populate our <see cref="CombatEvent" /> grid in the UI, using a <see cref="CombatEntity" /> selected by the user in
    ///     the UI.
    /// </summary>
    /// <param name="combatEntity">The data to populate our grid.</param>
    public void SetSelectedCombatEntity(CombatEntity? combatEntity)
    {
        if (combatEntity == null)
        {
            this.SelectedCombatEntity = null;
            this.SelectedEntityCombatEventTypeList = null;
            this.SelectedEntityPetCombatEventTypeList = null;
        }
        else
        {
            this.SelectedCombatEntity = combatEntity;
            if (combatEntity.CombatEventTypeListForEntity != null)
                this.SelectedEntityCombatEventTypeList =
                    new ObservableCollection<CombatEventType>(combatEntity.CombatEventTypeListForEntity);
            if (combatEntity.CombatEventTypeListForEntityPets != null)
                this.SelectedEntityPetCombatEventTypeList =
                    new ObservableCollection<CombatPetEventType>(combatEntity.CombatEventTypeListForEntityPets);
        }

        this.EventTypeDisplayFilter =
            this.SelectedEntityCombatEventTypeListDisplayedFilterOptions.FirstOrDefault(eventType =>
                eventType.EventTypeId.Equals("OVERALL"))!;

        this.OnPropertyChanged(nameof(this.SelectedCombatEntity));
        this.OnPropertyChanged(nameof(this.SelectedCombatEntityEventTypeTotalDamage));
        this.OnPropertyChanged(nameof(this.SelectedCombatEntityEventTypeMaxDamage));
        this.OnPropertyChanged(nameof(this.SelectedCombatEntityPetEventTypeTotalDamage));
        this.OnPropertyChanged(nameof(this.SelectedCombatEntityPetEventTypeMaxDamage));

        this.OnPropertyChanged(nameof(this.SelectedEntityCombatEventTypeList));
        this.OnPropertyChanged(nameof(this.SelectedEntityCombatEventTypeListDisplayedFilterOptions));
        this.OnPropertyChanged(nameof(this.FilteredSelectedEntityCombatEventList));
        this.OnPropertyChanged(nameof(this.EventTypeDisplayFilter));
        this.OnPropertyChanged(nameof(this.SelectedEntityPetCombatEventTypeList));
    }

    #endregion

    #region Other Members

    /// <summary>
    ///     Event handler used to save changes to the main app settings when a property changes.
    /// </summary>
    private void DefaultOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Settings.Default.Save();
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    ///     A helper method created to support the <see cref="INotifyPropertyChanged" /> implementation of this class.
    /// </summary>
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        //if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        this.OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}