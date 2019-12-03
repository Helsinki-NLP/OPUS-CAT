using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Environment;

namespace FiskmoTranslationProvider
{


    public partial class FiskmoConfDialog : Form
    {
        private DirectoryInfo fiskmoModelDir;
        private ModelManager modelManager;
        private string newerModel;

        public FiskmoOptions Options
        {
            get;
            set;
        }

        public FiskmoConfDialog(FiskmoOptions options, Sdl.LanguagePlatform.Core.LanguagePair[] languagePairs)
        {
            string sourceLang = languagePairs[0].SourceCulture.TwoLetterISOLanguageName.ToLower();
            string targetLang = languagePairs[0].TargetCulture.TwoLetterISOLanguageName.ToLower();
            this.modelManager = new ModelManager();
            Options = options;
            InitializeComponent();
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this.aboutBox.LoadFile(Path.Combine(assemblyFolder,"LICENSE.rtf"));
            /*Add code for disabling form if language pair is not correct, in case it's needed.
             * if (!((sSourceCulture == "fi" || sSourceCulture == "sv") && (sSourceCulture == "fi" || sSourceCulture == "sv"))))
            {
                this.DisableAll();
            }*/



            //If no model is loaded, deactivate Save button
            var existingModel = this.modelManager.GetLatestModelDir(sourceLang, targetLang);
            if (existingModel == null)
            {
                this.infoBox.AppendText($"No local model for language pair {sourceLang}-{targetLang}.");
                this.Save_btn.Enabled = false;
            }
            else
            {
                var existingModelName = new DirectoryInfo(existingModel).Name;
                this.infoBox.AppendText($"Local model for the language pair {sourceLang}-{targetLang} is ");
                this.infoBox.SelectionFont = new System.Drawing.Font(this.infoBox.Font, System.Drawing.FontStyle.Bold);
                this.infoBox.AppendText(existingModelName);
                this.infoBox.SelectionFont = new System.Drawing.Font(this.infoBox.Font, System.Drawing.FontStyle.Regular);
                this.infoBox.AppendText("." + Environment.NewLine);
                this.Save_btn.Enabled = true;
            }

            this.newerModel = this.modelManager.CheckForNewerModel(sourceLang, targetLang);
            if (this.newerModel != null)
            {
                //Activate the Load model button 
                this.LoadModel_btn.Visible = true;
                this.LoadModel_btn.Enabled = true;
                var newModelName = new DirectoryInfo(newerModel).Name;
                this.infoBox.AppendText("New model ");
                this.infoBox.SelectionFont = new System.Drawing.Font(this.infoBox.Font, System.Drawing.FontStyle.Bold);
                this.infoBox.AppendText(newModelName);
                this.infoBox.SelectionFont = new System.Drawing.Font(this.infoBox.Font, System.Drawing.FontStyle.Regular);
                this.infoBox.AppendText(" can be downloaded from the Fiskmö repository.");
            }

            if (existingModel == null && this.newerModel == null)
            {
                this.infoBox.Text = "No model available for the language pair";
            }
        }

        /*private void DisableAll()
        {
            foreach (Control c in this.Controls)
            {

            }
        }*/

        private void FiskmoConfDialog_Load(object sender, EventArgs e)
        {
            /*this.address_txtbox.Text = Options.serverAddress;
            this.port_txtbox.Text = Options.port;
            this.textBoxCustomer.Text = Options.client;
            this.textBoxSubject.Text = Options.subject;
            this.textBoxOtherFeatures.Text = Options.otherFeatures;*/
            
            //Only set pregenerate to unchecked if pregenerate option is False (this means
            //the default state is checked)
            if (Options.pregenerateMt == "False")
            {
                this.pregenerateCheckbox.Checked = false;
            }
            else
            {
                this.pregenerateCheckbox.Checked = true;
            }
        }

        private void Save_btn_Click(object sender, EventArgs e)
        {
            /*Options.serverAddress = this.address_txtbox.Text.Trim();
            Options.port = this.port_txtbox.Text.Trim();
            Options.client = this.textBoxCustomer.Text.Trim();
            Options.subject = this.textBoxSubject.Text.Trim();
            Options.otherFeatures = this.textBoxOtherFeatures.Text.Trim();*/
            Options.pregenerateMt = this.pregenerateCheckbox.Checked.ToString();
            this.DialogResult = DialogResult.OK;

        }
       
        private void Cancel_btn_Click(object sender, EventArgs e)
        {            
            this.DialogResult = DialogResult.Cancel;
        }
    
        private void LoadModel_btn_Click(object sender, EventArgs e)
        {
            this.downloadProgress.Visible = true;
            this.modelManager.DownloadModel(
                this.newerModel, wc_DownloadProgressChanged, wc_DownloadComplete);
        }

        async void wc_DownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            this.infoBox.Text = "Model loaded, extracting it";
            await Task.Run(() => this.modelManager.ExtractModel());

            this.downloadProgress.Visible = false;
            this.Save_btn.Enabled = true;
            this.LoadModel_btn.Visible = false;
            this.LoadModel_btn.Enabled = false;
            
            this.infoBox.Text = "Current model loaded, click Save to start using Fiskmö plugin";

        }

        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.downloadProgress.Value = e.ProgressPercentage;
        }
        
    }
}
    