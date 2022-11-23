

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

        [YamlMember(Alias = "source-pattern-is-case-sensitive", ApplyNamingConventions = false)]
        public bool SourcePatternIsCaseSensitive
        {
            get => _sourcePatternIsCaseSensitive;
            set
            {
                _sourcePatternIsCaseSensitive = value;
                this.UpdateSourcePatternRegex();
            }
        }

        private void UpdateSourcePatternRegex()
        {
            if (this.SourcePattern != null)
            {
                RegexOptions sourcePatternOptions = RegexOptions.None;
                if (!this.SourcePatternIsCaseSensitive)
                {
                    sourcePatternOptions = RegexOptions.IgnoreCase;
                }

                if (this.SourcePatternIsRegex)
                {
                    this.sourcePatternRegex = new Regex($"\\b{this.SourcePattern}\\b",sourcePatternOptions);
                }
                else
                {
                    this.sourcePatternRegex = new Regex($"\\b{Regex.Escape(this.SourcePattern)}\\b", sourcePatternOptions);
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
        private bool _sourcePatternIsCaseSensitive;

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