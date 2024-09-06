// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Windows;
using System.Windows.Controls;
using log4net;
using zxeltor.StoCombat.Analyzer.Classes;
using zxeltor.StoCombat.Analyzer.Helpers;

namespace zxeltor.StoCombat.Analyzer.Controls;

/// <summary>
///     Interaction logic for AboutControl.xaml
/// </summary>
public partial class AboutControl : UserControl
{
    #region Private Fields

    private readonly ILog Log = LogManager.GetLogger(typeof(AboutControl));

    #endregion

    #region Constructors

    public AboutControl()
    {
        this.InitializeComponent();
    }

    #endregion

    #region Public Properties

    private CombatLogManager? CombatLogManagerContext => this.DataContext as CombatLogManager;

    private MainWindow? MainWindow => Application.Current.MainWindow as MainWindow;

    #endregion

    #region Other Members

    private void Browse_OnMouseLeftButtonUp(object sender, RoutedEventArgs e)
    {
        if (!(e.Source is Button button))
            return;

        if (button.Tag is not string tagString) return;

        AppHelper.DisplayHelpUrlInBrowser(this.MainWindow, tagString);
    }

    #endregion
}