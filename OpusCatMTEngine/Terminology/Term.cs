

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace OpusCatMTEngine
{

    public class Term
    {

        public Term()
        {

        }

        public Term(
            string sourcePattern,
            string targetLemma,
            IsoLanguage sourceLang,
            IsoLanguage targetLang)
        {
            this.SourcePattern = sourcePattern;
            this.TargetLemma = targetLemma;
            this.SourceLanguageCode = sourceLang.ShortestIsoCode;
            this.TargetLanguageCode = sourceLang.ShortestIsoCode;
        }

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

        [YamlMember(Alias = "match-source-lemma", ApplyNamingConventions = false)]
        public bool MatchSourceLemma
        {
            get => _matchSourceLemma;
            set
            {
                _matchSourceLemma = value;
                //Nullify this to make sure a fresh lemma is generated when needed
                this.SourceLemmas = null;
            }
        }

        public List<string> SourceLemmas
        {
            get
            {
                if (_sourceLemmas == null)
                {
                    if (this.SourceLanguageCode != null)
                    {
                        _sourceLemmas = PythonNetHelper.Lemmatize(
                            this.SourceLanguageCode,
                            this.SourcePattern).Select(x => x.Item3).ToList();
                    }
                }
                return _sourceLemmas;
            }
            set => _sourceLemmas = value; }

        [YamlMember(Alias = "target-lemma", ApplyNamingConventions = false)]
        public string TargetLemma { get; set; }

        [YamlMember(Alias = "source-language-code", ApplyNamingConventions = false)]
        public string SourceLanguageCode { get; set; }

        [YamlMember(Alias = "target-language-code", ApplyNamingConventions = false)]
        public string TargetLanguageCode { get; set; }

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
                    this.sourcePatternRegex = new Regex($"\\b{this.SourcePattern}\\b", sourcePatternOptions);
                }
                else
                {
                    this.sourcePatternRegex = new Regex($"\\b{Regex.Escape(this.SourcePattern)}\\b", sourcePatternOptions);

                    //Nullify source lemma (it will be generated when requested)
                    this.SourceLemmas = null;
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
        private bool _matchSourceLemma;
        private List<string> _sourceLemmas;

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