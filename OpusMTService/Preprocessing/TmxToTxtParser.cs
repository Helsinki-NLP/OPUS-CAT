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
            foreach (var descendant in seg.DescendantNodes())
            {
                if (descendant is XText)
                {
                    segText.Append(descendant);
                }

                var descendantElement = descendant as XElement;
                if (descendantElement != null)
                {
                    if (includeTagPairs)
                    {
                        if (descendantElement.Name == "bpt")
                        {
                            segText.Append(" TAGPAIRSTART ");
                        }
                        if (descendantElement.Name == "ept")
                        {
                            segText.Append(" TAGPAIREND ");
                        }
                    }

                    if (includePlaceholderTags && descendantElement.Name == "ph")
                    {
                        segText.Append(" PLACEHOLDER ");
                    }

                }
            }

            return segText.ToString();
        }
        
        public static ParallelFilePair ParseTmxToParallelFiles(
            string tmxFile, 
            string sourceCode, 
            string targetCode,
            bool includePlaceholderTags,
            bool includeTagPairs)
        {
            var sourceFile = new FileInfo($"{tmxFile}.{sourceCode}.txt");
            var targetFile = new FileInfo($"{tmxFile}.{targetCode}.txt");
            var tmx = XDocument.Load(tmxFile);
            var tus = tmx.Descendants("tu");

            using (var sourceWriter = sourceFile.CreateText())
            using (var targetWriter = targetFile.CreateText())
            {
                foreach (var tu in tus)
                {
                    var sourceSeg =
                        tu.Descendants("seg").FirstOrDefault(
                            x => x.Parent.Attribute(XNamespace.Xml + "lang").Value.ToLower().StartsWith(sourceCode));
                    var targetSeg =
                        tu.Descendants("seg").FirstOrDefault(
                            x => x.Parent.Attribute(XNamespace.Xml + "lang").Value.ToLower().StartsWith(targetCode));
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
