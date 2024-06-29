// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Windows;
using System.Windows.Threading;
using log4net;

//using System.Windows.Threading;

namespace zxeltor.StoCombatAnalyzer.Interface.Controls;

/// <summary>
///     Interaction logic for ProgressDialog.xaml
/// </summary>
public partial class ProgressDialog : Window
{
    private readonly ILog _log = LogManager.GetLogger(typeof(ProgressDialog));
    private Action _actionToRun;

    public ProgressDialog(Window parent, Action actionToRun, string? message = "Processing data")
    {
        this.InitializeComponent();

        this.uiLabelMessage.Content = message;

        this.Owner = parent;
        this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

        this.ContentRendered += this.ProgressDialog_ContentRendered;
        this.Closed += this.OnClosed;

        this._actionToRun = actionToRun ?? throw new ArgumentNullException(nameof(actionToRun));
    }

    private void ProgressDialog_ContentRendered(object? sender, EventArgs e)
    {
        this.ContentRendered -= this.ProgressDialog_ContentRendered;
        try
        {
            // Run the action in background. This gives the UI chance to update.
            var dispatchOp = this.Dispatcher.BeginInvoke(this._actionToRun, DispatcherPriority.Background);
            dispatchOp.Wait();
            this.DialogResult = true;
        }
        catch (Exception exception)
        {
            this._log.Error($"Failed to run background task: {this._actionToRun.Method}", exception);
            this.DialogResult = false;
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        this.Closed -= this.OnClosed;
        this._actionToRun = null;
    }
}