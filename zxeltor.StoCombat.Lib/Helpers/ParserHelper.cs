// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.IO;
using System.Text;
using log4net;
using zxeltor.StoCombat.Lib.Model.CombatLog;
using zxeltor.StoCombat.Lib.Model.CombatMap;
using zxeltor.StoCombat.Lib.Parser;
using zxeltor.Types.Lib.Helpers;
using zxeltor.Types.Lib.Result;

namespace zxeltor.StoCombat.Lib.Helpers;

/// <summary>
///     A wrapper around static helper methods use to parse STO combat files, and other combat entities.
/// </summary>
public static class ParserHelper
{
    #region Static Fields and Constants

    private static readonly ILog Log = LogManager.GetLogger(typeof(ParserHelper));

    #endregion

    #region Public Members

    /// <summary>
    ///     Get a combat list from the STO log folder.
    ///     <para>Note: A valid folder path, and file search pattern, need to be defined in the parser settings for this work.</para>
    /// </summary>
    /// <param name="combatLogParseSettings">The parse settings</param>
    /// <param name="finalCombatListResult">The combat list parsed from the STO log folder.</param>
    /// <returns>
    ///     <see cref="CombatLogParserResult" />
    /// </returns>
    public static CombatLogParserResult TryGetCombatList(CombatLogParseSettings combatLogParseSettings,
        out List<Combat>? finalCombatListResult)
    {
        var resultFinal = new CombatLogParserResult();
        finalCombatListResult = new List<Combat>();

        var parseStartTimeUtc = DateTime.UtcNow;

        var resultEvents = TryGetCombatEventsListFromStoCombatFolder(combatLogParseSettings, parseStartTimeUtc,
            out var combatEventListOrderedByTimestamp);
        resultFinal.MergeResult(resultEvents);
        if (resultFinal.MaxLevel >= ResultLevel.Halt)
            return resultFinal;

        var resultCombatList = TryGetCombatListFromCombatEvents(combatLogParseSettings,
            combatEventListOrderedByTimestamp,
            out var combatListResult);
        resultFinal.MergeResult(resultCombatList);
        if (resultFinal.MaxLevel >= ResultLevel.Halt)
            return resultFinal;

        finalCombatListResult = combatListResult;

        var resultPostProcessing = TryApplyCombatListPostProcessing(combatLogParseSettings, finalCombatListResult);
        resultFinal.MergeResult(resultPostProcessing);
        if (resultFinal.MaxLevel >= ResultLevel.Halt)
            return resultFinal;

        finalCombatListResult.ForEach(combat => combat.LockObject());

        var eventsCount = combatListResult.Sum(combat => combat.AllCombatEvents.Count);
        var howFarBackInTime = TimeSpan.FromHours(combatLogParseSettings.HowFarBackForCombatInHours);
        var completionMessage =
            $"Successfully parsed \"{combatListResult.Count}\" combats from \"{eventsCount}\" events from \"{resultFinal.StoCombatLogFiles.Count}\" files for the past \"{howFarBackInTime.TotalHours}\" hours.";
        Log.Debug(completionMessage);
        resultFinal.AddMessage(completionMessage);

        return resultFinal;
    }

    /// <summary>
    ///     Get a combat list from using the specified file list. If isJsonFileList is true, the logic assumes you attempting
    ///     to load a
    ///     list of combat.json files. If false, the logic assumes you're loading STO combat log files.
    ///     <para>Note: Loading files this way, ignores times limitations when parsing the data.</para>
    /// </summary>
    /// <param name="combatLogParseSettings">The parse settings</param>
    /// <param name="filesToParse">A list of files to parse</param>
    /// <param name="isJsonFileList">
    ///     If true, the file list is combat JSON. If false, the file list is standard STO combat log
    ///     text files.
    /// </param>
    /// <param name="finalCombatListResult">The combat list parsed from the STO log folder.</param>
    /// <returns>
    ///     <see cref="CombatLogParserResult" />
    /// </returns>
    public static CombatLogParserResult TryGetCombatListFromFiles(CombatLogParseSettings combatLogParseSettings,
        List<string> filesToParse, bool isJsonFileList, out List<Combat>? finalCombatListResult)
    {
        var resultFinal = new CombatLogParserResult();
        finalCombatListResult = new List<Combat>();

        var resultEvents = isJsonFileList
            ? TryGetCombatEventsListFromCombatJsonFileList(filesToParse, out var combatEventListOrderedByTimestamp)
            : TryGetCombatEventsListFromFileList(filesToParse, out combatEventListOrderedByTimestamp);

        resultFinal.MergeResult(resultEvents);
        if (resultFinal.MaxLevel >= ResultLevel.Halt)
            return resultFinal;

        var resultCombatList = TryGetCombatListFromCombatEvents(combatLogParseSettings,
            combatEventListOrderedByTimestamp,
            out var combatListResult);
        resultFinal.MergeResult(resultCombatList);
        if (resultFinal.MaxLevel >= ResultLevel.Halt)
            return resultFinal;

        finalCombatListResult = combatListResult;

        var resultPostProcessing = TryApplyCombatListPostProcessing(combatLogParseSettings, finalCombatListResult);
        resultFinal.MergeResult(resultPostProcessing);
        if (resultFinal.MaxLevel >= ResultLevel.Halt)
            return resultFinal;

        finalCombatListResult.ForEach(combat =>
        {
            combat.ImportedDate = DateTime.Now;
            combat.LockObject();
        });

        var eventsCount = combatListResult.Sum(combat => combat.AllCombatEvents.Count);
        var howFarBackInTime = TimeSpan.FromHours(combatLogParseSettings.HowFarBackForCombatInHours);
        var completionMessage =
            $"Successfully parsed \"{combatListResult.Count}\" combats from \"{eventsCount}\" events from \"{resultFinal.StoCombatLogFiles.Count}\" files for the past \"{howFarBackInTime.TotalHours}\" hours.";
        Log.Debug(completionMessage);
        resultFinal.AddMessage(completionMessage);

        return resultFinal;
    }

