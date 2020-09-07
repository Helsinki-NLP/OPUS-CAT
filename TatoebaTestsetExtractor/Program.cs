using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TatoebaTestsetExtractor
{
    class Program
    {

        //This is a simple program for extracting a smallish testsets from the
        //Tatoebe Challenge test sets (https://github.com/Helsinki-NLP/Tatoeba-Challenge/tree/master/data/test)
        static void Main(string[] args)
        {
            var testdir = args[0];

            var outputdir = new DirectoryInfo("filtered_test");
            if (!outputdir.Exists)
            {
                outputdir.Create();
            }

            foreach (DirectoryInfo dir in Directory.EnumerateDirectories(testdir).Select(x => new DirectoryInfo(x)))
            {
                var langdir = dir.Name.Split('-');
                var sourceLang = Program.ConvertIsoCode(langdir[0]);
                var targetLang = Program.ConvertIsoCode(langdir[1]);

                if (sourceLang != null && targetLang != null && sourceLang != targetLang)
                {
                    Program.FilterTestFile(dir.GetFiles("test.txt").Single(),outputdir,sourceLang,targetLang);
                }

            }

            
        }


        private static void FilterTestFile(FileInfo wholeTestFile, DirectoryInfo outputdir,string sourceLang, string targetLang)
        {
            //Take the first 1000
            
            int filtercount = 0;
            List<string> sourceLines = new List<string>();
            List<string> targetLines = new List<string>();
            using (var reader = wholeTestFile.OpenText())
            {
                while (!reader.EndOfStream)
                {
                    if (filtercount > 500)
                    {
                        var langpairdir = outputdir.CreateSubdirectory($"{sourceLang}-{targetLang}");
                        langpairdir.Create();
                        var sourceOutput = new FileInfo(Path.Combine(langpairdir.FullName, $"tatoeba.{sourceLang}.txt"));
                        var targetOutput = new FileInfo(Path.Combine(langpairdir.FullName, $"tatoeba.{targetLang}.txt"));
                        using (var sourceWriter = sourceOutput.CreateText())
                        using (var targetWriter = targetOutput.CreateText())
                        {
                            sourceWriter.Write(String.Join("\n",sourceLines));
                            targetWriter.Write(String.Join("\n",targetLines));
                        }
                        break;
                    }
                    else
                    {
                        filtercount++;
                    }

                    var line = reader.ReadLine();
                    var linesplit = line.Split('\t');
                    sourceLines.Add(linesplit[2]);
                    targetLines.Add(linesplit[3]);
                }
            }

        }

        private static string ConvertIsoCode(string name)
        {
            //Strip region code if there is one
            name = Regex.Replace(name, "-[A-Z]{2}", "");

            if (name.Length != 3)
            {
                //throw new ArgumentException("name must be three letters.");
                return null;
            }

            name = name.ToLower();

            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            foreach (CultureInfo culture in cultures)
            {
                if (culture.ThreeLetterISOLanguageName.ToLower() == name)
                {
                    return culture.TwoLetterISOLanguageName.ToLower();
                }
            }

            return null;
        }
    }
}
