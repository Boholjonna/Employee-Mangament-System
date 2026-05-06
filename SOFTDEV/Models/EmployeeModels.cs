using System.Windows.Media;

namespace SOFTDEV
{
    /// <summary>
    /// Carries an employee's id, name, position, salary and payroll
    /// for display in the AdminOverviewUI financial cards.
    /// </summary>
    public class EmployeeFinancialInfo
    {
        public int     Id       { get; set; }
        public string  Name     { get; set; } = string.Empty;
        public string  Position { get; set; } = string.Empty;
        public decimal Salary   { get; set; }
        public decimal Payroll  { get; set; }

        /// <summary>Used as the display string in the ComboBox.</summary>
        public override string ToString() => Name;
    }

    /// <summary>
    /// Carries all displayable fields for a single employee,
    /// used to populate the Employee Detail Panel in AdminEmployeesView.
    /// </summary>
    public class EmployeeDetail
    {
        public string  Name             { get; set; } = string.Empty;
        public string  Position         { get; set; } = string.Empty;
        public decimal Salary           { get; set; } = 0m;
        public decimal Payroll          { get; set; } = 0m;
        public string  DateHired        { get; set; } = string.Empty;
        public string  ContactNo        { get; set; } = string.Empty;
        public string  Address          { get; set; } = string.Empty;
        public string  EmergencyContact { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a task item in the employee's task list.
    /// </summary>
    public class TaskItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Gets the status color as a Brush for binding.
        /// </summary>
        public Brush StatusBrush
        {
            get
            {
                if (string.IsNullOrEmpty(StatusColor))
                    return new SolidColorBrush(Color.FromRgb(123, 97, 255)); // Default purple

                try
                {
                    var converter = new BrushConverter();
                    var brush = converter.ConvertFromString(StatusColor);
                    return brush as Brush ?? new SolidColorBrush(Color.FromRgb(123, 97, 255));
                }
                catch
                {
                    return new SolidColorBrush(Color.FromRgb(123, 97, 255)); // Default purple
                }
            }
        }
    }

    /// <summary>
    /// Represents a notification item.
    /// </summary>
    public class NotificationItem
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents an upcoming event.
    /// </summary>
    public class UpcomingEvent
    {
        public string EventName { get; set; } = string.Empty;
        public string EventDate { get; set; } = string.Empty;
    }
}
