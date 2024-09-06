// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using zxeltor.Types.Lib.Result;

namespace zxeltor.StoCombat.Lib.Parser;

public static class CombatLogParserResultExtensions
{
    #region Public Members
    /// <summary>
    ///     Used to merge the supplied Result into the parent.
    /// </summary>
    /// <param name="thisResult">The parent Result</param>
    /// <param name="appendResult">The Result to merge into the current</param>
    /// <returns>The parent Result with the merged data.</returns>
    public static CombatLogParserResult MergeResult(this CombatLogParserResult thisResult, CombatLogParserResult appendResult)
    {
        thisResult.SuccessFull = appendResult.SuccessFull;
        thisResult.MaxLevel = appendResult.MaxLevel;

        if(appendResult.ResultMessages.Count > 0)
            thisResult.ResultMessages.AddRange(appendResult.ResultMessages);
        
        if(appendResult.RejectedObjects.Count > 0)
            thisResult.RejectedObjects.AddRange(appendResult.RejectedObjects);

        if(appendResult.StoCombatLogFiles.Count > 0)
            thisResult.StoCombatLogFiles.AddRange(appendResult.StoCombatLogFiles);

        return thisResult;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="thisResult">The parent Result</param>
    /// <param name="message">A message string to include in the detail collection.</param>
    /// <param name="resultLevel">Set the result level for the detail collection. This defaults to Info.</param>
    /// <returns>he parent Result with the merged data.</returns>
    public static CombatLogParserResult AddMessage(this CombatLogParserResult thisResult, string? message, ResultLevel resultLevel = ResultLevel.Info)
    {
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentNullException("message");
        thisResult.ResultMessages.Add(new CombatLogParserResultMessage(message, resultLevel));
        return thisResult;
    }

    #endregion
}