using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpusCatMTEngine
{
    internal class SentencePiecePreprocessor : IPreprocessor
    {
        dynamic sentencePieceProcessor;
        public SentencePiecePreprocessor(string spmPath)
        {
            using (Py.GIL())
            {
                dynamic sentencepiece = PythonEngine.ImportModule("sentencepiece");
                this.sentencePieceProcessor = sentencepiece.SentencePieceProcessor(spmPath);
            }
        }

        public string PostprocessSentence(string rawTranslation)
        {
            return rawTranslation.Replace(" ", "").Replace("▁", " ").Trim();
        }

        //Term symbols are added before segmentation, they need to be desegmented
        private string FixTerminologySymbols(string segmentedSentence)
        {
            var termsSymbols = new List<string>() { "<term_start>", "<term_mask>", "<term_end>", "<trans_end>" };
            var wordsInSegmented = segmentedSentence.Split('▁');
            foreach (var word in wordsInSegmented)
            {
                var unsegmentedWord = word.Replace(" ", "");
                if (termsSymbols.Contains(unsegmentedWord))
                {
                    segmentedSentence = segmentedSentence.Replace($"▁{word}", $"{unsegmentedWord} ");
                }
            }
            
            return segmentedSentence;
        }

        public string PreprocessSentence(string sentence)
        {
            string preprocessedSentence;
            using (Py.GIL())
            {
                var preprocessedSentenceArray = (string[])this.sentencePieceProcessor.encode(sentence, out_type: "str");
                preprocessedSentence = String.Join(" ", preprocessedSentenceArray);
            }

            preprocessedSentence = this.FixTerminologySymbols(preprocessedSentence);

            return preprocessedSentence;
        }
    }
}
