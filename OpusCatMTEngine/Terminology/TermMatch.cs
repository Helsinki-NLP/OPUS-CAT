using System;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace OpusCatMTEngine
{

    public class TermMatch
    {

        public TermMatch(Term term, Match termMatch)
        {
            this.Term = term;

        }

        public TermMatch(Term term, int start, int length, bool lemmaMatch)
        {
            this.Term = term;
            this.Length = length;
            this.Start = start;
            this.LemmaMatch = lemmaMatch;
        }

        public Term Term { get; private set; }
        public int Length { get; private set; }
        public int Start { get; private set; }
        public bool LemmaMatch { get; private set; }
    }
}