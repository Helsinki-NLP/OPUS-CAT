using Avalonia.Controls;
using Avalonia.Controls.Documents;

namespace OpusCatMtEngine
{
    public partial class MousableInline : UserControl
    {
        public MousableInline()
        {
            InitializeComponent();
        }

        public string MouseOverText
        {
            get => "";
            set => ToolTip.SetTip(this, value);
        }
    }
}
