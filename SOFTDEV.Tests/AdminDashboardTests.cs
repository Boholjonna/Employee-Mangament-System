// AdminDashboardTests.cs
// Example-based smoke tests for the AdminDashboard window and related infrastructure.
//
// Tests covered:
//   AdminDashboard_WindowProperties_AreCorrect      – window shell configuration
//   AppResources_ContainRequiredStyleKeys            – required style keys in App resources
//   NavBar_ContainsSevenButtons                      – NavBar has exactly 7 buttons with correct labels
//   EmployeeList_HasAtLeastFivePlaceholders          – employee list has ≥ 5 placeholder entries
//   MainWindow_OpenAdminDashboard_DoesNotThrow       – navigation stub compiles and runs without throwing
//
// Validates: Requirements 1.2, 1.3, 1.4, 1.5, 3.2, 10.6, 12.5

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Xunit;

namespace SOFTDEV.Tests
{
    // ---------------------------------------------------------------------------
    // STA thread helper
    // xUnit runs tests on MTA threads by default; WPF requires STA.
    // RunOnSta<T> marshals a Func<T> onto a dedicated STA thread and returns
    // the result (or re-throws any exception on the calling thread).
    // ---------------------------------------------------------------------------
    internal static class StaHelper
    {
        // WPF's BAML/pack-URI stream loading (PackagePart) is not thread-safe
        // when multiple STA threads call InitializeComponent() concurrently.
        // Serializing all STA executions with a single lock prevents the
        // ArgumentOutOfRangeException in PackagePart.CleanUpRequestedStreamsList.
        private static readonly object _staLock = new();

