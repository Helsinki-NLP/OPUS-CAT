using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OpusCatMTEngine
{
    [DataContract]
    public class ParallelSentence
    {
        public ParallelSentence(string source, string target)
        {
            this.Source = source;
            this.Target = target;
        }

        [DataMember]
        public string Source { get; private set; }
        [DataMember]
        public string Target { get; private set; }
    }
}
