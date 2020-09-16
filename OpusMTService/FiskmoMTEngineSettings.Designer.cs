﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FiskmoMTEngine {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
    internal sealed partial class FiskmoMTEngineSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static FiskmoMTEngineSettings defaultInstance = ((FiskmoMTEngineSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new FiskmoMTEngineSettings())));
        
        public static FiskmoMTEngineSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://object.pouta.csc.fi/OPUS-MT-models/")]
        public string ModelStorageUrl {
            get {
                return ((string)(this["ModelStorageUrl"]));
            }
            set {
                this["ModelStorageUrl"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("fiskmo/")]
        public string LocalFiskmoDir {
            get {
                return ((string)(this["LocalFiskmoDir"]));
            }
            set {
                this["LocalFiskmoDir"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("8477")]
        public string MtServicePort {
            get {
                return ((string)(this["MtServicePort"]));
            }
            set {
                this["MtServicePort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("customize.yml")]
        public string CustomizationBaseConfig {
            get {
                return ((string)(this["CustomizationBaseConfig"]));
            }
            set {
                this["CustomizationBaseConfig"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("translations.sqlite")]
        public string TranslationDBName {
            get {
                return ((string)(this["TranslationDBName"]));
            }
            set {
                this["TranslationDBName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("logs")]
        public string LogDir {
            get {
                return ((string)(this["LogDir"]));
            }
            set {
                this["LogDir"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Marian")]
        public string MarianDir {
            get {
                return ((string)(this["MarianDir"]));
            }
            set {
                this["MarianDir"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Preprocessing")]
        public string PreprocessingDir {
            get {
                return ((string)(this["PreprocessingDir"]));
            }
            set {
                this["PreprocessingDir"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("50")]
        public int ModelTagMaxLength {
            get {
                return ((int)(this["ModelTagMaxLength"]));
            }
            set {
                this["ModelTagMaxLength"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool StartHttpService {
            get {
                return ((bool)(this["StartHttpService"]));
            }
            set {
                this["StartHttpService"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("TatoebaTestsets")]
        public string TatoebaDir {
            get {
                return ((string)(this["TatoebaDir"]));
            }
            set {
                this["TatoebaDir"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("100")]
        public int OODValidSetSize {
            get {
                return ((int)(this["OODValidSetSize"]));
            }
            set {
                this["OODValidSetSize"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("train.log")]
        public string TrainLogName {
            get {
                return ((string)(this["TrainLogName"]));
            }
            set {
                this["TrainLogName"] = value;
            }
        }
    }
}
