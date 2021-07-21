using OpusCatMTEngine;
using OpusMTInterface;
using Sdl.LanguagePlatform.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpusCatTranslationProvider
{
    class TagRestorer
    {
        private Segment sourceSegment;
        private TranslationPair translation;
        private OpusCatProviderElementVisitor tagVisitor;
        private Segment translationSegment;

        private static Regex notNumberOrLetter = new Regex(@"[^\p{L}\p{N}]");

        public TagRestorer(
            Segment sourceSegment, 
            TranslationPair translation, 
            OpusCatProviderElementVisitor tagVisitor,
            Segment translationSegment)
        {
            this.sourceSegment = sourceSegment;
            this.translation = translation;
            this.tagVisitor = tagVisitor;
            this.translationSegment = translationSegment;
        }

        private void ReplaceInjectedTags()
        {
            var split = Regex.Split(this.translation.translation, @"\b(PLACEHOLDER|TAGPAIRSTART ?| ?TAGPAIREND)\b");

            //Tag starts and ends must match, so need a stack to keep track of what tags
            //have been applied
            var tagStack = new Stack<Tag>();

            foreach (var part in split)
            {
                //Remove potential spaces from after TAGPAIRSTARTS and before TAGPAIREND
                var normalpart = part.Replace("TAGPAIRSTART ", "TAGPAIRSTART");
                normalpart = normalpart.Replace(" TAGPAIREND", "TAGPAIREND");

                switch (normalpart)
                {
                    case "PLACEHOLDER":
                        if (this.tagVisitor.Placeholders.Count != 0)
                        {
                            this.translationSegment.Add(this.tagVisitor.Placeholders.Dequeue());
                        }
                        break;
                    case "TAGPAIRSTART":
                        if (this.tagVisitor.TagStarts.Count != 0)
                        {
                            var startTag = this.tagVisitor.TagStarts.Dequeue();
                            tagStack.Push(startTag);
                            translationSegment.Add(startTag);
                        }
                        break;
                    case "TAGPAIREND":
                        if (tagStack.Count != 0)
                        {
                            var correspondingStartTag = tagStack.Pop();
                            var endTag = this.tagVisitor.TagEnds[correspondingStartTag.TagID];
                            translationSegment.Add(endTag);
                        }
                        break;
                    default:
                        translationSegment.Add(part);
                        break;
                }
            }

            //Insert missing end tags
            foreach (var excessStartTag in tagStack)
            {
                var nonEndedTagIndex = translationSegment.Elements.IndexOf(excessStartTag);
                var endTag = this.tagVisitor.TagEnds[excessStartTag.TagID];
                translationSegment.Elements.Insert(nonEndedTagIndex + 1, endTag);
            }
        
        }

        internal void ProcessTags()
        {
            //Check for injected tags
            if (Regex.IsMatch(this.translation.translation, @"\b(PLACEHOLDER|TAGPAIRSTART ?| ?TAGPAIREND)\b"))
            {
                this.ReplaceInjectedTags();
            }
            else if (this.translation.AlignmentString != null)
            {
                this.AddTagsByAligment();
            }

            



        }

        private void AddTagsByAligment()
        {
            //Go through the elements consuming the segmented source, fenceposting the tags with
            //segmented source indexes
            Dictionary<int, Tag> tagDict = new Dictionary<int, Tag>();

            Queue<string> sourceSubwordQueue = new Queue<string>(this.translation.SegmentedSourceSentence);
            int subwordIndex = 0;
            foreach (var segElement in this.sourceSegment.Elements)
            {
                if (segElement.GetType() == typeof(Text))
                {
                    //Only check for numbers and letters, as they should be identical with both BPE
                    //and SentencePiece subwords.
                    string elementText = notNumberOrLetter.Replace(((Text)segElement).Value,"");
                    //consume the element from the segmented source
                    string nextSourceSubword = notNumberOrLetter.Replace(sourceSubwordQueue.Dequeue(),"");

                    while (elementText.StartsWith(nextSourceSubword))
                    {
                        elementText = elementText.Substring(nextSourceSubword.Length);
                        if (sourceSubwordQueue.Count > 0)
                        {
                            nextSourceSubword = notNumberOrLetter.Replace(sourceSubwordQueue.Dequeue(), "");
                        }
                        subwordIndex++;
                    }
                }
                else if (segElement.GetType() == typeof(Tag))
                {
                    tagDict[subwordIndex] = (Tag)segElement;
                }
            }

        }
    }
}
