using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SOFTDEV.Views
{
    public partial class PerformanceView : UserControl
    {
        public PerformanceView()
        {
            InitializeComponent();
            DrawCharts();
        }

        private void DrawCharts()
        {
            DrawPerformanceTrend();
            DrawKPIBarChart();
        }

        private void DrawPerformanceTrend()
        {
            PerformanceTrendChart.Children.Clear();

            double width = 700;
            double height = 250;
            double[] data = { 3.5, 3.7, 3.9, 4.0, 4.1, 4.2 }; // Monthly scores

            // Draw grid
            for (int i = 0; i <= 5; i++)
            {
                double y = height - (i * height / 5);
                Line gridLine = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = width,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromRgb(42, 42, 62)),
                    StrokeThickness = 1
                };
                PerformanceTrendChart.Children.Add(gridLine);
            }

            // Draw line
            double segmentWidth = width / (data.Length - 1);
            for (int i = 0; i < data.Length - 1; i++)
            {
                double x1 = i * segmentWidth;
                double y1 = height - ((data[i] / 5.0) * height);
                double x2 = (i + 1) * segmentWidth;
                double y2 = height - ((data[i + 1] / 5.0) * height);

                Line line = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = new SolidColorBrush(Color.FromRgb(123, 97, 255)),
                    StrokeThickness = 3
                };
                PerformanceTrendChart.Children.Add(line);

                Ellipse point = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = new SolidColorBrush(Color.FromRgb(123, 97, 255))
                };
                Canvas.SetLeft(point, x1 - 5);
                Canvas.SetTop(point, y1 - 5);
                PerformanceTrendChart.Children.Add(point);
            }

            // Last point
            Ellipse lastPoint = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(Color.FromRgb(123, 97, 255))
            };
            Canvas.SetLeft(lastPoint, width - 5);
            Canvas.SetTop(lastPoint, height - ((data[data.Length - 1] / 5.0) * height) - 5);
            PerformanceTrendChart.Children.Add(lastPoint);
        }

        private void DrawKPIBarChart()
        {
            KPIBarChart.Children.Clear();

            double width = 380;
            double height = 300;
            string[] labels = { "Task Completion", "Attendance", "Quality", "Teamwork", "Innovation" };
            double[] values = { 87, 96, 82, 90, 78 };
            Color[] colors = {
                Color.FromRgb(123, 97, 255),
                Color.FromRgb(76, 175, 80),
                Color.FromRgb(255, 152, 0),
                Color.FromRgb(33, 150, 243),
                Color.FromRgb(156, 39, 176)
            };

            double barHeight = 40;
            double spacing = 15;

            for (int i = 0; i < labels.Length; i++)
            {
                double y = i * (barHeight + spacing);
                double barWidth = (values[i] / 100.0) * width;

                // Draw bar
                Rectangle bar = new Rectangle
                {
                    Width = barWidth,
                    Height = barHeight,
                    Fill = new SolidColorBrush(colors[i]),
                    RadiusX = 4,
                    RadiusY = 4
                };
                Canvas.SetLeft(bar, 0);
                Canvas.SetTop(bar, y);
                KPIBarChart.Children.Add(bar);

                // Draw label
                TextBlock label = new TextBlock
                {
                    Text = labels[i],
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold
                };
                Canvas.SetLeft(label, 8);
                Canvas.SetTop(label, y + 5);
                KPIBarChart.Children.Add(label);

                // Draw value
                TextBlock value = new TextBlock
                {
                    Text = $"{values[i]}%",
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 11,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(value, 8);
                Canvas.SetTop(value, y + 22);
                KPIBarChart.Children.Add(value);
            }
        }

        private void DownloadReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Performance report download coming soon!\n\nAvailable formats:\n• PDF\n• Excel\n• CSV", 
                "Download Report", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
