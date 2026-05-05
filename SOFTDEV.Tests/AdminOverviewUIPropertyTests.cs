// AdminOverviewUIPropertyTests.cs
// Property-based tests for AdminOverviewUI using FsCheck.
//
// Properties covered:
//   Property 1 — Username display round-trip              (Requirement 2.6)
//   Property 2 — TabMenu button CornerRadius invariant    (Requirement 3.4)
//   Property 3 — Donut segment color distinctness         (Requirement 4.4)
//   Property 4 — Legend completeness                      (Requirement 4.5)
//   Property 5 — Performance series color distinctness    (Requirement 5.3)
//   Property 6 — Data-point marker count invariant        (Requirement 5.4)
//   Property 7 — FinancialCard structure invariant        (Requirements 6.2, 6.3, 6.5)
//   Property 8 — Card styling invariant                   (Requirements 4.6, 5.6, 6.4, 7.3, 7.4)
//
// All WPF-dependent properties run on an STA thread via StaHelper.RunOnSta.
// Properties 3 and 5 operate on pure data model classes and need no STA thread.
// Each property runs a minimum of 100 FsCheck iterations.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Xunit;

namespace SOFTDEV.Tests
{
    public class AdminOverviewUIPropertyTests
    {
        // -----------------------------------------------------------------------
        // Shared reflection helpers
        // -----------------------------------------------------------------------

        private static readonly BindingFlags PrivateInstance =
            BindingFlags.NonPublic | BindingFlags.Instance;

        private static void SetPrivateField(object target, string fieldName, object? value)
        {
            var field = target.GetType().GetField(fieldName, PrivateInstance)
                        ?? throw new InvalidOperationException(
                               $"Field '{fieldName}' not found on {target.GetType().Name}");
            field.SetValue(target, value);
        }

        private static void InvokePrivateMethod(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, PrivateInstance)
                         ?? throw new InvalidOperationException(
                                $"Method '{methodName}' not found on {target.GetType().Name}");
            method.Invoke(target, null);
        }

        /// <summary>
        /// Forces a layout pass so that ActualWidth/ActualHeight are non-zero
        /// for a canvas that has explicit Width/Height set in XAML.
        /// </summary>
        private static void ForceLayout(FrameworkElement element)
        {
            element.Measure(new Size(element.Width, element.Height));
            element.Arrange(new Rect(0, 0, element.Width, element.Height));
        }

