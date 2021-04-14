using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpusCatMTEngine
{
    class TranslationDbHelper
    {
        internal static void WriteTranslationToDb(string sourceText, TranslationPair translation, string model)
        {
            var translationDb = HelperFunctions.GetOpusCatDataPath(OpusCatMTEngineSettings.Default.TranslationDBName);

            using (var m_dbConnection = new SQLiteConnection($"Data Source={translationDb};Version=3;"))
            {
                m_dbConnection.Open();

                using (SQLiteCommand insert =
                    new SQLiteCommand("INSERT or REPLACE INTO translations (sourcetext, translation, segmentedsource, segmentedtranslation, alignment, model) VALUES (@sourcetext,@translation,@segmentedsource,@segmentedtranslation,@alignment,@model)", m_dbConnection))
                {
                    insert.Parameters.Add(new SQLiteParameter("@sourcetext", sourceText));
                    insert.Parameters.Add(new SQLiteParameter("@translation", translation.Translation));
                    insert.Parameters.Add(new SQLiteParameter("@segmentedsource", String.Join(" ",translation.SegmentedSourceSentence)));
                    insert.Parameters.Add(new SQLiteParameter("@segmentedtranslation", String.Join(" ", translation.SegmentedTranslation)));
                    insert.Parameters.Add(new SQLiteParameter("@alignment", translation.AlignmentString));
                    insert.Parameters.Add(new SQLiteParameter("@model", model));
                    insert.ExecuteNonQuery();
                }
            }
        }

        internal static TranslationPair FetchTranslationFromDb(string sourceText, string model)
        {
            var translationDb = HelperFunctions.GetOpusCatDataPath(OpusCatMTEngineSettings.Default.TranslationDBName);

            List<TranslationPair> translationPairs = new List<TranslationPair>();

            using (var m_dbConnection = new SQLiteConnection($"Data Source={translationDb};Version=3;"))
            {
                m_dbConnection.Open();

                using (SQLiteCommand fetch =
                    new SQLiteCommand("SELECT DISTINCT segmentedsource,segmentedtranslation,alignment FROM translations WHERE sourcetext=@sourcetext AND model=@model LIMIT 1", m_dbConnection))
                {
                    fetch.Parameters.Add(new SQLiteParameter("@sourcetext", sourceText));
                    fetch.Parameters.Add(new SQLiteParameter("@model", model));
                    SQLiteDataReader r = fetch.ExecuteReader();

                    while (r.Read())
                    {
                        var segmentedSource = Convert.ToString(r["segmentedsource"]);
                        var segmentedTranslation = Convert.ToString(r["segmentedtranslation"]);
                        var alignment = Convert.ToString(r["alignment"]);

                        translationPairs.Add(new TranslationPair(segmentedSource, segmentedTranslation, alignment));
                    }
                }

            }

            return translationPairs.SingleOrDefault();
        }
    }
}
