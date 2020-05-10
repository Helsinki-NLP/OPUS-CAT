﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Text.RegularExpressions;
using Serilog;
using System.Data.SQLite;
using System.Data;
using System.Windows.Controls.Primitives;

namespace FiskmoMTEngine
{
    public class MarianBatchTranslator
    {
        private string langpair;

        public string SourceCode { get; }
        public string TargetCode { get; }
        
        private StreamWriter utf8Writer;
        private DirectoryInfo modelDir;

        public string SystemName { get; }
        
        public MarianBatchTranslator(string modelDir, string sourceCode, string targetCode)
        {
            this.langpair = $"{sourceCode}-{targetCode}";
            this.SourceCode = sourceCode;
            this.TargetCode = targetCode;
            this.modelDir = new DirectoryInfo(modelDir);
            this.SystemName = $"{sourceCode}-{targetCode}_" + this.modelDir.Name;
            
           
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
            ExternalProcess.StartInfo.RedirectStandardInput = false;
            ExternalProcess.StartInfo.RedirectStandardOutput = false;
            ExternalProcess.StartInfo.RedirectStandardError = true;
            ExternalProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            ExternalProcess.ErrorDataReceived += errorDataHandler;
            
            ExternalProcess.StartInfo.CreateNoWindow = true;
            //ExternalProcess.StartInfo.CreateNoWindow = false;
            
            ExternalProcess.Start();
            ExternalProcess.BeginErrorReadLine();

            return ExternalProcess;
        }

        internal void BatchTranslate(List<string> input)
        {
            Log.Information($"Starting batch translator for model {this.SystemName}.");
            
            var cmd = "TranslateBatchSentencePiece.bat";
                        
            FileInfo spInput = this.PreprocessInput(input);
            FileInfo spOutput = new FileInfo(
                spInput.FullName.Replace($".{SourceCode}", $".{TargetCode}"));
            //Check if batch.yml exists, if not create it from decode.yml

            var args = $"{this.modelDir.FullName} {spInput.FullName} {spOutput.FullName}";
            var batchProcess = this.StartProcessWithCmd(cmd, args);

            batchProcess.Exited += (x,y)=> BatchProcess_Exited(input, spOutput,x,y);
        }

        private void BatchProcess_Exited(List<string> input, FileInfo spOutput,object sender, EventArgs e)
        {
            Log.Information($"Batch translation process for model {this.SystemName} exited. Saving results.");
            Queue<string> inputQueue = new Queue<string>(input);
            using (var reader = spOutput.OpenText())
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var nonSpLine = (line.Replace(" ", "")).Replace("▁", " ").Trim();
                    var sourceLine = inputQueue.Dequeue();
                    TranslationDbHelper.WriteTranslationToDb(sourceLine, nonSpLine, this.SystemName);
                }
            }
        }

        private FileInfo PreprocessInput(List<string> input)
        {
            var fileGuid = Guid.NewGuid();
            var srcFile = new FileInfo(Path.Combine(Path.GetTempPath(), $"{fileGuid}.{this.SourceCode}"));

            using (var srcStream = new StreamWriter(srcFile.FullName, true, Encoding.UTF8))
            {
                foreach (var line in input)
                {
                    srcStream.WriteLine(line);
                }
            }

            var spmModel = this.modelDir.GetFiles("target.spm").Single();
            var spSrcFile = MarianHelper.PreprocessLanguage(srcFile, new DirectoryInfo(Path.GetTempPath()), this.SourceCode, spmModel);
            return spSrcFile;
        }

        private void errorDataHandler(object sender, DataReceivedEventArgs e)
        {
            Log.Information(e.Data);
        }
        
    }

}
    