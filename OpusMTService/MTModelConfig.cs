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

        [YamlMember(Alias = "tag-method", ApplyNamingConventions = false)]
        public TagMethod TagMethod { get => tagMethod; set => tagMethod = value; }

        [YamlMember(Alias = "model-tags", ApplyNamingConventions = false)]
        public ObservableCollection<string> ModelTags = new ObservableCollection<string>();
        private TagMethod tagMethod;

        public MTModelConfig()
        {
            this.TagMethod = TagMethod.Remove;
            this.ModelTags = new ObservableCollection<string>();
        }
    }
}
