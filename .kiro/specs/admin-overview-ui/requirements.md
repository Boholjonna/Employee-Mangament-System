# Requirements Document

## Introduction

AdminOverviewUI is a new WPF Window in the SOFTDEV application that opens when the admin presses the "OVERVIEW" button in AdminDashboard. It provides a high-level operational snapshot for administrators, displaying task completion status via a donut chart, employee performance trends via a multi-line graph, and financial summaries (monthly salary and payroll) via dedicated cards. The window follows the same dark-themed visual language as AdminDashboard, using only built-in WPF drawing primitives — no external charting or icon libraries.

---

## Glossary

- **AdminOverviewUI**: The new WPF Window (`AdminOverviewUI.xaml`) opened from AdminDashboard when the OVERVIEW button is clicked.
- **AdminDashboard**: The existing WPF Window (`AdminDashboard.xaml`) that hosts the navigation bar containing the OVERVIEW button.
- **NavBar**: The horizontal row of rounded navigation buttons at the top of AdminDashboard and AdminOverviewUI.
- **TopNavBar**: The top strip of AdminOverviewUI containing the company logo/name on the left and action icons + user profile on the right.
- **TabMenu**: The horizontal row of rounded pill-shaped navigation buttons below the TopNavBar.
- **TaskOverviewCard**: A card in the main content grid that renders a donut chart of task statuses using WPF Path geometries.
- **EmployeePerformancesCard**: A card in the main content grid that renders a multi-line performance graph using WPF Polyline/Path.
- **FinancialCard**: A smaller card displaying a circular peso icon and a large monetary amount; two instances exist — Monthly Salary and Payroll Summary.
- **DonutChart**: A circular chart composed of WPF Path arc segments, with no external charting library dependency.
- **PerformanceGraph**: A multi-line graph drawn with WPF Polyline/Path elements and visible data-point markers.
- **PurpleAccentBrush**: The global brush resource defined in App.xaml with color `#7b61ff`.
- **PurpleHoverBrush**: The global brush resource defined in App.xaml with color `#6a52e0`.
- **CardBackgroundBrush**: The global brush resource defined in App.xaml with color `#15151b`.
- **DarkBackgroundBrush**: The global brush resource defined in App.xaml with color `#0a0a0a`.

---

## Requirements

### Requirement 1: Window Initialization and Navigation Entry Point

**User Story:** As an admin, I want the AdminOverviewUI window to open when I click the OVERVIEW button in AdminDashboard, so that I can access the overview dashboard without navigating away from the application.

#### Acceptance Criteria

1. WHEN the admin clicks the OverviewButton in AdminDashboard, THE AdminDashboard SHALL open a new instance of AdminOverviewUI.
2. THE AdminOverviewUI SHALL use `WindowStartupLocation="CenterScreen"` and a minimum size of 1200×700 pixels.
3. THE AdminOverviewUI SHALL set its `Background` to `DarkBackgroundBrush` (`#0a0a0a`), consistent with AdminDashboard.
4. THE AdminOverviewUI SHALL use the `Segoe UI` font family as the default font throughout the window.
5. WHEN AdminOverviewUI is opened, THE AdminOverviewUI SHALL display all sections (TopNavBar, TabMenu, and main content grid) fully rendered without requiring user interaction.

---

### Requirement 2: Top Navigation Bar

**User Story:** As an admin, I want a top navigation bar with the company branding on the left and quick-access controls on the right, so that I can identify the application and access common actions from any screen.

#### Acceptance Criteria

1. THE TopNavBar SHALL display a left-aligned text element reading "COMPANY NAME/LOGO" with `Foreground` set to `PurpleAccentBrush` (`#7b61ff`).
2. THE TopNavBar SHALL display a right-aligned group containing a Search button, a Notifications button, a circular user profile image placeholder, and a user name label.
3. THE TopNavBar Search button SHALL use a Unicode magnifying-glass symbol (🔍) as its content and the `CircleButtonStyle` defined in App.xaml.
4. THE TopNavBar Notifications button SHALL use a Unicode bell symbol (🔔) as its content and the `CircleButtonStyle` defined in App.xaml.
5. THE TopNavBar user profile image placeholder SHALL be rendered as a circular `Border` with `CornerRadius` of 18 and a `👤` Unicode symbol inside.
6. THE TopNavBar user name label SHALL display the username passed to AdminOverviewUI at construction time.
7. WHEN the mouse pointer enters any TopNavBar action button, THE TopNavBar action button SHALL transition its background to `PurpleHoverBrush` (`#6a52e0`) via a `Trigger` in its `ControlTemplate`.

---

### Requirement 3: Tab Menu

**User Story:** As an admin, I want a horizontal tab menu with clearly distinguished active and inactive states, so that I can see which section I am currently viewing and navigate to other sections.

#### Acceptance Criteria

1. THE TabMenu SHALL contain at minimum the following buttons in order: OVERVIEW, My Team, Attendance, Task Management, and at least one additional navigation item.
2. THE TabMenu OVERVIEW button SHALL render with a fully opaque `PurpleAccentBrush` background and include a home Unicode symbol (🏠) alongside the label "OVERVIEW".
3. THE TabMenu non-active buttons SHALL render with a semi-transparent purple background (opacity ≈ 0.4) and white foreground text.
4. ALL TabMenu buttons SHALL have a `CornerRadius` of 15–20 pixels, consistent with the pill-button aesthetic.
5. WHEN the mouse pointer enters a non-active TabMenu button, THE TabMenu button SHALL increase its background opacity to fully opaque `PurpleHoverBrush` via a `Trigger`.
6. THE TabMenu SHALL be laid out as a horizontal `StackPanel` or `WrapPanel` with uniform spacing between buttons.

