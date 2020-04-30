namespace FiskmoMTPlugin
{
    /// <summary>
    /// Class for storing the Dummy MT plugin settings.
    /// </summary>
    /// <remarks>
    /// Implementation checklist:
    ///     - There is an options class, with proper generic and secure subclasses (the secure options class can be omitted).
	///     - The options class is a simple entity class, does not call any services, and simply gives back the saved or the default settings.
	///     - The options class does not store/load its own settings.
    /// </remarks>
    public class FiskmoMTOptions : MemoQ.MTInterfaces.PluginSettingsObject<FiskmöMTGeneralSettings, FiskmöMTSecureSettings>
    {
        /// <summary>
        /// Create instance by deserializing from provided serialized settings.
        /// </summary>
        public FiskmoMTOptions(MemoQ.MTInterfaces.PluginSettings serializedSettings)
            : base(serializedSettings)
        {
        }

        /// <summary>
        /// Create instance by providing the settings objects.
        /// </summary>
        public FiskmoMTOptions(FiskmöMTGeneralSettings generalSettings, FiskmöMTSecureSettings secureSettings)
            : base(generalSettings, secureSettings)
        {
        }
    }

    /// <summary>
    /// General settings, content preserved when settings are exported.
    /// </summary>
    public class FiskmöMTGeneralSettings
    {
        public string MtServicePort = "8477";
    }

    /// <summary>
    /// Settings, whether inline tags and/or formatting should be included in the request sent to the machine translation provider.
    /// </summary>
    public enum FormattingAndTagsUsageOption
    {
        Plaintext = 0,
        OnlyFormatting = 1,
        BothFormattingAndTags = 2,
    }

    /// <summary>
    /// Secure settings, content not preserved when settings leave the machine.
    /// </summary>
    public class FiskmöMTSecureSettings
    {
        
    }
}
