using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SOFTDEV.Views
{
    /// <summary>
    /// Reports view — shows the Employee Performance table.
    /// </summary>
    public partial class ReportsView : UserControl
    {
        private readonly string _username;

        /// <summary>Called when the user clicks the Back button. Wire this up before showing the view.</summary>
        public Action? OnBack { get; set; }

        public ReportsView(string username = "Admin")
        {
            _username = username;
            InitializeComponent();
            GreetingLabel.Text = $"Hello {username}! 👋";
            LoadPerformanceData();
        }

        // ── Data loading ──────────────────────────────────────────────

        private void LoadPerformanceData()
        {
            var rows = BuildPerformanceRows();

            if (rows.Count == 0)
            {
                EmptyLabel.Visibility    = Visibility.Visible;
                PerformanceTable.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyLabel.Visibility    = Visibility.Collapsed;
                PerformanceTable.Visibility = Visibility.Visible;
                PerformanceTable.ItemsSource = rows;
            }
        }

        /// <summary>
        /// Builds performance rows by joining employee, attendance, and task data.
        /// Falls back to placeholder rows when the database is unavailable.
        /// </summary>
        private List<PerformanceReportItem> BuildPerformanceRows()
        {
            var result = new List<PerformanceReportItem>();

            try
            {
                // Pull employees from DB
                var employees = DatabaseHelper.GetAllEmployees();
                if (employees.Count == 0)
                    return FallbackRows();

                foreach (var emp in employees)
                {
                    // Attendance %
                    var attendanceRecords = DatabaseHelper.GetAttendanceByEmployee(emp.Name);
                    int total   = attendanceRecords.Count;
                    int present = 0;
                    foreach (var r in attendanceRecords)
                    {
                        string s = r.Status?.ToLower() ?? "";
                        if (s == "present" || s == "late")
                            present++;
                    }
                    string attendancePct = total > 0
                        ? $"{(present * 100 / total)}%"
                        : "—";

                    // Task summary
                    var tasks       = DatabaseHelper.GetTasksByEmployee(emp.Name);
                    int taskCount   = tasks.Count;
                    int completed   = 0;
                    foreach (var t in tasks)
                        if (t.IsCompleted) completed++;

                    string taskSummary = taskCount > 0
                        ? $"{completed}/{taskCount} done"
                        : "No tasks";

                    // Performance score (simple formula: avg of attendance % and task completion %)
                    double attScore  = total > 0 ? (double)present / total * 100 : 0;
                    double taskScore = taskCount > 0 ? (double)completed / taskCount * 100 : 0;
                    double score     = total > 0 || taskCount > 0
                        ? (attScore + taskScore) / 2
                        : 0;

                    string scoreStr = total > 0 || taskCount > 0
                        ? $"{score:F0} / 100"
                        : "—";

                    // Status
                    string status;
                    string statusColor;
                    if (score >= 80)      { status = "Excellent"; statusColor = "#2ecc71"; }
                    else if (score >= 60) { status = "Good";      statusColor = "#7b61ff"; }
                    else if (score >= 40) { status = "Average";   statusColor = "#f39c12"; }
                    else                  { status = "Poor";      statusColor = "#e74c3c"; }

                    result.Add(new PerformanceReportItem
                    {
                        EmployeeName     = emp.Name,
                        Department       = emp.Position,   // position used as department
                        TaskSummary      = taskSummary,
                        AttendancePercent = attendancePct,
                        PerformanceScore = scoreStr,
                        Status           = status,
                        StatusColor      = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString(statusColor)),
                    });
                }
            }
            catch
            {
                return FallbackRows();
            }

            return result.Count > 0 ? result : FallbackRows();
        }

        /// <summary>Placeholder rows shown when the database is unavailable.</summary>
        private static List<PerformanceReportItem> FallbackRows()
        {
            return new List<PerformanceReportItem>
            {
                new PerformanceReportItem
                {
                    EmployeeName      = "Alice Santos",
                    Department        = "Engineering",
                    TaskSummary       = "8/10 done",
                    AttendancePercent = "95%",
                    PerformanceScore  = "90 / 100",
                    Status            = "Excellent",
                    StatusColor       = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                },
                new PerformanceReportItem
                {
                    EmployeeName      = "Bob Reyes",
                    Department        = "Management",
                    TaskSummary       = "5/8 done",
                    AttendancePercent = "88%",
                    PerformanceScore  = "75 / 100",
                    Status            = "Good",
                    StatusColor       = new SolidColorBrush(Color.FromRgb(123, 97, 255)),
                },
                new PerformanceReportItem
                {
                    EmployeeName      = "Carol Lim",
                    Department        = "QA",
                    TaskSummary       = "3/7 done",
                    AttendancePercent = "72%",
                    PerformanceScore  = "58 / 100",
                    Status            = "Average",
                    StatusColor       = new SolidColorBrush(Color.FromRgb(243, 156, 18)),
                },
                new PerformanceReportItem
                {
                    EmployeeName      = "David Cruz",
                    Department        = "Design",
                    TaskSummary       = "6/6 done",
                    AttendancePercent = "100%",
                    PerformanceScore  = "98 / 100",
                    Status            = "Excellent",
                    StatusColor       = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                },
                new PerformanceReportItem
                {
                    EmployeeName      = "Eva Mendoza",
                    Department        = "DevOps",
                    TaskSummary       = "2/9 done",
                    AttendancePercent = "60%",
                    PerformanceScore  = "35 / 100",
                    Status            = "Poor",
                    StatusColor       = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                },
            };
        }

        // ── Event handlers ────────────────────────────────────────────

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            OnBack?.Invoke();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPerformanceData();
        }
    }

    /// <summary>
    /// One row in the Employee Performance table.
    /// </summary>
    public class PerformanceReportItem
    {
        public string EmployeeName      { get; set; } = string.Empty;
        public string Department        { get; set; } = string.Empty;
        public string TaskSummary       { get; set; } = string.Empty;
        public string AttendancePercent { get; set; } = string.Empty;
        public string PerformanceScore  { get; set; } = string.Empty;
        public string Status            { get; set; } = string.Empty;
        public Brush  StatusColor       { get; set; } = new SolidColorBrush(Color.FromRgb(123, 97, 255));
    }
}
