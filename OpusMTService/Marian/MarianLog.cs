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
        List<int> updateDurations = new List<int>();

        MarianTrainerConfig trainingConfig;

        Regex totalLineCountRegex = new Regex(@".*Done reading (?<totalLineCount>[\d,]+) sentences$");
        Regex updateRegex = new Regex(@".*\: Sen\. (?<linesSoFar>[\d,]+) \: Cost.*: Time (?<duration>[\d,]+).*");
        Regex translationDurationRegex = new Regex(@".*Total translation time: (?<translationDuration>\d+).*");

        private int linesSoFar;
        public int SentencesSoFar { get => linesSoFar; set => linesSoFar = value; }

        //When continuing training, the training starts from the middle, and it's not immediately
        //apparent what sentence the count starts from. So record the sentence count of the first
        //update and use that as zero (effectively this means the sentence count will always be one
        //update behind, but it's only used for generating the remaining time estimate).
        private int? firstBatchSentenceCount;
        
        public int EstimatedTranslationDuration { get; internal set; }
        public int EstimatedRemainingTotalTime { get; private set; }
        public MarianTrainerConfig TrainingConfig { get => trainingConfig; set => trainingConfig = value; }

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
                        var sentenceCount = Convert.ToInt32(updateLineMatch.Groups["linesSoFar"].Value.Replace(",", ""));
                        if (this.firstBatchSentenceCount == null)
                        {
                            this.firstBatchSentenceCount = sentenceCount;
                        }
                        else
                        {
                            //The value might contain comma as thousand separator
                            this.SentencesSoFar = sentenceCount;
                            this.updateDurations.Add(Convert.ToInt32(updateLineMatch.Groups["duration"].Value));
                        }
                    }
                    else if ((translationDurationMatch = this.translationDurationRegex.Match(data)).Success)
                    {
                        this.translationDurations.Add(Convert.ToInt32(translationDurationMatch.Groups["translationDuration"].Value));
                        this.EstimatedTranslationDuration = Convert.ToInt32(this.translationDurations.Average());
                    }
                }
            }
            catch (Exception ex)
            {
                
            }

            if (this.updateDurations.Count > 0)
            {
                this.EstimatedRemainingTotalTime = this.EstimateRemainingTime();
            }
            else
            {
                this.EstimatedRemainingTotalTime = 0;
            }
        }

        //Estimate remaining time based on update and validation durations
        private int EstimateRemainingTime(Boolean updateTimeIncludesValidationTime=true)
        {
            var validFreq = Convert.ToInt32(this.TrainingConfig.validFreq.TrimEnd('u'));
            var sentencesSoFarForThisRun = this.SentencesSoFar - this.firstBatchSentenceCount.Value;
            var sentencesLeft = (this.TotalLines - sentencesSoFarForThisRun);
            //This time will actually include validation translation time as well, since the update time for the first
            //update since translation will include translation time.
            var updatesPerUpdateLine = Int32.Parse(this.trainingConfig.dispFreq);
            var avgUpdateTime = this.updateDurations.Average() / updatesPerUpdateLine; 
            var avgSentencesPerUpdate = sentencesSoFarForThisRun / (this.updateDurations.Count * updatesPerUpdateLine);
            var estimatedBatchesLeft = sentencesLeft / avgSentencesPerUpdate;
            var estimatedUpdateTimeLeft = Convert.ToInt32(avgUpdateTime * estimatedBatchesLeft);

            //When the training starts, the completion time appears optimistic due to validation time
            //not being included. To counter this, add some extra time to the first estimates, and
            //remove it gradually once there's enough data to make a good estimate.

            if (this.updateDurations.Count * updatesPerUpdateLine < (validFreq * 5))
            {
                estimatedUpdateTimeLeft = estimatedUpdateTimeLeft * (1 + (1 / (this.updateDurations.Count*updatesPerUpdateLine)));
            }


            //Update time already includes validation time, no need to add it. That seems like a bug, though, so
            //it might be changed in future Marian versions, so keep this code here.
            //Currently only batch based valid freq supported
            if (!updateTimeIncludesValidationTime)
            {
                int estimatedValidationTimeLeft = 0;
                if (this.TrainingConfig.validFreq.EndsWith("u"))
                {
                    estimatedValidationTimeLeft = (estimatedBatchesLeft / validFreq) * this.EstimatedTranslationDuration;
                }
                return estimatedUpdateTimeLeft + estimatedValidationTimeLeft;
            }
            else
            {
                return estimatedUpdateTimeLeft;
            }
            
            //TODO: estimate to for multiple epochs and other stopping conditions (after-batches)
        }
    }
}
