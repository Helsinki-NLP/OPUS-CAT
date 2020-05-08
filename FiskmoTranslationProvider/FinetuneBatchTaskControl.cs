using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sdl.Desktop.IntegrationApi;

namespace FiskmoTranslationProvider
{
    public partial class FinetuneBatchTaskControl : UserControl, ISettingsAware<FinetuneBatchTaskSettings>
    {
        private FinetuneBatchTaskSettings settings;
        private FinetuneWpfControl wpfControl;

        public FinetuneBatchTaskControl()
        {

            InitializeComponent();
            this.wpfControl = new FinetuneWpfControl();

            this.fineTuneControlHost.Child = this.wpfControl;
        }

        public FinetuneBatchTaskSettings Settings { get => (FinetuneBatchTaskSettings)this.wpfControl.DataContext;  set => this.wpfControl.DataContext = value; }
        
    }
}