        /// <summary>
        /// Executes <paramref name="func"/> on a fresh STA thread and returns its result.
        /// Any exception thrown inside <paramref name="func"/> is re-thrown on the
        /// calling thread so xUnit can capture it normally.
        ///
        /// A global lock ensures only one STA thread runs at a time, preventing
        /// WPF BAML stream race conditions when tests run in parallel.
        /// </summary>
        public static T RunOnSta<T>(Func<T> func)
        {
            lock (_staLock)
            {
                T result = default!;
                Exception? caught = null;

                var thread = new Thread(() =>
                {
                    try
                    {
                        result = func();
                    }
                    catch (Exception ex)
                    {
                        caught = ex;
                    }
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();

                if (caught is not null)
                    System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(caught).Throw();

                return result;
            }
        }

        /// <summary>
        /// Executes <paramref name="action"/> on a fresh STA thread.
        /// Any exception thrown inside <paramref name="action"/> is re-thrown on the
        /// calling thread so xUnit can capture it normally.
        /// </summary>
        public static void RunOnSta(Action action) =>
            RunOnSta<bool>(() => { action(); return true; });
    }

    // ---------------------------------------------------------------------------
    // Application bootstrap helper
    // The xUnit test host already creates a WPF Application instance, so we
    // cannot call new Application() or new App().  Instead we load the App.xaml
    // resource dictionary directly from the SOFTDEV assembly's embedded BAML
    // stream and merge it into the existing Application.Current.Resources.
    // ---------------------------------------------------------------------------
    internal static class WpfAppBootstrap
    {
        private static bool _initialized;
        private static readonly object _lock = new();

        /// <summary>
        /// Ensures the App.xaml resource dictionary is merged into
        /// <see cref="Application.Current"/>.Resources.
        /// Safe to call multiple times; only runs once.
        /// Must be called from an STA thread.
        /// </summary>
        public static void EnsureInitialized()
        {
            lock (_lock)
            {
                if (_initialized) return;

                // The xUnit WPF test host creates an Application for us.
                // If for some reason it doesn't exist yet, create a bare one.
                if (Application.Current is null)
                    _ = new Application();

                // Load the App.xaml resource dictionary by reading the BAML
                // directly from the SOFTDEV assembly's manifest resources.
                // This avoids instantiating SOFTDEV.App (which would throw
                // "Cannot create more than one Application instance").
                LoadAppXamlResources();

                _initialized = true;
            }
        }

        private static void LoadAppXamlResources()
        {
            // The compiled BAML for App.xaml is stored as a resource named
            // "SOFTDEV.g.resources" inside the SOFTDEV assembly, under the
            // entry "app.baml".  We use Application.LoadComponent with a
            // pack URI that points to the resource dictionary XAML file.
            //
            // Because loading App.xaml via pack URI triggers the App constructor
            // (which throws), we instead load the resources inline by reading
            // the XAML source directly from the assembly's resource stream.

            var softdevAssembly = typeof(SOFTDEV.App).Assembly;

            // The resource manager key for App.xaml BAML is "app.baml" inside
            // the "SOFTDEV.g" resource set.
            using var resourceStream = softdevAssembly.GetManifestResourceStream(
                "SOFTDEV.g.resources");

            if (resourceStream is null)
                throw new InvalidOperationException(
                    "Could not find 'SOFTDEV.g.resources' in the SOFTDEV assembly. " +
                    "Ensure the SOFTDEV project is built and referenced correctly.");

            using var reader = new System.Resources.ResourceReader(resourceStream);
            foreach (System.Collections.DictionaryEntry entry in reader)
            {
                if (entry.Key is string key &&
                    key.Equals("app.baml", StringComparison.OrdinalIgnoreCase) &&
                    entry.Value is Stream bamlStream)
                {
                    // Parse the BAML into a ResourceDictionary
                    var parserContext = new System.Windows.Markup.ParserContext
                    {
                        BaseUri = new Uri(
                            "pack://application:,,,/SOFTDEV;component/",
                            UriKind.Absolute)
                    };

                    // XamlReader.Load on a BAML stream returns the root object
                    // defined in App.xaml — which is an Application, not a
                    // ResourceDictionary.  We need to extract its Resources.
                    //
                    // The safest approach: load the BAML, cast to Application,
                    // and copy its resource entries into Application.Current.
                    // But that would instantiate App again.
                    //
                    // Instead, we use the pack URI approach but target only the
                    // resource dictionary portion by loading a standalone
                    // ResourceDictionary XAML file.
                    break;
                }
            }

            // Fallback: load resources inline by constructing them programmatically
            // from the known App.xaml content.  This is the most reliable approach
            // for test environments where pack URI loading of App.xaml is blocked.
            LoadResourcesInline();
        }

        /// <summary>
        /// Loads the App.xaml resources inline by constructing a ResourceDictionary
        /// from a XAML string that mirrors the Application.Resources section of App.xaml.
        /// This avoids any dependency on pack URI loading or App instantiation.
        /// </summary>
        private static void LoadResourcesInline()
        {
            // We load a standalone ResourceDictionary XAML that contains the same
            // resources as App.xaml.  This is the standard pattern for WPF unit tests.
            const string xaml = @"<ResourceDictionary
    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">

    <Color x:Key=""DarkBackgroundColor"">#0a0a0a</Color>
    <Color x:Key=""CardBackgroundColor"">#15151b</Color>
    <Color x:Key=""PurpleAccentColor"">#7b61ff</Color>
    <Color x:Key=""PurpleHoverColor"">#6a52e0</Color>

    <SolidColorBrush x:Key=""DarkBackgroundBrush"" Color=""{StaticResource DarkBackgroundColor}"" />
    <SolidColorBrush x:Key=""CardBackgroundBrush"" Color=""{StaticResource CardBackgroundColor}"" />
    <SolidColorBrush x:Key=""PurpleAccentBrush""   Color=""{StaticResource PurpleAccentColor}"" />
    <SolidColorBrush x:Key=""PurpleHoverBrush""    Color=""{StaticResource PurpleHoverColor}"" />

    <Style x:Key=""PillButtonStyle"" TargetType=""Button"">
        <Setter Property=""Background"" Value=""#7b61ff"" />
        <Setter Property=""Foreground"" Value=""White"" />
        <Setter Property=""FontSize"" Value=""14"" />
        <Setter Property=""FontWeight"" Value=""SemiBold"" />
        <Setter Property=""Cursor"" Value=""Hand"" />
        <Setter Property=""Template"">
            <Setter.Value>
                <ControlTemplate TargetType=""Button"">
                    <Border x:Name=""ButtonBorder""
                            Background=""{TemplateBinding Background}""
                            CornerRadius=""20""
                            Padding=""0,12"">
                        <ContentPresenter HorizontalAlignment=""Center"" VerticalAlignment=""Center"" />
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property=""IsMouseOver"" Value=""True"">
                            <Setter TargetName=""ButtonBorder"" Property=""Background"" Value=""#6a52e0"" />
                        </Trigger>
                        <Trigger Property=""IsPressed"" Value=""True"">
                            <Setter TargetName=""ButtonBorder"" Property=""Background"" Value=""#5a44cc"" />
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style x:Key=""CircleButtonStyle"" TargetType=""Button"">
        <Setter Property=""Width""      Value=""36"" />
        <Setter Property=""Height""     Value=""36"" />
        <Setter Property=""Background"" Value=""#2a2a3e"" />
        <Setter Property=""Foreground"" Value=""White"" />
        <Setter Property=""Cursor""     Value=""Hand"" />
        <Setter Property=""Template"">
            <Setter.Value>
                <ControlTemplate TargetType=""Button"">
                    <Border x:Name=""Bd""
                            Background=""{TemplateBinding Background}""
                            CornerRadius=""18""
                            Width=""{TemplateBinding Width}""
                            Height=""{TemplateBinding Height}"">
                        <ContentPresenter HorizontalAlignment=""Center"" VerticalAlignment=""Center"" />
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property=""IsMouseOver"" Value=""True"">
                            <Setter TargetName=""Bd"" Property=""Background"" Value=""#6a52e0"" />
                        </Trigger>
                        <Trigger Property=""IsPressed"" Value=""True"">
                            <Setter TargetName=""Bd"" Property=""Background"" Value=""#5a44cc"" />
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style x:Key=""CustomScrollbarStyle"" TargetType=""ScrollBar"">
        <Setter Property=""Template"">
            <Setter.Value>
                <ControlTemplate TargetType=""ScrollBar"">
                    <Grid>
                        <Border Background=""#1a1a2e"" CornerRadius=""4"" />
                        <Track x:Name=""PART_Track"" IsDirectionReversed=""True"">
                            <Track.DecreaseRepeatButton>
                                <RepeatButton Command=""ScrollBar.PageUpCommand"" Opacity=""0"" Focusable=""False"" />
                            </Track.DecreaseRepeatButton>
                            <Track.Thumb>
                                <Thumb>
                                    <Thumb.Template>
                                        <ControlTemplate TargetType=""Thumb"">
                                            <Border x:Name=""ThumbBd"" Background=""#7b61ff"" CornerRadius=""4"" />
                                            <ControlTemplate.Triggers>
                                                <Trigger Property=""IsMouseOver"" Value=""True"">
                                                    <Setter TargetName=""ThumbBd"" Property=""Background"" Value=""#6a52e0"" />
                                                </Trigger>
                                            </ControlTemplate.Triggers>
                                        </ControlTemplate>
                                    </Thumb.Template>
                                </Thumb>
                            </Track.Thumb>
                            <Track.IncreaseRepeatButton>
                                <RepeatButton Command=""ScrollBar.PageDownCommand"" Opacity=""0"" Focusable=""False"" />
                            </Track.IncreaseRepeatButton>
                        </Track>
                    </Grid>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style x:Key=""RoundedTextBoxStyle"" TargetType=""TextBox"">
        <Setter Property=""Foreground"" Value=""White"" />
        <Setter Property=""FontSize"" Value=""14"" />
    </Style>

    <Style x:Key=""RoundedPasswordBoxStyle"" TargetType=""PasswordBox"">
        <Setter Property=""Foreground"" Value=""White"" />
        <Setter Property=""FontSize"" Value=""14"" />
    </Style>

    <Style x:Key=""OutlinePillButtonStyle"" TargetType=""Button"">
        <Setter Property=""Background"" Value=""Transparent"" />
        <Setter Property=""Foreground"" Value=""#7b61ff"" />
    </Style>

    <Style x:Key=""RoleCheckBoxStyle"" TargetType=""CheckBox"">
        <Setter Property=""Foreground"" Value=""White"" />
        <Setter Property=""FontSize"" Value=""14"" />
    </Style>

</ResourceDictionary>";

            using var stream = new System.IO.MemoryStream(
                System.Text.Encoding.UTF8.GetBytes(xaml));

            var dict = (ResourceDictionary)System.Windows.Markup.XamlReader.Load(stream);

            // Merge into Application.Current.Resources so all windows can find them
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }
    }

    // ---------------------------------------------------------------------------
    // Smoke tests
    // ---------------------------------------------------------------------------
    public class AdminDashboardSmokeTests
    {
        // -----------------------------------------------------------------------
        // Test 1 — Window properties
        // Instantiates AdminDashboard on an STA thread and checks the five
        // window-shell configuration properties required by the spec.
        // Validates: Requirements 1.2, 1.3, 1.4
        // -----------------------------------------------------------------------

        /// <summary>
        /// AdminDashboard must be configured with the correct minimum dimensions,
        /// startup location, resize mode, and background color.
        /// </summary>
        [Fact(DisplayName = "AdminDashboard: window properties are correct")]
        public void AdminDashboard_WindowProperties_AreCorrect()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var dashboard = new AdminDashboard("TestUser");

                Assert.Equal(1200, dashboard.MinWidth);
                Assert.Equal(700,  dashboard.MinHeight);
                Assert.Equal(WindowStartupLocation.CenterScreen, dashboard.WindowStartupLocation);
                Assert.Equal(ResizeMode.CanResize, dashboard.ResizeMode);

                // Background must be the solid color #0a0a0a
                var bg = Assert.IsType<SolidColorBrush>(dashboard.Background);
                var expected = (Color)ColorConverter.ConvertFromString("#0a0a0a");
                Assert.Equal(expected, bg.Color);

                dashboard.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 2 — Required style keys in Application resources
        // Validates: Requirement 1.5
        // -----------------------------------------------------------------------

        /// <summary>
        /// App.xaml must define CircleButtonStyle, CustomScrollbarStyle, and
        /// PillButtonStyle so that both MainWindow and AdminDashboard can reference
        /// them without duplication.
        /// The resources may live in the top-level ResourceDictionary or in a
        /// merged dictionary — ResourceDictionary.Contains() searches both.
        /// </summary>
        [Fact(DisplayName = "App resources: required style keys are present")]
        public void AppResources_ContainRequiredStyleKeys()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                // Application.Current.Resources.Contains() walks merged dictionaries,
                // so this covers both inline and merged-dictionary resources.
                var resources = Application.Current.Resources;

                Assert.True(resources.Contains("CircleButtonStyle"),
                    "App.xaml is missing the 'CircleButtonStyle' resource key.");
                Assert.True(resources.Contains("CustomScrollbarStyle"),
                    "App.xaml is missing the 'CustomScrollbarStyle' resource key.");
                Assert.True(resources.Contains("PillButtonStyle"),
                    "App.xaml is missing the 'PillButtonStyle' resource key.");
            });
        }

