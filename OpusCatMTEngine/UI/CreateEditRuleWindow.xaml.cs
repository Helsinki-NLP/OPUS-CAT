using System;
using System.Collections.Generic;
using System.Linq;
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

namespace OpusCatMTEngine
{
    public partial class CreateEditRuleWindow : Window
    {
        public CreateEditRuleWindow()
        {
            InitializeComponent();
        }

        private void PreEditTest_Click(object sender, RoutedEventArgs e)
        {
            var ruleCollection = new AutoEditRuleCollection();
            ruleCollection.AddRule(
                new AutoEditRule() {
                    SourcePattern = this.PreEditPattern.Text, Replacement = this.PreEditReplacement.Text
                });

            TextRange textRange = new TextRange(this.SourceBox.Document.ContentStart, this.SourceBox.Document.ContentEnd);

            var result = ruleCollection.ProcessRules(textRange.Text);

            var resultRun = new Run(result.Result);
            var resultBlock = new Paragraph(resultRun);
            this.EditedSourceBox.Document.Blocks.Clear();
            this.EditedSourceBox.Document.Blocks.Add(resultBlock);
        }
    }
}
