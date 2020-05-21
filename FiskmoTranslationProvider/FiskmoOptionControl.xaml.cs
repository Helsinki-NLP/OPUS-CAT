using Sdl.LanguagePlatform.Core;
using Sdl.ProjectAutomation.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
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

namespace FiskmoTranslationProvider
{
    /// <summary>
    /// Interaction logic for FiskmoOptions.xaml
    /// </summary>
    public partial class FiskmoOptionControl : UserControl, INotifyPropertyChanged
    {


        public FiskmoOptionControl(FiskmoOptionsFormWPF hostForm, FiskmoOptions options, Sdl.LanguagePlatform.Core.LanguagePair[] languagePairs)
        {
            this.DataContext = this;
            
            this.Options = options;
            this.projectLanguagePairs = languagePairs.Select(
                x => $"{x.SourceCulture.TwoLetterISOLanguageName}-{x.TargetCulture.TwoLetterISOLanguageName}").ToList();

            InitializeComponent();
            this.ConnectionControl.LanguagePairs = this.projectLanguagePairs;
            this.ConnectionControl.AddModelTag(this.Options.modelTag);


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

        private FiskmoOptions options;
        private List<string> projectLanguagePairs;
        private FiskmoOptionsFormWPF hostForm;
        
        public FiskmoOptions Options { get => options; set => options = value; }

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
    }
}
