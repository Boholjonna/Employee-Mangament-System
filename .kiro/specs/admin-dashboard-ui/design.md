# Design Document — Admin Dashboard UI

## Overview

The Admin Dashboard is a standalone WPF `Window` (`AdminDashboard.xaml`) added to the existing SOFTDEV application. It presents a data-rich, multi-column administrative interface that shares the application's established dark theme: deep black background (`#0a0a0a`), dark charcoal card surfaces (`#15151b`), and vibrant purple accents (`#7b61ff`).

The window is self-contained and is not wired to `MainWindow` in this phase. All styles are consumed from `App.xaml`; no styles are redefined inline. The calendar is implemented as a custom grid (no default WPF `Calendar` control). All icons are Unicode characters or WPF `Path` geometry — no external image files.

### Key Design Decisions

| Decision | Rationale |
|---|---|
| Standalone `Window`, not a `UserControl` | Matches the requirement for independent launch; avoids coupling to `MainWindow` |
| Custom calendar grid via `UniformGrid` / `ItemsControl` | Avoids the opinionated styling of the default `Calendar` control; gives full visual control |
| `ItemsControl` for employee list (not `ListView`) | Simpler template control; `ListView` selection chrome is unnecessary here |
| `ScrollViewer` with `CustomScrollbarStyle` reference | Reuses the App.xaml style; no inline redefinition |
| Code-behind only (no MVVM) | Consistent with the existing `MainWindow.xaml.cs` pattern; MVVM is out of scope |
| Calendar data generated in code-behind | Keeps XAML clean; month/day logic is pure C# and is the primary testable surface |

---

## Architecture

The dashboard follows the same code-behind pattern used by `MainWindow`. There is no separate ViewModel layer.

```
AdminDashboard.xaml          ← XAML layout (structure, styles, named elements)
AdminDashboard.xaml.cs       ← Code-behind (event handlers, calendar generation, employee data)
App.xaml                     ← Shared styles/resources (PillButtonStyle, CircleButtonStyle,
                                CustomScrollbarStyle, color brushes)
```

### Component Hierarchy (logical)

```
AdminDashboard (Window)
├── RootGrid (Grid, 1 row)
│   ├── HeaderSection (StackPanel, vertical)
│   │   ├── ControlGroup (StackPanel, horizontal, HorizontalAlignment=Right)
│   │   │   ├── SearchButton (Button, CircleButtonStyle)
│   │   │   ├── NotificationButton (Button, CircleButtonStyle)
│   │   │   ├── UserNameButton (Button, PillButtonStyle, Content="NAME")
│   │   │   └── AvatarButton (Button, CircleButtonStyle)
│   │   └── NavBarPanel (StackPanel, horizontal)
│   │       ├── OverviewButton (Button, PillButtonStyle, StackPanel content)
│   │       ├── EmployeesButton (Button, PillButtonStyle)
│   │       ├── AttendanceButton (Button, PillButtonStyle)
│   │       ├── ToDoButton (Button, PillButtonStyle)
│   │       ├── ReportsButton (Button, PillButtonStyle)
│   │       ├── LeavesButton (Button, PillButtonStyle)
│   │       └── SettingsButton (Button, PillButtonStyle)
│   └── MainContentGrid (Grid, 3 columns: 1*, 0.8*, 1.2*)
│       ├── LeftColumn (StackPanel, Grid.Column=0)
│       │   ├── GreetingText (TextBlock)
│       │   ├── StatsGrid (UniformGrid, Rows=2, Columns=2)
│       │   │   ├── StatCard_Employees (Border/Card_Surface)
│       │   │   ├── StatCard_OnLeave (Border/Card_Surface)
│       │   │   ├── StatCard_NewJoinee (Border/Card_Surface)
│       │   │   └── StatCard_Holidays (Border/Card_Surface)
│       │   ├── AttendancePanel (Border/Card_Surface)
│       │   └── LeaveApprovalsPanel (Border/Card_Surface)
│       ├── CenterColumn (StackPanel, Grid.Column=1)
│       │   └── CalendarView (custom Grid-based)
│       │       ├── CalendarHeader (StackPanel: month/year label + nav buttons)
│       │       ├── DayHeaderRow (UniformGrid, Columns=7)
│       │       └── CalendarDaysGrid (ItemsControl bound to CalendarDays list)
│       └── RightColumn (Border/Card_Surface, Grid.Column=2)
│           ├── MyEmployeesHeader (TextBlock)
│           ├── EmployeeListControl (ItemsControl + ScrollViewer)
│           └── EmployeeListFooterButton (Button, CircleButtonStyle)
```

