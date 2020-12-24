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

namespace OpusCatMTEngine
{
    /// <summary>
    /// Interaction logic for TranslateWindow.xaml
    /// </summary>
    public partial class TagEditView: UserControl
    {

        private MTModel model;

        public TagEditView(MTModel selectedModel)
        {
            this.Model = selectedModel;
            this.DataContext = selectedModel;
            this.Title = $"Edit tags for {Model.Name}";
            InitializeComponent();
            this.TagList.ItemsSource = selectedModel.ModelConfig.ModelTags;
        }

        public MTModel Model { get => model; set => model = value; }
        public string Title { get; private set; }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            this.Model.ModelConfig.ModelTags.Add(this.TagTextBox.Text);
        }

        private void DeleteTag_Click(object sender, RoutedEventArgs e)
        {
            string tag = (string)((Button)sender).Tag;
            this.Model.ModelConfig.ModelTags.Remove(tag);
        }
    }
}
