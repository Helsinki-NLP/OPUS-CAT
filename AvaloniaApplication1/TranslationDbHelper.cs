using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using OpusCatMtEngine;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Data.Sqlite;

namespace OpusCatMtEngine
{
    class TranslationDbHelper
    {
        //The short-term storage is cleared when restarted, sqlite db is used for long-term storage (may be disabled in settings)
        private static ConcurrentDictionary<Tuple<string, string>, TranslationPair> shortTermMtStorage = 
            new ConcurrentDictionary<Tuple<string, string>, TranslationPair>();

        internal static async void SetupTranslationDb()
        {

            var translationTableColumns =
                new List<string>() {
                    "model",
                    "sourcetext",
                    "translation",
                    "segmentedsource",
                    "segmentedtranslation",
                    "alignment",
                    "additiondate",
                    "segmentationmethod",
                    "targetlanguage",
                    "maxlength"
                };

            var translationDb = new FileInfo(HelperFunctions.GetOpusCatDataPath(OpusCatMtEngineSettings.Default.TranslationDBName));
            
            //If the translation db has a size of 0, it should be deleted (size 0 dbs are caused
            //by db creation bugs). 
            if (translationDb.Exists && translationDb.Length == 0)
            {
                translationDb.Delete();
            }

            //Check that db structure is current
            bool tableValid = true;
            if (translationDb.Exists)
            {
                using (var m_dbConnection =
                    new SqliteConnection($"Data Source={translationDb.FullName}"))
                {
                    m_dbConnection.Open();

                    using (SqliteCommand verify_table =
                        new SqliteCommand("PRAGMA table_info(translations);", m_dbConnection))
                    {
                        SqliteDataReader r = verify_table.ExecuteReader();

                        List<string> tableColumnStrikeoutList = new List<string>(translationTableColumns);

                        while (r.Read())
                        {
                            var columnName = Convert.ToString(r["name"]);
                            if (tableColumnStrikeoutList.Count == 0)
                            {
                                //this means there are more columns in the table than expected,
                                //so probably the table has been created with newer version
                                tableValid = false;
                            }
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
                var box = MessageBoxManager.GetMessageBoxStandard(
                    OpusCatMtEngine.Properties.Resources.App_ConfirmDbCaption,
                    OpusCatMtEngine.Properties.Resources.App_InvalidDbMessage,             
                    ButtonEnum.OkCancel);

                var result = await box.ShowAsync();

                if (result == ButtonResult.Ok)
                {
                    translationDb.Delete();
                }
                else
                {
                    OpusCatMtEngineSettings.Default.CacheMtInDatabase = false;
                    OpusCatMtEngineSettings.Default.Save();
                    OpusCatMtEngineSettings.Default.Reload();
                }
            }

            translationDb.Refresh();
            if (!translationDb.Exists)
            {
                CreateTranslationDb();
            }

            //Remove old translation from the db (time period can be set in settings)
            if (OpusCatMtEngineSettings.Default.CacheMtInDatabase)
            {
                TranslationDbHelper.RemoveOldTranslations();
            }
        }

        private static void RemoveOldTranslations()
        {
            var translationDb = new FileInfo(
                HelperFunctions.GetOpusCatDataPath(OpusCatMtEngineSettings.Default.TranslationDBName));

            using (var m_dbConnection = new SqliteConnection($"Data Source={translationDb}"))
            {
                m_dbConnection.Open();

                using (SqliteCommand deleteOld =
                    new SqliteCommand("DELETE FROM translations WHERE additiondate <= date('now',@period)", m_dbConnection))
                {
                    deleteOld.Parameters.Add(
                        new SqliteParameter(
                            "@period", $"-{OpusCatMtEngineSettings.Default.DatabaseRemovalInterval} days"));
                    deleteOld.ExecuteNonQuery();
                }
            }
        }

        private static void CreateTranslationDb()
        {
            var translationDb = new FileInfo(HelperFunctions.GetOpusCatDataPath(OpusCatMtEngineSettings.Default.TranslationDBName));
            
            using (var m_dbConnection = new SqliteConnection($"Data Source={translationDb}"))
            {
                m_dbConnection.Open();

                string sql = "create table translations (model TEXT, sourcetext TEXT, translation TEXT, segmentedsource TEXT, segmentedtranslation TEXT, alignment TEXT, additiondate DATETIME, segmentationmethod TEXT, targetlanguage TEXT, maxlength INTEGER, PRIMARY KEY (model,sourcetext,targetlanguage,maxlength))";

                using (SqliteCommand command = new SqliteCommand(sql, m_dbConnection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void WriteTranslationToSqliteDb(
            string sourceText, 
            TranslationPair translation, 
            string model, 
            SegmentationMethod segmentationMethod, 
            string targetLanguage)
        {
            
            var translationDb = new FileInfo(HelperFunctions.GetOpusCatDataPath(OpusCatMtEngineSettings.Default.TranslationDBName));
            
            if (translationDb.Length == 0)
            {
                translationDb.Delete();
            }

            if (!translationDb.Exists)
            {
                TranslationDbHelper.CreateTranslationDb();
            }

            using (var m_dbConnection = new SqliteConnection($"Data Source={translationDb}"))
            {
                m_dbConnection.Open();

                using (SqliteCommand insert =
                    new SqliteCommand(
                        "INSERT or REPLACE INTO translations (sourcetext, translation, segmentedsource, segmentedtranslation, alignment, model, additiondate, segmentationmethod, targetlanguage, maxlength) VALUES (@sourcetext,@translation,@segmentedsource,@segmentedtranslation,@alignment,@model,CURRENT_TIMESTAMP,@segmentationmethod,@targetlanguage,@maxlength)", m_dbConnection))
                {
                    insert.Parameters.Add(new SqliteParameter("@sourcetext", sourceText));
                    insert.Parameters.Add(new SqliteParameter("@translation", translation.Translation));
                    insert.Parameters.Add(new SqliteParameter("@segmentedsource", String.Join(" ", translation.SegmentedSourceSentence)));
                    insert.Parameters.Add(new SqliteParameter("@segmentedtranslation", String.Join(" ", translation.SegmentedTranslation)));
                    insert.Parameters.Add(new SqliteParameter("@alignment", translation.AlignmentString));
                    insert.Parameters.Add(new SqliteParameter("@model", model));
                    insert.Parameters.Add(new SqliteParameter("@segmentationmethod", segmentationMethod.ToString()));
                    insert.Parameters.Add(new SqliteParameter("@targetlanguage", targetLanguage));
                    insert.Parameters.Add(new SqliteParameter("@maxlength", OpusCatMtEngineSettings.Default.MaxLength));
                    insert.ExecuteNonQuery();
                }
            }
        }

        internal static void WriteTranslationToDb(
            string sourceText, 
            TranslationPair translation, 
            string model,
            SegmentationMethod segmentationMethod,
            string targetLanguage)
        {
            TranslationDbHelper.shortTermMtStorage.GetOrAdd(new Tuple<string, string>(sourceText, model), translation);
            if (OpusCatMtEngineSettings.Default.CacheMtInDatabase)
            {
                try
                {
                    TranslationDbHelper.WriteTranslationToSqliteDb(sourceText, translation, model, segmentationMethod, targetLanguage);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    TranslationDbHelper.SetupTranslationDb();
                }
            }
        }

        private static TranslationPair FetchTranslationFromSqliteDb(string sourceText, string model, string targetLanguage)
        {
            var translationDb = HelperFunctions.GetOpusCatDataPath(OpusCatMtEngineSettings.Default.TranslationDBName);

            List<TranslationPair> translationPairs = new List<TranslationPair>();
            using (var m_dbConnection = new SqliteConnection($"Data Source={translationDb};Version=3;"))
            {
                m_dbConnection.Open();

                using (SqliteCommand fetch =
                    new SqliteCommand("SELECT DISTINCT translation,segmentedsource,segmentedtranslation,alignment,segmentationmethod FROM translations WHERE sourcetext=@sourcetext AND model=@model AND targetlanguage=@targetlanguage LIMIT 1", m_dbConnection))
                {
                    fetch.Parameters.Add(new SqliteParameter("@sourcetext", sourceText));
                    fetch.Parameters.Add(new SqliteParameter("@model", model));
                    fetch.Parameters.Add(new SqliteParameter("@targetlanguage", targetLanguage));
                    SqliteDataReader r = fetch.ExecuteReader();

                    while (r.Read())
                    {
                        var segmentedSource = Convert.ToString(r["segmentedsource"]);
                        var segmentedTranslation = Convert.ToString(r["segmentedtranslation"]);
                        var translation = Convert.ToString(r["translation"]);
                        var alignment = Convert.ToString(r["alignment"]);
                        var segmentationMethodString = Convert.ToString(r["segmentationmethod"]);
                        SegmentationMethod segmentationMethod;
                        Enum.TryParse<SegmentationMethod>(segmentationMethodString,out segmentationMethod);
                        translationPairs.Add(
                            new TranslationPair(
                                translation, 
                                segmentedSource,
                                segmentedTranslation, 
                                alignment, 
                                segmentationMethod, 
                                targetLanguage));
                    }
                }

            }
            return translationPairs.SingleOrDefault();
        }

        internal static TranslationPair FetchTranslationFromDb(string sourceText, string model, string targetLanguage)
        {
            TranslationPair translationPair = null;
            if (OpusCatMtEngineSettings.Default.CacheMtInDatabase)
            {
                try
                {
                    translationPair = FetchTranslationFromSqliteDb(sourceText, model,targetLanguage);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    translationPair = null;
                }
            }

            if (translationPair == null)
            {
                TranslationDbHelper.shortTermMtStorage.TryGetValue(
                    new Tuple<string, string>(sourceText, model), out translationPair);
            }
            
            return translationPair;
        }
    }
}
