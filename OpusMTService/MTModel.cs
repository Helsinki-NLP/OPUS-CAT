using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpusMTService
{
    public class MTModel
    {
        private List<string> sourceLanguages;
        private List<string> targetLanguages;
        private string name;

        public List<string> TargetLanguages { get => targetLanguages; set => targetLanguages = value; }
        public List<string> SourceLanguages { get => sourceLanguages; set => sourceLanguages = value; }
        public string Name { get => name; set => name = value; }

        public MTModel(List<string> sourceLanguages, List<string> targetLanguages, string name)
        {
            this.SourceLanguages = sourceLanguages;
            this.TargetLanguages = targetLanguages;
            this.Name = name;
        }

        public MTModel(string modelPath)
        {
            char separator;
            if (modelPath.Contains('/'))
            {
                separator = '/';
            }
            else
            {
                separator = '\\';
            }
            var pathSplit = modelPath.Split(separator);
            //Multiple source languages separated by plus symbols
            this.SourceLanguages = pathSplit[0].Split('-')[0].Split('+').ToList();
            this.TargetLanguages = pathSplit[0].Split('-')[1].Split('+').ToList();
            this.Name = pathSplit[1];
        }

        public string SourceLanguageString
        {
            get { return String.Join("+", this.SourceLanguages); }
        }

        public string TargetLanguageString
        {
            get { return String.Join("+", this.TargetLanguages); }
        }

    }
}