---

### Requirement 4: Task Overview Card (Donut Chart)

**User Story:** As an admin, I want a visual donut chart showing the breakdown of task statuses, so that I can quickly assess overall task health at a glance.

#### Acceptance Criteria

1. THE TaskOverviewCard SHALL be placed in the main content grid and display a title "Task Overview".
2. THE TaskOverviewCard SHALL render a DonutChart composed exclusively of WPF `Path` elements using `ArcSegment` geometry — no external charting library SHALL be used.
3. THE DonutChart SHALL represent four task-status segments: Done (55%), In-progress (25%), Late (15%), and Other (5%).
4. EACH DonutChart segment SHALL use a visually distinct fill color to differentiate the four statuses.
5. THE TaskOverviewCard SHALL display a legend listing each status label alongside its percentage value.
6. THE TaskOverviewCard SHALL have a `Background` of `CardBackgroundBrush` (`#15151b`), a `CornerRadius` of 15–20 pixels, and a `DropShadowEffect` for depth.
7. IF the task data cannot be loaded, THEN THE TaskOverviewCard SHALL display a fallback message "No data available" in place of the chart.

---

### Requirement 5: Employee Performances Card (Multi-Line Graph)

**User Story:** As an admin, I want a multi-line performance graph showing employee performance trends over time, so that I can identify high and low performers quickly.

#### Acceptance Criteria

1. THE EmployeePerformancesCard SHALL be placed in the main content grid and display a title "Employee Performances".
2. THE EmployeePerformancesCard SHALL render a PerformanceGraph composed exclusively of WPF `Polyline` or `Path` elements — no external charting library SHALL be used.
3. THE PerformanceGraph SHALL display at least two distinct data series, each rendered as a smoothed or straight line in a different color.
4. EACH data series in the PerformanceGraph SHALL include visible circular data-point markers at each data coordinate.
5. THE PerformanceGraph SHALL display labeled axes: a horizontal time/category axis and a vertical performance-value axis.
6. THE EmployeePerformancesCard SHALL have a `Background` of `CardBackgroundBrush` (`#15151b`), a `CornerRadius` of 15–20 pixels, and a `DropShadowEffect` for depth.
7. IF the performance data cannot be loaded, THEN THE EmployeePerformancesCard SHALL display a fallback message "No data available" in place of the graph.

---

### Requirement 6: Financial Cards (Monthly Salary & Payroll Summary)

**User Story:** As an admin, I want compact financial summary cards showing salary and payroll figures, so that I can monitor financial metrics without navigating to a separate report.

#### Acceptance Criteria

1. THE AdminOverviewUI SHALL display two FinancialCards: one labeled "Monthly Salary" and one labeled "Payroll Summary".
2. EACH FinancialCard SHALL display a circular icon containing the Philippine Peso symbol "₱" rendered in `PurpleAccentBrush`.
3. EACH FinancialCard SHALL display a large-text monetary amount below or beside the circular icon.
4. EACH FinancialCard SHALL have a `Background` of `CardBackgroundBrush` (`#15151b`), a `CornerRadius` of 15–20 pixels, and a `DropShadowEffect` for depth.
5. THE FinancialCard circular icon SHALL be rendered as a `Border` with `CornerRadius` equal to half its width/height, giving a perfect circle.
6. IF financial data cannot be loaded, THEN EACH FinancialCard SHALL display "₱ —" as the amount placeholder.

---

### Requirement 7: Visual Styling and Consistency

**User Story:** As an admin, I want the AdminOverviewUI to look and feel identical to AdminDashboard, so that the application has a consistent visual identity.

#### Acceptance Criteria

1. THE AdminOverviewUI SHALL reuse the global brush resources `DarkBackgroundBrush`, `CardBackgroundBrush`, `PurpleAccentBrush`, and `PurpleHoverBrush` defined in App.xaml — no duplicate color literals SHALL be introduced for these values.
2. THE AdminOverviewUI SHALL reuse the `PillButtonStyle`, `CircleButtonStyle`, and `CustomScrollbarStyle` defined in App.xaml wherever applicable.
3. ALL card containers (TaskOverviewCard, EmployeePerformancesCard, FinancialCards) SHALL apply a `DropShadowEffect` with a subtle blur radius (10–20 pixels) and low opacity (0.3–0.5).
4. ALL card containers SHALL use a `CornerRadius` in the range of 15–20 pixels.
5. THE AdminOverviewUI SHALL use only Unicode symbols or emoji for icons — no FontAwesome, MaterialDesign, or other external icon libraries SHALL be referenced.
6. WHILE the window is displayed, THE AdminOverviewUI SHALL maintain a minimum window size of 1200×700 pixels to prevent layout overflow.

---

### Requirement 8: Scrollability and Responsive Layout

**User Story:** As an admin, I want the main content area to be scrollable when the window is resized below the content's natural height, so that no content is clipped or inaccessible.

#### Acceptance Criteria

1. THE AdminOverviewUI main content area SHALL be wrapped in a `ScrollViewer` with `VerticalScrollBarVisibility="Auto"`.
2. WHERE the `CustomScrollbarStyle` is applied, THE ScrollViewer SHALL render the scrollbar thumb in `PurpleAccentBrush` consistent with AdminDashboard.
3. THE main content grid SHALL use proportional `ColumnDefinition` widths so that cards resize gracefully when the window width changes.
