// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Windows;
using zxeltor.ConfigUtilsHelpers.Extensions;

namespace zxeltor.StoCombatAnalyzer.Interface.Controls;

/// <summary>
///     Interaction logic for EditTextFieldDialog.xaml
/// </summary>
public partial class EditTextFieldDialog : Window
{
    #region Constructors

    public EditTextFieldDialog(Window parent)
    {
        this.InitializeComponent();

        this.Owner = parent;
        this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

        this.ContentRendered += this.OnContentRendered;
    }

    #endregion

    #region Public Members

    public bool? ShowDialog(string description, ref string? value)
    {
        this.uiLabelDescription.Content = description;
        this.uiTextBoxValue.Text = value ?? string.Empty;

        var dialogResult = this.ShowDialog();

        if (dialogResult.HasValue && dialogResult.Value)
        {
            value = this.uiTextBoxValue.Text.RemoveSpecialChars();
            return true;
        }

        return false;
    }

    #endregion

    #region Other Members

    private void OnContentRendered(object? sender, EventArgs e)
    {
        this.ContentRendered -= this.OnContentRendered;
        this.uiTextBoxValue.Focus();
    }

    private void UiButtonSave_OnClick(object sender, RoutedEventArgs e)
    {
        this.DialogResult = true;
    }

    private void UiButtonCancel_OnClick(object sender, RoutedEventArgs e)
    {
        this.DialogResult = false;
    }

    #endregion
}