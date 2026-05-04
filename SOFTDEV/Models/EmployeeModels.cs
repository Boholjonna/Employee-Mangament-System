using System.Windows.Media;

namespace SOFTDEV
{
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
