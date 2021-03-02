using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpusCatTranslationProvider;
using Studio.AssemblyResolver;

namespace SettingsUiTester
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            AssemblyResolver.Resolve();
            Program.ShowSettings(args);
        }

        static void ShowSettings(string[] args)
        {
            Sdl.LanguagePlatform.Core.LanguagePair[] languagePairs = new Sdl.LanguagePlatform.Core.LanguagePair[] { new Sdl.LanguagePlatform.Core.LanguagePair("en", "fi") };
            var settingsWindow = new OpusCatTranslationProvider.OpusCatOptionControl(null,new OpusCatOptions(), languagePairs, null);
            Window window = new Window
            {
                Title = "UI settings",
                Content = settingsWindow
            };
            window.ShowDialog();
        }
    }
}
