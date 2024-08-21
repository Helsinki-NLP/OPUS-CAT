﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OpusCatMtEngine
{
    public static class MatchColorPicker
    {
        private static int colorIndex = 0;
        public static Brush[] MatchColorList = new Brush[]
        {
            Brushes.Chartreuse,
            Brushes.CadetBlue,
            Brushes.HotPink,
            Brushes.LightGreen,
            Brushes.DodgerBlue,
            Brushes.Fuchsia,
            Brushes.Orange,
            Brushes.Aquamarine
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
