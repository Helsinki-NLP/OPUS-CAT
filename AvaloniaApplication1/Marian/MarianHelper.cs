using Avalonia.Controls.Converters;
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

namespace OpusCatMtEngine
{
    
    class MarianHelper
    {

        internal static Process StartProcessInBackgroundWithRedirects(
            string command,
            EventHandler exitCallback = null,
            DataReceivedEventHandler errorDataHandler = null)
        {
            var fileName = command.Split()[0];
            var args = command.Substring(fileName.Length);
            var pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process ExternalProcess = new Process();

            ExternalProcess.StartInfo.FileName = fileName;
            ExternalProcess.StartInfo.Arguments = args;
            ExternalProcess.StartInfo.UseShellExecute = false;
            //ExternalProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;

            ExternalProcess.StartInfo.WorkingDirectory = pluginDir;
            ExternalProcess.StartInfo.RedirectStandardInput = true;
            ExternalProcess.StartInfo.RedirectStandardOutput = true;
            ExternalProcess.StartInfo.RedirectStandardError = true;
            ExternalProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            //Async error data handler is used for tracking progress
            if (errorDataHandler != null)
            {
                ExternalProcess.ErrorDataReceived += errorDataHandler;
            }
            else
            {
                ExternalProcess.ErrorDataReceived += defaultErrorDataHandler;
            }

            ExternalProcess.StartInfo.CreateNoWindow = true;

            if (exitCallback != null)
            {
                ExternalProcess.Exited += exitCallback;
            }

            ExternalProcess.Start();

            //Add process to job object to make sure it is closed if the engine crashes without
            //calling the exit code.
            //TODO: fix this for crossplatform (see if there's an universal way, otherwise
            //do bespoke for all)
            ChildProcessTracker.AddProcess(ExternalProcess);

            ExternalProcess.EnableRaisingEvents = true;
            ExternalProcess.BeginErrorReadLine();

            ExternalProcess.StandardInput.AutoFlush = true;

            AppDomain.CurrentDomain.ProcessExit += (x, y) => CurrentDomain_ProcessExit(x, y, ExternalProcess);

            return ExternalProcess;
        }

        internal static Process StartProcessDirectlyInBackgroundWithRedirects(
            string command,
            string args,
            EventHandler exitCallback = null,
            DataReceivedEventHandler errorDataHandler = null)
        {
            var pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process ExternalProcess = new Process();

            ExternalProcess.StartInfo.FileName = command;
            ExternalProcess.StartInfo.Arguments = args;
            ExternalProcess.StartInfo.UseShellExecute = false;
            //ExternalProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;

            ExternalProcess.StartInfo.WorkingDirectory = pluginDir;
            ExternalProcess.StartInfo.RedirectStandardInput = true;
            ExternalProcess.StartInfo.RedirectStandardOutput = true;
            ExternalProcess.StartInfo.RedirectStandardError = true;
            ExternalProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            //Async error data handler is used for tracking progress
            if (errorDataHandler != null)
            {
                ExternalProcess.ErrorDataReceived += errorDataHandler;
            }
            else
            {
                ExternalProcess.ErrorDataReceived += defaultErrorDataHandler;
            }

            ExternalProcess.StartInfo.CreateNoWindow = true;

            if (exitCallback != null)
            {
                ExternalProcess.Exited += exitCallback;
            }

            ExternalProcess.Start();

            //Add process to job object to make sure it is closed if the engine crashes without
            //calling the exit code.
            //TODO: fix this
            ChildProcessTracker.AddProcess(ExternalProcess);

            ExternalProcess.EnableRaisingEvents = true;
            ExternalProcess.BeginErrorReadLine();

            ExternalProcess.StandardInput.AutoFlush = true;

            AppDomain.CurrentDomain.ProcessExit += (x, y) => CurrentDomain_ProcessExit(x, y, ExternalProcess);

            return ExternalProcess;
        }

