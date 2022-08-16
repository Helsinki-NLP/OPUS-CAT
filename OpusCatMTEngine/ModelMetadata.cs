using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace OpusCatMTEngine
{
    public class ModelMetadata
    {
        //This does not appear to be used anywhere, currently these model yaml files
        //are parsed dynamically

        [YamlMember(Alias = "release", ApplyNamingConventions = false)]
        public string Release { get; set; }

        [YamlMember(Alias = "release-date", ApplyNamingConventions = false)]
        public DateTimeOffset ReleaseDate { get; set; }

        [YamlMember(Alias = "dataset-name", ApplyNamingConventions = false)]
        public string DatasetName { get; set; }

        [YamlMember(Alias = "modeltype", ApplyNamingConventions = false)]
        public string Modeltype { get; set; }

        [YamlMember(Alias = "pre-processing", ApplyNamingConventions = false)]
        public string PreProcessing { get; set; }

        [YamlMember(Alias = "subwords", ApplyNamingConventions = false)]
        public Subword[] Subwords { get; set; }

        [YamlMember(Alias = "subword-models", ApplyNamingConventions = false)]
        public Subword[] SubwordModels { get; set; }

        [YamlMember(Alias = "source-languages", ApplyNamingConventions = false)]
        public string[] SourceLanguages { get; set; }

        [YamlMember(Alias = "target-languages", ApplyNamingConventions = false)]
        public string[] TargetLanguages { get; set; }

        [YamlMember(Alias = "test-data", ApplyNamingConventions = false)]
        public string[] TestData { get; set; }

        [YamlMember(Alias = "use-target-labels", ApplyNamingConventions = false)]
        public string[] UseTargetLabels { get; set; }

        [YamlMember(Alias = "BLEU-scores", ApplyNamingConventions = false)]
        public string[] BleuScores { get; set; }

        [YamlMember(Alias = "chr-F-scores", ApplyNamingConventions = false)]
        public string[] ChrFScores { get; set; }
    }

    public class Subword
    {
        [YamlMember(Alias = "source", ApplyNamingConventions = false)]
        public string Source { get; set; }

        [YamlMember(Alias = "target", ApplyNamingConventions = false)]
        public string Target { get; set; }
    }
}
