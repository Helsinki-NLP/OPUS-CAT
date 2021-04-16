using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpusCatMTEngine
{
    class TranslationDbHelper
    {
        //The short-term storage is cleared when restarted, sqlite db is used for long-term storage (may be disabled in settings)
        private static ConcurrentDictionary<Tuple<string, string>, TranslationPair> shortTermMtStorage = 
            new ConcurrentDictionary<Tuple<string, string>, TranslationPair>();

        internal static void SetupTranslationDb()
        {

            var translationTableColumns =
                new List<string>() {
                    "model",
                    "sourcetext",
                    "translation",
                    "segmentedsource",
                    "segmentedtranslation",
                    "alignment",
                    "additiondate"
                };

            var translationDb = new FileInfo(HelperFunctions.GetOpusCatDataPath(OpusCatMTEngineSettings.Default.TranslationDBName));

            //Check that db structure is current
            bool tableValid = true;
            if (translationDb.Exists)
            {
                using (var m_dbConnection =
                    new SQLiteConnection($"Data Source={translationDb.FullName};Version=3;"))
                {
                    m_dbConnection.Open();

                    using (SQLiteCommand verify_table =
                        new SQLiteCommand("PRAGMA table_info(translations);", m_dbConnection))
                    {
                        SQLiteDataReader r = verify_table.ExecuteReader();

                        List<string> tableColumnStrikeoutList = new List<string>(translationTableColumns);

                        while (r.Read())
                        {
                            var columnName = Convert.ToString(r["name"]);
                            if (tableColumnStrikeoutList[0] == columnName)
                            {
                                tableColumnStrikeoutList.RemoveAt(0);
                            }
                            else
                            {
                                tableValid = false;
                            }
                        }

                        tableValid = tableValid && !tableColumnStrikeoutList.Any();
                        verify_table.Dispose();
                 
                    }
                    m_dbConnection.Close();
                    m_dbConnection.Dispose();
                }
            }

            if (!tableValid)
            {
                MessageBoxResult result = MessageBox.Show(OpusCatMTEngine.Properties.Resources.App_InvalidDbMessage,
                                          OpusCatMTEngine.Properties.Resources.App_ConfirmDbCaption,
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    translationDb.Delete();
                }
                else
                {
                    OpusCatMTEngineSettings.Default.CacheMtInDatabase = false;
                    OpusCatMTEngineSettings.Default.Save();
                    OpusCatMTEngineSettings.Default.Reload();
                }
            }

            translationDb.Refresh();
            if (!translationDb.Exists)
            {
                SQLiteConnection.CreateFile(translationDb.FullName);
                using (var m_dbConnection = new SQLiteConnection($"Data Source={translationDb};Version=3;"))
                {
                    m_dbConnection.Open();

                    string sql = "create table translations (model TEXT, sourcetext TEXT, translation TEXT, segmentedsource TEXT, segmentedtranslation TEXT, alignment TEXT, additiondate DATETIME, PRIMARY KEY (model,sourcetext))";

                    using (SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        internal static void WriteTranslationToDb(string sourceText, TranslationPair translation, string model)
        {
            TranslationDbHelper.shortTermMtStorage.GetOrAdd(new Tuple<string, string>(sourceText, model), translation);
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
                    new SQLiteCommand("SELECT DISTINCT translation,segmentedsource,segmentedtranslation,alignment FROM translations WHERE sourcetext=@sourcetext AND model=@model LIMIT 1", m_dbConnection))
                {
                    fetch.Parameters.Add(new SQLiteParameter("@sourcetext", sourceText));
                    fetch.Parameters.Add(new SQLiteParameter("@model", model));
                    SQLiteDataReader r = fetch.ExecuteReader();

                    while (r.Read())
                    {
                        var segmentedSource = Convert.ToString(r["segmentedsource"]);
                        var segmentedTranslation = Convert.ToString(r["segmentedtranslation"]);
                        var translation = Convert.ToString(r["translation"]);
                        var alignment = Convert.ToString(r["alignment"]);

                        translationPairs.Add(new TranslationPair(translation, segmentedSource, segmentedTranslation, alignment));
                    }
                }

            }

            return translationPairs.SingleOrDefault();
        }
    }
}
