# Requirements Document

## Introduction

This feature adds a standalone Admin Dashboard UI to the existing SOFTDEV WPF application. The dashboard is a multi-column, data-rich interface styled with the application's established dark theme (deep black background `#0a0a0a`, dark charcoal card surfaces `#15151b`, and vibrant purple accents `#7b61ff`). It is implemented as a standalone `Window` named `AdminDashboard` and is not wired to `MainWindow` at this stage. All interactive elements use rounded or pill shapes consistent with the existing design language.

## Glossary

- **AdminDashboard**: The standalone WPF `Window` that contains the entire dashboard UI.
- **Dashboard**: The complete admin interface composed of a Header Section and a Main Content Area.
- **Header_Section**: The top area of the Dashboard containing the Control Group and the Navigation Bar.
- **Control_Group**: The top-right horizontal arrangement of action buttons and the user identity pill.
- **Navigation_Bar**: The horizontal row of pill-shaped navigation buttons below the Control Group.
- **Main_Content_Area**: The three-column grid layout below the Header Section.
- **Left_Column**: The leftmost vertical section of the Main Content Area, containing stats, attendance, and leave approval panels.
- **Center_Column**: The middle vertical section of the Main Content Area, containing the Calendar View.
- **Right_Column**: The rightmost vertical section of the Main Content Area, containing the Employee List.
- **Stats_Grid**: A 2×2 grid of stat cards inside the Left Column.
- **Stat_Card**: A single dark rounded container displaying a metric title and a circle-arrow action button.
- **Attendance_Panel**: A rounded container in the Left Column showing date, time, and clock-in/lunch-break controls.
- **Leave_Approvals_Panel**: A rounded container in the Left Column showing a pending leave approval entry.
- **Calendar_View**: A custom grid-based full-month calendar built without the default WPF Calendar control.
- **Employee_List**: A scrollable list of employee entries inside the Right Column.
- **Employee_Entry**: A single row in the Employee List containing a circular avatar, name, and position text.
- **Pill_Button**: A button with fully rounded ends (CornerRadius ≥ 20) styled with the purple accent.
- **Circle_Button**: A perfectly circular button (equal Width and Height, CornerRadius = half of size).
- **Dark_Scrollbar**: A custom ScrollBar ControlTemplate with a dark track and rounded purple thumb.
- **Purple_Accent**: The color `#7b61ff` used for labels, headings, buttons, and highlight elements.
- **Card_Surface**: A rounded Border with background `#15151b` used as a container for panels and cards.

---

## Requirements

### Requirement 1: AdminDashboard Window Shell

**User Story:** As an admin, I want a dedicated dashboard window, so that I can access all administrative functions in one place without affecting the login screen.

#### Acceptance Criteria

1. THE AdminDashboard SHALL be implemented as a WPF `Window` in the `SOFTDEV` namespace with the filename `AdminDashboard.xaml`.
2. THE AdminDashboard SHALL use a deep black background (`#0a0a0a`) for its root surface.
3. THE AdminDashboard SHALL open independently and SHALL NOT be automatically launched from `MainWindow` during this implementation phase.
4. THE AdminDashboard SHALL reuse the global styles defined in `App.xaml` (e.g., `PillButtonStyle`, `CircleButtonStyle`, `CustomScrollbarStyle`).

---

### Requirement 2: Header Control Group

**User Story:** As an admin, I want quick-access controls in the top-right corner, so that I can search, view notifications, and identify the current user at a glance.

#### Acceptance Criteria

1. THE Header_Section SHALL contain a Control_Group aligned to the top-right of the Dashboard.
2. THE Control_Group SHALL contain a circular Search button displaying a search/magnifier icon.
3. THE Control_Group SHALL contain a circular Notification button displaying a bell icon.
4. THE Control_Group SHALL contain a Pill_Button displaying the text "NAME" representing the current user identity.
5. THE Control_Group SHALL contain a small Circle_Button adjacent to the "NAME" pill serving as an avatar placeholder.
6. WHEN the Search button is clicked, THE AdminDashboard SHALL provide a placeholder handler (no-op or stub) for future search functionality.
7. WHEN the Notification button is clicked, THE AdminDashboard SHALL provide a placeholder handler for future notification functionality.

---

### Requirement 3: Navigation Bar

**User Story:** As an admin, I want a navigation bar with clearly labeled sections, so that I can switch between different administrative views.

#### Acceptance Criteria

1. THE Navigation_Bar SHALL be positioned below the Control_Group within the Header_Section.
2. THE Navigation_Bar SHALL contain exactly seven Pill_Button navigation items in this order: "OVERVIEW", "Employees", "Attendance", "To Do", "Reports", "Leaves", "Settings".
3. THE Navigation_Bar "OVERVIEW" button SHALL display a delta (triangle) icon alongside its label text.
4. THE Navigation_Bar SHALL arrange all navigation buttons horizontally in a `StackPanel` with `Orientation="Horizontal"`.
5. WHEN a navigation button is clicked, THE AdminDashboard SHALL provide a placeholder handler for future navigation logic.

---

### Requirement 4: Main Content Area Layout

**User Story:** As an admin, I want the main content divided into three proportional columns, so that related information is grouped and the layout is balanced on wide screens.

#### Acceptance Criteria

1. THE Main_Content_Area SHALL use a `Grid` with three `ColumnDefinition` entries using proportional widths (e.g., `1*`, `0.8*`, `1.2*`).
2. THE Main_Content_Area SHALL display a greeting text "Hello Admin Name! 👋" styled with the Purple_Accent color and bold font.
3. THE Left_Column SHALL occupy the first column of the Main_Content_Area grid.
4. THE Center_Column SHALL occupy the second column of the Main_Content_Area grid.
5. THE Right_Column SHALL occupy the third column of the Main_Content_Area grid.

