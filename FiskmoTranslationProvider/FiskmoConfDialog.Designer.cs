namespace FiskmoTranslationProvider
{   
    partial class FiskmoConfDialog
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
            this.components = new System.ComponentModel.Container();
            this.Save_btn = new System.Windows.Forms.Button();
            this.Cancel_btn = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip2 = new System.Windows.Forms.ToolTip(this.components);
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.infoBox = new System.Windows.Forms.RichTextBox();
            this.downloadProgress = new System.Windows.Forms.ProgressBar();
            this.Customize_btn = new System.Windows.Forms.Button();
            this.groupBoxCon = new System.Windows.Forms.GroupBox();
            this.mtServicePortLabel = new System.Windows.Forms.Label();
            this.mtOriginCheckbox = new System.Windows.Forms.CheckBox();
            this.pregenerateCheckbox = new System.Windows.Forms.CheckBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.aboutBox = new System.Windows.Forms.RichTextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.useAllModelsCheckBox = new System.Windows.Forms.CheckBox();
            this.maskedTextBox1 = new System.Windows.Forms.MaskedTextBox();
            this.tabPage1.SuspendLayout();
            this.groupBoxCon.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // Save_btn
            // 
            this.Save_btn.Location = new System.Drawing.Point(15, 295);
            this.Save_btn.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Save_btn.Name = "Save_btn";
            this.Save_btn.Size = new System.Drawing.Size(120, 35);
            this.Save_btn.TabIndex = 6;
            this.Save_btn.Text = "Save";
            this.Save_btn.UseVisualStyleBackColor = true;
            this.Save_btn.Click += new System.EventHandler(this.Save_btn_Click);
            // 
            // Cancel_btn
            // 
            this.Cancel_btn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_btn.Location = new System.Drawing.Point(389, 295);
            this.Cancel_btn.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Cancel_btn.Name = "Cancel_btn";
            this.Cancel_btn.Size = new System.Drawing.Size(120, 35);
            this.Cancel_btn.TabIndex = 7;
            this.Cancel_btn.Text = "Cancel";
            this.Cancel_btn.UseVisualStyleBackColor = true;
            this.Cancel_btn.Click += new System.EventHandler(this.Cancel_btn_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.infoBox);
            this.tabPage1.Controls.Add(this.downloadProgress);
            this.tabPage1.Controls.Add(this.Customize_btn);
            this.tabPage1.Controls.Add(this.groupBoxCon);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage1.Size = new System.Drawing.Size(490, 233);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Fiskmö settings";
            // 
            // infoBox
            // 
            this.infoBox.Location = new System.Drawing.Point(7, 115);
            this.infoBox.Name = "infoBox";
            this.infoBox.ReadOnly = true;
            this.infoBox.Size = new System.Drawing.Size(465, 69);
            this.infoBox.TabIndex = 28;
            this.infoBox.Text = "";
            // 
            // downloadProgress
            // 
            this.downloadProgress.Location = new System.Drawing.Point(10, 189);
            this.downloadProgress.Name = "downloadProgress";
            this.downloadProgress.Size = new System.Drawing.Size(354, 32);
            this.downloadProgress.TabIndex = 27;
            this.downloadProgress.Visible = false;
            // 
            // Customize_btn
            // 
            this.Customize_btn.Location = new System.Drawing.Point(370, 190);
            this.Customize_btn.Name = "Customize_btn";
            this.Customize_btn.Size = new System.Drawing.Size(102, 31);
            this.Customize_btn.TabIndex = 25;
            this.Customize_btn.Text = "Customize";
            this.Customize_btn.UseVisualStyleBackColor = true;
            this.Customize_btn.Click += new System.EventHandler(this.Customize_btn_Click);
            // 
            // groupBoxCon
            // 
            this.groupBoxCon.Controls.Add(this.maskedTextBox1);
            this.groupBoxCon.Controls.Add(this.mtServicePortLabel);
            this.groupBoxCon.Controls.Add(this.mtOriginCheckbox);
            this.groupBoxCon.Controls.Add(this.pregenerateCheckbox);
            this.groupBoxCon.Location = new System.Drawing.Point(7, 8);
            this.groupBoxCon.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBoxCon.Name = "groupBoxCon";
            this.groupBoxCon.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBoxCon.Size = new System.Drawing.Size(465, 100);
            this.groupBoxCon.TabIndex = 24;
            this.groupBoxCon.TabStop = false;
            this.groupBoxCon.Text = "Settings";
            // 
            // mtServicePortLabel
            // 
            this.mtServicePortLabel.AutoSize = true;
            this.mtServicePortLabel.Location = new System.Drawing.Point(6, 63);
            this.mtServicePortLabel.Name = "mtServicePortLabel";
            this.mtServicePortLabel.Size = new System.Drawing.Size(116, 20);
            this.mtServicePortLabel.TabIndex = 20;
            this.mtServicePortLabel.Text = "MT service port";
            // 
            // mtOriginCheckbox
            // 
            this.mtOriginCheckbox.AutoSize = true;
            this.mtOriginCheckbox.Checked = true;
            this.mtOriginCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mtOriginCheckbox.Location = new System.Drawing.Point(295, 26);
            this.mtOriginCheckbox.Name = "mtOriginCheckbox";
            this.mtOriginCheckbox.Size = new System.Drawing.Size(164, 24);
            this.mtOriginCheckbox.TabIndex = 19;
            this.mtOriginCheckbox.Text = "Show MT as origin";
            this.mtOriginCheckbox.UseVisualStyleBackColor = true;
            // 
            // pregenerateCheckbox
            // 
            this.pregenerateCheckbox.AutoSize = true;
            this.pregenerateCheckbox.Checked = true;
            this.pregenerateCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.pregenerateCheckbox.Location = new System.Drawing.Point(6, 26);
            this.pregenerateCheckbox.Name = "pregenerateCheckbox";
            this.pregenerateCheckbox.Size = new System.Drawing.Size(154, 24);
            this.pregenerateCheckbox.TabIndex = 18;
            this.pregenerateCheckbox.Text = "Pre-generate MT";
            this.pregenerateCheckbox.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(15, 15);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(498, 266);
            this.tabControl1.TabIndex = 23;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.aboutBox);
            this.tabPage2.Location = new System.Drawing.Point(4, 29);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(490, 233);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "About";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // aboutBox
            // 
            this.aboutBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aboutBox.Location = new System.Drawing.Point(0, 0);
            this.aboutBox.Name = "aboutBox";
            this.aboutBox.Size = new System.Drawing.Size(490, 233);
            this.aboutBox.TabIndex = 0;
            this.aboutBox.Text = "";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.useAllModelsCheckBox);
            this.tabPage3.Location = new System.Drawing.Point(4, 29);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(490, 233);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Advanced";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // useAllModelsCheckBox
            // 
            this.useAllModelsCheckBox.AutoSize = true;
            this.useAllModelsCheckBox.Checked = true;
            this.useAllModelsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useAllModelsCheckBox.Location = new System.Drawing.Point(6, 6);
            this.useAllModelsCheckBox.Name = "useAllModelsCheckBox";
            this.useAllModelsCheckBox.Size = new System.Drawing.Size(203, 24);
            this.useAllModelsCheckBox.TabIndex = 20;
            this.useAllModelsCheckBox.Text = "Use all available models";
            this.useAllModelsCheckBox.UseVisualStyleBackColor = true;
            // 
            // maskedTextBox1
            // 
            this.maskedTextBox1.Location = new System.Drawing.Point(128, 60);
            this.maskedTextBox1.Name = "maskedTextBox1";
            this.maskedTextBox1.Size = new System.Drawing.Size(103, 26);
            this.maskedTextBox1.TabIndex = 22;
            // 
            // FiskmoConfDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(526, 344);
            this.Controls.Add(this.Save_btn);
            this.Controls.Add(this.Cancel_btn);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FiskmoConfDialog";
            this.Text = "Fiskmö Configuration";
            this.Load += new System.EventHandler(this.FiskmoConfDialog_Load);
            this.tabPage1.ResumeLayout(false);
            this.groupBoxCon.ResumeLayout(false);
            this.groupBoxCon.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Cancel_btn;
        private System.Windows.Forms.Button Save_btn;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolTip toolTip2;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.GroupBox groupBoxCon;
        private System.Windows.Forms.CheckBox pregenerateCheckbox;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.Button Customize_btn;
        private System.Windows.Forms.ProgressBar downloadProgress;
        private System.Windows.Forms.RichTextBox infoBox;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.RichTextBox aboutBox;
        private System.Windows.Forms.CheckBox mtOriginCheckbox;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.CheckBox useAllModelsCheckBox;
        private System.Windows.Forms.Label mtServicePortLabel;
        private System.Windows.Forms.MaskedTextBox maskedTextBox1;
    }
}