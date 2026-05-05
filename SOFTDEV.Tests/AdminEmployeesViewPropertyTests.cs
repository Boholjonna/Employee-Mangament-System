// AdminEmployeesViewPropertyTests.cs
// Property-based tests for AdminEmployeesView using FsCheck.
//
// Properties covered:
//   Property 1 — Username Display Invariant           (Requirement 1.2)
//   Property 2 — ItemsControl Count Matches Employee List (Requirements 5.1, 6.1)
//   Property 3 — Fallback Guarantee on DB Failure     (Requirements 6.2, 6.3)
//
// All WPF-dependent properties run on an STA thread via StaHelper.RunOnSta.
// Each property runs a minimum of 100 FsCheck iterations.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Xunit;

namespace SOFTDEV.Tests
{
    public class AdminEmployeesViewPropertyTests
    {
        // -----------------------------------------------------------------------
        // FsCheck generators
        // -----------------------------------------------------------------------

        /// <summary>
        /// Generates a non-null, non-empty string of printable ASCII characters.
        /// </summary>
        private static Gen<string> NonEmptyStringGen() =>
            Gen.NonEmptyListOf(Gen.Choose(32, 126).Select(c => (char)c))
               .Select(chars => string.Concat(chars));

        /// <summary>
        /// Generates a list of 1–20 <see cref="EmployeeEntry"/> objects, each with
        /// random non-empty Name and Position strings.
        /// </summary>
        private static Gen<List<EmployeeEntry>> EmployeeListGen() =>
            Gen.Choose(1, 20).SelectMany(count =>
                Gen.Zip(NonEmptyStringGen(), NonEmptyStringGen())
                   .ListOf(count)
                   .Select(pairs => pairs
                       .Select(p => new EmployeeEntry(p.Item1, p.Item2))
                       .ToList()));

        // -----------------------------------------------------------------------
        // Property 1 — Username Display Invariant
        //
        // For any non-null, non-empty username string, constructing
        // AdminEmployeesView(username, null) and reading UserNameButton.Content
        // must return the original username unchanged.
        //
        // Validates: Requirement 1.2
        // -----------------------------------------------------------------------

        /// <summary>
        /// For any non-empty username, the UserNameButton.Content must equal the
        /// username passed to the constructor.
        /// Minimum 100 FsCheck iterations.
        ///
        /// **Validates: Requirements 1.2**
        /// </summary>
        [Property(MaxTest = 100, DisplayName = "Property 1: Username Display Invariant")]
        public Property Property1_UsernameDisplayInvariant()
        {
            return Prop.ForAll(
                NonEmptyStringGen().ToArbitrary(),
                username =>
                {
                    bool result = false;
                    string failureReason = string.Empty;

                    StaHelper.RunOnSta(() =>
                    {
                        WpfAppBootstrap.EnsureInitialized();

                        var window = new AdminEmployeesView(username, null);
                        try
                        {
                            var userNameButton = window.FindName("UserNameButton") as Button;

                            if (userNameButton is null)
                            {
                                failureReason = "UserNameButton not found in AdminEmployeesView.";
                                return;
                            }

                            if (!Equals(userNameButton.Content, username))
                            {
                                failureReason =
                                    $"Expected UserNameButton.Content == \"{username}\", " +
                                    $"but got \"{userNameButton.Content}\".";
                                return;
                            }

                            result = true;
                        }
                        finally
                        {
                            window.Close();
                        }
                    });

                    return result.Label(failureReason);
                });
        }

        // -----------------------------------------------------------------------
        // Property 2 — ItemsControl Count Matches Employee List
        //
        // For any randomly generated List<EmployeeEntry> (1–20 entries), after
        // assigning it to window.Employees and refreshing EmployeeListControl,
        // EmployeeListControl.Items.Count must equal window.Employees.Count.
        //
        // Validates: Requirements 5.1, 6.1
        // -----------------------------------------------------------------------

