﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace zxeltor.StoCombatAnalyzer.Interface.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.9.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"^(?<year>\d+):(?<month>\d+):(?<day>\d+):(?<hour>\d+):(?<min>\d+):(?<sec>\d+)\.(?<milli>\d+)::(?<OwnerDisplay>[^|]*),(?<OwnerId>[^|]*),(?<SourceLabel>[^|]*),(?<SourceId>[^|]*),(?<TargetLabel>[^|]*),(?<TargetId>[^|]*),(?<EventLabel>[^|]*),(?<EventId>[^|]*),(?<Type>[^|]*),(?<Flags>[^|]*),(?<Magnitude>[^|]*),(?<MagnitudeBase>[^|]*)")]
        public string CombatLogLineRegex {
            get {
                return ((string)(this["CombatLogLineRegex"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool EnableSettingsEdit {
            get {
                return ((bool)(this["EnableSettingsEdit"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("c:\\temp")]
        public string CombatLogPath {
            get {
                return ((string)(this["CombatLogPath"]));
            }
            set {
                this["CombatLogPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("combatlog*.log")]
        public string CombatLogPathFilePattern {
            get {
                return ((string)(this["CombatLogPathFilePattern"]));
            }
            set {
                this["CombatLogPathFilePattern"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int MaxNumberOfCombatsToDisplay {
            get {
                return ((int)(this["MaxNumberOfCombatsToDisplay"]));
            }
            set {
                this["MaxNumberOfCombatsToDisplay"] = value;
            }
        }
    }
}
