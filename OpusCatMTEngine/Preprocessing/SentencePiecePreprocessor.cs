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

        public string PreprocessSentence(string sentence)
        {
            string preprocessedSentence;
            using (Py.GIL())
            {
                var preprocessedSentenceArray = (string[])this.sentencePieceProcessor.encode(sentence, out_type: "str");
                preprocessedSentence = String.Join(" ", preprocessedSentenceArray);
            }
            return preprocessedSentence;
        }
    }
}
