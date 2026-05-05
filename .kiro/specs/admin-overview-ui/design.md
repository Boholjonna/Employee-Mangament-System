# Design Document — AdminOverviewUI

## Overview

AdminOverviewUI is a new WPF `Window` (`AdminOverviewUI.xaml` / `AdminOverviewUI.xaml.cs`) added to the SOFTDEV application. It opens when the admin clicks the **OVERVIEW** button in `AdminDashboard` and provides a high-level operational snapshot: task completion status (donut chart), employee performance trends (multi-line graph), and financial summaries (two compact cards).

The window follows the same dark-themed visual language as `AdminDashboard`, reusing all global brush and style resources from `App.xaml`. All charts are drawn with WPF `Path`/`ArcSegment`/`Polyline` primitives — no external charting or icon libraries are introduced.

### Key Design Decisions

- **No new global resources**: All colors and styles come from `App.xaml`. No color literals are duplicated.
- **Pure WPF drawing primitives**: Donut chart uses `Path` + `ArcSegment`; performance graph uses `Polyline` + `Ellipse` markers. This keeps the dependency footprint identical to the rest of the project.
- **ViewModel-style data classes**: Lightweight C# data classes (no full MVVM framework) hold chart and financial data, keeping the code-behind clean and testable.
- **Graceful fallback**: Each chart/financial card checks for null/empty data and shows a "No data available" or "₱ —" placeholder, preventing blank or broken UI states.
- **Username passed at construction**: `AdminOverviewUI(string username)` mirrors the `AdminDashboard(string username)` pattern already in use.

---

## Architecture

```
AdminDashboard.xaml.cs
  └─ NavButton_Click (OverviewButton sender)
       └─ new AdminOverviewUI(username).Show()

AdminOverviewUI.xaml          ← Window layout (XAML)
AdminOverviewUI.xaml.cs       ← Code-behind: data loading, chart drawing

Supporting data classes (in AdminOverviewUI.xaml.cs or separate files):
  DonutSegment                ← Label, Percentage, Color
  PerformanceSeries           ← Label, Color, List<Point>
  FinancialData               ← MonthlySalary, PayrollSummary (decimal?)
```

### Layout Structure

```
Window (AdminOverviewUI)
├── Grid (root, 2 rows)
│   ├── Row 0 — Header StackPanel
│   │   ├── TopNavBar (DockPanel: company name left, controls right)
│   │   └── TabMenu (StackPanel horizontal: OVERVIEW active + 4+ tabs)
│   └── Row 1 — ScrollViewer (VerticalScrollBarVisibility=Auto)
│       └── Grid (main content, 3 proportional columns)
│           ├── Column 0 — TaskOverviewCard (donut chart + legend)
│           ├── Column 1 — EmployeePerformancesCard (multi-line graph)
│           └── Column 2 — FinancialCards (StackPanel: Monthly Salary + Payroll Summary)
```

---

## Components and Interfaces

### 1. Window Entry Point — `AdminDashboard.xaml.cs`

The existing `NavButton_Click` handler is updated to detect when `OverviewButton` is the sender and open `AdminOverviewUI`:

```csharp
private void NavButton_Click(object sender, RoutedEventArgs e)
{
    if (sender == OverviewButton)
    {
        var overviewWindow = new AdminOverviewUI(_username);
        overviewWindow.Show();
    }
    // other nav buttons handled here in future
}
```

`_username` is stored as a private field set in the `AdminDashboard` constructor.

### 2. `AdminOverviewUI` Window

**Constructor signature:**
```csharp
public AdminOverviewUI(string username)
```

**Responsibilities:**
- Set `Title`, `MinWidth = 1200`, `MinHeight = 700`, `WindowStartupLocation = CenterScreen`
- Bind `username` to the TopNavBar username label
- Call `LoadTaskData()`, `LoadPerformanceData()`, `LoadFinancialData()`
- Call `DrawDonutChart()` and `DrawPerformanceGraph()` after data is loaded

### 3. TopNavBar

