using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace FiskmoMTEngine
{
    public class MarianDecoderConfig
    {
        public List<string> models { get; set; }
        public List<string> vocabs { get; set; }
        [YamlMember(Alias = "relative-paths", ApplyNamingConventions = false)]
        public string relativePaths { get; set; }
        [YamlMember(Alias = "beam-size", ApplyNamingConventions = false)]
        public string beamSize { get; set; }
        public string normalize { get; set; }
        [YamlMember(Alias = "word-penalty", ApplyNamingConventions = false)]
        public string wordPenalty { get; set; }
        [YamlMember(Alias = "mini-batch", ApplyNamingConventions = false)]
        public string miniBatch { get; set; }
        [YamlMember(Alias = "maxi-batch", ApplyNamingConventions = false)]
        public string maxiBatch { get; set; }
        [YamlMember(Alias = "maxi-batch-sort", ApplyNamingConventions = false)]
        public string maxiBatchSort { get; set; }
    }
}
