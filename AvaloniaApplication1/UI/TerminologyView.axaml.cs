using Avalonia.Controls;
using System.Collections.ObjectModel;

namespace OpusCatMtEngine
{
    public partial class TerminologyView : UserControl
    {
        private MTModel selectedModel;
        private ObservableCollection<Terminology> terminologies;

        public TerminologyView()
        {
            InitializeComponent();
        }

        public TerminologyView(MTModel selectedModel, ObservableCollection<Terminology> terminologies)
        {
            this.selectedModel = selectedModel;
            this.terminologies = terminologies;
        }

        public string Title { get; internal set; }
    }
}
