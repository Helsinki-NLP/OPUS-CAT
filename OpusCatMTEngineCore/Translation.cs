using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OpusCatMtEngine
{
    [DataContract]
    public class Translation
    {
        [DataMember]
        public string translation;

        [DataMember]
        public string alignment;

        public Translation(string translation)
        {
            this.translation = translation;
        }
    }
}
