using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpusCatMTEngine
{
    public class AutoEditRuleMatch
    {
        private string output;

        public AutoEditRuleMatch(AutoEditRule rule, Match match, int matchIndex)
        {
            this.Rule = rule;
            this.Match = match;
            this.MatchIndex = matchIndex;
            //This is used to prevent the repetetion of the source matches in cases where source pattern
            //is triggered and there are not enough source matches for each target match (the usual scenario)
            this.RepeatedSourceMatch = false;
        }

        public AutoEditRule Rule { get; }
        public Match Match { get; }
        public Match SourceMatch { get; set; }

        public string Output
        {
            get
            {
                if (String.IsNullOrEmpty(output))
                {
                    return this.Match.Result(this.Rule.Replacement);
                }
                else
                {
                    return output;
                }
            }

            set { output = value; }
        }

        public int OutputIndex { get; set; }
        public int OutputLength { get; set; }
        
        public int MatchIndex { get; }
        public bool RepeatedSourceMatch { get; internal set; }
    }
}
