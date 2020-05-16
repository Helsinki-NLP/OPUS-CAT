using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FiskmoTranslationProvider
{
    public partial class FiskmoOptionsFormWPF : Form
    {
        public FiskmoOptionsFormWPF(FiskmoOptions options, Sdl.LanguagePlatform.Core.LanguagePair[] languagePairs)
        {
            this.Options = options;
            InitializeComponent();
            this.wpfHost.Child = new FiskmoOptionControl(this, options, languagePairs);
        }

        public FiskmoOptions Options { get; internal set; }

    }
}
