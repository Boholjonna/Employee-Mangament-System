using System.Windows;

namespace SOFTDEV
{
    /// <summary>
    /// A styled time-picker dialog for selecting a clock-in time.
    /// After the user confirms, <see cref="SelectedTime"/> contains the result
    /// as a formatted string (e.g. "09:30 AM").
    /// </summary>
    public partial class ClockInTimePickerDialog : Window
    {
        // ── State ─────────────────────────────────────────────────────
        private int  _hour   = 8;   // 1–12
        private int  _minute = 0;   // 0–59
        private bool _isAm   = true;

        // ── Result ────────────────────────────────────────────────────
        /// <summary>
        /// The time chosen by the user, e.g. "09:30 AM".
        /// Only valid when <see cref="Window.DialogResult"/> is <c>true</c>.
        /// </summary>
        public string SelectedTime { get; private set; } = string.Empty;

        // ── Constructor ───────────────────────────────────────────────
        public ClockInTimePickerDialog()
        {
            InitializeComponent();
            RefreshDisplay();
        }

        // ── Spinner handlers ──────────────────────────────────────────

        private void HourUp_Click(object sender, RoutedEventArgs e)
        {
            _hour = _hour >= 12 ? 1 : _hour + 1;
            RefreshDisplay();
        }

        private void HourDown_Click(object sender, RoutedEventArgs e)
        {
            _hour = _hour <= 1 ? 12 : _hour - 1;
            RefreshDisplay();
        }

        private void MinuteUp_Click(object sender, RoutedEventArgs e)
        {
            _minute = _minute >= 59 ? 0 : _minute + 1;
            RefreshDisplay();
        }

        private void MinuteDown_Click(object sender, RoutedEventArgs e)
        {
            _minute = _minute <= 0 ? 59 : _minute - 1;
            RefreshDisplay();
        }

        // ── AM / PM toggle ────────────────────────────────────────────

        private void AmPm_Click(object sender, RoutedEventArgs e)
        {
            _isAm = (sender == AmButton);
            RefreshDisplay();
        }

        // ── Confirm / Cancel ──────────────────────────────────────────

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            SelectedTime = $"{_hour:D2}:{_minute:D2} {(_isAm ? "AM" : "PM")}";
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        // ── Display refresh ───────────────────────────────────────────

        private void RefreshDisplay()
        {
            HourDisplay.Text   = _hour.ToString("D2");
            MinuteDisplay.Text = _minute.ToString("D2");

            // Highlight the active AM/PM button
            AmButton.Opacity = _isAm  ? 1.0 : 0.4;
            PmButton.Opacity = !_isAm ? 1.0 : 0.4;
        }
    }
}
