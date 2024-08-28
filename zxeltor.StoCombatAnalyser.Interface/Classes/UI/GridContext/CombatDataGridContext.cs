﻿// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using log4net;
using zxeltor.ConfigUtilsHelpers.Helpers;
using zxeltor.StoCombatAnalyzer.Interface.Properties;
using zxeltor.StoCombatAnalyzer.Lib.Model.CombatLog;

namespace zxeltor.StoCombatAnalyzer.Interface.Classes.UI.GridContext;

/// <summary>
///     A DataGridContext for <see cref="CombatEntity" />
/// </summary>
public class CombatDataGridContext : DataGridContext<Combat>
{
    #region Static Fields and Constants

    private static readonly ILog Log = LogManager.GetLogger(typeof(CombatDataGridContext));

    #endregion

    #region Constructors

    /// <inheritdoc />
    public CombatDataGridContext() : base()
    {
    }

    #endregion

    #region Public Members
    /// <summary>
    ///     Get the default context from application settings, or create a new one.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public new static CombatDataGridContext? GetDefaultContext()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(Settings.Default.CombatControlGridDisplayList))
                if (SerializationHelper.TryDeserializeString(
                        Settings.Default.CombatControlGridDisplayList, out CombatDataGridContext? context))
                    if (context != null)
                        return context;
        }
        catch (Exception e)
        {
            Log.Error("Failed to deserialize map detection settings from app config.", e);
        }

        var newConfig = new CombatDataGridContext()
        {
            GridColumns =
            {
                new DataGridColumnConfig(nameof(CombatEvent.OriginalFileName), isVisible: false),
                new DataGridColumnConfig(nameof(CombatEvent.OriginalFileLineNumber), isVisible: false),
                new DataGridColumnConfig(nameof(CombatEvent.OriginalHashCode), isVisible: false),
                new DataGridColumnConfig(nameof(CombatEvent.IsOwnerModified), isVisible: false),
                new DataGridColumnConfig(nameof(CombatEvent.Timestamp)),

                new DataGridColumnConfig(nameof(CombatEvent.IsOwnerPlayer)),
                new DataGridColumnConfig(nameof(CombatEvent.OwnerDisplay)),
                new DataGridColumnConfig(nameof(CombatEvent.OwnerInternal), isVisible: false),
                new DataGridColumnConfig(nameof(CombatEvent.OwnerInternalStripped), isVisible: false),

                new DataGridColumnConfig(nameof(CombatEvent.IsOwnerPetEvent)),
                new DataGridColumnConfig(nameof(CombatEvent.SourceDisplay)),
                new DataGridColumnConfig(nameof(CombatEvent.SourceInternal), isVisible: false),
                new DataGridColumnConfig(nameof(CombatEvent.SourceInternalStripped), isVisible: false),

                new DataGridColumnConfig(nameof(CombatEvent.IsTargetPlayer)),
                new DataGridColumnConfig(nameof(CombatEvent.TargetDisplay)),
                new DataGridColumnConfig(nameof(CombatEvent.TargetInternal), isVisible: false),
                new DataGridColumnConfig(nameof(CombatEvent.TargetInternalStripped), isVisible: false),

                new DataGridColumnConfig(nameof(CombatEvent.EventDisplay)),
                new DataGridColumnConfig(nameof(CombatEvent.EventInternal), isVisible: false),
                new DataGridColumnConfig(nameof(CombatEvent.Type)),
                new DataGridColumnConfig(nameof(CombatEvent.Flags)),
                new DataGridColumnConfig(nameof(CombatEvent.Magnitude)),
                new DataGridColumnConfig(nameof(CombatEvent.MagnitudeBase)),
                new DataGridColumnConfig(nameof(CombatEvent.OriginalFileLineString), isVisible: false)
            }
        };

        Settings.Default.CombatControlGridDisplayList = SerializationHelper.Serialize(newConfig);
        Settings.Default.Save();

        return newConfig;
    }

    #endregion
}