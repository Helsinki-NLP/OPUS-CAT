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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FiskmoTranslationProvider
{
    /// <summary>
    /// Interaction logic for FiskmoOptions.xaml
    /// </summary>
    public partial class FiskmoOptionControl : UserControl, IDataErrorInfo, INotifyPropertyChanged
    {
        public string this[string columnName]
        {
            get
            {
                return Validate(columnName);
            }
        }

        private void ServicePortBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        public FiskmoOptionControl(FiskmoOptionsFormWPF hostForm, FiskmoOptions options)
        {
            InitializeComponent();
            this.options = options;
            this.hostForm = hostForm;
        }

        private string Validate(string propertyName)
        {
            // Return error message if there is error on else return empty or null string
            string validationMessage = string.Empty;
            switch (propertyName)
            {
                case "ServicePortBox":
                    if (this.ServicePortBox != null && this.ServicePortBox != "")
                    {
                        var portNumber = Int32.Parse(this.ServicePortBox);
                        if (portNumber < 1024 || portNumber > 65535)
                        {
                            validationMessage = "Error";
                        }
                    }
                    else
                    {
                        validationMessage = "Error";
                    }

                    break;
            }

            return validationMessage;
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
        private FiskmoOptionsFormWPF hostForm;

        public string ServicePortBox
        {
            get => this.options.mtServicePort;
            set
            {
                this.options.mtServicePort = value;
                NotifyPropertyChanged();
            }
        }

        
        public Boolean PregenerateMt
        {
            get => this.options.pregenerateMt;
            set
            {
                this.options.pregenerateMt = value;
                NotifyPropertyChanged();
            }
        }
        public Boolean ShowMtAsOrigin
        {
            get => this.options.showMtAsOrigin;
            set
            {
                this.options.showMtAsOrigin = value;
                NotifyPropertyChanged();
            }
        }

        public string Error
        {
            get { return "...."; }
        }

        public FiskmoOptionControl()
        {
            InitializeComponent();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.hostForm.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            this.hostForm.DialogResult = System.Windows.Forms.DialogResult.OK;
        }


    }
}
