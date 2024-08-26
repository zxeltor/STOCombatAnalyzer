// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Windows;
using zxeltor.StoCombatAnalyzer.Lib.Model.CombatMap;

namespace zxeltor.StoCombatAnalyzer.Interface.Controls;

/// <summary>
///     Interaction logic for DetectionSettingsMapEditor.xaml
/// </summary>
public partial class DetectionSettingsMapEditor : Window
{
    #region Constructors

    public DetectionSettingsMapEditor()
    {
        this.InitializeComponent();
    }

    #endregion

    #region Public Members

    public bool? ShowDialog(CombatMap combatMap)
    {
        this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        this.DataContext = combatMap;
        this.uiTextBoxMaxPlayers.Focus();
        return base.ShowDialog();
    }

    #endregion

    #region Other Members

    private void UiButtonOk_OnClick(object sender, RoutedEventArgs e)
    {
        this.DialogResult = true;
    }

    #endregion
}