        internal static Process StartProcessInBackgroundWithRedirects(
            string fileName, 
            string args, 
            EventHandler exitCallback=null,
            DataReceivedEventHandler errorDataHandler=null)
        {
            var pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process ExternalProcess = new Process();

            ExternalProcess.StartInfo.FileName = fileName;
            ExternalProcess.StartInfo.Arguments = args;
            ExternalProcess.StartInfo.UseShellExecute = false;
            //ExternalProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;

            ExternalProcess.StartInfo.WorkingDirectory = pluginDir;
            ExternalProcess.StartInfo.RedirectStandardInput = true;
            ExternalProcess.StartInfo.RedirectStandardOutput = true;
            ExternalProcess.StartInfo.RedirectStandardError = true;
            ExternalProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            
            //Async error data handler is used for tracking progress
            if (errorDataHandler != null)
            {
                ExternalProcess.ErrorDataReceived += errorDataHandler;
            }
            else
            {
                ExternalProcess.ErrorDataReceived += defaultErrorDataHandler;
            }
            
            ExternalProcess.StartInfo.CreateNoWindow = true;

            if (exitCallback != null)
            {
                ExternalProcess.Exited += exitCallback;
            }

            ExternalProcess.Start();
            
            //Add process to job object to make sure it is closed if the engine crashes without
            //calling the exit code.
            //TODO
            ChildProcessTracker.AddProcess(ExternalProcess);

            ExternalProcess.EnableRaisingEvents = true;
            ExternalProcess.BeginErrorReadLine();
            
            ExternalProcess.StandardInput.AutoFlush = true;

            AppDomain.CurrentDomain.ProcessExit += (x, y) => CurrentDomain_ProcessExit(x, y, ExternalProcess);

            return ExternalProcess;
        }

        
        private static void outputHandler(object sender, DataReceivedEventArgs e)
        {
            //throw new NotImplementedException();
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

            //Add process to job object to make sure it is closed if the engine crashes without
            //calling the exit code.
            ChildProcessTracker.AddProcess(ExternalProcess);

            ExternalProcess.EnableRaisingEvents = true;
            //ExternalProcess.BeginErrorReadLine();

            AppDomain.CurrentDomain.ProcessExit += (x, y) => CurrentDomain_ProcessExit(x, y, ExternalProcess);

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

        private static void defaultErrorDataHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == "sentence processed")
            {
                return;
            }
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
                line = Regex.Replace(line, @" PLACEHOLDER\d* ([.,!?:;])", "$1");
                line = Regex.Replace(line, @"PLACEHOLDER\d*", "");
            }

            if (!includeTagPairs)
            {
                line = Regex.Replace(line, @"TAGPAIRSTART\d*", "");
                line = Regex.Replace(line, @" TAGPAIREND\d* ([.,!?:;])", "$1");
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
            FileInfo segmentationModel,
            bool includePlaceholderTags,
            bool includeTagPairs,
            string targetLanguageToPrefix = null)
        {
            
            var preprocessedFile = new FileInfo(Path.Combine(directory.FullName, $"preprocessed_{languageFile.Name}"));

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

            //Marian doesn't like spaces in names
            var segmentedFile = new FileInfo(Path.Combine(directory.FullName, $"seg_{languageFile.Name.Replace(" ", "_")}"));

            IPreprocessor preprocessor;
            switch (segmentationModel.Extension)
            {
                case ".spm":
                    preprocessor = new SentencePiecePreprocessor(segmentationModel.FullName);
                    /*var spArgs = $"\"{preprocessedFile.FullName}\" --model \"{segmentationModel.FullName}\" --output \"{segmentedFile.FullName}\"";
                    var segmentationProcess = MarianHelper.StartProcessInBackgroundWithRedirects("Preprocessing\\spm_encode.exe", spArgs);
                    segmentationProcess.WaitForExit();*/
                    break;
                case ".bpe":
                    preprocessor = new MosesBpePreprocessor("", segmentationModel.FullName, languageCode, null);
                    /*
                    //Truecasing is not used in any models, so this is a dummy tc model (empty). So it does not
                    //matter is source.tcmodel is used for target language.
                    var tcModelPath = $@"{directory.FullName}\source.tcmodel";

                    //TODO: this would not work, since pipes no longer work (cmd.exe was removed from start process).
                    //Combine the py scripts and use those directly (add input file parameter).
                    var mosesProcess = MarianHelper.StartProcessInBackgroundWithRedirects(
                        $"type {preprocessedFile.FullName} | Preprocessing\\StartMosesBpePreprocessPipe.bat {languageCode} \"{tcModelPath}\" \"{segmentationModel.FullName}\" > {segmentedFile.FullName}");
                    mosesProcess.WaitForExit();*/
                    break;
                default:
                    throw new Exception("No segmentation model found");
                    break;
            }

            using (var preprocessedFileReader = preprocessedFile.OpenText())
            using (var segmentedFileWriter = new StreamWriter(segmentedFile.FullName))
            {
                String line;
                while ((line = preprocessedFileReader.ReadLine()) != null)
                {
                    var segmentedLine = preprocessor.PreprocessSentence(line);
                    segmentedFileWriter.WriteLine(segmentedLine);
                }
            }
         
            if (targetLanguageToPrefix != null)
            {
                var segmentedWithTargetPrefix = new FileInfo(Path.Combine(directory.FullName, $"prefix_{languageFile.Name.Replace(" ", "_")}"));

                using (var segFile = segmentedFile.OpenText())
                using (var prefixWriter = new StreamWriter(segmentedWithTargetPrefix.FullName))
                {
                    String line;
                    while ((line = segFile.ReadLine()) != null)
                    {
                        var prefixedLine = $">>{targetLanguageToPrefix}<< {line}";
                        prefixWriter.WriteLine(prefixedLine);
                    }
                }

                return segmentedWithTargetPrefix;
            }
            else
            {
                return segmentedFile;
            }

            
        }