A `DockPanel` inside Row 0 of the root grid:
- **Left**: `TextBlock` — "COMPANY NAME/LOGO", `Foreground=PurpleAccentBrush`, `FontWeight=Bold`, `FontSize=18`
- **Right**: `StackPanel` (Horizontal) containing:
  - `Button` (SearchButton) — Content="🔍", `Style=CircleButtonStyle`
  - `Button` (NotificationButton) — Content="🔔", `Style=CircleButtonStyle`
  - `Border` (AvatarBorder) — `Width=36`, `Height=36`, `CornerRadius=18`, `Background=PurpleAccentBrush`, inner `TextBlock` "👤"
  - `TextBlock` (UsernameLabel) — bound to constructor `username` parameter, `Foreground=White`

### 4. TabMenu

A horizontal `StackPanel` below the TopNavBar:

| Button | State | Background | Opacity | Content |
|---|---|---|---|---|
| OVERVIEW | Active | `PurpleAccentBrush` | 1.0 | "🏠 OVERVIEW" |
| My Team | Inactive | `PurpleAccentBrush` | 0.4 | "My Team" |
| Attendance | Inactive | `PurpleAccentBrush` | 0.4 | "Attendance" |
| Task Management | Inactive | `PurpleAccentBrush` | 0.4 | "Task Management" |
| Reports | Inactive | `PurpleAccentBrush` | 0.4 | "Reports" |

All buttons use `PillButtonStyle` with `CornerRadius=20` (within the 15–20 px range). Inactive buttons have `Opacity=0.4`; an `IsMouseOver` trigger in the button template raises opacity to 1.0 and sets background to `PurpleHoverBrush`.

### 5. TaskOverviewCard

A `Border` (`CardBackgroundBrush`, `CornerRadius=18`, `DropShadowEffect`) containing:
- `TextBlock` title — "Task Overview"
- `Canvas` (DonutCanvas) — WPF `Path` arc segments drawn in code-behind
- `StackPanel` (LegendPanel) — one row per segment: colored square + label + percentage

**Donut chart drawing** (code-behind, `DrawDonutChart()`):
- Converts each `DonutSegment.Percentage` to a sweep angle (degrees = percentage × 3.6)
- Uses `ArcSegment` with `IsLargeArc = sweepAngle > 180` to draw each slice
- Inner circle (hole) is drawn as a filled `Ellipse` in `CardBackgroundBrush` on top
- If `_taskSegments` is null or empty, the canvas is hidden and a fallback `TextBlock` "No data available" is shown

**Default data (hardcoded, replaceable by DB call):**

| Status | Percentage | Color |
|---|---|---|
| Done | 55% | `#4ade80` (green) |
| In-progress | 25% | `#7b61ff` (purple) |
| Late | 15% | `#f87171` (red) |
| Other | 5% | `#94a3b8` (slate) |

### 6. EmployeePerformancesCard

A `Border` (`CardBackgroundBrush`, `CornerRadius=18`, `DropShadowEffect`) containing:
- `TextBlock` title — "Employee Performances"
- `Canvas` (GraphCanvas) — WPF `Polyline` lines + `Ellipse` markers drawn in code-behind
- Axis labels: `TextBlock` elements positioned along X (months) and Y (score 0–100) axes

**Graph drawing** (code-behind, `DrawPerformanceGraph()`):
- Normalizes data points to canvas pixel coordinates
- Draws one `Polyline` per `PerformanceSeries` with its `Color`
- Draws one `Ellipse` (8×8 px, same color) per data point as a marker
- Draws axis tick labels as `TextBlock` elements added to the canvas
- If `_performanceSeries` is null or empty, shows fallback "No data available"

**Default data (hardcoded, replaceable by DB call):**

| Series | Color | Points (month index, score) |
|---|---|---|
| Employee A | `#7b61ff` | (0,72),(1,78),(2,75),(3,82),(4,88),(5,85) |
| Employee B | `#4ade80` | (0,60),(1,65),(2,70),(3,68),(4,74),(5,80) |

### 7. FinancialCards

