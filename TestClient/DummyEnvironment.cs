using System;
using MemoQ.Addins.Common.DataStructures;
using MemoQ.MTInterfaces;

namespace MT_SDK
{
    /// <summary>
    /// Dummy environment to be able to initialize the plugins.
    /// </summary>
    public class DummyEnvironment : IEnvironment
    {
        /// <summary>
        /// The two-letter UI language code of the application.
        /// </summary>
        public string UILang
        {
            get { return "eng"; }
        }

        /// <summary>
        /// Handles the plugin availability changed events.
        /// </summary>
        public void PluginAvailabilityChanged() { }

        /// <summary>
        /// Parse the string for a TMX segment, i.e.: "<seg>...</seg>"
        /// </summary>
        public Segment ParseTMXSeg(string str)
        {
            return Segment.Empty;
        }

        /// <summary>
        /// Serialize the segment as a TMX segment, i.e.: "<seg>...</seg>"
        /// </summary>
        public string WriteTMXSeg(Segment seg)
        {
            return string.Empty;
        }

        /// <summary>
        /// Returns the localized text which is belonging to the specified key.
        /// If returns null the MT plugin should display its own default texts.
        /// </summary>
        public string GetResourceString(string pluginName, string key)
        {
            return null;
        }

        /// <summary>
        /// Shows the localized web help otherwise the deployed (offline) English help.
        /// </summary>
        public void ShowHelp(string helpTopicId)
        {
            return;
        }
    }
}
