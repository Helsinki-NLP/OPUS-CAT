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
        
        public static ParallelFilePair ParseTmxToParallelFiles(string tmxFile, string sourceCode, string targetCode)
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
                            x => x.Parent.Attribute(XNamespace.Xml + "lang").Value == sourceCode);
                    var targetSeg =
                        tu.Descendants("seg").FirstOrDefault(
                            x => x.Parent.Attribute(XNamespace.Xml + "lang").Value == sourceCode);
                    if (sourceSeg != null && targetSeg != null)
                    {
                        var sourceText = String.Join("",sourceSeg.DescendantNodes().OfType<XText>().Select(x => x.Value));
                        sourceWriter.WriteLine(sourceText);
                        var targetText = String.Join("", targetSeg.DescendantNodes().OfType<XText>().Select(x => x.Value));
                        sourceWriter.WriteLine(targetText);
                    }
                }
            }

            return new ParallelFilePair(sourceFile, targetFile);
        }

    }
}
