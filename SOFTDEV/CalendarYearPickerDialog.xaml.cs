using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SOFTDEV
{
    /// <summary>
    /// A styled year-picker dialog.
    /// Shows 12 years at a time (one decade page) with ◀ / ▶ decade navigation.
    /// Clicking a year confirms the selection immediately.
    /// </summary>
    public partial class CalendarYearPickerDialog : Window
    {
        // ── State ─────────────────────────────────────────────────────
        private int _currentDecadeStart;   // first year shown on the current page
        private readonly int _initialYear;

        // ── Result ────────────────────────────────────────────────────
        /// <summary>The year chosen by the user. Valid only when DialogResult is true.</summary>
        public int SelectedYear { get; private set; }

        // ── Constructor ───────────────────────────────────────────────
        public CalendarYearPickerDialog(int currentYear)
        {
            InitializeComponent();

            _initialYear        = currentYear;
            SelectedYear        = currentYear;

            // Align decade start to the nearest lower multiple of 12
            _currentDecadeStart = currentYear - (currentYear % 12);

            RefreshYearGrid();
        }

        // ── Decade navigation ─────────────────────────────────────────

        private void PrevDecade_Click(object sender, RoutedEventArgs e)
        {
            _currentDecadeStart -= 12;
            RefreshYearGrid();
        }

        private void NextDecade_Click(object sender, RoutedEventArgs e)
        {
            _currentDecadeStart += 12;
            RefreshYearGrid();
        }

        // ── Cancel ────────────────────────────────────────────────────

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        // ── Grid builder ─────────────────────────────────────────────

        private void RefreshYearGrid()
        {
            DecadeRangeLabel.Text = $"{_currentDecadeStart} – {_currentDecadeStart + 11}";

            YearGrid.Children.Clear();

            for (int i = 0; i < 12; i++)
            {
                int year = _currentDecadeStart + i;

                bool isSelected = (year == _initialYear);

                var btn = new Button
                {
                    Content         = year.ToString(),
                    Tag             = year,
                    Margin          = new Thickness(4),
                    FontSize        = 14,
                    FontWeight      = isSelected ? FontWeights.Bold : FontWeights.Normal,
                    Foreground      = Brushes.White,
                    Background      = isSelected
                                        ? (Brush)Application.Current.Resources["PurpleAccentBrush"]
                                        : new SolidColorBrush(Color.FromRgb(0x2a, 0x2a, 0x3e)),
                    BorderThickness = new Thickness(0),
                    Cursor          = System.Windows.Input.Cursors.Hand,
                };

                // Rounded corners via a simple ControlTemplate override
                btn.Style = (Style)Application.Current.Resources["PillButtonStyle"];

                // Override background after style is applied
                btn.Background = isSelected
                    ? (Brush)Application.Current.Resources["PurpleAccentBrush"]
                    : new SolidColorBrush(Color.FromArgb(0x55, 0x7b, 0x61, 0xff));

                btn.Click += YearButton_Click;

                YearGrid.Children.Add(btn);
            }
        }

        private void YearButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int year)
            {
                SelectedYear = year;
                DialogResult = true;
            }
        }
    }
}
