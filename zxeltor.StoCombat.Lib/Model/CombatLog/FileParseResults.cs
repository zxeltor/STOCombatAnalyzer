// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.IO;

namespace zxeltor.StoCombat.Lib.Model.CombatLog;

/// <summary>
///     A container used to track parsing statistics for our combat log files.
/// </summary>
public class FileParseResults
{
    /// <summary>
    ///     The main constructor
    /// </summary>
    /// <param name="fileName">The name of the file</param>
    public FileParseResults(string fileName)
    {
        this.FileName = Path.GetFileName(fileName);
    }

    /// <summary>
    ///     The name of the combat log file.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    ///     The number of lines from the file successfully parsed.
    /// </summary>
    public long SuccessfulParses { get; set; } = 0;

    /// <summary>
    ///     The number of lines from the file that failed to parse correctly.
    /// </summary>
    public long FailedParses { get; set; } = 0;

    /// <inheritdoc />
    public override string ToString()
    {
        return
            $"F={Path.GetFileName(this.FileName)}, S={this.SuccessfulParses}, F={this.FailedParses}";
    }

    /// <summary>
    ///     A string property used to provide a string representation of this object for logging purposes.
    /// </summary>
    /// <returns>The pretty log string representation of this object.</returns>
    public string ToLog()
    {
        return
            $"{Path.GetFileName(this.FileName)}, ParsedLines={this.SuccessfulParses}, FailedLines={this.FailedParses}";
    }
}