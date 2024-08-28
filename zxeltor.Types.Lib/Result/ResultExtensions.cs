// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

namespace zxeltor.Types.Lib.Result;

public static class ResultExtensions
{
    #region Public Members
    /// <summary>
    ///     Used to merge the supplied Result into the parent.
    /// </summary>
    /// <param name="thisDto">The parent Result</param>
    /// <param name="dtoToAppend">The Result to merge into the current</param>
    /// <returns>The parent Result with the merged data.</returns>
    public static Result AddLast(this Result thisDto, Result dtoToAppend)
    {
        thisDto.SuccessFull = dtoToAppend.SuccessFull;
        thisDto.Level = dtoToAppend.Level;
        dtoToAppend.Details.ToList().ForEach(resultDetail => thisDto.Details.Add(resultDetail));
        return thisDto;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="thisDto">The parent Result</param>
    /// <param name="message">A message string to include in the detail collection.</param>
    /// <param name="exception">An exception to include in the detail collection.</param>
    /// <param name="resultLevel">Set the result level for the detail collection. This defaults to Info.</param>
    /// <returns>he parent Result with the merged data.</returns>
    public static Result AddLast(this Result thisDto, string message, Exception? exception = null, ResultLevel resultLevel = ResultLevel.Info)
    {
        var newResult = new Result(message, exception, resultLevel);
        return thisDto.AddLast(newResult);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="thisDto">The parent Result</param>
    /// <param name="exception">An exception to include in the detail collection.</param>
    /// <param name="resultLevel">Set the result level for the detail collection. This defaults to Halt.</param>
    /// <returns>he parent Result with the merged data.</returns>
    public static Result AddLast(this Result thisDto, Exception exception, ResultLevel resultLevel = ResultLevel.Halt)
    {
        var newResult = new Result(exception, resultLevel);
        return thisDto.AddLast(newResult);
    }

    #endregion
}