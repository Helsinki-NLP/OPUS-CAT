using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace OpusCatMTEngine
{
    public class AutoEditRuleCollection
    {
        [YamlMember(Alias = "edit-rules", ApplyNamingConventions = false)]
        public List<AutoEditRule> EditRules;

        [YamlMember(Alias = "collection-name", ApplyNamingConventions = false)]
        public string CollectionName { get; set; }

        [YamlMember(Alias = "collection-guid", ApplyNamingConventions = false)]
        public string CollectionGuid;

        public AutoEditRuleCollection()
        {
        }

        public void AddRule(AutoEditRule rule)
        {
            if (this.EditRules == null)
            {
                this.EditRules = new List<AutoEditRule>() { rule };
            }
            else
            {
                this.EditRules.Add(rule);
            }
        }

        private Dictionary<int, List<AutoEditRuleMatch>> GetAllMatches(string uneditedInput)
        {
            Dictionary<int, List<AutoEditRuleMatch>> regexMatches = new Dictionary<int, List<AutoEditRuleMatch>>();
            foreach (var rule in this.EditRules)
            {
                
                //If source pattern exists, use it as condition for rule application
                if (!String.IsNullOrEmpty(rule.SourcePattern))
                {
                    //Note that we check for the trigger in the unedited source (don't
                    //want to do serial rule application here)
                    var uneditedSourceMatches = rule.SourcePatternRegex.Matches(uneditedInput);
                    if (uneditedSourceMatches.Count > 1)
                    {
                        int matchIndex = 0;
                        foreach (Match match in uneditedSourceMatches)
                        {
                            var newRuleMatch = new AutoEditRuleMatch(rule, match, matchIndex);
                            if (regexMatches.ContainsKey(match.Index))
                            {
                                regexMatches[match.Index].Add(newRuleMatch);
                            }
                            else
                            {
                                regexMatches[match.Index] = new List<AutoEditRuleMatch>() { newRuleMatch };
                            }
                            matchIndex += 1;
                        }
                    }
                }
            }

            return regexMatches;
        }

        public AutoEditResult ProcessRules(string uneditedSource)
        {
            string editedSource = uneditedSource;

            List<AutoEditRuleMatch> appliedReplacements = new List<AutoEditRuleMatch>();
            List<Tuple<int,int>> coveredUneditedSourceSpans = new List<Tuple<int, int>>();
            //Collect matches for all rules
            Dictionary<int, List<AutoEditRuleMatch>> uneditedSourceMatches = this.GetAllMatches(uneditedSource);

            int endOfLastMatchIndex = -1;
            //How much the length of the edited source has changed in comparison with unedited source
            int editingOffset = 0;
            foreach (var matchesAtPosition in uneditedSourceMatches.OrderBy(x => x.Key))
            {
                //Select the longest match (selection could be based on other factors, but this is 
                //the simplest)
                var longestMatch = matchesAtPosition.Value.OrderBy(x => x.Match.Length).Last();
                //Remove the original text
                var matchLength = longestMatch.Match.Length;
                editedSource = editedSource.Remove(matchesAtPosition.Key + editingOffset, matchLength);
                //Replace with rule replacement
                var replacement = longestMatch.Match.Result(longestMatch.Rule.Replacement);
                editedSource = editedSource.Insert(matchesAtPosition.Key + editingOffset, replacement);

                longestMatch.OutputIndex = matchesAtPosition.Key + editingOffset;
                longestMatch.OutputLength = replacement.Length;
                appliedReplacements.Add(longestMatch);
                
                //Update loop counters
                editingOffset += replacement.Length - matchLength;
                endOfLastMatchIndex = longestMatch.Match.Index + matchLength;
                
            }

            return new AutoEditResult(editedSource, appliedReplacements);
        }

        public static AutoEditRuleCollection CreateFromFile(FileInfo ruleFileInfo)
        {
            AutoEditRuleCollection editRuleCollection;
            var deserializer = new Deserializer();
            using (var reader = ruleFileInfo.OpenText())
            {
                editRuleCollection = deserializer.Deserialize<AutoEditRuleCollection>(reader);
            }

            return editRuleCollection;
        }
    }
}