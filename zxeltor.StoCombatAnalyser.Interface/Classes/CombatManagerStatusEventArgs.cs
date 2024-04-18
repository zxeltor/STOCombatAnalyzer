// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

namespace zxeltor.StoCombatAnalyzer.Interface.Classes;

/// <summary>
///     An event arg used by <see cref="CombatLogManager" /> when passing status events back to the main window.
/// </summary>
public class CombatManagerStatusEventArgs : EventArgs
{
    /// <summary>
    ///     The main constructor
    /// </summary>
    /// <param name="statusMessage">A message pass back to the main window.</param>
    /// <param name="isError">True if the message is an error. False otherwise.</param>
    public CombatManagerStatusEventArgs(string statusMessage, bool isError = false)
    {
        this.StatusMessage = statusMessage;
        this.IsError = isError;
    }

    /// <summary>
    ///     The status message from <see cref="CombatLogManager" />
    /// </summary>
    public string StatusMessage { get; }

    /// <summary>
    ///     True if the message is an error. False otherwise.
    /// </summary>
    public bool IsError { get; }
}