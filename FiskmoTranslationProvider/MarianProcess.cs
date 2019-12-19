using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace FiskmoTranslationProvider
{
    public class MarianProcess
    {
        /*private Process tokenizeAndTruecaseProcess;
        private Process bpeProcess;
        private Process decoderProcess;*/
        private Process mtPipe;
        private string langpair;

        public string SourceCode { get; }
        public string TargetCode { get; }
        public bool Faulted { get; private set; }
        public Process MtPipe { get => mtPipe; set => mtPipe = value; }

        private string modelDir;

        public string SystemName { get; }

        private ConcurrentDictionary<string, string> translationCache;
        private ConcurrentDictionary<string, TimeSpan> translationDurations;
        private string mtPipeCmds;
        private bool sentencePiecePostProcess;
        private static readonly Object lockObj = new Object();

        //This seems like the only way to cleanly close the NMT window when Studio is closing.
        //On some systems the unclean closing was causing an error pop-up, that's why this was added.
        //On my dev system the was no pop-up, but the error was reported in Event Viewer.
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

        public MarianProcess(string modelDir, string sourceCode, string targetCode)
        {
            this.Faulted = false;
            this.langpair = $"{sourceCode}-{targetCode}";
            this.SourceCode = sourceCode;
            this.TargetCode = targetCode;
            this.modelDir = modelDir;
            this.SystemName = $"{sourceCode}-{targetCode}_" + (new DirectoryInfo(this.modelDir)).Name;
            this.translationCache = new ConcurrentDictionary<string, string>();
            this.translationDurations = new ConcurrentDictionary<string, TimeSpan>();

            //Both moses+BPE and sentencepiece preprocessing are supported, check which one model is using
            if (Directory.GetFiles(this.modelDir).Any(x=> new FileInfo(x).Name == "source.spm"))
            {
                this.mtPipeCmds = "StartSentencePieceMtPipe.bat";
                this.sentencePiecePostProcess = true;
            }
            else
            {
                this.mtPipeCmds = "StartMosesBpeMtPipe.bat";
                this.sentencePiecePostProcess = false;
            }

            this.MtPipe = this.StartProcessWithCmd(this.mtPipeCmds, this.modelDir);
            
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            KillProcessAndChildren(this.mtPipe.Id);
        }

        private Process StartProcessWithCmd(string fileName, string args)
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
            ExternalProcess.StartInfo.RedirectStandardError = false;
            ExternalProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            ExternalProcess.StartInfo.CreateNoWindow = true;
            //ExternalProcess.StartInfo.CreateNoWindow = false;
            
            ExternalProcess.Start();
            ExternalProcess.StandardInput.AutoFlush = true;

            return ExternalProcess;
        }

        private Process StartProcessWithRedirects(string fileName, string args)
        {
            var pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process ExternalProcess = new Process();
 
            ExternalProcess.StartInfo.FileName = Path.Combine(pluginDir, fileName);
            ExternalProcess.StartInfo.Arguments = args;
            ExternalProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ExternalProcess.StartInfo.UseShellExecute = false;

            ExternalProcess.StartInfo.WorkingDirectory = pluginDir;
            ExternalProcess.StartInfo.RedirectStandardInput = true;
            ExternalProcess.StartInfo.RedirectStandardOutput = true;
            ExternalProcess.StartInfo.RedirectStandardError = true;


            ExternalProcess.Start();
            
            return ExternalProcess;
        }

        public void ShutdownMtPipe()
        {
            this.MtPipe.Close();
        }

        private string TranslateSentence(string sourceSentence)
        {
            this.MtPipe.StandardInput.WriteLine(sourceSentence);
            this.MtPipe.StandardInput.Flush();

            //There should only ever be a single line in the stdout, since there's only one line of
            //input per stdout readline, and marian decoder will never insert line breaks into translations.
            string translation = this.MtPipe.StandardOutput.ReadLine();

            if (this.sentencePiecePostProcess)
            {
                translation = (translation.Replace(" ", "")).Replace("▁", " ");
            }

            return translation;
        }

        public string Translate(string sourceText)
        {
            
            if (this.MtPipe.HasExited)
            {
                if (this.modelDir == null)
                {
                    throw new Exception($"No local Fiskmö model exists for language pair {this.langpair}. Open the Settings dialog of Fiskmö translation provider to download the latest model.");
                }

                if (this.translationCache.IsEmpty)
                {
                    var pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    throw new Exception(
                    "Fiskmo plugin MT functionality startup failed. " +
                    "This is probably caused by the security settings (e.g. domain policy or antivirus) used in the computer. " +
                    "MT functionality requires that the script StartMtPipe.bat and the executables within it can be executed. " +
                    $"The script can be found in the Fiskmö plugin directory ({pluginDir}). " +
                    "If you have no control over the security settings, please contact your system administrator.");
                }
                else
                {
                    throw new Exception("Fiskmo plugin MT functionality has stopped working. Restarting Trados Studio may resolve the problem.");
                }
            }

            if (this.translationCache.ContainsKey(sourceText))
            {
                return this.translationCache[sourceText];
            }
            else
            {
                lock (MarianProcess.lockObj)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    //Check again if the translation has been produced during the lock
                    //waiting period
                    if (this.translationCache.ContainsKey(sourceText))
                    {
                        return this.translationCache[sourceText];
                    }

                    //It might be the case that the source text contains multiple sentences,
                    //potentially even line breaks, so the text needs to be split on line breaks.
                    //(sentence splitting might be nice, but having multiple sentences on one line
                    //doesn't break anything, while multiple lines cause desyncing problems.
                    var splitSource = sourceText.Split(new[] {"\r\n","\r","\n"},StringSplitOptions.None);

                    StringBuilder translationBuilder = new StringBuilder();
                    foreach (var sourceSentence in splitSource)
                    {
                        translationBuilder.Append(this.TranslateSentence(sourceSentence));
                    }

                    var translation = translationBuilder.ToString();
                
                    this.translationCache[sourceText] = translation;
                    sw.Stop();
                    this.translationDurations[sourceText] = sw.Elapsed;

                    return translation;
                }
            }
            
        }
    }

}
    