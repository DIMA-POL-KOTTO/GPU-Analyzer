using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;


namespace GPU_Analyzer.ViewModels.Converters
{
    public class GraphConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var history = values[0] as IList<float>;
            if (history == null || history.Count == 0)
            {
                return new PointCollection();
            }

            double width = (double)values[1]; // ActualWidth из Canvas
            double height = (double)values[2]; // ActualHeight из Canvas

            bool fixed100 = parameter?.ToString() == "Fixed100";

            float max = fixed100 ? 100 : history.Max();
            float min = fixed100 ? 0 : history.Min();

            if (max - min < 1)
            {
                max = min + 1;
            }
            var points = new PointCollection();
            if (history.Count == 1)
            {
                points.Add(new Point(0, height - height / 2));
                return points;
            }
            double step = width/(history.Count-1);
            for (int i = 0; i < history.Count; i++)
            {
                double x = i * step;
                double norm = (history[i] - min) / (max - min);
                double y = height - norm * height;
                points.Add(new Point(x, y));
            }
            return points;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
