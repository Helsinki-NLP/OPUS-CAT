

using System;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace OpusCatMTEngine
{

    public class Term
    {

        [YamlMember(Alias = "source-pattern", ApplyNamingConventions = false)]
        public string SourcePattern
        {
            get => _sourcePattern;
            set
            {
                _sourcePattern = value;
                this.UpdateSourcePatternRegex();
            }
        }

        [YamlMember(Alias = "target-lemma", ApplyNamingConventions = false)]
        public string TargetLemma { get; set; }

        [YamlMember(Alias = "source-pattern-is-regex", ApplyNamingConventions = false)]
        public bool SourcePatternIsRegex
        {
            get => _sourcePatternIsRegex;
            set
            {
                _sourcePatternIsRegex = value;
                this.UpdateSourcePatternRegex();
            }
        }

        private void UpdateSourcePatternRegex()
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
        }

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

        private bool _sourcePatternIsRegex;
        private string _sourcePattern;

        [YamlIgnore]
        public Regex SourcePatternRegex
        {
            get
            {
                return this.sourcePatternRegex;
            }
        }

    }
}