using MemoQ.MTInterfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpusCatMTPlugin
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
    public partial class OpusCatMTOptionsForm : Form
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

        public OpusCatMTOptions Options { get; set; }

        public OpusCatMTOptionsForm(IEnvironment environment)
        {
            InitializeComponent();
            this.environment = environment;

            // localization
            localizeContent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            mtServicePortTextBox.Text = Options.GeneralSettings.MtServicePort;
            btnHelp.Enabled = isShowHelpSupported();
        }

        private void localizeContent()
        {
            this.Text = LocalizationHelper.Instance.GetResourceString("OptionsFormCaption");
            this.lnkRetrieveLangs.Text = LocalizationHelper.Instance.GetResourceString("RetrieveLanguagesLinkText");
            this.lblSupportedLanguages.Text = LocalizationHelper.Instance.GetResourceString("SupportedLanguagesLabelText");
            this.btnOK.Text = LocalizationHelper.Instance.GetResourceString("OkButtonText");
            this.btnCancel.Text = LocalizationHelper.Instance.GetResourceString("CancelButtonText");
            this.btnHelp.Text = LocalizationHelper.Instance.GetResourceString("HelpButtonText");
            this.mtServicePortLabel.Text = LocalizationHelper.Instance.GetResourceString("MtServicePortText");
            this.instructionTextBox.Text = LocalizationHelper.Instance.GetResourceString("InstructionTextBoxText");
        }
        
        private async void lnkRetrieveLangs_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            setControlsEnabledState(false);
            btnOK.Enabled = false;
            lbLanguages.Items.Clear();

            // do the update in the background
            loginResult = await loginCore("user", "user");

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
                string tokenCode = await Task.Run(() => OpusCatMTServiceHelper.Login(userName, password, this.mtServicePortTextBox.Text));

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
                    loginResult.SupportedLanguages = await Task.Run(() => OpusCatMTServiceHelper.ListSupportedLanguages(tokenCode,this.mtServicePortTextBox.Text));
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
            //tbUserName.Enabled = enabled;
            //tbPassword.Enabled = enabled;
            lnkRetrieveLangs.Enabled = enabled;
            progressBar.Visible = !enabled;
        }

        private void OpusCatMTOptionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private bool isShowHelpSupported()
        {
            // If the resource is remote and downloaded to an old client, it can happen that the client does not support ShowHelp yet.            
            return environment.GetType().GetInterface(nameof(IEnvironment2)) != null;
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            (environment as IEnvironment2)?.ShowHelp("opuscat_help.html");
        }

    }
}