    /// <summary>
    ///     Purge the sto combat logs folder.
    ///     <para>Note: If there's only one log found, it won't be purged.</para>
    /// </summary>
    /// <param name="combatLogParseSettings">Parser settings</param>
    /// <param name="filesPurged">A list of files purged.</param>
    /// <returns>
    ///     <see cref="Result" />
    /// </returns>
    public static Result TryPurgeCombatLogFolder(CombatLogParseSettings combatLogParseSettings,
        out List<string> filesPurged)
    {
        filesPurged = [];
        var finalResult = new Result();

        try
        {
            if (!Directory.Exists(combatLogParseSettings.CombatLogPath))
            {
                var errorResponse =
                    "Can't purge the combat logs. The folder doesn't exist. Check CombatLogPath in settings.";
                Log.Error(
                    $"Can't purge the combat logs. The folder doesn't exist. \"{combatLogParseSettings.CombatLogPath}\"");
                return finalResult.AddLast(new Result(errorResponse, resultLevel: ResultLevel.Halt));
            }

            var combatLogInfoList = Directory
                .GetFiles(combatLogParseSettings.CombatLogPath, combatLogParseSettings.CombatLogPathFilePattern,
                    SearchOption.TopDirectoryOnly).Select(file => new FileInfo(file)).ToList();

            if (!combatLogInfoList.Any() || combatLogInfoList.Count == 1)
                return finalResult.AddLast(new Result("No combat logs to parse.", resultLevel: ResultLevel.Halt));

            var tmpFilesPurged = new List<string>();

            combatLogInfoList.ForEach(fileInfo =>
            {
                if (fileInfo.IsReadOnly) return;

                if (DateTime.Now - fileInfo.LastWriteTime <=
                    TimeSpan.FromDays(combatLogParseSettings.HowLongToKeepLogsInDays)) return;

                File.Delete(fileInfo.FullName);
                tmpFilesPurged.Add(fileInfo.Name);
            });

            filesPurged.AddRange(tmpFilesPurged);
        }
        catch (Exception ex)
        {
            var errorResponse = $"Can't purge the combat logs. Reason={ex.Message}";
            Log.Error("Failed to purge combat logs folder.", ex);
            finalResult.AddLast(new Result(errorResponse, ex, ResultLevel.Halt));
        }

        return finalResult;
    }

    #endregion

    #region Other Members

