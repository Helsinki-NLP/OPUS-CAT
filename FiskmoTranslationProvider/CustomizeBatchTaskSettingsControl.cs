using Sdl.Desktop.IntegrationApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FiskmoTranslationProvider
{
    class CustomizeBatchTaskSettingsControl : UserControl, ISettingsAware<CustomizeBatchTaskSettings>
    {
        private CustomizeBatchTaskSettings settings;

        public CustomizeBatchTaskSettings Settings
        {
            get
            {
                return settings;
            }

            set
            {
                settings = value;
            }
        }
    }
}
