using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OpusCatMtEngine
{
    /// <summary>
    /// Interaction logic for SelectTmxLangPair.xaml
    /// </summary>
    public partial class SelectTmxLangPairWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private IEnumerable<KeyValuePair<Tuple<string, string>, int>> _eligiblePairs;
        private KeyValuePair<Tuple<string, string>, int> _selectedPair;

        public SelectTmxLangPairWindow(IEnumerable<KeyValuePair<Tuple<string, string>, int>> eligibleLangPairs)
        {
            this.DataContext = this;
            this.EligiblePairs = eligibleLangPairs;
            this.SelectedPair = this.EligiblePairs.First();
            InitializeComponent();
        }

        public IEnumerable<KeyValuePair<Tuple<string, string>, int>> EligiblePairs
        {
            get => _eligiblePairs;
            set
            {
                _eligiblePairs = value;
                NotifyPropertyChanged();
            }
        }
        public KeyValuePair<Tuple<string, string>, int> SelectedPair
        {
            get => _selectedPair;
            set
            {
                _selectedPair = value;
                NotifyPropertyChanged();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void UseSelected_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
