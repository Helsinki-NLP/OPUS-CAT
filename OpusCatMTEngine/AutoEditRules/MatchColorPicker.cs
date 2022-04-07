using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OpusCatMTEngine
{
    public static class MatchColorPicker
    {
        private static int colorIndex = 0;
        public static Brush[] MatchColorList = new Brush[]
        {
            Brushes.Chartreuse,
            Brushes.CadetBlue,
            Brushes.ForestGreen,
            Brushes.DeepPink,
            Brushes.DodgerBlue,
            Brushes.Fuchsia,
            Brushes.Orange,
            Brushes.Indigo
        };

        public static void ResetIndex()
        {
            MatchColorPicker.colorIndex = 0;
        }

        public static Brush GetNextMatchColor()
        {

            var matchColorIndex = MatchColorPicker.colorIndex % MatchColorPicker.MatchColorList.Length;
            MatchColorPicker.colorIndex += 1;
            return MatchColorPicker.MatchColorList[matchColorIndex];
        }
    }
}
