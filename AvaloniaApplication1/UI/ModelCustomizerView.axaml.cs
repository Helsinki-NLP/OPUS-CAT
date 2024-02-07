using Avalonia.Controls;

namespace OpusCatMtEngine
{
    public partial class ModelCustomizerView : UserControl
    {
        public ModelCustomizerView(MTModel selectedModel)
        {
            InitializeComponent();
        }

        public string Title { get; internal set; }
    }
}
