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

        //This is used to remove those characters from a string that may be added or changed
        //during MT preprocessing. Needed because the Trados segment has to be mapped to the
        //subword indices in the source sentence of the MT.
        private static Regex variantCharacters = new Regex(@"[^\p{L}\p{N},.]");
        private Dictionary<int, List<Tag>> tagsInSourceDict;
        private Dictionary<int, List<Tag>> tagsInTargetDict;

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

        private void GetTagPositions()
        {
            //Go through the elements consuming the segmented source, fenceposting the tags with
            //segmented source indexes
            this.tagsInSourceDict = new Dictionary<int, List<Tag>>();

            Queue<string> sourceSubwordQueue = new Queue<string>(this.translation.SegmentedSourceSentence);
            int subwordIndex = 0;
            foreach (var segElement in this.sourceSegment.Elements)
            {
                Type elementType = segElement.GetType();

                if (elementType == typeof(Text))
                {
                    //Only check for numbers and letters, as they should be identical with both BPE
                    //and SentencePiece subwords.
                    string elementText = variantCharacters.Replace(((Text)segElement).Value, "");
                    //consume the element from the segmented source
                    string nextSourceSubword = variantCharacters.Replace(sourceSubwordQueue.Dequeue(), "");
                    subwordIndex++;

                    while (elementText.StartsWith(nextSourceSubword))
                    {
                        elementText = elementText.Substring(nextSourceSubword.Length);
                        if (elementText.Length > 0 && sourceSubwordQueue.Count > 0)
                        {
                            nextSourceSubword =
                                variantCharacters.Replace(sourceSubwordQueue.Dequeue(), "");
                            subwordIndex++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else if (elementType == typeof(Tag))
                {
                    if (this.tagsInSourceDict.ContainsKey(subwordIndex))
                    {
                        this.tagsInSourceDict[subwordIndex].Add((Tag)segElement);
                    }
                    else
                    {
                        this.tagsInSourceDict[subwordIndex] = new List<Tag>() { (Tag)segElement };
                    }
                }
            }

            this.tagsInTargetDict = new Dictionary<int, List<Tag>>();
            //Place tags in target based on the source positions
            foreach (var tagsInSourceIndex in this.tagsInSourceDict)
            {

                if (translation.SegmentedAlignmentSourceToTarget.ContainsKey(tagsInSourceIndex.Key))
                {
                    this.tagsInTargetDict[translation.SegmentedAlignmentSourceToTarget[tagsInSourceIndex.Key].First()] =
                        tagsInSourceIndex.Value;
                }
                else
                {
                    //If there is no explicit alignment between the source and target positions, get the nearest alignment
                    var sourcePositions = this.translation.SegmentedAlignmentSourceToTarget.Keys;
                    var nearestAlignedPosition = sourcePositions.OrderBy(x => Math.Abs(x - tagsInSourceIndex.Key)).First();
                    this.tagsInTargetDict[translation.SegmentedAlignmentSourceToTarget[nearestAlignedPosition].First()] =
                        tagsInSourceIndex.Value;
                }
            }
        }

        private void AddTagsByAligment()
        {
            this.GetTagPositions();

            //Construct the translation segment by adding the subwords and the tags.
            int subwordIndex = 0;
            foreach (var subword in this.translation.SegmentedTranslation)
            {
                if (this.tagsInTargetDict.ContainsKey(subwordIndex))
                {
                    this.translationSegment.AddRange(this.tagsInTargetDict[subwordIndex]);
                }
                if (subwordIndex == 0)
                {
                    this.translationSegment.Add(subword.Replace('▁', ' ').TrimStart(' '));
                }
                else
                {
                    this.translationSegment.Add(subword.Replace('▁', ' '));
                }
                
                subwordIndex++;
            }

            this.FixTagSpacing();
        }

        private void FixTagSpacing()
        {

            for (var elementIndex = 0; elementIndex < this.translationSegment.Elements.Count; elementIndex++)
            {
                Tag tag = this.translationSegment.Elements[elementIndex] as Tag;

                Text previousElement = null;
                if (elementIndex > 0)
                {
                    previousElement = this.translationSegment.Elements[elementIndex - 1] as Text;
                }

                Text nextElement = null;
                if (elementIndex < this.translationSegment.Elements.Count - 1)
                {
                    nextElement = this.translationSegment.Elements[elementIndex + 1] as Text;
                }

                if (tag != null)
                {
                    switch (tag.Type)
                    {
                        case TagType.Start:
                            if (nextElement != null && nextElement.Value.StartsWith(" "))
                            {
                                nextElement.Value = nextElement.Value.TrimStart(' ');
                                if (!previousElement.Value.EndsWith(" "))
                                {
                                    previousElement.Value = previousElement.Value + " ";
                                }
                            }
                            break;
                        case TagType.End:
                            if (previousElement != null && previousElement.Value.EndsWith(" "))
                            {
                                previousElement.Value = previousElement.Value.TrimEnd(' ');
                                if (!nextElement.Value.StartsWith(" "))
                                {
                                    nextElement.Value = " " + nextElement.Value;
                                }
                            }
                            break;
                        case TagType.Standalone:
                            if (previousElement != null && !previousElement.Value.EndsWith(" "))
                            {
                                previousElement.Value = previousElement.Value + " ";
                            }

                            if (nextElement != null)
                            {

                                nextElement.Value = Regex.Replace(nextElement.Value, @"(^[^ ,.!?:;])", @" $1");
                                nextElement.Value = Regex.Replace(nextElement.Value, @"^ +([,.!?:;])", @"$1");
                                
                            }
                            break;
                        default:
                            break;
                    }

                }
            }
        }
    }
}
