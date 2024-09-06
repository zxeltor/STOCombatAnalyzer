// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Text.RegularExpressions;
using log4net;
using zxeltor.Types.Lib.Extensions;

namespace zxeltor.StoCombat.Lib.Model.CombatLog;

/// <summary>
///     This class represents a line of data from the STO combat logs.
/// </summary>
public class CombatEvent : IEquatable<CombatEvent>
{
    #region Static Fields and Constants

    private static readonly Regex RegexPlayerInternalStripper =
        new(@"P\[[\w#]+@[\w#]+\s+(?<stripped_name>[\S ]+@[\S]+)\]", RegexOptions.Compiled);

    private static readonly Regex RegexNonPlayerInternalStripper =
        new(@"C\[[\w#]+\s+(?<stripped_name>[\w#]+)]", RegexOptions.Compiled);

    #endregion

    #region Private Fields

    private readonly ILog _log = LogManager.GetLogger(nameof(CombatEvent));

    #endregion

    #region Constructors

    /// <summary>
    ///     Constructor need for JSON deserialization
    /// </summary>
    public CombatEvent()
    {
    }

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

        this.ParseLogFileEntry(combatLogEntry);
    }

    #endregion

    #region Public Properties

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
    public int OriginalHashCode { get; set; }

    /// <summary>
    ///     If true this event was from a Player. False it was for a Non-Player.
    /// </summary>
    public bool IsOwnerPlayer { get; set; }

    /// <summary>
    ///     True if the damage from this event came from an entities pet. False otherwise.
    /// </summary>
    public bool IsOwnerPetEvent { get; set; }

    /// <summary>
    ///     True if this event had no owner assigned to it originally. False otherwise.
    ///     <para>
    ///         If true, this means the target related information was used to populate the owner information. Assuming this
    ///         was something the entity cast on itself?
    ///     </para>
    /// </summary>
    public bool IsOwnerModified { get; set; }

    /// <summary>
    ///     Timestamp
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was part of entry 0 in the CSV.
    ///     </para>
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    ///     Display name of owner.
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was part of entry 0 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public string OwnerDisplay { get; set; }

    /// <summary>
    ///     Internal name of owner
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 1 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public string OwnerInternal { get; set; }

    /// <summary>
    ///     The OwnerInternal id stripped of it's ID wrapper.
    /// </summary>
    public string OwnerInternalStripped { get; set; }

    /// <summary>
    ///     Display name of source(only appears if Pet/Gravity Well etc.)
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 2 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public string SourceDisplay { get; set; }

    /// <summary>
    ///     Internal name of source
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 3 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public string SourceInternal { get; set; }

    /// <summary>
    ///     The SourceInternal id stripped of it's ID wrapper.
    /// </summary>
    public string SourceInternalStripped { get; set; }

    /// <summary>
    ///     Display name of target
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 4 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public string TargetDisplay { get; set; }

    /// <summary>
    ///     Internal name of target
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 5 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public string TargetInternal { get; set; }

    /// <summary>
    ///     True if the target is a non-player. False otherwise.
    /// </summary>
    public bool IsTargetPlayer { get; set; }

    /// <summary>
    ///     The TargetInternal id stripped of it's ID wrapper.
    /// </summary>
    public string TargetInternalStripped { get; set; }

    /// <summary>
    ///     Display name of event
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 6 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public string EventDisplay { get; set; }

    /// <summary>
    ///     Internal name of event
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 7 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public string EventInternal { get; set; }

    /// <summary>
    ///     Type (Shield or Plasma/Antiproton etc.)
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 8 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    ///     Flags (Critical, Flank, Dodge, Miss etc.)
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 9 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public string Flags { get; set; }

    /// <summary>
    ///     Magnitude
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 10 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public double Magnitude { get; set; }

    /// <summary>
    ///     Base magnitude
    ///     <para>
    ///         This was a field parsed directly from the original CSV log file entry.
    ///         This was entry 11 in the CSV. Entries are zero based.
    ///     </para>
    /// </summary>
    public double MagnitudeBase { get; set; }

    #endregion

    #region Public Members

    /// <inheritdoc />
    public bool Equals(CombatEvent? other)
    {
        return this.GetHashCode() == other?.GetHashCode();
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

    #endregion

    #region Other Members

    /// <summary>
    ///     This method is used to parse line from a STO combat log.
    /// </summary>
    /// <param name="combatLogEntry">A line entry from a combat log</param>
    private void ParseLogFileEntry(string combatLogEntry)
    {
        if (combatLogEntry == null)
            throw new NullReferenceException(nameof(combatLogEntry));

        var combatLogEntryDataArray = combatLogEntry.RemoveSpecialChars().Split(',');

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

        if (!string.IsNullOrWhiteSpace(this.SourceDisplay) && !string.IsNullOrWhiteSpace(this.SourceInternal) && !this.SourceInternal.Equals("*"))
        {
            this.IsOwnerPetEvent = true;

            this.StripInternalId(this.SourceInternal, out var sourceInternalIdStripped, out var sourceInternalIsPlayer);
            this.SourceInternalStripped = sourceInternalIdStripped;
        }

        if (!string.IsNullOrWhiteSpace(this.OwnerDisplay) && !string.IsNullOrWhiteSpace(this.OwnerInternal) &&
            !this.OwnerInternal.Equals("*"))
        {
            this.StripInternalId(this.OwnerInternal, out var ownerInternalIdStripped, out var ownerInternalIsPlayer);
            this.IsOwnerPlayer = ownerInternalIsPlayer;
            this.OwnerInternalStripped = ownerInternalIdStripped;
        }

        if (!string.IsNullOrWhiteSpace(this.TargetDisplay) && !string.IsNullOrWhiteSpace(this.TargetInternal) &&
            !this.TargetInternal.Equals("*"))
        {
            this.StripInternalId(this.TargetInternal, out var targetInternalIdToStripped, out var targetInternalIsPlayer);
            this.IsTargetPlayer = targetInternalIsPlayer;
            this.TargetInternalStripped = targetInternalIdToStripped;
        }
    }

    /// <summary>
    ///     Strip an internal id of its wrapper.
    /// </summary>
    /// <param name="internalIdToStrip">The internal id to strip</param>
    /// <param name="strippedInternalId">The stripped internal id</param>
    /// <param name="isPlayer">True is the internalId is for a player. False otherwise.</param>
    /// <returns>True if successfully stripped internal id. False otherwise.</returns>
    private void StripInternalId(string internalIdToStrip, out string strippedInternalId, out bool isPlayer)
    {
        strippedInternalId = null;
        isPlayer = false;

        try
        {
            /*
             Example internal ids
             P[2117915@4455101 zxeltor@zxeltor]
             P[12121895@25092315 Darjekt Haan@vaultcultist#9414]
             C[62 Space_Tzenkethi_Frigate_Carrier_Pet]
            */
            if (internalIdToStrip.StartsWith("P[", StringComparison.CurrentCultureIgnoreCase))
            {
                isPlayer = true;

                if (RegexPlayerInternalStripper.IsMatch(internalIdToStrip))
                {
                    var groups = RegexPlayerInternalStripper.Match(internalIdToStrip).Groups;

                    if (groups.TryGetValue("stripped_name", out var value) && value.Success)
                    {
                        strippedInternalId = value.Value;
                        return;
                    }
                }
            }

            if (internalIdToStrip.StartsWith("C[", StringComparison.CurrentCultureIgnoreCase))
            {
                if (RegexNonPlayerInternalStripper.IsMatch(internalIdToStrip))
                {
                    var groups = RegexNonPlayerInternalStripper.Match(internalIdToStrip).Groups;

                    if (groups.TryGetValue("stripped_name", out var value) && value.Success)
                    {
                        strippedInternalId = value.Value;
                        return;
                    }
                }
            }

            //this._log.Debug($"Failed to parse internal id: \"{internalIdToStrip}\"");
        }
        catch (Exception e)
        {
            this._log.Error($"Failed to parse internal id: \"{internalIdToStrip}\"", e);
        }

        strippedInternalId = internalIdToStrip;
    }

    #endregion
}