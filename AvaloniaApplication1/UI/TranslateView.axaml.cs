using Avalonia.Controls;

namespace OpusCatMtEngine
{
    public partial class TranslateView : UserControl
    {
        public TranslateView(MTModel selectedModel)
        {
            InitializeComponent();
        }

        public string Title { get; internal set; }
    }
}
