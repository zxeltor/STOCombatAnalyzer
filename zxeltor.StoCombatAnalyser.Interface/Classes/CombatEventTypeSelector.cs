// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace zxeltor.StoCombatAnalyzer.Interface.Classes
{
    public class CombatEventTypeSelector : INotifyPropertyChanged, IEquatable<CombatEventTypeSelector>
    {
        private string _eventInternal;
        private string _eventDisplay;
        private string _label;
        public Guid InstanceID { get; private set; }

        public string EventInternal { get => this._eventInternal; set => SetField(ref this._eventInternal, value); }
        public string EventDisplay { get => this._eventDisplay; set => SetField(ref this._eventDisplay, value); }
        public string Label { get => this._label; set => SetField(ref this._label, value); }

        public CombatEventTypeSelector(string _eventInternal, string? _eventDisplay = null, string? _label = null)
        {
            this.InstanceID = Guid.NewGuid();
            this.EventInternal = _eventInternal;
            this.EventDisplay = _eventDisplay ?? _eventInternal;
            this.Label = _label ?? _eventDisplay ?? _eventInternal;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

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

        public override string ToString()
        {
            return $"Id={InstanceID}, Internal={EventInternal}, Display={EventDisplay}";
        }

        public bool Equals(CombatEventTypeSelector? other)
        {
            if (other == null) return false;
            return this.EventInternal.Equals(other.EventInternal);
        }
    }
}
