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
        private bool _isClockedIn = false;
        private string _employeeName = string.Empty;
        private string _clockInTime  = string.Empty;
        private List<AttendanceRecord> _attendanceHistory = new();

        public AttendanceView() : this(string.Empty) { }

        public AttendanceView(string employeeName)
        {
            InitializeComponent();
            _employeeName = employeeName;
            InitializeTimer();
            LoadAttendanceHistory();
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

        private void LoadAttendanceHistory()
        {
            _attendanceHistory.Clear();

            if (!string.IsNullOrEmpty(_employeeName))
            {
                try
                {
                    var records = DatabaseHelper.GetAttendanceByEmployee(_employeeName);
                    foreach (var r in records)
                    {
                        _attendanceHistory.Add(new AttendanceRecord
                        {
                            Date      = r.Date,
                            ClockIn   = r.TimeIn,
                            ClockOut  = string.IsNullOrEmpty(r.TimeOut) ? "In Progress" : r.TimeOut,
                            Hours     = string.IsNullOrEmpty(r.TotalHours) ? "-" : r.TotalHours,
                            Status    = r.Status,
                            StatusColor = r.StatusColor,
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AttendanceView] LoadAttendanceHistory error: {ex.Message}");
                }
            }

            // Fall back to sample data if nothing loaded
            if (_attendanceHistory.Count == 0)
            {
                _attendanceHistory.Add(new AttendanceRecord { Date = "No records yet", ClockIn = "-", ClockOut = "-", Hours = "-", Status = "-", StatusColor = "#aaaaaa" });
            }

            AttendanceHistoryControl.ItemsSource = null;
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
            DateTime now        = DateTime.Now;
            string   dbTime     = now.ToString("HH:mm:ss");   // 24-hour for MySQL TIME column
            string   displayTime = now.ToString("hh:mm tt");  // 12-hour for UI display

            if (!_isClockedIn)
            {
                // ── Clock IN ──────────────────────────────────────────
                _clockInTime = dbTime;
                _isClockedIn = true;

                ClockInOutButton.Content = "🕐 Clock Out";
                ClockStatusText.Text = "Clocked In";
                ClockStatusText.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                ClockInTimeText.Text = displayTime;

                if (!string.IsNullOrEmpty(_employeeName))
                {
                    try
                    {
                        bool ok = DatabaseHelper.RecordAttendance(_employeeName, dbTime, isClockingIn: true);
                        if (!ok)
                            MessageBox.Show("Could not save clock-in to database.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Clock-in DB error:\n{ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                MessageBox.Show($"Clocked in at {displayTime}", "Clock In", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // ── Clock OUT ─────────────────────────────────────────
                _isClockedIn = false;

                ClockInOutButton.Content = "🕐 Clock In";
                ClockStatusText.Text = "Clocked Out";
                ClockStatusText.Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 170));

                if (!string.IsNullOrEmpty(_employeeName))
                {
                    try
                    {
                        bool ok = DatabaseHelper.RecordAttendance(_employeeName, dbTime, isClockingIn: false);
                        if (!ok)
                            MessageBox.Show("Could not save clock-out to database.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Clock-out DB error:\n{ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                MessageBox.Show($"Clocked out at {displayTime}", "Clock Out", MessageBoxButton.OK, MessageBoxImage.Information);

                // Refresh the history list to show the completed record
                LoadAttendanceHistory();
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
