using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace OpusCatMTEngine
{

    public class Terminology
    {
        [YamlMember(Alias = "terms", ApplyNamingConventions = false)]
        public ObservableCollection<Term> Terms { get; set; }

        [YamlMember(Alias = "terminology-name", ApplyNamingConventions = false)]
        public string TerminologyName { get; set; }

        [YamlMember(Alias = "terminology-guid", ApplyNamingConventions = false)]
        public string TerminologyGuid;
        
        [YamlMember(Alias = "global-terminology", ApplyNamingConventions = false)]
        public Boolean GlobalTerminology { get; set; }
        
    }
}