---

## Components and Interfaces

### 1. AdminDashboard Window Shell

- `x:Class="SOFTDEV.AdminDashboard"`, file `AdminDashboard.xaml`
- `Background="{StaticResource DarkBackgroundBrush}"` (`#0a0a0a`)
- `MinWidth="1200"`, `MinHeight="700"`, `WindowStartupLocation="CenterScreen"`, `ResizeMode="CanResize"`
- Root layout: single `Grid` with two rows — `Auto` for `HeaderSection`, `*` for `MainContentGrid`

### 2. Header Section

**ControlGroup** — `StackPanel` with `Orientation="Horizontal"`, `HorizontalAlignment="Right"`, `Margin="20,16,20,8"`:

| Element | Name | Style | Content |
|---|---|---|---|
| Search button | `SearchButton` | `CircleButtonStyle` | `🔍` (U+1F50D) |
| Notification button | `NotificationButton` | `CircleButtonStyle` | `🔔` (U+1F514) |
| User pill | `UserNameButton` | `PillButtonStyle` | `"NAME"` |
| Avatar | `AvatarButton` | `CircleButtonStyle` | `👤` (U+1F464) |

**NavBarPanel** — `StackPanel` with `Orientation="Horizontal"`, `Margin="20,0,20,16"`:

The OVERVIEW button uses a `StackPanel` as its `Content` containing two `TextBlock` children: a triangle icon (`▲`, U+25B2) and the label `"OVERVIEW"`. All other nav buttons use a plain string `Content`.

All seven buttons use `PillButtonStyle` with `Margin="0,0,8,0"` between items.

### 3. Main Content Area

`MainContentGrid` — `Grid` with:
```xml
<Grid.ColumnDefinitions>
    <ColumnDefinition Width="1*" />
    <ColumnDefinition Width="0.8*" />
    <ColumnDefinition Width="1.2*" />
</Grid.ColumnDefinitions>
```

### 4. Left Column

A `StackPanel` in `Grid.Column="0"` with `Margin="20,0,10,20"`.

**Greeting** — `TextBlock` with `Text="Hello Admin Name! 👋"`, `Foreground="{StaticResource PurpleAccentBrush}"`, `FontWeight="Bold"`, `FontSize="20"`, `Margin="0,0,0,16"`.

**StatsGrid** — `UniformGrid` with `Rows="2"` and `Columns="2"`, `Margin="0,0,0,12"`. Each of the four cells is a `Border` (Card_Surface) with `Background="{StaticResource CardBackgroundBrush}"`, `CornerRadius="12"`, `Margin="4"`, `Padding="16"`. Inside each `Border`:
- A `TextBlock` for the title (purple, `FontSize="12"`, `FontWeight="Bold"`)
- A `Button` using `CircleButtonStyle` with a circle-arrow icon (`↻`, U+21BB), `HorizontalAlignment="Right"`

Named elements: `StatCard0` through `StatCard3` on the outer `Border` elements.

**AttendancePanel** — `Border` (Card_Surface), `x:Name="AttendancePanel"`, `Margin="0,0,0,12"`, `Padding="16"`. Contains a `StackPanel`:
- Title `TextBlock`: `"Attendance"`, purple, bold
- Date `TextBlock`: `"May 23, 2025"`, white
- Time `TextBlock`: `"09:00 AM"`, white
- Status `TextBlock`: `"Clock In"`, purple
- A horizontal `StackPanel` with two `Button` elements using `PillButtonStyle`:
  - `ClockInOutButton`: `"Clock IN/OUT"`
  - `LunchBreakButton`: `"LUNCH BREAK"`

