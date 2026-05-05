// AdminEmployeesViewTests.cs
// Example-based smoke tests for the AdminEmployeesView window.
//
// Tests covered:
//   Constructor_SetsUserNameButtonContent          – UserNameButton.Content equals the provided username (Req 1.2)
//   BackButton_Click_ShowsOwnerAndClosesView        – back navigation restores owner dashboard (Req 1.3)
//   EmployeesButton_IsActiveTab                    – EmployeesButton has PurpleAccentBrush bg and Opacity 1.0 (Req 2.1, 2.3)
//   Employees_HasAtLeastThreeEntriesAfterConstruct  – fallback guarantee when DB is unavailable (Req 6.2, 6.3)
//   EmployeeListControl_CountMatchesEmployees       – ItemsControl item count equals Employees.Count (Req 5.1, 6.1)
//
// All WPF tests run on a dedicated STA thread via StaHelper.RunOnSta() and call
// WpfAppBootstrap.EnsureInitialized() to merge App.xaml resources — reusing the
// helpers already defined in AdminDashboardTests.cs.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xunit;

namespace SOFTDEV.Tests
{
    /// <summary>
    /// Example-based tests for <see cref="AdminEmployeesView"/>.
    /// </summary>
    public class AdminEmployeesViewTests
    {
        // -----------------------------------------------------------------------
        // Test 4.1 — Constructor sets UserNameButton.Content to the provided username
        // Validates: Requirement 1.2
        // -----------------------------------------------------------------------

        /// <summary>
        /// When AdminEmployeesView is constructed with a username string, the
        /// UserNameButton.Content property must equal that exact string.
        /// </summary>
        [Fact(DisplayName = "AdminEmployeesView: constructor sets UserNameButton.Content to provided username")]
        public void Constructor_SetsUserNameButtonContent()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                const string username = "Alice";
                var window = new AdminEmployeesView(username, ownerDashboard: null);

                var userNameButton = window.FindName("UserNameButton") as Button;
                Assert.NotNull(userNameButton);
                Assert.Equal(username, userNameButton!.Content);

                window.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 4.2 — BackButton_Click calls Show() on the owner and closes the view
        // Validates: Requirement 1.3
        // -----------------------------------------------------------------------

        /// <summary>
        /// Invoking BackButton_Click must call Show() on the owner dashboard window
        /// and close AdminEmployeesView itself.
        ///
        /// Strategy: pass a real (but never-shown) Window as the owner stub.
        /// BackButton_Click is a private method, so we invoke it via reflection.
        /// After invocation, assert that the owner window is visible
        /// (Visibility == Visible) and that the employees view is no longer loaded
        /// (IsLoaded == false after Close()).
        /// </summary>
        [Fact(DisplayName = "AdminEmployeesView: BackButton_Click shows owner and closes the view")]
        public void BackButton_Click_ShowsOwnerAndClosesView()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                // Create a stub owner window — never shown, so it starts hidden.
                var ownerStub = new Window
                {
                    Width         = 100,
                    Height        = 100,
                    WindowStyle   = WindowStyle.None,
                    ShowInTaskbar = false,
                    Visibility    = Visibility.Hidden,
                };

                var employeesView = new AdminEmployeesView("TestUser", ownerStub);

                // BackButton_Click is private — invoke it via reflection.
                var method = typeof(AdminEmployeesView).GetMethod(
                    "BackButton_Click",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                Assert.NotNull(method);

                method!.Invoke(employeesView, new object[]
                {
                    employeesView,                          // sender (unused)
                    new RoutedEventArgs(Button.ClickEvent)  // event args (unused)
                });

                // The owner stub must now be visible (Show() was called).
                Assert.Equal(Visibility.Visible, ownerStub.Visibility);

                // The employees view must be closed (IsLoaded becomes false after Close()).
                Assert.False(employeesView.IsLoaded,
                    "AdminEmployeesView should be closed after BackButton_Click.");

                // Clean up the owner stub.
                ownerStub.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 4.3 — FilterButton_Click does not throw
        // Validates: Requirement 4.5
        // -----------------------------------------------------------------------

        /// <summary>
        /// Invoking FilterButton_Click (a placeholder handler that only logs to
        /// System.Diagnostics.Debug) must not throw any exception.
        ///
        /// Strategy: raise the Button.Click routed event on FilterButton via
        /// RaiseEvent so the handler is invoked through the normal WPF event
        /// pipeline. Record.Exception is used to assert no exception propagates.
        /// </summary>
        [Fact(DisplayName = "AdminEmployeesView: FilterButton_Click does not throw")]
        public void FilterButton_Click_DoesNotThrow()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminEmployeesView("TestUser", ownerDashboard: null);

                var filterButton = window.FindName("FilterButton") as Button;
                Assert.NotNull(filterButton);

                // Raise the Click routed event — this invokes FilterButton_Click
                // through the normal WPF event pipeline.
                var ex = Record.Exception(() =>
                    filterButton!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)));

                Assert.Null(ex);

                window.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 4.4 — SortButton_Click does not throw
        // Validates: Requirements 4.5
        // -----------------------------------------------------------------------

        /// <summary>
        /// Clicking SortButton must not throw any exception.
        /// SortButton_Click is a placeholder that only logs to Debug.
        /// </summary>
        [Fact(DisplayName = "AdminEmployeesView: SortButton_Click does not throw")]
        public void SortButton_Click_DoesNotThrow()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminEmployeesView("TestUser", ownerDashboard: null);

                var sortButton = window.FindName("SortButton") as Button;
                Assert.NotNull(sortButton);

