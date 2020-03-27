using Sdl.FileTypeSupport.Framework.BilingualApi;
using Sdl.TranslationStudioAutomation.IntegrationApi;
using System;
using System.Collections.Generic;
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
        private string sourceCode;
        private string targetCode;

        //private DirectoryInfo fiskmoModelDir;
        //private ModelManager modelManager;
        //private string newerModel;

        public FiskmoOptions Options
        {
            get;
            set;
        }

        public FiskmoConfDialog(FiskmoOptions options, Sdl.LanguagePlatform.Core.LanguagePair[] languagePairs)
        {
            this.sourceCode = languagePairs[0].SourceCulture.TwoLetterISOLanguageName.ToLower();
            this.targetCode = languagePairs[0].TargetCulture.TwoLetterISOLanguageName.ToLower();
            //this.modelManager = new ModelManager();
            Options = options;
            InitializeComponent();
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this.aboutBox.LoadFile(Path.Combine(assemblyFolder,"LICENSE.rtf"));

            /*
            //Add code for disabling form if language pair is not correct, in case it's needed.
            if 
                ((!((sourceCode == "fi" || sourceCode == "sv") && (targetCode == "fi" || targetCode == "sv"))) &&
                !FiskmoTpSettings.Default.SupportAllLanguagePairs)
            {
                this.infoBox.Text = "The Fiskmö plugin only supports translation between Swedish and Finnish.";
                this.Save_btn.Enabled = false;
                return;
            }



            //If no model is loaded, deactivate Save button
            var existingModel = this.modelManager.GetLatestModelDir(sourceCode, targetCode);
            if (existingModel == null)
            {
                this.infoBox.AppendText($"No local model for language pair {sourceCode}-{targetCode}.");
                this.Save_btn.Enabled = false;
            }
            else
            {
                var existingModelName = new DirectoryInfo(existingModel).Name;
                this.infoBox.AppendText($"Local model for the language pair {sourceCode}-{targetCode} is ");
                this.infoBox.SelectionFont = new System.Drawing.Font(this.infoBox.Font, System.Drawing.FontStyle.Bold);
                this.infoBox.AppendText(existingModelName);
                this.infoBox.SelectionFont = new System.Drawing.Font(this.infoBox.Font, System.Drawing.FontStyle.Regular);
                this.infoBox.AppendText("." + Environment.NewLine);
                this.Save_btn.Enabled = true;
            }

            this.newerModel = this.modelManager.CheckForNewerModel(sourceCode, targetCode);
            if (this.newerModel != null)
            {
                //Activate the Load model button 
                this.Customize_btn.Visible = true;
                this.Customize_btn.Enabled = true;
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
            }*/

        }
        

        private void FiskmoConfDialog_Load(object sender, EventArgs e)
        {
            
            //Only set pregenerate to unchecked if pregenerate option is False (this means
            //the default state is checked)
            this.pregenerateCheckbox.Checked = Options.pregenerateMt == "True";
            this.mtOriginCheckbox.Checked = Options.showMtAsOrigin == "True";
            this.useAllModelsCheckBox.Checked = Options.useAllModels;
            this.mtServicePortBox.Text = Options.mtServicePort;
        }

        private void Save_btn_Click(object sender, EventArgs e)
        {
            Options.pregenerateMt = this.pregenerateCheckbox.Checked.ToString();
            Options.showMtAsOrigin = this.mtOriginCheckbox.Checked.ToString();
            Options.useAllModels = this.useAllModelsCheckBox.Checked;
            Options.mtServicePort = this.mtServicePortBox.Text;
            this.DialogResult = DialogResult.OK;
        }
       
        private void Cancel_btn_Click(object sender, EventArgs e)
        {            
            this.DialogResult = DialogResult.Cancel;
        }

        //this would work best as batch task
        private void Customize_btn_Click(object sender, EventArgs e)
        {
            this.downloadProgress.Visible = true;

            EditorController editorController = SdlTradosStudio.Application.GetController<EditorController>();
            List<Tuple<string, string>> tuningSet = new List<Tuple<string, string>>();
            foreach (var doc in editorController.GetDocuments())
            {
                foreach (var segmentPair in doc.SegmentPairs)
                {
                    if (segmentPair.Properties.ConfirmationLevel == Sdl.Core.Globalization.ConfirmationLevel.Translated ||
                        segmentPair.Properties.ConfirmationLevel == Sdl.Core.Globalization.ConfirmationLevel.ApprovedTranslation)
                    {
                        var allSourceTextItems = segmentPair.Source.AllSubItems.Where(x => x is IText);
                        var sourceText = String.Join(" ", allSourceTextItems);

                        var allTargetTextItems = segmentPair.Target.AllSubItems.Where(x => x is IText);
                        var targetText = String.Join(" ", allTargetTextItems);

                        tuningSet.Add(new Tuple<string, string>(sourceText, targetText));
                    }
                }
            }

            FiskmöMTServiceHelper.Customize(this.Options, tuningSet, this.sourceCode, this.targetCode);
        }

        /*
        async void wc_DownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            this.infoBox.Text = "Model loaded, extracting it";
            await Task.Run(() => this.modelManager.ExtractModel());

            this.downloadProgress.Visible = false;
            this.Save_btn.Enabled = true;
            this.Customize_btn.Visible = false;
            this.Customize_btn.Enabled = false;
            
            this.infoBox.Text = "Current model loaded, click Save to start using Fiskmö plugin";

        }
        
        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.downloadProgress.Value = e.ProgressPercentage;
        }
        */

    }
}
    