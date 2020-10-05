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
        List<int> translationDurations = new List<int>();

        Regex totalLineCountRegex = new Regex(@".*Done reading (?<totalLineCount>[\d,]+) sentences$");
        Regex updateRegex = new Regex(@".*\: Sen\. (?<linesSoFar>[\d,]+) \: Cost");
        Regex translationDurationRegex = new Regex(@".*Total translation time: (?<translationDuration>\d+).*");

        private int linesSoFar;
        public int LinesSoFar { get => linesSoFar; set => linesSoFar = value; }
        
        private int avgTranslationDuration;
        public int AvgTranslationDuration { get => avgTranslationDuration; set => avgTranslationDuration = value; }

        internal void ParseTrainLogLine(string data)
        {

            //This parsing with regexes is prone to failing, so don't let it crash everything
            try
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
                    Match updateLineMatch;
                    Match translationDurationMatch;
                    if ((updateLineMatch = this.updateRegex.Match(data)).Success)
                    {
                        this.LinesSoFar = Int32.Parse(updateLineMatch.Groups["linesSoFar"].Value);
                    }
                    else if ((translationDurationMatch = this.translationDurationRegex.Match(data)).Success)
                    {
                        this.translationDurations.Add(Int32.Parse(translationDurationMatch.Groups["translationDuration"].Value));
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
        }


    }
}