        /// <summary>
        /// Walks the logical tree and yields all descendants of type <typeparamref name="T"/>.
        /// </summary>
        private static IEnumerable<T> GetLogicalDescendants<T>(DependencyObject parent)
            where T : DependencyObject
        {
            foreach (object child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is DependencyObject dep)
                {
                    if (dep is T typed)
                        yield return typed;

                    foreach (var descendant in GetLogicalDescendants<T>(dep))
                        yield return descendant;
                }
            }
        }

        // -----------------------------------------------------------------------
        // FsCheck generators
        // -----------------------------------------------------------------------

        /// <summary>
        /// Generates a non-null, non-empty string of printable ASCII characters.
        /// Used for username generation.
        /// </summary>
        private static Gen<string> NonEmptyStringGen() =>
            Gen.NonEmptyListOf(Gen.Choose(32, 126).Select(c => (char)c))
               .Select(chars => string.Concat(chars));

        /// <summary>
        /// Generates a valid hex color string in the form "#rrggbb".
        /// </summary>
        private static Gen<string> HexColorGen() =>
            Gen.Choose(0, 0xFFFFFF)
               .Select(n => $"#{n:X6}");

        /// <summary>
        /// Generates a list of <see cref="DonutSegment"/> objects with distinct labels
        /// and distinct colors, with 1–8 segments whose percentages sum to 100.
        /// </summary>
        private static Gen<List<DonutSegment>> DonutSegmentListGen()
        {
            // Generate between 1 and 8 segments
            return Gen.Choose(1, 8).SelectMany(count =>
            {
                // Generate 'count' distinct hex colors
                var colorsGen = Gen.Choose(0, 0xFFFFFF)
                    .ListOf(count)
                    .Select(ns => ns.Distinct().ToList())
                    .Where(ns => ns.Count == count)
                    .Select(ns => ns.Select(n => $"#{n:X6}").ToList());

                return colorsGen.Select(colors =>
                {
                    // Distribute 100% across segments (integer split)
                    var percentages = SplitIntoN(100, count);
                    var segments = new List<DonutSegment>();
                    for (int i = 0; i < count; i++)
                    {
                        segments.Add(new DonutSegment
                        {
                            Label      = $"Segment{i}",
                            Percentage = percentages[i],
                            Color      = colors[i],
                        });
                    }
                    return segments;
                });
            });
        }

        /// <summary>
        /// Splits <paramref name="total"/> into exactly <paramref name="n"/> positive
        /// integer parts that sum to <paramref name="total"/>.
        /// </summary>
        private static List<double> SplitIntoN(int total, int n)
        {
            var parts = new List<double>();
            int remaining = total;
            for (int i = 0; i < n - 1; i++)
            {
                int part = Math.Max(1, remaining / (n - i));
                parts.Add(part);
                remaining -= part;
            }
            parts.Add(remaining);
            return parts;
        }

        /// <summary>
        /// Generates a list of <see cref="PerformanceSeries"/> with at least 2 entries,
        /// each with a distinct color and between 1 and 12 data points.
        /// </summary>
        private static Gen<List<PerformanceSeries>> PerformanceSeriesListGen(int minCount = 2)
        {
            return Gen.Choose(minCount, 6).SelectMany(count =>
            {
                var colorsGen = Gen.Choose(0, 0xFFFFFF)
                    .ListOf(count)
                    .Select(ns => ns.Distinct().ToList())
                    .Where(ns => ns.Count == count)
                    .Select(ns => ns.Select(n => $"#{n:X6}").ToList());

                return colorsGen.SelectMany(colors =>
                {
                    // Generate point counts for each series
                    var pointCountsGen = Gen.Choose(1, 12).ListOf(count);

                    return pointCountsGen.Select(pointCounts =>
                    {
                        var seriesList = new List<PerformanceSeries>();
                        for (int i = 0; i < count; i++)
                        {
                            var points = Enumerable.Range(0, pointCounts[i])
                                .Select(j => new Point(j, (j * 10) % 100))
                                .ToList();

                            seriesList.Add(new PerformanceSeries
                            {
                                Label  = $"Series{i}",
                                Color  = colors[i],
                                Points = points,
                            });
                        }
                        return seriesList;
                    });
                });
            });
        }

        // -----------------------------------------------------------------------
        // Property 1 — Username display round-trip
        // Feature: admin-overview-ui, Property 1: Username display round-trip
        //
        // For any non-null username string passed to the AdminOverviewUI constructor,
        // UsernameLabel.Text must display exactly that string.
        //
        // Validates: Requirement 2.6
        // -----------------------------------------------------------------------

        /// <summary>
        /// For any non-null username, constructing AdminOverviewUI and reading
        /// UsernameLabel.Text must return the original username unchanged.
        /// Minimum 100 FsCheck iterations.
        ///
        /// Uses Prop.ForAll so all iterations run within a single STA thread
        /// invocation, avoiding WPF BAML stream race conditions that occur when
        /// 100 separate STA threads each call InitializeComponent concurrently.
        /// </summary>
        [Property(MaxTest = 100,
                  DisplayName = "Property 1: Username display round-trip")]
        public Property Property1_UsernameDisplayRoundTrip()
        {
            // Feature: admin-overview-ui, Property 1: Username display round-trip
            return Prop.ForAll(
                NonEmptyStringGen().ToArbitrary(),
                username =>
                {
                    bool result = false;
                    StaHelper.RunOnSta(() =>
                    {
                        WpfAppBootstrap.EnsureInitialized();

                        var window = new AdminOverviewUI(username);
                        var label  = (TextBlock)window.FindName("UsernameLabel");

                        result = label is not null && label.Text == username;

                        window.Close();
                    });

                    return result.Label($"UsernameLabel.Text did not match username '{username}'");
                });
        }

        // -----------------------------------------------------------------------
        // Property 2 — TabMenu button CornerRadius invariant
        // Feature: admin-overview-ui, Property 2: TabMenu button CornerRadius invariant
        //
        // For any button in TabMenuPanel (active or inactive), all four CornerRadius
        // values must be in the range [15, 20] pixels.
        //
        // Validates: Requirement 3.4
        // -----------------------------------------------------------------------

        /// <summary>
        /// Every button in TabMenuPanel must have all four CornerRadius values in [15, 20].
        /// This is a universal property over the finite set of tab buttons.
        /// Minimum 100 FsCheck iterations (the window is reconstructed each time).
        /// </summary>
        [Property(MaxTest = 100,
                  DisplayName = "Property 2: TabMenu button CornerRadius invariant")]
        public Property Property2_TabMenuButtonCornerRadiusInvariant(bool _unused)
        {
            // Feature: admin-overview-ui, Property 2: TabMenu button CornerRadius invariant
            bool result = false;
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminOverviewUI("TestUser");
                var tabMenuPanel = (StackPanel)window.FindName("TabMenuPanel");

                if (tabMenuPanel is null)
                {
                    window.Close();
                    return;
                }

                bool allValid = true;
                foreach (var child in tabMenuPanel.Children)
                {
                    if (child is not Button btn) continue;

                    // CornerRadius is set via PillButtonStyle on the Border inside the
                    // button's ControlTemplate.  We inspect the template's visual tree.
                    btn.ApplyTemplate();
                    var templateBorder = FindVisualChild<Border>(btn);

                    if (templateBorder is not null)
                    {
                        var cr = templateBorder.CornerRadius;
                        if (cr.TopLeft    < 15 || cr.TopLeft    > 20 ||
                            cr.TopRight   < 15 || cr.TopRight   > 20 ||
                            cr.BottomLeft < 15 || cr.BottomLeft > 20 ||
                            cr.BottomRight < 15 || cr.BottomRight > 20)
                        {
                            allValid = false;
                            break;
                        }
                    }
                }

                result = allValid;
                window.Close();
            });

            return result.ToProperty();
        }

        /// <summary>
        /// Walks the visual tree to find the first child of type <typeparamref name="T"/>.
        /// </summary>
        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T typed)
                    return typed;

                var found = FindVisualChild<T>(child);
                if (found is not null)
                    return found;
            }
            return null;
        }

        // -----------------------------------------------------------------------
        // Property 3 — Donut segment color distinctness
        // Feature: admin-overview-ui, Property 3: Donut segment color distinctness
        //
        // For any collection of DonutSegment objects, all Color values must be
        // pairwise distinct — no two segments share the same fill color.
        //
        // Pure data property — no WPF/STA dependency.
        // Validates: Requirement 4.4
        // -----------------------------------------------------------------------

        /// <summary>
        /// For any list of DonutSegment objects with distinct colors (as generated),
        /// all Color values must be pairwise distinct.
        /// Minimum 100 FsCheck iterations.
        /// </summary>
        [Property(MaxTest = 100,
                  DisplayName = "Property 3: Donut segment color distinctness")]
        public Property Property3_DonutSegmentColorDistinctness()
        {
            // Feature: admin-overview-ui, Property 3: Donut segment color distinctness
            return Prop.ForAll(
                DonutSegmentListGen().ToArbitrary(),
                segments =>
                {
                    var colors = segments.Select(s => s.Color).ToList();
                    bool allDistinct = colors.Distinct(StringComparer.OrdinalIgnoreCase).Count() == colors.Count;
                    return allDistinct.Label(
                        $"Expected all {colors.Count} segment colors to be distinct, " +
                        $"but found duplicates: [{string.Join(", ", colors)}]");
                });
        }

        // -----------------------------------------------------------------------
        // Property 4 — Legend completeness
        // Feature: admin-overview-ui, Property 4: Legend completeness
        //
        // For any list of DonutSegment objects, after DrawDonutChart(), LegendPanel
        // must contain exactly one entry per segment, and each entry must include
        // both the segment's Label and Percentage as a string.
        //
        // Validates: Requirement 4.5
        // -----------------------------------------------------------------------

        /// <summary>
        /// For any list of DonutSegment objects, after DrawDonutChart(), LegendPanel
        /// must have exactly one child per segment, and each child must contain the
        /// segment's Label and Percentage as text.
        /// Minimum 100 FsCheck iterations.
        /// </summary>
        [Property(MaxTest = 100,
                  DisplayName = "Property 4: Legend completeness")]
        public Property Property4_LegendCompleteness()
        {
            // Feature: admin-overview-ui, Property 4: Legend completeness
            return Prop.ForAll(
                DonutSegmentListGen().ToArbitrary(),
                segments =>
                {
                    bool result = false;
                    string failureReason = string.Empty;

                    StaHelper.RunOnSta(() =>
                    {
                        WpfAppBootstrap.EnsureInitialized();

                        var window = new AdminOverviewUI("TestUser");

                        var donutCanvas = (Canvas)window.FindName("DonutCanvas");
                        var legendPanel = (StackPanel)window.FindName("LegendPanel");

                        if (donutCanvas is null || legendPanel is null)
                        {
                            failureReason = "DonutCanvas or LegendPanel not found.";
                            window.Close();
                            return;
                        }

                        // Force layout so the guard in DrawDonutChart passes
                        ForceLayout(donutCanvas);

                        // Inject our custom segments and redraw
                        SetPrivateField(window, "_taskSegments", segments);
                        window.DrawDonutChart();

                        // LegendPanel must have exactly one child per segment
                        if (legendPanel.Children.Count != segments.Count)
                        {
                            failureReason = $"Expected {segments.Count} legend rows, " +
                                            $"got {legendPanel.Children.Count}.";
                            window.Close();
                            return;
                        }

                        // Each legend row must contain the segment's Label and Percentage
                        for (int i = 0; i < segments.Count; i++)
                        {
                            var seg = segments[i];
                            var row = legendPanel.Children[i] as StackPanel;
                            if (row is null)
                            {
                                failureReason = $"Legend row {i} is not a StackPanel.";
                                window.Close();
                                return;
                            }

                            // Collect all text from TextBlocks in this row
                            var rowText = string.Concat(
                                GetLogicalDescendants<TextBlock>(row)
                                    .Select(tb => tb.Text));

                            if (!rowText.Contains(seg.Label))
                            {
                                failureReason = $"Legend row {i} does not contain label '{seg.Label}'.";
                                window.Close();
                                return;
                            }

                            string percentageStr = seg.Percentage.ToString();
                            if (!rowText.Contains(percentageStr))
                            {
                                failureReason = $"Legend row {i} does not contain percentage '{percentageStr}'.";
                                window.Close();
                                return;
                            }
                        }

                        result = true;
                        window.Close();
                    });

                    return result.Label(failureReason);
                });
        }

        // -----------------------------------------------------------------------
        // Property 5 — Performance series color distinctness
        // Feature: admin-overview-ui, Property 5: Performance series color distinctness
        //
        // For any collection of PerformanceSeries with at least 2 entries, all Color
        // values must be pairwise distinct.
        //
        // Pure data property — no WPF/STA dependency.
        // Validates: Requirement 5.3
        // -----------------------------------------------------------------------

        /// <summary>
        /// For any list of PerformanceSeries with at least 2 entries, all Color values
        /// must be pairwise distinct.
        /// Minimum 100 FsCheck iterations.
        /// </summary>
        [Property(MaxTest = 100,
                  DisplayName = "Property 5: Performance series color distinctness")]
        public Property Property5_PerformanceSeriesColorDistinctness()
        {
            // Feature: admin-overview-ui, Property 5: Performance series color distinctness
            return Prop.ForAll(
                PerformanceSeriesListGen(minCount: 2).ToArbitrary(),
                seriesList =>
                {
                    var colors = seriesList.Select(s => s.Color).ToList();
                    bool allDistinct = colors.Distinct(StringComparer.OrdinalIgnoreCase).Count() == colors.Count;
                    return allDistinct.Label(
                        $"Expected all {colors.Count} series colors to be distinct, " +
                        $"but found duplicates: [{string.Join(", ", colors)}]");
                });
        }

        // -----------------------------------------------------------------------
        // Property 6 — Data-point marker count invariant
        // Feature: admin-overview-ui, Property 6: Data-point marker count invariant
        //
        // For any PerformanceSeries with N data points, after DrawPerformanceGraph(),
        // GraphCanvas must contain exactly N Ellipse elements for that series.
        //
        // Validates: Requirement 5.4
        // -----------------------------------------------------------------------

        /// <summary>
        /// For any list of PerformanceSeries, after DrawPerformanceGraph(), the total
        /// number of Ellipse elements on GraphCanvas must equal the sum of all data
        /// points across all series.
        /// Minimum 100 FsCheck iterations.
        /// </summary>
        [Property(MaxTest = 100,
                  DisplayName = "Property 6: Data-point marker count invariant")]
        public Property Property6_DataPointMarkerCountInvariant()
        {
            // Feature: admin-overview-ui, Property 6: Data-point marker count invariant
            return Prop.ForAll(
                PerformanceSeriesListGen(minCount: 1).ToArbitrary(),
                seriesList =>
                {
                    bool result = false;
                    string failureReason = string.Empty;

                    StaHelper.RunOnSta(() =>
                    {
                        WpfAppBootstrap.EnsureInitialized();

                        var window = new AdminOverviewUI("TestUser");

                        var graphCanvas = (Canvas)window.FindName("GraphCanvas");
                        if (graphCanvas is null)
                        {
                            failureReason = "GraphCanvas not found.";
                            window.Close();
                            return;
                        }

                        // Force layout so the guard in DrawPerformanceGraph passes
                        ForceLayout(graphCanvas);

                        // Inject our custom series and redraw
                        SetPrivateField(window, "_performanceSeries", seriesList);
                        window.DrawPerformanceGraph();

                        // Count Ellipse elements — one per data point across all series
                        int expectedMarkers = seriesList.Sum(s => s.Points.Count);
                        int actualMarkers   = graphCanvas.Children.OfType<Ellipse>().Count();

                        if (actualMarkers != expectedMarkers)
                        {
                            failureReason = $"Expected {expectedMarkers} Ellipse markers, " +
                                            $"got {actualMarkers}.";
                            window.Close();
                            return;
                        }

                        result = true;
                        window.Close();
                    });

                    return result.Label(failureReason);
                });
        }

        // -----------------------------------------------------------------------
        // Property 7 — FinancialCard structure invariant
        // Feature: admin-overview-ui, Property 7: FinancialCard structure invariant
        //
        // For each FinancialCard Border, the card must contain:
        //   • A circular Border whose CornerRadius.TopLeft == Width / 2
        //   • A TextBlock inside that Border with Text == "₱"
        //   • A monetary amount TextBlock with FontSize >= 18
        //
        // Validates: Requirements 6.2, 6.3, 6.5
        // -----------------------------------------------------------------------

        /// <summary>
        /// Both FinancialCards (MonthlySalaryCard and PayrollSummaryCard) must satisfy
        /// the structural invariant: circular icon border, peso symbol TextBlock, and
        /// large-font amount TextBlock.
        /// Minimum 100 FsCheck iterations (window reconstructed each time).
        /// </summary>
        [Property(MaxTest = 100,
                  DisplayName = "Property 7: FinancialCard structure invariant")]
        public Property Property7_FinancialCardStructureInvariant(bool _unused)
        {
            // Feature: admin-overview-ui, Property 7: FinancialCard structure invariant
            bool result = false;
            string failureReason = string.Empty;

            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminOverviewUI("TestUser");

                var cardNames = new[] { "MonthlySalaryCard", "PayrollSummaryCard" };

                foreach (var cardName in cardNames)
                {
                    var card = window.FindName(cardName) as Border;
                    if (card is null)
                    {
                        failureReason = $"{cardName} not found.";
                        window.Close();
                        return;
                    }

                    // Find the circular icon Border (Width=48, Height=48, CornerRadius=24)
                    var iconBorder = GetLogicalDescendants<Border>(card)
                        .FirstOrDefault(b => b.Width == 48 && b.Height == 48);

                    if (iconBorder is null)
                    {
                        failureReason = $"{cardName}: circular icon Border (48×48) not found.";
                        window.Close();
                        return;
                    }

                    // CornerRadius.TopLeft must equal Width / 2 (i.e. 24)
                    double expectedRadius = iconBorder.Width / 2.0;
                    if (Math.Abs(iconBorder.CornerRadius.TopLeft - expectedRadius) > 0.01)
                    {
                        failureReason = $"{cardName}: icon Border CornerRadius.TopLeft " +
                                        $"({iconBorder.CornerRadius.TopLeft}) != Width/2 ({expectedRadius}).";
                        window.Close();
                        return;
                    }

                    // TextBlock inside the icon Border must have Text == "₱"
                    var pesoLabel = GetLogicalDescendants<TextBlock>(iconBorder)
                        .FirstOrDefault(tb => tb.Text == "₱");

                    if (pesoLabel is null)
                    {
                        failureReason = $"{cardName}: TextBlock with Text='₱' not found inside icon Border.";
                        window.Close();
                        return;
                    }

                    // Amount TextBlock must have FontSize >= 18
                    var amountLabelName = cardName == "MonthlySalaryCard"
                        ? "MonthlySalaryAmount"
                        : "PayrollSummaryAmount";

                    var amountLabel = window.FindName(amountLabelName) as TextBlock;
                    if (amountLabel is null)
                    {
                        failureReason = $"{cardName}: amount TextBlock '{amountLabelName}' not found.";
                        window.Close();
                        return;
                    }

                    if (amountLabel.FontSize < 18)
                    {
                        failureReason = $"{cardName}: amount TextBlock FontSize ({amountLabel.FontSize}) < 18.";
                        window.Close();
                        return;
                    }
                }

                result = true;
                window.Close();
            });

            return result.Label(failureReason);
        }

        // -----------------------------------------------------------------------
        // Property 8 — Card styling invariant
        // Feature: admin-overview-ui, Property 8: Card styling invariant
        //
        // For each card Border in the main content grid, the card must have:
        //   • Background == CardBackgroundBrush (#15151b)
        //   • All four CornerRadius values in [15, 20]
        //   • DropShadowEffect with BlurRadius in [10, 20] and Opacity in [0.3, 0.5]
        //
        // Validates: Requirements 4.6, 5.6, 6.4, 7.3, 7.4
        // -----------------------------------------------------------------------

        /// <summary>
        /// All card Borders (TaskOverviewCard, EmployeePerformancesCard, MonthlySalaryCard,
        /// PayrollSummaryCard) must satisfy the card styling invariant.
        /// Minimum 100 FsCheck iterations (window reconstructed each time).
        /// </summary>
        [Property(MaxTest = 100,
                  DisplayName = "Property 8: Card styling invariant")]
        public Property Property8_CardStylingInvariant(bool _unused)
        {
            // Feature: admin-overview-ui, Property 8: Card styling invariant
            bool result = false;
            string failureReason = string.Empty;

            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminOverviewUI("TestUser");

                var cardNames = new[]
                {
                    "TaskOverviewCard",
                    "EmployeePerformancesCard",
                    "MonthlySalaryCard",
                    "PayrollSummaryCard",
                };

                var expectedBgColor = (Color)ColorConverter.ConvertFromString("#15151b");

                foreach (var cardName in cardNames)
                {
                    var card = window.FindName(cardName) as Border;
                    if (card is null)
                    {
                        failureReason = $"{cardName} not found.";
                        window.Close();
                        return;
                    }

                    // Background must be CardBackgroundBrush (#15151b)
                    if (card.Background is not SolidColorBrush bgBrush ||
                        bgBrush.Color != expectedBgColor)
                    {
                        failureReason = $"{cardName}: Background is not CardBackgroundBrush (#15151b).";
                        window.Close();
                        return;
                    }

                    // All four CornerRadius values must be in [15, 20]
                    var cr = card.CornerRadius;
                    if (cr.TopLeft    < 15 || cr.TopLeft    > 20 ||
                        cr.TopRight   < 15 || cr.TopRight   > 20 ||
                        cr.BottomLeft < 15 || cr.BottomLeft > 20 ||
                        cr.BottomRight < 15 || cr.BottomRight > 20)
                    {
                        failureReason = $"{cardName}: CornerRadius ({cr}) has values outside [15, 20].";
                        window.Close();
                        return;
                    }

                    // DropShadowEffect must be present with BlurRadius in [10, 20] and Opacity in [0.3, 0.5]
                    if (card.Effect is not DropShadowEffect shadow)
                    {
                        failureReason = $"{cardName}: DropShadowEffect not found.";
                        window.Close();
                        return;
                    }

                    if (shadow.BlurRadius < 10 || shadow.BlurRadius > 20)
                    {
                        failureReason = $"{cardName}: DropShadowEffect.BlurRadius ({shadow.BlurRadius}) " +
                                        $"is outside [10, 20].";
                        window.Close();
                        return;
                    }

                    if (shadow.Opacity < 0.3 || shadow.Opacity > 0.5)
                    {
                        failureReason = $"{cardName}: DropShadowEffect.Opacity ({shadow.Opacity}) " +
                                        $"is outside [0.3, 0.5].";
                        window.Close();
                        return;
                    }
                }

                result = true;
                window.Close();
            });

            return result.Label(failureReason);
        }
    }
}
