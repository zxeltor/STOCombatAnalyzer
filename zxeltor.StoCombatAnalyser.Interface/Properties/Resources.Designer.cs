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
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("zxeltor.StoCombatAnalyzer.Interface.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to STO Combat Log Analyzer.
        /// </summary>
        public static string ApplicationName {
            get {
                return ResourceManager.GetString("ApplicationName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This is used to examine a selected Combat from the Combat List dropdown. When a Player or NonPlayer entity is selected here, the rest of the controls in the UI update with information for the selected entity..
        /// </summary>
        public static string combat_details {
            get {
                return ResourceManager.GetString("combat_details", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This is a breakdown of event types for the selected combat entity. If you click on a particular bar, the DataGrid and Magnitude Plot will update with more specific data for the selected event type..
        /// </summary>
        public static string combat_event_type_breakdown {
            get {
                return ResourceManager.GetString("combat_event_type_breakdown", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This displays the raw STO combat log data for the selected event type. By default, not all data columns from the combat log are displayed. You can enable/disable the columns in the display by right clicking on the datagrid and interacting with the context menu that’s displayed.
        ///
        ///Note: Keep in mind this shows the raw data. ABS(Magnitude) is used to calculate the DPS and other damage values displayed in the UI..
        /// </summary>
        public static string combat_events_datagrid {
            get {
                return ResourceManager.GetString("combat_events_datagrid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This is a scatter plot of the Magnitude data vs. time for the selected event type. You can use the control to switch between the various groupings of event types. You can also display the MagnitudeBase data on the grid if selected.
        ///
        ///Note: Keep in mind this shows the raw data. ABS(Magnitude) is used to calculate the DPS and other damage values displayed in the UI..
        /// </summary>
        public static string combat_events_plot {
            get {
                return ResourceManager.GetString("combat_events_plot", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Used to export the currently selected Combat entity to a JSON file. The JSON is primarily used for troubleshooting purposes.
        ///
        ///Note: A Combat entity needs to be selected in the &quot;Log File Analyzer&quot; tab for this to work..
        /// </summary>
        public static string export_combat_json {
            get {
                return ResourceManager.GetString("export_combat_json", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Export map detection settings to a JSON file..
        /// </summary>
        public static string export_detection_json {
            get {
                return ResourceManager.GetString("export_detection_json", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://github.com/zxeltor/STOCombatAnalyzer.Settings.
        /// </summary>
        public static string GithubMapDetectRepoUrl {
            get {
                return ResourceManager.GetString("GithubMapDetectRepoUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://github.com/zxeltor/STOCombatAnalyzer.
        /// </summary>
        public static string GithubRepoUrl {
            get {
                return ResourceManager.GetString("GithubRepoUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://github.com/zxeltor/STOCombatAnalyzer/wiki.
        /// </summary>
        public static string GithubRepoWikiUrl {
            get {
                return ResourceManager.GetString("GithubRepoWikiUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Import a new or modified CombatMapDetectionSettings.json file. These settings affect how a map is determined for a Combat entity.
        ///
        ///Note: The new settings won&apos;t be reflected in the interface until you run &quot;Parse Log(s)&quot;..
        /// </summary>
        public static string import_detection_json {
            get {
                return ResourceManager.GetString("import_detection_json", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Reset map detection settings to application default.
        ///
        ///Note: The new settings won&apos;t be reflected in the interface until you run &quot;Parse Log(s)&quot;..
        /// </summary>
        public static string reset_detection_json {
            get {
                return ResourceManager.GetString("reset_detection_json", resourceCulture);
            }
        }
    }
}
