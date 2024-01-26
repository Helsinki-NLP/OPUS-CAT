using OpusCatMtEngine;
using Sdl.Core.Globalization;
using Sdl.FileTypeSupport.Framework.BilingualApi;
using Sdl.LanguagePlatform.TranslationMemory;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpusCatTranslationProvider
{
    public class FileReader : AbstractBilingualContentProcessor
    {
        internal List<ParallelSentence> FileTranslations;
        internal List<string> FileNewSegments;
        private FinetuneBatchTaskSettings settings;
        private OpusCatMarkupDataVisitor sourceVisitor;
        private OpusCatMarkupDataVisitor targetVisitor;

        public FileReader(FinetuneBatchTaskSettings settings)
        {
            this.settings = settings;
            this.FileTranslations = new List<ParallelSentence>();
            this.FileNewSegments = new List<string>();
            this.sourceVisitor = new OpusCatMarkupDataVisitor();
            this.targetVisitor = new OpusCatMarkupDataVisitor();
        }
        
        public override void ProcessParagraphUnit(IParagraphUnit paragraphUnit)
        {
      
            // Check if this paragraph actually contains segments 
            // If not, it is just a structure tag content, which is not processed
            if (paragraphUnit.IsStructure)
            {
                return;
            }
            
            foreach (ISegmentPair segmentPair in paragraphUnit.SegmentPairs)
            {
                if (segmentPair.Properties.ConfirmationLevel == ConfirmationLevel.Translated ||
                    segmentPair.Properties.ConfirmationLevel == ConfirmationLevel.ApprovedTranslation ||
                    segmentPair.Properties.ConfirmationLevel == ConfirmationLevel.ApprovedSignOff ||
                    (segmentPair.Properties.ConfirmationLevel == ConfirmationLevel.Draft && segmentPair.Properties.TranslationOrigin.MatchPercent == 100))
                {
                    this.sourceVisitor.Reset();
                    segmentPair.Source.AcceptVisitor(this.sourceVisitor);
                    this.targetVisitor.Reset(this.sourceVisitor.TagStarts);
                    segmentPair.Target.AcceptVisitor(this.targetVisitor);

                    //Add translation only if there's actual text content on both sides (not just tags)
                    if (this.sourceVisitor.SegmentContainsText && this.targetVisitor.SegmentContainsText)
                    {
                        FileTranslations.Add(new ParallelSentence(
                            this.sourceVisitor.PlainText,
                            this.targetVisitor.PlainText));
                    }
                    else
                    {
                        
                    }
                }
                else
                {
                    this.sourceVisitor.Reset();
                    segmentPair.Source.AcceptVisitor(this.sourceVisitor);
                    //If segment does not have translation, add it to new strings and look for fuzzies
                    FileNewSegments.Add(this.sourceVisitor.PlainText);
                }
            }
        }
    }
}