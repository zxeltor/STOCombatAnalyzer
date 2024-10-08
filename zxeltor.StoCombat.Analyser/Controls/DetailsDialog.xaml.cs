﻿// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Windows;

namespace zxeltor.StoCombat.Analyzer.Controls;

/// <summary>
///     A custom dialog box which supports displaying extended details, with an option to display without blocking the app
///     calling it.
/// </summary>
public partial class DetailsDialog : Window
{
    #region Private Fields

    // Determines if the dialog should block when opened.
    private readonly bool _isModalDialog;

    #endregion

    #region Constructors

    /// <summary>
    ///     The main constructor. Made private so only the static <see cref="Show" /> and <see cref="ShowDialog" /> methods
    ///     were the only way to create instances of this class.
    /// </summary>
    /// <param name="owner">The parent window.</param>
    /// <param name="isModalDialog">Determines if the dialog should block the caller when this dialog is displayed.</param>
    /// <param name="message">The message to display to the user.</param>
    /// <param name="caption">
    ///     The caption for the dialog.
    ///     <para>If not provided, a default is used.</para>
    /// </param>
    /// <param name="includeCancel">
    ///     True to include a cancel button. False otherwise.
    ///     <para>If not provided, the cancel button won't be displayed.</para>
    /// </param>
    /// <param name="detailsBoxList">
    ///     A list of details to be displayed in the details box.
    ///     <para>If not provided, the details box won't be displayed.</para>
    /// </param>
    /// <param name="detailsBoxCaption">
    ///     A caption to use for the detail box inside the dialog.
    ///     <para>If not provided, a default is used, assuming details are provided.</para>
    /// </param>
    private DetailsDialog(Window? owner, bool isModalDialog, string? header, string message, string? caption = null,
        bool includeCancel = false,
        string? detailsBoxCaption = null, List<string>? detailsBoxList = null)
    {
        this.InitializeComponent();

        this.Owner = owner ??= Application.Current.MainWindow;
        this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

        this.Title = string.IsNullOrWhiteSpace(caption) ? this.Title : caption;

        this._isModalDialog = isModalDialog;

        this.uiTextBlockHeader.Text = header ?? string.Empty;
        this.uiTextBlockMessage.Text = message;

        this.uiButtonOk.Click += this.UiButtonOk_Click;

        this.Closed += this.OnClosed;
    }

    #endregion

    #region Public Members

    /// <summary>
    ///     Show the dialog, without blocking the caller.
    /// </summary>
    /// <param name="owner">The parent window.</param>
    /// <param name="message">The message to display to the user.</param>
    /// <param name="caption">
    ///     The caption for the dialog.
    ///     <para>If not provided, a default is used.</para>
    /// </param>
    /// <param name="includeCancel">
    ///     True to include a cancel button. False otherwise.
    ///     <para>If not provided, the cancel button won't be displayed.</para>
    /// </param>
    /// <param name="detailsBoxList">
    ///     A list of details to be displayed in the details box.
    ///     <para>If not provided, the details box won't be displayed.</para>
    /// </param>
    /// <param name="detailsBoxCaption">
    ///     A caption to use for the detail box inside the dialog.
    ///     <para>If not provided, a default is used, assuming details are provided.</para>
    /// </param>
    public static void Show(Window? owner, string message, string? caption = null, string? header = null)
    {
        var dialog = new DetailsDialog(owner, false, header, message, caption);
        dialog.Show();
    }

    /// <summary>
    ///     Show the dialog, and blocking the caller until the dialog is closed.
    /// </summary>
    /// <param name="owner">The parent window.</param>
    /// <param name="message">The message to display to the user.</param>
    /// <param name="caption">
    ///     The caption for the dialog.
    ///     <para>If not provided, a default is used.</para>
    /// </param>
    /// <param name="includeCancel">
    ///     True to include a cancel button. False otherwise.
    ///     <para>If not provided, the cancel button won't be displayed.</para>
    /// </param>
    /// <param name="detailsBoxList">
    ///     A list of details to be displayed in the details box.
    ///     <para>If not provided, the details box won't be displayed.</para>
    /// </param>
    /// <param name="detailsBoxCaption">
    ///     A caption to use for the detail box inside the dialog.
    ///     <para>If not provided, a default is used, assuming details are provided.</para>
    /// </param>
    /// <returns>True if the OK button was clicked. False otherwise.</returns>
    public static bool ShowDialog(Window? owner, string header, string message, string? caption = null)
    {
        var dialog = new DetailsDialog(owner, true, header, message, caption);

        var dialogResult = dialog.ShowDialog();

        if (dialogResult.HasValue && dialogResult.Value) return true;

        return false;
    }

    #endregion

    #region Other Members

    private void UiButtonOk_Click(object sender, RoutedEventArgs e)
    {
        if (this._isModalDialog) this.DialogResult = true;

        this.Close();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        this.Closed -= this.OnClosed;

        this.uiButtonOk.Click -= this.UiButtonOk_Click;
    }

    #endregion
}