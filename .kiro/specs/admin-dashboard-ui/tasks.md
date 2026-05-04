# Implementation Plan: Admin Dashboard UI

## Overview

Implement the `AdminDashboard` WPF window for the SOFTDEV application. The work proceeds in six stages: data models → XAML layout → code-behind logic → example-based tests → property-based tests → build verification. Each stage builds directly on the previous one. No MVVM; code-behind pattern only, matching `MainWindow.xaml.cs`. All styles come from `App.xaml`.

---

## Tasks

- [x] 1. Create data model files
  - Create `SOFTDEV/CalendarDayItem.cs` — `public record CalendarDayItem(int Day, bool IsCurrentMonth, bool IsHighlighted)` in the `SOFTDEV` namespace. Add XML doc comment: `Day == 0` indicates a leading padding cell outside the current month.
  - Create `SOFTDEV/EmployeeEntry.cs` — `public record EmployeeEntry(string Name, string Position)` in the `SOFTDEV` namespace. Add a constructor guard that replaces `null` with `string.Empty` for both properties.
  - Both records must be `public` so the test project can reference them directly.
  - _Requirements: 8.4, 9.5, 9.6_

- [x] 2. Create AdminDashboard.xaml — window shell and header
  - Create `SOFTDEV/AdminDashboard.xaml` with `x:Class="SOFTDEV.AdminDashboard"`, `Title="Admin Dashboard"`, `MinWidth="1200"`, `MinHeight="700"`, `WindowStartupLocation="CenterScreen"`, `ResizeMode="CanResize"`, `Background="{StaticResource DarkBackgroundBrush}"`.
  - Root layout: a single `Grid` with two `RowDefinition` entries — `Auto` for the header row, `*` for the main content row.
  - **ControlGroup** (`StackPanel`, `Orientation="Horizontal"`, `HorizontalAlignment="Right"`, `Margin="20,16,20,8"`): four children in order:
    - `SearchButton` — `CircleButtonStyle`, content `🔍`, `Click="SearchButton_Click"`
    - `NotificationButton` — `CircleButtonStyle`, content `🔔`, `Click="NotificationButton_Click"`
    - `UserNameButton` — `PillButtonStyle`, content `"NAME"`, `Width="80"`, `Height="30"`, `Click="UserNameButton_Click"`
    - `AvatarButton` — `CircleButtonStyle`, content `👤`, `Click="AvatarButton_Click"`
  - **NavBarPanel** (`StackPanel`, `x:Name="NavBarPanel"`, `Orientation="Horizontal"`, `Margin="20,0,20,16"`): seven `Button` children all using `PillButtonStyle` with `Margin="0,0,8,0"`:
    - `OverviewButton` — `Content` is a `StackPanel` with two `TextBlock` children: `Text="▲"` (icon) and `Text="OVERVIEW"` (label); `Click="NavButton_Click"`
    - `EmployeesButton` — `Content="Employees"`, `Click="NavButton_Click"`
    - `AttendanceButton` — `Content="Attendance"`, `Click="NavButton_Click"`
    - `ToDoButton` — `Content="To Do"`, `Click="NavButton_Click"`
    - `ReportsButton` — `Content="Reports"`, `Click="NavButton_Click"`
    - `LeavesButton` — `Content="Leaves"`, `Click="NavButton_Click"`
    - `SettingsButton` — `Content="Settings"`, `Click="NavButton_Click"`
  - Wrap ControlGroup and NavBarPanel in a `StackPanel` (`Orientation="Vertical"`) placed in `Grid.Row="0"`.
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 2.1, 2.2, 2.3, 2.4, 2.5, 3.1, 3.2, 3.3, 3.4_

