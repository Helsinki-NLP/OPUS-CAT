using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FiskmoMTEngine
{
    class MarianHelper
    {

        internal static Process StartProcessInBackgroundWithRedirects(string fileName, string args)
        {
            var pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process ExternalProcess = new Process();

            ExternalProcess.StartInfo.FileName = "cmd";
            ExternalProcess.StartInfo.Arguments = $"/c {fileName} {args}";
            ExternalProcess.StartInfo.UseShellExecute = false;
            //ExternalProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;

            ExternalProcess.StartInfo.WorkingDirectory = pluginDir;
            ExternalProcess.StartInfo.RedirectStandardInput = true;
            ExternalProcess.StartInfo.RedirectStandardOutput = true;
            ExternalProcess.StartInfo.RedirectStandardError = true;
            ExternalProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            ExternalProcess.ErrorDataReceived += errorDataHandler;

            ExternalProcess.StartInfo.CreateNoWindow = true;
            //ExternalProcess.StartInfo.CreateNoWindow = false;

            ExternalProcess.Start();
            ExternalProcess.BeginErrorReadLine();

            ExternalProcess.StandardInput.AutoFlush = true;

            return ExternalProcess;
        }

        internal static Process StartProcessInWindow(string fileName, string args)
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
            ExternalProcess.EnableRaisingEvents = true;
            //ExternalProcess.BeginErrorReadLine();

            AppDomain.CurrentDomain.ProcessExit += (x, y) => CurrentDomain_ProcessExit(x, y, ExternalProcess) ;

            return ExternalProcess;
        }

        private static void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
                    ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e, Process externalProcess)
        {
            KillProcessAndChildren(externalProcess.Id);
        }

        private static void errorDataHandler(object sender, DataReceivedEventArgs e)
        {
            Log.Information(e.Data);
        }

        internal static string PreprocessLine(
            string line, 
            string languageCode, 
            bool includePlaceholderTags, 
            bool includeTagPairs)
        {
            if (!includePlaceholderTags)
            {
                line = Regex.Replace(line, @"PLACEHOLDER\d*", "");
            }

            if (!includeTagPairs)
            {
                line = Regex.Replace(line, @"TAGPAIRSTART\d*", "");
                line = Regex.Replace(line, @"TAGPAIREND\d*", "");
            }

            var preprocessedLine =
                MosesPreprocessor.RunMosesPreprocessing(line, languageCode);
            preprocessedLine = MosesPreprocessor.PreprocessSpaces(preprocessedLine);

            return preprocessedLine;
        }

        internal static FileInfo PreprocessLanguage(
            FileInfo languageFile,
            DirectoryInfo directory, 
            string languageCode, 
            FileInfo spmModel,
            bool includePlaceholderTags,
            bool includeTagPairs)
        {
            var preprocessedFile = new FileInfo(Path.Combine(directory.FullName, $"preprocessed_{languageFile.Name}"));
            var spFile = new FileInfo(Path.Combine(directory.FullName, $"sp_{languageFile.Name}"));


            using (var rawFile = languageFile.OpenText())
            using (var preprocessedWriter = new StreamWriter(preprocessedFile.FullName))
            {
                String line;
                while ((line = rawFile.ReadLine()) != null)
                {
                    var preprocessedLine = MarianHelper.PreprocessLine(line, languageCode, includePlaceholderTags, includeTagPairs);
                    preprocessedWriter.WriteLine(preprocessedLine);
                }
            }

            var spArgs = $"{preprocessedFile.FullName} --model {spmModel.FullName} --output {spFile.FullName}";
            MarianHelper.StartProcessInWindow("spm_encode.exe", spArgs);

            return spFile;
        }
    }
}
