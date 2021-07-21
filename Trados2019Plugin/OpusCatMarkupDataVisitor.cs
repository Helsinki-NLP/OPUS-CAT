using Sdl.FileTypeSupport.Framework.BilingualApi;
using Sdl.LanguagePlatform.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpusCatTranslationProvider
{
    public class OpusCatMarkupDataVisitor : IMarkupDataVisitor
    {
        private StringBuilder plainText;
        private Dictionary<string, ITagPair> sourceTagStarts;
        private bool _segmentContainsText = false;
        public Dictionary<string, IPlaceholderTag> Placeholders { get; set; }
        public string PlainText { get => plainText.ToString(); }
        public Dictionary<string, ITagPair> TagStarts { get; set; }
        public Dictionary<string, ITagPair> TagEnds { get; set; }
        public bool SegmentContainsText { get => _segmentContainsText; internal set => _segmentContainsText = value; }

        public void Reset()
        {
            this.SegmentContainsText = false;
            this.plainText = new StringBuilder();
            this.Placeholders = new Dictionary<string, IPlaceholderTag>();
            this.TagStarts = new Dictionary<string, ITagPair>();
            this.TagEnds = new Dictionary<string, ITagPair>();
        }


        public void VisitCommentMarker(ICommentMarker commentMarker)
        {

        }

        public void VisitLocationMarker(ILocationMarker location)
        {

        }

        public void VisitLockedContent(ILockedContent lockedContent)
        {

        }

        public void VisitOtherMarker(IOtherMarker marker)
        {

        }

        public void VisitPlaceholderTag(IPlaceholderTag tag)
        {
            /*var placeholder = $" PLACEHOLDER{this.Placeholders.Keys.Count} ";
            this.plainText.Append(placeholder);
            this.Placeholders[placeholder] = tag;*/

            this.plainText.Append(" PLACEHOLDER ");
        }

        public void VisitRevisionMarker(IRevisionMarker revisionMarker)
        {

        }

        private void VisitChildren(IAbstractMarkupDataContainer container)
        {
            foreach (var item in container)
            {

                item.AcceptVisitor(this);
            }
        }

        public void VisitSegment(ISegment segment)
        {
            this.VisitChildren(segment);
        }

        public void VisitTagPair(ITagPair tagPair)
        {
            //Aligning tags is difficult due to strange behavior of Trados API (all tags having the same id)
            //Because of this, don't use ordinals, just TAGPAIRSTART and TAGPAIREND
            /*
            string startTag;
            if (this.sourceTagStarts != null)
            {
                var tagId = tagPair.TagProperties.TagId;
                
                if (this.sourceTagStarts.Values.Any(x => x.TagProperties.TagId == tagId))
                {
                    startTag = this.sourceTagStarts.SingleOrDefault(x => x.Value.TagProperties.TagId == tagId).Key;
                }
                else
                {
                    startTag = $" TAGPAIRSTART{this.TagStarts.Keys.Count} ";
                }
            }
            else
            {
                startTag = $" TAGPAIRSTART{this.TagStarts.Keys.Count} ";
            }

            this.plainText.Append(startTag);
            this.TagStarts[startTag] = tagPair;

            this.VisitChildren(tagPair);

            var endTag = startTag.Replace("TAGPAIRSTART","TAGPAIREND");
            this.plainText.Append(endTag);
            this.TagEnds[endTag] = tagPair;*/

            this.plainText.Append(" TAGPAIRSTART ");
            this.VisitChildren(tagPair);
            this.plainText.Append(" TAGPAIREND ");

        }

        public void VisitText(IText text)
        {
            string textString = text.ToString();
            if (Regex.IsMatch(textString, @"[^\s]"))
            {
                this.SegmentContainsText = true;
            }
            this.plainText.Append(textString);
        }

        internal void Reset(Dictionary<string, ITagPair> tagStarts)
        {
            this.sourceTagStarts = tagStarts;
            this.Reset();
        }
    }
}
