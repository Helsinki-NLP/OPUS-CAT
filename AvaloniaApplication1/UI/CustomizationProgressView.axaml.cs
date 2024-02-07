using Avalonia.Controls;

namespace OpusCatMtEngine
{
    public partial class CustomizationProgressView : UserControl
    {
        public CustomizationProgressView(MTModel selectedModel)
        {
            InitializeComponent();
        }

        public string Title { get; internal set; }
    }
}
