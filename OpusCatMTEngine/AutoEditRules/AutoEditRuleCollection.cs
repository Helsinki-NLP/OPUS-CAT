using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace OpusCatMTEngine
{
    public class AutoEditRuleCollection
    {
        [YamlMember(Alias = "edit-rules", ApplyNamingConventions = false)]
        public ObservableCollection<AutoEditRule> EditRules { get; set; }

        [YamlMember(Alias = "collection-name", ApplyNamingConventions = false)]
        public string CollectionName { get; set; }

        [YamlMember(Alias = "collection-guid", ApplyNamingConventions = false)]
        public string CollectionGuid;

        [YamlMember(Alias = "collection-type", ApplyNamingConventions = false)]
        public string CollectionType;

        public AutoEditRuleCollection()
        {
        }

        public void Save()
        {
            var editRuleDir = new DirectoryInfo(
                HelperFunctions.GetOpusCatDataPath(OpusCatMTEngineSettings.Default.EditRuleDir));
            if (!editRuleDir.Exists)
            {
                editRuleDir.Create();
            }

            var ruleCollectionTempPath = Path.Combine(
                editRuleDir.FullName, $"{this.CollectionGuid}_temp.yml");
            var ruleCollectionPath = Path.Combine(
                editRuleDir.FullName, $"{this.CollectionGuid}.yml");
            var serializer = new Serializer();
            
            //Don't replace current file yet
            using (var writer = File.CreateText(ruleCollectionTempPath))
            {
                serializer.Serialize(writer, this, typeof(AutoEditRuleCollection));
            }

            string backup = ruleCollectionPath + ".bak";
            File.Delete(backup);
            File.Replace(ruleCollectionTempPath, ruleCollectionPath, backup, true);
            try
            {
                File.Delete(backup);
            }
            catch
            {
                // optional:
                // filesToDeleteLater.Add(backup);
            }

        }

        public void AddRule(AutoEditRule rule)
        {
            if (this.EditRules == null)
            {
                this.EditRules = new ObservableCollection<AutoEditRule>() { rule };
            }
            else
            {
                this.EditRules.Add(rule);
            }
        }

        private Dictionary<int, List<AutoEditRuleMatch>> GetAllSourceMatches(string uneditedInput)
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
                    if (uneditedSourceMatches.Count > 0)
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

        private Dictionary<int, List<AutoEditRuleMatch>> GetAllOutputMatches(string source, string uneditedOutput)
        {
            Dictionary<int, List<AutoEditRuleMatch>> regexMatches = 
                new Dictionary<int, List<AutoEditRuleMatch>>();
            foreach (var rule in this.EditRules)
            {
                //If source pattern exists, use it as condition for rule application
                MatchCollection sourceMatches = null;
                if (!String.IsNullOrEmpty(rule.SourcePattern))
                {
                    //Note that we check for the trigger in the unedited source (don't
                    //want to do serial rule application here)
                    sourceMatches = rule.SourcePatternRegex.Matches(source);
                }

                //If source pattern exists but return no matchs, don't get matches for the rule
                if (sourceMatches == null || sourceMatches.Count > 0)
                {
                    var outputMatches = rule.OutputPatternRegex.Matches(uneditedOutput);
                
                    if (outputMatches.Count > 0)
                    {
                        int matchIndex = 0;
                        foreach (Match match in outputMatches)
                        {
                            var newRuleMatch = new AutoEditRuleMatch(rule, match, matchIndex);

                            //if sourceMatches is not null, there is at least one of them
                            //(checked earlier)
                            if (sourceMatches != null)
                            {
                                if (sourceMatches.Count > matchIndex)
                                {
                                    newRuleMatch.SourceMatch = sourceMatches[matchIndex];
                                }
                                else
                                {
                                    newRuleMatch.RepeatedSourceMatch = true;
                                    newRuleMatch.SourceMatch = sourceMatches[0];
                                }
                            }

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

        internal void ReplaceRule(AutoEditRule rule, AutoEditRule replacement)
        {
            var existingIndex = this.EditRules.IndexOf(rule);
            this.EditRules[existingIndex] = replacement;
        }

        public AutoEditResult ProcessPreEditRules(string unedited)
        {
            string edited = unedited;

            List<AutoEditRuleMatch> appliedReplacements = new List<AutoEditRuleMatch>();
            List<Tuple<int, int>> coveredUneditedSourceSpans = new List<Tuple<int, int>>();
            //Collect matches for all rules
            Dictionary<int, List<AutoEditRuleMatch>> uneditedSourceMatches = this.GetAllSourceMatches(unedited);

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
                edited = edited.Remove(matchesAtPosition.Key + editingOffset, matchLength);
                //Replace with rule replacement
                var replacement = longestMatch.Match.Result(longestMatch.Rule.Replacement);
                
                edited = edited.Insert(matchesAtPosition.Key + editingOffset, replacement);

                longestMatch.OutputIndex = matchesAtPosition.Key + editingOffset;
                longestMatch.OutputLength = replacement.Length;
                appliedReplacements.Add(longestMatch);

                //Update loop counters
                editingOffset += replacement.Length - matchLength;
                endOfLastMatchIndex = longestMatch.Match.Index + matchLength;

            }

            return new AutoEditResult(edited, appliedReplacements);
        }
        
        public AutoEditResult ProcessPostEditRules(string source, string unedited)
        {
            string edited = unedited;

            List<AutoEditRuleMatch> appliedReplacements = new List<AutoEditRuleMatch>();
            List<Tuple<int, int>> coveredUneditedOutputSpans = new List<Tuple<int, int>>();
            //Collect matches for all rules
            Dictionary<int, List<AutoEditRuleMatch>> uneditedSourceMatches = this.GetAllOutputMatches(source,unedited);

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
                edited = edited.Remove(matchesAtPosition.Key + editingOffset, matchLength);
                //Replace with rule replacement
                var replacement = longestMatch.Match.Result(longestMatch.Rule.Replacement);

                var sourceGroupMatches = Regex.Matches(replacement, @"\$<(\d+)>");
                if (sourceGroupMatches.Count > 0)
                {
                    foreach (Match match in sourceGroupMatches)
                    {
                        //source group index is guaranteed to be int, since it matches \d+
                        int sourceGroupIndex = int.Parse(match.Groups[1].Value);

                        if (longestMatch.SourceMatch.Groups.Count > sourceGroupIndex)
                        {
                            var sourceGroup = longestMatch.SourceMatch.Groups[sourceGroupIndex];
                            replacement = replacement.Replace($"$<{sourceGroupIndex}>", sourceGroup.Value);
                            longestMatch.Output = replacement;
                        }
                    }
                }

                edited = edited.Insert(matchesAtPosition.Key + editingOffset, replacement);

                longestMatch.OutputIndex = matchesAtPosition.Key + editingOffset;
                longestMatch.OutputLength = replacement.Length;
                appliedReplacements.Add(longestMatch);

                //Update loop counters
                editingOffset += replacement.Length - matchLength;
                endOfLastMatchIndex = longestMatch.Match.Index + matchLength;

            }

            return new AutoEditResult(edited, appliedReplacements);
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