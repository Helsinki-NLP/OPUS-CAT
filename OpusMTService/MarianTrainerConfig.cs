using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace OpusMTService
{
    public class MarianTrainerConfig
    {
        public string model { get; set; }
        public List<string> vocabs { get; set; }

        [YamlMember(Alias = "valid-sets", ApplyNamingConventions = false)]
        public List<string> ValidSets { get; set; }
        [YamlMember(Alias = "train-sets", ApplyNamingConventions = false)]
        public List<string> TrainSets { get; set; }
        [YamlMember(Alias = "valid-freq", ApplyNamingConventions = false)]
        public string validFreq { get; set; }

        [YamlMember(Alias = "valid-metrics", ApplyNamingConventions = false)]
        public List<string> validMetrics { get; set; }

        [YamlMember(Alias = "valid-log", ApplyNamingConventions = false)]
        public string validLog { get; set; }

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
        public string workspace { get; set; }

    }
}