    /// <summary>
    ///     Find a map for the provided combat entity from our MapDetectionSettings object.
    /// </summary>
    /// <param name="combatLogParseSettings">Parser settings</param>
    /// <param name="combat">The provided combat entity</param>
    /// <returns>A combat map</returns>
    private static CombatMap? DetermineMapFromCombatMapDetectionSettings(
        CombatLogParseSettings combatLogParseSettings, Combat combat)
    {
        CombatMap? combatMap = null;

        var currentCombatPlayerCount = combat.PlayerEntities.Count;
        var mapDetectionSettings = combatLogParseSettings.MapDetectionSettings;

        // If we don't have our detection settings, or the combat instance has no unique
        // ids, then no point in continuing on.
        if (mapDetectionSettings == null) return combatMap;
        if (combat.UniqueEntityIds == null || combat.UniqueEntityIds.Count == 0) return combatMap;

        var filteredMapList = new List<CombatMap>();

        // Enforce various map detection settings from the main ui settings tab, if any are enabled.
        if (combatLogParseSettings.IsEnforceMapMinPlayerCount || combatLogParseSettings.IsEnforceMapMaxPlayerCount)
            foreach (var map in mapDetectionSettings.CombatMapEntityList)
            {
                if (combatLogParseSettings.IsEnforceMapMinPlayerCount)
                    if (map.MinPlayers > 0)
                        if (currentCombatPlayerCount < map.MinPlayers)
                            continue;

                if (combatLogParseSettings.IsEnforceMapMaxPlayerCount)
                    if (map.MaxPlayers > 0)
                        if (currentCombatPlayerCount > map.MaxPlayers)
                            continue;

                filteredMapList.Add(map);
            }
        else
            filteredMapList.AddRange(mapDetectionSettings.CombatMapEntityList);

        // Get a unique list of unique identifiers from our unique entities list.
        // We use both the entity label and id to construct our list
        var currentEntityUniqueIdentifiers = combat.UniqueEntityIds
            .Where(entity => !string.IsNullOrWhiteSpace(entity.Id))
            .Select(entity => entity.Id)
            .Union(combat.UniqueEntityIds.Where(entity => !string.IsNullOrWhiteSpace(entity.Label))
                .Select(entity => entity.Label))
            .Distinct().ToList();

        var idsFoundInMapList = false;
        var idsFoundInGenericList = false;

        // Find a match in our CombatMapDetectionSettings object based on unique ids.
        foreach (var currentEntity in currentEntityUniqueIdentifiers)
        {
            // If the currentEntityId is found in a map exclusion list,
            // then it gets thrown out of the rest of the detection logic,
            // and the maps with the ID are marked as exceptions for the
            // current combat entity being checked.
            var mapExceptions =
                (from map in filteredMapList
                    from ent in map.MapEntityExclusions
                    where map.IsEnabled && ent.IsEnabled && currentEntity.Contains(ent.Pattern)
                    select map).ToList();
            if (mapExceptions.Count > 0)
            {
                mapExceptions.ForEach(map => map.IsAnException = true);
                continue;
            }

            // If the currentEntityId is found in main exclusion list,
            // then it gets thrown out of the rest of the detection logic.
            var isException =
                mapDetectionSettings.EntityExclusionList.Where(ent => ent.IsEnabled).FirstOrDefault(ent =>
                    currentEntity.Contains(ent.Pattern));
            if (isException != null)
                continue;

            // Check to see if the currentEntityId is unique to a map. If a map
            // is found we return without doing further logic.
            var uniqueToMap =
                (from map in filteredMapList
                    from ent in map.MapEntities
                    where map.IsEnabled && ent.IsEnabled && !map.IsAnException && currentEntity.Contains(ent.Pattern) &&
                          ent.IsUniqueToMap
                    select map).FirstOrDefault();
            if (uniqueToMap != null)
                return uniqueToMap;

            // Get any match counts from our map list.
            var entitiesFoundInMap =
                (from map in filteredMapList
                    from ent in map.MapEntities
                    where map.IsEnabled && ent.IsEnabled && !map.IsAnException && currentEntity.Contains(ent.Pattern)
                    select ent).ToList();
            if (entitiesFoundInMap.Count > 0)
            {
                entitiesFoundInMap.ForEach(ent => ent.IncrementCombatMatchCount());
                idsFoundInMapList = true;
                continue;
            }

            // If we find any matches in the main map list, we don't need to bother
            // with checking our generic ground and space maps for matches.
            if (idsFoundInMapList)
                continue;

            // Get any match counts from our generic ground map.
            var entitiesFoundInGenericGroundMap =
                (from ent in mapDetectionSettings.GenericGroundMap.MapEntities
                    where ent.IsEnabled && currentEntity.Contains(ent.Pattern)
                    select ent).ToList();
            if (entitiesFoundInGenericGroundMap.Count > 0)
            {
                entitiesFoundInGenericGroundMap.ForEach(ent => ent.IncrementCombatMatchCount());
                idsFoundInGenericList = true;
            }

            // Get any match counts from our generic space map.
            var entitiesFoundInGenericSpacedMap =
                (from ent in mapDetectionSettings.GenericSpaceMap.MapEntities
                    where ent.IsEnabled && currentEntity.Contains(ent.Pattern)
                    select ent).ToList();
            if (entitiesFoundInGenericSpacedMap.Count > 0)
            {
                entitiesFoundInGenericSpacedMap.ForEach(ent => ent.IncrementCombatMatchCount());
                idsFoundInGenericList = true;
            }
        }

        // If we found any matches from our map list, we pick the one with the highest match count.
        if (idsFoundInMapList)
        {
            var mapResult = filteredMapList
                .Where(map => map.IsEnabled && !map.IsAnException)
                .OrderByDescending(map => map.CombatMatchCountForMap).First();

            return mapResult;
        }

        // If we found any matches from the generic ground and space maps, we pick the one with the highest match count.
        if (idsFoundInGenericList)
        {
            var mapResult = mapDetectionSettings.GenericGroundMap.CombatMatchCountForMap >=
                            mapDetectionSettings.GenericSpaceMap.CombatMatchCountForMap
                ? mapDetectionSettings.GenericGroundMap
                : mapDetectionSettings.GenericSpaceMap;
            return mapResult;
        }


        // If all of our checks fail, we return our null object.
        return combatMap;
    }

    /// <summary>
    /// </summary>
    /// <param name="combatLogParseSettings">Parser settings</param>
    /// <param name="combatListResult">The combat list</param>
    /// <returns>
    ///     <see cref="CombatLogParserResult" />
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static CombatLogParserResult TryApplyCombatListPostProcessing(
        CombatLogParseSettings combatLogParseSettings,
        List<Combat> combatListResult
    )
    {
        var resultFinal = new CombatLogParserResult();

        var resultRejection = TryApplyParserRejectLogicToCombatList(combatLogParseSettings, combatListResult);
        resultFinal.MergeResult(resultRejection);
        if (resultFinal.MaxLevel >= ResultLevel.Halt)
            return resultFinal;

        var resultSplitLogic = TryApplyRejectUnrelatedEntitiesToCombatList(combatLogParseSettings, combatListResult);
        resultFinal.MergeResult(resultSplitLogic);
        if (resultFinal.MaxLevel >= ResultLevel.Halt)
            return resultFinal;

        var resultDetectMap = TryDetermineMapsForCombatList(combatLogParseSettings, combatListResult);
        resultFinal.MergeResult(resultDetectMap);

        return resultFinal;
    }

