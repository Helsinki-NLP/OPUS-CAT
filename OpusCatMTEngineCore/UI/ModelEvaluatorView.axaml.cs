using Avalonia.Controls;

namespace OpusCatMtEngine
{
    public partial class ModelEvaluatorView : UserControl
    {
        public ModelEvaluatorView(System.Collections.Generic.IEnumerable<MTModel> selectedModels)
        {
            InitializeComponent();
        }

        public string Title { get; internal set; }
    }
}
