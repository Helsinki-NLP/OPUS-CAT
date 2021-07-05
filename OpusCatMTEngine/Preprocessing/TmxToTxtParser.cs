using Serilog;
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
    public class TmxToTxtParser
    {
        public Dictionary<Tuple<string,string>, int> TmxLangCounts { get; private set; }

        public TmxToTxtParser()
        {
            this.TmxLangCounts = new Dictionary<Tuple<string, string>, int>();
        }

        //Tmx may contain tags. They can be converted to tag tokens for use in training or omitted.
        private string FilterTextAndTags(
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
        
        public ParallelFilePair ParseTmxToParallelFiles(
            string tmxFile, 
            IsoLanguage sourceLang,
            IsoLanguage targetLang,
            bool includePlaceholderTags,
            bool includeTagPairs)
        {
            
            var sourceFile = new FileInfo($"{tmxFile}.{sourceLang.ShortestIsoCode}.txt");
            var targetFile = new FileInfo($"{tmxFile}.{targetLang.ShortestIsoCode}.txt");
            
            //TODO: update this to use XMLReader, this might cause ouf of memory errors with
            //very large files
            XDocument tmx;
            try
            {
                tmx = XDocument.Load(tmxFile);
            }
            catch (System.Xml.XmlException ex)
            {
                Log.Error($"{tmxFile} is not a valid tmx file");
                return null;
            }
            
            var tus = tmx.Descendants("tu");

            int extractedPairCount = 0;

            using (var sourceWriter = sourceFile.CreateText())
            using (var targetWriter = targetFile.CreateText())
            {
                foreach (var tu in tus)
                {
                    var segs = tu.Descendants("seg");

                    XElement sourceSeg = null;
                    XElement targetSeg = null;
                    List<string> segLangs = new List<string>();
                    foreach (var seg in segs)
                    {
                        var segLang = seg.Parent.Attribute(XNamespace.Xml + "lang").Value.ToLower();
                        segLangs.Add(segLang);

                        if (sourceLang.IsCompatibleTmxLang(segLang))
                        {
                            sourceSeg = seg;
                        }

                        if (targetLang.IsCompatibleTmxLang(segLang))
                        {
                            targetSeg = seg;
                        }
                    }
                    
                    if (sourceSeg != null && targetSeg != null)
                    {
                        var sourceText = this.FilterTextAndTags(sourceSeg, includePlaceholderTags, includeTagPairs);
                        sourceWriter.WriteLine(sourceText);
                        var targetText = this.FilterTextAndTags(targetSeg, includePlaceholderTags, includeTagPairs);
                        targetWriter.WriteLine(targetText);
                        extractedPairCount++;
                    }

                    foreach (var lang1 in segLangs)
                    {
                        foreach (var lang2 in segLangs)
                        {
                            if (lang1 == lang2)
                            {
                                continue;
                            }

                            var langTuple = new Tuple<string, string>(lang1, lang2);
                            if (this.TmxLangCounts.ContainsKey(langTuple))
                            {
                                this.TmxLangCounts[langTuple] = this.TmxLangCounts[langTuple] + 1;
                            }
                            else
                            {
                                this.TmxLangCounts[langTuple] = 1;
                            }
                        }
                    }
                    
                }
            }

            var filePair = new ParallelFilePair(sourceFile, targetFile);
            filePair.SentenceCount = extractedPairCount;

            return filePair;
        }

    }
}
