using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiskmoMTEngine
{
    //A convenience class for handling bilingual file pairs, i.e. the same content in two languages, in two parallel files
    public class ParallelFilePair
    {
        
        public ParallelFilePair(FileInfo source, FileInfo target)
        {
            this.Source = source;
            this.Target = target;
        }

        public ParallelFilePair(string sourcePath, string targetPath)
        {
            this.Source = new FileInfo(sourcePath);
            this.Target = new FileInfo(targetPath);
        }

        public FileInfo Source { get; private set; }
        public FileInfo Target { get; private set; }
    }
}
