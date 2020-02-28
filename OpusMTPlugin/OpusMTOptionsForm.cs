using MemoQ.MTInterfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpusMTPlugin
{
    /// <summary>
    /// This class represents the options form of the dummy MT plugin.
    /// </summary>
    /// <remarks>
    /// Implementation checklist:
    ///     - 	There is a configuration dialog, where the user is able to configure the plugin.
    ///     -   The user cannot save the settings until all of the mandatory parameters were not configured correctly.
    ///     -   The dialog collects the user modifications in the memory and saves only when the user OKs the dialog.
    ///     - 	The dialog does not call any blocking service in the user interface thread; it has to use background threads.
    ///     -   Check UI so that it is displayed correctly at high DPI settings.
    /// </remarks>
    public partial class OpusMTOptionsForm : Form
    {
        private delegate void LoginDelegate(string userName, string password);
        private IEnvironment environment;

        private class LoginResult
        {
            public string UserName;
            public string Password;
            public bool LoginSuccessful;
            public List<string> SupportedLanguages;
            public Exception Exception;
        }

        private LoginResult loginResult;

        public OpusMTOptions Options { get; set; }

        public OpusMTOptionsForm(IEnvironment environment)
        {
            InitializeComponent();
            this.environment = environment;

            // localization
            localizeContent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            tbUserName.Text = Options.SecureSettings.UserName;
            tbPassword.Text = Options.SecureSettings.Password;
            foreach (string lang in Options.GeneralSettings.SupportedLanguages)
                lbLanguages.Items.Add(lang);

            this.btnOK.Enabled = !string.IsNullOrEmpty(Options.SecureSettings.UserName);
            btnHelp.Enabled = isShowHelpSupported();
            cbFormattingTags.SelectedIndex = (int)Options.GeneralSettings.FormattingAndTagUsage;
        }

        private void localizeContent()
        {
            this.Text = LocalizationHelper.Instance.GetResourceString("OptionsFormCaption");
            this.lblUserName.Text = LocalizationHelper.Instance.GetResourceString("UserNameLabelText");
            this.lblPassword.Text = LocalizationHelper.Instance.GetResourceString("PasswordLabelText");
            this.lnkRetrieveLangs.Text = LocalizationHelper.Instance.GetResourceString("RetrieveLanguagesLinkText");
            this.lblSupportedLanguages.Text = LocalizationHelper.Instance.GetResourceString("SupportedLanguagesLabelText");
            this.btnOK.Text = LocalizationHelper.Instance.GetResourceString("OkButtonText");
            this.btnCancel.Text = LocalizationHelper.Instance.GetResourceString("CancelButtonText");
            this.btnHelp.Text = LocalizationHelper.Instance.GetResourceString("HelpButtonText");
            this.lblTagsFormatting.Text = LocalizationHelper.Instance.GetResourceString("TagsAndFormattingLabelText");

            this.cbFormattingTags.Items.Add(LocalizationHelper.Instance.GetResourceString("PlainTextOnly"));
            this.cbFormattingTags.Items.Add(LocalizationHelper.Instance.GetResourceString("TextAndFormatting"));
            this.cbFormattingTags.Items.Add(LocalizationHelper.Instance.GetResourceString("FormattingAndTags"));
        }

        private void tbUserNamePassword_TextChanged(object sender, EventArgs e)
        {
            lnkRetrieveLangs.Enabled = !string.IsNullOrEmpty(tbUserName.Text) && !string.IsNullOrEmpty(tbPassword.Text);
            btnOK.Enabled = false;
        }

        private async void lnkRetrieveLangs_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            setControlsEnabledState(false);
            btnOK.Enabled = false;
            lbLanguages.Items.Clear();

            // do the update in the background
            loginResult = await loginCore(tbUserName.Text, tbPassword.Text);

            handleLoginFinished();
        }

        private async Task<LoginResult> loginCore(string userName, string password)
        {
            var loginResult = new LoginResult()
            {
                UserName = userName,
                Password = password
            };

            try
            {
                // try to login
                // Do not call any blocking service in the user interface thread; it has to use background threads.
                string tokenCode = await Task.Run(() => OpusMTServiceHelper.Login(userName, password));

                if (string.IsNullOrEmpty(tokenCode))
                {
                    // invalid user name or password
                    loginResult.LoginSuccessful = false;
                }
                else
                {
                    // successful login
                    loginResult.LoginSuccessful = true;
                    // try to get the list of the supported languages in the background
                    // Do not call any blocking service in the user interface thread; it has to use background threads.
                    loginResult.SupportedLanguages = await Task.Run(() => OpusMTServiceHelper.ListSupportedLanguages(tokenCode));
                }
            }
            catch (Exception ex)
            {
                loginResult.Exception = ex;
            }

            return loginResult;
        }

        private void handleLoginFinished()
        {
            // it is possible that the form has disposed during the background operation (e.g. the user clicked on the cancel button)
            if (!IsDisposed)
            {
                if (loginResult.Exception != null)
                {
                    // there was an error, display for the user
                    string caption = LocalizationHelper.Instance.GetResourceString("CommunicationErrorCaption");
                    string text = LocalizationHelper.Instance.GetResourceString("CommunicationErrorText");
                    MessageBox.Show(this, string.Format(text, loginResult.Exception.Message), caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (!loginResult.LoginSuccessful)
                {
                    // the user name of the password is invalid, display for the user
                    string caption = LocalizationHelper.Instance.GetResourceString("InvalidUserNameCaption");
                    string text = LocalizationHelper.Instance.GetResourceString("InvalidUserNameText");
                    MessageBox.Show(this, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    // we have managed to get the supported languages, display them in the listbox
                    lbLanguages.Items.Clear();
                    foreach (string lang in loginResult.SupportedLanguages)
                    {
                        lbLanguages.Items.Add(lang);
                    }

                    btnOK.Enabled = loginResult.SupportedLanguages.Count > 0;
                }

                setControlsEnabledState(true);
            }
        }

        private void setControlsEnabledState(bool enabled)
        {
            tbUserName.Enabled = enabled;
            tbPassword.Enabled = enabled;
            lnkRetrieveLangs.Enabled = enabled;
            progressBar.Visible = !enabled;
        }

        private void DummyMTOptionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == System.Windows.Forms.DialogResult.OK && loginResult != null)
            {
                // if there was a modification, we have to save the changes
                Options.SecureSettings.UserName = loginResult.UserName;
                Options.SecureSettings.Password = loginResult.Password;

                Options.GeneralSettings.SupportedLanguages = loginResult.SupportedLanguages.ToArray();
                Options.GeneralSettings.FormattingAndTagUsage = (FormattingAndTagsUsageOption)cbFormattingTags.SelectedIndex;
            }
        }

        private bool isShowHelpSupported()
        {
            // If the resource is remote and downloaded to an old client, it can happen that the client does not support ShowHelp yet.            
            return environment.GetType().GetInterface(nameof(IEnvironment2)) != null;
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            (environment as IEnvironment2)?.ShowHelp("googlemt-settings.html");
        }
    }
}
