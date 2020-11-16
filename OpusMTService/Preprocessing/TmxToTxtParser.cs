using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FiskmoMTEngine
{
    public static class TmxToTxtParser
    {

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
                    segText.Append(top);
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
                    else
                    {
                        foreach (var node in topElement.Nodes().Reverse())
                        {
                            nodeStack.Push(node);
                        }
                    }
                }
            }

            return segText.ToString();
        }
        
        public static ParallelFilePair ParseTmxToParallelFiles(
            string tmxFile, 
            IsoLanguage sourceLang,
            IsoLanguage targetLang,
            bool includePlaceholderTags,
            bool includeTagPairs)
        {
            var sourceFile = new FileInfo($"{tmxFile}.{sourceLang.Iso639_3Code}.txt");
            var targetFile = new FileInfo($"{tmxFile}.{targetLang.Iso639_3Code}.txt");
            var tmx = XDocument.Load(tmxFile);
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
