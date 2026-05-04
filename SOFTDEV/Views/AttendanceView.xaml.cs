using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SOFTDEV.Views
{
    public partial class AttendanceView : UserControl
    {
        private DispatcherTimer _timer;
        private bool _isClockedIn = true;
        private List<AttendanceRecord> _attendanceHistory = new();

        public AttendanceView()
        {
            InitializeComponent();
            InitializeTimer();
            InitializeSampleData();
            DrawAttendanceChart();
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CurrentTimeText.Text = DateTime.Now.ToString("hh:mm:ss tt");
            CurrentDateText.Text = DateTime.Now.ToString("dddd, MMMM d, yyyy");
        }

        private void InitializeSampleData()
        {
            _attendanceHistory = new List<AttendanceRecord>
            {
                new AttendanceRecord { Date = "May 4, 2026", ClockIn = "08:30 AM", ClockOut = "In Progress", Hours = "-", Status = "Present", StatusColor = "#4caf50" },
                new AttendanceRecord { Date = "May 3, 2026", ClockIn = "08:45 AM", ClockOut = "05:15 PM", Hours = "8.5", Status = "Present", StatusColor = "#4caf50" },
                new AttendanceRecord { Date = "May 2, 2026", ClockIn = "09:15 AM", ClockOut = "05:30 PM", Hours = "8.25", Status = "Late", StatusColor = "#ff9800" },
                new AttendanceRecord { Date = "May 1, 2026", ClockIn = "08:30 AM", ClockOut = "05:00 PM", Hours = "8.5", Status = "Present", StatusColor = "#4caf50" },
                new AttendanceRecord { Date = "Apr 30, 2026", ClockIn = "-", ClockOut = "-", Hours = "-", Status = "Absent", StatusColor = "#f44336" },
                new AttendanceRecord { Date = "Apr 29, 2026", ClockIn = "08:25 AM", ClockOut = "05:10 PM", Hours = "8.75", Status = "Present", StatusColor = "#4caf50" },
                new AttendanceRecord { Date = "Apr 28, 2026", ClockIn = "08:35 AM", ClockOut = "05:05 PM", Hours = "8.5", Status = "Present", StatusColor = "#4caf50" },
                new AttendanceRecord { Date = "Apr 27, 2026", ClockIn = "09:05 AM", ClockOut = "05:20 PM", Hours = "8.25", Status = "Late", StatusColor = "#ff9800" }
            };

            AttendanceHistoryControl.ItemsSource = _attendanceHistory;
        }

        private void DrawAttendanceChart()
        {
            AttendanceTrendChart.Children.Clear();

            double width = 380;
            double height = 180;
            double[] data = { 1, 1, 1, 0.5, 1, 1, 0, 1, 1, 1, 1, 1, 0.5, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0.5, 1 };

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
                AttendanceTrendChart.Children.Add(gridLine);
            }

            // Draw line chart
            double segmentWidth = width / (data.Length - 1);
            for (int i = 0; i < data.Length - 1; i++)
            {
                double x1 = i * segmentWidth;
                double y1 = height - (data[i] * height * 0.9);
                double x2 = (i + 1) * segmentWidth;
                double y2 = height - (data[i + 1] * height * 0.9);

                Line line = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = new SolidColorBrush(Color.FromRgb(123, 97, 255)),
                    StrokeThickness = 2
                };
                AttendanceTrendChart.Children.Add(line);
            }
        }

        private void ClockInOut_Click(object sender, RoutedEventArgs e)
        {
            _isClockedIn = !_isClockedIn;

            if (_isClockedIn)
            {
                ClockInOutButton.Content = "🕐 Clock Out";
                ClockStatusText.Text = "Clocked In";
                ClockStatusText.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                ClockInTimeText.Text = DateTime.Now.ToString("hh:mm tt");
                MessageBox.Show($"Clocked in at {DateTime.Now:hh:mm tt}", "Clock In", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                ClockInOutButton.Content = "🕐 Clock In";
                ClockStatusText.Text = "Clocked Out";
                ClockStatusText.Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 170));
                MessageBox.Show($"Clocked out at {DateTime.Now:hh:mm tt}", "Clock Out", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Break_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Break time recorded!", "Break", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MonthFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Filter attendance history by month
        }

        private void SubmitLeaveRequest_Click(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please select start and end dates.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(LeaveReasonTextBox.Text))
            {
                MessageBox.Show("Please provide a reason for leave.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Leave request submitted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Clear form
            LeaveTypeComboBox.SelectedIndex = 0;
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            LeaveReasonTextBox.Clear();
        }
    }

    public class AttendanceRecord
    {
        public string Date { get; set; } = string.Empty;
        public string ClockIn { get; set; } = string.Empty;
        public string ClockOut { get; set; } = string.Empty;
        public string Hours { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
    }
}
