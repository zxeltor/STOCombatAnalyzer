// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using log4net;
using zxeltor.StoCombatAnalyzer.Lib.Model;
using zxeltor.StoCombatAnalyzer.Lib.Model.CombatLog;
using zxeltor.StoCombatAnalyzer.Lib.Model.CombatMap;
using zxeltor.Types.Lib.Result;

namespace zxeltor.StoCombatAnalyzer.Lib.Helpers;

public static class CombatLogHelper
{
    #region Static Fields and Constants

    private static readonly ILog Log = LogManager.GetLogger(typeof(CombatLogHelper));

    #endregion

    #region Public Members

    /// <summary>
    ///     Purge the sto combat logs folder.
    ///     <para>Note: If there's only one log found, it won't be purged.</para>
    /// </summary>
    /// <param name="filesPurged">A list of files purged.</param>
    /// <param name="errorResponse">An error to report back to the UI.</param>
    /// <returns>True if successful. False otherwise</returns>
    public static Result TryPurgeCombatLogFolder(CombatLogParseSettings combatLogParseSettings, out List<string> filesPurged)
    {
        filesPurged = [];
        var errorResponse = string.Empty;
        var finalResult = new Result();

        try
        {
            if (!Directory.Exists(combatLogParseSettings.CombatLogPath))
            {
                errorResponse =
                    "Can't purge the combat logs. The folder doesn't exist. Check CombatLogPath in settings.";
                Log.Error(
                    $"Can't purge the combat logs. The folder doesn't exist. \"{combatLogParseSettings.CombatLogPath}\"");
                return finalResult.AddLast(new Result(errorResponse, resultLevel: ResultLevel.Halt));
            }

            var combatLogInfoList = Directory
                .GetFiles(combatLogParseSettings.CombatLogPath, combatLogParseSettings.CombatLogPathFilePattern,
                    SearchOption.TopDirectoryOnly).Select(file => new FileInfo(file)).ToList();

            if (!combatLogInfoList.Any() || combatLogInfoList.Count == 1) return finalResult.AddLast(new Result("No combat logs to parse.", resultLevel: ResultLevel.Halt)); ;

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
            errorResponse = $"Can't purge the combat logs. Reason={ex.Message}";
            Log.Error("Failed to purge combat logs folder.", ex);
            finalResult.AddLast(new Result(errorResponse, ex, ResultLevel.Halt));
        }

        return finalResult;
    }

