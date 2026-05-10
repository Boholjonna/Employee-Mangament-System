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
            Loaded += (s, e) => DrawCharts();
        }

        private void DrawCharts()
        {
            DrawPerformanceTrend();
            DrawKPIBarChart();

            // Set the rounded progress fills for team comparison
            try
            {
                double yourPercent = 84.0;
                double teamPercent = 76.0;
                double containerWidth = YourScoreBar?.ActualWidth > 0 ? YourScoreBar.ActualWidth : 200;
                if (YourScoreFill != null) YourScoreFill.Width = containerWidth * (yourPercent / 100.0);
                if (TeamAvgFill != null) TeamAvgFill.Width = containerWidth * (teamPercent / 100.0);
            }
            catch { }
        }

        private void DrawPerformanceTrend()
        {
            PerformanceTrendChart.Children.Clear();
            double width = PerformanceTrendChart.ActualWidth > 0 ? PerformanceTrendChart.ActualWidth : 700;
            double height = PerformanceTrendChart.ActualHeight > 0 ? PerformanceTrendChart.ActualHeight : 250;
            double[] data = { 3.5, 3.7, 3.9, 4.0, 4.1, 4.2 }; // Monthly scores
            double bottomLabelSpace = 26;
            double chartHeight = Math.Max(1, height - bottomLabelSpace);

            // Draw grid
            for (int i = 0; i <= 5; i++)
            {
                double y = chartHeight - (i * chartHeight / 5);
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
                double y1 = chartHeight - ((data[i] / 5.0) * chartHeight);
                double x2 = (i + 1) * segmentWidth;
                double y2 = chartHeight - ((data[i + 1] / 5.0) * chartHeight);

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
            Canvas.SetTop(lastPoint, chartHeight - ((data[data.Length - 1] / 5.0) * chartHeight) - 5);
            PerformanceTrendChart.Children.Add(lastPoint);

            // Draw month labels for the last 6 months (up to current month)
            DateTime startMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-(data.Length - 1));
            for (int i = 0; i < data.Length; i++)
            {
                TextBlock monthLabel = new TextBlock
                {
                    Text = startMonth.AddMonths(i).ToString("MMM"),
                    Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 170)),
                    FontSize = 11,
                    FontWeight = FontWeights.Medium
                };

                monthLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double labelX = (i * segmentWidth) - (monthLabel.DesiredSize.Width / 2);
                labelX = Math.Max(0, Math.Min(width - monthLabel.DesiredSize.Width, labelX));

                Canvas.SetLeft(monthLabel, labelX);
                Canvas.SetTop(monthLabel, chartHeight + 6);
                PerformanceTrendChart.Children.Add(monthLabel);
            }
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
