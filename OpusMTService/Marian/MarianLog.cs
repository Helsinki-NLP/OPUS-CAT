using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FiskmoMTEngine
{
    //This is an object for the information contained in a Marian training log.
    //The log is parsed 
    public class MarianLog
    {
        public MarianLog()
        {

        }

        private int totalLines;
        public int TotalLines { get => totalLines; set => totalLines = value; }

        Regex totalLineCountRegex = new Regex(@".*Done reading (?<totalLineCount>\d+) sentences$");
        Regex updateRegex = new Regex(@".*\: Sen\. (?<linesSoFar>\d+) \: Cost");

        private int linesSoFar;
        public int LinesSoFar { get => linesSoFar; set => linesSoFar = value; }

        
        internal void ParseTrainLogLine(string data)
        {
            if (this.totalLines == 0)
            {
                var totalLinesMatch = this.totalLineCountRegex.Match(data);
                if (totalLinesMatch.Success)
                {
                    this.TotalLines = Int32.Parse(totalLinesMatch.Groups["totalLineCount"].Value);

                }
            }
            else
            {
                var updateLineMatch = this.updateRegex.Match(data);
                if (updateLineMatch.Success)
                {
                    this.LinesSoFar = Int32.Parse(updateLineMatch.Groups["linesSoFar"].Value);
                }
            }
        }


    }
}
