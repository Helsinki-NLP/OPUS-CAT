using OpusMTInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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
using System.Windows.Threading;

namespace FiskmoMTEngine
{
    /// <summary>
    /// Interaction logic for TranslateWindow.xaml
    /// </summary>
    public partial class TagEditWindow : Window
    {
        
        private MTModel model;

        public TagEditWindow(MTModel selectedModel)
        {
            this.Model = selectedModel;
            this.DataContext = selectedModel;
            this.Title = $"Edit tags for {Model.Name}";
            InitializeComponent();
            this.TagList.ItemsSource = selectedModel.ModelTags;
        }

        public MTModel Model { get => model; set => model = value; }

 
        private void add_Click(object sender, RoutedEventArgs e)
        {
            this.Model.ModelTags.Add(this.TagTextBox.Text);
        }

        private void DeleteTag_Click(object sender, RoutedEventArgs e)
        {
            string tag = (string)((Button)sender).Tag;
            this.Model.ModelTags.Remove(tag);
        }
    }
}
