using Avalonia.Controls;
using System.Collections.ObjectModel;

namespace OpusCatMtEngine
{
    public partial class EditRulesView : UserControl
    {
        private MTModel selectedModel;
        private ObservableCollection<AutoEditRuleCollection> autoPreEditRuleCollections;
        private ObservableCollection<AutoEditRuleCollection> autoPostEditRuleCollections;

        public EditRulesView()
        {
            InitializeComponent();
        }

        public EditRulesView(MTModel selectedModel, ObservableCollection<AutoEditRuleCollection> autoPreEditRuleCollections, ObservableCollection<AutoEditRuleCollection> autoPostEditRuleCollections)
        {
            this.selectedModel = selectedModel;
            this.autoPreEditRuleCollections = autoPreEditRuleCollections;
            this.autoPostEditRuleCollections = autoPostEditRuleCollections;
        }

        public string Title { get; internal set; }
    }
}
