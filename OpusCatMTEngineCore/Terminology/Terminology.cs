using OpusCatMtEngine;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace OpusCatMtEngine
{

    public class Terminology
    {
        [YamlMember(Alias = "terms", ApplyNamingConventions = false)]
        public ObservableCollection<Term> Terms { get; set; }

        [YamlMember(Alias = "terminology-name", ApplyNamingConventions = false)]
        public string TerminologyName { get; set; }

        [YamlMember(Alias = "terminology-guid", ApplyNamingConventions = false)]
        public string TerminologyGuid;
        private FileInfo terminologyFile;

        [YamlMember(Alias = "global-terminology", ApplyNamingConventions = false)]
        public Boolean GlobalTerminology { get; set; }

        public Terminology()
        {
            this.Terms = new ObservableCollection<Term>();
            this.TerminologyGuid = Guid.NewGuid().ToString();
        }

        public void Save(DirectoryInfo terminologyDir = null)
        {
            //If dir arg is null, save to opus-cat data directory. Dir arg is used with
            //exporting rules.
            if (terminologyDir == null)
            {
                terminologyDir = new DirectoryInfo(
                    HelperFunctions.GetOpusCatDataPath(OpusCatMtEngineSettings.Default.TerminologyDir));
                if (!terminologyDir.Exists)
                {
                    terminologyDir.Create();
                }
            }

            var terminologyTempPath = Path.Combine(
                terminologyDir.FullName, $"{this.TerminologyGuid}_temp.yml");
            var terminologyPath = Path.Combine(
                terminologyDir.FullName, $"{this.TerminologyGuid}.yml");
            var serializer = new Serializer();

            //Don't replace current file yet
            using (var writer = File.CreateText(terminologyTempPath))
            {
                serializer.Serialize(writer, this, typeof(Terminology));
            }

            if (!File.Exists(terminologyPath))
            {
                File.Move(terminologyTempPath, terminologyPath);
            }
            else
            {
                //Safe replacement according to Jon Skeet
                string backup = terminologyPath + ".bak";
                File.Delete(backup);
                File.Replace(terminologyTempPath, terminologyPath, backup, true);
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

            //This is only set when saving the rulecollection or loading it from a file
            this.terminologyFile = new FileInfo(terminologyPath);
        }

        public static Terminology CreateFromFile(FileInfo terminologyFileInfo, bool assignNewId = false)
        {
            Terminology terminology;
            var deserializer = new Deserializer();
            using (var reader = terminologyFileInfo.OpenText())
            {
                terminology = deserializer.Deserialize<Terminology>(reader);
            }
            terminology.terminologyFile = terminologyFileInfo;
            if (assignNewId)
            {
                terminology.TerminologyGuid = Guid.NewGuid().ToString();
            }
            return terminology;
        }

    }
}