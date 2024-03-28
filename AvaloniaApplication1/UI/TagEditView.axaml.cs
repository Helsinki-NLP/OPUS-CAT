using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace OpusCatMtEngine
{
    public partial class TagEditView : UserControl
    {
        public TagEditView()
        {
        }

        private MTModel model;

        public TagEditView(MTModel selectedModel)
        {
            this.Model = selectedModel;
            this.DataContext = selectedModel;
            this.Title = String.Format(Properties.Resources.Tags_EditTagsTitle, Model.Name);
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
