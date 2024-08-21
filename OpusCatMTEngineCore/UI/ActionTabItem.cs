using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;

namespace OpusCatMtEngine
{
    // This class will be the Tab int the TabControl
    public class ActionTabItem
    {
        // This will be the text in the tab control
        public string Header { get; set; }
        // This will be the content of the tab control It is a UserControl whits you need to create manualy
        public Avalonia.Controls.UserControl Content { get; set; }

        public Boolean Closable { get; set; }
    }
}