**LeaveApprovalsPanel** — `Border` (Card_Surface), `x:Name="LeaveApprovalsPanel"`, `Padding="16"`. Contains:
- Title `TextBlock`: `"Leave Approvals"`, purple, bold
- A horizontal `StackPanel` containing:
  - A `Border` (circle highlight): `Width="56"`, `Height="56"`, `CornerRadius="28"`, `Background="{StaticResource PurpleAccentBrush}"`, containing a `TextBlock` `"May 23"` centered in white
  - A vertical `StackPanel` with:
    - Name `TextBlock`: purple, bold
    - Description `TextBlock`: `Foreground="#aaaaaa"`, normal weight

### 5. Center Column — Calendar View

A `StackPanel` in `Grid.Column="1"` with `Margin="10,0,10,20"`.

**CalendarHeader** — horizontal `StackPanel` containing:
- `CalendarMonthLabel` (`TextBlock`): displays `"May 2025"`, purple, bold, `FontSize="16"`
- Previous/Next navigation `Button` elements using `CircleButtonStyle` (`◀` / `▶`)

**DayHeaderRow** — `UniformGrid` with `Columns="7"`. Seven `TextBlock` children: `"Sun"`, `"Mon"`, `"Tue"`, `"Wed"`, `"Thu"`, `"Fri"`, `"Sat"`. Each centered, `Foreground="#aaaaaa"`, `FontSize="11"`.

**CalendarDaysGrid** — `ItemsControl` named `CalendarDaysControl`, bound to `CalendarDays` (a `List<CalendarDayItem>` set in code-behind). Uses a `WrapPanel` as `ItemsPanel` with `ItemWidth` set to 1/7 of available width, or alternatively an `UniformGrid` with `Columns="7"` as the panel. Each item template renders a `Border` with:
- `Width="36"`, `Height="36"`, `CornerRadius="18"` when highlighted (purple background), otherwise transparent
- A centered `TextBlock` with the day number

The `CalendarDayItem` model:

```csharp
public record CalendarDayItem(int Day, bool IsCurrentMonth, bool IsHighlighted);
```

Calendar generation logic (in code-behind):

```csharp
private List<CalendarDayItem> GenerateCalendarDays(int year, int month, int highlightDay)
{
    var days = new List<CalendarDayItem>();
    var firstDay = new DateTime(year, month, 1);
    int startOffset = (int)firstDay.DayOfWeek; // 0=Sun
    int daysInMonth = DateTime.DaysInMonth(year, month);

    // Leading empty cells
    for (int i = 0; i < startOffset; i++)
        days.Add(new CalendarDayItem(0, false, false));

    // Month days
    for (int d = 1; d <= daysInMonth; d++)
        days.Add(new CalendarDayItem(d, true, d == highlightDay));

    return days;
}
```

### 6. Right Column — Employee List

A `Border` (Card_Surface) in `Grid.Column="2"` with `Margin="10,0,20,20"`, `Padding="16"`. Contains a `Grid` with three rows (`Auto`, `*`, `Auto`):

- Row 0: `TextBlock` `"My Employees"`, purple, bold, `FontSize="16"`
- Row 1: `ScrollViewer` with `VerticalScrollBarVisibility="Auto"` and a custom `ScrollBar` style referencing `CustomScrollbarStyle`. Inside: `ItemsControl` named `EmployeeListControl` bound to `Employees` (`List<EmployeeEntry>`).
- Row 2: `Button` using `CircleButtonStyle` (`↻`), `HorizontalAlignment="Right"`

Each `EmployeeEntry` item template:
```xml
<DataTemplate>
    <StackPanel Orientation="Horizontal" Margin="0,6,0,6">
        <Border Width="36" Height="36" CornerRadius="18"
                Background="{StaticResource PurpleAccentBrush}"
                Margin="0,0,12,0">
            <TextBlock Text="👤" HorizontalAlignment="Center"
                       VerticalAlignment="Center" />
        </Border>
        <StackPanel VerticalAlignment="Center">
            <TextBlock Text="{Binding Name}" Foreground="#7b61ff"
                       FontWeight="Bold" FontSize="13" />
            <TextBlock Text="{Binding Position}" Foreground="#aaaaaa"
                       FontSize="11" />
        </StackPanel>
    </StackPanel>
</DataTemplate>
```

