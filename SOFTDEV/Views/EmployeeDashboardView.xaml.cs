using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using SOFTDEV;

namespace SOFTDEV.Views
{
    public partial class EmployeeDashboardView : UserControl
    {
        public EmployeeDashboardView()
        {
            InitializeComponent();
            Loaded += EmployeeDashboardView_Loaded;
        }

        private void EmployeeDashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize sample data
            NotificationsControl.ItemsSource = new List<NotificationItem>
            {
                new NotificationItem
                {
                    Title = "Leave Request Approved",
                    Message = "Your leave request for June 10-12 has been approved.",
                    Time = "2 hours ago"
                },
                new NotificationItem
                {
                    Title = "New Task Assigned",
                    Message = "You have been assigned to 'Client Presentation' task.",
                    Time = "5 hours ago"
                },
                new NotificationItem
                {
                    Title = "Performance Review Scheduled",
                    Message = "Your quarterly review is scheduled for May 30.",
                    Time = "1 day ago"
                }
            };

            UpcomingEventsControl.ItemsSource = new List<UpcomingEvent>
            {
                new UpcomingEvent { EventName = "Team Building Activity", EventDate = "May 25, 2026" },
                new UpcomingEvent { EventName = "Performance Review", EventDate = "May 30, 2026" },
                new UpcomingEvent { EventName = "Company Town Hall", EventDate = "June 1, 2026" }
            };

            // Draw charts
            DrawProgressChart();
            DrawAttendanceChart();
        }

        private void DrawProgressChart()
        {
            ProgressChartCanvas.Children.Clear();

            double width = ProgressChartCanvas.ActualWidth > 0 ? ProgressChartCanvas.ActualWidth : 500;
            double height = 200;
            double[] data = { 45, 52, 68, 75, 85 };

            // Draw grid lines
            for (int i = 0; i <= 4; i++)
            {
                double y = height - (i * height / 4);
                Line gridLine = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = width,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromRgb(42, 42, 62)),
                    StrokeThickness = 1
                };
                ProgressChartCanvas.Children.Add(gridLine);
            }

            // Draw line chart
            double segmentWidth = width / (data.Length - 1);
            for (int i = 0; i < data.Length - 1; i++)
            {
                double x1 = i * segmentWidth;
                double y1 = height - (data[i] / 100.0 * height);
                double x2 = (i + 1) * segmentWidth;
                double y2 = height - (data[i + 1] / 100.0 * height);

                Line line = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = new SolidColorBrush(Color.FromRgb(123, 97, 255)),
                    StrokeThickness = 3
                };
                ProgressChartCanvas.Children.Add(line);

                Ellipse point = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = new SolidColorBrush(Color.FromRgb(123, 97, 255))
                };
                Canvas.SetLeft(point, x1 - 4);
                Canvas.SetTop(point, y1 - 4);
                ProgressChartCanvas.Children.Add(point);
            }

            Ellipse lastPoint = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Color.FromRgb(123, 97, 255))
            };
            Canvas.SetLeft(lastPoint, width - 4);
            Canvas.SetTop(lastPoint, height - (data[data.Length - 1] / 100.0 * height) - 4);
            ProgressChartCanvas.Children.Add(lastPoint);
        }

        private void DrawAttendanceChart()
        {
            AttendanceChartCanvas.Children.Clear();

            double width = AttendanceChartCanvas.ActualWidth > 0 ? AttendanceChartCanvas.ActualWidth : 500;
            double height = 200;
            double[] data = { 1, 1, 1, 0.5, 1, 1, 1 };
            string[] labels = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

            double barWidth = width / data.Length * 0.7;
            double spacing = width / data.Length;

            for (int i = 0; i < data.Length; i++)
            {
                double barHeight = data[i] * height * 0.8;
                double x = i * spacing + (spacing - barWidth) / 2;
                double y = height - barHeight;

                Rectangle bar = new Rectangle
                {
                    Width = barWidth,
                    Height = barHeight,
                    Fill = data[i] == 1
                        ? new SolidColorBrush(Color.FromRgb(123, 97, 255))
                        : data[i] == 0.5
                        ? new SolidColorBrush(Color.FromRgb(255, 152, 0))
                        : new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                    RadiusX = 4,
                    RadiusY = 4
                };
                Canvas.SetLeft(bar, x);
                Canvas.SetTop(bar, y);
                AttendanceChartCanvas.Children.Add(bar);

                TextBlock label = new TextBlock
                {
                    Text = labels[i],
                    Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 170)),
                    FontSize = 10
                };
                Canvas.SetLeft(label, x + barWidth / 2 - 10);
                Canvas.SetTop(label, height + 5);
                AttendanceChartCanvas.Children.Add(label);
            }
        }
    }
}
