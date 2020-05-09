using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FiskmoMTEngine
{
    class MarianHelper
    {
        internal static Process StartProcessWithCmd(string fileName, string args)
        {
            var serviceDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process ExternalProcess = new Process();

            ExternalProcess.StartInfo.FileName = "cmd";
            ExternalProcess.StartInfo.Arguments = $"/c {fileName} {args}";
            ExternalProcess.StartInfo.UseShellExecute = false;
            //ExternalProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;

            ExternalProcess.StartInfo.WorkingDirectory = serviceDir;
            //ExternalProcess.StartInfo.RedirectStandardInput = true;
            //ExternalProcess.StartInfo.RedirectStandardOutput = true;
            //ExternalProcess.StartInfo.RedirectStandardError = true;
            //ExternalProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            //ExternalProcess.ErrorDataReceived += errorDataHandler;

            ExternalProcess.StartInfo.CreateNoWindow = false;

            ExternalProcess.Start();
            //ExternalProcess.BeginErrorReadLine();

            //ExternalProcess.StandardInput.AutoFlush = true;

            return ExternalProcess;
        }

        internal static FileInfo PreprocessLanguage(FileInfo languageFile, DirectoryInfo directory, string languageCode, FileInfo spmModel)
        {
            var preprocessedFile = new FileInfo(Path.Combine(directory.FullName, $"preprocessed_{languageFile.Name}"));
            var spFile = new FileInfo(Path.Combine(directory.FullName, $"sp_{languageFile.Name}"));


            using (var rawFile = languageFile.OpenText())
            using (var preprocessedWriter = new StreamWriter(preprocessedFile.FullName))
            {
                String line;
                while ((line = rawFile.ReadLine()) != null)
                {
                    var preprocessedLine =
                        MosesPreprocessor.RunMosesPreprocessing(line, languageCode);
                    preprocessedLine = MosesPreprocessor.PreprocessSpaces(preprocessedLine);
                    preprocessedWriter.WriteLine(preprocessedLine);
                }
            }

            var spArgs = $"{preprocessedFile.FullName} --model {spmModel.FullName} --output {spFile.FullName}";
            MarianHelper.StartProcessWithCmd("spm_encode.exe", spArgs);

            return spFile;
        }
    }
}
