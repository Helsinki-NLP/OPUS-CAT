using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace OpusMTService
{

    class MarianCustomizer
    {
        private DirectoryInfo customDir;
        private DirectoryInfo modelDir;
        private FileInfo customSource;
        private FileInfo customTarget;
        private string customLabel;
        private string sourceCode;
        private string targetCode;
        private FileInfo spSource;
        private FileInfo spTarget;
        private MTModel selectedModel;
        
        private void CopyModelDir(DirectoryInfo modelDir,string customLabel)
        {
            this.customDir = new DirectoryInfo($"{modelDir.FullName}_{customLabel}");
            if (this.customDir.Exists)
            {
                throw new Exception("custom model directory exists already");
            }
            customDir.Create();

            foreach (FileInfo file in modelDir.GetFiles())
            {
                file.CopyTo(Path.Combine(customDir.FullName, file.Name));
            }
        }

        public void Customize()
        {
            //First copy the model to new dir
            this.CopyModelDir(this.modelDir, this.customLabel);

            //Preprocess input files
            this.PreprocessInput();

            var decoderYaml = this.customDir.GetFiles("decoder.yml").Single();
            var deserializer = new Deserializer();
            var decoderSettings = deserializer.Deserialize<MarianDecoderConfig>(decoderYaml.OpenText());

            MarianTrainerConfig trainingConfig;
            using (var reader = new StreamReader(OpusMTServiceSettings.Default.CustomizationBaseConfig))
            {
                trainingConfig = deserializer.Deserialize<MarianTrainerConfig>(reader);
            }
                
            trainingConfig.TrainSets = new List<string>
                    {
                        spSource.FullName,
                        spTarget.FullName
                    };
            trainingConfig.vocabs = new List<string>
                    {
                        Path.Combine(this.customDir.FullName, decoderSettings.vocabs[0]),
                        Path.Combine(this.customDir.FullName, decoderSettings.vocabs[0])
                    };

            trainingConfig.model = Path.Combine(this.customDir.FullName, decoderSettings.models.Single());

            /*var trainingConfig = new MarianTrainerConfig
            {
                model = Path.Combine(this.customDir.FullName, decoderSettings.models.Single()),
                TrainSets = new List<string>
                    {
                        spSource.FullName,
                        spTarget.FullName
                    },
                vocabs = new List<string>
                    {
                        Path.Combine(this.customDir.FullName, decoderSettings.vocabs[0]),
                        Path.Combine(this.customDir.FullName, decoderSettings.vocabs[0])
                    },
                dispFreq = "10",
                saveFreq = "300u",
                miniBatchWords = "400",
                cpuThreads = "3",
                overwrite = "true",
                afterEpochs = "3",
                workspace = "4092"
            };*/

            var serializer = new Serializer();
            var configPath = Path.Combine(this.customDir.FullName, "train.yml");
            using (var writer = File.CreateText(configPath))
            {
                serializer.Serialize(writer, trainingConfig, typeof(MarianTrainerConfig));
            }
            var trainingArgs = $"--config {configPath}";
                /*$"--model {Path.Combine(this.customDir.FullName,decoderSettings.models.Single())} " +
                $"--train-sets {spSource.FullName} {spTarget.FullName} " +
                $"--vocabs {Path.Combine(this.customDir.FullName, decoderSettings.vocabs[0])} " +
                $"{Path.Combine(this.customDir.FullName, decoderSettings.vocabs[0])} " +
                $"--disp-freq 10 " +
                $"--save-freq=300u " +
                $"--mini-batch-words=400 " +
                $"--cpu-threads=3 " +
                $"--overwrite " +
                $"--after-epochs=3 " +
                $"-w 4092";*/

            this.StartProcessWithCmd("marian.exe",trainingArgs);
        }

        private void PreprocessInput()
        {
            var sourceSpm = this.customDir.GetFiles("source.spm").Single();
            var targetSpm = this.customDir.GetFiles("target.spm").Single();

            this.spSource = this.PreprocessLanguage(this.customSource,this.sourceCode, sourceSpm);
            this.spTarget = this.PreprocessLanguage(this.customTarget, this.targetCode, targetSpm);

        }

        private FileInfo PreprocessLanguage(FileInfo languageFile, string languageCode, FileInfo spmModel)
        {
            var preprocessedFile = new FileInfo(Path.Combine(this.customDir.FullName, $"preprocessed_{languageFile.Name}"));
            var spFile = new FileInfo(Path.Combine(this.customDir.FullName, $"sp_{languageFile.Name}"));


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
            this.StartProcessWithCmd("spm_encode.exe", spArgs);

            return spFile;
        }

        public MarianCustomizer(
            MTModel model,
            FileInfo customSource,
            FileInfo customTarget,
            string customLabel)
        {
            this.modelDir = new DirectoryInfo(model.InstallDir);
            this.customSource = customSource;
            this.customTarget = customTarget;
            this.customLabel = customLabel;
            this.sourceCode = model.SourceLanguageString;
            this.targetCode = model.TargetLanguageString;
        }


        private Process StartProcessWithCmd(string fileName, string args)
        {
            Process ExternalProcess = new Process();

            ExternalProcess.StartInfo.FileName = "cmd";
            ExternalProcess.StartInfo.Arguments = $"/c {fileName} {args}";
            ExternalProcess.StartInfo.UseShellExecute = false;
            //ExternalProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;

            ExternalProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            /*ExternalProcess.StartInfo.RedirectStandardInput = true;
            ExternalProcess.StartInfo.RedirectStandardOutput = true;
            ExternalProcess.StartInfo.RedirectStandardError = false;*/
            ExternalProcess.StartInfo.CreateNoWindow = false;
            ExternalProcess.Start();
            
            return ExternalProcess;
        }
    }
}
