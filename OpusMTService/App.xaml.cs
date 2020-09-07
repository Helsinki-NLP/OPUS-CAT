using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FiskmoMTEngine
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception
            MessageBox.Show($"A fatal exception occurred. See details in log file. Exception: {e.Exception.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            Log.Error(e.Exception.ToString());
        }

        private void SetupLogging()
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                FiskmoMTEngineSettings.Default.LocalFiskmoDir,
                FiskmoMTEngineSettings.Default.LogDir);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Path.Combine(logDir, "fiskmö_log.txt"), rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        private void SetupTranslationDb()
        {


            var translationDb = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                FiskmoMTEngineSettings.Default.LocalFiskmoDir,
                FiskmoMTEngineSettings.Default.TranslationDBName);
            if (!File.Exists(translationDb))
            {
                SQLiteConnection.CreateFile(translationDb);
                using (var m_dbConnection = new SQLiteConnection($"Data Source={translationDb};Version=3;"))
                {
                    m_dbConnection.Open();

                    string sql = "create table translations (model TEXT, sourcetext TEXT, translation TEXT, PRIMARY KEY (model,sourcetext))";

                    using (SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Application.Current.DispatcherUnhandledException += App_DispatcherUnhandledException;

            //Create appdata dir
            var fiskmoAppdataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                FiskmoMTEngineSettings.Default.LocalFiskmoDir);
            if (!Directory.Exists(fiskmoAppdataDir))
            {
                Directory.CreateDirectory(fiskmoAppdataDir);
            }

            this.CopyConfigs();
            this.SetupTranslationDb();
            this.SetupLogging();

            //Accessing the model storage on pouta requires this.
            Log.Information("Setting Tls12 as security protocol (required for accessing online model storage");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Log.Information("Opening Fiskmö MT service window");

            // Create the startup window
            MainWindow wnd = new MainWindow();
            // Show the window
            wnd.Show();
        }

        /// <summary>
        /// Copy customization config file from the executable dir (those are kept as default which you can revert to)
        /// </summary>
        private void CopyConfigs()
        {
            FileInfo baseCustomizeYml = new FileInfo(
                HelperFunctions.GetLocalAppDataPath(FiskmoMTEngineSettings.Default.CustomizationBaseConfig));
            FileInfo defaultCustomizeYml = new FileInfo(FiskmoMTEngineSettings.Default.CustomizationBaseConfig);
            //There might be a previous customize.yml file present, don't overwrite it unless it's older
            if (!baseCustomizeYml.Exists || (defaultCustomizeYml.LastWriteTime > baseCustomizeYml.LastWriteTime))
            {
                File.Copy(FiskmoMTEngineSettings.Default.CustomizationBaseConfig, baseCustomizeYml.FullName,true);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