A vertical `StackPanel` in Column 2 containing two `Border` cards (`CardBackgroundBrush`, `CornerRadius=18`, `DropShadowEffect`).

Each card layout:
```
Border (card)
└── StackPanel (Horizontal)
    ├── Border (circular icon, Width=48, Height=48, CornerRadius=24, Background=PurpleAccentBrush)
    │   └── TextBlock "₱" (Foreground=White, FontSize=20, FontWeight=Bold, centered)
    └── StackPanel (Vertical, Margin=12,0,0,0)
        ├── TextBlock (card title: "Monthly Salary" or "Payroll Summary", Foreground=#aaaaaa, FontSize=13)
        └── TextBlock (amount: e.g. "₱ 125,000.00", Foreground=White, FontSize=22, FontWeight=Bold)
```

If `_financialData` is null, amount TextBlocks display "₱ —".

---

## Data Models

```csharp
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
    public string       Label  { get; set; } = string.Empty;
    public string       Color  { get; set; } = "#ffffff";
    public List<Point>  Points { get; set; } = new();
}

/// <summary>Financial summary data for the two financial cards.</summary>
public class FinancialData
{
    public decimal? MonthlySalary   { get; set; }
    public decimal? PayrollSummary  { get; set; }
}
```

`Point` is `System.Windows.Point` (already available in WPF, no new dependency).

---

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system — essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

