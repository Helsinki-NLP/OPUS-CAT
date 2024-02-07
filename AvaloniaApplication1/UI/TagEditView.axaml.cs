using Avalonia.Controls;

namespace OpusCatMtEngine
{
    public partial class TagEditView : UserControl
    {
        public TagEditView(MTModel selectedModel)
        {
            InitializeComponent();
        }

        public string Title { get; internal set; }
    }
}