        internal static FileInfo LinesToFile(IEnumerable<string> lines, string sourceCode)
        {
            var fileGuid = Guid.NewGuid();
            var srcFile = new FileInfo(Path.Combine(Path.GetTempPath(), $"{fileGuid}.{sourceCode}"));

            using (var srcStream = srcFile.CreateText())
            {
                foreach (var line in lines)
                {
                    srcStream.WriteLine(line);
                }
            }

            return srcFile;
        }

        internal static void GenerateAlignments(FileInfo spSource, FileInfo spTarget, FileInfo alignmentFile, FileInfo priorsFile)
        {
            var alignArgs = $"-s \"{spSource.FullName}\" -t \"{spTarget.FullName}\" -f \"{alignmentFile.FullName}.fwd\" -r \"{alignmentFile.FullName}.rev\"";
            Log.Information($"Aligning fine-tuning corpus with args {alignArgs}");
            var alignProcess = MarianHelper.StartProcessInBackgroundWithRedirects("python Alignment\\align.py", alignArgs);
            alignProcess.WaitForExit();

            var symmetryArgs = $"-c grow-diag-final -i \"{alignmentFile.FullName}.fwd\" -j \"{alignmentFile.FullName}.rev\" > \"{alignmentFile.FullName}\"";
            Log.Information($"Symmetrisizing alignment with args {symmetryArgs}");
            var symmetryProcess = MarianHelper.StartProcessInBackgroundWithRedirects("Alignment\\atools.exe", symmetryArgs);
            symmetryProcess.WaitForExit();
        }
        
        /* This uses marian-scorer to generate the alignment, but for some reason it's unstable and very slow
        internal static void GenerateAlignments(
            FileInfo spSource,
            FileInfo spTarget,
            FileInfo alignmentFile,
            FileInfo modelFile,
            FileInfo srcVocabFile,
            FileInfo trgVocabFile
            )
        {
            var scoresAndAlignmentsFile = new FileInfo($"{alignmentFile.FullName}.scores");
            var spArgs = $"score --train-sets {spSource.FullName} {spTarget.FullName} --model {modelFile.FullName} --vocabs {srcVocabFile.FullName} {trgVocabFile.FullName} --output {scoresAndAlignmentsFile.FullName} --alignment hard --quiet";
            var spmProcess = MarianHelper.StartProcessInBackgroundWithRedirects("Marian\\marian.exe", spArgs);

            spmProcess.WaitForExit();

            using (var reader = scoresAndAlignmentsFile.OpenText())
            using (var writer = alignmentFile.CreateText())
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    writer.WriteLine(line.Split(new string[] { "|||" },StringSplitOptions.None)[1]);
                }
            }
        }*/

    }
}
