﻿// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;

/// <summary>
///     This class represents a line of data from the STO combat logs.
/// </summary>
public class CombatEvent : INotifyPropertyChanged, IEquatable<CombatEvent>
{
    ///// <summary>
    /////     ToddDo: I'm experimenting with using regex to parse log file lines. At the moment, it's considerably slower than my
    /////     original method.
    /////     Will revisit this when I can.
    ///// </summary>
    //private static readonly Regex RegexCombatLogLineRegex =
    //    new(Settings.Default.CombatLogLineRegex, RegexOptions.Compiled);

    /// <summary>
    ///     The main constructor
    /// </summary>
    /// <param name="filename">The source file where this entry came from.</param>
    /// <param name="combatLogEntry">The line from the source file.</param>
    /// <param name="lineNumber">The file line number where this entry originated from.</param>
    public CombatEvent(string filename, string combatLogEntry, long lineNumber)
    {
        this.OriginalFileName = filename;
        this.OriginalFileLineString = combatLogEntry;
        this.OriginalFileLineNumber = lineNumber;

        this.ParseLogFileEntryOldSchool(combatLogEntry);
    }

    /// <summary>
    ///     The source file where this entry came from.
    /// </summary>
    public string OriginalFileName { get; set; }

    /// <summary>
    ///     The line from the source file.
    /// </summary>
    public long OriginalFileLineNumber { get; set; }

    /// <summary>
    ///     The file line number where this entry originated from.
    /// </summary>
    public string OriginalFileLineString { get; set; }

    /// <summary>
    ///     A hashcode derived from properties in this class. An attempt to create a unique id.
    /// </summary>
    public int OriginalHashCode { get; private set; }

    /// <summary>
    ///     If true this event was from a Player. False it was for a Non-Player.
    /// </summary>
    public bool IsPlayerEntity { get; private set; }

    /// <summary>
    ///     True if the damage from this event came from an entities pet. False otherwise.
    /// </summary>
    public bool IsPetEvent { get; private set; }

    /// <summary>
    ///     True if this event had no owner assigned to it originally. False otherwise.
    ///     <para>
    ///         If true, this means the target related information was used to populate the owner information. Assuming this
    ///         was something the entity cast on itself?
    ///     </para>
    /// </summary>
    public bool IsOwnerModified { get; private set; }

    /// <summary>
    ///     Timestamp
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was part of entry 0 in the CSV.
    ///     </para>
    /// </summary>
    public DateTime Timestamp { get; private set; }

    /// <summary>
    ///     Display name of owner.
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was part of entry 0 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public string OwnerDisplay { get; private set; }

    /// <summary>
    ///     Internal name of owner
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 1 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public string OwnerInternal { get; private set; }

    /// <summary>
    ///     The OwnerInternal id stripped of it's ID wrapper.
    /// </summary>
    public string OwnerInternalStripped { get; private set; }

    /// <summary>
    ///     Display name of source(only appears if Pet/Gravity Well etc)
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 2 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public string SourceDisplay { get; private set; }

    /// <summary>
    ///     Internal name of source
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 3 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public string SourceInternal { get; private set; }

    /// <summary>
    ///     Display name of target
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 4 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public string TargetDisplay { get; private set; }

    /// <summary>
    ///     Internal name of target
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 5 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public string TargetInternal { get; private set; }

    /// <summary>
    ///     True if the target is a non-player. False otherwise.
    /// </summary>
    public bool IsTargetNonPlayer { get; private set; }

    /// <summary>
    ///     The TargetInternal id stripped of it's ID wrapper.
    /// </summary>
    public string TargetInternalStripped { get; private set; }

    /// <summary>
    ///     Display name of event
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 6 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public string EventDisplay { get; private set; }

    /// <summary>
    ///     Internal name of event
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 7 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public string EventInternal { get; private set; }

    /// <summary>
    ///     Type (Shield or Plasma/Antiproton etc)
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 8 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    ///     Flags (Critical, Flank, Dodge, Miss etc)
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 9 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public string Flags { get; private set; }

    /// <summary>
    ///     Magnitude
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 10 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public double Magnitude { get; private set; }

    /// <summary>
    ///     Base magnitude
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 11 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public double MagnitudeBase { get; private set; }

