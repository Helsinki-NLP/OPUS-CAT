using Sdl.Core.Globalization;
using Sdl.LanguagePlatform.TranslationMemory;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinetuneTestsetExtractor
{
    public class TestsetExtractor
    {
        private string filteredTMPath;
        private int batches;
        private string masterTMNameWithoutExtension;
        private int batchSize;

        public TestsetExtractor(string filteredTMPath, int batches, string masterTMNameWithoutExtension, int batchSize)
        {
            this.filteredTMPath = filteredTMPath;
            this.batches = batches;
            this.masterTMNameWithoutExtension = masterTMNameWithoutExtension;
            this.batchSize = batchSize;
        }

        public Language TargetLang { get; set; }
        public Language SourceLang { get; set; }

        public FileInfo ExtractTestset()
        {
            FileBasedTranslationMemory tm = new FileBasedTranslationMemory(this.filteredTMPath);

            var tmLangdir = tm.GetLanguageDirection(tm.SupportedLanguageDirections.Single());
            this.SourceLang = new Language(tmLangdir.SourceLanguage);
            this.TargetLang = new Language(tmLangdir.TargetLanguage);
            //Move the iterator to the end of the TM and go backwards to get the newest segments (although
            //order may not be based on insertion time)
            RegularIterator iterator = new RegularIterator
            {
                MaxCount = this.batchSize,
                Forward = false,
                PositionFrom = tmLangdir.GetTranslationUnitCount()
            };

            List<TranslationUnit> finetuneTestset = new List<TranslationUnit>();

            TranslationUnit[] tuBatch = tmLangdir.GetTranslationUnits(ref iterator);
            for (var batchIndex = 0; batchIndex < this.batches; batchIndex++)
            {
                tuBatch = tmLangdir.GetTranslationUnits(ref iterator);
                finetuneTestset.AddRange(tuBatch);
                foreach (var tu in tuBatch)
                {
                    tmLangdir.DeleteTranslationUnit(tu.ResourceId);
                }
            }

            FileInfo sourceFile = new FileInfo($"{this.masterTMNameWithoutExtension}_finetunetest_source.txt");
            FileInfo targetFile = new FileInfo($"{this.masterTMNameWithoutExtension}_finetunetest_target.txt");

            using (var sourceWriter = sourceFile.CreateText())
            using (var targetWriter = targetFile.CreateText())
            {
                foreach (var tu in finetuneTestset)
                {
                    var plainSource = tu.SourceSegment.ToPlain();
                    var plainTarget = tu.TargetSegment.ToPlain();
                    if (!(plainSource.Contains("\n") || plainTarget.Contains("\n")))
                    {
                        sourceWriter.WriteLine(tu.SourceSegment.ToPlain());
                        targetWriter.WriteLine(tu.TargetSegment.ToPlain());
                    }
                }
            }

            tm.Save();
            return sourceFile;
        }
    }
}
