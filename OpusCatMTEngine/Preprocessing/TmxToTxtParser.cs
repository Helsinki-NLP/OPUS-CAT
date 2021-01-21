using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace OpusCatMTEngine
{
    public static class TmxToTxtParser
    {

        //Tmx may contain tags. They can be converted to tag tokens for use in training or omitted.
        private static string FilterTextAndTags(
            XElement seg,
            bool includePlaceholderTags,
            bool includeTagPairs)
        {
            StringBuilder segText = new StringBuilder();

            var nodeStack = new Stack<XNode>();
            nodeStack.Push(seg);

            while (nodeStack.Count > 0)
            {
                XNode top = nodeStack.Pop();

                if (top is XText)
                {
                    //The text node may contain escaped characters, parse them
                    //by using the Value property.
                    var unescapedText = ((XText)top).Value;
                    
                    segText.Append(((XText)top).Value);
                    continue;
                }

                var topElement = top as XElement;
                if (topElement != null)
                {
                    if (topElement.Name == "bpt")
                    {
                        if (includeTagPairs)
                        {
                            segText.Append(" TAGPAIRSTART ");
                        }
                    }
                    else if (topElement.Name == "ept")
                    {
                        if (includeTagPairs)
                        {
                            segText.Append(" TAGPAIREND ");
                        }
                    }
                    else if ((topElement.Name == "ph") ||
                            (topElement.Name == "it") ||
                            (topElement.Name == "ut"))
                    {
                        if (includePlaceholderTags)
                        {
                            segText.Append(" PLACEHOLDER ");
                        }
                        else
                        {
                            //Placeholder tags may be attached to neighboring tokens
                            //Add space to avoid concatenation. Extra spaces should be
                            //cleaned up during preprocessing anyway.
                            segText.Append(" ");
                        }
                    }
                    //This handles elements with inner structure that should be included in the training corpus
                    else
                    {
                        foreach (var node in topElement.Nodes().Reverse())
                        {
                            nodeStack.Push(node);
                        }
                    }
                }
            }

            var uncleanText = segText.ToString();

            //The text may contain tags (as result of unescaping entities), remove them
            var taglessText = Regex.Replace(uncleanText,"<[^> ]+>","");

            //Segments may have multiple line breaks in them, especially segments from old TMs.
            //Simply remove the line breaks to make sure the lines different language files sync.
            //It might be best to skip these segments altogether, since they probably won't
            //benefit training.
            var delinedText = Regex.Replace(taglessText, "[\r\n]", " ");

            //There may still be some residual entities (like if entities have been encoded twice, such as &auml
            //becoming &amp;auml), convert those

            var unescaped = HttpUtility.HtmlDecode(delinedText);
            return unescaped;
        }
        
        public static ParallelFilePair ParseTmxToParallelFiles(
            string tmxFile, 
            IsoLanguage sourceLang,
            IsoLanguage targetLang,
            bool includePlaceholderTags,
            bool includeTagPairs)
        {
            var sourceFile = new FileInfo($"{tmxFile}.{sourceLang.ShortestIsoCode}.txt");
            var targetFile = new FileInfo($"{tmxFile}.{targetLang.ShortestIsoCode}.txt");

            XDocument tmx;
            try
            {
                tmx = XDocument.Load(tmxFile);
            }
            catch (System.Xml.XmlException ex)
            {
                return null;
            }
            
            var tus = tmx.Descendants("tu");

            using (var sourceWriter = sourceFile.CreateText())
            using (var targetWriter = targetFile.CreateText())
            {
                foreach (var tu in tus)
                {
                    var sourceSeg =
                        tu.Descendants("seg").FirstOrDefault(
                            x => sourceLang.IsCompatibleTmxLang(x.Parent.Attribute(XNamespace.Xml + "lang").Value.ToLower()));
                    var targetSeg =
                        tu.Descendants("seg").FirstOrDefault(
                            x => targetLang.IsCompatibleTmxLang(x.Parent.Attribute(XNamespace.Xml + "lang").Value.ToLower()));
                    if (sourceSeg != null && targetSeg != null)
                    {
                        var sourceText = TmxToTxtParser.FilterTextAndTags(sourceSeg, includePlaceholderTags, includeTagPairs);
                        sourceWriter.WriteLine(sourceText);
                        var targetText = TmxToTxtParser.FilterTextAndTags(targetSeg, includePlaceholderTags, includeTagPairs);
                        targetWriter.WriteLine(targetText);
                    }
                }
            }

            return new ParallelFilePair(sourceFile, targetFile);
        }

    }
}
