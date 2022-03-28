﻿

using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace OpusCatMTEngine
{
    
    public class AutoEditRule
    {
        [YamlMember(Alias = "output-pattern", ApplyNamingConventions = false)]
        public string OutputPattern { get; set; }

        [YamlMember(Alias = "replacement", ApplyNamingConventions = false)]
        public string Replacement { get; set; }

        [YamlMember(Alias = "source-pattern", ApplyNamingConventions = false)]
        public string SourcePattern { get; set; }

        //The regexes will be called over and over, so use the same instantation to prevent
        //regex recompilation
        private Regex outputPatternRegex;
        [YamlIgnore]
        public Regex OutputPatternRegex
        {
            get
            {
                if (this.outputPatternRegex == null)
                {
                    if (this.OutputPattern != null)
                    {
                        this.outputPatternRegex = new Regex(this.OutputPattern);
                    }
                    else
                    {
                        return null;
                    }
                }
                return outputPatternRegex;
            }
        }

        private Regex sourcePatternRegex;

        [YamlIgnore]
        public Regex SourcePatternRegex
        {
            get
            {
                if (this.sourcePatternRegex == null)
                {
                    if (this.SourcePattern != null)
                    {
                        this.sourcePatternRegex = new Regex(this.SourcePattern);
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