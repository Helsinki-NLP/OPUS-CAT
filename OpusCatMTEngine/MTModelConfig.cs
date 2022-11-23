using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace OpusCatMTEngine
{
    public class MTModelConfig : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        [YamlMember(Alias = "model-tags", ApplyNamingConventions = false)]
        public ObservableCollection<string> ModelTags = new ObservableCollection<string>();
        
        [YamlMember(Alias = "include-placeholder-tags", ApplyNamingConventions = false)]
        public bool IncludePlaceholderTags { get => includePlaceholderTags; set => includePlaceholderTags = value; }
        private bool includePlaceholderTags;

        [YamlMember(Alias = "include-tag-pairs", ApplyNamingConventions = false)]
        public bool IncludeTagPairs { get => includeTagPairs; set => includeTagPairs = value; }
        private bool includeTagPairs;
        
        [YamlMember(Alias = "finetuned", ApplyNamingConventions = false)]
        public bool Finetuned { get => finetuned; set { finetuned = value; } }
        private bool finetuned;

        //These are not used, they are for backwards-compatibility
        [YamlMember(Alias = "finetuning-initiated", ApplyNamingConventions = false)]
        public bool FinetuningInitiated { get => finetuningInitiated; set { finetuningInitiated = value; NotifyPropertyChanged(); } }
        private bool finetuningInitiated;

        [YamlMember(Alias = "finetuning-complete", ApplyNamingConventions = false)]
        public bool FinetuningComplete { get => finetuningComplete; set { finetuningComplete = value; NotifyPropertyChanged(); } }

        [YamlMember(Alias = "auto-pre-edit-rule-collection-guids", ApplyNamingConventions = false)]
        public ObservableCollection<string> AutoPreEditRuleCollectionGuids { get; internal set; }

        [YamlMember(Alias = "auto-post-edit-rule-collection-guids", ApplyNamingConventions = false)]
        public ObservableCollection<string> AutoPostEditRuleCollectionGuids { get; internal set; }

        [YamlMember(Alias = "terminology-guid", ApplyNamingConventions = false)]
        public string TerminologyGuid { get; internal set; }

        [YamlMember(Alias = "source-languages", ApplyNamingConventions = false)]
        public ObservableCollection<string> SourceLanguageCodes { get; internal set; }

        [YamlMember(Alias = "target-languages", ApplyNamingConventions = false)]
        public ObservableCollection<string> TargetLanguageCodes { get; internal set; }

        private bool finetuningComplete;

        public MTModelConfig()
        {
            this.ModelTags = new ObservableCollection<string>();
            this.AutoPostEditRuleCollectionGuids = new ObservableCollection<string>();
            this.AutoPreEditRuleCollectionGuids = new ObservableCollection<string>();
        }

    }
}