    /// <summary>
    ///     Parse a group of STO combat logs, and construct a <see cref="Combat" /> entity hierarchy.
    /// </summary>
    /// <param name="filesToParse">A list of combat logs to parse.</param>
    public static Result TryGetCombatLogEntriesFromLogFiles(CombatLogParseSettings combatLogParseSettings, out List<Combat>? combatListResult)
    {
        combatListResult = null;
        var resultFinal = new Result();
        
        if (string.IsNullOrWhiteSpace(combatLogParseSettings.CombatLogPath))
            return resultFinal.AddLast(new Result(new ArgumentNullException(nameof(combatLogParseSettings.CombatLogPath))));

        if (string.IsNullOrWhiteSpace(combatLogParseSettings.CombatLogPathFilePattern))
            return resultFinal.AddLast(new Result(new ArgumentNullException(nameof(combatLogParseSettings.CombatLogPathFilePattern))));

        var combatEventList = new LinkedList<CombatEvent>();

        // A list of objects used to track successful and unsuccessful file log parses.
        var fileParsedResults = new Dictionary<string, FileParseResults>();
        
        resultFinal.AddLast(new Result("Parsing log files.", resultLevel: ResultLevel.Debug));

        var howFarBackInTime = TimeSpan.FromHours(combatLogParseSettings.HowFarBackForCombatInHours);
        var dateTimeTest = DateTime.Now;

        List<FileInfo>? filesToParse = null;

        // If no files are provided, we attempt to get a list from our combat log folder.
        if (filesToParse == null)
        {
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

                    return resultFinal.AddLast(new Result(new Exception(errorString), resultLevel: ResultLevel.Halt));
                }

                // Get a list of 1 or more files, if any exist.
                filesToParse = Directory.GetFiles(combatLogPath, combatLogFilePattern, SearchOption.TopDirectoryOnly)
                    .ToList().Select(file => new FileInfo(file)).ToList();

                if (filesToParse.Count == 0)
                {
                    var errorString =
                        $"No combat log files we're found in the selected folder. Go to settings and set \"CombatLogPath\" to a valid folder, and check \"CombatLogPathFilePattern\".";
                    Log.Error(errorString);
                    return resultFinal.AddLast(new Result(new Exception(errorString), ResultLevel.Halt));
                }

                // Filter out files based on LastWriteTime and howFarBack
                filesToParse = filesToParse.OrderBy(fileInfo => fileInfo.LastWriteTime).Where(fileInfo =>
                        dateTimeTest - fileInfo.LastWriteTime <= howFarBackInTime)
                    .ToList();

                if (filesToParse.Count == 0)
                {
                    var errorString =
                        $"Combat log(s) were found, but they're too old. They fell outside the timespan defined by the \"HowFarBackForCombat\" setting.";
                    Log.Error(errorString);
                    return resultFinal.AddLast(new Result(new Exception(errorString), ResultLevel.Halt));
                }
            }
            catch (Exception ex)
            {
                var errorString =
                    $"Failed to get log files using pattern=\"{combatLogPath}\" and path=\"{combatLogFilePattern}\"";
                Log.Error(errorString, ex);
                return resultFinal.AddLast(new Result(new Exception(errorString), ResultLevel.Halt));
            }
        }

        //var parseResultDto = new Result();
        // Loop through each log file found in our log folder.
        filesToParse.ForEach(fileInfoEntry =>
        {
            try
            {
                // Since we can read in multiple log files, let's filter out the ones which are too old.
                using (var sr = File.OpenText(fileInfoEntry.FullName))
                {
                    //this.AddToLogAndLogSummaryInUi($"Parsing log: {Path.GetFileName(fileInfoEntry.FullName)}");
                    resultFinal.AddLast(new Result($"Parsing log: {Path.GetFileName(fileInfoEntry.FullName)}",
                        resultLevel: ResultLevel.Debug));

                    fileParsedResults.Add(fileInfoEntry.FullName, new FileParseResults(fileInfoEntry.FullName));
                    var fileLineCounter = 0;

                    while (sr.ReadLine() is { } fileLine)
                    {
                        fileLineCounter++;
                        try
                        {
                            var combatEvent = new CombatEvent(Path.GetFileName(fileInfoEntry.FullName), fileLine,
                                fileLineCounter);

                            // Filter out combat events which are too old.
                            if (dateTimeTest - combatEvent.Timestamp > howFarBackInTime) continue;

                            fileParsedResults[fileInfoEntry.FullName].SuccessfulParses += 1;
                            combatEventList.AddLast(combatEvent);
                        }
                        catch (Exception ex)
                        {
                            fileParsedResults[fileInfoEntry.FullName].FailedParses += 1;
                            var errorString =
                                $"Failed to parse log file=\"{fileInfoEntry.FullName}\", at line={fileLineCounter}. File line string={fileLine}";
                            resultFinal.AddLast(new Result(errorString, ex, ResultLevel.Error));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorString = $"Failed while parsing log file=\"{fileInfoEntry.FullName}\"";
                Log.Error(errorString, ex);
                resultFinal.AddLast(new Result( errorString, ex, ResultLevel.Error));
            }
        });

        // This should never be true, but let's play it safe.
        if (combatEventList.Count == 0)
        {
            var errorString =
                $"No combat data was returned. Combat log data was found, but it fell outside the timespan defined by the \"HowFarBackForCombat\" setting.";
            Log.Error(errorString);

            return resultFinal.AddLast(new Result(errorString, resultLevel: ResultLevel.Halt));
        }

        try
        {
            // Construct our Combat hierarchy
            resultFinal.AddLast(TryAddCombatEvents(combatEventList.OrderBy(res => res.Timestamp).ToList(), combatLogParseSettings, out combatListResult));

            var completionMessage =
                $"Successfully parsed \"{combatEventList.Count}\" events from \"{fileParsedResults.Keys.Count}\" files for the past \"{howFarBackInTime.TotalHours}\" hours.";

            Log.Debug(completionMessage);

            return resultFinal.AddLast(new Result(completionMessage));
        }
        catch (Exception ex)
        {
            var errorString = "Failed to parse combat logs.";
            Log.Error(errorString, ex);
            return resultFinal.AddLast(new Result(errorString, ex, ResultLevel.Halt));
        }
    }

    #endregion

    #region Other Members

    /// <summary>
    ///     This is called during log file parsing, to group <see cref="CombatEvent" /> objects into our <see cref="Combat" />
    ///     hierarchy.
    ///     <para>This method assumes the combat event list is sorted by timestamp.</para>
    /// </summary>
    /// <param name="combatEventList">
    ///     A list of <see cref="CombatEvent" /> used to construct our <see cref="Combat" />
    ///     hieracrchy.
    /// </param>
    private static Result TryAddCombatEvents(List<CombatEvent>? combatEventList,
        CombatLogParseSettings combatLogParseSettings, out List<Combat>? combatListResult)
    {
        combatListResult = null;

        if (combatEventList == null || combatEventList.Count == 0) return new Result("There's no combat events available.", resultLevel: ResultLevel.Halt);

        var combatList = new List<Combat>();

        combatEventList.ForEach(combatEvent =>
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

        combatList.ForEach(combat => combat.LockObject());

        if (combatLogParseSettings.MapDetectionSettings != null)
            DetermineMapsForCombatList(combatLogParseSettings.MapDetectionSettings, combatList);

        combatListResult = combatList;

        return new Result();
    }

    /// <summary>
    ///     Make an effort to determine a map for each of our combat instances.
    /// </summary>
    private static void DetermineMapsForCombatList(CombatMapDetectionSettings mapDetectionSettings,
        List<Combat> combatList)
    {
        // Go through each Combat object and determine its map
        combatList.ForEach(combat =>
        {
            mapDetectionSettings.ResetCombatMatchCountForAllMaps();

            var combatMapFound = DetermineMapFromCombatMapDetectionSettings(mapDetectionSettings, combat);
            if (combatMapFound != null) combat.Map = combatMapFound.Name;
        });
    }

    /// <summary>
    ///     Find a map for the provided combat entity from our MapDetectionSettings object.
    /// </summary>
    /// <param name="combat">The provided combat entity</param>
    /// <returns>A combat map</returns>
    private static CombatMap? DetermineMapFromCombatMapDetectionSettings(
        CombatMapDetectionSettings mapDetectionSettings, Combat combat)
    {
        CombatMap? combatMap = null;

        var currentCombatPlayerCount = combat.PlayerEntities.Count;

        var filteredMapList = mapDetectionSettings.CombatMapEntityList
            .Where(map => map.MaxPlayers == 0 || currentCombatPlayerCount <= map.MaxPlayers).ToList();

        var currentCombatUniqueIds = combat.UniqueEntityIds.Where(id => !string.IsNullOrWhiteSpace(id.Id))
            .Select(id => id.Id)
            .Union(combat.UniqueEntityIds.Where(id => !string.IsNullOrWhiteSpace(id.Label)).Select(id => id.Label))
            .Distinct().ToList();

        var idsFoundInMapList = false;
        var idsFoundInGenericList = false;

        // Find a match in our CombatMapDetectionSettings object based on our entity ids.
        foreach (var currentEntity in currentCombatUniqueIds)
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

    #endregion
}