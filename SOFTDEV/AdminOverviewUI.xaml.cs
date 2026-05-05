using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace SOFTDEV
{
    // ── Data model classes ────────────────────────────────────────────────────

    /// <summary>One slice of the donut chart.</summary>
    public class DonutSegment
    {
        public string Label      { get; set; } = string.Empty;
        public double Percentage { get; set; }   // 0–100
        public string Color      { get; set; } = "#ffffff"; // hex color string
    }

    /// <summary>One line series in the performance graph.</summary>
    public class PerformanceSeries
    {
        public string           Label  { get; set; } = string.Empty;
        public string           Color  { get; set; } = "#ffffff";
        public List<Point>      Points { get; set; } = new();
    }

    /// <summary>Financial summary data for the two financial cards.</summary>
    public class FinancialData
    {
        public decimal? MonthlySalary  { get; set; }
        public decimal? PayrollSummary { get; set; }
    }

    // ── Window code-behind ────────────────────────────────────────────────────

    /// <summary>
    /// AdminOverviewUI — high-level operational snapshot for administrators.
    /// Displays task completion (donut chart), employee performance trends
    /// (multi-line graph), and financial summaries (two compact cards).
    /// </summary>
    public partial class AdminOverviewUI : Window
    {
        // ── Private fields ────────────────────────────────────────────
        private string _username;
        private Window? _ownerDashboard;
        private List<DonutSegment>?    _taskSegments;
        private List<PerformanceSeries>? _performanceSeries;
        private FinancialData?         _financialData;

        // ── Constructor ───────────────────────────────────────────────

        /// <summary>
        /// Opens the AdminOverviewUI window for the given <paramref name="username"/>.
        /// Loads all data and draws charts after the window is initialized.
        /// </summary>
        /// <param name="username">The username to display in the TopNavBar.</param>
        /// <param name="ownerDashboard">
        /// Optional reference to the <see cref="AdminDashboard"/> that opened this window.
        /// When provided, the Back button will close this window and restore the dashboard.
        /// </param>
        public AdminOverviewUI(string username, Window? ownerDashboard = null)
        {
            InitializeComponent();

            _username        = username;
            _ownerDashboard  = ownerDashboard;
            UsernameLabel.Text = username;

            // Load data
            LoadTaskData();
            LoadPerformanceData();
            LoadFinancialData();

            // Draw charts once the window layout has been measured so that
            // ActualWidth / ActualHeight are available on the canvases.
            Loaded += (_, _) =>
            {
                DrawDonutChart();
                DrawPerformanceGraph();
            };
        }

        // ── Back navigation ───────────────────────────────────────────

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _ownerDashboard?.Show();
            this.Close();
        }

        // ── Data loading ──────────────────────────────────────────────

        /// <summary>
        /// Populates <see cref="_taskSegments"/> with the four default task-status segments.
        /// </summary>
        private void LoadTaskData()
        {
            _taskSegments = new List<DonutSegment>
            {
                new DonutSegment { Label = "Done",        Percentage = 55, Color = "#4ade80" },
                new DonutSegment { Label = "In-progress", Percentage = 25, Color = "#7b61ff" },
                new DonutSegment { Label = "Late",        Percentage = 15, Color = "#f87171" },
                new DonutSegment { Label = "Other",       Percentage =  5, Color = "#94a3b8" },
            };
        }

        /// <summary>
        /// Populates <see cref="_performanceSeries"/> with two default employee series.
        /// </summary>
        private void LoadPerformanceData()
        {
            _performanceSeries = new List<PerformanceSeries>
            {
                new PerformanceSeries
                {
                    Label  = "Employee A",
                    Color  = "#7b61ff",
                    Points = new List<Point>
                    {
                        new Point(0, 72),
                        new Point(1, 78),
                        new Point(2, 75),
                        new Point(3, 82),
                        new Point(4, 88),
                        new Point(5, 85),
                    }
                },
                new PerformanceSeries
                {
                    Label  = "Employee B",
                    Color  = "#4ade80",
                    Points = new List<Point>
                    {
                        new Point(0, 60),
                        new Point(1, 65),
                        new Point(2, 70),
                        new Point(3, 68),
                        new Point(4, 74),
                        new Point(5, 80),
                    }
                },
            };
        }

        /// <summary>
        /// Populates <see cref="_financialData"/> with default values and binds them to the UI.
        /// </summary>
        private void LoadFinancialData()
        {
            _financialData = new FinancialData
            {
                MonthlySalary  = 125000m,
                PayrollSummary = 980000m,
            };

            BindFinancialData();
        }

        // ── Financial card binding ────────────────────────────────────

        /// <summary>
        /// Formats and assigns financial amounts to the two FinancialCard amount labels.
        /// Displays "₱ —" when <see cref="_financialData"/> is null.
        /// </summary>
        private void BindFinancialData()
        {
            if (_financialData is null)
            {
                MonthlySalaryAmount.Text  = "₱ —";
                PayrollSummaryAmount.Text = "₱ —";
                return;
            }

            MonthlySalaryAmount.Text  = _financialData.MonthlySalary  is null
                ? "₱ —"
                : $"₱ {_financialData.MonthlySalary.Value:N2}";

            PayrollSummaryAmount.Text = _financialData.PayrollSummary is null
                ? "₱ —"
                : $"₱ {_financialData.PayrollSummary.Value:N2}";
        }

        // ── Donut chart drawing ───────────────────────────────────────

        /// <summary>
        /// Draws the donut chart on <see cref="DonutCanvas"/> using WPF Path / ArcSegment
        /// primitives. Shows a fallback message when data is unavailable.
        /// </summary>
        internal void DrawDonutChart()
        {
            // Guard: canvas must have been measured before drawing.
            if (DonutCanvas.ActualWidth == 0 || DonutCanvas.ActualHeight == 0)
                return;

            // Fallback: no data.
            if (_taskSegments is null || _taskSegments.Count == 0)
            {
                DonutCanvas.Visibility      = Visibility.Collapsed;
                DonutFallbackText.Visibility = Visibility.Visible;
                return;
            }

            DonutCanvas.Children.Clear();
            LegendPanel.Children.Clear();

            double width      = DonutCanvas.ActualWidth;
            double height     = DonutCanvas.ActualHeight;
            double cx         = width  / 2.0;
            double cy         = height / 2.0;
            double outerRadius = Math.Min(cx, cy) - 4;   // small inset so shadow isn't clipped
            double innerRadius = outerRadius * 0.55;      // hole size

            double startAngle = -90.0; // start at 12 o'clock

            foreach (var segment in _taskSegments)
            {
                double sweepAngle = segment.Percentage * 3.6; // 100% → 360°

                var path = BuildArcPath(cx, cy, outerRadius, innerRadius, startAngle, sweepAngle,
                                        ParseColor(segment.Color));
                DonutCanvas.Children.Add(path);

                startAngle += sweepAngle;

                // Legend row
                LegendPanel.Children.Add(BuildLegendRow(segment));
            }

            // Inner hole — overlay a filled ellipse in CardBackgroundBrush to create the donut hole.
            var hole = new Ellipse
            {
                Width  = innerRadius * 2,
                Height = innerRadius * 2,
                Fill   = (Brush)Application.Current.Resources["CardBackgroundBrush"],
            };
            Canvas.SetLeft(hole, cx - innerRadius);
            Canvas.SetTop(hole,  cy - innerRadius);
            DonutCanvas.Children.Add(hole);
        }

        /// <summary>
        /// Builds a WPF <see cref="Path"/> representing one donut arc segment.
        /// </summary>
        private static Path BuildArcPath(double cx, double cy,
                                          double outerR, double innerR,
                                          double startDeg, double sweepDeg,
                                          Color fillColor)
        {
            // Clamp sweep to just under 360° to avoid degenerate full-circle arcs.
            double clampedSweep = Math.Min(sweepDeg, 359.9999);

            double startRad = ToRadians(startDeg);
            double endRad   = ToRadians(startDeg + clampedSweep);

            // Outer arc points
            var outerStart = new Point(cx + outerR * Math.Cos(startRad),
                                       cy + outerR * Math.Sin(startRad));
            var outerEnd   = new Point(cx + outerR * Math.Cos(endRad),
                                       cy + outerR * Math.Sin(endRad));

            // Inner arc points (traversed in reverse to close the shape)
            var innerEnd   = new Point(cx + innerR * Math.Cos(endRad),
                                       cy + innerR * Math.Sin(endRad));
            var innerStart = new Point(cx + innerR * Math.Cos(startRad),
                                       cy + innerR * Math.Sin(startRad));

            bool isLargeArc = clampedSweep > 180;

            var figure = new PathFigure { StartPoint = outerStart, IsClosed = true };

            // Outer arc (clockwise)
            figure.Segments.Add(new ArcSegment(
                outerEnd,
                new Size(outerR, outerR),
                0,
                isLargeArc,
                SweepDirection.Clockwise,
                true));

            // Line to inner arc end
            figure.Segments.Add(new LineSegment(innerEnd, true));

            // Inner arc (counter-clockwise, back to start)
            figure.Segments.Add(new ArcSegment(
                innerStart,
                new Size(innerR, innerR),
                0,
                isLargeArc,
                SweepDirection.Counterclockwise,
                true));

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            return new Path
            {
                Data = geometry,
                Fill = new SolidColorBrush(fillColor),
            };
        }

        /// <summary>Builds a single legend row for a donut segment.</summary>
        private static StackPanel BuildLegendRow(DonutSegment segment)
        {
            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin      = new Thickness(0, 4, 0, 0),
            };

            var swatch = new Rectangle
            {
                Width  = 12,
                Height = 12,
                Fill   = new SolidColorBrush(ParseColor(segment.Color)),
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center,
            };

            var label = new TextBlock
            {
                Text       = $"{segment.Label} — {segment.Percentage}%",
                Foreground = Brushes.White,
                FontSize   = 12,
                VerticalAlignment = VerticalAlignment.Center,
            };

            row.Children.Add(swatch);
            row.Children.Add(label);
            return row;
        }

        // ── Performance graph drawing ─────────────────────────────────

        /// <summary>
        /// Draws the multi-line performance graph on <see cref="GraphCanvas"/> using WPF
        /// Polyline and Ellipse primitives. Shows a fallback message when data is unavailable.
        /// </summary>
        internal void DrawPerformanceGraph()
        {
            // Guard: canvas must have been measured before drawing.
            if (GraphCanvas.ActualWidth == 0 || GraphCanvas.ActualHeight == 0)
                return;

            // Fallback: no data.
            if (_performanceSeries is null || _performanceSeries.Count == 0)
            {
                GraphCanvas.Visibility      = Visibility.Collapsed;
                GraphFallbackText.Visibility = Visibility.Visible;
                return;
            }

            GraphCanvas.Children.Clear();

            double canvasW = GraphCanvas.ActualWidth;
            double canvasH = GraphCanvas.ActualHeight;

            // Reserve margins for axis labels.
            const double leftMargin   = 40;
            const double rightMargin  = 10;
            const double topMargin    = 10;
            const double bottomMargin = 30;

            double plotW = canvasW - leftMargin - rightMargin;
            double plotH = canvasH - topMargin  - bottomMargin;

            // Determine the number of X points from the first series.
            int pointCount = 0;
            foreach (var s in _performanceSeries)
                if (s.Points.Count > pointCount)
                    pointCount = s.Points.Count;

            // ── Y-axis labels (0, 25, 50, 75, 100) ───────────────────
            int[] yTicks = { 0, 25, 50, 75, 100 };
            foreach (int tick in yTicks)
            {
                double py = topMargin + plotH - (tick / 100.0 * plotH);

                var lbl = new TextBlock
                {
                    Text       = tick.ToString(),
                    Foreground = new SolidColorBrush(Color.FromRgb(0xaa, 0xaa, 0xaa)),
                    FontSize   = 10,
                };
                Canvas.SetLeft(lbl, 0);
                Canvas.SetTop(lbl,  py - 7);
                GraphCanvas.Children.Add(lbl);

                // Subtle grid line
                var gridLine = new Line
                {
                    X1              = leftMargin,
                    Y1              = py,
                    X2              = leftMargin + plotW,
                    Y2              = py,
                    Stroke          = new SolidColorBrush(Color.FromArgb(0x33, 0xff, 0xff, 0xff)),
                    StrokeThickness = 0.5,
                };
                GraphCanvas.Children.Add(gridLine);
            }

            // ── X-axis labels (month names) ───────────────────────────
            string[] monthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                    "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            for (int i = 0; i < pointCount; i++)
            {
                double px = leftMargin + (pointCount <= 1 ? plotW / 2.0
                                                          : i * plotW / (pointCount - 1));

                string monthLabel = i < monthNames.Length ? monthNames[i] : (i + 1).ToString();

                var lbl = new TextBlock
                {
                    Text       = monthLabel,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xaa, 0xaa, 0xaa)),
                    FontSize   = 10,
                };
                Canvas.SetLeft(lbl, px - 10);
                Canvas.SetTop(lbl,  topMargin + plotH + 8);
                GraphCanvas.Children.Add(lbl);
            }

            // ── Series lines and data-point markers ───────────────────
            foreach (var series in _performanceSeries)
            {
                if (series.Points.Count == 0)
                    continue;

                Color seriesColor = ParseColor(series.Color);
                var   brush       = new SolidColorBrush(seriesColor);

                var polyline = new Polyline
                {
                    Stroke          = brush,
                    StrokeThickness = 2,
                    StrokeLineJoin  = PenLineJoin.Round,
                };

                foreach (var pt in series.Points)
                {
                    double px = leftMargin + (pointCount <= 1 ? plotW / 2.0
                                                              : pt.X * plotW / (pointCount - 1));
                    double py = topMargin + plotH - (pt.Y / 100.0 * plotH);

                    polyline.Points.Add(new Point(px, py));

                    // Data-point marker (8×8 ellipse, centred on the point)
                    const double markerSize = 8;
                    var marker = new Ellipse
                    {
                        Width  = markerSize,
                        Height = markerSize,
                        Fill   = brush,
                    };
                    Canvas.SetLeft(marker, px - markerSize / 2);
                    Canvas.SetTop(marker,  py - markerSize / 2);
                    GraphCanvas.Children.Add(marker);
                }

                // Add the polyline before markers so markers render on top.
                GraphCanvas.Children.Insert(GraphCanvas.Children.Count - series.Points.Count, polyline);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────

        /// <summary>Converts degrees to radians.</summary>
        private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

        /// <summary>
        /// Parses a hex color string (e.g. "#7b61ff" or "7b61ff") into a <see cref="Color"/>.
        /// Returns <see cref="Colors.White"/> on parse failure.
        /// </summary>
        internal static Color ParseColor(string hex)
        {
            try
            {
                return (Color)ColorConverter.ConvertFromString(hex);
            }
            catch
            {
                return Colors.White;
            }
        }
    }
}