The `EmployeeEntry` model:

```csharp
public record EmployeeEntry(string Name, string Position);
```

Placeholder data (initialized in constructor):

```csharp
Employees = new List<EmployeeEntry>
{
    new("Alice Johnson",  "Software Engineer"),
    new("Bob Martinez",   "Product Manager"),
    new("Carol White",    "UX Designer"),
    new("David Kim",      "QA Engineer"),
    new("Eva Patel",      "DevOps Engineer"),
};
```

### 7. ScrollViewer + CustomScrollbarStyle Wiring

The `ScrollViewer` in the employee list references the `CustomScrollbarStyle` via:

```xml
<ScrollViewer VerticalScrollBarVisibility="Auto">
    <ScrollViewer.Resources>
        <Style TargetType="ScrollBar"
               BasedOn="{StaticResource CustomScrollbarStyle}" />
    </ScrollViewer.Resources>
    <ItemsControl x:Name="EmployeeListControl" ... />
</ScrollViewer>
```

This applies the dark-track/purple-thumb style without redefining it inline.

---

## Data Models

### CalendarDayItem

```csharp
/// <summary>
/// Represents a single cell in the calendar grid.
/// Day == 0 indicates a leading/trailing empty cell (outside the current month).
/// </summary>
public record CalendarDayItem(
    int Day,            // 1–31 for valid days; 0 for padding cells
    bool IsCurrentMonth,
    bool IsHighlighted  // true for exactly one cell (the selected/today date)
);
```

### EmployeeEntry

```csharp
/// <summary>
/// Represents a single row in the Employee List.
/// </summary>
public record EmployeeEntry(string Name, string Position);
```

### CalendarState (implicit, held in code-behind fields)

```csharp
private int _calendarYear  = DateTime.Today.Year;
private int _calendarMonth = DateTime.Today.Month;
private int _highlightDay  = DateTime.Today.Day;
private List<CalendarDayItem> _calendarDays = new();
```

---

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system — essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

The primary testable logic surface in this feature is the **calendar day generation algorithm** (`GenerateCalendarDays`) and the **structural invariants** of the data models. The XAML layout and style application are verified by example-based smoke tests.

Property-based testing is applied using **CsCheck** (already referenced in `SOFTDEV.Tests.csproj`).

---

### Property 1: Stat card structural invariant

*For any* stat card in the Stats_Grid, the card must contain a title `TextBlock` with the Purple_Accent foreground color and a `Circle_Button` with a circle-arrow icon. This invariant must hold for all four stat cards regardless of their content.

**Validates: Requirements 5.4, 5.5**

---

### Property 2: Calendar cell coverage for any month

*For any* valid (year, month) pair, the list of `CalendarDayItem` objects produced by `GenerateCalendarDays` must satisfy all of the following simultaneously:

1. The total cell count is between 28 and 42 (inclusive) — at least 4 rows × 7 columns, at most 6 rows × 7 columns.
2. The number of cells where `IsCurrentMonth == true` equals exactly `DateTime.DaysInMonth(year, month)`.
3. The day values for current-month cells form the contiguous sequence `1, 2, …, DaysInMonth`.
4. The leading padding cells (before day 1) count equals `(int)new DateTime(year, month, 1).DayOfWeek`.
5. Exactly one cell has `IsHighlighted == true` when a valid `highlightDay` (1 ≤ highlightDay ≤ DaysInMonth) is provided.

**Validates: Requirements 8.4, 8.5**

---

### Property 3: Employee entry structural invariant

*For any* `EmployeeEntry` with a non-empty `Name` and non-empty `Position`, the entry's data must be round-trippable: constructing an `EmployeeEntry` and reading back its `Name` and `Position` properties must return the original values unchanged.

**Validates: Requirements 9.5, 9.6**

---

## Error Handling

| Scenario | Handling |
|---|---|
| `GenerateCalendarDays` called with invalid month (< 1 or > 12) | Guard clause throws `ArgumentOutOfRangeException` |
| `GenerateCalendarDays` called with invalid year (< 1) | Guard clause throws `ArgumentOutOfRangeException` |
| `highlightDay` outside 1–DaysInMonth | Clamped to valid range; no cell is highlighted if 0 is passed |
| Click handlers (nav, attendance, search, notification) | No-op stubs; log to `Debug.WriteLine` for traceability |
| `EmployeeEntry` with null Name or Position | Constructor guards replace null with `string.Empty` |