    /// <summary>
    ///     Implements multiple basic parser rejection features, which attempts to clean up the combat list.
    /// </summary>
    /// <param name="combatLogParseSettings">The parser settings</param>
    /// <param name="combatList">The combat list</param>
    /// <returns>
    ///     <see cref="CombatLogParserResult" />
    /// </returns>
    private static CombatLogParserResult TryApplyParserRejectLogicToCombatList(
        CombatLogParseSettings combatLogParseSettings,
        List<Combat> combatList)
    {
        var resultFinal = new CombatLogParserResult();

        if (!combatLogParseSettings.IsRejectCombatWithNoPlayers
            && !combatLogParseSettings.IsRejectCombatIfUserPlayerNotIncluded
            && !combatLogParseSettings.IsEnforceCombatEventMinimum)
            return resultFinal;

        foreach (var combat in combatList.ToList())
        {
            if (combatLogParseSettings.IsRejectCombatWithNoPlayers && combat.PlayerEntities.Count == 0)
            {
                combat.Rejected = true;
                combat.RejectionReason = "Reject If No Players";
                combat.RejectionDetails =
                    $"Rejected Combat: Start={combat.CombatStart?.ToString("g")}, Reason={combat.RejectionReason}";
                if (!combatLogParseSettings.IsDisplayRejectedParserItemsInUi) combatList.Remove(combat);
                Log.Debug(combat.RejectionDetails);
                resultFinal.AddMessage(combat.RejectionDetails);
                resultFinal.AddRejectedObject(combat);
                continue;
            }

            if (combatLogParseSettings.IsRejectCombatIfUserPlayerNotIncluded &&
                !string.IsNullOrWhiteSpace(combatLogParseSettings.MyCharacter))
            {
                var accountPlayer = combat.PlayerEntities.FirstOrDefault(ent =>
                    ent.OwnerInternal.Contains(combatLogParseSettings.MyCharacter,
                        StringComparison.CurrentCultureIgnoreCase));

                if (accountPlayer == null)
                {
                    combat.Rejected = true;
                    combat.RejectionReason = "Reject If No Players From Account";
                    combat.RejectionDetails =
                        $"Rejected Combat: Start={combat.CombatStart?.ToString("g")}, Reason={combat.RejectionReason}";
                    if (!combatLogParseSettings.IsDisplayRejectedParserItemsInUi) combatList.Remove(combat);
                    Log.Debug(combat.RejectionDetails);
                    resultFinal.AddMessage(combat.RejectionDetails);
                    resultFinal.AddRejectedObject(combat);
                    continue;
                }
            }

            if (combatLogParseSettings.IsEnforceCombatEventMinimum &&
                combat.AllCombatEvents.Count < combatLogParseSettings.CombatEventCountMinimum)
            {
                combat.Rejected = true;
                combat.RejectionReason = "Enforce Event Minimum";
                combat.RejectionDetails =  $"Rejected Combat: Start={combat.CombatStart?.ToString("g")}, Reason={combat.RejectionReason}";

                if (!combatLogParseSettings.IsDisplayRejectedParserItemsInUi) combatList.Remove(combat);

                Log.Debug(combat.RejectionDetails);
                resultFinal.AddMessage(combat.RejectionDetails);
                resultFinal.AddRejectedObject(combat);
            }
        }

        return resultFinal;
    }

