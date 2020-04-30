using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FiskmoMTPlugin;
using MemoQ.Addins.Common.DataStructures;
using MemoQ.Addins.Common.Framework;
using MemoQ.Addins.Common.Utils;
using MemoQ.MTInterfaces;

namespace MT_SDK
{
    /// <summary>
    /// The main form of the sample application.
    /// </summary>
    public partial class MainForm : Form
    {
        private bool initializing;

        private List<IPluginInfo> plugins;

        private ICurrentEngine mtEngine;

        public MainForm()
        {
            InitializeComponent();

            initializing = false;

            // initialize the list of the plugins
            plugins = new List<IPluginInfo>();

            // add the Dummy MT plugin to the list
            plugins.Add(PluginInfoFactory.Create(new FiskmöMTPluginDirector()));

            // add other plugin directors

            // check the ModuleAttribute attributes
            checkModuleAttributes(plugins);

            if (plugins.Count > 0)
            {
                // set the environment for each plugin
                var environment = new DummyEnvironment();
                foreach (var pluginInfo in plugins)
                {
                    pluginInfo.SetEnvironment(environment);
                    lbPlugins.Items.Add(pluginInfo);
                }

                lbPlugins.SelectedIndex = 0;

                populateLanguageSelectors();
            }
            else
            {
                gbPluginDetails.Enabled = false;
                gbTranslation.Enabled = false;
            }
        }

