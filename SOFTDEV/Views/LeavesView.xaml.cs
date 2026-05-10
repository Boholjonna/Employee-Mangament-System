using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SOFTDEV.Views
{
    /// <summary>
    /// Leaves view — shows leave requests table and summary stat cards.
    /// </summary>
    public partial class LeavesView : UserControl
    {
        private readonly string _username;

        public LeavesView(string username = "Admin")
        {
            _username = username;
            InitializeComponent();
            GreetingLabel.Text = $"Hello {username}! 👋";
            LoadData();
        }

        // ── Data loading ──────────────────────────────────────────────

        private void LoadData()
        {
            var rows = BuildLeaveRows();

            // Stat counts
            int pending  = 0;
            int approved = 0;
            int onLeave  = 0;

            foreach (var r in rows)
            {
                string s = r.Status?.ToLower() ?? "";
                if (s == "pending")  pending++;
                if (s == "approved") approved++;
                if (s == "on leave" || s == "approved") onLeave++;
            }

            PendingLabel.Text  = $"Pending Request: {pending}";
            ApprovedLabel.Text = $"Approved Leaves: {approved}";
            OnLeaveLabel.Text  = $"Employees on Leave: {onLeave}";

            if (rows.Count == 0)
            {
                EmptyLabel.Visibility  = Visibility.Visible;
                LeavesTable.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyLabel.Visibility  = Visibility.Collapsed;
                LeavesTable.Visibility = Visibility.Visible;
                LeavesTable.ItemsSource = rows;
            }
        }

        /// <summary>
        /// Tries to pull leave data from the DB.
        /// Falls back to placeholder rows when the table doesn't exist yet.
        /// </summary>
        private List<LeaveItem> BuildLeaveRows()
        {
            try
            {
                var dbRows = DatabaseHelper.GetAllLeaveRequests();
                if (dbRows.Count > 0)
                    return dbRows;
            }
            catch { /* DB table not yet created — use fallback */ }

            return FallbackRows();
        }

        private static List<LeaveItem> FallbackRows() => new()
        {
            new LeaveItem
            {
                EmployeeName = "Alice Santos",
                Department   = "Engineering",
                LeaveType    = "Sick Leave",
                StartDate    = "May 10, 2026",
                EndDate      = "May 12, 2026",
                Status       = "Approved",
                StatusColor  = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
            },
            new LeaveItem
            {
                EmployeeName = "Bob Reyes",
                Department   = "Management",
                LeaveType    = "Vacation",
                StartDate    = "May 15, 2026",
                EndDate      = "May 20, 2026",
                Status       = "Pending",
                StatusColor  = new SolidColorBrush(Color.FromRgb(243, 156, 18)),
            },
            new LeaveItem
            {
                EmployeeName = "Carol Lim",
                Department   = "QA",
                LeaveType    = "Emergency",
                StartDate    = "May 8, 2026",
                EndDate      = "May 9, 2026",
                Status       = "Approved",
                StatusColor  = new SolidColorBrush(Color.FromRgb(46, 204, 113)),
            },
            new LeaveItem
            {
                EmployeeName = "David Cruz",
                Department   = "Design",
                LeaveType    = "Vacation",
                StartDate    = "Jun 1, 2026",
                EndDate      = "Jun 5, 2026",
                Status       = "Pending",
                StatusColor  = new SolidColorBrush(Color.FromRgb(243, 156, 18)),
            },
            new LeaveItem
            {
                EmployeeName = "Eva Mendoza",
                Department   = "DevOps",
                LeaveType    = "Sick Leave",
                StartDate    = "May 5, 2026",
                EndDate      = "May 6, 2026",
                Status       = "Rejected",
                StatusColor  = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
            },
        };

        // ── Event handlers ────────────────────────────────────────────

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }

    /// <summary>One row in the Leaves table.</summary>
    public class LeaveItem
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string Department   { get; set; } = string.Empty;
        public string LeaveType    { get; set; } = string.Empty;
        public string StartDate    { get; set; } = string.Empty;
        public string EndDate      { get; set; } = string.Empty;
        public string Status       { get; set; } = string.Empty;
        public Brush  StatusColor  { get; set; } = new SolidColorBrush(Color.FromRgb(123, 97, 255));
    }
}