        /// <summary>
        /// For any list of EmployeeEntry objects (1–20 entries), assigning the list
        /// to window.Employees and refreshing EmployeeListControl.ItemsSource must
        /// result in EmployeeListControl.Items.Count == window.Employees.Count.
        /// Minimum 100 FsCheck iterations.
        ///
        /// **Validates: Requirements 5.1, 6.1**
        /// </summary>
        [Property(MaxTest = 100, DisplayName = "Property 2: ItemsControl Count Matches Employee List")]
        public Property Property2_ItemsControlCountMatchesEmployeeList()
        {
            return Prop.ForAll(
                EmployeeListGen().ToArbitrary(),
                employees =>
                {
                    bool result = false;
                    string failureReason = string.Empty;

                    StaHelper.RunOnSta(() =>
                    {
                        WpfAppBootstrap.EnsureInitialized();

                        var window = new AdminEmployeesView("TestUser", null);
                        try
                        {
                            var listControl = window.FindName("EmployeeListControl") as ItemsControl;

                            if (listControl is null)
                            {
                                failureReason = "EmployeeListControl not found in AdminEmployeesView.";
                                return;
                            }

                            // Assign the generated list and refresh the ItemsSource binding
                            window.Employees = employees;
                            listControl.ItemsSource = null;
                            listControl.ItemsSource = window.Employees;

                            if (listControl.Items.Count != window.Employees.Count)
                            {
                                failureReason =
                                    $"Expected EmployeeListControl.Items.Count == {window.Employees.Count}, " +
                                    $"but got {listControl.Items.Count}.";
                                return;
                            }

                            result = true;
                        }
                        finally
                        {
                            window.Close();
                        }
                    });

                    return result.Label(failureReason);
                });
        }

        // -----------------------------------------------------------------------
        // Property 3 — Fallback Guarantee on DB Failure
        //
        // Since LoadEmployees() calls DatabaseHelper.GetAllEmployees() which will
        // fail in the test environment (no DB), the constructor already exercises
        // the fallback path. This property verifies the invariant holds across
        // 100 iterations, and also directly tests the empty-list path via reflection.
        //
        // Validates: Requirements 6.2, 6.3
        // -----------------------------------------------------------------------

        /// <summary>
        /// After construction (which exercises the DB-failure fallback path),
        /// window.Employees.Count must be >= 3. Additionally, explicitly setting
        /// window.Employees to an empty list and invoking LoadEmployees() via
        /// reflection must also result in window.Employees.Count >= 3.
        /// Minimum 100 FsCheck iterations.
        ///
        /// **Validates: Requirements 6.2, 6.3**
        /// </summary>
        [Property(MaxTest = 100, DisplayName = "Property 3: Fallback Guarantee on DB Failure")]
        public Property Property3_FallbackGuaranteeOnDbFailure(bool _unused)
        {
            bool result = false;
            string failureReason = string.Empty;

            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var window = new AdminEmployeesView("TestUser", null);
                try
                {
                    // --- Part A: verify fallback was applied during construction ---
                    if (window.Employees is null)
                    {
                        failureReason = "window.Employees is null after construction.";
                        return;
                    }

                    if (window.Employees.Count < 3)
                    {
                        failureReason =
                            $"Expected window.Employees.Count >= 3 after construction " +
                            $"(fallback guarantee), but got {window.Employees.Count}.";
                        return;
                    }

                    // --- Part B: verify fallback is applied when Employees is empty ---
                    window.Employees = new List<EmployeeEntry>();

                    var loadMethod = typeof(AdminEmployeesView).GetMethod(
                        "LoadEmployees",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                    if (loadMethod is null)
                    {
                        failureReason = "LoadEmployees() private method not found via reflection.";
                        return;
                    }

                    loadMethod.Invoke(window, null);

                    if (window.Employees.Count < 3)
                    {
                        failureReason =
                            $"Expected window.Employees.Count >= 3 after invoking LoadEmployees() " +
                            $"on an empty list (fallback guarantee), but got {window.Employees.Count}.";
                        return;
                    }

                    result = true;
                }
                finally
                {
                    window.Close();
                }
            });

            return result.Label(failureReason);
        }
    }
}