---

### Requirement 5: Stats Grid

**User Story:** As an admin, I want a 2×2 grid of key metric cards, so that I can see the most important workforce numbers at a glance.

#### Acceptance Criteria

1. THE Stats_Grid SHALL be positioned at the top of the Left_Column.
2. THE Stats_Grid SHALL contain exactly four Stat_Card elements arranged in a 2×2 grid.
3. THE Stats_Grid Stat_Card titles SHALL be: "Number of Employees", "On Leave", "New Joinee", "Work Holidays / Others".
4. EACH Stat_Card SHALL be a Card_Surface (dark rounded container) containing a title `TextBlock` and a Circle_Button with a circle-arrow icon.
5. THE Stats_Grid Stat_Card title text SHALL use the Purple_Accent color.

---

### Requirement 6: Attendance Panel

**User Story:** As an admin, I want an attendance panel showing the current date, time, and clock-in controls, so that I can manage my own attendance directly from the dashboard.

#### Acceptance Criteria

1. THE Attendance_Panel SHALL be positioned below the Stats_Grid in the Left_Column.
2. THE Attendance_Panel SHALL be a Card_Surface containing a title label, a date label, a time label, and a "Clock In" status label.
3. THE Attendance_Panel SHALL contain two side-by-side Pill_Button controls: one labeled "Clock IN/OUT" and one labeled "LUNCH BREAK".
4. THE Attendance_Panel Pill_Button controls SHALL use the Purple_Accent color as their background.
5. WHEN the "Clock IN/OUT" button is clicked, THE AdminDashboard SHALL provide a placeholder handler for future attendance logic.
6. WHEN the "LUNCH BREAK" button is clicked, THE AdminDashboard SHALL provide a placeholder handler for future lunch-break logic.

---

### Requirement 7: Leave Approvals Panel

**User Story:** As an admin, I want a leave approvals panel showing pending requests, so that I can quickly review and act on employee leave submissions.

#### Acceptance Criteria

1. THE Leave_Approvals_Panel SHALL be positioned below the Attendance_Panel in the Left_Column.
2. THE Leave_Approvals_Panel SHALL be a Card_Surface containing a panel title label.
3. THE Leave_Approvals_Panel SHALL display a prominent circular highlight element labeled "May 23" styled with the Purple_Accent color.
4. THE Leave_Approvals_Panel SHALL display a circular profile placeholder, a name label, and a description text label to the right of the "May 23" highlight.
5. THE Leave_Approvals_Panel text labels SHALL use the Purple_Accent color for the name and a lighter foreground for the description.

---

### Requirement 8: Calendar View

**User Story:** As an admin, I want a full-month calendar in the center column, so that I can see the current month's schedule and identify highlighted dates.

#### Acceptance Criteria

1. THE Calendar_View SHALL be positioned in the Center_Column of the Main_Content_Area.
2. THE Calendar_View SHALL be implemented as a custom grid-based layout and SHALL NOT use the default WPF `Calendar` control.
3. THE Calendar_View SHALL display a 7-column day-of-week header row (Sun through Sat or Mon through Sun).
4. THE Calendar_View SHALL display a full month grid of date cells (up to 6 rows × 7 columns).
5. THE Calendar_View SHALL highlight exactly one date cell using the Purple_Accent color as the cell background to indicate the current or selected date.
6. THE Calendar_View SHALL display a month/year label (e.g., "May 2025") above the day grid.
7. THE Calendar_View date cells and header cells SHALL use consistent font sizing and the established color palette.

---

### Requirement 9: Employee List

**User Story:** As an admin, I want a scrollable list of my employees in the right column, so that I can quickly review team members and their roles.

#### Acceptance Criteria

1. THE Employee_List SHALL be positioned inside a Card_Surface in the Right_Column.
2. THE Right_Column Card_Surface SHALL display a "My Employees" header label styled with the Purple_Accent color.
3. THE Employee_List SHALL use a `ListView` or `ItemsControl` with vertical scrolling enabled.
4. THE Employee_List scrollbar SHALL use the Dark_Scrollbar style (dark track, rounded purple thumb) consistent with the `CustomScrollbarStyle` defined in `App.xaml`.
5. EACH Employee_Entry SHALL display a circular avatar placeholder, a name `TextBlock`, and a position `TextBlock`.
6. THE Employee_List SHALL be populated with at least five placeholder Employee_Entry items for design verification.
7. THE Right_Column Card_Surface SHALL contain a Circle_Button with a circle-arrow icon positioned at the bottom-right of the container as a footer action button.

---

### Requirement 10: Visual Consistency and Styling

**User Story:** As an admin, I want the dashboard to be visually consistent with the existing application theme, so that the UI feels cohesive and professional.

#### Acceptance Criteria

1. THE AdminDashboard SHALL apply the Purple_Accent color (`#7b61ff`) to all text labels, headings, and primary interactive elements.
2. THE AdminDashboard SHALL use Card_Surface containers (background `#15151b`, rounded corners) for all panel and card elements.
3. THE AdminDashboard SHALL use only flat, vector-like Unicode characters or Path geometry for icons (search, bell, arrows, triangle) with no external image dependencies for icons.
4. THE AdminDashboard SHALL apply consistent `CornerRadius` values: ≥ 20 for Pill_Button elements, equal half-size for Circle_Button elements, and ≥ 10 for Card_Surface containers.
5. THE AdminDashboard SHALL use a consistent font hierarchy: bold weight for section headers and normal weight for detail labels, all in the Purple_Accent or white foreground.
6. WHERE the `CustomScrollbarStyle` is defined in `App.xaml`, THE AdminDashboard SHALL reference it for all scrollable regions rather than redefining it inline.