This feature is a WPF UI window. Most acceptance criteria are UI configuration checks (SMOKE/EXAMPLE). However, several criteria express universal rules that hold across all inputs and are suitable for property-based testing: username display, chart color distinctness, legend completeness, data-point marker counts, and card styling invariants. These are tested using [FsCheck](https://fscheck.github.io/FsCheck/) (the standard .NET property-based testing library), configured for a minimum of 100 iterations per property.

---

### Property 1: Username Display Round-Trip

*For any* non-null username string passed to the `AdminOverviewUI` constructor, the `UsernameLabel` TextBlock in the TopNavBar SHALL display exactly that string.

**Validates: Requirements 2.6**

---

### Property 2: TabMenu Button CornerRadius Invariant

*For any* button in the TabMenu (active or inactive), its rendered `CornerRadius` SHALL have all four values in the range [15, 20] pixels.

**Validates: Requirements 3.4**

---

### Property 3: Donut Segment Color Distinctness

*For any* collection of `DonutSegment` objects passed to the donut chart, all segment `Color` values SHALL be pairwise distinct — no two segments share the same fill color.

**Validates: Requirements 4.4**

---

### Property 4: Legend Completeness

*For any* collection of `DonutSegment` objects, the rendered legend panel SHALL contain exactly one entry per segment, and each entry SHALL include both the segment's `Label` text and its `Percentage` value formatted as a string.

**Validates: Requirements 4.5**

---

### Property 5: Performance Series Color Distinctness

*For any* collection of `PerformanceSeries` objects with at least two entries, all series `Color` values SHALL be pairwise distinct — no two series share the same stroke color.

**Validates: Requirements 5.3**

---

### Property 6: Data-Point Marker Count Invariant

*For any* `PerformanceSeries` with N data points, the graph canvas SHALL contain exactly N circular marker `Ellipse` elements associated with that series after `DrawPerformanceGraph()` is called.

**Validates: Requirements 5.4**

---

### Property 7: FinancialCard Structure Invariant

*For any* FinancialCard rendered in the AdminOverviewUI, the card SHALL contain:
- A circular `Border` whose `CornerRadius.TopLeft` equals exactly half its `Width` (and half its `Height`), giving a perfect circle
- A `TextBlock` inside that circular `Border` with `Text = "₱"` and `Foreground = PurpleAccentBrush`
- A `TextBlock` for the monetary amount with `FontSize ≥ 18`

**Validates: Requirements 6.2, 6.3, 6.5**

---

### Property 8: Card Styling Invariant

*For any* card container `Border` in the main content grid (TaskOverviewCard, EmployeePerformancesCard, each FinancialCard), the card SHALL have:
- `Background = CardBackgroundBrush`
- `CornerRadius` with all four values in the range [15, 20] pixels
- A `DropShadowEffect` with `BlurRadius` in [10, 20] and `Opacity` in [0.3, 0.5]

**Validates: Requirements 4.6, 5.6, 6.4, 7.3, 7.4**

---

## Error Handling

| Scenario | Behavior |
|---|---|
| `LoadTaskData()` returns null or empty list | `DonutCanvas` hidden; fallback `TextBlock` "No data available" shown |
| `LoadPerformanceData()` returns null or empty list | `GraphCanvas` hidden; fallback `TextBlock` "No data available" shown |
| `LoadFinancialData()` returns null | Both FinancialCard amount labels display "₱ —" |
| `username` is null or empty | `UsernameLabel` displays empty string (no crash) |
| Window resized below 1200×700 | `MinWidth`/`MinHeight` constraints prevent further shrinking; `ScrollViewer` provides vertical scroll if content overflows |
| Chart canvas has zero size at draw time | `DrawDonutChart()` / `DrawPerformanceGraph()` guard against zero-dimension canvas with an early return |

---

## Testing Strategy

### Unit Tests (xUnit, existing `SOFTDEV.Tests` project)

Unit tests cover specific examples, edge cases, and error conditions:

- **Window initialization**: Verify `MinWidth=1200`, `MinHeight=700`, `WindowStartupLocation=CenterScreen`, `Background=DarkBackgroundBrush`, `FontFamily=Segoe UI`
- **TopNavBar structure**: Verify SearchButton content "🔍", NotificationButton content "🔔", avatar CornerRadius=18, username label binding
- **TabMenu structure**: Verify button order (OVERVIEW, My Team, Attendance, Task Management, Reports), OVERVIEW button content includes "🏠", non-active button opacity ≈ 0.4
- **TaskOverviewCard fallback**: Pass null/empty data, verify "No data available" is visible
- **EmployeePerformancesCard fallback**: Pass null/empty data, verify "No data available" is visible
- **FinancialCard fallback**: Pass null financial data, verify both amount labels show "₱ —"
- **NavButton_Click routing**: Verify that clicking OverviewButton in AdminDashboard instantiates AdminOverviewUI
- **Donut chart segment count**: Verify default data produces exactly 4 Path elements on the canvas
- **Axis labels**: Verify both X and Y axis label elements are present and non-empty after graph draw

### Property-Based Tests (FsCheck, existing `SOFTDEV.Tests` project)

Each property test runs a minimum of **100 iterations** via FsCheck's `Prop.ForAll`. Each test is tagged with a comment referencing the design property:

```
// Feature: admin-overview-ui, Property 1: Username display round-trip
// Feature: admin-overview-ui, Property 2: TabMenu button CornerRadius invariant
// Feature: admin-overview-ui, Property 3: Donut segment color distinctness
// Feature: admin-overview-ui, Property 4: Legend completeness
// Feature: admin-overview-ui, Property 5: Performance series color distinctness
// Feature: admin-overview-ui, Property 6: Data-point marker count invariant
// Feature: admin-overview-ui, Property 7: FinancialCard structure invariant
// Feature: admin-overview-ui, Property 8: Card styling invariant
```

**PBT library**: [FsCheck](https://fscheck.github.io/FsCheck/) (`FsCheck` NuGet package, already available in the .NET ecosystem; add `FsCheck.Xunit` for xUnit integration).

**Note on WPF UI testing**: Properties 2, 6, 7, and 8 require inspecting rendered WPF elements. These tests must run on an STA thread (use `[STAThread]` or an STA-aware xUnit fixture). Properties 3, 4, and 5 operate on pure data model classes (`DonutSegment`, `PerformanceSeries`) and have no WPF dependency, making them straightforward to run in any test context.

### Integration / Smoke Tests

- Verify `CustomScrollbarStyle` is applied to the main `ScrollViewer`'s `ScrollBar`
- Verify main content `Grid` `ColumnDefinitions` use star (`*`) proportional sizing
- Verify no external assembly references are added beyond the existing project dependencies
