using System;
using System.Linq;
using MemoQ.Addins.Common.DataStructures;
using MemoQ.Addins.Common.Utils;
using MemoQ.MTInterfaces;
using OpusCatTranslationProvider;
using OpusCatMtEngine;
using System.Net.Http;
using System.Net;
using System.Web.Services.Description;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace OpusCatMTPlugin
{
    /// <summary>
    /// Session that perform actual translation or storing translations. Created on a segment-by-segment basis, or once for batch operations.
    /// </summary>
    /// <remarks>
    /// Implementation checklist:
    ///     - The MTException class is used to wrap the original exceptions occurred during the translation.
    ///     - All allocated resources are disposed correctly in the session.
    /// </remarks>
    public class OpusCatMTSession : ISession
    {
        
        /// <summary>
        /// The source language.
        /// </summary>
        private readonly string srcLangCode;

        /// <summary>
        /// The target language.
        /// </summary>
        private readonly string trgLangCode;

        /// <summary>
        /// Options of the plugin.
        /// </summary>
        private readonly OpusCatMTOptions options;
        private HttpClient opusCatConnection;

        public OpusCatMTSession(string srcLangCode, string trgLangCode, OpusCatMTOptions options)
        {
            this.srcLangCode = srcLangCode;
            this.trgLangCode = trgLangCode;
            this.options = options;
            this.opusCatConnection = 
                new HttpClient() 
                    { BaseAddress = new Uri($"http://localhost:{options.GeneralSettings.MtServicePort}/MtRestService/") };
        }

        #region ISession Members

        /// <summary>
        /// Translates a single segment, possibly using a fuzzy TM hit for improvement
        /// </summary>
        public TranslationResult TranslateCorrectSegment(Segment segm, Segment tmSource, Segment tmTarget)
        {
            TranslationResult result = new TranslationResult();

            try
            {
                string textToTranslate = createTextFromSegment(segm, FormattingAndTagsUsageOption.Plaintext);
                var response = this.opusCatConnection.GetAsync($"TranslateJson?tokenCode=0&input={HttpUtility.UrlEncode(textToTranslate)}&srcLangCode={this.srcLangCode}&trgLangCode={this.trgLangCode}&modelTag=\"\"");
                var jsonResponse = response.Result.Content;
                TranslationPair translation = JsonConvert.DeserializeObject<TranslationPair>(jsonResponse.ReadAsStringAsync().Result);
                result.Translation = createSegmentFromResult(segm, translation);
            }
            catch (Exception e)
            {
                // Use the MTException class is to wrap the original exceptions occurred during the translation.
                string localizedMessage = LocalizationHelper.Instance.GetResourceString("NetworkError");
                result.Exception = new MTException(string.Format(localizedMessage, e.Message), string.Format("A network error occured ({0}).", e.Message), e);
            }

            return result;
        }

        /// <summary>
        /// Translates multiple segments, possibly using a fuzzy TM hit for improvement
        /// </summary>
        public TranslationResult[] TranslateCorrectSegment(Segment[] segs, Segment[] tmSources, Segment[] tmTargets)
        {
            TranslationResult[] results = new TranslationResult[segs.Length];

            try
            {
                var texts = segs.Select(s => createTextFromSegment(s, FormattingAndTagsUsageOption.Plaintext)).ToList();
                int i = 0;
                foreach (string textToTranslate in texts) {
                    var response = this.opusCatConnection.GetAsync($"TranslateJson?tokenCode=0&input={HttpUtility.UrlEncode(textToTranslate)}&srcLangCode={this.srcLangCode}&trgLangCode={this.trgLangCode}&modelTag=\"\"");
                    var jsonResponse = response.Result.Content;
                    TranslationPair translation = JsonConvert.DeserializeObject<TranslationPair>(jsonResponse.ReadAsStringAsync().Result);
                    results[i] = new TranslationResult();
                    results[i].Translation = createSegmentFromResult(segs[i], translation);
                    i++;
                }
            }
            catch (Exception e)
            {
                // Use the MTException class is to wrap the original exceptions occurred during the translation.
                for (var i = 0;i < results.Count();i++)
                {
                    if (results[i] == null)
                    {
                        results[i] = new TranslationResult();
                    }

                    string localizedMessage = LocalizationHelper.Instance.GetResourceString("NetworkError");
                    results[i].Exception = new MTException(string.Format(localizedMessage, e.Message), string.Format("A network error occured ({0}).", e.Message), e);
                }
            }

            return results;
        }

        /// <summary>
        /// Creates the text to translate from the segment according to the settings. Appends tags and formatting if needed.
        /// </summary>
        private string createTextFromSegment(Segment segment, FormattingAndTagsUsageOption formattingAndTagOption)
        {
            switch (formattingAndTagOption)
            {
                case FormattingAndTagsUsageOption.OnlyFormatting:
                    return SegmentHtmlConverter.ConvertSegment2Html(segment, false, true);
                case FormattingAndTagsUsageOption.BothFormattingAndTags:
                    return SegmentHtmlConverter.ConvertSegment2Html(segment, true, true);
                default:
                    return segment.PlainText;
            }
        }

        private Segment createSegmentFromResult(Segment originalSegment, TranslationPair translation)
        {
            if (options.GeneralSettings.RestoreTags)
            {
                var xmlSegmentString = SegmentXMLConverter.ConvertSegment2Xml(originalSegment, true, true);
                var restorer = new TagRestorer(xmlSegmentString, translation);
                var target = restorer.AddTagsByAlignment();
                var targetSegment = SegmentXMLConverter.ConvertXML2Segment(target, originalSegment.ITags);
                var normalizedTargetSegment =
                    TagWhitespaceNormalizer.NormalizeWhitespaceAroundTags(originalSegment, targetSegment, this.srcLangCode, this.trgLangCode);
                return normalizedTargetSegment;
            }
            else
            {
                return SegmentBuilder.CreateFromString(translation.translation);
            }

        }

        #endregion


        #region IDisposable Members

        public void Dispose()
        {
            // dispose your resources if needed
        }

        #endregion
    }
}
