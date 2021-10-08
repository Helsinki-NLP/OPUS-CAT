using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Shapes;

namespace OpusCatMTEngine
{
    /// <summary>
    /// Interaction logic for Overlay.xaml
    /// </summary>
    public partial class Overlay : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Overlay()
        {
            InitializeComponent();
            this.Show();
        }

        public string MtFontSize
        {
            get { return OpusCatMTEngineSettings.Default.OverlayFontsize.ToString(); }
            set
            {
                int intSize;
                var success = Int32.TryParse(value, out intSize);
                if (success)
                {
                    OpusCatMTEngineSettings.Default.OverlayFontsize = Int32.Parse(value);
                    OpusCatMTEngineSettings.Default.Save();
                }
                NotifyPropertyChanged();
            }
        }

        private void PreviewNumberInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }
        
        internal void ShowMessageInOverlay(string message)
        {
            this.TranslationBox.Document.Blocks.Clear();
            this.TranslationBox.Document.Blocks.Add(new Paragraph(new Run(message)));
        }

        internal void UpdateTranslation(TranslationPair result)
        {
            this.ShowMessageInOverlay(result.Translation);
        }

        internal void ClearTranslation()
        {
            this.TranslationBox.Document.Blocks.Clear();
        }
    }
}