    /// <summary>
    ///     A feature which attempts to locate and remove combat entities incorrectly
    ///     merged into a combat instance.
    /// </summary>
    /// <param name="combatLogParseSettings">The parser settings</param>
    /// <param name="combatList">The combat list</param>
    /// <returns>
    ///     <see cref="CombatLogParserResult" />
    /// </returns>
    private static CombatLogParserResult TryApplyRejectUnrelatedEntitiesToCombatList(
        CombatLogParseSettings combatLogParseSettings,
        List<Combat> combatList)
    {
        var resultFinal = new CombatLogParserResult();
        if (!combatLogParseSettings.IsRemoveEntityOutliers
            || (!combatLogParseSettings.IsRemoveEntityOutliersPlayers
                && !combatLogParseSettings.IsRejectCombatIfUserPlayerNotIncluded))
            return resultFinal;

        var timeSpanBetweenCombats = TimeSpan.FromSeconds(combatLogParseSettings.HowLongBeforeNewCombatInSeconds);
        DateTime combatStartPlusOffset;
        DateTime combatEndMinusOffset;
        TimeSpan originalCombatDuration;
        DateTime originalCombatStart;
        DateTime originalCombatEnd;
        string entityTypeLabel;

        combatList.ForEach(combat =>
        {
            // If the entire combat object has already been rejected, then we don't need to process it any further.
            if (combat.Rejected) return;
            // Other checks should make this check unnecessary, but some of the checks are configurable, so let's make sure.
            if (!combat.CombatStart.HasValue || !combat.CombatEnd.HasValue || !combat.CombatDuration.HasValue) return;
            // This logic is based on timeSpanBetweenCombats, so reject combat with to short of a duration.
            if (combat.CombatDuration < timeSpanBetweenCombats * 3) return;

            // We keep these values, since they're all based on linq queries, which are affected when we start removing
            // entities.
            originalCombatDuration = combat.CombatDuration.Value;
            originalCombatStart = combat.CombatStart.Value;
            originalCombatEnd = combat.CombatEnd.Value;

            // Here we check for an account player.
            CombatEntity? accountPlayer = null;
            if (!string.IsNullOrWhiteSpace(combatLogParseSettings.MyCharacter))
                accountPlayer = combat.PlayerEntities.FirstOrDefault(en =>
                    en.OwnerInternal.Contains(combatLogParseSettings.MyCharacter));

            // Populate our entities list. It's configurable whether we use Players and/or NonPlayers.
            var combatEntities = new List<CombatEntity>();
            if (combatLogParseSettings.IsRemoveEntityOutliersPlayers) combatEntities.AddRange(combat.PlayerEntities);
            if (combatLogParseSettings.IsRemoveEntityOutliersNonPlayers)
                combatEntities.AddRange(combat.NonPlayerEntities);

            // The offset we attach to the start and end times here, are regions of time, where unrelated entities may exist.
            combatStartPlusOffset = combat.CombatStart.Value + timeSpanBetweenCombats;
            combatEndMinusOffset = combat.CombatEnd.Value - timeSpanBetweenCombats;

            // Get a list of entities which end before combatStartPlusOffset, or start after combatEndMinusOffset.
            var possibleRejectEntities = combatEntities.Where(entity =>
                    entity.EntityCombatEnd < combatStartPlusOffset || entity.EntityCombatStart > combatEndMinusOffset)
                .ToList();

            // Get a list of entities which seem to be valid.
            //var validEntities = combatEntities.Where(entity => entity.EntityCombatEnd >= combatStartPlusOffset || entity.EntityCombatStart <= combatEndMinusOffset).ToList();
            var validEntities = combatEntities.Where(entity =>
                    !(entity.EntityCombatEnd < combatStartPlusOffset ||
                      entity.EntityCombatStart > combatEndMinusOffset))
                .ToList();

            // If we found or our STO account player, we want to make sure they're removed from the following tests.
            if (accountPlayer != null)
            {
                possibleRejectEntities.Remove(accountPlayer);
                validEntities.Remove(accountPlayer);
            }

            possibleRejectEntities.ForEach(possibleRejectEntity =>
            {
                if (!possibleRejectEntity.EntityCombatStart.HasValue ||
                    !possibleRejectEntity.EntityCombatEnd.HasValue ||
                    !possibleRejectEntity.EntityCombatDuration.HasValue) return;

                // Get a unique list of targets from the reject entity.
                var rejectEntityTargets = possibleRejectEntity.CombatEventsList.Select(ent => ent.TargetInternal)
                    .Distinct().ToList();

                // Get a count of how many times a valid entity targeted our rejected entity.
                var otherEntityHasRejectEntityTargetedCount = validEntities.Sum(ent =>
                    ent.CombatEventsList.Count(ev => ev.TargetInternal.Equals(possibleRejectEntity.OwnerInternal)));

                // Get a count of how many times the rejected entity has target a valid entity.
                var rejectedHasOtherEntityTargetedCount =
                    (from ent in validEntities from ev in ent.CombatEventsList select ev.OwnerInternal).Distinct()
                    .Count(ownerId => rejectEntityTargets.Contains(ownerId));

                // If there's no interaction between the reject entity, and what we think are valid entities, then
                // we assume the reject entity is an unrelated entity that was merged into the current combat instance.
                if (otherEntityHasRejectEntityTargetedCount > 0 || rejectedHasOtherEntityTargetedCount > 0) return;

                // In theory, if we get this far, it's been decided the reject entity is unrelated to the combat instance.
                possibleRejectEntity.Rejected = true;
                possibleRejectEntity.RejectionReason = "Remove Unrelated Entity";
                if (combatLogParseSettings.IsDisplayRejectedParserItemsInUi) combat.RejectedCombatEntities.Add(new RejectedCombatEntity(possibleRejectEntity, originalCombatStart, originalCombatEnd, originalCombatDuration));

                if (possibleRejectEntity.IsPlayer)
                {
                    entityTypeLabel = "Player";
                    combat.PlayerEntities.Remove(possibleRejectEntity);
                }
                else
                {
                    entityTypeLabel = "NonPlayer";
                    combat.NonPlayerEntities.Remove(possibleRejectEntity);
                }

                var rejection =
                    new StringBuilder($"Rejected {entityTypeLabel}: \"{possibleRejectEntity.OwnerInternal}\" from ");
                rejection.Append(
                    $"Combat: Duration={originalCombatDuration}, Start={originalCombatStart.ToString("HH:mm:ss.fff")}, End={originalCombatEnd.ToString("HH:mm:ss.fff")}. ");
                rejection.Append(
                    $"Entity: Duration={possibleRejectEntity.EntityCombatDuration}, Start={possibleRejectEntity.EntityCombatStart?.ToString("HH:mm:ss.fff")}, End={possibleRejectEntity.EntityCombatEnd?.ToString("HH:mm:ss.fff")}");

                possibleRejectEntity.RejectionDetails = rejection.ToString();
                Log.Debug(possibleRejectEntity.RejectionDetails);
                resultFinal.AddMessage(possibleRejectEntity.RejectionDetails);
                resultFinal.AddRejectedObject(possibleRejectEntity);
            });
        });

        return resultFinal;
    }

