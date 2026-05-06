using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SOFTDEV
{
    /// <summary>
    /// Admin Dashboard window — code-behind.
    /// </summary>
    public partial class AdminDashboard : Window
    {
        // ── Private fields ────────────────────────────────────────────
        private string _username;

        // ── Private calendar state fields ─────────────────────────────
        private int _calendarYear  = DateTime.Today.Year;
        private int _calendarMonth = DateTime.Today.Month;
        private int _highlightDay  = DateTime.Today.Day;
        private int _selectedDay   = 0;   // 0 = no user selection yet
        private List<CalendarDayItem> _calendarDays = new();

        // ── Public properties ─────────────────────────────────────────
        /// <summary>Gets or sets the list of employees shown in the employee list panel.</summary>
        public List<EmployeeEntry> Employees { get; set; } = new();

        /// <summary>Gets or sets the list of calendar day cells for the current month view.</summary>
        public List<CalendarDayItem> CalendarDays { get; set; } = new();

        // ── Constructor ───────────────────────────────────────────────
        public AdminDashboard(string username)
        {
            _username = username;

            InitializeComponent();

            GreetingText.Text        = $"Hello, {username}!";
            AttendanceDateLabel.Text = DateTime.Today.ToString("MMMM dd, yyyy");

            LoadEmployees();
            LoadEmployeeCount();

            RefreshCalendar();
        }

        // ── Employee list ─────────────────────────────────────────────

        /// <summary>Fetches all employees from the database and binds them to the list.
        /// Falls back to placeholder entries when the database is unavailable.</summary>
        private void LoadEmployees()
        {
            Employees = DatabaseHelper.GetAllEmployees();

            if (Employees.Count == 0)
            {
                Employees = new List<EmployeeEntry>
                {
                    new EmployeeEntry("Alice Santos",   "Software Engineer"),
                    new EmployeeEntry("Bob Reyes",      "Project Manager"),
                    new EmployeeEntry("Carol Lim",      "QA Analyst"),
                    new EmployeeEntry("David Cruz",     "UI/UX Designer"),
                    new EmployeeEntry("Eva Mendoza",    "DevOps Engineer"),
                };
            }

            EmployeeListControl.ItemsSource = null;
            EmployeeListControl.ItemsSource = Employees;
        }

        /// <summary>Queries the employee count from the DB and updates the stat card.</summary>
        private void LoadEmployeeCount()
        {
            int count = DatabaseHelper.GetEmployeeCount();
            EmployeeCountLabel.Text = count >= 0 ? count.ToString() : "—";
        }

        // ── Calendar generation ───────────────────────────────────────

        /// <summary>
        /// Generates the list of <see cref="CalendarDayItem"/> objects for the given month.
        /// </summary>
        internal List<CalendarDayItem> GenerateCalendarDays(int year, int month, int highlightDay, int selectedDay = 0)
        {
            if (year < 1)
                throw new ArgumentOutOfRangeException(nameof(year), year, "Year must be greater than or equal to 1.");
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), month, "Month must be between 1 and 12.");

            int daysInMonth = DateTime.DaysInMonth(year, month);
            highlightDay = Math.Max(0, Math.Min(highlightDay, daysInMonth));

            var days = new List<CalendarDayItem>();

            int startOffset = (int)new DateTime(year, month, 1).DayOfWeek;
            for (int i = 0; i < startOffset; i++)
                days.Add(new CalendarDayItem(0, false, false, false));

            for (int d = 1; d <= daysInMonth; d++)
                days.Add(new CalendarDayItem(d, true, d == highlightDay, d == selectedDay));

            return days;
        }

        /// <summary>Regenerates the calendar and pushes it to the UI.</summary>
        private void RefreshCalendar()
        {
            CalendarDays = GenerateCalendarDays(_calendarYear, _calendarMonth, _highlightDay, _selectedDay);
            _calendarDays = CalendarDays;
            CalendarDaysControl.ItemsSource = CalendarDays;
            CalendarMonthLabel.Content = $"{new DateTime(_calendarYear, _calendarMonth, 1):MMMM yyyy}";
        }

        // ── Control Group handlers ────────────────────────────────────
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(SearchButton_Click));
        }

        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(NotificationButton_Click));
        }

        private void UserNameButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(UserNameButton_Click));
        }

        private void AvatarButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(AvatarButton_Click));
        }

        // ── Navigation Bar handler ────────────────────────────────────
        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == OverviewButton)
            {
                var overviewUI = new AdminOverviewUI(_username, this);
                this.Hide();
                overviewUI.Show();
            }
            else if (sender == EmployeesButton)
            {
                var employeesView = new AdminEmployeesView(_username, this);
                this.Hide();
                employeesView.Show();
            }
            else if (sender == AttendanceButton)
            {
                var attendanceDashboard = new AttendanceDashboard(_username);
                this.Hide();
                attendanceDashboard.Show();
            }
            System.Diagnostics.Debug.WriteLine(nameof(NavButton_Click));
        }

        // ── Left Column handlers ──────────────────────────────────────
        private void StatCardRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadEmployeeCount();
        }

        private void ClockInOut_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ClockInTimePickerDialog { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                ClockInTimeLabel.Text   = dialog.SelectedTime;
                ClockInStatusLabel.Text = "Clocked In";
            }
        }

        private void LunchBreak_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(LunchBreak_Click));
        }

        // ── Calendar — month label click (year picker) ────────────────
        private void CalendarMonthLabel_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CalendarYearPickerDialog(_calendarYear) { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                _calendarYear = dialog.SelectedYear;
                _selectedDay  = 0;   // clear day selection when year changes
                RefreshCalendar();
            }
        }

        // ── Calendar — day cell click ─────────────────────────────────
        private void CalendarDay_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int day && day > 0)
            {
                _selectedDay = day;
                RefreshCalendar();

                // Update the attendance date label to reflect the chosen date
                var chosen = new DateTime(_calendarYear, _calendarMonth, day);
                AttendanceDateLabel.Text = chosen.ToString("MMMM dd, yyyy");
            }
        }

        // ── Center Column (Calendar) nav handlers ────────────────────
        private void PrevMonth_Click(object sender, RoutedEventArgs e)
        {
            if (_calendarMonth == 1) { _calendarMonth = 12; _calendarYear--; }
            else                     { _calendarMonth--; }
            _selectedDay = 0;
            RefreshCalendar();
        }

        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            if (_calendarMonth == 12) { _calendarMonth = 1; _calendarYear++; }
            else                      { _calendarMonth++; }
            _selectedDay = 0;
            RefreshCalendar();
        }

        // ── Right Column (Employee List) handlers ─────────────────────
        private void EmployeeListRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadEmployees();
        }
    }
}