        private void checkModuleAttributes(List<IPluginInfo> plugins)
        {
            var sb = new StringBuilder();
            var pluginsToRemove = new List<IPluginInfo>();

            foreach (var plugin in plugins)
            {
                bool attrFound = false;

                foreach (Attribute attr in plugin.Director.GetType().Assembly.GetCustomAttributes(true))
                {
                    if (attr is ModuleAttribute)
                    {
                        attrFound = true;
                        break;
                    }
                }

                if (!attrFound)
                {
                    pluginsToRemove.Add(plugin);
                    sb.AppendLine(plugin.Director.FriendlyName);
                }
            }

            if (sb.Length > 0)
            {
                foreach (var plugin in pluginsToRemove)
                    plugins.Remove(plugin);

                MessageBox.Show(this, string.Format("The assemblies of following plugins is not marked with the ModuleAttribute attribute.\n\n{0}", sb.ToString()), "Missing attribute", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Populates the language selector comboboxes.
        /// </summary>
        private void populateLanguageSelectors()
        {
            initializing = true;

            var supportedLangs = new List<string>() { "eng", "fin", "fre", "ger", "hun", "ita", "por", "spa" };

            cbSourceLanguage.Items.Add("Select language");
            cbTargetLanguage.Items.Add("Select language");
            foreach (var language in supportedLangs)
                cbSourceLanguage.Items.Add(new Language(language));
            foreach (var language in supportedLangs)
                cbTargetLanguage.Items.Add(new Language(language));
            cbSourceLanguage.SelectedIndex = 0;
            cbTargetLanguage.SelectedIndex = 0;

            initializing = false;
        }

        /// <summary>
        /// Checks the selected languages in the comboboxes and displays messages for the user.
        /// </summary>
        /// <returns>True if everything is OK, false otherwise.</returns>
        private bool checkSelectedLanguages()
        {
            // not selected
            if (cbSourceLanguage.SelectedIndex == -1 || cbTargetLanguage.SelectedIndex == -1)
                return false;

            // "Select language" is selected
            if (cbSourceLanguage.SelectedIndex == 0 || cbTargetLanguage.SelectedIndex == 0)
            {
                btnTranslate.Enabled = false;
                return false;
            }

            // the same language is selected on both sides
            if (Language.Equals(cbSourceLanguage.SelectedItem as Language, cbTargetLanguage.SelectedItem as Language))
            {
                btnTranslate.Enabled = false;
                lblMessage.Text = "The source and the target languages are the same.";
                return false;
            }

            var actPlugin = lbPlugins.SelectedItem as IPluginInfo;

            // the selected plugin does not support the selected language pair
            if (!actPlugin.IsLanguagePairSupported((cbSourceLanguage.SelectedItem as Language).LangCode, (cbTargetLanguage.SelectedItem as Language).LangCode))
            {
                btnTranslate.Enabled = false;
                btnStoreTranslation.Enabled = false;
                lblMessage.Text = "This language pair is not supported by the selected plugin.";
                return false;
            }

            btnStoreTranslation.Enabled = actPlugin.StoringTranslationSupported;
            tbTarget.ReadOnly = !actPlugin.StoringTranslationSupported;

            return true;
        }

        /// <summary>
        /// Updates the controls' states based on the selected plugin settings.
        /// </summary>
        public void UpdateControls()
        {
            initializing = true;

            var actPlugin = lbPlugins.SelectedItem as IPluginInfo;
            var actPluginDirector = actPlugin.Director;
            lblNameValue.Text = actPluginDirector.FriendlyName;
            lblCopyrightValue.Text = actPluginDirector.CopyrightText;
            lblInteractiveLookupValue.Text = actPluginDirector.InteractiveSupported ? "True" : "False";
            lblBatchLookupValue.Text = actPluginDirector.BatchSupported ? "True" : "False";
            lblStoringTranslationsValue.Text = (lbPlugins.SelectedItem as IPluginInfo).StoringTranslationSupported ? "True" : "False";
            pictureBox.Image = actPluginDirector.DisplayIcon;
            lblConfiguredValue.Text = actPlugin.IsPluginConfigured ? "Configured" : "Not configured";
            cbEnabled.Checked = actPlugin.IsPluginEnabled;
            gbTranslation.Enabled = actPlugin.IsPluginEnabled && actPlugin.IsPluginConfigured;

            checkSelectedLanguages();

            initializing = false;
        }

        private void lbPlugins_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void lnkConfigure_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            (lbPlugins.SelectedItem as IPluginInfo).ShowOptionsForm(this);
            UpdateControls();
        }

        private void cbEnabled_CheckedChanged(object sender, EventArgs e)
        {
            (lbPlugins.SelectedItem as IPluginInfo).IsPluginEnabled = cbEnabled.Checked;
            UpdateControls();
        }

        private void cbLanguages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (initializing) return;

            tbTarget.Text = string.Empty;

            if (!checkSelectedLanguages()) return;

            lblMessage.Text = string.Empty;
            btnTranslate.Enabled = true;

            // dispose the existing MT engine if exists
            if (mtEngine != null)
            {
                mtEngine.Dispose();
                mtEngine = null;
            }

            // intialize the new MT engine based on the selected languages
            mtEngine = (lbPlugins.SelectedItem as IPluginInfo).CreateEngine((cbSourceLanguage.SelectedItem as Language).LangCode, (cbTargetLanguage.SelectedItem as Language).LangCode);
        }

        private void btnTranslate_Click(object sender, EventArgs e)
        {
            if (tbSource.Lines.Length == 0) return;

            this.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            lblMessage.Text = string.Empty;

            using (var session = this.mtEngine.CreateLookupSession())
            {
                if (this.tbSource.Lines.Length == 1)
                {
                    var segment = getOneSegmentsFromTextbox(this.tbSource);
                    TranslationResult result = session.TranslateCorrectSegment(segment, Segment.Empty, Segment.Empty);

                    if (result.Exception == null)
                    {
                        this.tbTarget.Text = result.Translation.PlainText;
                    }
                    else
                    {
                        if (!(result.Exception is MTException))
                        {
                            MessageBox.Show(this, "The plugin has to throw MTException.", "Incorrect exception thrown", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show(this, string.Format("There was an error during the translation.\n\n{0}", result.Exception.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    var segments = getSegmentsFromTextbox(tbSource);
                    var tmSources = getEmptySegments(segments.Length);
                    var tmTargets = getEmptySegments(segments.Length);

                    TranslationResult[] results = session.TranslateCorrectSegment(segments, tmSources, tmTargets);

                    if (results.Any(r => r.Exception != null))
                    {
                        if (!(results[0].Exception is MTException))
                        {
                            MessageBox.Show(this, "The plugin has to throw MTException.", "Incorrect exception thrown", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show(this, string.Format("There was an error during the translation.\n\n{0}", results[0].Exception.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (TranslationResult result in results)
                        {
                            sb.AppendLine(result.Translation.PlainText);
                        }

                        this.tbTarget.Text = sb.ToString();
                    }
                }
            }

            this.Cursor = Cursors.Arrow;
            this.Enabled = true;
        }

        private void btnStoreTranslation_Click(object sender, EventArgs e)
        {
            if (tbSource.Lines.Length == 0) return;
            if (tbTarget.Lines.Length == 0) return;

            var director = lbPlugins.SelectedItem as IPluginInfo;
            if (!director.StoringTranslationSupported)
            {
                MessageBox.Show("Engine not supporting storing translations.");
                return;
            }

            this.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            lblMessage.Text = string.Empty;

            using (var session = this.mtEngine.CreateSessionForStoringTranslation())
            {
                if (this.tbSource.Lines.Length == 1)
                {
                    var transUnit = new TranslationUnit(getOneSegmentsFromTextbox(this.tbSource), getOneSegmentsFromTextbox(this.tbTarget));
                    try
                    {
                        session.StoreTranslation(transUnit);
                        lblMessage.Text = "Translation stored.";
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(this, string.Format("There was an error during storing the translation.\n\n{0}", exception.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    var transUnitsToAdd = getTranslationUnitsFromTextboxes(this.tbSource, this.tbTarget);
                    try
                    {
                        var indicesStored = session.StoreTranslation(transUnitsToAdd);
                        lblMessage.Text = "Translations stored.";

                        if (indicesStored.Length != transUnitsToAdd.Length)
                        {
                            MessageBox.Show(this, "Not all translations were stored by the plugin.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(this, string.Format("There was an error during strogin the translations.\n\n{0}", exception.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            this.Cursor = Cursors.Arrow;
            this.Enabled = true;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mtEngine != null)
            {
                mtEngine.Dispose();
                mtEngine = null;
            }
        }

        private static Segment[] getEmptySegments(int count)
        {
            var segments = new Segment[count];
            for (int i = 0; i < count; ++i)
                segments[i] = Segment.Empty;
            return segments;
        }

        private static Segment getOneSegmentsFromTextbox(TextBox textbox)
        {
            return SegmentBuilder.CreateFromString(textbox.Lines[0]);
        }

        private static Segment[] getSegmentsFromTextbox(TextBox textbox)
        {
            var segments = new List<Segment>();
            foreach (string line in textbox.Lines)
                segments.Add(SegmentBuilder.CreateFromString(line));
            return segments.ToArray();
        }

        private static TranslationUnit[] getTranslationUnitsFromTextboxes(TextBox tbSource, TextBox tbTarget)
        {
            var transUnits = new List<TranslationUnit>();
            for (int i = 0; i < tbSource.Lines.Length && i < tbTarget.Lines.Length; ++i)
                transUnits.Add(new TranslationUnit(
                                SegmentBuilder.CreateFromString(tbSource.Lines[i]),
                                SegmentBuilder.CreateFromString(tbTarget.Lines[i])));
            return transUnits.ToArray();
        }
    }
}
