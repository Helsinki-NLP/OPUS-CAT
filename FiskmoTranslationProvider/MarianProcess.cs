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
        private ConcurrentDictionary<string, string> translationCache;
        private string mtPipeCmds;
        private static readonly Object lockObj = new Object();

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
            this.translationCache = new ConcurrentDictionary<string, string>();
            this.mtPipeCmds = "StartMtPipe.bat";
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
                    //Check again if the translation has been produced during the lock
                    //waiting period
                    if (this.translationCache.ContainsKey(sourceText))
                    {
                        return this.translationCache[sourceText];
                    }

                    //The mt pipe may have been closed, so restart it if needed
                    //DISABLED AS IT MAY CAUSE CYCLE OF RESTARTS
                    /*if (this.mtPipe.HasExited)
                    {
                        this.mtPipe = this.StartProcessWithCmd(this.mtPipeCmds, this.modelDir);
                    }*/

                    //Should test how this functions with segments containing linebreaks, since
                    //it only reads one line from output. Potentially stuff might remain in the
                    //buffer

                    this.MtPipe.StandardInput.WriteLine(sourceText);
                    this.MtPipe.StandardInput.Flush();
                    StringBuilder translationBuilder = new StringBuilder();
                    translationBuilder.Append(this.MtPipe.StandardOutput.ReadLine());

                    //The translation from the pipe is truecased, do recasing here, since
                    //doing it correctly requires access to the source string
                    var truecasedTranslation = translationBuilder.ToString();
                    string translation;
                    if (Char.IsUpper(sourceText[0]))
                    {
                        translation = truecasedTranslation.Substring(0,1).ToUpper() + truecasedTranslation.Substring(1);
                    }
                    else
                    {
                        translation = truecasedTranslation;
                    }
                    this.translationCache[sourceText] = translation;
                    
                    return translation;
                }
            }
            
        }
    }

}
    