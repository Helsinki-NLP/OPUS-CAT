using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
    /// Interaction logic for FinetuneWpfControl.xaml
    /// </summary>
    public partial class FinetuneWpfControl : UserControl, INotifyPropertyChanged, IHasOpusCatOptions
    {
        private FinetuneBatchTaskSettings settings;
        private OpusCatOptions options;

        public FinetuneBatchTaskSettings Settings
        {
            get
            {
                return settings;
            }
            set
            {
                settings = value;
            }
        }

        public OpusCatOptions Options
        {
            get
            {
                return options;
            }
            set
            {
                options = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        

        public FinetuneWpfControl(FinetuneBatchTaskSettings Settings)
        {
            this.DataContext = this;

            //Settings is the object that is passed on to the batch task.
            this.Settings = Settings;
            //Mode defaults, changeable with radio buttons
            this.Settings.Finetune = true;
            this.Settings.PreOrderMtForNewSegments = true;
            //Some settings are initially held in a OpusCatOptions object (the shared properties
            //with the translation provider settings).
            this.Options = new OpusCatOptions();
            //Whenever the options change, also update the option URI string in settings
            this.Options.PropertyChanged += Options_PropertyChanged;
            InitializeComponent();
            this.TagBox.ItemsSource = new ObservableCollection<string>() { "<new tag>" };
        }

        private void Options_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.settings.ProviderOptions = this.Options.Uri.ToString();
        }

        private void ModeButton_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = ((RadioButton)sender);
            if (radioButton.IsChecked.Value)
            {
                switch (radioButton.Name)
                {
                    case "FinetuneAndTranslate":
                        this.Settings.PreOrderMtForNewSegments = true;
                        this.Settings.Finetune = true;
                        break;
                    case "FinetuneOnly":
                        this.Settings.Finetune = true;
                        this.Settings.PreOrderMtForNewSegments = false;
                        break;
                    case "TranslateOnly":
                        this.Settings.Finetune = false;
                        this.Settings.PreOrderMtForNewSegments = true;
                        break;
                }

                if (this.ConnectionControl != null && this.ConnectionControl.AllModelTags != null)
                {
                    if (this.Settings.Finetune)
                    {
                        this.TagBox.ItemsSource = new ObservableCollection<string>() { "" };
                        this.TagBox.SelectedIndex = 0;
                    }
                    else
                    {
                        this.TagBox.ItemsSource = this.ConnectionControl.AllModelTags;
                    }
                }
                
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void TagBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.settings.Finetune)
            { 
            }
            else
            {
                this.ConnectionControl.TagBox_SelectionChanged(sender, e);
            }
        }

        private void NumberBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int textAsInt;
            var isInt = Int32.TryParse(e.Text, out textAsInt);
            
            e.Handled = !(isInt && textAsInt <= 100);
        }
    }
}
