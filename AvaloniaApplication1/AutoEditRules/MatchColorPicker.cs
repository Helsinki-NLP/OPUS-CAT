using Avalonia.Media;

namespace OpusCatMtEngine
{
    public static class MatchColorPicker
    {
        private static int colorIndex = 0;
        public static IBrush[] MatchColorList = new IBrush[]
        {
            Brushes.CadetBlue,
            Brushes.HotPink,
            Brushes.LightGreen,
            Brushes.DodgerBlue,
            Brushes.Fuchsia,
            Brushes.Orange,
            Brushes.Aquamarine,
            Brushes.CornflowerBlue,
            Brushes.Chartreuse
            
        };

        public static void ResetIndex()
        {
            MatchColorPicker.colorIndex = 0;
        }

        public static IBrush GetNextMatchColor()
        {

            var matchColorIndex = MatchColorPicker.colorIndex % MatchColorPicker.MatchColorList.Length;
            MatchColorPicker.colorIndex += 1;
            return MatchColorPicker.MatchColorList[matchColorIndex];
        }
    }
}
