namespace OpusCatMTPlugin
{
    partial class OpusCatMTOptionsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpusCatMTOptionsForm));
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lnkRetrieveLangs = new System.Windows.Forms.LinkLabel();
            this.lbLanguages = new System.Windows.Forms.ListBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblSupportedLanguages = new System.Windows.Forms.Label();
            this.btnHelp = new System.Windows.Forms.Button();
            this.mtServicePortLabel = new System.Windows.Forms.Label();
            this.mtServicePortTextBox = new System.Windows.Forms.TextBox();
            this.instructionTextBox = new System.Windows.Forms.TextBox();
            this.RestoreTagsCheckbox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(415, 379);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(112, 35);
            this.btnOK.TabIndex = 8;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(535, 379);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(112, 35);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lnkRetrieveLangs
            // 
            this.lnkRetrieveLangs.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lnkRetrieveLangs.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkRetrieveLangs.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.lnkRetrieveLangs.Location = new System.Drawing.Point(12, 157);
            this.lnkRetrieveLangs.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lnkRetrieveLangs.Name = "lnkRetrieveLangs";
            this.lnkRetrieveLangs.Size = new System.Drawing.Size(564, 31);
            this.lnkRetrieveLangs.TabIndex = 4;
            this.lnkRetrieveLangs.TabStop = true;
            this.lnkRetrieveLangs.Text = "Retrieve language pair information";
            this.lnkRetrieveLangs.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkRetrieveLangs_LinkClicked);
            // 
            // lbLanguages
            // 
            this.lbLanguages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbLanguages.FormattingEnabled = true;
            this.lbLanguages.ItemHeight = 20;
            this.lbLanguages.Location = new System.Drawing.Point(18, 245);
            this.lbLanguages.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lbLanguages.Name = "lbLanguages";
            this.lbLanguages.Size = new System.Drawing.Size(749, 124);
            this.lbLanguages.TabIndex = 6;
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.progressBar.Location = new System.Drawing.Point(18, 384);
            this.progressBar.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(389, 25);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.TabIndex = 7;
            this.progressBar.Visible = false;
            // 
            // lblSupportedLanguages
            // 
            this.lblSupportedLanguages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSupportedLanguages.Location = new System.Drawing.Point(12, 190);
            this.lblSupportedLanguages.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSupportedLanguages.Name = "lblSupportedLanguages";
            this.lblSupportedLanguages.Size = new System.Drawing.Size(755, 53);
            this.lblSupportedLanguages.TabIndex = 5;
            this.lblSupportedLanguages.Text = "Local language pairs (additional language pairs can be installed in the OPUS-CAT " +
    "MT Engine application)";
            // 
            // btnHelp
            // 
            this.btnHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnHelp.Location = new System.Drawing.Point(655, 379);
            this.btnHelp.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(112, 35);
            this.btnHelp.TabIndex = 10;
            this.btnHelp.Text = "&Help";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // mtServicePortLabel
            // 
            this.mtServicePortLabel.AutoSize = true;
            this.mtServicePortLabel.Location = new System.Drawing.Point(12, 90);
            this.mtServicePortLabel.Name = "mtServicePortLabel";
            this.mtServicePortLabel.Size = new System.Drawing.Size(444, 20);
            this.mtServicePortLabel.TabIndex = 11;
            this.mtServicePortLabel.Text = "Port (must  be the same as the port in OPUS-CAT MT Engine)";
            // 
            // mtServicePortTextBox
            // 
            this.mtServicePortTextBox.Location = new System.Drawing.Point(645, 84);
            this.mtServicePortTextBox.Name = "mtServicePortTextBox";
            this.mtServicePortTextBox.Size = new System.Drawing.Size(123, 26);
            this.mtServicePortTextBox.TabIndex = 12;
            // 
            // instructionTextBox
            // 
            this.instructionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.instructionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.instructionTextBox.Location = new System.Drawing.Point(18, 12);
            this.instructionTextBox.Multiline = true;
            this.instructionTextBox.Name = "instructionTextBox";
            this.instructionTextBox.ReadOnly = true;
            this.instructionTextBox.Size = new System.Drawing.Size(750, 66);
            this.instructionTextBox.TabIndex = 13;
            this.instructionTextBox.Text = resources.GetString("instructionTextBox.Text");
            // 
            // RestoreTagsCheckbox
            // 
            this.RestoreTagsCheckbox.AutoSize = true;
            this.RestoreTagsCheckbox.Location = new System.Drawing.Point(17, 126);
            this.RestoreTagsCheckbox.Name = "RestoreTagsCheckbox";
            this.RestoreTagsCheckbox.Size = new System.Drawing.Size(231, 24);
            this.RestoreTagsCheckbox.TabIndex = 14;
            this.RestoreTagsCheckbox.Text = "Restore tags (experimental)";
            this.RestoreTagsCheckbox.UseVisualStyleBackColor = true;
            // 
            // OpusCatMTOptionsForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(780, 428);
            this.Controls.Add(this.RestoreTagsCheckbox);
            this.Controls.Add(this.instructionTextBox);
            this.Controls.Add(this.mtServicePortTextBox);
            this.Controls.Add(this.mtServicePortLabel);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.lblSupportedLanguages);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lbLanguages);
            this.Controls.Add(this.lnkRetrieveLangs);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OpusCatMTOptionsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = " ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OpusCatMTOptionsForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.LinkLabel lnkRetrieveLangs;
        private System.Windows.Forms.ListBox lbLanguages;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblSupportedLanguages;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Label mtServicePortLabel;
        private System.Windows.Forms.TextBox mtServicePortTextBox;
        private System.Windows.Forms.TextBox instructionTextBox;
        private System.Windows.Forms.CheckBox RestoreTagsCheckbox;
    }
}