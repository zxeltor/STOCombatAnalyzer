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
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("20")]
        public int HowLongBeforeNewCombat {
            get {
                return ((int)(this["HowLongBeforeNewCombat"]));
            }
            set {
                this["HowLongBeforeNewCombat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool PurgeCombatLogs {
            get {
                return ((bool)(this["PurgeCombatLogs"]));
            }
            set {
                this["PurgeCombatLogs"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("7")]
        public long HowLongToKeepLogs {
            get {
                return ((long)(this["HowLongToKeepLogs"]));
            }
            set {
                this["HowLongToKeepLogs"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool DebugLogging {
            get {
                return ((bool)(this["DebugLogging"]));
            }
            set {
                this["DebugLogging"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string MyCharacter {
            get {
                return ((string)(this["MyCharacter"]));
            }
            set {
                this["MyCharacter"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("{\"JsonVersion\":\"1.3.3\",\"Comments\":[\"JsonVersion: Uses Semantic Versioning 2.0.0 (" +
            "Major,Minor,Patch)\",\"GenericGroundMap: When a map isn\'t detected when checking C" +
            "ombatMapEntityList, we use this to determine if the Combat entity is on a ground" +
            " based map.\",\"GenericSpaceMap: When a map isn\'t detected when checking CombatMap" +
            "EntityList, we use this to determine if the Combat entity is on a space based ma" +
            "p.\",\"EntityExclusionList: This is used to filter out game entity ids from the ma" +
            "p detect process.\",\"CombatMapEntityList: The main list of map definitions. This " +
            "collection is used first when trying to detect a map for a Combat entity.\"],\"Ent" +
            "ityExclusionList\":[{\"Pattern\":\"Space_Nimbus_Pirate_Distress\",\"IsUniqueToMap\":fal" +
            "se}],\"CombatMapEntityList\":[{\"Name\":\"Infected Space\",\"MapEntities\":[{\"Pattern\":\"" +
            "Space_Borg_Dreadnought_Raidisode_Sibrian_Final_Boss\",\"IsUniqueToMap\":true}],\"Map" +
            "EntityExclusions\":[]},{\"Name\":\"Azure Nebula Rescue\",\"MapEntities\":[{\"Pattern\":\"M" +
            "ission_Space_Romulan_Colony_Flagship_Lleiset\",\"IsUniqueToMap\":true}],\"MapEntityE" +
            "xclusions\":[]},{\"Name\":\"Battle At The Binary Stars\",\"MapEntities\":[{\"Pattern\":\"S" +
            "pace_Klingon_Dreadnought_Dsc_Sarcophagus\",\"IsUniqueToMap\":true}],\"MapEntityExclu" +
            "sions\":[]},{\"Name\":\"Battle At Procyon V\",\"MapEntities\":[{\"Pattern\":\"Event_Procyo" +
            "n_5_Queue_Krenim_Dreadnaught_Annorax\",\"IsUniqueToMap\":true}],\"MapEntityExclusion" +
            "s\":[]},{\"Name\":\"Borg Disconnected\",\"MapEntities\":[{\"Pattern\":\"Mission_Space_Borg" +
            "_Queen_Diamond_Brg_Queue_Liberation\",\"IsUniqueToMap\":true}],\"MapEntityExclusions" +
            "\":[]},{\"Name\":\"Counterpoint\",\"MapEntities\":[{\"Pattern\":\"Mission_Starbase_Mirror_" +
            "Ds9_Mu_Queue\",\"IsUniqueToMap\":true}],\"MapEntityExclusions\":[]},{\"Name\":\"Crystall" +
            "ine Entity\",\"MapEntities\":[{\"Pattern\":\"Space_Crystalline_Entity_2018\",\"IsUniqueT" +
            "oMap\":true}],\"MapEntityExclusions\":[]},{\"Name\":\"Gateway To Grethor\",\"MapEntities" +
            "\":[{\"Pattern\":\"Event_Ico_Qonos_Space_Herald_Dreadnaught\",\"IsUniqueToMap\":true}]," +
            "\"MapEntityExclusions\":[]},{\"Name\":\"Herald Sphere\",\"MapEntities\":[{\"Pattern\":\"Mis" +
            "sion_Space_Federation_Science_Herald_Sphere\",\"IsUniqueToMap\":true}],\"MapEntityEx" +
            "clusions\":[]},{\"Name\":\"Operation Riposte\",\"MapEntities\":[{\"Pattern\":\"Msn_Dsc_Pri" +
            "ors_System_Tfo_Orbital_Platform_1_Fed_Dsc\",\"IsUniqueToMap\":true}],\"MapEntityExcl" +
            "usions\":[]},{\"Name\":\"Cure Found\",\"MapEntities\":[{\"Pattern\":\"Space_Borg_Dreadnoug" +
            "ht_R02\",\"IsUniqueToMap\":true}],\"MapEntityExclusions\":[]},{\"Name\":\"Days Of Doom\"," +
            "\"MapEntities\":[{\"Pattern\":\"Space_Klingon_Tos_X3_Battlecruiser\",\"IsUniqueToMap\":t" +
            "rue}],\"MapEntityExclusions\":[]},{\"Name\":\"Dranuur Gauntlet\",\"MapEntities\":[{\"Patt" +
            "ern\":\"Msn_Luk_Colony_Dranuur_Queue_System_Upgradeable_Satellite\",\"IsUniqueToMap\"" +
            ":true}],\"MapEntityExclusions\":[]},{\"Name\":\"Khitomer Space\",\"MapEntities\":[{\"Patt" +
            "ern\":\"Space_Borg_Dreadnought_Raidisode_Khitomer_Intro_Boss\",\"IsUniqueToMap\":true" +
            "}],\"MapEntityExclusions\":[]},{\"Name\":\"Storming The Spire\",\"MapEntities\":[{\"Patte" +
            "rn\":\"Mission_Spire_Space_Voth_Frigate\",\"IsUniqueToMap\":true}],\"MapEntityExclusio" +
            "ns\":[]},{\"Name\":\"Swarm\",\"MapEntities\":[{\"Pattern\":\"Space_Drantzuli_Alpha_Battles" +
            "hip\",\"IsUniqueToMap\":true}],\"MapEntityExclusions\":[]},{\"Name\":\"To Hell With Hono" +
            "r\",\"MapEntities\":[{\"Pattern\":\"Mission_Beta_Lankal_Destructible_Reactor\",\"IsUniqu" +
            "eToMap\":true}],\"MapEntityExclusions\":[]},{\"Name\":\"Gravity Kills\",\"MapEntities\":[" +
            "{\"Pattern\":\"Space_Federation_Dreadnought_Jupiter_Class_Carrier\",\"IsUniqueToMap\":" +
            "true},{\"Pattern\":\"Msn_Luk_Hypermass_Queue_System_Tzk_Protomatter_Facility\",\"IsUn" +
            "iqueToMap\":true}],\"MapEntityExclusions\":[]},{\"Name\":\"Hive Space\",\"MapEntities\":[" +
            "{\"Pattern\":\"Space_Borg_Dreadnought_Hive_Intro\",\"IsUniqueToMap\":true}],\"MapEntity" +
            "Exclusions\":[]},{\"Name\":\"Operation Wolf\",\"MapEntities\":[{\"Pattern\":\"Ground_Feder" +
            "ation_Capt_Mirror_Runabout_Tfo\",\"IsUniqueToMap\":true}],\"MapEntityExclusions\":[]}" +
            ",{\"Name\":\"Bug Hunt\",\"MapEntities\":[{\"Pattern\":\"Bluegills_Ground_Boss\",\"IsUniqueT" +
            "oMap\":true}],\"MapEntityExclusions\":[]},{\"Name\":\"Miner Instabilities\",\"MapEntitie" +
            "s\":[{\"Pattern\":\"Msn_Edren_Queue_Ground_Gorn_Lt_Tos_Range_Rock\",\"IsUniqueToMap\":t" +
            "rue}],\"MapEntityExclusions\":[]},{\"Name\":\"Jupiter Station Showdown\",\"MapEntities\"" +
            ":[{\"Pattern\":\"Msn_Ground_Capt_Mirror_Janeway_Boss_Unkillable\",\"IsUniqueToMap\":tr" +
            "ue}],\"MapEntityExclusions\":[]},{\"Name\":\"Nukara Prime: Transdimensional Tactics\"," +
            "\"MapEntities\":[{\"Pattern\":\"Mission_Event_Tholian_Invasion_Ext_Boss\",\"IsUniqueToM" +
            "ap\":true}],\"MapEntityExclusions\":[]},{\"Name\":\"Battle of Wolf 359\",\"MapEntities\":" +
            "[{\"Pattern\":\"Space_Borg_Dreadnought_Wolf359\",\"IsUniqueToMap\":true}],\"MapEntityEx" +
            "clusions\":[]},{\"Name\":\"Nimbus\",\"MapEntities\":[{\"Pattern\":\"Ground_Renegades_Lt_Go" +
            "rn_Nimbus_Sniper\",\"IsUniqueToMap\":true},{\"Pattern\":\"Ground_Gorn\",\"IsUniqueToMap\"" +
            ":false},{\"Pattern\":\"Beast_Dewan_Arthropod\",\"IsUniqueToMap\":false},{\"Pattern\":\"Ni" +
            "mbus\",\"IsUniqueToMap\":false}],\"MapEntityExclusions\":[]},{\"Name\":\"Nukara\",\"MapEnt" +
            "ities\":[{\"Pattern\":\"Ground_Tholian\",\"IsUniqueToMap\":false}],\"MapEntityExclusions" +
            "\":[]},{\"Name\":\"Defend Rh\'Ihho Station\",\"MapEntities\":[{\"Pattern\":\"Ground_Elachi\"" +
            ",\"IsUniqueToMap\":false}],\"MapEntityExclusions\":[{\"Pattern\":\"Ground_Elachi_Ensign" +
            "_Friendly_Rift\",\"IsUniqueToMap\":false}]},{\"Name\":\"Borg Battle Royale\",\"MapEntiti" +
            "es\":[{\"Pattern\":\"Range_Mirror\",\"IsUniqueToMap\":false}],\"MapEntityExclusions\":[]}" +
            "],\"GenericGroundMap\":{\"Name\":\"Generic Ground\",\"MapEntities\":[{\"Pattern\":\"Ground_" +
            "\",\"IsUniqueToMap\":false}],\"MapEntityExclusions\":[]},\"GenericSpaceMap\":{\"Name\":\"G" +
            "eneric Space\",\"MapEntities\":[{\"Pattern\":\"Space_\",\"IsUniqueToMap\":false}],\"MapEnt" +
            "ityExclusions\":[]}}")]
        public string DefaultCombatMapList {
            get {
                return ((string)(this["DefaultCombatMapList"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string UserCombatMapList {
            get {
                return ((string)(this["UserCombatMapList"]));
            }
            set {
                this["UserCombatMapList"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("48")]
        public int HowFarBackForCombat {
            get {
                return ((int)(this["HowFarBackForCombat"]));
            }
            set {
                this["HowFarBackForCombat"] = value;
            }
        }
    }
}
