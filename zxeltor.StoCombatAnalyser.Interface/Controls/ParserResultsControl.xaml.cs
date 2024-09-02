// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Windows;
using System.Windows.Controls;
using zxeltor.StoCombatAnalyzer.Interface.Classes;

namespace zxeltor.StoCombatAnalyzer.Interface.Controls;

/// <summary>
///     Interaction logic for ParserResultsControl.xaml
/// </summary>
public partial class ParserResultsControl : UserControl
{
    #region Constructors

    public ParserResultsControl()
    {
        this.InitializeComponent();
    }

    #endregion

    #region Public Properties

    private CombatLogManager? CombatLogManagerContext => this.DataContext as CombatLogManager;

    private MainWindow? MainWindow => Application.Current.MainWindow as MainWindow;

    #endregion

    #region Other Members

    private void UiButtonParseLog_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;

        var combatLogManagerContext = this.CombatLogManagerContext;
        if (combatLogManagerContext != null && combatLogManagerContext.IsExecutingBackgroundProcess)
        {
            e.Handled = true;
            return;
        }

        this.MainWindow?.ParseLogFiles(null);
    }

    #endregion
}