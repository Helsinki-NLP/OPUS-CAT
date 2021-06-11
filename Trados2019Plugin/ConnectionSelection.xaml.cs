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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpusCatTranslationProvider
{
    /// <summary>
    /// Interaction logic for ConnectionSelection.xaml
    /// </summary>
    public partial class ConnectionSelection : UserControl, INotifyPropertyChanged
    {
        private OpusCatOptions options;

        public ConnectionSelection()
        {
            InitializeComponent();
            PropertyChanged(this, new PropertyChangedEventArgs(null));
            this.DataContextChanged += ConnectionControl_DataContextChanged;
            this.ElgConnectionControl.PropertyChanged += ConnectionControl_PropertyChanged;
            this.ConnectionControl.PropertyChanged += ConnectionControl_PropertyChanged;
        }

        private void ConnectionControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged("ConnectionStatus");
            NotifyPropertyChanged("ConnectionColor");
        }

        private void ConnectionControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is IHasOpusCatOptions)
            {

                this.options = ((IHasOpusCatOptions)e.NewValue).Options;
                PropertyChanged(this, new PropertyChangedEventArgs(null));
            }
        }
        
        public ConnectionControl ConnectionControlElement
        {
            get { return this.ConnectionControl; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        
        public bool UseElgConnection
        {
            get
            {
                if (this.DataContext == null)
                {
                    return false;
                }
                return this.options.opusCatSource == OpusCatOptions.OpusCatSource.Elg;
            }
            set
            {
                this.options.opusCatSource = OpusCatOptions.OpusCatSource.Elg;
                PropertyChanged(this, new PropertyChangedEventArgs(null));
            }
        }

        public Brush ConnectionColor
        {
            get
            {
                switch (this.options.opusCatSource)
                {
                    case OpusCatOptions.OpusCatSource.OpusCatMtEngine:
                        return this.ConnectionControl.ConnectionColor;
                    case OpusCatOptions.OpusCatSource.Elg:
                        return this.ElgConnectionControl.ConnectionColor;
                    default:
                        return null;
                }
            }
        }

        public string ConnectionStatus
        {
            get
            {
                switch (this.options.opusCatSource)
                {
                    case OpusCatOptions.OpusCatSource.OpusCatMtEngine:
                        return this.ConnectionControl.ConnectionStatus;
                    case OpusCatOptions.OpusCatSource.Elg:
                        return this.ElgConnectionControl.ConnectionStatus;
                    default:
                        return null;
                }
            }
        }


        public bool UseOpusCatConnection
        {
            get
            {
                if (this.DataContext == null)
                {
                    return false;
                }
                
                return this.options.opusCatSource == OpusCatOptions.OpusCatSource.OpusCatMtEngine;
            }
            set
            {
                this.options.opusCatSource = OpusCatOptions.OpusCatSource.OpusCatMtEngine;
                this.ConnectionControl.Refresh();
                PropertyChanged(this, new PropertyChangedEventArgs(null));
            }
        }

        public List<string> LanguagePairs
        {
            set
            {
                this.ElgConnectionControl.LanguagePairs = value;
                this.ConnectionControl.LanguagePairs = value;
            }
        }

        private void SourceRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (this.DataContext == null)
            {
                return;
            }
            
            var radioButton = (RadioButton)sender;
            if (radioButton.Name == "OpusCatRadioButton")
            {
                this.options.opusCatSource = OpusCatOptions.OpusCatSource.OpusCatMtEngine;
            }
            else if (radioButton.Name == "ElgRadioButton")
            {
                this.options.opusCatSource = OpusCatOptions.OpusCatSource.Elg;
            }
        }
    }
}
