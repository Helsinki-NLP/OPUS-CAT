using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace OpusCatMTEngine
{
    public class MarianTrainerConfig
    {
        public string model { get; set; }
        public string sqlite { get; set; }

        public List<string> vocabs { get; set; }

        [YamlMember(Alias = "valid-sets", ApplyNamingConventions = false)]
        public List<string> ValidSets { get; set; }
        [YamlMember(Alias = "train-sets", ApplyNamingConventions = false)]
        public List<string> trainSets { get; set; }
        [YamlMember(Alias = "valid-freq", ApplyNamingConventions = false)]
        public string validFreq { get; set; }

        [YamlMember(Alias = "valid-script-args", ApplyNamingConventions = false)]
        public List<string> validScriptArgs { get; set; }

        [YamlMember(Alias = "beam-size", ApplyNamingConventions = false)]
        public string beamSize { get; set; }

        [YamlMember(Alias = "learn-rate", ApplyNamingConventions = false)]
        public string learnRate { get; set; }

        [YamlMember(Alias = "lr-report", ApplyNamingConventions = false)]
        public string lrReport { get; set; }

        [YamlMember(Alias = "valid-script-path", ApplyNamingConventions = false)]
        public string validScriptPath { get; set; }

        [YamlMember(Alias = "valid-translation-output", ApplyNamingConventions = false)]
        public string validTranslationOutput { get; set; }

        [YamlMember(Alias = "valid-max-length", ApplyNamingConventions = false)]
        public string validMaxLength { get; set; }

        [YamlMember(Alias = "guided-alignment", ApplyNamingConventions = false)]
        public string guidedAlignment { get; set; }

        [YamlMember(Alias = "valid-metrics", ApplyNamingConventions = false)]
        public List<string> validMetrics { get; set; }

        [YamlMember(Alias = "valid-log", ApplyNamingConventions = false)]
        public string validLog { get; set; }
        public string log { get; set; }

        [YamlMember(Alias = "disp-freq", ApplyNamingConventions = false)]
        public string dispFreq { get; set; }
        [YamlMember(Alias = "save-freq", ApplyNamingConventions = false)]
        public string saveFreq { get; set; }
        [YamlMember(Alias = "mini-batch-words", ApplyNamingConventions = false)]
        public string miniBatchWords { get; set; }
        [YamlMember(Alias = "cpu-threads", ApplyNamingConventions = false)]
        public string cpuThreads { get; set; }
        public string overwrite { get; set; }
        [YamlMember(Alias = "after-epochs", ApplyNamingConventions = false)]
        public string afterEpochs { get; set; }

        [YamlMember(Alias = "after-batches", ApplyNamingConventions = false)]
        public string afterBatches { get; set; }

        [YamlMember(Alias = "early-stopping", ApplyNamingConventions = false)]
        public string earlyStopping { get; set; }

        public string workspace { get; set; }

        public string normalize { get; set; }

        [YamlMember(Alias = "gradient-checkpointing", ApplyNamingConventions = false)]
        public string gradientCheckpointing { get; set; }
        [YamlMember(Alias = "shuffle-in-ram", ApplyNamingConventions = false)]
        public string shuffleInRam { get; set; }

        [YamlMember(Alias = "keep-best", ApplyNamingConventions = false)]
        public string keepBest { get; set; }

    }
}
