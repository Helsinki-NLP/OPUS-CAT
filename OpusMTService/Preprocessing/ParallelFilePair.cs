using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpusCatMTEngine
{
    //A convenience class for handling bilingual file pairs, i.e. the same content in two languages, in two parallel files
    public class ParallelFilePair
    {
        
        public ParallelFilePair(FileInfo source, FileInfo target)
        {
            this.Source = source;
            this.Target = target;
        }

        //combine two file pairs
        public ParallelFilePair(ParallelFilePair pair1, ParallelFilePair pair2, string combinedPath, int pair1Lines=1000, int pair2Lines=1000)
        {
            this.Source = HelperFunctions.CombineFiles(
                pair1.Source, 
                pair2.Source, 
                Path.Combine(combinedPath,"combined.source"),
                pair1Lines,
                pair2Lines);
            this.Target = HelperFunctions.CombineFiles(
                pair1.Target, 
                pair2.Target,
                Path.Combine(combinedPath, "combined.target"),
                pair1Lines,
                pair2Lines);
        }
        
        public ParallelFilePair(string sourcePath, string targetPath)
        {
            this.Source = new FileInfo(sourcePath);
            this.Target = new FileInfo(targetPath);
        }

        public ParallelFilePair(List<Tuple<string, string>> biText, string srcPath, string trgPath)
        {
            Regex linebreakRegex = new Regex(@"\r\n?|\n");
            FileInfo srcFile = new FileInfo(srcPath);
            FileInfo trgFile = new FileInfo(trgPath);
            using (var srcStream = srcFile.CreateText())
            using (var trgStream = trgFile.CreateText())
            {
                foreach (var pair in biText)
                {
                    //Make sure to remove line breaks from the items before writing them, otherwise the line
                    //breaks can mess marian processing up
                    srcStream.WriteLine(linebreakRegex.Replace(pair.Item1, " "));
                    trgStream.WriteLine(linebreakRegex.Replace(pair.Item2, " "));
                }
            }

            this.Source = srcFile;
            this.Target = trgFile;
        }

        public FileInfo Source { get; private set; }
        public FileInfo Target { get; private set; }
    }
}
