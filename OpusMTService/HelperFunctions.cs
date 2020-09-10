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

        internal static Tuple<FileInfo,FileInfo> GetTatoebaFileInfos(string sourceCode, string targetCode)
        {
            var testsets = Directory.GetDirectories(FiskmoMTEngineSettings.Default.TatoebaDir);
            var testsetDir = testsets.Single(
                x => x.EndsWith($"{sourceCode}-{targetCode}") || x.EndsWith($"{targetCode}-{sourceCode}"));
            var source = Directory.GetFiles(testsetDir, $"tatoeba.{sourceCode}.txt").Select(x => new FileInfo(x)).Single();
            var target = Directory.GetFiles(testsetDir, $"tatoeba.{targetCode}.txt").Select(x => new FileInfo(x)).Single();
            return new Tuple<FileInfo, FileInfo>(source, target);
        }
    }
}