- [x] 3. Add AdminDashboard.xaml — main content grid and left column
  - Add `MainContentGrid` (`Grid`, `Grid.Row="1"`) with three `ColumnDefinition` entries: `Width="1*"`, `Width="0.8*"`, `Width="1.2*"`.
  - **Left column** (`StackPanel`, `Grid.Column="0"`, `Margin="20,0,10,20"`):
    - `GreetingText` (`TextBlock`): `Text="Hello Admin Name! 👋"`, `Foreground="{StaticResource PurpleAccentBrush}"`, `FontWeight="Bold"`, `FontSize="20"`, `Margin="0,0,0,16"`.
    - `StatsGrid` (`UniformGrid`, `Rows="2"`, `Columns="2"`, `Margin="0,0,0,12"`): four `Border` children (`x:Name="StatCard0"` through `StatCard3`), each with `Background="{StaticResource CardBackgroundBrush}"`, `CornerRadius="12"`, `Margin="4"`, `Padding="16"`. Inside each `Border` a `StackPanel` containing:
      - Title `TextBlock` (purple, `FontSize="12"`, `FontWeight="Bold"`) with titles: `"Number of Employees"`, `"On Leave"`, `"New Joinee"`, `"Work Holidays / Others"`.
      - `Button` using `CircleButtonStyle`, content `↻`, `HorizontalAlignment="Right"`, `Click="StatCardRefresh_Click"`.
    - `AttendancePanel` (`Border`, `x:Name="AttendancePanel"`, Card_Surface, `Margin="0,0,0,12"`, `Padding="16"`): inner `StackPanel` containing:
      - Title `TextBlock`: `"Attendance"`, purple, bold.
      - Date `TextBlock`: `"May 23, 2025"`, white.
      - Time `TextBlock`: `"09:00 AM"`, white.
      - Status `TextBlock`: `"Clock In"`, purple.
      - Horizontal `StackPanel` with `ClockInOutButton` (`PillButtonStyle`, `Content="Clock IN/OUT"`, `Click="ClockInOut_Click"`) and `LunchBreakButton` (`PillButtonStyle`, `Content="LUNCH BREAK"`, `Margin="8,0,0,0"`, `Click="LunchBreak_Click"`).
    - `LeaveApprovalsPanel` (`Border`, `x:Name="LeaveApprovalsPanel"`, Card_Surface, `Padding="16"`): inner `StackPanel` containing:
      - Title `TextBlock`: `"Leave Approvals"`, purple, bold.
      - Horizontal `StackPanel` with:
        - Circle highlight `Border` (`Width="56"`, `Height="56"`, `CornerRadius="28"`, `Background="{StaticResource PurpleAccentBrush}"`): inner `TextBlock` `"May 23"`, white, centered.
        - Vertical `StackPanel` (`Margin="12,0,0,0"`): name `TextBlock` (purple, bold) and description `TextBlock` (`Foreground="#aaaaaa"`).
  - _Requirements: 4.1, 4.2, 4.3, 5.1, 5.2, 5.3, 5.4, 5.5, 6.1, 6.2, 6.3, 6.4, 7.1, 7.2, 7.3, 7.4, 7.5, 10.1, 10.2, 10.3, 10.4, 10.5_

- [x] 4. Add AdminDashboard.xaml — center column (calendar view)
  - **Center column** (`StackPanel`, `Grid.Column="1"`, `Margin="10,0,10,20"`):
    - Outer `Border` (Card_Surface, `Padding="16"`): inner `StackPanel` containing:
      - **CalendarHeader** (horizontal `StackPanel`): `CalendarMonthLabel` (`TextBlock`, `x:Name="CalendarMonthLabel"`, purple, bold, `FontSize="16"`, `HorizontalAlignment="Left"`, flex grow via `Margin`), `PrevMonthButton` (`CircleButtonStyle`, content `◀`, `Click="PrevMonth_Click"`), `NextMonthButton` (`CircleButtonStyle`, content `▶`, `Click="NextMonth_Click"`).
      - **DayHeaderRow** (`UniformGrid`, `Columns="7"`, `Margin="0,8,0,4"`): seven `TextBlock` children `"Sun"` through `"Sat"`, each `HorizontalAlignment="Center"`, `Foreground="#aaaaaa"`, `FontSize="11"`.
      - **CalendarDaysGrid** (`ItemsControl`, `x:Name="CalendarDaysControl"`, `Margin="0,4,0,0"`):
        - `ItemsPanel`: `ItemsPanelTemplate` containing `UniformGrid` with `Columns="7"`.
        - `ItemTemplate`: `DataTemplate` with a `Border` (`Width="36"`, `Height="36"`, `Margin="2"`) whose `Background` and `CornerRadius` are set via a `DataTrigger` on `IsHighlighted` (highlighted: `Background="{StaticResource PurpleAccentBrush}"`, `CornerRadius="18"`; default: `Background="Transparent"`, `CornerRadius="0"`). Inside the `Border`: a centered `TextBlock` bound to `Day`, `Foreground="White"`, `FontSize="13"`, with a `DataTrigger` that sets `Visibility="Collapsed"` when `Day == 0`.
  - _Requirements: 4.4, 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 8.7, 10.3, 10.7_

