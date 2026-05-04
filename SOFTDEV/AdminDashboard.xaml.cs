using System;
using System.Collections.Generic;
using System.Windows;

namespace SOFTDEV
{
    /// <summary>
    /// Admin Dashboard window — code-behind.
    /// </summary>
    public partial class AdminDashboard : Window
    {
        // ── Private calendar state fields ─────────────────────────────
        private int _calendarYear  = DateTime.Today.Year;
        private int _calendarMonth = DateTime.Today.Month;
        private int _highlightDay  = DateTime.Today.Day;
        private List<CalendarDayItem> _calendarDays = new();

        // ── Public properties ─────────────────────────────────────────
        /// <summary>Gets or sets the list of employees shown in the employee list panel.</summary>
        public List<EmployeeEntry> Employees { get; set; } = new();

        /// <summary>Gets or sets the list of calendar day cells for the current month view.</summary>
        public List<CalendarDayItem> CalendarDays { get; set; } = new();

        // ── Constructor ───────────────────────────────────────────────
        public AdminDashboard(string username)
        {
            InitializeComponent();

            GreetingText.Text = $"Hello, {username}! 👋";

            Employees = new List<EmployeeEntry>
            {
                new("Alice Johnson",  "Software Engineer"),
                new("Bob Martinez",   "Product Manager"),
                new("Carol White",    "UX Designer"),
                new("David Kim",      "QA Engineer"),
                new("Eva Patel",      "DevOps Engineer"),
            };

            EmployeeListControl.ItemsSource = Employees;

            RefreshCalendar();
        }

        // ── Calendar generation ───────────────────────────────────────

        /// <summary>
        /// Generates the list of <see cref="CalendarDayItem"/> objects for the given month.
        /// Leading padding cells (Day == 0) are added so that day 1 falls on the correct
        /// column of a Sunday-first 7-column grid.
        /// </summary>
        /// <param name="year">The calendar year (must be ≥ 1).</param>
        /// <param name="month">The calendar month (1–12).</param>
        /// <param name="highlightDay">
        /// The day to highlight (1–DaysInMonth). Values outside this range are clamped;
        /// 0 means no cell is highlighted.
        /// </param>
        /// <returns>A list of <see cref="CalendarDayItem"/> objects representing the calendar grid.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="year"/> is less than 1, or <paramref name="month"/> is
        /// outside the range 1–12.
        /// </exception>
        internal List<CalendarDayItem> GenerateCalendarDays(int year, int month, int highlightDay)
        {
            if (year < 1)
                throw new ArgumentOutOfRangeException(nameof(year), year, "Year must be greater than or equal to 1.");
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), month, "Month must be between 1 and 12.");

            int daysInMonth = DateTime.DaysInMonth(year, month);

            // Clamp highlightDay to [0, daysInMonth]; 0 means no highlight.
            highlightDay = Math.Max(0, Math.Min(highlightDay, daysInMonth));

            var days = new List<CalendarDayItem>();

            // Leading padding cells so that day 1 lands on the correct weekday column.
            int startOffset = (int)new DateTime(year, month, 1).DayOfWeek; // 0 = Sunday
            for (int i = 0; i < startOffset; i++)
                days.Add(new CalendarDayItem(0, false, false));

            // Current-month day cells.
            for (int d = 1; d <= daysInMonth; d++)
                days.Add(new CalendarDayItem(d, true, d == highlightDay));

            return days;
        }

        /// <summary>
        /// Regenerates the calendar day list for the current <see cref="_calendarYear"/> /
        /// <see cref="_calendarMonth"/> and pushes it to the UI.
        /// </summary>
        private void RefreshCalendar()
        {
            CalendarDays = GenerateCalendarDays(_calendarYear, _calendarMonth, _highlightDay);
            _calendarDays = CalendarDays;
            CalendarDaysControl.ItemsSource = CalendarDays;
            CalendarMonthLabel.Text = $"{new DateTime(_calendarYear, _calendarMonth, 1):MMMM yyyy}";
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
            System.Diagnostics.Debug.WriteLine(nameof(NavButton_Click));
        }

        // ── Left Column handlers ──────────────────────────────────────
        private void StatCardRefresh_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(StatCardRefresh_Click));
        }

        private void ClockInOut_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(ClockInOut_Click));
        }

        private void LunchBreak_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(LunchBreak_Click));
        }

        // ── Center Column (Calendar) handlers ────────────────────────
        private void PrevMonth_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(PrevMonth_Click));
            if (_calendarMonth == 1)
            {
                _calendarMonth = 12;
                _calendarYear--;
            }
            else
            {
                _calendarMonth--;
            }
            RefreshCalendar();
        }

        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(NextMonth_Click));
            if (_calendarMonth == 12)
            {
                _calendarMonth = 1;
                _calendarYear++;
            }
            else
            {
                _calendarMonth++;
            }
            RefreshCalendar();
        }

        // ── Right Column (Employee List) handlers ─────────────────────
        private void EmployeeListRefresh_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(nameof(EmployeeListRefresh_Click));
        }
    }
}
