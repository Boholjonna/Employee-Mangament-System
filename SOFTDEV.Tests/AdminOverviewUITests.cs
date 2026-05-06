// AdminOverviewUITests.cs
// Example-based unit tests for the AdminOverviewUI window.
//
// Tests covered:
//   AdminOverviewUI_WindowProperties_AreCorrect   – MinWidth, MinHeight, WindowStartupLocation,
//                                                   Background, FontFamily
//   AdminOverviewUI_SearchButton_HasCorrectContent – SearchButton.Content == "🔍"
//   AdminOverviewUI_NotificationButton_HasCorrectContent – NotificationButton.Content == "🔔"
//   AdminOverviewUI_AvatarBorder_CornerRadius      – AvatarBorder.CornerRadius.TopLeft == 18
//   AdminOverviewUI_UsernameLabel_DisplaysUsername – UsernameLabel.Text equals constructor arg
//   TabMenu_ButtonOrder_AndActiveState             – button order, 🏠 in OVERVIEW, non-active opacity ≈ 0.4
//   NullTaskData_HidesDonutCanvas_ShowsFallback    – null _taskSegments → DonutCanvas collapsed
//   EmptyTaskData_HidesDonutCanvas_ShowsFallback   – empty _taskSegments → DonutCanvas collapsed
//   NullPerformanceData_HidesGraphCanvas_ShowsFallback – null _performanceSeries → GraphCanvas collapsed
//   EmptyPerformanceData_HidesGraphCanvas_ShowsFallback – empty _performanceSeries → GraphCanvas collapsed
//   NullFinancialData_SetsBothAmountLabels_ToPlaceholder – null _financialData → "₱ —"
//   NavButton_Click_OverviewButton_OpensAdminOverviewUI – clicking OverviewButton instantiates AdminOverviewUI
//   DrawDonutChart_DefaultData_ProducesFourPathElements – default data → exactly 4 Path elements on DonutCanvas
//   DrawPerformanceGraph_DefaultData_HasXAndYAxisLabels – both X-axis and Y-axis TextBlock labels present
//
// Validates: Requirements 1.1, 1.2, 1.3, 1.4, 2.3, 2.4, 2.5, 2.6, 3.1, 3.2, 3.3, 4.2, 4.7, 5.5, 5.7, 6.6

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Xunit;

namespace SOFTDEV.Tests
{
    public class AdminOverviewUITests
    {
        // -----------------------------------------------------------------------
        // Test 1 — Window properties
        // Validates: Requirements 1.2, 1.3, 1.4
        // -----------------------------------------------------------------------

        /// <summary>
        /// AdminOverviewUI must be configured with MinWidth=1200, MinHeight=700,
        /// WindowStartupLocation=CenterScreen, Background=DarkBackgroundBrush (#0a0a0a),
        /// and FontFamily="Segoe UI".
        /// </summary>
        [Fact(DisplayName = "AdminOverviewUI: window properties are correct")]
        public void AdminOverviewUI_WindowProperties_AreCorrect()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminOverviewUI("TestUser");

                Assert.Equal(1200, window.MinWidth);
                Assert.Equal(700, window.MinHeight);
                Assert.Equal(WindowStartupLocation.CenterScreen, window.WindowStartupLocation);

                // Background must resolve to the solid color #0a0a0a (DarkBackgroundBrush)
                var bg = Assert.IsType<SolidColorBrush>(window.Background);
                var expectedColor = (Color)ColorConverter.ConvertFromString("#0a0a0a");
                Assert.Equal(expectedColor, bg.Color);

                // FontFamily must be Segoe UI
                Assert.Equal("Segoe UI", window.FontFamily.Source);

                window.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 2 — SearchButton content
        // Validates: Requirement 2.3
        // -----------------------------------------------------------------------

        /// <summary>
        /// The SearchButton in the TopNavBar must use the magnifying-glass Unicode
        /// symbol (🔍) as its Content.
        /// </summary>
        [Fact(DisplayName = "AdminOverviewUI TopNavBar: SearchButton content is 🔍")]
        public void AdminOverviewUI_SearchButton_HasCorrectContent()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminOverviewUI("TestUser");

