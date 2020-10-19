using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FiskmoMTEngine.MarianCustomizer;

namespace FiskmoMTEngine
{
    //Used with progress handlers to communicate the state of long-running Marian tasks (customization, batch translation)
    public class MarianCustomizationStatus
    {
        public CustomizationStep CustomizationStep;
        public int? EstimatedSecondsRemaining;
        public MarianCustomizationStatus(CustomizationStep step, int? estimatedRemainingTotalTime)
        {
            this.CustomizationStep = step;
            this.EstimatedSecondsRemaining = estimatedRemainingTotalTime;
        }
    }
}
