using Sdl.FileTypeSupport.Framework.BilingualApi;
using Sdl.LanguagePlatform.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiskmoTranslationProvider
{
    class FiskmoProviderElementVisitor : ISegmentElementVisitor
    {

        private StringBuilder plainText;
        private Dictionary<string, Tag> sourceTagStarts;
        private Dictionary<string, Tag> sourceTagEnds;

        public string PlainText
        {
            get 
            {
                if (plainText == null)
                {
                    return "";
                }

                return plainText.ToString();
            }
        }

        //public Dictionary<string,Tag> Placeholders { get; set;}

        public Queue<Tag> Placeholders { get; set; }

        //public Dictionary<string, Tag> TagStarts { get; set; }
        public Queue<Tag> TagStarts { get; set; }
        //public Dictionary<string, Tag> TagEnds { get; set; }
        public Queue<Tag> TagEnds { get; set; }

        public void Reset()
        {
            plainText = new StringBuilder();
            this.Placeholders = new Queue<Tag>();
            this.TagStarts = new Queue<Tag>();
            this.TagEnds = new Queue<Tag>();
        }


        public FiskmoProviderElementVisitor()
        {
            this.Placeholders = new Queue<Tag>();
            this.TagStarts = new Queue<Tag>();
            this.TagEnds = new Queue<Tag>();
        }

        #region ISegmentElementVisitor Members

        public void VisitDateTimeToken(Sdl.LanguagePlatform.Core.Tokenization.DateTimeToken token)
        {
            this.plainText.Append(token.Text);
        }

        public void VisitMeasureToken(Sdl.LanguagePlatform.Core.Tokenization.MeasureToken token)
        {
            this.plainText.Append(token.Text);
        }

        public void VisitNumberToken(Sdl.LanguagePlatform.Core.Tokenization.NumberToken token)
        {
            this.plainText.Append(token.Text);
        }

        public void VisitSimpleToken(Sdl.LanguagePlatform.Core.Tokenization.SimpleToken token)
        {
            this.plainText.Append(token.Text);
        }

        public void VisitTag(Tag tag)
        {
            //Only include standalone/placeholder tags, include as PLACEHOLDER{n}
            if (tag.Type == TagType.Standalone || tag.Type == TagType.TextPlaceholder)
            {
                //var placeholder = $" PLACEHOLDER{this.Placeholders.Keys.Count} ";
                var placeholder = $" PLACEHOLDER ";
                this.plainText.Append(placeholder);
                //this.Placeholders[placeholder] = tag;
                this.Placeholders.Enqueue(tag);
            }
            else if (tag.Type == TagType.Start)
            {
                /*
                string startTag;
                if (this.sourceTagStarts.Values.Any(x => x.TagID == tag.TagID))
                {
                    startTag = this.sourceTagStarts.First(x => x.Value.TagID == tag.TagID).Key;
                }
                else
                {
                    startTag = $" TAGPAIRSTART{this.TagStarts.Keys.Count} ";
                }

                this.plainText.Append(startTag);
                this.TagStarts[startTag] = tag;*/
                this.plainText.Append(" TAGPAIRSTART ");
                this.TagStarts.Enqueue(tag);
            }
            else if (tag.Type == TagType.End)
            {
                /*string endTag;
                if (this.sourceTagEnds.Values.Any(x => x.TagID == tag.TagID))
                {
                    endTag = this.sourceTagEnds.First(x => x.Value.TagID == tag.TagID).Key;
                }
                else
                {
                    endTag = $" TAGPAIREND{this.TagEnds.Keys.Count} ";
                }
                
                this.plainText.Append(endTag);
                this.TagEnds[endTag] = tag;*/
                this.plainText.Append(" TAGPAIREND ");
                this.TagEnds.Enqueue(tag);
            }
        }

        public void VisitTagToken(Sdl.LanguagePlatform.Core.Tokenization.TagToken token)
        {
            this.plainText.Append(token.Text);
        }

        public void VisitText(Text text)
        {
            this.plainText.Append(text);
        }

        internal void Reset(Dictionary<string, Tag> tagStarts, Dictionary<string, Tag> tagEnds)
        {
            /*this.sourceTagStarts = TagStarts;
            this.sourceTagEnds = TagEnds;
            this.Reset();*/
        }

        #endregion
    }
}
