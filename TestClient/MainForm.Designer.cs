namespace MT_SDK
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.lbPlugins = new System.Windows.Forms.ListBox();
            this.gbPluginDetails = new System.Windows.Forms.GroupBox();
            this.lblStoringTranslationsValue = new System.Windows.Forms.Label();
            this.lblStoringTranslations = new System.Windows.Forms.Label();
            this.lblEnabled = new System.Windows.Forms.Label();
            this.lblConfiguredValue = new System.Windows.Forms.Label();
            this.lblConfigured = new System.Windows.Forms.Label();
            this.cbEnabled = new System.Windows.Forms.CheckBox();
            this.lnkConfigure = new System.Windows.Forms.LinkLabel();
            this.lblBatchLookupValue = new System.Windows.Forms.Label();
            this.lblBatchLookup = new System.Windows.Forms.Label();
            this.lblInteractiveLookupValue = new System.Windows.Forms.Label();
            this.lblInteractiveLookup = new System.Windows.Forms.Label();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.lblCopyrightValue = new System.Windows.Forms.Label();
            this.lblNameValue = new System.Windows.Forms.Label();
            this.lblCopyright = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.gbTranslation = new System.Windows.Forms.GroupBox();
            this.btnStoreTranslation = new System.Windows.Forms.Button();
            this.lblMessage = new System.Windows.Forms.Label();
            this.btnTranslate = new System.Windows.Forms.Button();
            this.tbTarget = new System.Windows.Forms.TextBox();
            this.tbSource = new System.Windows.Forms.TextBox();
            this.cbTargetLanguage = new System.Windows.Forms.ComboBox();
            this.cbSourceLanguage = new System.Windows.Forms.ComboBox();
            this.gbPluginDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.gbTranslation.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbPlugins
            // 
            this.lbPlugins.FormattingEnabled = true;
            this.lbPlugins.Location = new System.Drawing.Point(12, 8);
            this.lbPlugins.Name = "lbPlugins";
            this.lbPlugins.Size = new System.Drawing.Size(680, 95);
            this.lbPlugins.TabIndex = 0;
            this.lbPlugins.SelectedIndexChanged += new System.EventHandler(this.lbPlugins_SelectedIndexChanged);
            // 
            // gbPluginDetails
            // 
            this.gbPluginDetails.Controls.Add(this.lblStoringTranslationsValue);
            this.gbPluginDetails.Controls.Add(this.lblStoringTranslations);
            this.gbPluginDetails.Controls.Add(this.lblEnabled);
            this.gbPluginDetails.Controls.Add(this.lblConfiguredValue);
            this.gbPluginDetails.Controls.Add(this.lblConfigured);
            this.gbPluginDetails.Controls.Add(this.cbEnabled);
            this.gbPluginDetails.Controls.Add(this.lnkConfigure);
            this.gbPluginDetails.Controls.Add(this.lblBatchLookupValue);
            this.gbPluginDetails.Controls.Add(this.lblBatchLookup);
            this.gbPluginDetails.Controls.Add(this.lblInteractiveLookupValue);
            this.gbPluginDetails.Controls.Add(this.lblInteractiveLookup);
            this.gbPluginDetails.Controls.Add(this.pictureBox);
            this.gbPluginDetails.Controls.Add(this.lblCopyrightValue);
            this.gbPluginDetails.Controls.Add(this.lblNameValue);
            this.gbPluginDetails.Controls.Add(this.lblCopyright);
            this.gbPluginDetails.Controls.Add(this.lblName);
            this.gbPluginDetails.Location = new System.Drawing.Point(12, 112);
            this.gbPluginDetails.Name = "gbPluginDetails";
            this.gbPluginDetails.Size = new System.Drawing.Size(680, 158);
            this.gbPluginDetails.TabIndex = 2;
            this.gbPluginDetails.TabStop = false;
            this.gbPluginDetails.Text = "Plugin details";
            // 
            // lblStoringTranslationsValue
            // 
            this.lblStoringTranslationsValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblStoringTranslationsValue.Location = new System.Drawing.Point(104, 99);
            this.lblStoringTranslationsValue.Name = "lblStoringTranslationsValue";
            this.lblStoringTranslationsValue.Size = new System.Drawing.Size(480, 16);
            this.lblStoringTranslationsValue.TabIndex = 14;
            // 
            // lblStoringTranslations
            // 
            this.lblStoringTranslations.Location = new System.Drawing.Point(4, 99);
            this.lblStoringTranslations.Name = "lblStoringTranslations";
            this.lblStoringTranslations.Size = new System.Drawing.Size(96, 16);
            this.lblStoringTranslations.TabIndex = 13;
            this.lblStoringTranslations.Text = "Storing translations";
            // 
            // lblEnabled
            // 
            this.lblEnabled.Location = new System.Drawing.Point(4, 137);
            this.lblEnabled.Name = "lblEnabled";
            this.lblEnabled.Size = new System.Drawing.Size(96, 16);
            this.lblEnabled.TabIndex = 10;
            this.lblEnabled.Text = "Enabled";
            // 
            // lblConfiguredValue
            // 
            this.lblConfiguredValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblConfiguredValue.Location = new System.Drawing.Point(104, 116);
            this.lblConfiguredValue.Name = "lblConfiguredValue";
            this.lblConfiguredValue.Size = new System.Drawing.Size(480, 16);
            this.lblConfiguredValue.TabIndex = 9;
            // 
            // lblConfigured
            // 
            this.lblConfigured.Location = new System.Drawing.Point(4, 118);
            this.lblConfigured.Name = "lblConfigured";
            this.lblConfigured.Size = new System.Drawing.Size(96, 16);
            this.lblConfigured.TabIndex = 8;
            this.lblConfigured.Text = "Configured";
            // 
            // cbEnabled
            // 
            this.cbEnabled.AutoSize = true;
            this.cbEnabled.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbEnabled.Location = new System.Drawing.Point(104, 135);
            this.cbEnabled.Name = "cbEnabled";
            this.cbEnabled.Size = new System.Drawing.Size(15, 14);
            this.cbEnabled.TabIndex = 11;
            this.cbEnabled.UseVisualStyleBackColor = true;
            this.cbEnabled.CheckedChanged += new System.EventHandler(this.cbEnabled_CheckedChanged);
            // 
            // lnkConfigure
            // 
            this.lnkConfigure.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lnkConfigure.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkConfigure.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.lnkConfigure.Location = new System.Drawing.Point(612, 116);
            this.lnkConfigure.Name = "lnkConfigure";
            this.lnkConfigure.Size = new System.Drawing.Size(64, 20);
            this.lnkConfigure.TabIndex = 12;
            this.lnkConfigure.TabStop = true;
            this.lnkConfigure.Text = "Configure";
            this.lnkConfigure.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lnkConfigure.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkConfigure_LinkClicked);
            // 
            // lblBatchLookupValue
            // 
            this.lblBatchLookupValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblBatchLookupValue.Location = new System.Drawing.Point(104, 80);
            this.lblBatchLookupValue.Name = "lblBatchLookupValue";
            this.lblBatchLookupValue.Size = new System.Drawing.Size(480, 16);
            this.lblBatchLookupValue.TabIndex = 7;
            // 
            // lblBatchLookup
            // 
            this.lblBatchLookup.Location = new System.Drawing.Point(4, 79);
            this.lblBatchLookup.Name = "lblBatchLookup";
            this.lblBatchLookup.Size = new System.Drawing.Size(96, 16);
            this.lblBatchLookup.TabIndex = 6;
            this.lblBatchLookup.Text = "Batch lookup";
            // 
            // lblInteractiveLookupValue
            // 
            this.lblInteractiveLookupValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblInteractiveLookupValue.Location = new System.Drawing.Point(104, 60);
            this.lblInteractiveLookupValue.Name = "lblInteractiveLookupValue";
            this.lblInteractiveLookupValue.Size = new System.Drawing.Size(480, 16);
            this.lblInteractiveLookupValue.TabIndex = 5;
            // 
            // lblInteractiveLookup
            // 
            this.lblInteractiveLookup.Location = new System.Drawing.Point(4, 60);
            this.lblInteractiveLookup.Name = "lblInteractiveLookup";
            this.lblInteractiveLookup.Size = new System.Drawing.Size(96, 16);
            this.lblInteractiveLookup.TabIndex = 4;
            this.lblInteractiveLookup.Text = "Interactive lookup";
            // 
            // pictureBox
            // 
            this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox.Location = new System.Drawing.Point(624, 16);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(48, 48);
            this.pictureBox.TabIndex = 8;
            this.pictureBox.TabStop = false;
            // 
            // lblCopyrightValue
            // 
            this.lblCopyrightValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblCopyrightValue.Location = new System.Drawing.Point(104, 40);
            this.lblCopyrightValue.Name = "lblCopyrightValue";
            this.lblCopyrightValue.Size = new System.Drawing.Size(480, 16);
            this.lblCopyrightValue.TabIndex = 3;
            // 
            // lblNameValue
            // 
            this.lblNameValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblNameValue.Location = new System.Drawing.Point(104, 20);
            this.lblNameValue.Name = "lblNameValue";
            this.lblNameValue.Size = new System.Drawing.Size(480, 16);
            this.lblNameValue.TabIndex = 1;
            // 
            // lblCopyright
            // 
            this.lblCopyright.Location = new System.Drawing.Point(4, 40);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(96, 16);
            this.lblCopyright.TabIndex = 2;
            this.lblCopyright.Text = "Copyright";
            // 
            // lblName
            // 
            this.lblName.Location = new System.Drawing.Point(4, 20);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(96, 16);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "Name";
            // 
            // gbTranslation
            // 
            this.gbTranslation.Controls.Add(this.btnStoreTranslation);
            this.gbTranslation.Controls.Add(this.lblMessage);
            this.gbTranslation.Controls.Add(this.btnTranslate);
            this.gbTranslation.Controls.Add(this.tbTarget);
            this.gbTranslation.Controls.Add(this.tbSource);
            this.gbTranslation.Controls.Add(this.cbTargetLanguage);
            this.gbTranslation.Controls.Add(this.cbSourceLanguage);
            this.gbTranslation.Location = new System.Drawing.Point(8, 276);
            this.gbTranslation.Name = "gbTranslation";
            this.gbTranslation.Size = new System.Drawing.Size(684, 220);
            this.gbTranslation.TabIndex = 2;
            this.gbTranslation.TabStop = false;
            this.gbTranslation.Text = "Translation";
            // 
            // btnStoreTranslation
            // 
            this.btnStoreTranslation.Enabled = false;
            this.btnStoreTranslation.Location = new System.Drawing.Point(274, 48);
            this.btnStoreTranslation.Name = "btnStoreTranslation";
            this.btnStoreTranslation.Size = new System.Drawing.Size(136, 23);
            this.btnStoreTranslation.TabIndex = 6;
            this.btnStoreTranslation.Text = "Store translation";
            this.btnStoreTranslation.UseVisualStyleBackColor = true;
            this.btnStoreTranslation.Click += new System.EventHandler(this.btnStoreTranslation_Click);
            // 
            // lblMessage
            // 
            this.lblMessage.ForeColor = System.Drawing.Color.Black;
            this.lblMessage.Location = new System.Drawing.Point(277, 79);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(131, 125);
            this.lblMessage.TabIndex = 4;
            this.lblMessage.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btnTranslate
            // 
            this.btnTranslate.Enabled = false;
            this.btnTranslate.Location = new System.Drawing.Point(274, 19);
            this.btnTranslate.Name = "btnTranslate";
            this.btnTranslate.Size = new System.Drawing.Size(136, 23);
            this.btnTranslate.TabIndex = 3;
            this.btnTranslate.Text = "Translate";
            this.btnTranslate.UseVisualStyleBackColor = true;
            this.btnTranslate.Click += new System.EventHandler(this.btnTranslate_Click);
            // 
            // tbTarget
            // 
            this.tbTarget.BackColor = System.Drawing.SystemColors.Window;
            this.tbTarget.Location = new System.Drawing.Point(420, 48);
            this.tbTarget.Multiline = true;
            this.tbTarget.Name = "tbTarget";
            this.tbTarget.ReadOnly = true;
            this.tbTarget.Size = new System.Drawing.Size(255, 156);
            this.tbTarget.TabIndex = 5;
            // 
            // tbSource
            // 
            this.tbSource.Location = new System.Drawing.Point(8, 48);
            this.tbSource.Multiline = true;
            this.tbSource.Name = "tbSource";
            this.tbSource.Size = new System.Drawing.Size(255, 156);
            this.tbSource.TabIndex = 2;
            // 
            // cbTargetLanguage
            // 
            this.cbTargetLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTargetLanguage.FormattingEnabled = true;
            this.cbTargetLanguage.Location = new System.Drawing.Point(420, 20);
            this.cbTargetLanguage.Name = "cbTargetLanguage";
            this.cbTargetLanguage.Size = new System.Drawing.Size(255, 21);
            this.cbTargetLanguage.TabIndex = 1;
            this.cbTargetLanguage.SelectedIndexChanged += new System.EventHandler(this.cbLanguages_SelectedIndexChanged);
            // 
            // cbSourceLanguage
            // 
            this.cbSourceLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSourceLanguage.FormattingEnabled = true;
            this.cbSourceLanguage.Location = new System.Drawing.Point(8, 20);
            this.cbSourceLanguage.Name = "cbSourceLanguage";
            this.cbSourceLanguage.Size = new System.Drawing.Size(255, 21);
            this.cbSourceLanguage.TabIndex = 0;
            this.cbSourceLanguage.SelectedIndexChanged += new System.EventHandler(this.cbLanguages_SelectedIndexChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(705, 506);
            this.Controls.Add(this.gbTranslation);
            this.Controls.Add(this.gbPluginDetails);
            this.Controls.Add(this.lbPlugins);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "MT SDK test client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.gbPluginDetails.ResumeLayout(false);
            this.gbPluginDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.gbTranslation.ResumeLayout(false);
            this.gbTranslation.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbPlugins;
        private System.Windows.Forms.GroupBox gbPluginDetails;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblCopyright;
        private System.Windows.Forms.Label lblCopyrightValue;
        private System.Windows.Forms.Label lblNameValue;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Label lblInteractiveLookupValue;
        private System.Windows.Forms.Label lblInteractiveLookup;
        private System.Windows.Forms.Label lblBatchLookupValue;
        private System.Windows.Forms.Label lblBatchLookup;
        private System.Windows.Forms.CheckBox cbEnabled;
        private System.Windows.Forms.LinkLabel lnkConfigure;
        private System.Windows.Forms.Label lblConfiguredValue;
        private System.Windows.Forms.Label lblConfigured;
        private System.Windows.Forms.Label lblEnabled;
        private System.Windows.Forms.GroupBox gbTranslation;
        private System.Windows.Forms.ComboBox cbTargetLanguage;
        private System.Windows.Forms.ComboBox cbSourceLanguage;
        private System.Windows.Forms.TextBox tbTarget;
        private System.Windows.Forms.TextBox tbSource;
        private System.Windows.Forms.Button btnTranslate;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Button btnStoreTranslation;
        private System.Windows.Forms.Label lblStoringTranslationsValue;
        private System.Windows.Forms.Label lblStoringTranslations;
    }
}

