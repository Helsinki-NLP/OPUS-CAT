using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpusCatMTEngine
{
    public class TranslationPair
    {
        
        public enum SegmentationMethod { SentencePiece, BPE};

        
        public string Translation
        {
            get
            {
                string translation = null;
                switch (this.SegmentationMethodUsed)
                {
                    case SegmentationMethod.SentencePiece:
                        translation = (String.Concat(this.SegmentedTranslation)).Replace("▁", " ").Trim();
                        break;
                    case SegmentationMethod.BPE:
                        break;
                }
                return translation;
            }
        }
        public string[] SegmentedSourceSentence { get; private set; }
        public string[] SegmentedTranslation { get; private set; }
        public Dictionary<int, List<int>> SegmentedAlignment { get; private set; }
        public string AlignmentString { get; private set; }
        public SegmentationMethod SegmentationMethodUsed { get; private set; }

        public TranslationPair(string segmentedSource, string translationAndAlignment)
        {
            var lastSeparator = translationAndAlignment.LastIndexOf("|||");
            var segmentedTranslation = translationAndAlignment.Substring(0, lastSeparator - 1);
            var alignment = translationAndAlignment.Substring(lastSeparator + 4);

            this.Initialize(segmentedSource, segmentedTranslation, alignment);
        }

        public TranslationPair(string segmentedSource, string segmentedTarget, string alignment)
        {
            this.Initialize(segmentedSource, segmentedTarget, alignment);
        }

        private void Initialize(string segmentedSource, string segmentedTarget, string alignment)
        {
            this.SegmentedSourceSentence = segmentedSource.Split(' ');
            this.SegmentedTranslation = segmentedTarget.Split(' ');
            this.SegmentedAlignment = TranslationPair.ParseAlignmentString(
                alignment,
                SegmentedSourceSentence.Length-1,
                SegmentedTranslation.Length-1);
            this.AlignmentString = alignment;
        }

        public static Dictionary<int,List<int>> ParseAlignmentString(string alignmentString, int highestSourceIndex, int highestTargetIndex)
        {
            var alignmentDict = new Dictionary<int, List<int>>();
            foreach (var alignmentPair in alignmentString.Split(' '))
            {
                var pairSplit = alignmentPair.Split('-');
                var sourceToken = int.Parse(pairSplit[0]);
                var targetToken = int.Parse(pairSplit[1]);

                //Remove indexes that are larger than the actual size of the token sequence (Marian seems to
                //align to the invisible end of sequence token?)
                if (sourceToken > highestSourceIndex || targetToken > highestTargetIndex)
                {
                    continue;
                }

                if (alignmentDict.ContainsKey(sourceToken))
                {
                    alignmentDict[sourceToken].Add(targetToken);
                }
                else
                {
                    alignmentDict[sourceToken] = new List<int>() { targetToken };
                }
            }
            return alignmentDict;
        }

        private Dictionary<int, List<int>> GenerateDesegmentedAlignment(string sourceSentence, string segmentedTranslation, string alignment)
        {
            //Generate a dict out of the alignment
            var alignmentDict = new Dictionary<int, List<int>>();
            foreach (var alignmentPair in alignment.Split(' '))
            {
                var pairSplit = alignmentPair.Split('-');
                var sourceToken = int.Parse(pairSplit[0]);
                var targetToken = int.Parse(pairSplit[1]);
                if (alignmentDict.ContainsKey(sourceToken))
                {
                    alignmentDict[sourceToken].Add(targetToken);
                }
                else
                {
                    alignmentDict[sourceToken] = new List<int>();
                }
            }

            //Now map segmented token indexes to desegmented indexes for the target
            var targetSegmentedToDesegmented = new Dictionary<int, int>();
            int desegmentedTokenIndex = -1;
            var targetSegmentedTokens = segmentedTranslation.Split(' ');
            for (var i = 0; i < targetSegmentedTokens.Length; i++)
            {
                if (targetSegmentedTokens[i].StartsWith("_"))
                {
                    desegmentedTokenIndex++;
                }
                targetSegmentedToDesegmented[i] = desegmentedTokenIndex;
            }

            //Next go through source mapping source desegmented tokens to target desegmented tokens
            var desegmentedAlignment = new Dictionary<int, List<int>>();
            desegmentedTokenIndex = -1;
            var sourceSegmentedTokens = sourceSentence.Split(' ');
            for (var i = 0; i < sourceSegmentedTokens.Length; i++)
            {
                if (sourceSegmentedTokens[i].StartsWith("_"))
                {
                    desegmentedTokenIndex++;
                }

                if (alignmentDict.ContainsKey(i))
                {
                    List<int> desegmentedTargetIndexes = new List<int>();
                    var segmentedTargetIndexes = alignmentDict[i];
                    foreach (var targetIndex in segmentedTargetIndexes)
                    {
                        desegmentedTargetIndexes.Add(targetSegmentedToDesegmented[targetIndex]);
                    }

                    if (desegmentedAlignment.ContainsKey(i))
                    {
                        desegmentedAlignment[desegmentedTokenIndex].AddRange(desegmentedTargetIndexes);
                    }
                    else
                    {
                        desegmentedAlignment[desegmentedTokenIndex] = desegmentedTargetIndexes;
                    }
                }
            }

            return desegmentedAlignment;

        }

    }
}
