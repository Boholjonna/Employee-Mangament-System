namespace SOFTDEV
{
    /// <summary>
    /// Represents one row from the attendance table.
    /// StatusColor is a hex string resolved from the Status value.
    /// </summary>
    public class AttendanceRecord_Model
    {
        /// <summary>
        /// The name of the employee associated with this attendance record.
        /// </summary>
        public string EmployeeName { get; set; } = string.Empty;

        /// <summary>
        /// The date of the attendance record (e.g., "2026-05-06").
        /// </summary>
        public string Date { get; set; } = string.Empty;

        /// <summary>
        /// The time the employee clocked in (e.g., "08:00 AM").
        /// </summary>
        public string TimeIn { get; set; } = string.Empty;

        /// <summary>
        /// The time the employee clocked out (e.g., "05:00 PM").
        /// </summary>
        public string TimeOut { get; set; } = string.Empty;

        /// <summary>
        /// The total hours worked for this attendance record (e.g., "8.5").
        /// </summary>
        public string TotalHours { get; set; } = string.Empty;

        /// <summary>
        /// The attendance status (e.g., "Present", "Late", "Absent", "On Leave").
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Hex color code for the StatusBadge background.
        /// Resolved by the ViewModel based on Status value.
        /// Present → #2ecc71, Late → #f39c12, Absent → #e74c3c, On Leave → #3498db
        /// </summary>
        public string StatusColor { get; set; } = "#7b61ff";
    }
}
