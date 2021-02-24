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
        internal static void WriteTranslationToDb(string sourceText, string translation, string model)
        {
            var translationDb = HelperFunctions.GetOpusCatDataPath(OpusCatMTEngineSettings.Default.TranslationDBName);

            using (var m_dbConnection = new SQLiteConnection($"Data Source={translationDb};Version=3;"))
            {
                m_dbConnection.Open();

                using (SQLiteCommand insert =
                    new SQLiteCommand("INSERT or REPLACE INTO translations (sourcetext, translation, model) VALUES (@sourcetext,@translation,@model)", m_dbConnection))
                {
                    insert.Parameters.Add(new SQLiteParameter("@sourcetext", sourceText));
                    insert.Parameters.Add(new SQLiteParameter("@translation", translation));
                    insert.Parameters.Add(new SQLiteParameter("@model", model));
                    insert.ExecuteNonQuery();
                }
            }
        }

        internal static string FetchTranslationFromDb(string sourceText, string model)
        {
            var translationDb = HelperFunctions.GetOpusCatDataPath(OpusCatMTEngineSettings.Default.TranslationDBName);

            List<string> items = new List<string>();

            using (var m_dbConnection = new SQLiteConnection($"Data Source={translationDb};Version=3;"))
            {
                m_dbConnection.Open();

                using (SQLiteCommand fetch =
                    new SQLiteCommand("SELECT DISTINCT translation FROM translations WHERE sourcetext=@sourcetext AND model=@model LIMIT 1", m_dbConnection))
                {
                    fetch.Parameters.Add(new SQLiteParameter("@sourcetext", sourceText));
                    fetch.Parameters.Add(new SQLiteParameter("@model", model));
                    SQLiteDataReader r = fetch.ExecuteReader();

                    while (r.Read())
                    {
                        items.Add(Convert.ToString(r["translation"]));
                    }
                }

            }

            return items.SingleOrDefault();
        }
    }
}
