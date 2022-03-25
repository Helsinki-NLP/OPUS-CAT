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
        public AutoEditRuleMatch(AutoEditRule rule, Match match, int matchIndex)
        {
            this.Rule = rule;
            this.Match = match;
            this.MatchIndex = matchIndex;
        }

        public AutoEditRule Rule { get; }
        public Match Match { get; }
        public Match SourceMatch { get; set; }
        public int OutputIndex { get; set; }
        public int OutputLength { get; set; }
        public string Output
        {
            get
            {
                return this.Match.Result(this.Rule.Replacement);
            }
        }
        public int MatchIndex { get; }
    }
}