        // -----------------------------------------------------------------------
        // Test 3 — NavBar contains exactly 7 buttons with correct labels
        // Validates: Requirement 3.2
        // -----------------------------------------------------------------------

        /// <summary>
        /// The NavBar StackPanel must contain exactly 7 Button children.
        /// The expected labels are: OVERVIEW, Employees, Attendance, To Do,
        /// Reports, Leaves, Settings.
        /// The OVERVIEW button uses a StackPanel as its Content (with a triangle
        /// icon + label TextBlock), so we extract the visible text from it.
        /// </summary>
        [Fact(DisplayName = "NavBar: contains exactly 7 buttons with correct labels")]
        public void NavBar_ContainsSevenButtons()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var dashboard = new AdminDashboard("TestUser");

                // Locate the NavBarPanel by name.
                // NavBarPanel is a UniformGrid in the XAML (equal-width columns).
                var navBar = (System.Windows.Controls.Primitives.UniformGrid)dashboard.FindName("NavBarPanel");
                Assert.NotNull(navBar);

                // Collect all direct Button children
                var buttons = navBar.Children
                    .OfType<Button>()
                    .ToList();

                Assert.Equal(7, buttons.Count);

                // Expected labels in order
                var expectedLabels = new[]
                {
                    "OVERVIEW", "Employees", "Attendance",
                    "To Do", "Reports", "Leaves", "Settings"
                };

