﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace zxeltor.StoCombat.Analyzer.Properties {
    
    
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
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string UserCombatDetectionSettings {
            get {
                return ((string)(this["UserCombatDetectionSettings"]));
            }
            set {
                this["UserCombatDetectionSettings"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("{\"JsonVersion\":\"1.5\",\"JsonVersionDescription\":\"\",\"Comments\":[\"JsonVersion: Uses S" +
            "emantic Versioning 2.0.0 (Major,Minor,Patch)\",\"GenericGroundMap: When a map isn\'" +
            "t detected when checking CombatMapEntityList, we use this to determine if the Co" +
            "mbat entity is on a ground based map.\",\"GenericSpaceMap: When a map isn\'t detect" +
            "ed when checking CombatMapEntityList, we use this to determine if the Combat ent" +
            "ity is on a space based map.\",\"EntityExclusionList: This is used to filter out g" +
            "ame entity ids from the map detect process.\",\"CombatMapEntityList: The main list" +
            " of map definitions. This collection is used first when trying to detect a map f" +
            "or a Combat entity.\"],\"EntityExclusionList\":[{\"IsEnabled\":true,\"Pattern\":\"Space_" +
            "Nimbus_Pirate_Distress\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Voth" +
            " Power Subcore\",\"IsUniqueToMap\":false}],\"CombatMapEntityList\":[{\"IsEnabled\":true" +
            ",\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities\":[{\"IsEnabled\":true,\"Pattern\":\"Space" +
            "_Borg_Dreadnought_Raidisode_Sibrian_Final_Boss\",\"IsUniqueToMap\":true}],\"MapEntit" +
            "yExclusions\":[],\"Name\":\"Event: Infected Space\"},{\"IsEnabled\":true,\"MaxPlayers\":5" +
            ",\"MinPlayers\":5,\"MapEntities\":[{\"IsEnabled\":true,\"Pattern\":\"Mission_Space_Romula" +
            "n_Colony_Flagship_Lleiset\",\"IsUniqueToMap\":true}],\"MapEntityExclusions\":[],\"Name" +
            "\":\"Event: Azure Nebula Rescue\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5," +
            "\"MapEntities\":[{\"IsEnabled\":true,\"Pattern\":\"Space_Klingon_Dreadnought_Dsc_Sarcop" +
            "hagus\",\"IsUniqueToMap\":true}],\"MapEntityExclusions\":[],\"Name\":\"Event: Battle At " +
            "The Binary Stars\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities\"" +
            ":[{\"IsEnabled\":true,\"Pattern\":\"Event_Procyon_5_Queue_Krenim_Dreadnaught_Annorax\"" +
            ",\"IsUniqueToMap\":true}],\"MapEntityExclusions\":[],\"Name\":\"Event: Battle At Procyo" +
            "n V\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities\":[{\"IsEnabled" +
            "\":true,\"Pattern\":\"Mission_Space_Borg_Queen_Diamond_Brg_Queue_Liberation\",\"IsUniq" +
            "ueToMap\":true}],\"MapEntityExclusions\":[],\"Name\":\"Event: Borg Disconnected\"},{\"Is" +
            "Enabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities\":[{\"IsEnabled\":true,\"Pa" +
            "ttern\":\"Mission_Starbase_Mirror_Ds9_Mu_Queue\",\"IsUniqueToMap\":false},{\"IsEnabled" +
            "\":true,\"Pattern\":\"Empok Nor\",\"IsUniqueToMap\":true},{\"IsEnabled\":true,\"Pattern\":\"" +
            "Msn_Mu_Bz_Badlands_System_Plasma_Controller_Boss\",\"IsUniqueToMap\":false},{\"IsEna" +
            "bled\":true,\"Pattern\":\"Space_Federation_Odyssey_Saucer_Pet_Mirror\",\"IsUniqueToMap" +
            "\":false},{\"IsEnabled\":true,\"Pattern\":\"Terran Frigate\",\"IsUniqueToMap\":false},{\"I" +
            "sEnabled\":true,\"Pattern\":\"Terran Odyssey Dreadnought\",\"IsUniqueToMap\":false},{\"I" +
            "sEnabled\":true,\"Pattern\":\"Space_Federation_Frigate_Mirror\",\"IsUniqueToMap\":false" +
            "},{\"IsEnabled\":true,\"Pattern\":\"Space_Federation_Frigate_Mirror_2\",\"IsUniqueToMap" +
            "\":false},{\"IsEnabled\":true,\"Pattern\":\"Space_Federation_Dreadnought_Odessey_Mirro" +
            "r\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Space_Federation_Odyssey_" +
            "Saucer_Pet_Mirror\",\"IsUniqueToMap\":false}],\"MapEntityExclusions\":[],\"Name\":\"Even" +
            "t: Counterpoint\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities\":" +
            "[{\"IsEnabled\":true,\"Pattern\":\"Space_Crystalline_Entity_2018\",\"IsUniqueToMap\":tru" +
            "e}],\"MapEntityExclusions\":[],\"Name\":\"Event: Crystalline Entity\"},{\"IsEnabled\":tr" +
            "ue,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities\":[{\"IsEnabled\":true,\"Pattern\":\"Eve" +
            "nt_Ico_Qonos_Space_Herald_Dreadnaught\",\"IsUniqueToMap\":true}],\"MapEntityExclusio" +
            "ns\":[],\"Name\":\"Event: Gateway To Grethor\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"Min" +
            "Players\":5,\"MapEntities\":[{\"IsEnabled\":true,\"Pattern\":\"Mission_Space_Federation_" +
            "Science_Herald_Sphere\",\"IsUniqueToMap\":true}],\"MapEntityExclusions\":[],\"Name\":\"E" +
            "vent: Herald Sphere\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntiti" +
            "es\":[{\"IsEnabled\":true,\"Pattern\":\"Msn_Dsc_Priors_System_Tfo_Orbital_Platform_1_F" +
            "ed_Dsc\",\"IsUniqueToMap\":true}],\"MapEntityExclusions\":[],\"Name\":\"Event: Operation" +
            " Riposte\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities\":[{\"IsEn" +
            "abled\":true,\"Pattern\":\"Space_Borg_Dreadnought_R02\",\"IsUniqueToMap\":true}],\"MapEn" +
            "tityExclusions\":[],\"Name\":\"Event: Cure Found\"},{\"IsEnabled\":true,\"MaxPlayers\":5," +
            "\"MinPlayers\":5,\"MapEntities\":[{\"IsEnabled\":true,\"Pattern\":\"Space_Klingon_Tos_X3_" +
            "Battlecruiser\",\"IsUniqueToMap\":true}],\"MapEntityExclusions\":[],\"Name\":\"Event: Da" +
            "ys Of Doom\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities\":[{\"Is" +
            "Enabled\":true,\"Pattern\":\"Msn_Luk_Colony_Dranuur_Queue_System_Upgradeable_Satelli" +
            "te\",\"IsUniqueToMap\":true}],\"MapEntityExclusions\":[],\"Name\":\"Event: Dranuur Gaunt" +
            "let\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities\":[{\"IsEnabled" +
            "\":true,\"Pattern\":\"Space_Borg_Dreadnought_Raidisode_Khitomer_Intro_Boss\",\"IsUniqu" +
            "eToMap\":true}],\"MapEntityExclusions\":[],\"Name\":\"Event: Khitomer Space\"},{\"IsEnab" +
            "led\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities\":[{\"IsEnabled\":true,\"Patter" +
            "n\":\"Mission_Spire_Space_Voth_Frigate\",\"IsUniqueToMap\":true}],\"MapEntityExclusion" +
            "s\":[],\"Name\":\"Event: Storming The Spire\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinP" +
            "layers\":5,\"MapEntities\":[{\"IsEnabled\":true,\"Pattern\":\"Space_Drantzuli_Alpha_Batt" +
            "leship\",\"IsUniqueToMap\":true}],\"MapEntityExclusions\":[],\"Name\":\"Event: Swarm\"},{" +
            "\"IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities\":[{\"IsEnabled\":true," +
            "\"Pattern\":\"Mission_Beta_Lankal_Destructible_Reactor\",\"IsUniqueToMap\":true}],\"Map" +
            "EntityExclusions\":[],\"Name\":\"Event: To Hell With Honor\"},{\"IsEnabled\":true,\"MaxP" +
            "layers\":5,\"MinPlayers\":5,\"MapEntities\":[{\"IsEnabled\":true,\"Pattern\":\"Space_Feder" +
            "ation_Dreadnought_Jupiter_Class_Carrier\",\"IsUniqueToMap\":true},{\"IsEnabled\":true" +
            ",\"Pattern\":\"Msn_Luk_Hypermass_Queue_System_Tzk_Protomatter_Facility\",\"IsUniqueTo" +
            "Map\":true}],\"MapEntityExclusions\":[],\"Name\":\"Event: Gravity Kills\"},{\"IsEnabled\"" +
            ":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities\":[{\"IsEnabled\":true,\"Pattern\":\"" +
            "Space_Borg_Dreadnought_Hive_Intro\",\"IsUniqueToMap\":true}],\"MapEntityExclusions\":" +
            "[],\"Name\":\"Event: Hive Space\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"" +
            "MapEntities\":[{\"IsEnabled\":true,\"Pattern\":\"Ground_Federation_Capt_Mirror_Runabou" +
            "t_Tfo\",\"IsUniqueToMap\":true}],\"MapEntityExclusions\":[],\"Name\":\"Event: Operation " +
            "Wolf\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities\":[{\"IsEnable" +
            "d\":true,\"Pattern\":\"Bluegills_Ground_Boss\",\"IsUniqueToMap\":true}],\"MapEntityExclu" +
            "sions\":[],\"Name\":\"Event: Bug Hunt\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayers" +
            "\":5,\"MapEntities\":[{\"IsEnabled\":true,\"Pattern\":\"Msn_Edren_Queue_Ground_Gorn_Lt_T" +
            "os_Range_Rock\",\"IsUniqueToMap\":true}],\"MapEntityExclusions\":[],\"Name\":\"Event: Mi" +
            "ner Instabilities\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities" +
            "\":[{\"IsEnabled\":true,\"Pattern\":\"Msn_Ground_Capt_Mirror_Janeway_Boss_Unkillable\"," +
            "\"IsUniqueToMap\":true}],\"MapEntityExclusions\":[],\"Name\":\"Event: Jupiter Station S" +
            "howdown\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities\":[{\"IsEna" +
            "bled\":true,\"Pattern\":\"Mission_Event_Tholian_Invasion_Ext_Boss\",\"IsUniqueToMap\":t" +
            "rue}],\"MapEntityExclusions\":[],\"Name\":\"Event: Nukara Prime: Transdimensional Tac" +
            "tics\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities\":[{\"IsEnable" +
            "d\":true,\"Pattern\":\"Space_Borg_Dreadnought_Wolf359\",\"IsUniqueToMap\":true}],\"MapEn" +
            "tityExclusions\":[],\"Name\":\"Event: Battle of Wolf 359\"},{\"IsEnabled\":true,\"MaxPla" +
            "yers\":0,\"MinPlayers\":1,\"MapEntities\":[{\"IsEnabled\":true,\"Pattern\":\"Ground_Renega" +
            "des_Lt_Gorn_Nimbus_Sniper\",\"IsUniqueToMap\":true},{\"IsEnabled\":true,\"Pattern\":\"Gr" +
            "ound_Gorn\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Beast_Dewan_Arthr" +
            "opod\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Adult Dewan Arthropod\"" +
            ",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Adult Sand Scorpion\",\"IsUni" +
            "queToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Dewan Arthropod\",\"IsUniqueToMap\":f" +
            "alse},{\"IsEnabled\":true,\"Pattern\":\"Giant Scorpion\",\"IsUniqueToMap\":false},{\"IsEn" +
            "abled\":true,\"Pattern\":\"Mature Sand Scorpion\",\"IsUniqueToMap\":false},{\"IsEnabled\"" +
            ":true,\"Pattern\":\"Sand Scorpion\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Patter" +
            "n\":\"Young Dewan Arthropod\",\"IsUniqueToMap\":false}],\"MapEntityExclusions\":[],\"Nam" +
            "e\":\"Map: Nimbus\"},{\"IsEnabled\":true,\"MaxPlayers\":0,\"MinPlayers\":1,\"MapEntities\":" +
            "[{\"IsEnabled\":true,\"Pattern\":\"Ground_Tholian\",\"IsUniqueToMap\":false},{\"IsEnabled" +
            "\":true,\"Pattern\":\"Mission_Tholian_Invasion_Trapped_Friend_Starfleet\",\"IsUniqueTo" +
            "Map\":false},{\"IsEnabled\":true,\"Pattern\":\"Ground_Tholian_Ensign\",\"IsUniqueToMap\":" +
            "false},{\"IsEnabled\":true,\"Pattern\":\"KDF Officer\",\"IsUniqueToMap\":false},{\"IsEnab" +
            "led\":true,\"Pattern\":\"Starfleet Officer\",\"IsUniqueToMap\":false},{\"IsEnabled\":true" +
            ",\"Pattern\":\"Tholian Drone Assistant\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"P" +
            "attern\":\"Trapped Officer\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Wo" +
            "rker\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Ground_Tholian_Ensign_" +
            "2\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Ground_Tholian_Ensign_3\"," +
            "\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Ground_Tholian_Ensign_Norewa" +
            "rd\",\"IsUniqueToMap\":false}],\"MapEntityExclusions\":[],\"Name\":\"Map: Nukara Prime\"}" +
            ",{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities\":[{\"IsEnabled\":tru" +
            "e,\"Pattern\":\"Ground_Elachi\",\"IsUniqueToMap\":false}],\"MapEntityExclusions\":[{\"IsE" +
            "nabled\":true,\"Pattern\":\"Ground_Elachi_Ensign_Friendly_Rift\",\"IsUniqueToMap\":fals" +
            "e}],\"Name\":\"Event: Defend Rh\'Ihho Station\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"Mi" +
            "nPlayers\":5,\"MapEntities\":[{\"IsEnabled\":true,\"Pattern\":\"Range_Mirror\",\"IsUniqueT" +
            "oMap\":false}],\"MapEntityExclusions\":[],\"Name\":\"Event: Borg Battle Royale\"},{\"IsE" +
            "nabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities\":[{\"IsEnabled\":true,\"Pat" +
            "tern\":\"of Ten Unimatrix 0047\",\"IsUniqueToMap\":true}],\"MapEntityExclusions\":[],\"N" +
            "ame\":\"Red Alert: Borg\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEnti" +
            "ties\":[{\"IsEnabled\":true,\"Pattern\":\"Space_Tholian_Dreadnought_Red_Alert\",\"IsUniq" +
            "ueToMap\":true}],\"MapEntityExclusions\":[],\"Name\":\"Red Alert: Tholian\"},{\"IsEnable" +
            "d\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities\":[{\"IsEnabled\":true,\"Pattern\"" +
            ":\"Space_Nakuhl_Constrictor_Node_Dreadnaught\",\"IsUniqueToMap\":true}],\"MapEntityEx" +
            "clusions\":[],\"Name\":\"Red Alert: Na\'kuhl\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinP" +
            "layers\":5,\"MapEntities\":[{\"IsEnabled\":true,\"Pattern\":\"Elachi_Battleship_Player_P" +
            "et_Control\",\"IsUniqueToMap\":true}],\"MapEntityExclusions\":[],\"Name\":\"Red Alert: E" +
            "lachi\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities\":[{\"IsEnabl" +
            "ed\":true,\"Pattern\":\"C.S.S. Tzen-Torun\",\"IsUniqueToMap\":true}],\"MapEntityExclusio" +
            "ns\":[],\"Name\":\"Red Alert: Tzenkethi\"},{\"IsEnabled\":true,\"MaxPlayers\":0,\"MinPlaye" +
            "rs\":1,\"MapEntities\":[{\"IsEnabled\":true,\"Pattern\":\"Kan\'Keli\",\"IsUniqueToMap\":true" +
            "},{\"IsEnabled\":true,\"Pattern\":\"Tak\'Terak\",\"IsUniqueToMap\":true},{\"IsEnabled\":tru" +
            "e,\"Pattern\":\"Kan\'Keli\",\"IsUniqueToMap\":true},{\"IsEnabled\":true,\"Pattern\":\"Broln’" +
            "ta Cruiser\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Rhas’bej Battles" +
            "hip\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Shuk-din Frigate\",\"IsUn" +
            "iqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Tzenkethi Shield Repair Unit\",\"Is" +
            "UniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Tzen’tar Dreadnought\",\"IsUnique" +
            "ToMap\":false}],\"MapEntityExclusions\":[],\"Name\":\"Map: Gon\'cra System: Tzenkethi B" +
            "attlezone\"},{\"IsEnabled\":true,\"MaxPlayers\":0,\"MinPlayers\":1,\"MapEntities\":[{\"IsE" +
            "nabled\":true,\"Pattern\":\"Space_Voth_Dreadnaught\",\"IsUniqueToMap\":false},{\"IsEnabl" +
            "ed\":true,\"Pattern\":\"Starbase_Player_Fed_T1\",\"IsUniqueToMap\":false},{\"IsEnabled\":" +
            "true,\"Pattern\":\"Voth Frigate\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\"" +
            ":\"Voth Palisade Frigate\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Ace" +
            "ton Drone\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Voth Bastion Crui" +
            "ser\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Voth Bulwark Battleship" +
            "\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Voth Citadel Dreadnought\"," +
            "\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Voth Ward Repair Ship\",\"IsUn" +
            "iqueToMap\":false}],\"MapEntityExclusions\":[],\"Name\":\"Map: Voth Battlezone\"},{\"IsE" +
            "nabled\":true,\"MaxPlayers\":0,\"MinPlayers\":1,\"MapEntities\":[{\"IsEnabled\":true,\"Pat" +
            "tern\":\"Dactylus Bioship\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Dro" +
            "mias Bio-Cruiser\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Nicor Bios" +
            "hip\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Heavy Bioplasmic Torped" +
            "o\",\"IsUniqueToMap\":false}],\"MapEntityExclusions\":[],\"Name\":\"Map: Undine Battlezo" +
            "ne\"},{\"IsEnabled\":true,\"MaxPlayers\":0,\"MinPlayers\":1,\"MapEntities\":[{\"IsEnabled\"" +
            ":true,\"Pattern\":\"Msn_Mu_Bz_Badlands_System_Plasma_Controller\",\"IsUniqueToMap\":fa" +
            "lse},{\"IsEnabled\":true,\"Pattern\":\"Stationmod_Battlecruiser_Com_Eng_Platform\",\"Is" +
            "UniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Fleet Support Platform\",\"IsUniq" +
            "ueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Plasma Control Station\",\"IsUniqueTo" +
            "Map\":false},{\"IsEnabled\":true,\"Pattern\":\"Terran Command Battleship\",\"IsUniqueToM" +
            "ap\":false},{\"IsEnabled\":true,\"Pattern\":\"Terran Cruiser\",\"IsUniqueToMap\":false},{" +
            "\"IsEnabled\":true,\"Pattern\":\"Terran Frigate\",\"IsUniqueToMap\":false}],\"MapEntityEx" +
            "clusions\":[],\"Name\":\"Map: Badlands\"},{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayer" +
            "s\":5,\"MapEntities\":[{\"IsEnabled\":true,\"Pattern\":\"Space_Holo_Projector_Jupiter_Tf" +
            "o\",\"IsUniqueToMap\":true},{\"IsEnabled\":true,\"Pattern\":\"Stationmod_Battlecruiser_C" +
            "om_Sci_Tachyon_Platform\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Dat" +
            "a Thief\",\"IsUniqueToMap\":true},{\"IsEnabled\":true,\"Pattern\":\"Jupiter Holo-Project" +
            "or\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Phaser Defense Platform\"" +
            ",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Photonic Turret\",\"IsUniqueT" +
            "oMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Tachyon Pulse Platform\",\"IsUniqueToMap" +
            "\":false},{\"IsEnabled\":true,\"Pattern\":\"Terran Command Battleship\",\"IsUniqueToMap\"" +
            ":false},{\"IsEnabled\":true,\"Pattern\":\"Terran Cruiser\",\"IsUniqueToMap\":false},{\"Is" +
            "Enabled\":true,\"Pattern\":\"Terran Escort\",\"IsUniqueToMap\":false},{\"IsEnabled\":true" +
            ",\"Pattern\":\"Terran Frigate\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"" +
            "Terran Hacker Ships\",\"IsUniqueToMap\":true},{\"IsEnabled\":true,\"Pattern\":\"Terran N" +
            "ebula Class Science Vessel\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"" +
            "Terran Science Vessel\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Terra" +
            "n Shieldship\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"U.S.S. Cryptei" +
            "a\",\"IsUniqueToMap\":true},{\"IsEnabled\":true,\"Pattern\":\"U.S.S. Voyager\",\"IsUniqueT" +
            "oMap\":false},{\"IsEnabled\":true,\"Pattern\":\"C[1343 Space_Federation_Frigate_Mirror" +
            "]\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"C[1428 Space_Federation_C" +
            "ruiser_Mirror]\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"C[1620 Space" +
            "_Federation_Frigate_Mirror]\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":" +
            "\"C[1657 Space_Federation_Science_Mirror_2]\",\"IsUniqueToMap\":false},{\"IsEnabled\":" +
            "true,\"Pattern\":\"Elite Defense Satellite\",\"IsUniqueToMap\":false},{\"IsEnabled\":tru" +
            "e,\"Pattern\":\"Federation Armitage Class Strike Wing Escort\",\"IsUniqueToMap\":false" +
            "},{\"IsEnabled\":true,\"Pattern\":\"Federation Command Cruiser\",\"IsUniqueToMap\":false" +
            "},{\"IsEnabled\":true,\"Pattern\":\"Federation Frigate\",\"IsUniqueToMap\":false},{\"IsEn" +
            "abled\":true,\"Pattern\":\"Federation Science Vessel\",\"IsUniqueToMap\":false},{\"IsEna" +
            "bled\":true,\"Pattern\":\"Fighter Escort\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"" +
            "Pattern\":\"I.K.S. Vorcha\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"I.S" +
            ".S. Enterprise\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"I.S.S. Reska" +
            "va\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Nebula Class Science Ves" +
            "sel\",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Terran Da Vinci Escort\"" +
            ",\"IsUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"To\'Duj Fighter Squadron\",\"I" +
            "sUniqueToMap\":false},{\"IsEnabled\":true,\"Pattern\":\"Tricobalt Device\",\"IsUniqueToM" +
            "ap\":false},{\"IsEnabled\":true,\"Pattern\":\"U.S.S. Defiant\",\"IsUniqueToMap\":false}]," +
            "\"MapEntityExclusions\":[],\"Name\":\"Event: Iuppiter Iratus\"}],\"GenericGroundMap\":{\"" +
            "IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"MapEntities\":[{\"IsEnabled\":true,\"" +
            "Pattern\":\"Ground_\",\"IsUniqueToMap\":false}],\"MapEntityExclusions\":[],\"Name\":\"Gene" +
            "ric Ground\"},\"GenericSpaceMap\":{\"IsEnabled\":true,\"MaxPlayers\":5,\"MinPlayers\":5,\"" +
            "MapEntities\":[{\"IsEnabled\":true,\"Pattern\":\"Space_\",\"IsUniqueToMap\":false}],\"MapE" +
            "ntityExclusions\":[],\"Name\":\"Generic Space\"}}")]
        public string DefaultCombatDetectionSettings {
            get {
                return ((string)(this["DefaultCombatDetectionSettings"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string StoCombatAnalyzerSettings {
            get {
                return ((string)(this["StoCombatAnalyzerSettings"]));
            }
            set {
                this["StoCombatAnalyzerSettings"] = value;
            }
        }
    }
}
