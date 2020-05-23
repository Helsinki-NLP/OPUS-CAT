using Sdl.Core.Settings;
using Sdl.Desktop.IntegrationApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiskmoTranslationProvider
{
    public class FinetuneBatchTaskSettingsPage : DefaultSettingsPage<FinetuneBatchTaskControl, FinetuneBatchTaskSettings>
    {

        private FinetuneBatchTaskSettings settings;
		
		private FinetuneBatchTaskControl control;
		public override object GetControl()
		{
			this.settings = ((ISettingsBundle)DataSource).GetSettingsGroup<FinetuneBatchTaskSettings>();
			this.control = base.GetControl() as FinetuneBatchTaskControl;
			if (this.control != null)
			{
				this.control.Settings = this.settings;
			}
			return this.control;
		}
	}
}
