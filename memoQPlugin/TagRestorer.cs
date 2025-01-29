
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using OpusCatMtEngine;

namespace OpusCatMTPlugin
{
    class TagRestorer
    {
        private List<String> sourceSegment;
        private TranslationPair translation;
        
        //This is used to remove those characters from a string that may be added or changed
        //during MT preprocessing. Needed because the Trados segment has to be mapped to the
        //subword indices in the source sentence of the MT.
        private static Regex variantCharacters = new Regex(@"[^\p{L}\p{N},.]");
        private Dictionary<int, List<String>> tagsInSourceDict;
        private Dictionary<int, List<String>> tagsInTargetDict;
        private List<string> taggedTarget;

        public TagRestorer(
            String segmentXmlString,
            OpusCatMtEngine.TranslationPair translation)
        {
            this.sourceSegment = Regex.Split(segmentXmlString, @"(<[^>]+>)").ToList();
            this.translation = translation;
        
        }

        /* This is for the earlier tag placeholder method, which is not 
         * really used, as it only supports rigid tag order. Leaving the code here in case
         * it makes a come-back
         * 
         * private void ReplaceInjectedTags()
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

        } */

        private void GetInitialPositions()
        {
            //Go through the elements consuming the segmented source, fenceposting the tags with
            //segmented source indexes
            this.tagsInSourceDict = new Dictionary<int, List<String>>();

            Queue<string> sourceSubwordQueue = new Queue<string>(this.translation.SegmentedSourceSentence);
            int subwordIndex = 0;
            foreach (var segElement in this.sourceSegment)
            {
                // The elements are either plain text or string tags
                if (!segElement.StartsWith("<") && sourceSubwordQueue.Count > 0)
                {
                    //Only check for numbers and letters, as they should be identical with both BPE
                    //and SentencePiece subwords.
                    string elementText = variantCharacters.Replace(segElement, "");
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
                //There may also be special character escape tags, this skips those
                else if (Regex.IsMatch(segElement,"^</?inline"))
                {
                    if (this.tagsInSourceDict.ContainsKey(subwordIndex))
                    {
                        this.tagsInSourceDict[subwordIndex].Add(segElement);
                    }
                    else
                    {
                        this.tagsInSourceDict[subwordIndex] = new List<String>() { segElement };
                    }
                }
            }

            this.tagsInTargetDict = new Dictionary<int, List<String>>();
            //Place tags in target based on the source positions
            foreach (var tagsInSourceIndex in this.tagsInSourceDict)
            {
                int targetIndex;
                if (translation.SegmentedAlignmentSourceToTarget.ContainsKey(tagsInSourceIndex.Key))
                {
                    targetIndex = translation.SegmentedAlignmentSourceToTarget[tagsInSourceIndex.Key].First();
                }
                else
                {
                    //If there is no explicit alignment between the source and target positions, get the nearest alignment
                    var sourcePositions = this.translation.SegmentedAlignmentSourceToTarget.Keys;
                    var closestSourceIndex = sourcePositions.OrderBy(x => Math.Abs(x - tagsInSourceIndex.Key)).First();
                    targetIndex = translation.SegmentedAlignmentSourceToTarget[closestSourceIndex].First();
                    //Adjust the position based on the direction where the closest source-aligned target word is found.
                    //Motivation: one word in source and translation, tag in front of word and behind the word. Need to
                    //place on tag in front of word and one behind the word also in the translation, but there is only
                    //one aligned word, hence need to adjust target index.
                    targetIndex = targetIndex + (Math.Sign(tagsInSourceIndex.Key - closestSourceIndex));
                }

                if (!this.tagsInTargetDict.ContainsKey(targetIndex))
                {
                    this.tagsInTargetDict[targetIndex] = new List<String>();
                }

                this.tagsInTargetDict[targetIndex].AddRange(tagsInSourceIndex.Value);
            }
        }

        /*private void BalanceTagPairs()
        {
            //Make sure all start tags have ending tags
            Stack<String> tagStack = new Stack<String>();

            foreach (var tagsInIndex in this.tagsInTargetDict.OrderBy(x => x.Key))
            {
                var tagIndex = 0;
                foreach (var tag in new List<String>(tagsInIndex.Value))
                {
                    if (tag.StartsWith("<inline"))
                    {
                        tagStack.Push(tag);
                    }
                    else if (tag.StartsWith("</inline"))
                    {
                        if (tagStack.Count == 0)
                        {
                            //Replace with next start tag
                        }
                         var activeStartTag = tagStack.Pop();
                        if (activeStartTag.TagID != tag.TagID)
                        {
                            //Find the tag that should be placed here
                            var correctEndTagPosition =
                                this.tagsInTargetDict.Single(
                                    x => x.Value.SingleOrDefault(y => y.TagID == activeStartTag.TagID && y.Type == TagType.End) != null);
                            var correctEndTag = correctEndTagPosition.Value.Single(y => y.TagID == activeStartTag.TagID);
                            this.tagsInTargetDict[correctEndTagPosition.Key].Remove(correctEndTag);
                            this.tagsInTargetDict[tagsInIndex.Key].Insert(tagIndex, correctEndTag);
                        }
                    }
                    tagIndex++;
                }
            }
        }*/

        private void GetTagPositions()
        {
            this.GetInitialPositions();

            //this.CorrectReversedTagPairs();
            
            //In MemoQ, the closing tags haven't got IDs, so they can't be crossed
            //this.HandleCrossedTagPairs();
        }

        internal String AddTagsByAlignment()
        {
            this.taggedTarget = new List<String>();

            this.GetTagPositions();

            //Construct the translation segment by adding the subwords and the tags.
            int subwordIndex = 0;

            foreach (var subword in this.translation.SegmentedTranslation)
            {
                if (this.tagsInTargetDict.ContainsKey(subwordIndex))
                {
                    this.taggedTarget.AddRange(this.tagsInTargetDict[subwordIndex]);
                }
                if (subwordIndex == 0)
                {
                    this.taggedTarget.Add(subword.Replace('▁', ' ').TrimStart(' '));
                }
                else
                {
                    this.taggedTarget.Add(subword.Replace('▁', ' '));
                }

                subwordIndex++;
            }

            //Add the final tags
            if (this.tagsInTargetDict.ContainsKey(subwordIndex))
            {
                this.taggedTarget.AddRange(this.tagsInTargetDict[subwordIndex]);
            }

            //this.FixTagSpacing();
            //this.MoveTagsToWordBoundaries();

            return String.Join("",taggedTarget);
        }

        /*
        private void MoveTagsToWordBoundaries()
        {
            List<int> textElementPositions = 
                this.translationSegment.Elements.Select(
                    (x, i) => new { el = x, idx = i}).Where(
                    x => x.el.GetType() == typeof(Text)).Select(x => x.idx).OrderBy(x => x).ToList();
            for (var elementIndex = 0; elementIndex < this.translationSegment.Elements.Count; elementIndex++)
            {
                Tag tag = this.translationSegment.Elements[elementIndex] as Tag;

                if (tag == null)
                {
                    continue;
                }

                Text previousTextElement = null;
                if (elementIndex > 0)
                {
                    var smallerElementIndexes = textElementPositions.Where(x => x < elementIndex);
                    if (smallerElementIndexes.Any())
                    {
                        var previousTextElementIndex = smallerElementIndexes.Last();
                        previousTextElement = this.translationSegment.Elements[previousTextElementIndex] as Text;
                    }
                }

                Text nextTextElement = null;
                if (elementIndex < this.translationSegment.Elements.Count - 1)
                {
                    var greaterElementIndexes = textElementPositions.Where(x => x > elementIndex);
                    if (greaterElementIndexes.Any())
                    {
                        var nextTextElementIndex = greaterElementIndexes.First();
                        nextTextElement = this.translationSegment.Elements[nextTextElementIndex] as Text;
                    }
                }

                switch (tag.Type)
                {
                    case TagType.Start:
                        if (
                            previousTextElement != null && 
                            nextTextElement != null &&
                            previousTextElement.Value.Any() &&
                            Char.IsLetter(previousTextElement.Value.Last()))
                        {
                            //Move letters from the end of previous element to the start of next text element in the segment
                            var textToMove = Regex.Match(previousTextElement.Value, @"([\w]+)$");
                            nextTextElement.Value = textToMove.Value + nextTextElement.Value;
                            previousTextElement.Value = Regex.Replace(previousTextElement.Value, @"([\w]+)$","");
                        }
                        break;
                    case TagType.End:
                        if (
                            nextTextElement != null && 
                            previousTextElement != null && 
                            nextTextElement.Value.Any() && 
                            Char.IsLetter(nextTextElement.Value.First()))
                        {
                            var textToMove = Regex.Match(nextTextElement.Value, @"(^[\w]+)");
                            previousTextElement.Value = previousTextElement.Value + textToMove.Value;
                            nextTextElement.Value = Regex.Replace(nextTextElement.Value, @"(^[\w]+)", "");
                        }
                        break;
                    case TagType.Standalone:
                        break;
                    default:
                        break;
                    
                }
            }
        }*/

        /*
        private void FixTagSpacing()
        {
            List<int> textElementPositions =
                this.taggedTarget.Select(
                    (x, i) => new { el = x, idx = i }).Where(
                    x => !Regex.IsMatch(x.el, "^</?inline")).Select(x => x.idx).OrderBy(x => x).ToList();

            for (var elementIndex = 0; elementIndex < this.translationSegment.Elements.Count; elementIndex++)
            {
                Tag tag = this.translationSegment.Elements[elementIndex] as Tag;

                Text previousTextElement = null;
                if (elementIndex > 0)
                {
                    var smallerElementIndexes = textElementPositions.Where(x => x < elementIndex);
                    if (smallerElementIndexes.Any())
                    {
                        var previousTextElementIndex = smallerElementIndexes.Last();
                        previousTextElement = this.translationSegment.Elements[previousTextElementIndex] as Text;
                    }
                }

                Text nextTextElement = null;
                if (elementIndex < this.translationSegment.Elements.Count - 1)
                {
                    var greaterElementIndexes = textElementPositions.Where(x => x > elementIndex);
                    if (greaterElementIndexes.Any())
                    {
                        var nextTextElementIndex = greaterElementIndexes.First();
                        nextTextElement = this.translationSegment.Elements[nextTextElementIndex] as Text;
                    }
                }

                if (tag != null)
                {
                    switch (tag.Type)
                    {
                        case TagType.Start:
                            if (nextTextElement != null && nextTextElement.Value.StartsWith(" "))
                            {
                                nextTextElement.Value = nextTextElement.Value.TrimStart(' ');
                                if (previousTextElement != null && !previousTextElement.Value.EndsWith(" "))
                                {
                                    previousTextElement.Value = previousTextElement.Value + " ";
                                }
                            }
                            break;
                        case TagType.End:
                            if (previousTextElement != null && previousTextElement.Value.EndsWith(" "))
                            {
                                previousTextElement.Value = previousTextElement.Value.TrimEnd(' ');
                                if (nextTextElement !=null && !nextTextElement.Value.StartsWith(" "))
                                {
                                    nextTextElement.Value = " " + nextTextElement.Value;
                                }
                            }
                            break;
                        case TagType.Standalone:
                            if (previousTextElement != null && !previousTextElement.Value.EndsWith(" "))
                            {
                                previousTextElement.Value = previousTextElement.Value + " ";
                            }

                            if (nextTextElement != null)
                            {

                                nextTextElement.Value = Regex.Replace(nextTextElement.Value, @"(^[^ ,.!?:;])", @" $1");
                                nextTextElement.Value = Regex.Replace(nextTextElement.Value, @"^ +([,.!?:;])", @"$1");             
                            }
                            break;
                        default:
                            break;
                    }

                }
            }
        }*/
    }
}
