using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AttendanceDevice.Config_Class
{
    internal static class InstitutionNameFontHelper
    {
        public static void ApplyAndFit(TextBlock textBlock, string institutionName, double maxWidth, double maxHeight)
        {
            if (textBlock == null)
                return;

            var name = institutionName?.Trim() ?? string.Empty;
            ApplyStyles(textBlock, name);
            textBlock.Text = name;
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.TextWrapping = TextWrapping.NoWrap;
            textBlock.MaxWidth = double.PositiveInfinity;

            if (maxWidth <= 0 || string.IsNullOrEmpty(name))
                return;

            const double maxFont = 36;
            const double minFont = 14;
            var bestSize = minFont;

            for (var fontSize = maxFont; fontSize >= minFont; fontSize -= 0.5)
            {
                textBlock.FontSize = fontSize;
                textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                if (textBlock.DesiredSize.Width <= maxWidth)
                {
                    bestSize = fontSize;
                    break;
                }
            }

            textBlock.FontSize = bestSize;
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            if (maxHeight <= 0 || textBlock.DesiredSize.Height <= maxHeight)
                return;

            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.MaxWidth = maxWidth;

            for (var fontSize = bestSize; fontSize >= minFont; fontSize -= 0.5)
            {
                textBlock.FontSize = fontSize;
                textBlock.Measure(new Size(maxWidth, maxHeight));
                if (textBlock.DesiredSize.Height <= maxHeight)
                    return;
            }

            textBlock.FontSize = minFont;
        }

        private static void ApplyStyles(TextBlock textBlock, string institutionName)
        {
            var isBengali = ScheduleDisplayHelper.ContainsBengaliScript(institutionName);
            textBlock.Foreground = new SolidColorBrush(Color.FromRgb(0, 31, 92));
            textBlock.FontFamily = isBengali
                ? new FontFamily("Nirmala UI, Vrinda, Kalpurush, Segoe UI")
                : new FontFamily("Segoe UI, Cambria, Georgia");
            textBlock.FontWeight = isBengali ? FontWeights.Bold : FontWeights.SemiBold;
        }
    }
}
