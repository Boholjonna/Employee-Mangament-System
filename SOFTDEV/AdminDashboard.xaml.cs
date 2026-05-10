using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SOFTDEV.ViewModels;
using SOFTDEV.Views;
using System.Linq;

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

        // ── Live clock timer ──────────────────────────────────────────
        private DispatcherTimer _clockTimer;

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
            StartLiveClock();

            // Highlight Overview as the active nav button on load
            SetActiveNavButton(OverviewButton);
        }

        // ── Nav button highlight helper ───────────────────────────────

        /// <summary>Sets the active nav button to darker purple; all others to lighter purple.</summary>
        private void SetActiveNavButton(Button active)
        {
            var all = new[] { OverviewButton, EmployeesButton, AttendanceButton, ToDoButton, ReportsButton, LeavesButton, SettingsButton };
            foreach (var btn in all)
                btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a294f9"));
            active.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5e4eb7"));
        }

        // ── Live clock ────────────────────────────────────────────────

        /// <summary>Starts a DispatcherTimer that updates the clock label every second.</summary>
        private void StartLiveClock()
        {
            // Set initial value immediately so there is no blank flash
            LiveClockLabel.Text = DateTime.Now.ToString("hh:mm:ss tt");

            _clockTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _clockTimer.Tick += (_, _) =>
            {
                LiveClockLabel.Text = DateTime.Now.ToString("hh:mm:ss tt");

                // Also keep the today-highlight correct when the date rolls over midnight
                int todayDay = DateTime.Today.Day;
                if (_highlightDay != todayDay && _calendarYear == DateTime.Today.Year && _calendarMonth == DateTime.Today.Month)
                {
                    _highlightDay = todayDay;
                    RefreshCalendar();
                }
            };
            _clockTimer.Start();
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

        /// <summary>Navigates to the Employees view.</summary>
        private void NavigateToEmployees()
        {
            var employeesView = new AdminEmployeesView(_username, this);
            this.Hide();
            employeesView.Show();
        }

        /// <summary>Navigates to the Reports tab (Employee Performance table).</summary>
        private void NavigateToReportsTab()
        {
            MainContentGrid.Children.Clear();
            MainContentGrid.ColumnDefinitions.Clear();

            SetActiveNavButton(ReportsButton);

            var reportsView = new ReportsView(_username)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment   = VerticalAlignment.Stretch,
                OnBack = RestoreDashboardView,
            };
            Grid.SetColumnSpan(reportsView, 3);
            MainContentGrid.Children.Add(reportsView);
        }

        /// <summary>Navigates to the Task Management tab.</summary>
        private void NavigateToToDoTab()
        {
            MainContentGrid.Children.Clear();
            MainContentGrid.ColumnDefinitions.Clear();

            SetActiveNavButton(ToDoButton);

            var vm = new AdminToDoViewModel(_username);
            var todoTab = new AdminToDoTab
            {
                DataContext = vm,
                OnBack = RestoreDashboardView
            };
            Grid.SetColumnSpan(todoTab, 3);
            MainContentGrid.Children.Add(todoTab);
        }

        /// <summary>Restores the default dashboard view (3-column layout).</summary>
        private void RestoreDashboardView()
        {
            // Reload the window to restore the original XAML layout
            var newDashboard = new AdminDashboard(_username);
            newDashboard.Show();
            this.Close();
        }

        /// <summary>Called externally to open the Task Management tab immediately after Show().</summary>
        public void OpenToDoTab() => NavigateToToDoTab();

        /// <summary>Called externally to open the Reports tab immediately after Show().</summary>
        public void OpenReportsTab() => NavigateToReportsTab();

        /// <summary>Called externally to open the Leaves tab immediately after Show().</summary>
        public void OpenLeavesTab() => NavigateToLeavesTab();

        /// <summary>Navigates to the Leaves tab.</summary>
        private void NavigateToLeavesTab()
        {
            MainContentGrid.Children.Clear();
            MainContentGrid.ColumnDefinitions.Clear();

            SetActiveNavButton(LeavesButton);

            var leavesView = new LeavesView(_username)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment   = VerticalAlignment.Stretch,
                OnBack = RestoreDashboardView,
            };
            Grid.SetColumnSpan(leavesView, 3);
            MainContentGrid.Children.Add(leavesView);
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
                // Stay on the dashboard and highlight Overview as active
                SetActiveNavButton(OverviewButton);
                RestoreDashboardView();
            }
            else if (sender == EmployeesButton)
            {
                SetActiveNavButton(EmployeesButton);
                NavigateToEmployees();
            }
            else if (sender == AttendanceButton)
            {
                SetActiveNavButton(AttendanceButton);
                var attendanceDashboard = new AttendanceDashboard(_username) { Owner = this };
                this.Hide();
                attendanceDashboard.Show();
            }
            else if (sender == ToDoButton)
            {
                NavigateToToDoTab();
            }
            else if (sender == ReportsButton)
            {
                NavigateToReportsTab();
            }
            else if (sender == LeavesButton)
            {
                NavigateToLeavesTab();
            }
            System.Diagnostics.Debug.WriteLine(nameof(NavButton_Click));
        }

        // ── Left Column handlers ──────────────────────────────────────
        private void StatCardRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadEmployeeCount();
        }

        /// <summary>Clicking the "Number of Employees" card opens the Employees view.</summary>
        private void StatCard0_Click(object sender, MouseButtonEventArgs e)
        {
            NavigateToEmployees();
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

        /// <summary>Clicking any employee row opens the Employees view.</summary>
        private void EmployeeListItem_Click(object sender, RoutedEventArgs e)
        {
            NavigateToEmployees();
        }
    }
}
