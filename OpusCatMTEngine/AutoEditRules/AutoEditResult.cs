using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpusCatMTEngine
{
    public class AutoEditResult
    {
        public AutoEditResult(string result, List<AutoEditRuleMatch> appliedReplacements)
        {
            this.Result = result;
            this.AppliedReplacements = appliedReplacements;
        }

        public string Result { get; }
        public List<AutoEditRuleMatch> AppliedReplacements { get; }
    }
}
