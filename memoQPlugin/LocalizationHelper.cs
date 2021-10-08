using System.Collections.Generic;
using MemoQ.MTInterfaces;

namespace OpusCatMTPlugin
{
    /// <summary>
    /// Singleton helper class to be able to localize the plugin's textual information.
    /// </summary>
    internal class LocalizationHelper
    {
        /// <summary>
        /// The default text to be used when the IEnvironment.GetResourceString returns with null.
        /// </summary>
        private Dictionary<string, string> defaultTexts = new Dictionary<string, string>()
        {
            { "OptionsFormCaption", "OPUS-CAT MT Engine Plugin settings" },
            { "RetrieveLanguagesLinkText", "Retrieve language pair information"},
            { "SupportedLanguagesLabelText", "Local language pairs (additional language pairs can be installed in the OPUS-CAT MT application)" },
            { "OkButtonText", "OK" },
            { "CancelButtonText", "Cancel" },
            { "HelpButtonText", "Help" },
            { "CommunicationErrorCaption", "Login error" },
            { "CommunicationErrorText", "There was an error during the communication with the service. Make sure the OPUS-CAT MT Engine application is running and that is uses the same port as the plugin.\n\n{0}" },
            { "InvalidUserNameCaption", "Login error" },
            { "InvalidUserNameText", "Invalid user name or password." },
            { "NetworkError", "A network error occured. Check that OPUS-CAT MT Engine application is running, and that MT Engine port is the same in the engine and the plugin: {0}" },
            { "PlainTextOnly", "Use plain text only" },
            { "TextAndFormatting", "Use text and formatting" },
            { "FormattingAndTags", "Use both formatting and tags" },
            { "TagsAndFormattingLabelText", "Tags and formatting"},
            { "MtServicePortText", "OPUS-CAT MT service port (must  be the same as the port in OPUS-CAT MT Engine)"},
            { "InstructionTextBoxText", "OPUS-CAT MT plugin fetches translations from a OPUS-CAT MT Engine application that needs to be installed separately on the computer where the plugin is used. Start the OPUS-CAT MT Engine application and install the MT models that you need before using the plugin." }
        };

        /// <summary>
        /// The singleton instance of the localization helper.
        /// </summary>
        private static LocalizationHelper instance = new LocalizationHelper();

        /// <summary>
        /// Private constructor to avoid multiple instances.
        /// </summary>
        private LocalizationHelper()
        { }

        /// <summary>
        /// The environment to be used to get localized texts from memoQ.
        /// </summary>
        private IEnvironment environment;

        /// <summary>
        /// The singleton instance of the localization helper.
        /// </summary>
        public static LocalizationHelper Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// Sets the environment to be able to get localized texts.
        /// </summary>
        /// <param name="environment"></param>
        public void SetEnvironment(IEnvironment environment)
        {
            this.environment = environment;
        }

        /// <summary>
        /// Gets the localized text belonging to the specified key.
        /// </summary>
        public string GetResourceString(string key)
        {
            // try to get the localized text from the environment
            string localizedText = environment.GetResourceString(OpusCatMTPluginDirector.PluginId, key);

            // use the default texts if the environment returns with null
            if (localizedText == $"MTPlugin.{OpusCatMTPluginDirector.PluginId}.{key}" || string.IsNullOrEmpty(localizedText))
                localizedText = defaultTexts[key];

            return localizedText;
        }
    }
}
