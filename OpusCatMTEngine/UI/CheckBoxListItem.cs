using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OpusCatMTEngine
{
    public class CheckBoxListItem<T> : INotifyPropertyChanged
    {
        private bool _checked;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public T Item { get; set; }
        public bool Checked
        {
            get { return _checked; }
            set { _checked = value; NotifyPropertyChanged(); }
        }

        public CheckBoxListItem(T item)
        {
            this.Item = item;
            this.Checked = false;
        }
    }
}