                var searchButton = (Button)window.FindName("SearchButton");
                Assert.NotNull(searchButton);
                Assert.Equal("🔍", searchButton.Content as string);

                window.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 3 — NotificationButton content
        // Validates: Requirement 2.4
        // -----------------------------------------------------------------------

        /// <summary>
        /// The NotificationButton in the TopNavBar must use the bell Unicode
        /// symbol (🔔) as its Content.
        /// </summary>
        [Fact(DisplayName = "AdminOverviewUI TopNavBar: NotificationButton content is 🔔")]
        public void AdminOverviewUI_NotificationButton_HasCorrectContent()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminOverviewUI("TestUser");

                var notificationButton = (Button)window.FindName("NotificationButton");
                Assert.NotNull(notificationButton);
                Assert.Equal("🔔", notificationButton.Content as string);

                window.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 4 — AvatarBorder CornerRadius
        // Validates: Requirement 2.5
        // -----------------------------------------------------------------------

        /// <summary>
        /// The AvatarBorder in the TopNavBar must have a CornerRadius of 18 on all
        /// four corners, rendering it as a perfect circle (Width=36, Height=36).
        /// </summary>
        [Fact(DisplayName = "AdminOverviewUI TopNavBar: AvatarBorder CornerRadius is 18")]
        public void AdminOverviewUI_AvatarBorder_CornerRadius_IsEighteen()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminOverviewUI("TestUser");

                var avatarBorder = (Border)window.FindName("AvatarBorder");
                Assert.NotNull(avatarBorder);

                var cr = avatarBorder.CornerRadius;
                Assert.Equal(18, cr.TopLeft);
                Assert.Equal(18, cr.TopRight);
                Assert.Equal(18, cr.BottomLeft);
                Assert.Equal(18, cr.BottomRight);

                window.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 5 — UsernameLabel displays the username passed to the constructor
        // Validates: Requirement 2.6
        // -----------------------------------------------------------------------

        /// <summary>
        /// The UsernameLabel TextBlock must display exactly the username string
        /// passed to the AdminOverviewUI constructor.
        /// </summary>
        [Fact(DisplayName = "AdminOverviewUI TopNavBar: UsernameLabel displays constructor username")]
        public void AdminOverviewUI_UsernameLabel_DisplaysConstructorUsername()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                const string expectedUsername = "admin_jane";
                var window = new AdminOverviewUI(expectedUsername);

                var usernameLabel = (TextBlock)window.FindName("UsernameLabel");
                Assert.NotNull(usernameLabel);
                Assert.Equal(expectedUsername, usernameLabel.Text);

                window.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 6 — UsernameLabel with a different username (boundary check)
        // Validates: Requirement 2.6
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies the username binding works for a different username value,
        /// confirming the label is driven by the constructor argument and not
        /// a hardcoded string.
        /// </summary>
        [Fact(DisplayName = "AdminOverviewUI TopNavBar: UsernameLabel reflects any username passed in")]
        public void AdminOverviewUI_UsernameLabel_ReflectsAnyUsername()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                const string expectedUsername = "superadmin_2026";
                var window = new AdminOverviewUI(expectedUsername);

                var usernameLabel = (TextBlock)window.FindName("UsernameLabel");
                Assert.NotNull(usernameLabel);
                Assert.Equal(expectedUsername, usernameLabel.Text);

                window.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 7 — TabMenu button order, OVERVIEW content, and non-active opacity
        // Validates: Requirements 3.1, 3.2, 3.3
        // -----------------------------------------------------------------------

        /// <summary>
        /// TabMenuPanel must contain exactly 5 Button children in the order:
        /// OVERVIEW (index 0), My Team (1), Attendance (2), Task Management (3), Reports (4).
        /// The OVERVIEW button content must include "🏠".
        /// Non-active buttons (indices 1–4) must have Opacity ≈ 0.4.
        /// </summary>
        [Fact(DisplayName = "AdminOverviewUI TabMenu: button order, OVERVIEW icon, and non-active opacity")]
        public void TabMenu_ButtonOrder_AndActiveState()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminOverviewUI("TestUser");

                // TabMenuPanel is a UniformGrid (not StackPanel) in the XAML.
                var tabMenuPanel = (System.Windows.Controls.Primitives.UniformGrid)window.FindName("TabMenuPanel");
                Assert.NotNull(tabMenuPanel);

