// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using zxeltor.Types.Lib.Collections;
using zxeltor.Types.Lib.Result;

namespace zxeltor.StoCombat.Lib.Parser;

public class CombatLogParserResult : INotifyPropertyChanged
{
    #region Private Fields

    private ResultLevel _level = ResultLevel.Debug;
    private bool _successFull = true;

    #endregion

    #region Constructors

    #endregion

    #region Public Properties

    public bool SuccessFull
    {
        get => this._successFull;
        internal set => this.SetField(ref this._successFull, value);
    }

    public LargeObservableCollection<CombatLogParserResultMessage> ResultMessages { get; set; } = [];

    public LargeObservableCollection<IRejectAbleEntity> RejectedObjects { get; set; } = [];

    public LargeObservableCollection<StoCombatLog> StoCombatLogFiles { get; set; } = [];

    /// <summary>
    ///     The maximum result level for all details.
    /// </summary>
    public ResultLevel MaxLevel
    {
        get => this._level;
        internal set => this.SetField(ref this._level, this._level < value ? value : this._level);
    }

    #endregion

    #region Public Members

    public void AddParseResult(string stringFileName, bool wasSuccessful = true)
    {
        if (string.IsNullOrWhiteSpace(stringFileName)) throw new ArgumentNullException(nameof(stringFileName));

        var stoCombatLog = this.StoCombatLogFiles.FirstOrDefault(sto =>
            sto.StoCombatLogFileInfo.Name.Equals(Path.GetFileName(stringFileName)));

        if (stoCombatLog == null)
        {
            stoCombatLog = new StoCombatLog(stringFileName);
            this.StoCombatLogFiles.Add(stoCombatLog);
        }

        stoCombatLog.FileLineCount++;
        if (wasSuccessful) stoCombatLog.ParsedLineCount++;
        else stoCombatLog.FailedParseLineCount++;
    }

    public void AddRejectedObject(IRejectAbleEntity rejectAbleEntity)
    {
        this.RejectedObjects.Add(rejectAbleEntity);
    }

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

public class CombatLogParserResultMessage
{
    #region Constructors

    public CombatLogParserResultMessage(string message, ResultLevel resultLevel = ResultLevel.Info)
    {
        this.Message = message;
        this.TimeStamp = DateTime.Now;
        this.ResultLevel = resultLevel;
    }

    #endregion

    #region Public Properties

    public DateTime TimeStamp { get; }
    public ResultLevel ResultLevel { get; }
    public string Message { get; }

    #endregion
}

public class StoCombatLog : IEquatable<StoCombatLog>, IEquatable<string>
{
    #region Constructors

    public StoCombatLog(string stoCombatLogFilePath)
    {
        this.StoCombatLogFileInfo = new FileInfo(stoCombatLogFilePath);
    }

    #endregion

    #region Public Properties

    public FileInfo StoCombatLogFileInfo { get; }
    public int FailedParseLineCount { get; internal set; }
    public int FileLineCount { get; internal set; }
    public int ParsedLineCount { get; internal set; }

    #endregion

    #region Public Members

    #region Implementation of IEquatable<StoCombatLog>

    /// <inheritdoc />
    public bool Equals(StoCombatLog? other)
    {
        if (other == null) return false;
        if (this.StoCombatLogFileInfo.Name.Equals(other.StoCombatLogFileInfo.Name)) return true;

        return false;
    }

    #endregion

    #region Implementation of IEquatable<string>

    /// <inheritdoc />
    public bool Equals(string? other)
    {
        if (string.IsNullOrWhiteSpace(other)) return false;
        if (this.StoCombatLogFileInfo.Name.Equals(other)) return true;

        return false;
    }

    #endregion

    #endregion
}