- [x] 5. Add AdminDashboard.xaml — right column (employee list)
  - **Right column** (`Border`, Card_Surface, `Grid.Column="2"`, `Margin="10,0,20,20"`, `Padding="16"`): inner `Grid` with three `RowDefinition` entries (`Auto`, `*`, `Auto`):
    - Row 0: `TextBlock` `"My Employees"`, purple, bold, `FontSize="16"`.
    - Row 1: `ScrollViewer` (`Grid.Row="1"`, `VerticalScrollBarVisibility="Auto"`):
      - `ScrollViewer.Resources`: `Style` with `TargetType="ScrollBar"` and `BasedOn="{StaticResource CustomScrollbarStyle}"` (no inline redefinition).
      - Inside: `ItemsControl` `x:Name="EmployeeListControl"`.
      - `ItemTemplate` `DataTemplate`: horizontal `StackPanel` (`Margin="0,6,0,6"`) with:
        - Circle avatar `Border` (`Width="36"`, `Height="36"`, `CornerRadius="18"`, `Background="{StaticResource PurpleAccentBrush}"`, `Margin="0,0,12,0"`): inner `TextBlock` `"👤"`, centered.
        - Vertical `StackPanel` (`VerticalAlignment="Center"`): `TextBlock` bound to `Name` (purple, bold, `FontSize="13"`) and `TextBlock` bound to `Position` (`Foreground="#aaaaaa"`, `FontSize="11"`).
    - Row 2: `Button` (`Grid.Row="2"`, `CircleButtonStyle`, content `↻`, `HorizontalAlignment="Right"`, `Click="EmployeeListRefresh_Click"`).
  - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6, 9.7, 10.6_

- [x] 6. Create AdminDashboard.xaml.cs — constructor and calendar generation
  - Create `SOFTDEV/AdminDashboard.xaml.cs` with `partial class AdminDashboard : Window` in the `SOFTDEV` namespace.
  - Declare private fields: `_calendarYear`, `_calendarMonth`, `_highlightDay` (initialized to `DateTime.Today`), `_calendarDays` (`List<CalendarDayItem>`).
  - Declare public property `Employees` (`List<EmployeeEntry>`) and public property `CalendarDays` (`List<CalendarDayItem>`).
  - Constructor: call `InitializeComponent()`, initialize `Employees` with the five placeholder entries (Alice Johnson / Software Engineer, Bob Martinez / Product Manager, Carol White / UX Designer, David Kim / QA Engineer, Eva Patel / DevOps Engineer), set `EmployeeListControl.ItemsSource = Employees`, call `RefreshCalendar()`.
  - Implement `internal List<CalendarDayItem> GenerateCalendarDays(int year, int month, int highlightDay)`:
    - Guard: throw `ArgumentOutOfRangeException` if `year < 1` or `month < 1` or `month > 12`.
    - Clamp `highlightDay` to `[0, DaysInMonth]` (0 means no highlight).
    - Add leading padding cells (`Day=0, IsCurrentMonth=false, IsHighlighted=false`) for `(int)new DateTime(year, month, 1).DayOfWeek` iterations.
    - Add current-month cells (`Day=d, IsCurrentMonth=true, IsHighlighted=(d == highlightDay)`) for `d` in `1..DaysInMonth`.
    - Return the list.
  - Implement `private void RefreshCalendar()`: call `GenerateCalendarDays`, assign result to `CalendarDays`, set `CalendarDaysControl.ItemsSource = CalendarDays`, update `CalendarMonthLabel.Text` to `$"{new DateTime(_calendarYear, _calendarMonth, 1):MMMM yyyy}"`.
  - Mark `GenerateCalendarDays` as `internal` (not `private`) so `SOFTDEV.Tests` can call it directly.
  - _Requirements: 8.4, 8.5, 8.6, 9.5, 9.6_

- [x] 7. Add AdminDashboard.xaml.cs — click handlers
  - Add all stub click handlers, each logging to `System.Diagnostics.Debug.WriteLine` with the handler name:
    - `SearchButton_Click` — stub for future search functionality. _Requirements: 2.6_
    - `NotificationButton_Click` — stub for future notification functionality. _Requirements: 2.7_
    - `UserNameButton_Click` — stub. _Requirements: 2.4_
    - `AvatarButton_Click` — stub. _Requirements: 2.5_
    - `NavButton_Click` — stub for future navigation logic. _Requirements: 3.5_
    - `StatCardRefresh_Click` — stub. _Requirements: 5.4_
    - `ClockInOut_Click` — stub for future attendance logic. _Requirements: 6.5_
    - `LunchBreak_Click` — stub for future lunch-break logic. _Requirements: 6.6_
    - `PrevMonth_Click` — decrement `_calendarMonth` (wrap to December of previous year if needed), call `RefreshCalendar()`. _Requirements: 8.6_
    - `NextMonth_Click` — increment `_calendarMonth` (wrap to January of next year if needed), call `RefreshCalendar()`. _Requirements: 8.6_
    - `EmployeeListRefresh_Click` — stub. _Requirements: 9.7_
  - _Requirements: 2.6, 2.7, 3.5, 6.5, 6.6, 8.6_

