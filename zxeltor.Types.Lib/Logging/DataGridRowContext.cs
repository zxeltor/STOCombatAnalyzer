// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using zxeltor.Types.Lib.Result;

namespace zxeltor.Types.Lib.Logging;

public class DataGridRowContext
{
    #region Constructors

    public DataGridRowContext(string message, ResultLevel resultLevel = ResultLevel.Info)
    {
        this.Timestamp = DateTime.Now;
        this.ResultLevel = resultLevel;
        this.Message = message;
    }

    public DataGridRowContext(string message, Exception exception, ResultLevel resultLevel = ResultLevel.Error)
    {
        this.Timestamp = DateTime.Now;
        this.ResultLevel = resultLevel;
        this.Message = message;
        this.Exception = exception;
    }

    public DataGridRowContext(Exception exception, ResultLevel resultLevel = ResultLevel.Error)
    {
        this.Timestamp = DateTime.Now;
        this.ResultLevel = resultLevel;
        this.Message = exception.Message;
        this.Exception = exception;
    }

    #endregion

    #region Public Properties

    public DateTime Timestamp { get; set; }
    public Exception? Exception { get; set; }
    public ResultLevel ResultLevel { get; set; }
    public string Message { get; set; }

    #endregion

    #region Public Members

    #region Overrides of Object

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Time={this.Timestamp:HH:mm:ss.fff}, Result={this.ResultLevel}, Message={this.Message}";
    }

    #endregion

    #endregion
}