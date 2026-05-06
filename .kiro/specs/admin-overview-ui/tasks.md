# Implementation Plan: AdminOverviewUI

## Overview

Implement the `AdminOverviewUI` WPF Window in C# for the SOFTDEV application. The work is split into four incremental steps: (1) update `AdminDashboard` to store the username and route the OVERVIEW button click, (2) create the XAML layout with TopNavBar, TabMenu, and the three-column content grid, (3) implement the code-behind with data models and chart-drawing logic, and (4) wire everything together and add tests. Each step builds directly on the previous one so there is no orphaned code.

## Tasks

- [x] 1. Update `AdminDashboard.xaml.cs` — store username and wire OVERVIEW navigation
  - Add a `private string _username;` field to `AdminDashboard`.
  - In the constructor, assign `_username = username;` before any other use.
  - Update `NavButton_Click` to check `if (sender == OverviewButton)` and call `new AdminOverviewUI(_username).Show();`.
  - No other nav-button branches need to be added at this stage.
  - _Requirements: 1.1_

- [x] 2. Create `AdminOverviewUI.xaml` — full window layout
  - [x] 2.1 Scaffold the Window element and root Grid
    - Declare `x:Class="SOFTDEV.AdminOverviewUI"`, `Title="Admin Overview"`, `MinWidth="1200"`, `MinHeight="700"`, `WindowStartupLocation="CenterScreen"`, `Background="{StaticResource DarkBackgroundBrush}"`, `FontFamily="Segoe UI"`.
    - Define a root `Grid` with two rows: `Height="Auto"` (header) and `Height="*"` (content).
    - _Requirements: 1.2, 1.3, 1.4_

  - [x] 2.2 Add TopNavBar (Row 0, part 1)
    - Inside Row 0, add a `StackPanel Orientation="Vertical"`.
    - Inside it, add a `DockPanel` for the TopNavBar.
    - Dock a `TextBlock` to the left: `Text="COMPANY NAME/LOGO"`, `Foreground="{StaticResource PurpleAccentBrush}"`, `FontWeight="Bold"`, `FontSize="18"`.
    - Dock a `StackPanel Orientation="Horizontal"` to the right containing:
      - `Button x:Name="SearchButton"` with `Style="{StaticResource CircleButtonStyle}"` and `Content="🔍"`.
      - `Button x:Name="NotificationButton"` with `Style="{StaticResource CircleButtonStyle}"` and `Content="🔔"`.
      - `Border x:Name="AvatarBorder"` with `Width="36"`, `Height="36"`, `CornerRadius="18"`, `Background="{StaticResource PurpleAccentBrush}"`, inner `TextBlock Text="👤"`.
      - `TextBlock x:Name="UsernameLabel"` with `Foreground="White"` and `VerticalAlignment="Center"`.
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6_

  - [x] 2.3 Add TabMenu (Row 0, part 2)
    - Below the TopNavBar `DockPanel`, add a horizontal `StackPanel x:Name="TabMenuPanel"`.
    - Add five `Button` elements using `PillButtonStyle`:
      - `x:Name="OverviewTabButton"` — `Content="🏠 OVERVIEW"`, `Background="{StaticResource PurpleAccentBrush}"`, `Opacity="1.0"`.
      - `x:Name="MyTeamTabButton"` — `Content="My Team"`, `Opacity="0.4"`.
      - `x:Name="AttendanceTabButton"` — `Content="Attendance"`, `Opacity="0.4"`.
      - `x:Name="TaskManagementTabButton"` — `Content="Task Management"`, `Opacity="0.4"`.
      - `x:Name="ReportsTabButton"` — `Content="Reports"`, `Opacity="0.4"`.
    - Inactive buttons use `Background="{StaticResource PurpleAccentBrush}"` with `Opacity="0.4"`; add an `IsMouseOver` trigger that sets `Opacity="1.0"` and `Background="{StaticResource PurpleHoverBrush}"`.
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6_

  - [x] 2.4 Add main content ScrollViewer and three-column Grid (Row 1)
    - In Row 1, add a `ScrollViewer VerticalScrollBarVisibility="Auto"` with `CustomScrollbarStyle` applied to its inner `ScrollBar`.
    - Inside the `ScrollViewer`, add a `Grid x:Name="MainContentGrid"` with three `ColumnDefinition` entries: `Width="1.1*"`, `Width="1.4*"`, `Width="0.9*"`.
    - _Requirements: 8.1, 8.2, 8.3_

  - [x] 2.5 Add TaskOverviewCard (Column 0)
    - In Column 0, add a `Border x:Name="TaskOverviewCard"` with `Background="{StaticResource CardBackgroundBrush}"`, `CornerRadius="18"`, and a `DropShadowEffect` (`BlurRadius="15"`, `Opacity="0.4"`, `Color="Black"`).
    - Inside, add a `StackPanel` containing:
      - `TextBlock Text="Task Overview"` (title, white, bold).
      - `Canvas x:Name="DonutCanvas"` (sized, e.g. `Width="200"` `Height="200"`).
      - `TextBlock x:Name="DonutFallbackText" Text="No data available" Visibility="Collapsed"`.
      - `StackPanel x:Name="LegendPanel"` (for legend rows).
    - _Requirements: 4.1, 4.2, 4.6, 4.7_

  - [x] 2.6 Add EmployeePerformancesCard (Column 1)
    - In Column 1, add a `Border x:Name="EmployeePerformancesCard"` with `Background="{StaticResource CardBackgroundBrush}"`, `CornerRadius="18"`, and a `DropShadowEffect` (`BlurRadius="15"`, `Opacity="0.4"`, `Color="Black"`).
    - Inside, add a `StackPanel` containing:
      - `TextBlock Text="Employee Performances"` (title, white, bold).
      - `Canvas x:Name="GraphCanvas"` (sized, e.g. `Width="400"` `Height="250"`).
      - `TextBlock x:Name="GraphFallbackText" Text="No data available" Visibility="Collapsed"`.
    - _Requirements: 5.1, 5.2, 5.6, 5.7_

  - [x] 2.7 Add FinancialCards (Column 2)
    - In Column 2, add a vertical `StackPanel x:Name="FinancialCardsPanel"`.
    - Add two `Border` cards (`x:Name="MonthlySalaryCard"` and `x:Name="PayrollSummaryCard"`), each with `Background="{StaticResource CardBackgroundBrush}"`, `CornerRadius="18"`, and a `DropShadowEffect` (`BlurRadius="15"`, `Opacity="0.4"`, `Color="Black"`).
    - Each card contains a horizontal `StackPanel` with:
      - A circular `Border` (`Width="48"`, `Height="48"`, `CornerRadius="24"`, `Background="{StaticResource PurpleAccentBrush}"`) containing `TextBlock Text="₱"` (`Foreground="White"`, `FontSize="20"`, `FontWeight="Bold"`, centered).
      - A vertical `StackPanel` (`Margin="12,0,0,0"`) with a title `TextBlock` (`Foreground="#aaaaaa"`, `FontSize="13"`) and an amount `TextBlock` (`x:Name="MonthlySalaryAmount"` / `x:Name="PayrollSummaryAmount"`, `Foreground="White"`, `FontSize="22"`, `FontWeight="Bold"`).
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_