- [x] 8. Checkpoint — build the solution
  - Run `dotnet build SOFTDEV/SOFTDEV.csproj` and confirm zero errors and zero warnings.
  - Ensure all named elements referenced in code-behind (`NavBarPanel`, `CalendarMonthLabel`, `CalendarDaysControl`, `EmployeeListControl`) are present in the XAML.
  - Ensure all `x:Name` attributes in XAML match the field names generated by the XAML compiler.
  - Ensure all `Click` event handler names in XAML match the method signatures in code-behind.
  - _Requirements: 1.1, 1.4_

- [x] 9. Implement example-based tests in AdminDashboardTests.cs
  - The five existing test stubs (`AdminDashboard_WindowProperties_AreCorrect`, `AppResources_ContainRequiredStyleKeys`, `NavBar_ContainsSevenButtons`, `EmployeeList_HasAtLeastFivePlaceholders`, `MainWindow_OpenAdminDashboard_DoesNotThrow`) are already scaffolded — they will pass once the window is implemented. Do not modify them.
  - Add the following five new `[Fact]` tests to the `AdminDashboardSmokeTests` class, each wrapped in `StaHelper.RunOnSta` and preceded by `WpfAppBootstrap.EnsureInitialized()`:

  - **`StatsGrid_ContainsFourCards`** — locate `StatCard0` through `StatCard3` via `dashboard.FindName(...)`, assert all four are non-null `Border` instances. _Validates: Req 5.2_

  - **`AttendancePanel_ContainsTwoPillButtons`** — locate `AttendancePanel` via `FindName`, walk its visual/logical tree to find all `Button` children, assert exactly two buttons have `Content` equal to `"Clock IN/OUT"` and `"LUNCH BREAK"`. _Validates: Req 6.3_

  - **`CalendarHeader_DisplaysMonthYearLabel`** — locate `CalendarMonthLabel` via `FindName`, assert `Text` is not null or empty and matches the pattern `"[Month] [Year]"` (e.g., `"May 2025"`). _Validates: Req 8.6_

  - **`CalendarDayHeader_HasSevenColumns`** — locate the `DayHeaderRow` `UniformGrid` by walking the logical tree from the center-column `StackPanel`, assert it has exactly 7 `TextBlock` children with texts `"Sun"`, `"Mon"`, `"Tue"`, `"Wed"`, `"Thu"`, `"Fri"`, `"Sat"`. _Validates: Req 8.3_

  - **`EmployeeList_UsesCustomScrollbarStyle`** — locate `EmployeeListControl` via `FindName`, walk up to its parent `ScrollViewer`, assert `ScrollViewer.Resources` contains a `Style` with `TargetType == typeof(ScrollBar)` and `BasedOn` referencing `CustomScrollbarStyle`. _Validates: Req 9.4, 10.6_

  - _Requirements: 1.2, 1.3, 1.4, 1.5, 3.2, 5.2, 6.3, 8.3, 8.6, 9.4, 9.6, 10.6, 12.5_

  - [ ]* 9.1 Write unit tests for GenerateCalendarDays edge cases
    - Add a separate `[Fact]`-based test class `CalendarGenerationUnitTests` (no STA needed — pure logic).
    - Test: `GenerateCalendarDays_May2025_ReturnsCorrectLeadingPadding` — May 1 2025 is Thursday (offset 4), assert first 4 cells have `Day==0` and `IsCurrentMonth==false`.
    - Test: `GenerateCalendarDays_Feb2024_LeapYear_Returns29CurrentMonthCells` — February 2024 is a leap year, assert 29 cells with `IsCurrentMonth==true`.
    - Test: `GenerateCalendarDays_InvalidMonth_ThrowsArgumentOutOfRangeException` — call with `month=0` and `month=13`, assert `ArgumentOutOfRangeException` thrown.
    - Test: `GenerateCalendarDays_InvalidYear_ThrowsArgumentOutOfRangeException` — call with `year=0`, assert `ArgumentOutOfRangeException` thrown.
    - Test: `GenerateCalendarDays_HighlightDay_ExactlyOneCellHighlighted` — call with `highlightDay=15` for any month, assert exactly one cell has `IsHighlighted==true` and its `Day==15`.
    - _Requirements: 8.4, 8.5_

