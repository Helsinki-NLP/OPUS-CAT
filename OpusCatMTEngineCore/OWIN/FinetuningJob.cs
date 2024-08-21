using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpusCatMtEngine
{
    public class FinetuningJob
    {
        public List<ParallelSentence> Input { get; set; }
        public List<ParallelSentence> Validation { get; set; }
        public List<string> UniqueNewSegments { get; set; }
        public bool IncludePlaceholderTags { get; set; }
        public bool IncludeTagPairs { get; set; }
        
        public FinetuningJob(
            List<ParallelSentence> input, 
            List<ParallelSentence> validation, 
            List<string> uniqueNewSegments, 
            bool includePlaceholderTags, 
            bool includeTagPairs)
        {
            this.Input = input;
            this.Validation = validation;
            this.UniqueNewSegments = uniqueNewSegments;
            this.IncludePlaceholderTags = includePlaceholderTags;
            this.IncludeTagPairs = includeTagPairs;
        }
    }
}
