// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

namespace zxeltor.Types.Lib.Result;

public class ResultDetail
{
    #region Constructors

    /// <summary>
    ///     The default constructor. An empty result.
    /// </summary>
    public ResultDetail()
    {
    }

    /// <summary>
    ///     Used to create a new result detail object.
    ///     <para>This defaults to a <see cref="ResultLevel"/> of Info</para>
    /// </summary>
    /// <param name="message">A message string for the result detail.</param>
    /// <param name="exception">An exception for the result detail.</param>
    /// <param name="resultLevel">Set the result level for the result detail. This defaults to Info.</param>
    public ResultDetail(string message, Exception? exception = null, ResultLevel resultLevel = ResultLevel.Info)
    {
        this.TimeStampUtc = DateTime.UtcNow;
        this.ResultLevel = resultLevel;
        this.Exception = exception;
        this.Message = message;
    }

    /// <summary>
    ///     Used to create a new result detail object.
    ///     <para>This defaults to a <see cref="ResultLevel"/> of Fatal</para>
    /// </summary>
    /// <param name="exception">An exception for the result detail.</param>
    /// <param name="resultLevel">Set the result level for the result detail. This defaults to Halt.</param>
    public ResultDetail(Exception exception, ResultLevel resultLevel = ResultLevel.Halt)
    {
        this.TimeStampUtc = DateTime.UtcNow;
        this.ResultLevel = resultLevel;
        this.Exception = exception;
        this.Message = exception.Message;
    }

    #endregion

    #region Public Properties
    /// <summary>
    ///     A message string provided with the result.
    /// </summary>
    public string? Message { get; set; }
    /// <summary>
    ///     The result level for this result.
    /// </summary>
    public ResultLevel ResultLevel { get; set; }
    /// <summary>
    ///     The exception for this result.
    /// </summary>
    public Exception? Exception { get; set; }
    /// <summary>
    ///     The datetime of when this result was created.
    /// </summary>
    public DateTime TimeStampUtc { get; set; }

    #endregion

    #region Public Members

    #region Overrides of Object

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{this.ResultLevel}, Message={this.Message}";
    }

    #endregion

    #endregion
}