- [x] 10. Implement property-based tests (CsCheck)
  - Add a new test class `AdminDashboardPropertyTests` in `SOFTDEV.Tests/AdminDashboardTests.cs` (or a new file `SOFTDEV.Tests/AdminDashboardPropertyTests.cs`).
  - Import `CsCheck` and `System.Linq`.

  - [x] 10.1 Write property test for Property 1 — stat card structural invariant
    - **Property 1: Stat card structural invariant**
    - **Validates: Requirements 5.4, 5.5**
    - Use `StaHelper.RunOnSta` to instantiate `AdminDashboard` once, then iterate over all four stat card `Border` elements (`StatCard0`–`StatCard3`).
    - For each card, walk its logical children and assert:
      - At least one `TextBlock` child has `Foreground` equal to the purple accent color (`#7b61ff`).
      - At least one `Button` child uses `CircleButtonStyle` (check `button.Style == Application.Current.Resources["CircleButtonStyle"]`).
    - Since the four cards are fixed (not randomly generated), implement as a `[Fact]` that iterates over all four cards — this satisfies the "for any stat card" universal quantifier over the finite set.
    - _Requirements: 5.4, 5.5_

  - [x] 10.2 Write property test for Property 2 — calendar cell coverage for any month
    - **Property 2: Calendar cell coverage for any month**
    - **Validates: Requirements 8.4, 8.5**
    - Use `Gen.Int[1, 9999].SelectMany(year => Gen.Int[1, 12].Select(month => (year, month)))` combined with `Gen.Int[1, 28]` for `highlightDay` (safe minimum across all months).
    - Instantiate `AdminDashboard` on STA once; call `dashboard.GenerateCalendarDays(year, month, highlightDay)` for each generated triple.
    - Assert all five sub-properties simultaneously:
      1. `days.Count >= 28 && days.Count <= 42`
      2. `days.Count(d => d.IsCurrentMonth) == DateTime.DaysInMonth(year, month)`
      3. `days.Where(d => d.IsCurrentMonth).Select(d => d.Day).SequenceEqual(Enumerable.Range(1, DateTime.DaysInMonth(year, month)))`
      4. `days.TakeWhile(d => !d.IsCurrentMonth).Count() == (int)new DateTime(year, month, 1).DayOfWeek`
      5. `days.Count(d => d.IsHighlighted) == 1` and `days.Single(d => d.IsHighlighted).Day == highlightDay`
    - Run with CsCheck default of 100 iterations (no override needed).
    - _Requirements: 8.4, 8.5_

  - [x] 10.3 Write property test for Property 3 — employee entry round-trip
    - **Property 3: Employee entry structural invariant**
    - **Validates: Requirements 9.5, 9.6**
    - Use `Gen.String.Where(s => !string.IsNullOrEmpty(s)).SelectMany(name => Gen.String.Where(s => !string.IsNullOrEmpty(s)).Select(pos => (name, pos)))`.
    - For each generated `(name, pos)` pair, construct `new EmployeeEntry(name, pos)` and assert `entry.Name == name` and `entry.Position == pos`.
    - Run with CsCheck default of 100 iterations.
    - _Requirements: 9.5, 9.6_

- [x] 11. Final checkpoint — run all tests and verify build
  - Run `dotnet build` on the solution to confirm zero errors.
  - Run `dotnet test SOFTDEV.Tests/SOFTDEV.Tests.csproj` and confirm all tests pass (the five pre-existing smoke tests plus the five new example-based tests plus the three property-based tests).
  - If any test fails, fix the corresponding implementation or test before marking this task complete.
  - Ensure no temporary or scratch files were created during implementation.
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 3.2, 5.2, 5.4, 5.5, 6.3, 8.3, 8.4, 8.5, 8.6, 9.4, 9.5, 9.6, 10.6_

---

## Notes

- Tasks marked with `*` are optional and can be skipped for a faster MVP build.
- `GenerateCalendarDays` must be `internal` (not `private`) so `SOFTDEV.Tests` can call it directly without reflection.
- All WPF instantiation tests must run on a dedicated STA thread via `StaHelper.RunOnSta`.
- `WpfAppBootstrap.EnsureInitialized()` must be called before any WPF object is instantiated in tests.
- Do NOT wire `AdminDashboard` to `MainWindow` startup — `MainWindow.OpenAdminDashboard()` already exists as a navigation stub.
- Do NOT redefine any style inline in `AdminDashboard.xaml` — reference `App.xaml` resources only.
- Property tests use CsCheck (already in `SOFTDEV.Tests.csproj`); no new package references needed.
- The `EmployeeEntry` null-guard in the constructor ensures Property 3 holds even when CsCheck generates edge-case strings.