    /// <summary>
    ///     Make an effort to determine a map for each of our combat instances.
    /// </summary>
    /// <param name="combatLogParseSettings">The parser settings</param>
    /// <param name="combatList">The combat list</param>
    /// <returns>
    ///     <see cref="CombatLogParserResult" />
    /// </returns>
    private static CombatLogParserResult TryDetermineMapsForCombatList(CombatLogParseSettings combatLogParseSettings,
        List<Combat> combatList)
    {
        var resultFinal = new CombatLogParserResult();

        if (combatLogParseSettings.MapDetectionSettings == null)
        {
            Log.Warn("Map Detection settings are null");
            resultFinal.AddMessage("Map Detection settings are null. Skipping map detection.", ResultLevel.Warning);
            return resultFinal;
        }

        // Go through each Combat object and determine its map
        foreach (var combat in combatList)
            try
            {
                combatLogParseSettings.MapDetectionSettings?.ResetCombatMatchCountForAllMaps();

                var combatMapFound = DetermineMapFromCombatMapDetectionSettings(combatLogParseSettings, combat);
                if (combatMapFound != null) combat.Map = combatMapFound.Name;
            }
            catch (Exception e)
            {
                var errorString = $"Error trying to determine map for combat starting at {combat.CombatStart}";
                Log.Error(errorString, e);
                resultFinal.AddMessage(errorString, ResultLevel.Error);
            }

        return resultFinal;
    }

    /// <summary>
    ///     This is called during log file parsing, to group <see cref="CombatEvent" /> objects into our <see cref="Combat" />
    ///     hierarchy.
    ///     <para>This method assumes the combat event list is sorted by timestamp.</para>
    /// </summary>
    /// <param name="combatLogParseSettings">The parser settings</param>
    /// <param name="combatEventListOrderedByTimestamp">A list of combat events, used to construct our combat hierarchy.</param>
    /// <param name="combatListResult">The combat list</param>
    /// <returns>
    ///     <see cref="CombatLogParserResult" />
    /// </returns>
    private static CombatLogParserResult TryGetCombatListFromCombatEvents(CombatLogParseSettings combatLogParseSettings,
        List<CombatEvent>? combatEventListOrderedByTimestamp, out List<Combat> combatListResult)
    {
        var finalResult = new CombatLogParserResult();
        combatListResult = new List<Combat>();

        if (combatEventListOrderedByTimestamp == null || combatEventListOrderedByTimestamp.Count == 0)
            return finalResult.AddMessage("There's no combat events available", ResultLevel.Halt);

        var combatList = new List<Combat>();

        combatEventListOrderedByTimestamp.ForEach(combatEvent =>
        {
            var latestCombat = combatList.LastOrDefault();
            // Take our first combat event, and use it to create our first combat instance.
            if (latestCombat == null)
            {
                latestCombat = new Combat(combatEvent, combatLogParseSettings);
                combatList.Add(latestCombat);
            }
            // We check our current combat event, with the last entry in our current combat instance. If they're more than
            // 90 seconds apart, we use the new combat event to create a new combat instance.
            else if (combatEvent.Timestamp - latestCombat.CombatEnd >
                     TimeSpan.FromSeconds(combatLogParseSettings.HowLongBeforeNewCombatInSeconds))
            {
                latestCombat = new Combat(combatEvent, combatLogParseSettings);
                combatList.Add(latestCombat);
            }
            // Go ahead and insert our new combat event into our current combat instance.
            else
            {
                latestCombat.AddCombatEvent(combatEvent, combatLogParseSettings);
            }
        });

        combatListResult = combatList;

        return finalResult;
    }

