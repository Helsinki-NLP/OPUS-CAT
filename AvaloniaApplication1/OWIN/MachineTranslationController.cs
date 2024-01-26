﻿using OpusMTInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.ServiceModel;
using System.Web;
using System.Net.Http;
using SentenceSplitterNet;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OpusCatMtEngine
{
    public class MtRestServiceController : ControllerBase
    {
        private readonly IMtProvider mtProvider;

        public MtRestServiceController(IMtProvider mtProvider)
        {
            this.mtProvider = mtProvider;
        }
        
        /// <summary>
        /// Call this method to get a token code for the further
        /// calls.Returns the token code is the credentials are
        /// valid (this dummy service allows the login request
        /// if the user name and the password are identical), 
        /// otherwise returns null.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The password.</param>
        /// <returns>The token code if the credentials are valid, null otherwise.</returns>
        /// 
        [HttpGet]
        public string Login(string userName, string password)
        {
            return userName.Equals(password) ? TokenCodeGenerator.Instance.GenerateTokenCode(userName) : null;
        }

        [HttpGet]
        public List<string> GetLanguagePairModelTags(string tokenCode, string srcLangCode, string trgLangCode)
        {
            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return null;

            return this.mtProvider.GetLanguagePairModelTags(srcLangCode, trgLangCode);
        }

        /// <summary>
        /// Call this method to get the supported languages of the service.
        /// </summary>
        /// <param name="tokenCode">The token code.</param>
        /// <returns>The supported languages.</returns>
        [HttpGet]
        public List<string> ListSupportedLanguagePairs(string tokenCode=null)
        {

            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return null;

            return this.mtProvider.GetAllLanguagePairs();
        }

        [HttpGet]
        public string CheckModelStatus(string tokenCode, string srcLangCode, string trgLangCode, string modelTag)
        {
            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return null;

            var sourceLang = new IsoLanguage(srcLangCode);
            var targetLang = new IsoLanguage(trgLangCode);

            return this.mtProvider.CheckModelStatus(sourceLang, targetLang, modelTag);
        }

        /// <summary>
        /// Call this method to get the translation for a single string.
        /// </summary>
        /// <param name="tokenCode">The token code.</param>
        /// <param name="input">The input string.</param>
        /// <param name="srcLangCode">The code of the source language.</param>
        /// <param name="trgLangCode">The code of the target language.</param>
        /// <returns>The translated input string.</returns>
        [HttpGet]
        public string Translate(string tokenCode = "", string input = "", string srcLangCode = "", string trgLangCode = "", string modelTag = "")
        {
            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return null;

            var sourceLang = new IsoLanguage(srcLangCode);
            var targetLang = new IsoLanguage(trgLangCode);

            return this.mtProvider.Translate(input, sourceLang, targetLang, modelTag).Result.Translation;
        }

        //For integration with Wordfast
        [HttpPost]
        public Translation TranslatePost(string tokenCode = "", string input = "", string srcLangCode = "", string trgLangCode = "", string modelTag = "")
        {
            var translation = this.Translate(tokenCode, input, srcLangCode, trgLangCode, modelTag);
            return new Translation(translation);
        }
        

        [HttpGet]
        public IActionResult TranslateJson(
            string tokenCode="", 
            string input="", 
            string srcLangCode="", 
            string trgLangCode="", 
            string modelTag="",
            Boolean inputIsSingleSentence=true)
        {
            if (input == null)
            {
                input = "";
            }
            //HttpContext.Current.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            var sourceLang = new IsoLanguage(srcLangCode);
            var targetLang = new IsoLanguage(trgLangCode);

            List<string> sentencesToTranslate;
            if (inputIsSingleSentence)
            {
                sentencesToTranslate = new List<string>() { input };
            }
            else
            {
                var splitter = new SentenceSplitter(sourceLang.ShortestIsoCode);
                sentencesToTranslate = splitter.Split(input);
            }
            
            
            TranslationPair finalTranslation = null;
            foreach (var sentence in sentencesToTranslate)
            {
                var translationPart = this.mtProvider.Translate(sentence, sourceLang, targetLang, modelTag);
                if (finalTranslation == null)
                {
                    finalTranslation = translationPart.Result;
                }
                else
                {
                    finalTranslation.AppendTranslationPair(translationPart.Result);
                }
            }
            
            //var response = Request.CreateResponse<TranslationPair>(HttpStatusCode.OK, finalTranslation);
            //response.Headers.Add("Access-Control-Allow-Origin", "*");
            return new OkObjectResult(finalTranslation);

            //return new Translation(translation.Result.Translation);
        }

        [HttpGet]
        public string TranslateParagraphJson(string tokenCode = "", string input = "", string srcLangCode = "", string trgLangCode = "", string modelTag = "", Boolean segmentedInput = true)
        {
            if (input == null)
            {
                input = "";
            }
            //HttpContext.Current.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            var sourceLang = new IsoLanguage(srcLangCode);
            var targetLang = new IsoLanguage(trgLangCode);

            var splitter = new SentenceSplitter(sourceLang.ShortestIsoCode);
            var sentencesToTranslate = splitter.Split(input);

            StringBuilder translationBuilder = new StringBuilder();

            foreach (var sentence in sentencesToTranslate)
            {
                var translation = this.mtProvider.Translate(sentence, sourceLang, targetLang, modelTag);
                translationBuilder.Append(translation.Result.Translation + " ");
            }

            return translationBuilder.ToString().Trim();
            
        }


        [HttpGet]
        public Stream TranslateStream(string tokenCode="", string input="", string srcLangCode="", string trgLangCode="", string modelTag="")
        {
            var sourceLang = new IsoLanguage(srcLangCode);
            var targetLang = new IsoLanguage(trgLangCode);

            /*WebOperationContext.Current.OutgoingResponse.ContentType = "text/plain; charset=utf-8";
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Connection: close");
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin: *");

            //This is for Wordfast Anywhere (probably other versions as well) compatibility, for some reason it doesn't accept a response with
            //the default Server header.
            WebOperationContext.Current.OutgoingResponse.Headers.Add(HttpResponseHeader.Server.ToString(), string.Empty);
            */
            var translation = this.mtProvider.Translate(input, sourceLang, targetLang, modelTag).Result;
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(translation.Translation));
            return stream;
            //var response = Request.CreateResponse<Stream>(HttpStatusCode.OK, stream);
            //response.Headers.Add("Access-Control-Allow-Origin", "*");
            

            //return response;
        }


        /// <summary>
        /// Call this method to get the translation for multiple strings in batch.
        /// NOTE: this is not currently used, for pretranslation PretranslateBatch is better.
        /// </summary>
        /// <param name="tokenCode">The token code.</param>
        /// <param name="input">The input strings.</param>
        /// <param name="srcLangCode">The code of the source language.</param>
        /// <param name="trgLangCode">The code of the target language.</param>
        /// <returns>The translated input strings.</returns>

        [HttpGet]
        public List<string> BatchTranslate(string tokenCode, List<string> input, string srcLangCode, string trgLangCode, string modelTag)
        {

            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return null;

            var sourceLang = new IsoLanguage(srcLangCode);
            var targetLang = new IsoLanguage(trgLangCode);

            List<TranslationPair> translations = new List<TranslationPair>();
            foreach (var sourceSegment in input)
            {
                translations.Add(this.mtProvider.Translate(sourceSegment, sourceLang, targetLang, modelTag).Result);
            }

            return translations.Select(x => x.Translation).ToList();
        }

        /// <summary>
        /// This will send a batch to the MT engine for pretranslation, which means
        /// the translations for the batch will be immediately available when requested
        /// </summary>
        /// <param name="tokenCode"></param>
        /// <param name="input"></param>
        /// <param name="srcLangCode"></param>
        /// <param name="trgLangCode"></param>
        /// 
        [HttpPost]
        public IActionResult PreOrderBatch([FromBody] List<string> input, string tokenCode="", string srcLangCode="", string trgLangCode="", string modelTag="")
        {
           
            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return new UnauthorizedResult();

            var sourceLang = new IsoLanguage(srcLangCode);
            var targetLang = new IsoLanguage(trgLangCode);

            if (input.Count == 0)
            {
                return new BadRequestResult();
            }

            foreach (var inputString in input)
            {
                this.mtProvider.Translate(inputString, sourceLang, targetLang, modelTag);
            }
            
            /* Batch preordering was done earlier with batch translation, but it doesn't seem
             * to be much quicker than normal translation, and it has the problem of providing all
             * the translations at once in the end. Using normal translation means the MT is ready
             * as soon as a sentence gets translated (you could do this for batch translation as well
             * by adding an outputline handler, but it's not implemented yet). Batch translation should be
             * much quicker, need to test for correct parameters, so stick with this. Using normal translate
             * is also more robust, one less thing to break.
            if (!this.ModelManager.BatchTranslationOngoing && !this.ModelManager.CustomizationOngoing)
            {
                this.ModelManager.PreTranslateBatch(input, sourceLang, targetLang, modelTag);
                return "batch translation started";
            }
            else
            {
                return "batch translation or customization already in process";
            }*/

            return new OkResult();
        }

        [HttpPost]
        public IActionResult Customize(
            [FromBody] FinetuningJob finetuningJob,
            string tokenCode,
            string srcLangCode,
            string trgLangCode,
            string modelTag)
        {
            if (!TokenCodeGenerator.Instance.TokenCodeIsValid(tokenCode))
                return null;

            var sourceLang = new IsoLanguage(srcLangCode);
            var targetLang = new IsoLanguage(trgLangCode);

            if (!this.mtProvider.FinetuningOngoing && !this.mtProvider.BatchTranslationOngoing)
            {
                this.mtProvider.StartCustomization(
                    finetuningJob.Input,
                    finetuningJob.Validation,
                    finetuningJob.UniqueNewSegments, 
                    sourceLang, targetLang, modelTag,
                    finetuningJob.IncludePlaceholderTags,
                    finetuningJob.IncludeTagPairs,null);
                return new OkResult();
            }
            else
            {
                //TODO: need to queue up customization, i.e. save data for starting later
                return StatusCode(503); //code for service unavailable
            }
        }


    }
}
