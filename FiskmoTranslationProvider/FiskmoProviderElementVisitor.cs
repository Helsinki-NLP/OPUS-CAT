using Sdl.FileTypeSupport.Framework.BilingualApi;
using Sdl.LanguagePlatform.Core;
using System.Collections.Generic;
using System.Text;

namespace FiskmoTranslationProvider
{
    class FiskmoProviderElementVisitor : ISegmentElementVisitor
    {

        /// <summary>
        /// This static class is for working with ISegment objects from the BilingualApi,
        /// which is not the same as the segments in LanguagePlatform.Core, they segments
        /// have different constituents (e.g. IPlaceHolderTag in BilingualApi and more generic Tag
        /// for LanguagePlatform.Core). Both need to be processed identically, so the static method
        /// for converting BilingualApi ISegments is included here in the LanguagaPlatform.Core visitor
        /// class.
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="includePlaceholderTags"></param>
        /// <returns></returns>
        public static string ExtractSegmentText(ISegment segment, bool includePlaceholderTags)
        {
            StringBuilder segmentText = new StringBuilder();
            int placeholderIndex = 0;
            foreach (var item in segment.AllSubItems)
            {
                if (item is IText)
                {
                    //Segments with line breaks might cause trouble, remove the line breaks
                    var itemText = item.ToString();
                    
                    if (itemText.Contains("\n"))
                    {

                    }
                    segmentText.Append(itemText);
                }
                else if (includePlaceholderTags && item is IPlaceholderTag)
                {
                    //segmentText.Append(((IPlaceholderTag)item).TagProperties.DisplayText);
                    segmentText.Append($"PLACEHOLDER{placeholderIndex}");
                    placeholderIndex++;
                }
            }

            return segmentText.ToString();
        }

        private FiskmoOptions _options;
        private string _plainText;
        public string PlainText
        {
            get 
            {
                if (_plainText == null)
                {
                    _plainText = "";
                }
                return _plainText;
            }
            set 
            {
                _plainText = value;
            }
        }

        public Dictionary<string,Tag> Placeholders { get; set;}

        public void Reset()
        {
            _plainText = "";
            this.Placeholders = new Dictionary<string, Tag>();
        }

        public FiskmoProviderElementVisitor(FiskmoOptions options)
        {
            _options = options;
            this.Placeholders = new Dictionary<string, Tag>();
        }

        #region ISegmentElementVisitor Members

        public void VisitDateTimeToken(Sdl.LanguagePlatform.Core.Tokenization.DateTimeToken token)
        {
            _plainText += token.Text;
        }

        public void VisitMeasureToken(Sdl.LanguagePlatform.Core.Tokenization.MeasureToken token)
        {
            _plainText += token.Text;
        }

        public void VisitNumberToken(Sdl.LanguagePlatform.Core.Tokenization.NumberToken token)
        {
            _plainText += token.Text;
        }

        public void VisitSimpleToken(Sdl.LanguagePlatform.Core.Tokenization.SimpleToken token)
        {
            _plainText += token.Text;
        }

        public void VisitTag(Tag tag)
        {
            //Only include standalone/placeholder tags, include as PLACEHOLDER{n}
            if (tag.Type == TagType.Standalone || tag.Type == TagType.TextPlaceholder)
            {
                var placeholder = $"PLACEHOLDER{this.Placeholders.Keys.Count}";
                _plainText += placeholder;
                this.Placeholders[placeholder] = tag;
            }
        }

        public void VisitTagToken(Sdl.LanguagePlatform.Core.Tokenization.TagToken token)
        {
            _plainText += token.Text;
        }

        public void VisitText(Text text)
        {
            _plainText += text;
        }

        #endregion
    }
}