- [x] 3. Create `AdminOverviewUI.xaml.cs` — data models, constructor, and chart drawing
  - [x] 3.1 Define data model classes and constructor
    - In the `SOFTDEV` namespace, define `DonutSegment` (`Label`, `Percentage`, `Color`), `PerformanceSeries` (`Label`, `Color`, `List<Point> Points`), and `FinancialData` (`decimal? MonthlySalary`, `decimal? PayrollSummary`).
    - Declare private fields: `private string _username;`, `private List<DonutSegment>? _taskSegments;`, `private List<PerformanceSeries>? _performanceSeries;`, `private FinancialData? _financialData;`.
    - Implement `public AdminOverviewUI(string username)`: call `InitializeComponent()`, store `_username`, set `UsernameLabel.Text = username`, then call `LoadTaskData()`, `LoadPerformanceData()`, `LoadFinancialData()`.
    - After data is loaded, call `DrawDonutChart()` and `DrawPerformanceGraph()`.
    - _Requirements: 1.1, 1.5, 2.6_

  - [x] 3.2 Implement `LoadTaskData()`, `LoadPerformanceData()`, `LoadFinancialData()`
    - `LoadTaskData()`: populate `_taskSegments` with the four default segments (Done 55% `#4ade80`, In-progress 25% `#7b61ff`, Late 15% `#f87171`, Other 5% `#94a3b8`).
    - `LoadPerformanceData()`: populate `_performanceSeries` with Employee A (`#7b61ff`, 6 monthly points) and Employee B (`#4ade80`, 6 monthly points) as specified in the design.
    - `LoadFinancialData()`: populate `_financialData` with `MonthlySalary = 125000m` and `PayrollSummary = 980000m`; then call `BindFinancialData()`.
    - _Requirements: 4.3, 5.3, 6.1_

  - [x] 3.3 Implement `BindFinancialData()`
    - If `_financialData` is null, set both amount labels to `"₱ —"`.
    - Otherwise, format `MonthlySalary` and `PayrollSummary` as `"₱ {value:N2}"` and assign to `MonthlySalaryAmount.Text` and `PayrollSummaryAmount.Text`.
    - _Requirements: 6.3, 6.6_

  - [x] 3.4 Implement `DrawDonutChart()`
    - Guard: if `DonutCanvas.ActualWidth == 0 || DonutCanvas.ActualHeight == 0`, return early.
    - If `_taskSegments` is null or empty, set `DonutCanvas.Visibility = Visibility.Collapsed` and `DonutFallbackText.Visibility = Visibility.Visible`, then return.
    - Clear `DonutCanvas.Children`.
    - Compute center and radii; iterate segments, converting `Percentage` to sweep angle (`degrees = percentage × 3.6`).
    - For each segment, create a `Path` with an `ArcSegment` (`IsLargeArc = sweepAngle > 180`), set `Fill` to the segment's color.
    - After all segments, overlay a filled `Ellipse` (inner hole) using `CardBackgroundBrush`.
    - Populate `LegendPanel`: for each segment add a horizontal `StackPanel` with a colored `Rectangle` (12×12) and a `TextBlock` showing `"{Label} — {Percentage}%"`.
    - _Requirements: 4.2, 4.3, 4.4, 4.5, 4.7_

  - [x] 3.5 Implement `DrawPerformanceGraph()`
    - Guard: if `GraphCanvas.ActualWidth == 0 || GraphCanvas.ActualHeight == 0`, return early.
    - If `_performanceSeries` is null or empty, set `GraphCanvas.Visibility = Visibility.Collapsed` and `GraphFallbackText.Visibility = Visibility.Visible`, then return.
    - Clear `GraphCanvas.Children`.
    - Normalize each series' `Points` to canvas pixel coordinates (map X to month index across canvas width, Y to score 0–100 across canvas height).
    - For each series, create a `Polyline` with `Stroke` set to the series color and `StrokeThickness="2"`, add to canvas.
    - For each data point, add an `Ellipse` (8×8 px, same color as series) positioned at the normalized coordinate.
    - Add X-axis `TextBlock` labels (month names/indices) and Y-axis `TextBlock` labels (0, 25, 50, 75, 100) positioned along the canvas edges.
    - _Requirements: 5.2, 5.3, 5.4, 5.5, 5.7_

  - [x] 3.6 Write unit tests for window initialization and TopNavBar structure
    - In `SOFTDEV.Tests`, create `AdminOverviewUITests.cs`.
    - Test: `AdminOverviewUI` constructor sets `MinWidth=1200`, `MinHeight=700`, `WindowStartupLocation=CenterScreen`, `Background=DarkBackgroundBrush`, `FontFamily=Segoe UI`.
    - Test: `SearchButton.Content == "🔍"`, `NotificationButton.Content == "🔔"`, `AvatarBorder.CornerRadius.TopLeft == 18`, `UsernameLabel.Text` equals the username passed to the constructor.
    - _Requirements: 1.2, 1.3, 1.4, 2.3, 2.4, 2.5, 2.6_

  - [x] 3.7 Write unit tests for TabMenu structure and fallback states
    - Test: TabMenu button order is OVERVIEW, My Team, Attendance, Task Management, Reports; OVERVIEW content contains "🏠"; non-active button opacity ≈ 0.4.
    - Test: Passing null/empty task data hides `DonutCanvas` and shows `DonutFallbackText`.
    - Test: Passing null/empty performance data hides `GraphCanvas` and shows `GraphFallbackText`.
    - Test: Passing null financial data sets both amount labels to `"₱ —"`.
    - _Requirements: 3.1, 3.2, 3.3, 4.7, 5.7, 6.6_

  - [x] 3.8 Write unit tests for NavButton_Click routing, donut segment count, and axis labels
    - Test: Clicking `OverviewButton` in `AdminDashboard` instantiates `AdminOverviewUI` (verify via subclassing or reflection).
    - Test: Default data produces exactly 4 `Path` elements on `DonutCanvas` after `DrawDonutChart()`.
    - Test: Both X-axis and Y-axis `TextBlock` label elements are present and non-empty on `GraphCanvas` after `DrawPerformanceGraph()`.
    - _Requirements: 1.1, 4.2, 5.5_

