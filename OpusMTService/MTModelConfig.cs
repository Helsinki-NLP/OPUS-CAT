using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace FiskmoMTEngine
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
        public bool Finetuned { get => finetuned; set { finetuned = value; NotifyPropertyChanged(); } }
        private bool finetuned;

        public MTModelConfig()
        {
            this.ModelTags = new ObservableCollection<string>();
        }

    }
}