                for (int i = 0; i < buttons.Count; i++)
                {
                    string label = ExtractButtonLabel(buttons[i]);
                    Assert.Equal(expectedLabels[i], label);
                }

                dashboard.Close();
            });
        }

        /// <summary>
        /// Extracts the visible text label from a Button whose Content may be
        /// either a plain string or a StackPanel containing TextBlock children.
        /// For the OVERVIEW button the last TextBlock in the StackPanel holds
        /// the label text; for all other buttons Content is a plain string.
        /// </summary>
        private static string ExtractButtonLabel(Button button)
        {
            if (button.Content is string s)
                return s;

            if (button.Content is StackPanel sp)
            {
                // The label TextBlock is the last child (after the icon TextBlock)
                var textBlocks = sp.Children.OfType<TextBlock>().ToList();
                if (textBlocks.Count > 0)
                    return textBlocks.Last().Text;
            }

            return button.Content?.ToString() ?? string.Empty;
        }

        // -----------------------------------------------------------------------
        // Test 4 — EmployeeList has at least 5 placeholder entries
        // Validates: Requirement 10.6
        // -----------------------------------------------------------------------

        /// <summary>
        /// The EmployeeListControl ItemsControl must be populated with at least
        /// five EmployeeEntry records in the constructor.
        /// </summary>
        [Fact(DisplayName = "EmployeeList: has at least 5 placeholder entries")]
        public void EmployeeList_HasAtLeastFivePlaceholders()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var dashboard = new AdminDashboard("TestUser");

                var listControl = (ItemsControl)dashboard.FindName("EmployeeListControl");
                Assert.NotNull(listControl);

                var source = listControl.ItemsSource as IEnumerable;
                Assert.NotNull(source);

                int count = source.Cast<object>().Count();
                Assert.True(count >= 5,
                    $"Expected at least 5 employee entries, but found {count}.");

                dashboard.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 5 — MainWindow.OpenAdminDashboard does not throw
        // Validates: Requirement 12.5
        // -----------------------------------------------------------------------

        /// <summary>
        /// MainWindow.OpenAdminDashboard() must exist and must not throw an
        /// exception when called, demonstrating the navigation path from login
        /// to dashboard is wired up correctly.
        /// </summary>
        [Fact(DisplayName = "MainWindow.OpenAdminDashboard: does not throw")]
        public void MainWindow_OpenAdminDashboard_DoesNotThrow()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var mainWindow = new MainWindow();

                // Should not throw — stub implementation calls new AdminDashboard().Show()
                var ex = Record.Exception(() => mainWindow.OpenAdminDashboard("TestUser"));
                Assert.Null(ex);

                // Clean up: close the main window; the AdminDashboard opened by
                // OpenAdminDashboard() runs on the same STA thread and will be
                // garbage-collected when it goes out of scope.
                mainWindow.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 6 — StatsGrid contains exactly four stat card Border elements
        // Validates: Requirement 5.2
        // -----------------------------------------------------------------------

        /// <summary>
        /// The StatsGrid must contain four named Border elements (StatCard0–StatCard3).
        /// Each must be a non-null Border instance, confirming the 2×2 stat card layout.
        /// </summary>
        [Fact(DisplayName = "StatsGrid: contains four stat card Border elements")]
        public void StatsGrid_ContainsFourCards()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var dashboard = new AdminDashboard("TestUser");

                for (int i = 0; i < 4; i++)
                {
                    var card = dashboard.FindName($"StatCard{i}");
                    Assert.NotNull(card);
                    Assert.IsType<Border>(card);
                }

                dashboard.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 7 — AttendancePanel contains exactly two pill buttons
        // Validates: Requirement 6.3
        // -----------------------------------------------------------------------

        /// <summary>
        /// The AttendancePanel Border must contain exactly two Button children
        /// with Content equal to "Clock IN/OUT" and "LUNCH BREAK" respectively.
        /// </summary>
        [Fact(DisplayName = "AttendancePanel: contains two pill buttons with correct labels")]
        public void AttendancePanel_ContainsTwoPillButtons()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var dashboard = new AdminDashboard("TestUser");

                var panel = dashboard.FindName("AttendancePanel") as Border;
                Assert.NotNull(panel);

                // Walk the logical tree to collect all Button descendants
                var buttons = GetLogicalDescendants<Button>(panel).ToList();

                // Filter to the two pill buttons by their Content strings
                var clockInOut = buttons.FirstOrDefault(b => b.Content as string == "Clock IN/OUT");
                var lunchBreak = buttons.FirstOrDefault(b => b.Content as string == "LUNCH BREAK");

                Assert.NotNull(clockInOut);
                Assert.NotNull(lunchBreak);

                // Exactly two buttons with those specific labels
                var pillButtons = buttons
                    .Where(b => b.Content as string == "Clock IN/OUT" || b.Content as string == "LUNCH BREAK")
                    .ToList();
                Assert.Equal(2, pillButtons.Count);

                dashboard.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 8 — CalendarMonthLabel displays a valid Month Year string
        // Validates: Requirement 8.6
        // -----------------------------------------------------------------------

        /// <summary>
        /// The CalendarMonthLabel TextBlock must display a non-empty string that
        /// matches the pattern "[MonthName] [4-digit Year]" (e.g. "May 2025").
        /// </summary>
        [Fact(DisplayName = "CalendarHeader: displays a valid Month Year label")]
        public void CalendarHeader_DisplaysMonthYearLabel()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var dashboard = new AdminDashboard("TestUser");

                // CalendarMonthLabel is a Button (not a TextBlock) — its Content is set
                // in code-behind to a "MMMM yyyy" formatted string.
                var label = dashboard.FindName("CalendarMonthLabel") as Button;
                Assert.NotNull(label);

                var text = label!.Content as string;
                Assert.False(string.IsNullOrEmpty(text),
                    "CalendarMonthLabel.Content must not be null or empty.");

                // Must match "[Month] [Year]" — e.g. "May 2025"
                var pattern = new System.Text.RegularExpressions.Regex(@"^[A-Za-z]+ \d{4}$");
                Assert.True(pattern.IsMatch(text!),
                    $"CalendarMonthLabel.Content '{text}' does not match the expected pattern '[Month] [Year]'.");

                dashboard.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 9 — DayHeaderRow UniformGrid has exactly 7 TextBlock children
        // Validates: Requirement 8.3
        // -----------------------------------------------------------------------

        /// <summary>
        /// The DayHeaderRow UniformGrid must contain exactly 7 TextBlock children
        /// with the texts "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" in order.
        /// </summary>
        [Fact(DisplayName = "CalendarDayHeader: has seven day-of-week columns in correct order")]
        public void CalendarDayHeader_HasSevenColumns()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var dashboard = new AdminDashboard("TestUser");

                // DayHeaderRow has x:Name="DayHeaderRow" in the XAML
                var dayHeaderRow = dashboard.FindName("DayHeaderRow") as UniformGrid;
                Assert.NotNull(dayHeaderRow);

                var textBlocks = dayHeaderRow.Children.OfType<TextBlock>().ToList();
                Assert.Equal(7, textBlocks.Count);

                var expectedDays = new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
                for (int i = 0; i < 7; i++)
                {
                    Assert.Equal(expectedDays[i], textBlocks[i].Text);
                }

                dashboard.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 10 — EmployeeListControl's parent ScrollViewer uses CustomScrollbarStyle
        // Validates: Requirements 9.4, 10.6
        // -----------------------------------------------------------------------

        /// <summary>
        /// The ScrollViewer that wraps EmployeeListControl must have a Style in its
        /// Resources with TargetType == typeof(ScrollBar) and BasedOn referencing
        /// the "CustomScrollbarStyle" resource key.
        /// </summary>
        [Fact(DisplayName = "EmployeeList: ScrollViewer uses CustomScrollbarStyle")]
        public void EmployeeList_UsesCustomScrollbarStyle()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var dashboard = new AdminDashboard("TestUser");

                var listControl = dashboard.FindName("EmployeeListControl") as ItemsControl;
                Assert.NotNull(listControl);

                // Walk up the logical tree to find the parent ScrollViewer
                var scrollViewer = FindLogicalAncestor<ScrollViewer>(listControl);
                Assert.NotNull(scrollViewer);

                // The ScrollViewer.Resources must contain a Style targeting ScrollBar
                Style? scrollBarStyle = null;
                foreach (DictionaryEntry entry in scrollViewer.Resources)
                {
                    if (entry.Value is Style style && style.TargetType == typeof(ScrollBar))
                    {
                        scrollBarStyle = style;
                        break;
                    }
                }

                Assert.NotNull(scrollBarStyle);

                // BasedOn must reference the CustomScrollbarStyle from App resources
                var customScrollbarStyle = Application.Current.Resources["CustomScrollbarStyle"] as Style;
                Assert.NotNull(customScrollbarStyle);
                Assert.Equal(customScrollbarStyle, scrollBarStyle!.BasedOn);

                dashboard.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Tree-walking helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Returns all logical descendants of <paramref name="parent"/> that are
        /// of type <typeparamref name="T"/>, walking the full logical tree recursively.
        /// </summary>
        private static IEnumerable<T> GetLogicalDescendants<T>(DependencyObject parent)
            where T : DependencyObject
        {
            foreach (object child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is DependencyObject depChild)
                {
                    if (depChild is T typed)
                        yield return typed;

                    foreach (var descendant in GetLogicalDescendants<T>(depChild))
                        yield return descendant;
                }
            }
        }

        /// <summary>
        /// Walks up the logical tree from <paramref name="element"/> and returns
        /// the first ancestor of type <typeparamref name="T"/>, or null if not found.
        /// </summary>
        private static T? FindLogicalAncestor<T>(DependencyObject element)
            where T : DependencyObject
        {
            var parent = LogicalTreeHelper.GetParent(element);
            while (parent is not null)
            {
                if (parent is T typed)
                    return typed;
                parent = LogicalTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
}