    /// <inheritdoc />
    public bool Equals(CombatEvent? other)
    {
        return this.GetHashCode() == other?.GetHashCode();
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     This method is used to parse line from a STO combat log.
    /// </summary>
    /// <param name="combatLogEntry">A line entry from a combat log</param>
    private void ParseLogFileEntryOldSchool(string combatLogEntry)
    {
        if (combatLogEntry == null)
            throw new NullReferenceException(nameof(combatLogEntry));

        var combatLogEntryDataArray = combatLogEntry.Split(',');

        if (combatLogEntryDataArray == null)
            throw new Exception("Failed to parse combat log entry. Line was not properly delimited with commas.");

        if (combatLogEntryDataArray.Length != 12)
            throw new Exception(
                "Failed to parse combat log entry. Parse did not return the expected number of data entries (12) from the Line.");

        // Here we parse the timestamp and the owner display from the first CSV entry (24:04:11:17:10:35.9::zxeltor)
        var timeAndOwnerLabel = combatLogEntryDataArray[0];

        var timeAndOwnerLabelArray = timeAndOwnerLabel.Split(":");
        var secondsArray = timeAndOwnerLabelArray[5].Split(".");

        if (timeAndOwnerLabelArray == null || !timeAndOwnerLabelArray.Any() || secondsArray == null ||
            !secondsArray.Any())
            throw new Exception($"Failed to parse {nameof(this.Timestamp)} and OwnerLabel from Line.");

        this.Timestamp = DateTime.MinValue;

        if (int.TryParse(timeAndOwnerLabelArray[0], out var year))
            if (int.TryParse(timeAndOwnerLabelArray[1], out var month))
                if (int.TryParse(timeAndOwnerLabelArray[2], out var day))
                    if (int.TryParse(timeAndOwnerLabelArray[3], out var hour))
                        if (int.TryParse(timeAndOwnerLabelArray[4], out var min))
                            if (int.TryParse(secondsArray[0], out var sec))
                                if (int.TryParse(secondsArray[1], out var fraction))
                                    this.Timestamp = new DateTime(2000 + year, month, day, hour, min, sec,
                                        fraction * 100, DateTimeKind.Local);

        if (this.Timestamp == DateTime.MinValue)
            throw new Exception($"Failed to parse {nameof(this.Timestamp)} from Line.");

        this.OwnerDisplay = timeAndOwnerLabelArray[7];
        this.OwnerInternal = combatLogEntryDataArray[1];
        this.SourceDisplay = combatLogEntryDataArray[2];
        this.SourceInternal = combatLogEntryDataArray[3];
        this.TargetDisplay = combatLogEntryDataArray[4];
        this.TargetInternal = combatLogEntryDataArray[5];
        this.EventDisplay = combatLogEntryDataArray[6];
        this.EventInternal = combatLogEntryDataArray[7];
        this.Type = combatLogEntryDataArray[8];
        this.Flags = combatLogEntryDataArray[9];

        if (!double.TryParse(combatLogEntryDataArray[10], out var magnitudeResult))
            throw new Exception($"Failed to parse {nameof(this.Magnitude)} from Line.");

        if (!double.TryParse(combatLogEntryDataArray[11], out var magnitudeBaseResult))
            throw new Exception($"Failed to parse {nameof(this.MagnitudeBase)} from Line.");

        this.Magnitude = magnitudeResult;
        this.MagnitudeBase = magnitudeBaseResult;

        this.OriginalHashCode = this.GetHashCode();

        // If no owner is specified, we assume the target is the owner, and assign the target as the owner.
        if (string.IsNullOrWhiteSpace(this.OwnerInternal))
        {
            // Prepend the astrix as an extra label to identify this wierd event.
            this.OwnerDisplay = this.TargetDisplay;
            this.OwnerInternal = this.TargetInternal;
            this.IsOwnerModified = true;
        }

        if (!string.IsNullOrWhiteSpace(this.SourceDisplay))
            this.IsPetEvent = true;

        if (!this.IsPetEvent && OwnerInternal.StartsWith("C["))
        {
            var splitResult = OwnerInternal.Split(" ");
            if (splitResult.Length == 2)
            {
                OwnerInternalStripped = splitResult[1].Replace("]", "");
            }
        }

        if (!this.IsPetEvent && TargetInternal.StartsWith("C["))
        {
            var splitResult = TargetInternal.Split(" ");
            if (splitResult.Length == 2)
            {
                IsTargetNonPlayer = true;
                TargetInternalStripped = splitResult[1].Replace("]", "");
            }
        }

        if (this.OwnerInternal.StartsWith("P["))
            this.IsPlayerEntity = true;
    }

    ///// <summary>
    /////     ToddDo: I'm experimenting with using regex to parse log file lines. At the moment, it's considerably slower than my
    /////     original method.
    /////     Will revisit this when I can.
    ///// </summary>
    //private void ParseLogFileEntryUsingRegex(string combatLogEntry)
    //{
    //    if (combatLogEntry == null)
    //        throw new NullReferenceException(nameof(combatLogEntry));

    //    var logRegexMatch = RegexCombatLogLineRegex.Match(combatLogEntry);

    //    if (logRegexMatch == null)
    //        throw new Exception(
    //            "Failed to parse combat log entry. Line was not properly delimited with commas, or you need to fix your CombatLogFileRegex setting.");

    //    // 24:04:11:17:10:35.9::zxeltor
    //    this.Timestamp = DateTime.MinValue;

    //    if (int.TryParse(logRegexMatch.Groups["year"].Value, out var year))
    //        if (int.TryParse(logRegexMatch.Groups["month"].Value, out var month))
    //            if (int.TryParse(logRegexMatch.Groups["day"].Value, out var day))
    //                if (int.TryParse(logRegexMatch.Groups["hour"].Value, out var hour))
    //                    if (int.TryParse(logRegexMatch.Groups["min"].Value, out var min))
    //                        if (int.TryParse(logRegexMatch.Groups["sec"].Value, out var sec))
    //                            if (int.TryParse(logRegexMatch.Groups["milli"].Value, out var fraction))
    //                                this.Timestamp = new DateTime(2000 + year, month, day, hour, min, sec,
    //                                    fraction * 100, DateTimeKind.Local);

    //    if (this.Timestamp == DateTime.MinValue)
    //        throw new Exception($"Failed to parse {nameof(this.Timestamp)} from Line.");

    //    this.OwnerDisplay = logRegexMatch.Groups["OwnerDisplay"].Value;
    //    this.OwnerInternal = logRegexMatch.Groups["OwnerInternal"].Value;
    //    this.SourceDisplay = logRegexMatch.Groups["SourceDisplay"].Value;
    //    this.SourceInternal = logRegexMatch.Groups["SourceInternal"].Value;
    //    this.TargetDisplay = logRegexMatch.Groups["TargetDisplay"].Value;
    //    this.TargetInternal = logRegexMatch.Groups["TargetInternal"].Value;
    //    this.EventTypeLabel = logRegexMatch.Groups["EventTypeLabel"].Value;
    //    this.EventTypeId = logRegexMatch.Groups["EventTypeId"].Value;
    //    this.Type = logRegexMatch.Groups["Type"].Value;
    //    this.Flags = logRegexMatch.Groups["Flags"].Value;

    //    if (!double.TryParse(logRegexMatch.Groups["Magnitude"].Value, out var magnitudeResult))
    //        throw new Exception($"Failed to parse {nameof(this.Magnitude)} from Line.");

    //    if (!double.TryParse(logRegexMatch.Groups["MagnitudeBase"].Value, out var magnitudeBaseResult))
    //        throw new Exception($"Failed to parse {nameof(this.MagnitudeBase)} from Line.");

    //    this.Magnitude = magnitudeResult;
    //    this.MagnitudeBase = magnitudeBaseResult;

    //    this.OriginalHashCode = this.GetHashCode();

    //    if (string.IsNullOrWhiteSpace(this.OwnerInternal))
    //    {
    //        this.OwnerDisplay = this.TargetDisplay;
    //        this.OwnerInternal = this.TargetInternal;
    //        this.IsOwnerModified = true;
    //    }

    //    if (!string.IsNullOrWhiteSpace(this.SourceDisplay) && !this.SourceDisplay.Equals("*"))
    //        this.IsPetEvent = true;

    //    if (this.OwnerInternal.StartsWith("P["))
    //        this.IsPlayerEntity = true;
    //}

    /// <summary>
    ///     A helper method created to support the <see cref="INotifyPropertyChanged" /> implementation of this class.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return
            $"Timestamp={this.Timestamp:s}, Owner={this.OwnerInternal}, Target={this.TargetInternal}, {this.EventInternal}, {this.Type}";
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Timestamp, this.OwnerInternal, this.SourceInternal, this.TargetInternal,
            this.EventInternal, this.Type,
            this.Flags, this.Magnitude);
    }
}