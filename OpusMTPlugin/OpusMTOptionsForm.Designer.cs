namespace OpusMTPlugin
{
    partial class OpusMTOptionsForm
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lnkRetrieveLangs = new System.Windows.Forms.LinkLabel();
            this.lbLanguages = new System.Windows.Forms.ListBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblSupportedLanguages = new System.Windows.Forms.Label();
            this.btnHelp = new System.Windows.Forms.Button();
            this.mtServicePort = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(327, 380);
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
            this.btnCancel.Location = new System.Drawing.Point(447, 380);
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
            this.lnkRetrieveLangs.Location = new System.Drawing.Point(12, 143);
            this.lnkRetrieveLangs.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lnkRetrieveLangs.Name = "lnkRetrieveLangs";
            this.lnkRetrieveLangs.Size = new System.Drawing.Size(564, 31);
            this.lnkRetrieveLangs.TabIndex = 4;
            this.lnkRetrieveLangs.TabStop = true;
            this.lnkRetrieveLangs.Text = "Retrieve language information";
            this.lnkRetrieveLangs.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkRetrieveLangs_LinkClicked);
            // 
            // lbLanguages
            // 
            this.lbLanguages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbLanguages.FormattingEnabled = true;
            this.lbLanguages.ItemHeight = 20;
            this.lbLanguages.Location = new System.Drawing.Point(18, 205);
            this.lbLanguages.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lbLanguages.Name = "lbLanguages";
            this.lbLanguages.Size = new System.Drawing.Size(660, 164);
            this.lbLanguages.TabIndex = 6;
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.progressBar.Location = new System.Drawing.Point(18, 389);
            this.progressBar.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(297, 18);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.TabIndex = 7;
            this.progressBar.Visible = false;
            // 
            // lblSupportedLanguages
            // 
            this.lblSupportedLanguages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSupportedLanguages.Location = new System.Drawing.Point(12, 174);
            this.lblSupportedLanguages.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSupportedLanguages.Name = "lblSupportedLanguages";
            this.lblSupportedLanguages.Size = new System.Drawing.Size(564, 25);
            this.lblSupportedLanguages.TabIndex = 5;
            this.lblSupportedLanguages.Text = "Supported language pairs";
            // 
            // btnHelp
            // 
            this.btnHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnHelp.Location = new System.Drawing.Point(567, 380);
            this.btnHelp.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(112, 35);
            this.btnHelp.TabIndex = 10;
            this.btnHelp.Text = "&Help";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // mtServicePort
            // 
            this.mtServicePort.AutoSize = true;
            this.mtServicePort.Location = new System.Drawing.Point(14, 108);
            this.mtServicePort.Name = "mtServicePort";
            this.mtServicePort.Size = new System.Drawing.Size(158, 20);
            this.mtServicePort.TabIndex = 11;
            this.mtServicePort.Text = "Opus MT service port";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(181, 105);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 26);
            this.textBox1.TabIndex = 12;
            // 
            // OpusMTOptionsForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(692, 428);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.mtServicePort);
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
            this.Name = "OpusMTOptionsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = " ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OpusMTOptionsForm_FormClosing);
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
        private System.Windows.Forms.Label mtServicePort;
        private System.Windows.Forms.TextBox textBox1;
    }
}