---

## Testing Strategy

### Dual Testing Approach

Both example-based unit tests and property-based tests are used. Example-based tests verify specific structural and configuration requirements (smoke tests, layout checks). Property-based tests verify universal invariants over the calendar generation logic and data models.

### Example-Based Tests (xUnit)

These tests are already partially scaffolded in `AdminDashboardTests.cs`. They cover:

| Test | Validates |
|---|---|
| `AdminDashboard_WindowProperties_AreCorrect` | Req 1.2, 1.3 — window shell config |
| `AppResources_ContainRequiredStyleKeys` | Req 1.4 — style reuse from App.xaml |
| `NavBar_ContainsSevenButtons` | Req 3.2 — nav bar count and labels |
| `EmployeeList_HasAtLeastFivePlaceholders` | Req 9.6 — placeholder data |
| `MainWindow_OpenAdminDashboard_DoesNotThrow` | Req 1.3 — independent launch |
| `StatsGrid_ContainsFourCards` | Req 5.2 — stat card count |
| `AttendancePanel_ContainsTwoPillButtons` | Req 6.3 — attendance buttons |
| `CalendarHeader_DisplaysMonthYearLabel` | Req 8.6 — month/year label |
| `CalendarDayHeader_HasSevenColumns` | Req 8.3 — day-of-week header |
| `EmployeeList_UsesCustomScrollbarStyle` | Req 9.4, 10.6 — scrollbar style reference |

All WPF instantiation tests run on a dedicated STA thread via the existing `StaHelper.RunOnSta` helper.

### Property-Based Tests (CsCheck)

CsCheck is already referenced in `SOFTDEV.Tests.csproj`. Each property test runs a minimum of **100 iterations**.

**Property 1 — Stat card structural invariant**

```
Feature: admin-dashboard-ui, Property 1: stat card structural invariant
```

Generate all four stat cards from the `AdminDashboard` instance and assert that each `Border` (Card_Surface) contains at least one `TextBlock` with `Foreground` equal to `#7b61ff` and at least one `Button` using `CircleButtonStyle`. Since the stat cards are fixed (not generated from random data), this is implemented as a parameterized example test iterating over all four cards.

**Property 2 — Calendar cell coverage for any month**

```
Feature: admin-dashboard-ui, Property 2: calendar cell coverage for any month
```

Generator: `Gen.Int[1, 9999].SelectMany(year => Gen.Int[1, 12].Select(month => (year, month)))` combined with a `highlightDay` drawn from `Gen.Int[1, 28]` (safe minimum for all months).

Assertions per generated (year, month, highlightDay):
- `days.Count >= 28 && days.Count <= 42`
- `days.Count(d => d.IsCurrentMonth) == DateTime.DaysInMonth(year, month)`
- `days.Where(d => d.IsCurrentMonth).Select(d => d.Day)` equals `Enumerable.Range(1, DaysInMonth)`
- `days.Count(d => d.IsHighlighted) == 1`
- `days.Single(d => d.IsHighlighted).Day == highlightDay`

**Property 3 — Employee entry round-trip**

```
Feature: admin-dashboard-ui, Property 3: employee entry structural invariant
```

Generator: `Gen.String.SelectMany(name => Gen.String.Select(pos => (name, pos)))` filtered to non-null, non-empty strings.

Assertion: `new EmployeeEntry(name, pos)` produces an object where `.Name == name` and `.Position == pos`.

### Test Configuration

- Minimum 100 iterations per CsCheck property test (CsCheck default is 100; no override needed)
- All WPF tests run on STA threads via `StaHelper.RunOnSta`
- App.xaml resources loaded via `WpfAppBootstrap.EnsureInitialized()` before any WPF instantiation

### What Is Not Tested

- Visual pixel-perfect rendering (requires UI automation or visual regression tools)
- Hover/press trigger animations (require mouse simulation)
- Actual clock-in/lunch-break business logic (placeholder stubs only at this stage)