    /// <summary>
    ///     Get a list of STO combat log files.
    /// </summary>
    /// <param name="combatLogParseSettings">The parser settings</param>
    /// <param name="parseStartTimeUtc">The time the parser was started</param>
    /// <param name="filesToParse">A list of sto log files to parse</param>
    /// <returns>
    ///     <see cref="CombatLogParserResult" />
    /// </returns>
    private static CombatLogParserResult TryGetCombatLogFileListFromStoCombatLogFolder(
        CombatLogParseSettings combatLogParseSettings,
        DateTime parseStartTimeUtc, out List<FileInfo> filesToParse)
    {
        var resultFinal = new CombatLogParserResult();

        var howFarBackInTime = TimeSpan.FromHours(combatLogParseSettings.HowFarBackForCombatInHours);
        filesToParse = new List<FileInfo>();

        // The combat log base folder from settings.
        var combatLogPath = combatLogParseSettings.CombatLogPath;
        // A search pattern to select log files in the base folder.
        // This uses standard wildcard conventions, so multiple files can be selected.
        var combatLogFilePattern = combatLogParseSettings.CombatLogPathFilePattern;

        try
        {
            // If our configured log folder isn't found, alert the user to do something about it.
            if (!Directory.Exists(combatLogPath))
            {
                var errorString =
                    $"The selected log folder doesn't exist: {combatLogPath}. Go to settings and set \"CombatLogPath\" to a valid folder.";
                Log.Error(errorString);
                return resultFinal.AddMessage(errorString, ResultLevel.Halt);
            }

            // Get a list of 1 or more files, if any exist.
            filesToParse = Directory.GetFiles(combatLogPath, combatLogFilePattern, SearchOption.TopDirectoryOnly)
                .ToList().Select(file => new FileInfo(file)).OrderBy(fileInfo => fileInfo.LastWriteTimeUtc).ToList();

            if (filesToParse.Count == 0)
            {
                var errorString =
                    "No combat log files we're found in the selected folder. Go to settings and set \"CombatLogPath\" to a valid folder, and check \"CombatLogPathFilePattern\".";
                Log.Error(errorString);
                return resultFinal.AddMessage(errorString, ResultLevel.Halt);
            }

            // Filter out files based on LastWriteTime and howFarBack
            if (howFarBackInTime > TimeSpan.Zero)
                filesToParse = filesToParse.Where(fileInfo =>
                        parseStartTimeUtc - fileInfo.LastWriteTimeUtc <= howFarBackInTime)
                    .ToList();

            if (filesToParse.Count == 0)
            {
                var errorString =
                    "Combat log(s) were found, but they're too old. They fell outside the timespan defined by the \"HowFarBackForCombat\" setting.";
                Log.Error(errorString);
                return resultFinal.AddMessage(errorString, ResultLevel.Halt);
            }
        }
        catch (Exception ex)
        {
            var errorString =
                $"Failed to get log files using pattern=\"{combatLogPath}\" and path=\"{combatLogFilePattern}\"";
            Log.Error(errorString, ex);
            return resultFinal.AddMessage(errorString, ResultLevel.Halt);
        }

        return resultFinal;
    }

    #endregion

    #region Get events list helpers

