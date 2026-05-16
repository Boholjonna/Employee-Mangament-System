using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SOFTDEV
{
    public partial class ManagerDashboard : Window
    {
        private readonly DispatcherTimer _clockTimer;
        private DateTime _calendarMonth;
        private bool _overviewActive = false;   // false = dashboard, true = charts

        public class TeamMember
        {
            public string Name     { get; set; } = string.Empty;
            public string Position { get; set; } = string.Empty;
        }

        public ManagerDashboard(string managerName)
        {
            InitializeComponent();

            GreetingText.Text         = $"Hello {managerName}! 👋";
            OverviewGreetingText.Text = $"Hello {managerName}! 👋";
            UserNameButton.Content    = managerName;

            AttendanceDateLabel.Text = DateTime.Now.ToString("MMM dd, yyyy | dddd");

            _calendarMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            BuildCalendar();

            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += (_, _) => LiveClockLabel.Text = DateTime.Now.ToString("hh:mm:ss tt");
            _clockTimer.Start();

            TeamMembersList.ItemsSource = new List<TeamMember>
            {
                new() { Name = "Name", Position = "member position" },
                new() { Name = "Name", Position = "member position" },
                new() { Name = "Name", Position = "member position" },
                new() { Name = "Name", Position = "member position" },
                new() { Name = "Name", Position = "member position" },
                new() { Name = "Name", Position = "member position" },
                new() { Name = "Name", Position = "member position" },
            };

            // Draw charts when overview panel becomes visible
            SizeChanged += (_, _) => { if (_overviewActive) DrawLineChart(); };
        }

        // ── Nav switching ─────────────────────────────────────────────────────

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            if (btn == OverviewButton)
            {
                _overviewActive = !_overviewActive;

                if (_overviewActive)
                {
                    DashboardPanel.Visibility = Visibility.Collapsed;
                    OverviewPanel.Visibility  = Visibility.Visible;

                    // Charts need a layout pass before drawing
                    Dispatcher.InvokeAsync(() =>
                    {
                        DrawDonutChart();
                        DrawLineChart();
                    }, System.Windows.Threading.DispatcherPriority.Loaded);
                }
                else
                {
                    OverviewPanel.Visibility  = Visibility.Collapsed;
                    DashboardPanel.Visibility = Visibility.Visible;
                }
            }
            // Other nav buttons can be wired here
        }

        // ── Calendar ──────────────────────────────────────────────────────────

        private void BuildCalendar()
        {
            CalendarMonthLabel.Content = _calendarMonth.ToString("MMMM  yyyy");

            var days        = new List<CalendarDayItem>();
            int startDow    = (int)_calendarMonth.DayOfWeek;
            int daysInMonth = DateTime.DaysInMonth(_calendarMonth.Year, _calendarMonth.Month);

            for (int i = 0; i < startDow; i++)
                days.Add(new CalendarDayItem(0, false, false));

            for (int d = 1; d <= daysInMonth; d++)
            {
                var date     = new DateTime(_calendarMonth.Year, _calendarMonth.Month, d);
                bool isToday = date.Date == DateTime.Today;
                days.Add(new CalendarDayItem(d, true, isToday));
            }

            CalendarDaysControl.ItemsSource = days;
        }

        private void PrevMonth_Click(object sender, RoutedEventArgs e)
        {
            _calendarMonth = _calendarMonth.AddMonths(-1);
            BuildCalendar();
        }

        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            _calendarMonth = _calendarMonth.AddMonths(1);
            BuildCalendar();
        }

        private void CalendarMonthLabel_Click(object sender, RoutedEventArgs e) { }
        private void CalendarDay_Click(object sender, RoutedEventArgs e) { }

        // ── Donut Chart ───────────────────────────────────────────────────────

        private void DrawDonutChart()
        {
            DonutCanvas.Children.Clear();

            const double cx     = 100;
            const double cy     = 100;
            const double outer  = 90;
            const double inner  = 52;

            var segments = new (double pct, Color color)[]
            {
                (0.55, Color.FromRgb(0x7b, 0x61, 0xff)),
                (0.25, Color.FromRgb(0x9b, 0x7f, 0xe8)),
                (0.15, Color.FromRgb(0x4a, 0x90, 0xd9)),
                (0.05, Color.FromRgb(0xe8, 0xc8, 0x4a)),
            };

            double angle = -Math.PI / 2;
            foreach (var (pct, color) in segments)
            {
                double sweep = pct * 2 * Math.PI;
                DonutCanvas.Children.Add(BuildArc(cx, cy, outer, inner, angle, angle + sweep, color));
                angle += sweep;
            }

            var lbl = new TextBlock
            {
                Text       = "Tasks",
                Foreground = Brushes.White,
                FontSize   = 13,
                FontWeight = FontWeights.SemiBold,
            };
            Canvas.SetLeft(lbl, cx - 20);
            Canvas.SetTop(lbl,  cy - 10);
            DonutCanvas.Children.Add(lbl);
        }

        private static Path BuildArc(double cx, double cy, double r1, double r2,
                                     double a1, double a2, Color color)
        {
            bool large = (a2 - a1) > Math.PI;

            var fig = new PathFigure
            {
                StartPoint = new Point(cx + r1 * Math.Cos(a1), cy + r1 * Math.Sin(a1)),
                IsClosed   = true,
            };
            fig.Segments.Add(new ArcSegment(
                new Point(cx + r1 * Math.Cos(a2), cy + r1 * Math.Sin(a2)),
                new Size(r1, r1), 0, large, SweepDirection.Clockwise, true));
            fig.Segments.Add(new LineSegment(
                new Point(cx + r2 * Math.Cos(a2), cy + r2 * Math.Sin(a2)), true));
            fig.Segments.Add(new ArcSegment(
                new Point(cx + r2 * Math.Cos(a1), cy + r2 * Math.Sin(a1)),
                new Size(r2, r2), 0, large, SweepDirection.Counterclockwise, true));

            var geo = new PathGeometry();
            geo.Figures.Add(fig);

            return new Path
            {
                Data            = geo,
                Fill            = new SolidColorBrush(color),
                Stroke          = new SolidColorBrush(Color.FromRgb(0x15, 0x15, 0x1b)),
                StrokeThickness = 2,
            };
        }

        // ── Line Chart ────────────────────────────────────────────────────────

        private static readonly double[] _mobile  = { 12, 14, 16, 18, 24 };
        private static readonly double[] _desktop = { 10, 13, 16, 18, 20 };
        private static readonly double[] _other   = {  8,  9, 14, 15, 16 };

        private void DrawLineChart()
        {
            LineChartCanvas.Children.Clear();
            YAxisCanvas.Children.Clear();
            LineChartCanvas.UpdateLayout();

            double w = LineChartCanvas.ActualWidth;
            double h = LineChartCanvas.ActualHeight;
            if (w < 10 || h < 10) return;

            const double yMin  = 0;
            const double yMax  = 25;
            const double yStep = 5;
            const int    xCnt  = 5;
            const double xPad  = 10;
            const double yPad  = 10;

            double plotW = w - xPad * 2;
            double plotH = h - yPad * 2;

            // Grid lines + Y labels
            for (double v = yMin; v <= yMax; v += yStep)
            {
                double y = yPad + plotH - (v - yMin) / (yMax - yMin) * plotH;
                LineChartCanvas.Children.Add(new Line
                {
                    X1 = 0, Y1 = y, X2 = w, Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x44)),
                    StrokeThickness = 1,
                });
                var lbl = new TextBlock
                {
                    Text       = ((int)v).ToString(),
                    Foreground = new SolidColorBrush(Color.FromRgb(0xaa, 0xaa, 0xaa)),
                    FontSize   = 10,
                };
                Canvas.SetRight(lbl, 4);
                Canvas.SetTop(lbl, y - 7);
                YAxisCanvas.Children.Add(lbl);
            }

            Point Pt(int xi, double val) => new(
                xPad + xi / (double)(xCnt - 1) * plotW,
                yPad + plotH - (val - yMin) / (yMax - yMin) * plotH);

            void Series(double[] data, Color line, Color dot)
            {
                var pts = new PointCollection();
                for (int i = 0; i < data.Length; i++) pts.Add(Pt(i, data[i]));

                LineChartCanvas.Children.Add(new Polyline
                {
                    Points = pts,
                    Stroke = new SolidColorBrush(line),
                    StrokeThickness = 2.5,
                    StrokeLineJoin  = PenLineJoin.Round,
                    Fill = Brushes.Transparent,
                });

                foreach (var p in pts)
                {
                    var e = new Ellipse { Width = 8, Height = 8, Fill = new SolidColorBrush(dot) };
                    Canvas.SetLeft(e, p.X - 4);
                    Canvas.SetTop(e,  p.Y - 4);
                    LineChartCanvas.Children.Add(e);
                }
            }

            Series(_mobile,  Color.FromRgb(0x4a, 0x90, 0xd9), Color.FromRgb(0x4a, 0x90, 0xd9));
            Series(_desktop, Color.FromRgb(0x9b, 0x7f, 0xe8), Color.FromRgb(0xcc, 0x99, 0xff));
            Series(_other,   Color.FromRgb(0xe8, 0xc8, 0x4a), Color.FromRgb(0xe8, 0xc8, 0x4a));
        }

        // ── Button stubs ──────────────────────────────────────────────────────

        private void SearchButton_Click(object sender, RoutedEventArgs e) { }
        private void NotificationButton_Click(object sender, RoutedEventArgs e) { }
        private void UserNameButton_Click(object sender, RoutedEventArgs e) { }
        private void AvatarButton_Click(object sender, RoutedEventArgs e) { }

        private void TotalTaskCard_Click(object sender, RoutedEventArgs e) { }
        private void MessageTeamCard_Click(object sender, RoutedEventArgs e) { }
        private void AssignTaskCard_Click(object sender, RoutedEventArgs e) { }
        private void ViewReportsCard_Click(object sender, RoutedEventArgs e) { }

        private void AttendanceArrow_Click(object sender, RoutedEventArgs e) { }
        private void ClockInOut_Click(object sender, RoutedEventArgs e)
        {
            ClockInTimeLabel.Text   = DateTime.Now.ToString("hh:mm tt");
            ClockInStatusLabel.Text = "Clocked In";
        }
        private void LunchBreak_Click(object sender, RoutedEventArgs e) { }
        private void LeaveApprovalsArrow_Click(object sender, RoutedEventArgs e) { }
        private void MyTeamArrow_Click(object sender, RoutedEventArgs e) { }

        private void TaskOverviewArrow_Click(object sender, RoutedEventArgs e) { }
        private void EmpPerfArrow_Click(object sender, RoutedEventArgs e) { }
        private void EmpPerfBottomArrow_Click(object sender, RoutedEventArgs e) { }
    }
}
