using Sdl.LanguagePlatform.Core;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using Sdl.ProjectAutomation.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpusCatTranslationProvider
{
    /// <summary>
    /// Interaction logic for OpusCatOptions.xaml
    /// </summary>
    public partial class OpusCatOptionControl : UserControl, INotifyPropertyChanged, IHasOpusCatOptions
    {

        public OpusCatOptionControl(OpusCatOptionsFormWPF hostForm, 
            OpusCatOptions options, 
            Sdl.LanguagePlatform.Core.LanguagePair[] languagePairs, 
            Sdl.LanguagePlatform.TranslationMemoryApi.ITranslationProviderCredentialStore credentialStore)
        {
            this.DataContext = this;
            this.CredentialStore = credentialStore;
            this.Options = options;

#if (TRADOS22)
            this.projectLanguagePairs = languagePairs.Select(
                x => $"{new CultureInfo(x.SourceCulture.Name).TwoLetterISOLanguageName}-" +
                $"{new CultureInfo(x.TargetCulture.Name).TwoLetterISOLanguageName}").ToList();
#else
            this.projectLanguagePairs = languagePairs.Select(
                x => $"{x.SourceCulture.TwoLetterISOLanguageName}-{x.TargetCulture.TwoLetterISOLanguageName}").ToList();
#endif

            InitializeComponent();
            this.ConnectionSelection.LanguagePairs = this.projectLanguagePairs;

            //Null indicates that all properties have changed. Populates the WPF form
            PropertyChanged(this, new PropertyChangedEventArgs(null));
            
            this.hostForm = hostForm;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private OpusCatOptions options;
        private List<string> projectLanguagePairs;
        private OpusCatOptionsFormWPF hostForm;

        public string MaxPreorderString { get { return $"segments (max {OpusCatTpSettings.Default.PregenerateSegmentCountMax})"; } }

        public ITranslationProviderCredentialStore CredentialStore { get; private set; }
        public OpusCatOptions Options { get => options; set => options = value; }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.hostForm.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            this.hostForm.DialogResult = System.Windows.Forms.DialogResult.OK;
        }


        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void TagBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ConnectionSelection.ConnectionControl.TagBox_SelectionChanged(sender, e);
        }

        private void NumberBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int textAsInt;
            var isInt = Int32.TryParse(e.Text, out textAsInt);
            //Fix this, why doesn't it implement max?
            
            e.Handled = !(isInt && textAsInt <= 10);
        }
    }
}
