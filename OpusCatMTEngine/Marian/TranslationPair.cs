using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OpusCatMTEngine
{
    public enum SegmentationMethod
    {
        Bpe,
        SentencePiece
    }

    [DataContract]
    public class TranslationPair
    {

        //For compatibility with the previous API
        [DataMember]
        public string translation {get; set;}

        [DataMember]
        public string Translation
        {
            get { return translation; }
            set { translation = value; }
        }

        [DataMember]
        public string[] SegmentedSourceSentence { get; private set; }

        [DataMember]
        public string[] SegmentedTranslation { get; internal set; }

        [DataMember]
        public Dictionary<int, List<int>> SegmentedAlignmentSourceToTarget { get; private set; }

        [DataMember]
        public Dictionary<int, List<int>> SegmentedAlignmentTargetToSource { get; private set; }

        [DataMember]
        public string AlignmentString { get; private set; }

        [DataMember]
        public string RawTranslation { get; private set; }

        [DataMember]
        public SegmentationMethod Segmentation { get; internal set; }
        public bool AutoEditedTranslation { get; internal set; }

        public TranslationPair()
        {

        }

        public TranslationPair(
            string segmentedSource, 
            string translationAndAlignment, 
            SegmentationMethod segmentationMethod,
            string targetLanguage)
        {
            var lastSeparator = translationAndAlignment.LastIndexOf("|||");
            var segmentedTranslation = translationAndAlignment.Substring(0, lastSeparator - 1);
            this.RawTranslation = segmentedTranslation;
            var alignment = translationAndAlignment.Substring(lastSeparator + "||| ".Length);

            this.Initialize(segmentedSource, segmentedTranslation, alignment, segmentationMethod, targetLanguage);
        }

        public TranslationPair(
            string translation,
            string segmentedSource,
            string segmentedTarget,
            string alignment,
            SegmentationMethod segmentationMethod,
            string targetCode)
        {
            this.Translation = translation;
            
            this.Initialize(segmentedSource, segmentedTarget, alignment, segmentationMethod,targetCode);
        }

        private void Initialize(
            string segmentedSource, 
            string segmentedTarget, 
            string alignment, 
            SegmentationMethod segmentationMethod,
            string targetCode)
        {
            this.SegmentedSourceSentence = segmentedSource.Split(' ');
            
            this.SegmentedTranslation = segmentedTarget.Split(' ');
            this.SegmentedAlignmentSourceToTarget = TranslationPair.ParseAlignmentString(
                alignment,
                SegmentedSourceSentence.Length - 1,
                SegmentedTranslation.Length - 1,
                false);

            this.SegmentedAlignmentTargetToSource = TranslationPair.ParseAlignmentString(
                alignment,
                SegmentedTranslation.Length - 1,
                SegmentedSourceSentence.Length - 1,
                true);

            this.AlignmentString = alignment;
            this.Segmentation = segmentationMethod;
        }

        public static Dictionary<int,List<int>> ParseAlignmentString(
            string alignmentString, 
            int highestSourceIndex, 
            int highestTargetIndex,
            bool reverseAlignmentDirection)
        {
            var alignmentDict = new Dictionary<int, List<int>>();
            foreach (var alignmentPair in alignmentString.Split(' '))
            {
                var pairSplit = alignmentPair.Split('-');
                int sourceToken, targetToken;
                if (!reverseAlignmentDirection)
                {
                    sourceToken = int.Parse(pairSplit[0]);
                    targetToken = int.Parse(pairSplit[1]);
                }
                else
                {
                    sourceToken = int.Parse(pairSplit[1]);
                    targetToken = int.Parse(pairSplit[0]);
                }

                //Remove indexes that are larger than the actual size of the token sequence (Marian seems to
                //align non-alignable to the invisible end of sequence token?)
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

        /// <summary>
        /// Desegmented alignment is the alignment after segments have been joined into tokens.
        /// </summary>
        /// <param name="sourceSentence"></param>
        /// <param name="segmentedTranslation"></param>
        /// <param name="alignment"></param>
        /// <returns></returns>
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

        //This joins up two translation pairs, used to produce single output when input 
        // has been segmented into separate sentences
        internal void AppendTranslationPair(TranslationPair translationPart)
        {   
            this.SegmentedAlignmentSourceToTarget = this.JoinAlignments(
                this.SegmentedAlignmentSourceToTarget, translationPart.SegmentedAlignmentSourceToTarget,
                this.SegmentedSourceSentence.Length, this.SegmentedTranslation.Length);

            this.SegmentedAlignmentTargetToSource = this.JoinAlignments(
                this.SegmentedAlignmentTargetToSource, translationPart.SegmentedAlignmentTargetToSource,
                this.SegmentedTranslation.Length,this.SegmentedSourceSentence.Length);

            this.SegmentedTranslation = 
                this.SegmentedTranslation.Concat(translationPart.SegmentedTranslation).ToArray();
            this.SegmentedSourceSentence = 
                this.SegmentedSourceSentence.Concat(translationPart.SegmentedSourceSentence).ToArray();

            this.Translation = $"{this.Translation} {translationPart.Translation}";
        }

        private Dictionary<int, List<int>> JoinAlignments(
            Dictionary<int,List<int>> firstAlignment,
            Dictionary<int, List<int>> secondAlignment,
            int keySequenceLength,
            int valueSequenceLength)
        {
            var offsetAlignment =
                secondAlignment.ToDictionary(
                    x => x.Key + keySequenceLength,
                    x => x.Value.Select(z => z + valueSequenceLength).ToList());

            return firstAlignment.Concat(
                offsetAlignment).ToDictionary(x => x.Key, y => y.Value);
        }
    }
}
