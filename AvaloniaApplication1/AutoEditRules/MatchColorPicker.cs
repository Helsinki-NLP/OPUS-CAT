using Avalonia.Media;

namespace OpusCatMtEngine
{
    public static class MatchColorPicker
    {
        private static int colorIndex = 0;
        public static Brush[] MatchColorList = new Brush[]
        {
            (Brush) Brushes.AliceBlue,
            (Brush) Brushes.CadetBlue,
            (Brush) Brushes.HotPink,
            (Brush) Brushes.LightGreen,
            (Brush) Brushes.DodgerBlue,
            (Brush) Brushes.Fuchsia,
            (Brush) Brushes.Orange,
            (Brush) Brushes.Aquamarine
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