                // Raise the Click routed event — this invokes SortButton_Click
                // through the normal WPF event pipeline.
                var ex = Record.Exception(() =>
                    sortButton!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)));

                Assert.Null(ex);

                window.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 4.5 — Control group button handlers do not throw
        // Validates: Requirement 8.2
        // -----------------------------------------------------------------------

        /// <summary>
        /// Clicking each of the four control group buttons (SearchButton,
        /// NotificationButton, UserNameButton, AvatarButton) must not throw any
        /// exception. Each handler is a placeholder that only logs to
        /// System.Diagnostics.Debug.
        ///
        /// Strategy: raise the Button.Click routed event on each button via
        /// RaiseEvent so the handler is invoked through the normal WPF event
        /// pipeline. Record.Exception is used to assert no exception propagates.
        /// </summary>
        [Fact(DisplayName = "AdminEmployeesView: SearchButton_Click does not throw")]
        public void SearchButton_Click_DoesNotThrow()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminEmployeesView("TestUser", ownerDashboard: null);

                var searchButton = window.FindName("SearchButton") as Button;
                Assert.NotNull(searchButton);

                var ex = Record.Exception(() =>
                    searchButton!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)));

                Assert.Null(ex);

                window.Close();
            });
        }

        [Fact(DisplayName = "AdminEmployeesView: NotificationButton_Click does not throw")]
        public void NotificationButton_Click_DoesNotThrow()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminEmployeesView("TestUser", ownerDashboard: null);

                var notificationButton = window.FindName("NotificationButton") as Button;
                Assert.NotNull(notificationButton);

                var ex = Record.Exception(() =>
                    notificationButton!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)));

                Assert.Null(ex);

                window.Close();
            });
        }

        [Fact(DisplayName = "AdminEmployeesView: UserNameButton_Click does not throw")]
        public void UserNameButton_Click_DoesNotThrow()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminEmployeesView("TestUser", ownerDashboard: null);

                var userNameButton = window.FindName("UserNameButton") as Button;
                Assert.NotNull(userNameButton);

                var ex = Record.Exception(() =>
                    userNameButton!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)));

                Assert.Null(ex);

                window.Close();
            });
        }

        [Fact(DisplayName = "AdminEmployeesView: AvatarButton_Click does not throw")]
        public void AvatarButton_Click_DoesNotThrow()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminEmployeesView("TestUser", ownerDashboard: null);

                var avatarButton = window.FindName("AvatarButton") as Button;
                Assert.NotNull(avatarButton);

                var ex = Record.Exception(() =>
                    avatarButton!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)));

                Assert.Null(ex);

                window.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 4.6 — EmployeesButton is the active nav tab
        // Validates: Requirements 2.1, 2.3
        // -----------------------------------------------------------------------

        /// <summary>
        /// The EmployeesButton in the AdminEmployeesView nav bar must have its
        /// Background set to PurpleAccentBrush (#7b61ff) and Opacity equal to 1.0,
        /// indicating it is the active navigation tab.
        /// </summary>
        [Fact(DisplayName = "AdminEmployeesView: EmployeesButton has PurpleAccentBrush background and Opacity 1.0")]
        public void EmployeesButton_IsActiveTab()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminEmployeesView("TestUser", ownerDashboard: null);

                var employeesButton = window.FindName("EmployeesButton") as Button;
                Assert.NotNull(employeesButton);

                // Opacity must be exactly 1.0 (active state).
                Assert.Equal(1.0, employeesButton!.Opacity);

                // Background must resolve to the PurpleAccentBrush color (#7b61ff).
                var purpleAccentBrush = Application.Current.Resources["PurpleAccentBrush"] as SolidColorBrush;
                Assert.NotNull(purpleAccentBrush);

                var buttonBg = employeesButton.Background as SolidColorBrush;
                Assert.NotNull(buttonBg);
                Assert.Equal(purpleAccentBrush!.Color, buttonBg!.Color);

                window.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 4.7 — Employees property has at least 3 entries after construction
        // Validates: Requirements 6.2, 6.3
        // -----------------------------------------------------------------------

        /// <summary>
        /// After construction, AdminEmployeesView.Employees must contain at least
        /// three entries. In the test environment the database is unavailable, so
        /// LoadEmployees() must fall back to the hardcoded placeholder list of
        /// Alice Santos, Bob Reyes, and Carol Lim.
        /// </summary>
        [Fact(DisplayName = "AdminEmployeesView: Employees property has at least 3 entries after construction")]
        public void Employees_HasAtLeastThreeEntriesAfterConstruction()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminEmployeesView("TestUser", ownerDashboard: null);

                Assert.NotNull(window.Employees);
                Assert.True(window.Employees.Count >= 3,
                    $"Expected at least 3 employees (fallback list), but found {window.Employees.Count}.");

                window.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Test 4.8 — EmployeeListControl.Items.Count equals Employees.Count
        // Validates: Requirements 5.1, 6.1
        // -----------------------------------------------------------------------

        /// <summary>
        /// After construction, the number of items rendered in EmployeeListControl
        /// must equal the count of entries in the Employees property.
        /// This verifies that LoadEmployees() correctly binds the collection to
        /// the ItemsControl.
        /// </summary>
        [Fact(DisplayName = "AdminEmployeesView: EmployeeListControl.Items.Count equals Employees.Count")]
        public void EmployeeListControl_CountMatchesEmployees()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminEmployeesView("TestUser", ownerDashboard: null);

                var listControl = window.FindName("EmployeeListControl") as ItemsControl;
                Assert.NotNull(listControl);

                Assert.Equal(window.Employees.Count, listControl!.Items.Count);

                window.Close();
            });
        }
    }
}