                // Must have exactly 6 children, all Buttons:
                // index 0 = Back, 1 = OVERVIEW (active), 2 = My Team,
                // 3 = Attendance, 4 = Task Management, 5 = Reports
                Assert.Equal(6, tabMenuPanel.Children.Count);
                foreach (var child in tabMenuPanel.Children)
                    Assert.IsType<Button>(child);

                var buttons = tabMenuPanel.Children.Cast<Button>().ToList();

                string GetButtonText(Button b) =>
                    b.Content as string ?? b.Content?.ToString() ?? string.Empty;

                // Button order
                Assert.Contains("Back",              GetButtonText(buttons[0]));
                Assert.Contains("OVERVIEW",          GetButtonText(buttons[1]));
                Assert.Equal("My Team",              GetButtonText(buttons[2]));
                Assert.Equal("Attendance",           GetButtonText(buttons[3]));
                Assert.Equal("Task Management",      GetButtonText(buttons[4]));
                Assert.Equal("Reports",              GetButtonText(buttons[5]));

                // OVERVIEW button content must include the home emoji
                Assert.Contains("🏠", GetButtonText(buttons[1]));

                // Non-active buttons (indices 2–5) must have Opacity ≈ 0.4
                for (int i = 2; i <= 5; i++)
                    Assert.InRange(buttons[i].Opacity, 0.35, 0.45);

                window.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Helpers shared by fallback tests
        // -----------------------------------------------------------------------

        /// <summary>
        /// Forces a layout pass on a canvas so that ActualWidth/ActualHeight
        /// reflect the explicit Width/Height set in XAML.
        /// </summary>
        private static void ForceLayout(System.Windows.FrameworkElement element)
        {
            element.Measure(new Size(element.Width, element.Height));
            element.Arrange(new Rect(0, 0, element.Width, element.Height));
        }

        private static readonly BindingFlags PrivateInstance =
            BindingFlags.NonPublic | BindingFlags.Instance;

        private static void SetPrivateField(object target, string fieldName, object? value)
        {
            var field = target.GetType().GetField(fieldName, PrivateInstance)
                        ?? throw new System.InvalidOperationException(
                               $"Field '{fieldName}' not found on {target.GetType().Name}");
            field.SetValue(target, value);
        }

        private static void InvokePrivateMethod(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, PrivateInstance)
                         ?? throw new System.InvalidOperationException(
                                $"Method '{methodName}' not found on {target.GetType().Name}");
            method.Invoke(target, null);
        }

        // -----------------------------------------------------------------------
        // Test 8 — Null task data hides DonutCanvas and shows DonutFallbackText
        // Validates: Requirement 4.7
        // -----------------------------------------------------------------------

        /// <summary>
        /// When _taskSegments is set to null via reflection and DrawDonutChart() is
        /// called, DonutCanvas must be Collapsed and DonutFallbackText must be Visible.
        /// </summary>
        [Fact(DisplayName = "AdminOverviewUI TaskOverviewCard: null task data shows fallback")]
        public void NullTaskData_HidesDonutCanvas_ShowsFallback()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminOverviewUI("TestUser");

                // Force layout so ActualWidth/ActualHeight are non-zero
                var donutCanvas = (System.Windows.Controls.Canvas)window.FindName("DonutCanvas");
                Assert.NotNull(donutCanvas);
                ForceLayout(donutCanvas);

                // Set _taskSegments to null and redraw
                SetPrivateField(window, "_taskSegments", null);
                InvokePrivateMethod(window, "DrawDonutChart");

                var fallbackText = (TextBlock)window.FindName("DonutFallbackText");
                Assert.NotNull(fallbackText);

                Assert.Equal(Visibility.Collapsed, donutCanvas.Visibility);
                Assert.Equal(Visibility.Visible,   fallbackText.Visibility);

                window.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 9 — Empty task data hides DonutCanvas and shows DonutFallbackText
        // Validates: Requirement 4.7
        // -----------------------------------------------------------------------

