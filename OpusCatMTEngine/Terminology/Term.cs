

using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace OpusCatMTEngine
{

    public class Term
    {
        
        [YamlMember(Alias = "source-pattern", ApplyNamingConventions = false)]
        public string SourcePattern { get; set; }

        [YamlMember(Alias = "target-lemma", ApplyNamingConventions = false)]
        public string TargetLemma { get; set; }

        [YamlMember(Alias = "source-pattern-is-regex", ApplyNamingConventions = false)]
        public bool SourcePatternIsRegex { get; set; }

        [YamlMember(Alias = "description", ApplyNamingConventions = false)]
        public string Description
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(description))
                {
                    return description;
                }
                else
                {
                    return $"";
                }
            }
            set => description = value;
        }
        
        private Regex sourcePatternRegex;
        private string description;

        [YamlIgnore]
        public Regex SourcePatternRegex
        {
            get
            {
                if (this.sourcePatternRegex == null)
                {
                    if (this.SourcePattern != null)
                    {
                        if (this.SourcePatternIsRegex)
                        {
                            this.sourcePatternRegex = new Regex(this.SourcePattern);
                        }
                        else
                        {
                            this.sourcePatternRegex = new Regex(Regex.Escape(this.SourcePattern));
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                return sourcePatternRegex;
            }
        }

    }
}