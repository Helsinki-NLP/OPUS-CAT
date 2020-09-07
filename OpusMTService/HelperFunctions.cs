using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiskmoMTEngine
{
    class HelperFunctions
    {
        public static string GetLocalAppDataPath(string restOfPath)
        {
            return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    FiskmoMTEngineSettings.Default.LocalFiskmoDir,
                    restOfPath);
        }
    }
}