- [x] 4. Checkpoint — Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 5. Add property-based tests (FsCheck)
  - [x] 5.1 Write property test — Property 1: Username display round-trip
    - For any non-null `string username`, construct `AdminOverviewUI(username)` and assert `UsernameLabel.Text == username`.
    - Tag: `// Feature: admin-overview-ui, Property 1: Username display round-trip`
    - Run on STA thread; minimum 100 FsCheck iterations.
    - **Validates: Requirements 2.6**

  - [x] 5.2 Write property test — Property 2: TabMenu button CornerRadius invariant
    - For any button in `TabMenuPanel`, assert all four `CornerRadius` values are in [15, 20].
    - Tag: `// Feature: admin-overview-ui, Property 2: TabMenu button CornerRadius invariant`
    - Run on STA thread; minimum 100 FsCheck iterations.
    - **Validates: Requirements 3.4**

  - [x] 5.3 Write property test — Property 3: Donut segment color distinctness
    - For any `List<DonutSegment>` with distinct labels, call `DrawDonutChart()` and assert all `Color` values are pairwise distinct.
    - Tag: `// Feature: admin-overview-ui, Property 3: Donut segment color distinctness`
    - Minimum 100 FsCheck iterations (pure data, no STA required).
    - **Validates: Requirements 4.4**

  - [x] 5.4 Write property test — Property 4: Legend completeness
    - For any `List<DonutSegment>`, after `DrawDonutChart()`, assert `LegendPanel.Children.Count == segments.Count` and each child contains the segment's `Label` and `Percentage` as a string.
    - Tag: `// Feature: admin-overview-ui, Property 4: Legend completeness`
    - Minimum 100 FsCheck iterations.
    - **Validates: Requirements 4.5**

  - [x] 5.5 Write property test — Property 5: Performance series color distinctness
    - For any `List<PerformanceSeries>` with ≥ 2 entries, assert all `Color` values are pairwise distinct.
    - Tag: `// Feature: admin-overview-ui, Property 5: Performance series color distinctness`
    - Minimum 100 FsCheck iterations (pure data, no STA required).
    - **Validates: Requirements 5.3**

  - [x] 5.6 Write property test — Property 6: Data-point marker count invariant
    - For any `PerformanceSeries` with N points, after `DrawPerformanceGraph()`, assert exactly N `Ellipse` elements are present on `GraphCanvas` for that series.
    - Tag: `// Feature: admin-overview-ui, Property 6: Data-point marker count invariant`
    - Run on STA thread; minimum 100 FsCheck iterations.
    - **Validates: Requirements 5.4**

  - [x] 5.7 Write property test — Property 7: FinancialCard structure invariant
    - For each FinancialCard `Border`, assert: the circular inner `Border`'s `CornerRadius.TopLeft == Width / 2`, the inner `TextBlock` has `Text == "₱"` and `Foreground == PurpleAccentBrush`, and the amount `TextBlock` has `FontSize >= 18`.
    - Tag: `// Feature: admin-overview-ui, Property 7: FinancialCard structure invariant`
    - Run on STA thread; minimum 100 FsCheck iterations.
    - **Validates: Requirements 6.2, 6.3, 6.5**

  - [x] 5.8 Write property test — Property 8: Card styling invariant
    - For each card `Border` in the main content grid, assert `Background == CardBackgroundBrush`, all four `CornerRadius` values are in [15, 20], and `DropShadowEffect.BlurRadius` is in [10, 20] with `Opacity` in [0.3, 0.5].
    - Tag: `// Feature: admin-overview-ui, Property 8: Card styling invariant`
    - Run on STA thread; minimum 100 FsCheck iterations.
    - **Validates: Requirements 4.6, 5.6, 6.4, 7.3, 7.4**

- [x] 6. Final checkpoint — Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for a faster MVP.
- All chart drawing happens in code-behind after `InitializeComponent()` so canvas dimensions are available.
- `DrawDonutChart()` and `DrawPerformanceGraph()` must be called after the window is loaded (or inside `Loaded` event) to ensure `ActualWidth`/`ActualHeight` are non-zero.
- FsCheck property tests that inspect WPF elements must run on an STA thread — use an STA-aware xUnit fixture or `[STAThread]` attribute.
- Properties 3 and 5 operate on pure C# data classes and have no WPF dependency, so they can run in any thread context.
- No new NuGet packages are needed beyond `FsCheck.Xunit` (already available in `SOFTDEV.Tests`).
