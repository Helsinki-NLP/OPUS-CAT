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
using Sdl.Desktop.IntegrationApi.Interfaces;

namespace OpusCatTranslationProvider
{
    public partial class FinetuneBatchTaskControl : UserControl, ISettingsAware<FinetuneBatchTaskSettings>
    #if TRADOS21
    , IUISettingsControl
    #endif
    {
        private FinetuneWpfControl wpfControl;

        public FinetuneBatchTaskControl()
        {
            InitializeComponent();
        }

        public FinetuneBatchTaskSettings Settings
        {
            get => (FinetuneBatchTaskSettings)this.wpfControl.Settings;
            set
            {
                this.wpfControl = new FinetuneWpfControl(value);
                this.fineTuneControlHost.Child = this.wpfControl;
            }
        }
    }
}
