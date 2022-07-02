﻿using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpusCatMTEngine
{
    internal class MosesBpePreprocessor : IPreprocessor
    {
        dynamic bpeProcessor, trueCaser, tokenizer, deTokenizer;
        public MosesBpePreprocessor(
            string tcModelPath, string bpeModelPath, string sourceLang, string targetLang)
        {
            using (Py.GIL())
            {
                dynamic sacremoses = PythonEngine.ImportModule("sacremoses");
                dynamic subwordnmt = PythonEngine.ImportModule("subword_nmt.apply_bpe");
                dynamic io = PythonEngine.ImportModule("io");
                this.bpeProcessor = subwordnmt.BPE(io.open(bpeModelPath, encoding: "utf-8"));
                this.trueCaser = sacremoses.MosesTruecaser(tcModelPath);
                this.tokenizer = sacremoses.MosesTokenizer(lang: sourceLang);
                if (targetLang != null)
                {
                    this.deTokenizer = sacremoses.MosesDetokenizer(lang: targetLang);
                }
            }
        }

        public string PreprocessSentence(string sentence)
        {
            string preprocessedSentence;
            using (Py.GIL())
            {
                var tokenizedSentence = String.Join(" ", (string[])this.tokenizer.tokenize(sentence));
                var truecasedSentence = String.Join(" ", (string[])this.trueCaser.truecase(tokenizedSentence));
                preprocessedSentence = this.bpeProcessor.process_line(truecasedSentence);
            }

            return preprocessedSentence;
        }
        
        public string PostprocessSentence(string segmentedSentence)
        {
            string postprocessedSentence;
            using (Py.GIL())
            {
                var desegmentedSentence = segmentedSentence.Replace("@@ ", "");
                postprocessedSentence = String.Join(" ", (string[])this.bpeProcessor.process_line(desegmentedSentence));
            }

            return postprocessedSentence;
        }
    }
}