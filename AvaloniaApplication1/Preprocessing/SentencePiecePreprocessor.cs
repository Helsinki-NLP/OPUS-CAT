using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpusCatMtEngine
{
    internal class SentencePiecePreprocessor : IPreprocessor
    {
        dynamic sentencePieceProcessor;
        dynamic targetSentencePieceProcessor;
        private Regex targetLemmaRegex;

        public List<Tuple<string, string>> TagRestorations { get; private set; }

        public SentencePiecePreprocessor(string spmPath, string targetSpmPath=null)
        {
            using (Py.GIL())
            {
                dynamic sentencepiece = Py.Import("sentencepiece");
                this.sentencePieceProcessor = sentencepiece.SentencePieceProcessor(spmPath);
                //Target spm is needed to segment target language lemmas when using soft term constraints
                if (targetSpmPath != null)
                {
                    this.targetSentencePieceProcessor = sentencepiece.SentencePieceProcessor(targetSpmPath);
                    this.targetLemmaRegex = new Regex("<term_end>(.*?)<trans_end>");
                }

                //Store the subwords for term tags to make it possible to restore them before translation
                this.TagRestorations = new List<Tuple<string, string>>();
                foreach (var termTag in new List<string>() {
                    "<term_start>", "<term_mask>", "<term_end>", "<trans_end>" })
                {
                    var tagSubwords = 
                        String.Join(" ",(string[])this.sentencePieceProcessor.encode_as_pieces(termTag));
                    this.TagRestorations.Add(new Tuple<string, string>(termTag, tagSubwords));
                }
                

            }
        }

        public string PostprocessSentence(string rawTranslation)
        {
            return rawTranslation.Replace(" ", "").Replace("▁", " ").Trim();
        }

        //Term symbols are added before segmentation, they need to be desegmented
        private string FixTerminologySymbols(string segmentedSentence)
        {
            foreach (var tagRestoration in this.TagRestorations)
            {
                segmentedSentence = segmentedSentence.Replace(tagRestoration.Item2, tagRestoration.Item1);
            }

            //Re-segment the term lemma with target spm model
            if (this.targetLemmaRegex != null)
            {
                var targetLemmaMatches = this.targetLemmaRegex.Matches(segmentedSentence);
                using (Py.GIL())
                {
                    foreach (Match match in targetLemmaMatches)
                    {
                        var desegmentedMatch = match.Groups[1].Value.Replace(" ", "").Replace("▁", " ").Trim();
                        var preprocessedLemmaArray = (string[])this.targetSentencePieceProcessor.encode(desegmentedMatch, out_type: "str");
                        segmentedSentence = segmentedSentence.Replace(
                            match.Value, $"<term_end> {String.Join(" ", preprocessedLemmaArray)} <trans_end>");
                    }
                }
            }

            return segmentedSentence;
        }

        public string PreprocessSentence(string sentence)
        {
            string preprocessedSentence;
            using (Py.GIL())
            {
                var preprocessedSentenceArray = (string[])this.sentencePieceProcessor.encode_as_pieces(sentence);
                preprocessedSentence = String.Join(" ", preprocessedSentenceArray);
            }

            preprocessedSentence = this.FixTerminologySymbols(preprocessedSentence);

            return preprocessedSentence;
        }
    }
}
