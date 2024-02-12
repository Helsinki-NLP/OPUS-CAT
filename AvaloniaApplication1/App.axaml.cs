using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Octokit;
using OpusCatMtEngine;
using Python.Runtime;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace OpusCatMtEngine
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Avalonia.Application
    {


        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {

            //System.Windows.Application.Current.DispatcherUnhandledException += App_DispatcherUnhandledException;

            /*TODO: use AVXless Marian
             * if (!App.HasAvxSupport())
            {
                MessageBox.Show(
                    "OPUS-CAT MT Engine requires a CPU with AVX support. Your CPU does not support AVX, so OPUS-CAT MT Engine cannot start.");
                System.Windows.Application.Current.Shutdown(1);
            }*/

            //Create data dir

            var opusCatDataDir = HelperFunctions.GetOpusCatDataPath();
            if (!Directory.Exists(opusCatDataDir))
            {
                Directory.CreateDirectory(opusCatDataDir);
            }

            this.CopyConfigs();
            this.SetupLogging();

            //Accessing the model storage on pouta requires this.
            //TODO: Check if this is still relevant (i.e. if download work, remove this)
            //Log.Information("Setting Tls12 as security protocol (required for accessing online model storage");
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            this.InitializePythonEngine();

            Log.Information("Opening OPUS-CAT MT Engine window");

            // Create the startup window
            //TODO: make it possible to start the engine without launching UI
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
                desktop.MainWindow.Show();
            }

            if (OpusCatMtEngineSettings.Default.DisplayOverlay)
            {
                App.OpenOverlay();
            }
            else
            {
                App.CloseOverlay();
            }

            //The update check is used to keep track of use counts, so disable it in DEBUG mode to keep counts
            //more accurate
#if !DEBUG
            this.CheckForUpdatesAsync();
#endif


            base.OnFrameworkInitializationCompleted();
        }
    

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

        /*TODO: check if this is needed in cross-platform, avalonia does not support it
         * void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception
            MessageBox.Show($"A fatal exception occurred. See details in log file. Exception: {e.Exception.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            Log.Error(e.Exception.ToString());
        }*/

        private void SetupLogging()
        {
            var logDir = HelperFunctions.GetOpusCatDataPath(OpusCatMtEngineSettings.Default.LogDir);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Path.Combine(logDir, "opuscat_log.txt"), rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }
        //TODO: this is Windows only, check if bergamot Marian works on old machines, 
        //otherwise do a cross-platform version
        /*public static bool HasAvxSupport()
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
        */
        

        private async void CheckForUpdatesAsync()
        {
            //Get info for all releases from Github
            try
            {
                var client = new GitHubClient(new ProductHeaderValue("OpusCatMtEngine"));
                var releases = await client.Repository.Release.GetAll(
                    OpusCatMtEngineSettings.Default.GithubOrg,
                    OpusCatMtEngineSettings.Default.GithubRepo);

                var releasesWithNotes = releases.Where(x => x.Assets.Any(y => y.Name == "release_notes.txt") && !x.Prerelease);
                var latestRelease = releasesWithNotes.OrderByDescending(x => x.PublishedAt).FirstOrDefault();
                if (latestRelease == null)
                {
                    Log.Information("No release with release notes found in Github repo");
                    return;
                }

                var downloadUrl = latestRelease.Assets.Single(x => x.Name == "release_notes.txt").BrowserDownloadUrl;



                string release_notes;
                using (var webClient = new WebClient())
                {
                    release_notes = webClient.DownloadString(new Uri(downloadUrl));
                }

                var latestVersion = new Version(release_notes.Split(new[] { '\r', '\n' }).First());
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

                if (latestVersion > currentVersion)
                {
                    string messageBoxText = "A new OPUS-CAT version is available. Click OK to download open the download page for new version.";
                    var box = MessageBoxManager.GetMessageBoxStandard(
                        "New version",
                        messageBoxText,
                        ButtonEnum.OkCancel);

                    var result = await box.ShowAsync();

                    if (result == ButtonResult.Ok)
                    {
                        System.Diagnostics.Process.Start(latestRelease.HtmlUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Exception during update check: {ex.Message}");
            }

        }


        
        private void InitializePythonEngine()
        {
#if WINDOWS
            //Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", ".\\python-3.8.10-embed-amd64\\python38.dll;");
            Environment.SetEnvironmentVariable("PATH", ".\\python-3.8.10-embed-amd64;");
            //Environment.SetEnvironmentVariable("PYTHONPATH", ".\\python-3.8.10-embed-amd64;");
            //Environment.SetEnvironmentVariable("PYTHONHOME", ".\\python-3.8.10-embed-amd64;");
            Runtime.PythonDLL = "..\\python-3.8.10-embed-amd64\\python38.dll";
            
#else
            //Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", ".\\python-3.8.10-embed-amd64\\python38.dll;");
            //Environment.SetEnvironmentVariable("PATH", ".\\python-3.8.10-embed-amd64;");
            //Environment.SetEnvironmentVariable("PYTHONPATH", ".\\python-3.8.10-embed-amd64;");
            Environment.SetEnvironmentVariable("PYTHONHOME", "/home/tommin/embed_python_test");
            Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", "/home/tommin/opt/lib:/home/tommin/embed_python_test/lib");
            Runtime.PythonDLL = "/home/tommin/embed_python_test/lib/libpython3.8.so.1.0";
#endif
            PythonEngine.PythonHome = "/home/tommin/embed_python_test";
            PythonEngine.Initialize();
            var home = PythonEngine.PythonHome;

            
            PythonEngine.BeginAllowThreads();
            using (Py.GIL())
            {
                Py.Import("sacremoses");
            }
        }

        /// <summary>
        /// Copy customization config file from the executable dir (those are kept as default which you can revert to)
        /// </summary>
        private void CopyConfigs()
        {
            FileInfo baseCustomizeYml = new FileInfo(
                HelperFunctions.GetOpusCatDataPath(OpusCatMtEngineSettings.Default.CustomizationBaseConfig));
            FileInfo defaultCustomizeYml = new FileInfo(OpusCatMtEngineSettings.Default.CustomizationBaseConfig);
            //There might be a previous customize.yml file present, don't overwrite it unless it's older
            if (!baseCustomizeYml.Exists || (defaultCustomizeYml.LastWriteTime > baseCustomizeYml.LastWriteTime))
            {
                File.Copy(OpusCatMtEngineSettings.Default.CustomizationBaseConfig, baseCustomizeYml.FullName, true);
            }
        }

        /*TODO: Do we need similar Avalonia methods? If it works, remove these
         * protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }*/
    }
}