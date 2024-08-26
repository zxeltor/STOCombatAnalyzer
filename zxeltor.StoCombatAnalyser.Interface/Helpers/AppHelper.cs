// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.IO;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace zxeltor.StoCombatAnalyzer.Interface.Helpers;

public static class AppHelper
{
    /// <summary>
    ///     Used to find child control of a given type.
    /// </summary>
    /// <typeparam name="T">The control type</typeparam>
    /// <param name="depObj">The parent control</param>
    /// <returns>An enumeration of child controls</returns>
    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj != null)
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T) yield return (T)child;

                foreach (var childOfChild in FindVisualChildren<T>(child)) yield return childOfChild;
            }
    }
}