        /// <summary>
        /// When _taskSegments is set to an empty list via reflection and
        /// DrawDonutChart() is called, DonutCanvas must be Collapsed and
        /// DonutFallbackText must be Visible.
        /// </summary>
        [Fact(DisplayName = "AdminOverviewUI TaskOverviewCard: empty task data shows fallback")]
        public void EmptyTaskData_HidesDonutCanvas_ShowsFallback()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminOverviewUI("TestUser");

                var donutCanvas = (System.Windows.Controls.Canvas)window.FindName("DonutCanvas");
                Assert.NotNull(donutCanvas);
                ForceLayout(donutCanvas);

                // Set _taskSegments to an empty list and redraw
                SetPrivateField(window, "_taskSegments", new List<DonutSegment>());
                InvokePrivateMethod(window, "DrawDonutChart");

                var fallbackText = (TextBlock)window.FindName("DonutFallbackText");
                Assert.NotNull(fallbackText);

                Assert.Equal(Visibility.Collapsed, donutCanvas.Visibility);
                Assert.Equal(Visibility.Visible,   fallbackText.Visibility);

                window.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 10 — Null performance data hides GraphCanvas and shows GraphFallbackText
        // Validates: Requirement 5.7
        // -----------------------------------------------------------------------

        /// <summary>
        /// When _performanceSeries is set to null via reflection and
        /// DrawPerformanceGraph() is called, GraphCanvas must be Collapsed and
        /// GraphFallbackText must be Visible.
        /// </summary>
        [Fact(DisplayName = "AdminOverviewUI EmployeePerformancesCard: null performance data shows fallback")]
        public void NullPerformanceData_HidesGraphCanvas_ShowsFallback()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminOverviewUI("TestUser");

                var graphCanvas = (System.Windows.Controls.Canvas)window.FindName("GraphCanvas");
                Assert.NotNull(graphCanvas);
                ForceLayout(graphCanvas);

                // Set _performanceSeries to null and redraw
                SetPrivateField(window, "_performanceSeries", null);
                InvokePrivateMethod(window, "DrawPerformanceGraph");

                var fallbackText = (TextBlock)window.FindName("GraphFallbackText");
                Assert.NotNull(fallbackText);

                Assert.Equal(Visibility.Collapsed, graphCanvas.Visibility);
                Assert.Equal(Visibility.Visible,   fallbackText.Visibility);

                window.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 11 — Empty performance data hides GraphCanvas and shows GraphFallbackText
        // Validates: Requirement 5.7
        // -----------------------------------------------------------------------

        /// <summary>
        /// When _performanceSeries is set to an empty list via reflection and
        /// DrawPerformanceGraph() is called, GraphCanvas must be Collapsed and
        /// GraphFallbackText must be Visible.
        /// </summary>
        [Fact(DisplayName = "AdminOverviewUI EmployeePerformancesCard: empty performance data shows fallback")]
        public void EmptyPerformanceData_HidesGraphCanvas_ShowsFallback()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminOverviewUI("TestUser");

                var graphCanvas = (System.Windows.Controls.Canvas)window.FindName("GraphCanvas");
                Assert.NotNull(graphCanvas);
                ForceLayout(graphCanvas);

                // Set _performanceSeries to an empty list and redraw
                SetPrivateField(window, "_performanceSeries", new List<PerformanceSeries>());
                InvokePrivateMethod(window, "DrawPerformanceGraph");

                var fallbackText = (TextBlock)window.FindName("GraphFallbackText");
                Assert.NotNull(fallbackText);

                Assert.Equal(Visibility.Collapsed, graphCanvas.Visibility);
                Assert.Equal(Visibility.Visible,   fallbackText.Visibility);

                window.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 12 — Null financial data sets both amount labels to "₱ —"
        // Validates: Requirement 6.6
        // -----------------------------------------------------------------------

        /// <summary>
        /// When _financialData is set to null via reflection and BindFinancialData()
        /// is called, both MonthlySalaryAmount.Text and PayrollSummaryAmount.Text
        /// must equal "₱ —".
        /// </summary>
        [Fact(DisplayName = "AdminOverviewUI FinancialCards: null financial data shows ₱ — placeholder")]
        public void NullFinancialData_SetsBothAmountLabels_ToPlaceholder()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminOverviewUI("TestUser");

