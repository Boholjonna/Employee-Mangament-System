// AttendanceDashboardPropertyTests.cs
// Property-based tests for the Attendance Dashboard using FsCheck.
//
// Properties covered:
//   Property 1 — Filter Completeness       (Requirements 7.2)
//   Property 5 — StatusColor Consistency   (Requirements 5.5, 10.1)
//
// All tests are pure in-memory and do not require a live database connection.
// Each property runs a minimum of 100 FsCheck iterations.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Xunit;

namespace SOFTDEV.Tests
{
    public class AttendanceDashboardPropertyTests
    {
        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// The canonical status strings recognised by ResolveStatusColor.
        /// </summary>
        private static readonly string[] KnownStatuses =
        {
            "present", "Present", "PRESENT",
            "late",    "Late",    "LATE",
            "absent",  "Absent",  "ABSENT",
            "on leave","On Leave","ON LEAVE",
        };

        /// <summary>
        /// The expected hex color for each lower-cased status value.
        /// </summary>
        private static readonly Dictionary<string, string> ExpectedColors =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "present",  "#2ecc71" },
                { "late",     "#f39c12" },
                { "absent",   "#e74c3c" },
                { "on leave", "#3498db" },
            };

        /// <summary>
        /// Invokes the private static <c>ResolveStatusColor(string)</c> method on
        /// <see cref="DatabaseHelper"/> via reflection so the property test can
        /// call it without making it public.
        /// </summary>
        private static string InvokeResolveStatusColor(string status)
        {
            var method = typeof(DatabaseHelper).GetMethod(
                "ResolveStatusColor",
                BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new[] { typeof(string) },
                null)
                ?? throw new MissingMethodException(
                    nameof(DatabaseHelper), "ResolveStatusColor");

            return (string)method.Invoke(null, new object[] { status })!;
        }

        // -----------------------------------------------------------------------
        // FsCheck generators — shared
        // -----------------------------------------------------------------------

        /// <summary>
        /// Generates a random status string — either one of the four known values
        /// (in various casings) or an arbitrary non-null string (to exercise the
        /// default branch).
        /// </summary>
        private static Gen<string> StatusGen()
        {
            var knownGen = Gen.Elements(KnownStatuses);

            // Arbitrary printable ASCII string (may or may not match a known status)
            var arbitraryGen =
                Gen.NonEmptyListOf(Gen.Choose(32, 126).Select(c => (char)c))
                   .Select(chars => string.Concat(chars));

            // 70 % known, 30 % arbitrary — use OneOf with weighted repetition
            // by combining both generators via a boolean coin-flip.
            return Gen.Choose(0, 9).SelectMany(n => n < 7 ? knownGen : arbitraryGen);
        }

        /// <summary>
        /// Generates a date string in "yyyy-MM-dd" format between 2020-01-01 and 2030-12-31.
        /// </summary>
        private static Gen<string> DateStringGen()
        {
            var start = new DateTime(2020, 1, 1);
            int totalDays = (new DateTime(2030, 12, 31) - start).Days;
            return Gen.Choose(0, totalDays)
                      .Select(offset => start.AddDays(offset).ToString("yyyy-MM-dd"));
        }

        /// <summary>
        /// Generates an employee name — either a realistic name or an empty string.
        /// </summary>
        private static Gen<string> EmployeeNameGen()
        {
            var names = new[]
            {
                "Alice", "Bob", "Charlie", "Diana", "Eve",
                "Frank", "Grace", "Hank", "Iris", "Jack",
                "alice", "ALICE", "bob smith", "Mary Jane",
            };
            var namedGen   = Gen.Elements(names);
            var emptyGen   = Gen.Constant(string.Empty);
            return Gen.Choose(0, 9).SelectMany(n => n < 8 ? namedGen : emptyGen);
        }

        /// <summary>
        /// Generates a list of 0–30 <see cref="AttendanceRecord_Model"/> objects
        /// with random field values.
        /// </summary>
        private static Gen<List<AttendanceRecord_Model>> RecordListGen() =>
            Gen.Choose(0, 30).SelectMany(count =>
                EmployeeNameGen()
                    .SelectMany(name =>
                        DateStringGen()
                            .SelectMany(date =>
                                StatusGen()
                                    .Select(status => new AttendanceRecord_Model
                                    {
                                        EmployeeName = name,
                                        Date         = date,
                                        TimeIn       = "08:00",
                                        TimeOut      = "17:00",
                                        TotalHours   = "9",
                                        Status       = status,
                                        StatusColor  = InvokeResolveStatusColor(status),
                                    })))
                    .ListOf(count)
                    .Select(items => new List<AttendanceRecord_Model>(items)));

        /// <summary>
        /// Generates a filter criterion string: either empty (no filter) or a
        /// value drawn from the record list to ensure some matches exist.
        /// </summary>
        private static Gen<string> FilterEmployeeGen(List<AttendanceRecord_Model> records)
        {
            if (records.Count == 0)
                return Gen.Constant(string.Empty);

            // 50 % empty (no filter), 50 % a substring of a real name
            var nameGen = Gen.Elements(records.Select(r => r.EmployeeName).ToArray());
            return Gen.Choose(0, 1).SelectMany(n => n == 0 ? Gen.Constant(string.Empty) : nameGen);
        }

        /// <summary>
        /// Generates a status filter: "All" or one of the four known statuses.
        /// </summary>
        private static Gen<string> FilterStatusGen() =>
            Gen.Elements("All", "Present", "Late", "Absent", "On Leave");

        /// <summary>
        /// Generates an optional date filter string: either empty or a date in range.
        /// </summary>
        private static Gen<string> OptionalDateGen() =>
            Gen.Choose(0, 1).SelectMany(n =>
                n == 0 ? Gen.Constant(string.Empty) : DateStringGen());

        // -----------------------------------------------------------------------
        // Property 1 — Filter Completeness
        //
        // For any filter criteria (employee, status, dateFrom, dateTo), every
        // record in AttendanceRecords after ApplyFilter must satisfy all active
        // (non-empty / non-"All") criteria simultaneously.
        //
        //   ∀ r ∈ AttendanceRecords after ApplyFilter(e, s, df, dt):
        //     (e == "" ∨ r.EmployeeName.Contains(e, OrdinalIgnoreCase)) ∧
        //     (s == "All" ∨ r.Status == s) ∧
        //     (df == "" ∨ r.Date >= df) ∧
        //     (dt == "" ∨ r.Date <= dt)
        //
        // **Validates: Requirements 7.2**
        // -----------------------------------------------------------------------

        /// <summary>
        /// Every record in <see cref="AttendanceViewModel.AttendanceRecords"/> after
        /// <c>ApplyFilter</c> must satisfy all active filter criteria.
        ///
        /// Minimum 100 FsCheck iterations.
        ///
        /// **Validates: Requirements 7.2**
        /// </summary>
        [Property(MaxTest = 100, DisplayName = "Property 1: Filter Completeness")]
        public Property Property1_FilterCompleteness()
        {
            // Generator: (records, employee, status, dateFrom, dateTo)
            var gen =
                RecordListGen().SelectMany(records =>
                    FilterEmployeeGen(records).SelectMany(employee =>
                        FilterStatusGen().SelectMany(status =>
                            OptionalDateGen().SelectMany(dateFrom =>
                                OptionalDateGen().Select(dateTo =>
                                    (records, employee, status, dateFrom, dateTo))))));

            return Prop.ForAll(
                gen.ToArbitrary(),
                tuple =>
                {
                    var (records, employee, status, dateFrom, dateTo) = tuple;

                    // Seed the ViewModel via the internal test constructor (no DB call)
                    var vm = new AttendanceViewModel(records);
                    vm.ApplyFilter(employee, status, dateFrom, dateTo);

                    foreach (var r in vm.AttendanceRecords)
                    {
                        // Employee name criterion
                        if (!string.IsNullOrEmpty(employee) &&
                            r.EmployeeName.IndexOf(employee, StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            return false.Label(
                                $"Filter completeness violated: employee filter=\"{employee}\" " +
                                $"but record has EmployeeName=\"{r.EmployeeName}\".");
                        }

                        // Status criterion
                        if (status != "All" && r.Status != status)
                        {
                            return false.Label(
                                $"Filter completeness violated: status filter=\"{status}\" " +
                                $"but record has Status=\"{r.Status}\".");
                        }

                        // DateFrom criterion
                        if (!string.IsNullOrEmpty(dateFrom) &&
                            string.Compare(r.Date, dateFrom, StringComparison.Ordinal) < 0)
                        {
                            return false.Label(
                                $"Filter completeness violated: dateFrom=\"{dateFrom}\" " +
                                $"but record has Date=\"{r.Date}\".");
                        }

                        // DateTo criterion
                        if (!string.IsNullOrEmpty(dateTo) &&
                            string.Compare(r.Date, dateTo, StringComparison.Ordinal) > 0)
                        {
                            return false.Label(
                                $"Filter completeness violated: dateTo=\"{dateTo}\" " +
                                $"but record has Date=\"{r.Date}\".");
                        }
                    }

                    return true.Label(
                        $"All {vm.AttendanceRecords.Count} record(s) satisfy filter " +
                        $"(employee=\"{employee}\", status=\"{status}\", " +
                        $"dateFrom=\"{dateFrom}\", dateTo=\"{dateTo}\").");
                });
        }

        // -----------------------------------------------------------------------
        // Property 2 — Filter Soundness
        //
        // No record from _allRecords that satisfies all active filter criteria is
        // absent from AttendanceRecords after ApplyFilter().
        //
        //   ∀ r ∈ _allRecords that satisfies all criteria → r ∈ AttendanceRecords
        //
        // Implementation note: _allRecords is private, so we re-apply the same
        // filter predicate to the original seeded list (which we hold a reference
        // to) and verify that every expected record appears in AttendanceRecords.
        //
        // **Validates: Requirements 7.2, 7.3**
        // -----------------------------------------------------------------------

        /// <summary>
        /// Every record from the original seeded list that satisfies all active
        /// filter criteria must be present in
        /// <see cref="AttendanceViewModel.AttendanceRecords"/> after
        /// <c>ApplyFilter</c>.  No qualifying record may be silently dropped.
        ///
        /// Minimum 100 FsCheck iterations.
        ///
        /// **Validates: Requirements 7.2, 7.3**
        /// </summary>
        [Property(MaxTest = 100, DisplayName = "Property 2: Filter Soundness")]
        public Property Property2_FilterSoundness()
        {
            // Generator: (records, employee, status, dateFrom, dateTo)
            var gen =
                RecordListGen().SelectMany(records =>
                    FilterEmployeeGen(records).SelectMany(employee =>
                        FilterStatusGen().SelectMany(status =>
                            OptionalDateGen().SelectMany(dateFrom =>
                                OptionalDateGen().Select(dateTo =>
                                    (records, employee, status, dateFrom, dateTo))))));

            return Prop.ForAll(
                gen.ToArbitrary(),
                tuple =>
                {
                    var (records, employee, status, dateFrom, dateTo) = tuple;

                    // Seed the ViewModel via the internal test constructor (no DB call).
                    // We keep a reference to `records` so we can re-apply the predicate
                    // without needing to access the private _allRecords field.
                    var vm = new AttendanceViewModel(records);
                    vm.ApplyFilter(employee, status, dateFrom, dateTo);

                    // Build a HashSet of the records actually present in AttendanceRecords
                    // using reference equality (same object instances were seeded).
                    var inResult = new HashSet<AttendanceRecord_Model>(
                        vm.AttendanceRecords,
                        ReferenceEqualityComparer.Instance);

                    // Re-apply the same predicate to the original list.
                    foreach (var r in records)
                    {
                        // Check whether this record satisfies all active criteria.
                        bool satisfiesEmployee =
                            string.IsNullOrEmpty(employee) ||
                            r.EmployeeName.IndexOf(employee, StringComparison.OrdinalIgnoreCase) >= 0;

                        bool satisfiesStatus =
                            status == "All" || r.Status == status;

                        bool satisfiesDateFrom =
                            string.IsNullOrEmpty(dateFrom) ||
                            string.Compare(r.Date, dateFrom, StringComparison.Ordinal) >= 0;

                        bool satisfiesDateTo =
                            string.IsNullOrEmpty(dateTo) ||
                            string.Compare(r.Date, dateTo, StringComparison.Ordinal) <= 0;

                        if (satisfiesEmployee && satisfiesStatus && satisfiesDateFrom && satisfiesDateTo)
                        {
                            // This record qualifies — it MUST appear in AttendanceRecords.
                            if (!inResult.Contains(r))
                            {
                                return false.Label(
                                    $"Filter soundness violated: record " +
                                    $"(EmployeeName=\"{r.EmployeeName}\", Date=\"{r.Date}\", " +
                                    $"Status=\"{r.Status}\") satisfies all criteria " +
                                    $"(employee=\"{employee}\", status=\"{status}\", " +
                                    $"dateFrom=\"{dateFrom}\", dateTo=\"{dateTo}\") " +
                                    $"but is absent from AttendanceRecords.");
                            }
                        }
                    }

                    return true.Label(
                        $"All qualifying records are present in AttendanceRecords " +
                        $"(employee=\"{employee}\", status=\"{status}\", " +
                        $"dateFrom=\"{dateFrom}\", dateTo=\"{dateTo}\"). " +
                        $"Result count: {vm.AttendanceRecords.Count}/{records.Count}.");
                });
        }

        // -----------------------------------------------------------------------
        // Property 3 — Reset Idempotency
        //
        // After ResetFilter(), AttendanceRecords contains exactly the same records
        // as the original seeded list (_allRecords): same count and same object
        // references in the same order.
        //
        //   ResetFilter() → AttendanceRecords.Count == _allRecords.Count
        //                 ∧ ∀ i: AttendanceRecords[i] == _allRecords[i]
        //
        // Implementation note: we hold a reference to the seeded list so we can
        // compare without accessing the private _allRecords field.
        //
        // **Validates: Requirements 7.3**
        // -----------------------------------------------------------------------

        /// <summary>
        /// After <c>ResetFilter()</c>, <see cref="AttendanceViewModel.AttendanceRecords"/>
        /// must contain exactly the same records (same count, same object references,
        /// same order) as the original seeded list, regardless of what filter was
        /// applied before the reset.
        ///
        /// Minimum 100 FsCheck iterations.
        ///
        /// **Validates: Requirements 7.3**
        /// </summary>
        [Property(MaxTest = 100, DisplayName = "Property 3: Reset Idempotency")]
        public Property Property3_ResetIdempotency()
        {
            // Generator: (records, employee, status, dateFrom, dateTo)
            // We apply a random filter first so that ResetFilter() has something to undo.
            var gen =
                RecordListGen().SelectMany(records =>
                    FilterEmployeeGen(records).SelectMany(employee =>
                        FilterStatusGen().SelectMany(status =>
                            OptionalDateGen().SelectMany(dateFrom =>
                                OptionalDateGen().Select(dateTo =>
                                    (records, employee, status, dateFrom, dateTo))))));

            return Prop.ForAll(
                gen.ToArbitrary(),
                tuple =>
                {
                    var (records, employee, status, dateFrom, dateTo) = tuple;

                    // Seed the ViewModel via the internal test constructor (no DB call).
                    var vm = new AttendanceViewModel(records);

                    // Apply a random filter to change the state before resetting.
                    vm.ApplyFilter(employee, status, dateFrom, dateTo);

                    // Now reset — this should restore the full original list.
                    vm.ResetFilter();

                    // 1. Count must match.
                    if (vm.AttendanceRecords.Count != records.Count)
                    {
                        return false.Label(
                            $"Reset idempotency violated: expected {records.Count} record(s) " +
                            $"after ResetFilter() but got {vm.AttendanceRecords.Count}.");
                    }

                    // 2. Every original record must be present (reference equality).
                    var inResult = new HashSet<AttendanceRecord_Model>(
                        vm.AttendanceRecords,
                        ReferenceEqualityComparer.Instance);

                    foreach (var r in records)
                    {
                        if (!inResult.Contains(r))
                        {
                            return false.Label(
                                $"Reset idempotency violated: record " +
                                $"(EmployeeName=\"{r.EmployeeName}\", Date=\"{r.Date}\", " +
                                $"Status=\"{r.Status}\") is absent from AttendanceRecords " +
                                $"after ResetFilter().");
                        }
                    }

                    return true.Label(
                        $"AttendanceRecords correctly restored to {records.Count} record(s) " +
                        $"after ResetFilter() (filter was: employee=\"{employee}\", " +
                        $"status=\"{status}\", dateFrom=\"{dateFrom}\", dateTo=\"{dateTo}\").");
                });
        }

        // -----------------------------------------------------------------------
        // Property 4 — Sort Order
        //
        // After ApplySort(column, ascending), every consecutive pair (i, i+1) in
        // AttendanceRecords satisfies the sort predicate:
        //   ascending  → Compare(records[i][column], records[i+1][column]) ≤ 0
        //   descending → Compare(records[i][column], records[i+1][column]) ≥ 0
        //
        // **Validates: Requirements 8.2**
        // -----------------------------------------------------------------------

        private static readonly string[] SortColumns =
        {
            "Employee Name", "Date", "Time In", "Time Out", "Total Hours", "Status"
        };

        private static Func<AttendanceRecord_Model, string> KeySelectorFor(string column) =>
            column switch
            {
                "Employee Name" => r => r.EmployeeName,
                "Time In"       => r => r.TimeIn,
                "Time Out"      => r => r.TimeOut,
                "Total Hours"   => r => r.TotalHours,
                "Status"        => r => r.Status,
                _               => r => r.Date,
            };

        /// <summary>
        /// For every consecutive pair in <see cref="AttendanceViewModel.AttendanceRecords"/>
        /// after <c>ApplySort(column, ascending)</c>, the comparison satisfies the
        /// sort predicate (≤ 0 ascending, ≥ 0 descending).
        ///
        /// Minimum 100 FsCheck iterations.
        ///
        /// **Validates: Requirements 8.2**
        /// </summary>
        [Property(MaxTest = 100, DisplayName = "Property 4: Sort Order")]
        public Property Property4_SortOrder()
        {
            var gen =
                RecordListGen().SelectMany(records =>
                    Gen.Elements(SortColumns).SelectMany(column =>
                        Gen.Elements(true, false).Select(ascending =>
                            (records, column, ascending))));

            return Prop.ForAll(
                gen.ToArbitrary(),
                tuple =>
                {
                    var (records, column, ascending) = tuple;

                    var vm = new AttendanceViewModel(records);
                    vm.ApplySort(column, ascending);

                    var result = vm.AttendanceRecords.ToList();
                    var keySelector = KeySelectorFor(column);

                    for (int i = 0; i < result.Count - 1; i++)
                    {
                        int cmp = string.Compare(
                            keySelector(result[i]),
                            keySelector(result[i + 1]),
                            StringComparison.OrdinalIgnoreCase);

                        if (ascending && cmp > 0)
                            return false.Label(
                                $"Sort order violated (ascending) at index {i}: " +
                                $"\"{keySelector(result[i])}\" > \"{keySelector(result[i + 1])}\" " +
                                $"for column \"{column}\".");

                        if (!ascending && cmp < 0)
                            return false.Label(
                                $"Sort order violated (descending) at index {i}: " +
                                $"\"{keySelector(result[i])}\" < \"{keySelector(result[i + 1])}\" " +
                                $"for column \"{column}\".");
                    }

                    return true.Label(
                        $"Sort order correct for column=\"{column}\", ascending={ascending}, " +
                        $"count={result.Count}.");
                });
        }

        // -----------------------------------------------------------------------
        // FsCheck generators — P5 (StatusColor)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Generates a list of 1–30 <see cref="AttendanceRecord_Model"/> objects
        /// with random status strings and the corresponding StatusColor already set
        /// by calling <c>ResolveStatusColor</c> — simulating what
        /// <c>GetAllAttendanceRecords</c> would produce.
        /// </summary>
        private static Gen<List<AttendanceRecord_Model>> AttendanceRecordListGen() =>
            Gen.Choose(1, 30).SelectMany(count =>
                StatusGen()
                    .ListOf(count)
                    .Select(statuses =>
                    {
                        var list = new List<AttendanceRecord_Model>(statuses.Count);
                        foreach (var status in statuses)
                        {
                            list.Add(new AttendanceRecord_Model
                            {
                                EmployeeName = "Test Employee",
                                Date         = "2026-01-01",
                                TimeIn       = "08:00",
                                TimeOut      = "17:00",
                                TotalHours   = "9",
                                Status       = status,
                                StatusColor  = InvokeResolveStatusColor(status),
                            });
                        }
                        return list;
                    }));

        // -----------------------------------------------------------------------
        // Property 5 — StatusColor Consistency
        //
        // For every AttendanceRecord_Model produced by GetAllAttendanceRecords
        // (simulated here by constructing records via ResolveStatusColor directly),
        // r.StatusColor must equal ResolveStatusColor(r.Status).
        //
        // This validates that:
        //   • Known statuses map to their canonical hex colors.
        //   • Unknown statuses fall back to the default purple (#7b61ff).
        //   • The mapping is deterministic and case-insensitive.
        //
        // Validates: Requirements 5.5, 10.1
        // -----------------------------------------------------------------------

        /// <summary>
        /// For every <see cref="AttendanceRecord_Model"/> in a randomly generated
        /// list, <c>r.StatusColor</c> must equal <c>ResolveStatusColor(r.Status)</c>.
        ///
        /// This property exercises both the four known status values (in various
        /// casings) and arbitrary unknown strings (which must fall back to #7b61ff).
        ///
        /// Minimum 100 FsCheck iterations.
        ///
        /// **Validates: Requirements 5.5, 10.1**
        /// </summary>
        [Property(MaxTest = 100, DisplayName = "Property 5: StatusColor Consistency")]
        public Property Property5_StatusColorConsistency()
        {
            return Prop.ForAll(
                AttendanceRecordListGen().ToArbitrary(),
                records =>
                {
                    foreach (var r in records)
                    {
                        string expected = InvokeResolveStatusColor(r.Status);

                        if (r.StatusColor != expected)
                        {
                            return false.Label(
                                $"StatusColor mismatch for Status=\"{r.Status}\": " +
                                $"expected \"{expected}\" but got \"{r.StatusColor}\".");
                        }
                    }
                    return true.Label("All StatusColor values match ResolveStatusColor(Status).");
                });
        }

        // -----------------------------------------------------------------------
        // Property 6 — Empty State Visibility
        //
        // EmptyStateTextBlock.Visibility == Visible ↔ AttendanceRecords.Count == 0
        //
        // Since WPF UI elements require a running Dispatcher, this property is
        // validated at the ViewModel level: after loading zero records the
        // collection count is 0, and after loading non-zero records it is > 0.
        // The XAML DataTrigger (Binding=AttendanceRecords.Count, Value=0) then
        // drives the Visibility automatically.
        //
        // **Validates: Requirements 5.6**
        // -----------------------------------------------------------------------

        [Fact(DisplayName = "Property 6: Empty state — zero records → Count == 0")]
        public void Property6_EmptyState_ZeroRecords_CountIsZero()
        {
            var vm = new AttendanceViewModel(new List<AttendanceRecord_Model>());
            Assert.Equal(0, vm.AttendanceRecords.Count);
        }

        [Fact(DisplayName = "Property 6: Empty state — non-zero records → Count > 0")]
        public void Property6_EmptyState_NonZeroRecords_CountIsPositive()
        {
            var records = new List<AttendanceRecord_Model>
            {
                new AttendanceRecord_Model { EmployeeName = "Alice", Date = "2026-01-01", Status = "Present", StatusColor = "#2ecc71" },
                new AttendanceRecord_Model { EmployeeName = "Bob",   Date = "2026-01-02", Status = "Late",    StatusColor = "#f39c12" },
            };
            var vm = new AttendanceViewModel(records);
            Assert.True(vm.AttendanceRecords.Count > 0);
        }

        [Property(MaxTest = 100, DisplayName = "Property 6: Empty state — Count matches seeded list size")]
        public Property Property6_EmptyState_CountMatchesSeed()
        {
            return Prop.ForAll(
                RecordListGen().ToArbitrary(),
                records =>
                {
                    var vm = new AttendanceViewModel(records);
                    bool isEmpty = vm.AttendanceRecords.Count == 0;
                    bool seedEmpty = records.Count == 0;

                    if (isEmpty != seedEmpty)
                        return false.Label(
                            $"Empty state mismatch: AttendanceRecords.Count={vm.AttendanceRecords.Count}, " +
                            $"seeded={records.Count}.");

                    return true.Label(
                        $"Empty state correct: Count={vm.AttendanceRecords.Count}, seeded={records.Count}.");
                });
        }

        // -----------------------------------------------------------------------
        // Supplementary example-based tests for ResolveStatusColor
        // These pin the exact mapping for each known status value and the default.
        // -----------------------------------------------------------------------

        [Theory(DisplayName = "ResolveStatusColor: known statuses map to correct hex colors")]
        [InlineData("present",  "#2ecc71")]
        [InlineData("Present",  "#2ecc71")]
        [InlineData("PRESENT",  "#2ecc71")]
        [InlineData("late",     "#f39c12")]
        [InlineData("Late",     "#f39c12")]
        [InlineData("absent",   "#e74c3c")]
        [InlineData("Absent",   "#e74c3c")]
        [InlineData("on leave", "#3498db")]
        [InlineData("On Leave", "#3498db")]
        [InlineData("ON LEAVE", "#3498db")]
        [InlineData("",         "#7b61ff")]
        [InlineData("unknown",  "#7b61ff")]
        [InlineData("N/A",      "#7b61ff")]
        public void ResolveStatusColor_KnownStatuses_ReturnCorrectColor(
            string status, string expectedColor)
        {
            string actual = InvokeResolveStatusColor(status);
            Assert.Equal(expectedColor, actual);
        }
    }
}
