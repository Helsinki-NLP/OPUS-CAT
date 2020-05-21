using Sdl.Core.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FiskmoTranslationProvider
{
    public class FinetuneBatchTaskSettings : SettingsGroup, INotifyPropertyChanged
    {

        public new event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public bool Finetune
        {
            get
            {
                return GetSetting<bool>(nameof(Finetune));
            }
            set
            {
                GetSetting<bool>(nameof(Finetune)).Value = value;
                NotifyPropertyChanged();
            }
        }

        public bool IncludePlaceholderTags
        {
            get
            {
                return GetSetting<bool>(nameof(IncludePlaceholderTags));
            }
            set
            {
                GetSetting<bool>(nameof(IncludePlaceholderTags)).Value = value;
                NotifyPropertyChanged();
            }
        }

        public bool BatchTranslate
        {
            get
            {
                return GetSetting<bool>(nameof(BatchTranslate));
            }
            set
            {
                GetSetting<bool>(nameof(BatchTranslate)).Value = value;
                NotifyPropertyChanged();
            }
        }


        public string ProviderOptions
        {
            get
            {
                return GetSetting<string>(nameof(ProviderOptions));
            }
            set
            {
                GetSetting<string>(nameof(ProviderOptions)).Value = value;
                NotifyPropertyChanged();
            }
        }

        public bool AddFiskmoProvider
        {
            get
            {
                return GetSetting<bool>(nameof(AddFiskmoProvider));
            }
            set
            {
                GetSetting<bool>(nameof(AddFiskmoProvider)).Value = value;
                NotifyPropertyChanged();
            }
        }
        
    }
}
