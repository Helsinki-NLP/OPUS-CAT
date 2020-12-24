using Sdl.Core.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OpusCatTranslationProvider
{
    public class FinetuneBatchTaskSettings : SettingsGroup, INotifyPropertyChanged, IDataErrorInfo
    {

        public string this[string columnName]
        {
            get
            {
                return Validate(columnName);
            }
        }

        public string Error
        {
            get { return "...."; }
        }

        private string Validate(string propertyName)
        {
            // Return error message if there is error on else return empty or null string
            string validationMessage = string.Empty;
            switch (propertyName)
            {
                case "MaxFinetuningSentences":
                    if (this.MaxFinetuningSentences != 0)
                    {
                        var maxSent = this.MaxFinetuningSentences;
                        if (maxSent < OpusCatTpSettings.Default.FinetuningMinSentencePairs ||
                            maxSent > OpusCatTpSettings.Default.FinetuningSentencePairsHardLimit)
                        {
                            validationMessage = "Error";
                        }
                    }
                    else
                    {
                        validationMessage = "Error";
                    }

                    break;
            }

            return validationMessage;
        }

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

        public bool IncludeTagPairs
        {
            get
            {
                return GetSetting<bool>(nameof(IncludeTagPairs));
            }
            set
            {
                GetSetting<bool>(nameof(IncludeTagPairs)).Value = value;
                NotifyPropertyChanged();
            }
        }

        public bool PreOrderMtForNewSegments
        {
            get
            {
                return GetSetting<bool>(nameof(PreOrderMtForNewSegments));
            }
            set
            {
                GetSetting<bool>(nameof(PreOrderMtForNewSegments)).Value = value;
                NotifyPropertyChanged();
            }
        }


        public string ProviderOptions
        {
            get
            {
                var settingValue = GetSetting<string>(nameof(ProviderOptions));
                if (settingValue == null || settingValue == "")
                {
                    return (new OpusCatOptions()).Uri.ToString();
                }
                else
                {
                    return settingValue;
                }
            }
            set
            {
                GetSetting<string>(nameof(ProviderOptions)).Value = value;
                NotifyPropertyChanged();
            }
        }

        public bool AddOpusCatProvider
        {
            get
            {
                return GetSetting<bool>(nameof(AddOpusCatProvider));
            }
            set
            {
                GetSetting<bool>(nameof(AddOpusCatProvider)).Value = value;
                NotifyPropertyChanged();
            }
        }

        public bool ExtractFuzzies
        {
            get
            {
                return GetSetting<bool>(nameof(ExtractFuzzies));
            }
            set
            {
                GetSetting<bool>(nameof(ExtractFuzzies)).Value = value;
                NotifyPropertyChanged();
            }
        }

        public bool ExtractConcordanceMatches
        {
            get
            {
                return GetSetting<bool>(nameof(ExtractConcordanceMatches));
            }
            set
            {
                GetSetting<bool>(nameof(ExtractConcordanceMatches)).Value = value;
                NotifyPropertyChanged();
            }
        }

        public bool ExtractFillerUnits
        {
            get
            {
                return GetSetting<bool>(nameof(ExtractFillerUnits));
            }
            set
            {
                GetSetting<bool>(nameof(ExtractFillerUnits)).Value = value;
                NotifyPropertyChanged();
            }
        }
        
        public int FuzzyMinPercentage
        {
            get
            {
                var settingValue = GetSetting<int>(nameof(FuzzyMinPercentage));
                if (settingValue == 0)
                {
                    return OpusCatTpSettings.Default.FinetuningMinFuzzyPercentage;
                }
                else
                {
                    return settingValue;
                }

            }
            set
            {
                GetSetting<int>(nameof(FuzzyMinPercentage)).Value = value;
                NotifyPropertyChanged();
            }
        }

        public int FuzzyMaxResults
        {
            get
            {
                var settingValue = GetSetting<int>(nameof(FuzzyMaxResults));
                if (settingValue == 0)
                {
                    return OpusCatTpSettings.Default.FinetuningMaxFuzzyResults;
                }
                else
                {
                    return settingValue;
                }
            }
            set
            {
                GetSetting<int>(nameof(FuzzyMaxResults)).Value = value;
                NotifyPropertyChanged();
            }
        }

        public int MaxConcordanceWindow
        {
            get
            {
                var settingValue = GetSetting<int>(nameof(MaxConcordanceWindow));
                if (settingValue == 0)
                {
                    return OpusCatTpSettings.Default.MaxConcordanceWindow;
                }
                else
                {
                    return settingValue;
                }
            }
            set
            {
                GetSetting<int>(nameof(MaxConcordanceWindow)).Value = value;
                NotifyPropertyChanged();
            }
        }

        public int ConcordanceMaxResults
        {
            get
            {
                var settingValue = GetSetting<int>(nameof(ConcordanceMaxResults));
                if (settingValue == 0)
                {
                    return OpusCatTpSettings.Default.FinetuningMaxConcordanceResults;
                }
                else
                {
                    return settingValue;
                }
            }
            set
            {
                GetSetting<int>(nameof(ConcordanceMaxResults)).Value = value;
                NotifyPropertyChanged();
            }
        }

        public int MaxFinetuningSentences
        {
            get
            {
                var settingValue = GetSetting<int>(nameof(MaxFinetuningSentences));
                if (settingValue == 0)
                {
                    return OpusCatTpSettings.Default.FinetuningMaxSentencePairs;
                }
                else
                {
                    return settingValue;
                }
            }
            set
            {
                GetSetting<int>(nameof(MaxFinetuningSentences)).Value = value;
                NotifyPropertyChanged();
            }
        }


    }
}
