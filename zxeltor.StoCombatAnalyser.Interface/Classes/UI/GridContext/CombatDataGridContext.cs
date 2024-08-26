// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using zxeltor.ConfigUtilsHelpers.Helpers;
using zxeltor.StoCombatAnalyzer.Interface.Properties;
using zxeltor.StoCombatAnalyzer.Lib.Model.CombatLog;

namespace zxeltor.StoCombatAnalyzer.Interface.Classes.UI.GridContext;

/// <summary>
///     A DataGridContext for <see cref="CombatEntity"/>
/// </summary>
public class CombatDataGridContext : DataGridContext<Combat>
{
    #region Constructors

    /// <inheritdoc />
    public CombatDataGridContext(string name) : base(name)
    {
    }

    #endregion

    #region Public Members

    public new static CombatDataGridContext GetDefaultContext(string name)
    {
        var newConfig = new CombatDataGridContext(name)
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