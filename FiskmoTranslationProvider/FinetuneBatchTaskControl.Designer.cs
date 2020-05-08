namespace FiskmoTranslationProvider
{
    partial class FinetuneBatchTaskControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.fineTuneControlHost = new System.Windows.Forms.Integration.ElementHost();
            this.SuspendLayout();
            // 
            // fineTuneControlHost
            // 
            this.fineTuneControlHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fineTuneControlHost.Location = new System.Drawing.Point(0, 0);
            this.fineTuneControlHost.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.fineTuneControlHost.Name = "fineTuneControlHost";
            this.fineTuneControlHost.Size = new System.Drawing.Size(225, 231);
            this.fineTuneControlHost.TabIndex = 0;
            this.fineTuneControlHost.Text = "fineTuneControlHost";
            this.fineTuneControlHost.Child = null;
            // 
            // FinetuneBatchTaskControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.fineTuneControlHost);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FinetuneBatchTaskControl";
            this.Size = new System.Drawing.Size(225, 231);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost fineTuneControlHost;
    
    }
}
