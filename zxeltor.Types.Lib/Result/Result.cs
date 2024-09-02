// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace zxeltor.Types.Lib.Result;

/// <summary>
///     A wrapper for a process result set.
/// </summary>
public class Result : INotifyPropertyChanged
{
    #region Private Fields

    private ObservableCollection<ResultDetail> _details = [];
    private ResultLevel _level = ResultLevel.Debug;
    private bool _successFull = true;

    #endregion
    /// <summary>
    ///     The default constructor. An empty result, which is Successful.
    /// </summary>
    public Result()
    {
    }

    public List<string> GetMessagesByMinLevel(ResultLevel minResultLevel)
    {
        return this.Details.Where(det => det.ResultLevel >= minResultLevel && det.Message != null)
            .Select(det => det.Message).ToList();
    }

    #region Constructors
    /// <summary>
    ///     Used to create a new result object.
    ///     <para>This defaults to a <see cref="ResultLevel"/> of Info</para>
    /// </summary>
    /// <param name="message">A message string to include in the detail collection.</param>
    /// <param name="exception">An exception to include in the detail collection.</param>
    /// <param name="resultLevel">Set the result level for the detail collection. This defaults to Info.</param>
    public Result(string message, Exception? exception = null, ResultLevel? resultLevel = ResultLevel.Info)
    {
        this.SuccessFull = resultLevel < ResultLevel.Error;
        this.Level = resultLevel.Value;
        this.Details.Add(new ResultDetail(message, exception, resultLevel.Value));
    }

    /// <summary>
    ///     Used to create a new result object using only an <see cref="Exception"/> and a <see cref="ResultLevel"/>
    /// </summary>
    /// <param name="ex">An exception to include in the detail collection.</param>
    /// <param name="resultLevel">Set the result level for the detail collection. This defaults to Halt.</param>
    public Result(Exception ex, ResultLevel? resultLevel = ResultLevel.Halt)
    {
        this.SuccessFull = resultLevel < ResultLevel.Error;
        this.Level = resultLevel.Value;
        this.Details.Add(new ResultDetail(ex, resultLevel.Value));
    }

    #endregion

    #region Public Properties
    /// <summary>
    ///     A list of result messages
    /// </summary>
    public List<string>? Messages
    {
        get
        {
            var detailsWithMessages = this.Details.Where(det => det.Message != null).ToList();
            if (detailsWithMessages.Count == 0) return null;
            return detailsWithMessages.Select(det => det.Message).ToList();
        }
    }

    /// <summary>
    ///     An aggregate exception for all exceptions in our details collection.
    /// </summary>
    public AggregateException? Exceptions
    {
        get
        {
            var detailsWithExceptions = this.Details.Where(det => det.Exception != null).ToList();
            if (detailsWithExceptions.Count == 0) return null;
            return new AggregateException(
                detailsWithExceptions.Select(det => new Exception(det.Message, det.Exception)));
        }
    }

    /// <summary>
    ///     True if the 
    /// </summary>
    public bool SuccessFull
    {
        get => this._successFull;
        internal set => this.SetField(ref this._successFull, value);
            //if (!value && this._successFull) 
    }

    /// <summary>
    ///     The maximum result level for all details.
    /// </summary>
    public ResultLevel Level
    {
        get => this._level;
        internal set => this.SetField(ref this._level, this._level < value ? value : this._level);
    }

    /// <summary>
    ///     The result detail collection
    /// </summary>
    public ObservableCollection<ResultDetail> Details
    {
        get => this._details;
        set => this.SetField(ref this._details, value);
    }

    #endregion

    #region Public Members

    #region Overrides of Object

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{this.SuccessFull}, Level={this.Level}, Details={this.Details.Count}";
    }

    #endregion

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Other Members

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        this.OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}