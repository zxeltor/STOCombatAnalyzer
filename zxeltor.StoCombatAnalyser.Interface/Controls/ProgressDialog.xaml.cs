// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Windows;
using System.Windows.Threading;
using log4net;
using zxeltor.StoCombatAnalyzer.Lib.Parser;

//using System.Windows.Threading;

namespace zxeltor.StoCombatAnalyzer.Interface.Controls;

/// <summary>
///     Interaction logic for ProgressDialog.xaml
/// </summary>
public partial class ProgressDialog : Window
{
    #region Private Fields

    private Func<CombatLogParserResult> _funcToRun;

    private readonly ILog _log = LogManager.GetLogger(typeof(ProgressDialog));

    #endregion

    #region Constructors

    public ProgressDialog(Window parent, Func<CombatLogParserResult> funcToRun, string? message = "Processing data")
    {
        this.InitializeComponent();

        this.uiLabelMessage.Content = message;

        this.Owner = parent;
        this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

        this.ContentRendered += this.ProgressDialog_ContentRendered;
        this.Closed += this.OnClosed;

        this._funcToRun = funcToRun ?? throw new ArgumentNullException(nameof(funcToRun));
    }

    #endregion

    #region Public Properties

    public CombatLogParserResult? ParseResult { get; set; }

    #endregion

    #region Other Members

    private void OnClosed(object? sender, EventArgs e)
    {
        this.Closed -= this.OnClosed;
        this._funcToRun = null;
    }

    private void ProgressDialog_ContentRendered(object? sender, EventArgs e)
    {
        this.ContentRendered -= this.ProgressDialog_ContentRendered;
        try
        {
            // Run the action in background. This gives the UI chance to update.
            var dispatchOp = this.Dispatcher.BeginInvoke(this._funcToRun, DispatcherPriority.Background);
            dispatchOp.Wait();

            this.ParseResult = dispatchOp.Result as CombatLogParserResult;

            this.DialogResult = true;
        }
        catch (Exception exception)
        {
            this._log.Error($"Failed to run background task: {this._funcToRun.Method}", exception);
            this.DialogResult = false;
        }
    }

    #endregion
}