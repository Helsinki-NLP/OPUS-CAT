using Avalonia.Platform.Storage;
using OpusCatMtEngine;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpusCatMtEngine
{
    public class HelperFunctions
    {
        public static string EnumToString(object value)
        {
            string EnumString;
            try
            {
                EnumString = Enum.GetName((value.GetType()), value);
                return Regex.Replace(EnumString, "_", " ");
            }
            catch
            {
                return string.Empty;
            }
        }

        

        public static FilePickerFileType ZipFilePickerType { get; } = new("Zip files")
        {
            Patterns = new[] { "*.zip" },
            MimeTypes = new[] { "application/x-zip" }
            //AppleUniformTypeIdentifiers = new[] { "public.image" },
        };


        public static FilePickerFileType TmxFilePickerType { get; } = new("Tmx files")
        {
            Patterns = new[] { "*.tmx" },
            MimeTypes = new[] { "text/xml" }
        };

        public static FilePickerFileType YmlFilePickerType { get; } = new("Yml files")
        {
            Patterns = new[] { "*.yml","*.yaml" },
            MimeTypes = new[] { "application/x-yaml" }
        };

        public static FilePickerFileType TbxFilePickerType { get; } = new ("Tbx files")
        {
            Patterns = new[] { "*.tbx" },
            MimeTypes = new[] { "text/xml" }
        };

        public static string GetOpusCatDataPath(string restOfPath=null)
        {
            if (OpusCatMtEngineSettings.Default.StoreOpusCatDataInLocalAppdata)
            {
                return GetLocalAppDataPath(restOfPath);
            }
            else
            {
                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (restOfPath == null)
                {
                    return Path.Combine(
                        assemblyFolder,
                        OpusCatMtEngineSettings.Default.LocalOpusCatDir);
                }
                else
                {
                    return Path.Combine(
                        assemblyFolder,
                        OpusCatMtEngineSettings.Default.LocalOpusCatDir,
                        restOfPath);
                }
            }

        }

        public static string GetLocalAppDataPath(string restOfPath)
        {
            if (restOfPath == null)
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    OpusCatMtEngineSettings.Default.LocalOpusCatDir);
            }
            else
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    OpusCatMtEngineSettings.Default.LocalOpusCatDir,
                    restOfPath);
            }
        }

        public static void SplitToFiles(List<Tuple<string, string>> biText, string srcPath, string trgPath)
        {
            Regex linebreakRegex = new Regex(@"\r\n?|\n");
            using (var srcStream = new StreamWriter(srcPath, true, Encoding.UTF8))
            using (var trgStream = new StreamWriter(trgPath, true, Encoding.UTF8))
            {
                foreach (var pair in biText)
                {
                    //Make sure to remove line breaks from the items before writing them, otherwise the line
                    //breaks can mess marian processing up
                    srcStream.WriteLine(linebreakRegex.Replace(pair.Item1, " "));
                    trgStream.WriteLine(linebreakRegex.Replace(pair.Item2, " "));
                }
            }
        }

        internal static ParallelFilePair GetTatoebaFileInfos(string sourceCode, string targetCode)
        {
            var processDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var testsets = Directory.GetDirectories(Path.Combine(processDir,OpusCatMtEngineSettings.Default.TatoebaDir));            
            var testsetDir = testsets.SingleOrDefault(
                x => x.EndsWith($"{sourceCode}-{targetCode}") || x.EndsWith($"{targetCode}-{sourceCode}"));
            if (testsetDir == null)
            {
                return null;
            }
            else
            {
                var source = Directory.GetFiles(testsetDir, $"tatoeba.{sourceCode}.txt").Select(x => new FileInfo(x)).Single();
                var target = Directory.GetFiles(testsetDir, $"tatoeba.{targetCode}.txt").Select(x => new FileInfo(x)).Single();
                return new ParallelFilePair(source, target);
            }
        }

        internal static FileInfo CombineFiles(FileInfo file1, FileInfo file2, string combinedPath, int file1Lines, int file2Lines)
        {
            FileInfo combinedFile = new FileInfo(combinedPath);
            using (var combinedWriter = combinedFile.CreateText())
            using (var reader1 = file1.OpenText())
            using (var reader2 = file2.OpenText())
            {
                string line;
                while (file1Lines != 0 && (line = reader1.ReadLine()) != null)
                {
                    combinedWriter.WriteLine(line);
                    file1Lines--;
                }
                while (file2Lines != 0 && (line = reader2.ReadLine()) != null)
                {
                    combinedWriter.WriteLine(line);
                    file1Lines--;
                }
            }

            return combinedFile;
        }

        //Split a file pair into two randomly (used for separating a validation set from training set)
        internal static (ParallelFilePair pair1, ParallelFilePair pair2) SplitFilePair(ParallelFilePair filePair, int pair2Size)
        {
            //First need to get the linecount of the file pair
            var lines = 0;
            using (var reader = filePair.Source.OpenText())
            {
                while (reader.ReadLine() != null)
                {
                    lines++;
                }
            }

            ParallelFilePair pair1 = 
                new ParallelFilePair(
                    $"{filePair.Source.DirectoryName}{Path.DirectorySeparatorChar}split1.{filePair.Source.Name}",
                    $"{filePair.Target.DirectoryName}{Path.DirectorySeparatorChar}split1.{filePair.Target.Name}");
            ParallelFilePair pair2 =
                new ParallelFilePair(
                    $"{filePair.Source.DirectoryName}{Path.DirectorySeparatorChar}split2.{filePair.Source.Name}",
                    $"{filePair.Target.DirectoryName}{Path.DirectorySeparatorChar}split2.{filePair.Target.Name}");

            var nthLine = lines / pair2Size;

            var writtenLines = 0;
            string sourceLine, targetLine;
            using (var sourcereader = filePair.Source.OpenText())
            using (var sourcewriter1 = pair1.Source.CreateText())
            using (var sourcewriter2 = pair2.Source.CreateText())
            using (var targetreader = filePair.Target.OpenText())
            using (var targetwriter1 = pair1.Target.CreateText())
            using (var targetwriter2 = pair2.Target.CreateText())
            {
                while
                    (((sourceLine = sourcereader.ReadLine()) != null) &&
                    ((targetLine = targetreader.ReadLine()) != null))
                {
                    if (writtenLines % nthLine == 0)
                    {
                        sourcewriter2.WriteLine(sourceLine);
                        targetwriter2.WriteLine(targetLine);
                    }
                    else
                    {
                        sourcewriter1.WriteLine(sourceLine);
                        targetwriter1.WriteLine(targetLine);
                    }
                    writtenLines++;
                }
            }

            return (pair1, pair2);

        }

        internal static ParallelFilePair GenerateDummyOODValidSet(DirectoryInfo modelDir)
        {
            FileInfo dummySource = new FileInfo(Path.Combine(modelDir.FullName,"dummyOOD.source"));
            FileInfo dummyTarget = new FileInfo(Path.Combine(modelDir.FullName, "dummyOOD.target"));

            using (var sourceWriter = dummySource.CreateText())
            using (var targetWriter = dummyTarget.CreateText())
            {
                for (var i = 0; i < OpusCatMtEngineSettings.Default.OODValidSetSize;i++)
                {
                    sourceWriter.WriteLine("0");
                    targetWriter.WriteLine("0");
                }
            }

            return new ParallelFilePair(dummySource, dummyTarget);
        }

        internal static string FixOpusYaml(string yamlString, string model)
        {
            yamlString = Regex.Replace(yamlString, "- (>>[^<]+<<)", "- \"$1\"");
            yamlString = Regex.Replace(yamlString, "(?<!- )'(>>[^<]+<<)'", "- \"$1\"");
            if (Regex.Match(yamlString, @"(?<!- )devset = top").Success)
            {
                Log.Information($"Corrupt yaml line in model {model} yaml file, applying fix");
                yamlString = Regex.Replace(yamlString, @"(?<!- )devset = top", "devset: top");

            }
            if (yamlString.Contains("unused dev/test data is added to training data"))
            {
                Log.Information($"Corrupt yaml line in model {model} yaml file, applying fix");
                yamlString = Regex.Replace(
                        yamlString,
                        @"unused dev/test data is added to training data",
                        "other: unused dev/test data is added to training data");
            }

            return yamlString;
        }
    }
}
