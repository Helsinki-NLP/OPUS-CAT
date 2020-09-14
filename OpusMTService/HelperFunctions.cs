using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiskmoMTEngine
{
    class HelperFunctions
    {
        public static string GetLocalAppDataPath(string restOfPath)
        {
            return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    FiskmoMTEngineSettings.Default.LocalFiskmoDir,
                    restOfPath);
        }

        internal static ParallelFilePair GetTatoebaFileInfos(string sourceCode, string targetCode)
        {
            var testsets = Directory.GetDirectories(FiskmoMTEngineSettings.Default.TatoebaDir);
            var testsetDir = testsets.Single(
                x => x.EndsWith($"{sourceCode}-{targetCode}") || x.EndsWith($"{targetCode}-{sourceCode}"));
            var source = Directory.GetFiles(testsetDir, $"tatoeba.{sourceCode}.txt").Select(x => new FileInfo(x)).Single();
            var target = Directory.GetFiles(testsetDir, $"tatoeba.{targetCode}.txt").Select(x => new FileInfo(x)).Single();
            return new ParallelFilePair(source, target);
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
    }
}