    private static CombatLogParserResult TryGetCombatEventsListFromStoCombatFolder(
        CombatLogParseSettings combatLogParseSettings, DateTime parseStartTimeUtc,
        out List<CombatEvent> combatEventListResults)
    {
        var resultFinal = new CombatLogParserResult();
        combatEventListResults = new List<CombatEvent>();

        if (string.IsNullOrWhiteSpace(combatLogParseSettings.CombatLogPath))
        {
            resultFinal.AddMessage($"Argument is null {nameof(combatLogParseSettings.CombatLogPath)}",
                ResultLevel.Halt);
            throw new ArgumentNullException(nameof(combatLogParseSettings.CombatLogPath));
        }

        if (string.IsNullOrWhiteSpace(combatLogParseSettings.CombatLogPathFilePattern))
        {
            resultFinal.AddMessage($"Argument is null {nameof(combatLogParseSettings.CombatLogPathFilePattern)}",
                ResultLevel.Halt);
            throw new ArgumentNullException(nameof(combatLogParseSettings.CombatLogPathFilePattern));
        }

        var howFarBackInTime = TimeSpan.Zero;

        var fileListResult =
            TryGetCombatLogFileListFromStoCombatLogFolder(combatLogParseSettings, parseStartTimeUtc,
                out var fileInfoList);
        resultFinal.MergeResult(fileListResult);
        if (resultFinal.MaxLevel >= ResultLevel.Halt)
            return resultFinal;

        foreach (var fileInfoEntry in fileInfoList)
            try
            {
                // Since we can read in multiple log files, let's filter out the ones which are too old.
                using (var sr = File.OpenText(fileInfoEntry.FullName))
                {
                    resultFinal.AddMessage($"Parsing log: {Path.GetFileName(fileInfoEntry.FullName)}",
                        ResultLevel.Debug);

                    var fileLineCounter = 0;

                    while (sr.ReadLine() is { } fileLine)
                    {
                        fileLineCounter++;
                        try
                        {
                            var combatEvent = new CombatEvent(Path.GetFileName(fileInfoEntry.FullName), fileLine,
                                fileLineCounter);

                            if (howFarBackInTime > TimeSpan.Zero)
                                // Filter out combat events which are too old.
                                if (parseStartTimeUtc.ToLocalTime() - combatEvent.Timestamp > howFarBackInTime)
                                    continue;

                            resultFinal.AddParseResult(fileInfoEntry.FullName);
                            combatEventListResults.Add(combatEvent);
                        }
                        catch (Exception ex)
                        {
                            var errorString =
                                $"Failed to parse log file=\"{fileInfoEntry.FullName}\", at line={fileLineCounter}. File line string={fileLine}";
                            Log.Error(errorString, ex);
                            resultFinal.AddMessage(errorString, ResultLevel.Error);
                            resultFinal.AddParseResult(fileInfoEntry.FullName, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorString = $"Failed while parsing log file=\"{fileInfoEntry.FullName}\"";
                Log.Error(errorString, ex);
                resultFinal.AddMessage(errorString, ResultLevel.Error);
            }

        if (combatEventListResults.Count == 0)
        {
            var errorString =
                "No combat data was returned. Combat log data was found, but it fell outside the timespan defined by the \"HowFarBackForCombat\" setting.";
            Log.Error(errorString);
            return resultFinal.AddMessage(errorString, ResultLevel.Halt);
        }

        combatEventListResults = combatEventListResults.OrderBy(ev => ev.Timestamp).ToList();

        return resultFinal;
    }

    private static CombatLogParserResult TryGetCombatEventsListFromFileList(List<string> filesToParse,
        out List<CombatEvent> combatEventListResults)
    {
        var resultFinal = new CombatLogParserResult();
        combatEventListResults = new List<CombatEvent>();

        var fileInfoList = filesToParse.Select(file => new FileInfo(file)).ToList();

        foreach (var fileInfoEntry in fileInfoList)
            try
            {
                // Since we can read in multiple log files, let's filter out the ones which are too old.
                using (var sr = File.OpenText(fileInfoEntry.FullName))
                {
                    resultFinal.AddMessage($"Parsing log: {Path.GetFileName(fileInfoEntry.FullName)}",
                        ResultLevel.Debug);

                    var fileLineCounter = 0;

                    while (sr.ReadLine() is { } fileLine)
                    {
                        fileLineCounter++;
                        try
                        {
                            var combatEvent = new CombatEvent(Path.GetFileName(fileInfoEntry.FullName), fileLine,
                                fileLineCounter);

                            resultFinal.AddParseResult(fileInfoEntry.FullName);
                            combatEventListResults.Add(combatEvent);
                        }
                        catch (Exception ex)
                        {
                            var errorString =
                                $"Failed to parse log file=\"{fileInfoEntry.FullName}\", at line={fileLineCounter}. File line string={fileLine}";
                            Log.Error(errorString, ex);
                            resultFinal.AddMessage(errorString, ResultLevel.Error);
                            resultFinal.AddParseResult(fileInfoEntry.FullName, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorString = $"Failed while parsing log file=\"{fileInfoEntry.FullName}\"";
                Log.Error(errorString, ex);
                resultFinal.AddMessage(errorString, ResultLevel.Error);
            }

        if (combatEventListResults.Count == 0)
        {
            var errorString =
                "No combat data was returned.";
            Log.Error(errorString);
            return resultFinal.AddMessage(errorString, ResultLevel.Halt);
        }

        combatEventListResults = combatEventListResults.OrderBy(ev => ev.Timestamp).ToList();

        return resultFinal;
    }

    private static CombatLogParserResult TryGetCombatEventsListFromCombatJsonFileList(List<string> filesToParse,
        out List<CombatEvent> combatEventListResults)
    {
        var resultFinal = new CombatLogParserResult();
        combatEventListResults = new List<CombatEvent>();

        var fileInfoList = filesToParse.Select(file => new FileInfo(file)).ToList();

        foreach (var fileInfoEntry in fileInfoList)
            try
            {
                // Since we can read in multiple log files, let's filter out the ones which are too old.
                using (var sr = File.OpenText(fileInfoEntry.FullName))
                {
                    resultFinal.AddMessage($"Parsing Combat JSON: {Path.GetFileName(fileInfoEntry.FullName)}",
                        ResultLevel.Debug);

                    var combat = SerializationHelper.Deserialize<Combat>(sr.ReadToEnd());
                    if (combat == null)
                        throw new NullReferenceException("Combat came back null during deserialization");

                    var fileLineCounter = 0;

                    foreach (var eventFromJson in combat.AllCombatEvents.OrderBy(ev => ev.Timestamp))
                    {
                        fileLineCounter++;
                        try
                        {
                            var combatEvent = new CombatEvent(Path.GetFileName(fileInfoEntry.FullName),
                                eventFromJson.OriginalFileLineString, fileLineCounter);
                            resultFinal.AddParseResult(fileInfoEntry.FullName);
                            combatEventListResults.Add(combatEvent);
                        }
                        catch (Exception ex)
                        {
                            var errorString =
                                $"Failed to parse Combat JSON=\"{fileInfoEntry.FullName}\", originalfileline={eventFromJson.OriginalFileLineNumber}, originalfilelinestring={eventFromJson.OriginalFileLineString}";
                            Log.Error(errorString, ex);
                            resultFinal.AddMessage(errorString, ResultLevel.Error);
                            resultFinal.AddParseResult(fileInfoEntry.FullName, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorString = $"Failed while parsing Combat JSON=\"{fileInfoEntry.FullName}\"";
                Log.Error(errorString, ex);
                resultFinal.AddMessage(errorString, ResultLevel.Error);
            }

        if (combatEventListResults.Count == 0)
        {
            var errorString = "No combat data was returned.";
            Log.Error(errorString);
            return resultFinal.AddMessage(errorString, ResultLevel.Halt);
        }

        return resultFinal;
    }

    #endregion
}