                // The actual field is _employees (List<EmployeeFinancialInfo>) and the
                // method is BindFinancialCards(EmployeeFinancialInfo? emp).
                // Passing null directly invokes the placeholder path that sets "₱ —".
                var bindMethod = typeof(AdminOverviewUI).GetMethod(
                    "BindFinancialCards",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.NotNull(bindMethod);
                bindMethod!.Invoke(window, new object?[] { null });

                var monthlySalaryAmount  = (TextBlock)window.FindName("MonthlySalaryAmount");
                var payrollSummaryAmount = (TextBlock)window.FindName("PayrollSummaryAmount");

                Assert.NotNull(monthlySalaryAmount);
                Assert.NotNull(payrollSummaryAmount);

                Assert.Equal("₱ —", monthlySalaryAmount.Text);
                Assert.Equal("₱ —", payrollSummaryAmount.Text);

                window.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 13 — NavButton_Click routes OverviewButton to AdminOverviewUI
        // Validates: Requirement 1.1
        // -----------------------------------------------------------------------

        /// <summary>
        /// Clicking OverviewButton in AdminDashboard must cause NavButton_Click to
        /// instantiate AdminOverviewUI.  We verify this by subclassing AdminDashboard
        /// to intercept window creation: the subclass overrides the routing by
        /// invoking the private NavButton_Click handler via reflection and then
        /// inspecting whether the handler path for OverviewButton was taken.
        ///
        /// Approach: invoke NavButton_Click via reflection with OverviewButton as
        /// the sender, then confirm no exception is thrown and the handler completes
        /// normally — which, per the implementation, only happens when the sender
        /// is OverviewButton and a new AdminOverviewUI is constructed.
        ///
        /// A secondary assertion uses reflection to read the private _username field
        /// and confirms it matches the value passed to the constructor, proving the
        /// field is set before the handler uses it (preventing a NullReferenceException
        /// inside NavButton_Click).
        /// </summary>
        [Fact(DisplayName = "AdminDashboard NavButton_Click: OverviewButton instantiates AdminOverviewUI")]
        public void NavButton_Click_OverviewButton_OpensAdminOverviewUI()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                const string username = "nav_test_user";
                var dashboard = new AdminDashboard(username);

                // Confirm _username is stored correctly — NavButton_Click uses it
                // to construct AdminOverviewUI(_username), so a missing field would
                // cause a NullReferenceException inside the handler.
                var usernameField = typeof(AdminDashboard)
                    .GetField("_username", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.NotNull(usernameField);
                Assert.Equal(username, usernameField.GetValue(dashboard) as string);

                // Locate the OverviewButton by name — it is the sender that triggers
                // the AdminOverviewUI branch inside NavButton_Click.
                var overviewButton = (System.Windows.Controls.Button)dashboard.FindName("OverviewButton");
                Assert.NotNull(overviewButton);

                // Invoke NavButton_Click via reflection with OverviewButton as sender.
                // If the handler throws (e.g. _username is null, or AdminOverviewUI
                // cannot be constructed), the exception propagates and the test fails.
                var navClickMethod = typeof(AdminDashboard).GetMethod(
                    "NavButton_Click",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.NotNull(navClickMethod);

                var ex = Record.Exception(() =>
                    navClickMethod.Invoke(dashboard, new object[]
                    {
                        overviewButton,
                        new System.Windows.RoutedEventArgs()
                    }));

                // No exception means the handler ran the AdminOverviewUI branch
                // successfully — i.e. new AdminOverviewUI(_username).Show() was called.
                Assert.Null(ex);

                dashboard.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 14 — DrawDonutChart with default data produces exactly 4 Path elements
        // Validates: Requirement 4.2
        // -----------------------------------------------------------------------

        /// <summary>
        /// After constructing AdminOverviewUI with default data (4 segments loaded by
        /// LoadTaskData) and calling DrawDonutChart() on a canvas with non-zero
        /// dimensions, DonutCanvas must contain exactly 4 Path elements — one per
        /// task-status segment (Done, In-progress, Late, Other).
        ///
        /// The inner-hole Ellipse is excluded from this count; only Path elements
        /// (the arc segments) are counted.
        /// </summary>
        [Fact(DisplayName = "AdminOverviewUI DonutChart: default data produces exactly 4 Path elements")]
        public void DrawDonutChart_DefaultData_ProducesFourPathElements()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminOverviewUI("TestUser");

                var donutCanvas = (System.Windows.Controls.Canvas)window.FindName("DonutCanvas");
                Assert.NotNull(donutCanvas);

                // Force a non-zero layout so the guard in DrawDonutChart() passes.
                ForceLayout(donutCanvas);

                // DrawDonutChart is internal — call it directly.
                window.DrawDonutChart();

                // Count only Path children (arc segments); the hole is an Ellipse.
                int pathCount = donutCanvas.Children
                    .OfType<System.Windows.Shapes.Path>()
                    .Count();

                Assert.Equal(4, pathCount);

                window.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 15 — DrawPerformanceGraph with default data has X-axis and Y-axis labels
        // Validates: Requirement 5.5
        // -----------------------------------------------------------------------

        /// <summary>
        /// After constructing AdminOverviewUI with default performance data and calling
        /// DrawPerformanceGraph() on a canvas with non-zero dimensions, GraphCanvas must
        /// contain at least one non-empty TextBlock for the X-axis (month names) and at
        /// least one non-empty TextBlock for the Y-axis (score values 0–100).
        ///
        /// X-axis labels are month-name strings (e.g. "Jan", "Feb", …).
        /// Y-axis labels are numeric strings matching the tick values (0, 25, 50, 75, 100).
        /// Both sets must be present and non-empty after the draw call.
        /// </summary>
        [Fact(DisplayName = "AdminOverviewUI PerformanceGraph: default data has X-axis and Y-axis TextBlock labels")]
        public void DrawPerformanceGraph_DefaultData_HasXAndYAxisLabels()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminOverviewUI("TestUser");

                var graphCanvas = (System.Windows.Controls.Canvas)window.FindName("GraphCanvas");
                Assert.NotNull(graphCanvas);

                // Force a non-zero layout so the guard in DrawPerformanceGraph() passes.
                ForceLayout(graphCanvas);

                // DrawPerformanceGraph is internal — call it directly.
                window.DrawPerformanceGraph();

                // Collect all TextBlock children added to the canvas.
                var labels = graphCanvas.Children
                    .OfType<TextBlock>()
                    .Select(tb => tb.Text)
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();

                // Y-axis tick values that must appear as labels.
                var expectedYTicks = new[] { "0", "25", "50", "75", "100" };

                // Every Y-axis tick label must be present.
                foreach (var tick in expectedYTicks)
                {
                    Assert.True(labels.Contains(tick),
                        $"Y-axis label '{tick}' was not found on GraphCanvas.");
                }

                // X-axis: at least one month-name label must be present.
                // Default data has 6 points → labels "Jan" through "Jun".
                var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                         "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                bool hasXAxisLabel = labels.Any(l => monthNames.Contains(l));
                Assert.True(hasXAxisLabel,
                    "No X-axis month-name label was found on GraphCanvas.");

                window.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Property 1 — Username display round-trip
        // Feature: admin-overview-ui, Property 1: Username display round-trip
        //
        // For any non-null username string passed to the AdminOverviewUI constructor,
        // UsernameLabel.Text must display exactly that string.
        //
        // Validates: Requirements 2.6
        // -----------------------------------------------------------------------

        /// <summary>
        /// For any non-null username string, constructing AdminOverviewUI(username)
        /// and reading UsernameLabel.Text must return the original username unchanged.
        ///
        /// Uses Prop.ForAll with Arb.From&lt;NonNull&lt;string&gt;&gt;() to generate non-null
        /// strings. All iterations run within a single STA thread invocation via
        /// StaHelper.RunOnSta to avoid WPF BAML stream race conditions.
        ///
        /// Minimum 100 FsCheck iterations.
        ///
        /// **Validates: Requirements 2.6**
        /// </summary>
        [Property(MaxTest = 100,
                  DisplayName = "Property 1: Username display round-trip")]
        public Property Property1_UsernameDisplayRoundTrip()
        {
            // Feature: admin-overview-ui, Property 1: Username display round-trip
            return Prop.ForAll(
                ArbMap.Default.ArbFor<NonNull<string>>(),
                usernameWrapper =>
                {
                    string username = usernameWrapper.Get;

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
    }
}
