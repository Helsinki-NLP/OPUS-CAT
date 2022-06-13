﻿using Octokit;
using Python.Runtime;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace OpusCatMTEngine
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static Overlay Overlay { get; private set; }

        public static void OpenOverlay()
        {
            if (App.Overlay == null)
            {
                App.Overlay = new Overlay();
            }
        }

        public static void CloseOverlay()
        {
            if (App.Overlay != null)
            {
                App.Overlay.Close();
                App.Overlay = null;
            }
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception
            MessageBox.Show($"A fatal exception occurred. See details in log file. Exception: {e.Exception.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            Log.Error(e.Exception.ToString());
        }

        private void SetupLogging()
        {
            var logDir = HelperFunctions.GetOpusCatDataPath(OpusCatMTEngineSettings.Default.LogDir);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Path.Combine(logDir, "opuscat_log.txt"), rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }
        
        public static bool HasAvxSupport()
        {
            try
            {
                return (GetEnabledXStateFeatures() & 4) != 0;
            }
            catch
            {
                return false;
            }
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern long GetEnabledXStateFeatures();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            System.Windows.Application.Current.DispatcherUnhandledException += App_DispatcherUnhandledException;

            if (!App.HasAvxSupport())
            {
                MessageBox.Show(
                    "OPUS-CAT MT Engine requires a CPU with AVX support. Your CPU does not support AVX, so OPUS-CAT MT Engine cannot start.");
                System.Windows.Application.Current.Shutdown(1);
            }

            //Create data dir

            var opusCatDataDir = HelperFunctions.GetOpusCatDataPath();
            if (!Directory.Exists(opusCatDataDir))
            {
                Directory.CreateDirectory(opusCatDataDir);
            }

            this.CopyConfigs();
            this.SetupLogging();

            //Accessing the model storage on pouta requires this.
            Log.Information("Setting Tls12 as security protocol (required for accessing online model storage");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Log.Information("Opening OPUS-CAT MT Engine window");

            // Create the startup window
            //TODO: make it possible to start the engine without launching UI
            MainWindow wnd = new MainWindow();
            // Show the window
            wnd.Show();

            if (OpusCatMTEngineSettings.Default.DisplayOverlay)
            {
                App.OpenOverlay();
            }
            else
            {
                App.CloseOverlay();
            }

            this.InitializePythonEngine();

            this.CheckForUpdates();
        }

        private void CheckForUpdates()
        {
            //Get info for all releases from Github
            var client = new GitHubClient(new ProductHeaderValue("OpusCatMTEngine"));
            var releases = client.Repository.Release.GetAll("Helsinki-NLP", "OPUS-CAT");
            var releases_with_notes = releases.Result.Where(x => x.Assets.Any(y => y.Name == "release_notes.txt"));
            var latest_release = releases_with_notes.OrderByDescending(x => x.PublishedAt).First();
            var download_url = latest_release.Assets.Single(x => x.Name == "release_notes.txt").BrowserDownloadUrl;
            string release_notes;
            using (var webClient = new WebClient())
            {
                release_notes = webClient.DownloadString(new Uri(download_url));
            }
            //First line of release notes is the version
            var latestVersion = new Version(release_notes.Split(new[] { '\r', '\n' }).First());
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            
            if (latestVersion > currentVersion)
            {
                string messageBoxText = "A new OPUS-CAT version is available. Click OK to download open the download page for new version.";
                string caption = "New version";
                MessageBoxButton button = MessageBoxButton.OKCancel;
                MessageBoxImage icon = MessageBoxImage.Information;
                MessageBoxResult result;
                result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
                if (result == MessageBoxResult.OK)
                {
                    System.Diagnostics.Process.Start(latest_release.HtmlUrl);
                }
            }
        }

        private void InitializePythonEngine()
        {
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", ".\\python-3.8.10-embed-amd64\\python38.dll");
            Environment.SetEnvironmentVariable("PATH", ".\\python-3.8.10-embed-amd64");
            Environment.SetEnvironmentVariable("PYTHONPATH", ".\\python-3.8.10-embed-amd64");
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
        }

        /// <summary>
        /// Copy customization config file from the executable dir (those are kept as default which you can revert to)
        /// </summary>
        private void CopyConfigs()
        {
            FileInfo baseCustomizeYml = new FileInfo(
                HelperFunctions.GetLocalAppDataPath(OpusCatMTEngineSettings.Default.CustomizationBaseConfig));
            FileInfo defaultCustomizeYml = new FileInfo(OpusCatMTEngineSettings.Default.CustomizationBaseConfig);
            //There might be a previous customize.yml file present, don't overwrite it unless it's older
            if (!baseCustomizeYml.Exists || (defaultCustomizeYml.LastWriteTime > baseCustomizeYml.LastWriteTime))
            {
                File.Copy(OpusCatMTEngineSettings.Default.CustomizationBaseConfig, baseCustomizeYml.FullName,true);
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
