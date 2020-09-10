using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OpusMTInterface
{
    [DataContract]
    public class Translation
    {
        [DataMember]
        string translation;

        public Translation(string translation)
        {
            this.translation = translation;
        }
    }
}
