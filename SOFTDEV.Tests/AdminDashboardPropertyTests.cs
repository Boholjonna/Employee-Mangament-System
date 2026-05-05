// AdminDashboardPropertyTests.cs
// Property-based tests for the AdminDashboard window using CsCheck.
//
// Properties covered:
//   Property 1 — Stat card structural invariant        (Requirements 5.4, 5.5)
//   Property 2 — Calendar cell coverage for any month  (Requirements 8.4, 8.5)
//   Property 3 — Employee entry structural invariant   (Requirements 9.5, 9.6)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CsCheck;
using Xunit;

namespace SOFTDEV.Tests
{
    // ---------------------------------------------------------------------------
    // Property 1 — Stat card structural invariant
    //
    // For any stat card in the Stats_Grid, the card must contain:
    //   • At least one TextBlock with Foreground equal to the purple accent (#7b61ff)
    //   • At least one Button whose Style is CircleButtonStyle
    //
    // The four stat cards are a fixed, finite set, so this is implemented as a
    // [Fact] that iterates over all four cards — satisfying the "for any stat card"
    // universal quantifier over the finite domain.
    //
    // Validates: Requirements 5.4, 5.5
    // ---------------------------------------------------------------------------
    public class AdminDashboardPropertyTests
    {
        // -----------------------------------------------------------------------
        // Property 1 — Stat card structural invariant
        // -----------------------------------------------------------------------

        /// <summary>
        /// Every stat card Border (StatCard0–StatCard3) must contain at least one
        /// TextBlock with the purple accent foreground and at least one Button using
        /// CircleButtonStyle.  This invariant must hold for all four cards.
        /// </summary>
        [Fact(DisplayName = "Property 1: every stat card has a purple TextBlock and a CircleButton")]
        public void StatCard_StructuralInvariant_HoldForAllFourCards()
        {
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();

                var dashboard = new AdminDashboard("TestUser");

                var purpleAccentColor = (Color)ColorConverter.ConvertFromString("#7b61ff");
                var circleButtonStyle = Application.Current.Resources["CircleButtonStyle"] as Style;
                Assert.NotNull(circleButtonStyle);

                for (int i = 0; i < 4; i++)
                {
                    var card = dashboard.FindName($"StatCard{i}") as Border;
                    Assert.NotNull(card);

                    // Collect all logical descendants
                    var textBlocks = GetLogicalDescendants<TextBlock>(card).ToList();
                    var buttons    = GetLogicalDescendants<Button>(card).ToList();

                    // At least one TextBlock must have the purple accent foreground
                    bool hasPurpleTextBlock = textBlocks.Any(tb =>
                    {
                        if (tb.Foreground is SolidColorBrush scb)
                            return scb.Color == purpleAccentColor;
                        return false;
                    });

                    Assert.True(hasPurpleTextBlock,
                        $"StatCard{i} does not contain a TextBlock with Foreground #7b61ff.");

                    // At least one Button must use CircleButtonStyle
                    bool hasCircleButton = buttons.Any(b => b.Style == circleButtonStyle);

                    Assert.True(hasCircleButton,
                        $"StatCard{i} does not contain a Button using CircleButtonStyle.");
                }

                dashboard.Close();
            });
        }

        // -----------------------------------------------------------------------
        // Property 2 — Calendar cell coverage for any month
        //
        // For any valid (year, month, highlightDay) triple, GenerateCalendarDays
        // must satisfy all five sub-properties simultaneously:
        //   1. Total cell count is between 28 and 42 (inclusive)
        //   2. Current-month cell count equals DateTime.DaysInMonth(year, month)
        //   3. Current-month day values form the contiguous sequence 1..DaysInMonth
        //   4. Leading padding cell count equals (int)new DateTime(year,month,1).DayOfWeek
        //   5. Exactly one cell is highlighted and its Day equals highlightDay
        //
        // Validates: Requirements 8.4, 8.5
        // -----------------------------------------------------------------------

        /// <summary>
        /// For any valid (year, month) pair and any highlightDay in [1, 28],
        /// the list produced by GenerateCalendarDays must satisfy all five
        /// calendar cell coverage sub-properties.
        /// </summary>
        [Fact(DisplayName = "Property 2: calendar cell coverage holds for any valid month")]
        public void CalendarCellCoverage_HoldsForAnyMonth()
        {
            // The entire test — dashboard creation, CsCheck sampling, and cleanup —
            // must run on the same STA thread because WPF objects are thread-affine.
            // StaHelper.RunOnSta spawns a single dedicated STA thread for the whole block.
            StaHelper.RunOnSta(() =>
            {
                WpfAppBootstrap.EnsureInitialized();
                var dashboard = new AdminDashboard("TestUser");

                try
                {
                    // Generator: (year, month, highlightDay)
                    // highlightDay is drawn from [1, 28] — safe minimum across all months
                    // (February in a non-leap year has 28 days).
                    var gen =
                        Gen.Int[1, 9999]
                           .SelectMany(year =>
                               Gen.Int[1, 12]
                                  .SelectMany(month =>
                                      Gen.Int[1, 28]
                                         .Select(hd => (year, month, hd))));

                    gen.Sample(triple =>
                    {
                        var (year, month, highlightDay) = triple;

                        List<CalendarDayItem> days = dashboard.GenerateCalendarDays(year, month, highlightDay);

                        int daysInMonth = DateTime.DaysInMonth(year, month);
                        int expectedLeadingPadding = (int)new DateTime(year, month, 1).DayOfWeek;

                        // Sub-property 1: total cell count in [28, 42]
                        Assert.True(days.Count >= 28 && days.Count <= 42,
                            $"({year}/{month}) total cell count {days.Count} is outside [28, 42].");

                        // Sub-property 2: current-month cell count == DaysInMonth
                        int currentMonthCount = days.Count(d => d.IsCurrentMonth);
                        Assert.Equal(daysInMonth, currentMonthCount);

                        // Sub-property 3: current-month day values form 1..DaysInMonth in order
                        var currentMonthDays = days
                            .Where(d => d.IsCurrentMonth)
                            .Select(d => d.Day)
                            .ToList();
                        var expectedSequence = Enumerable.Range(1, daysInMonth).ToList();
                        Assert.Equal(expectedSequence, currentMonthDays);

                        // Sub-property 4: leading padding count == DayOfWeek of the 1st
                        int leadingPadding = days.TakeWhile(d => !d.IsCurrentMonth).Count();
                        Assert.Equal(expectedLeadingPadding, leadingPadding);

                        // Sub-property 5: exactly one highlighted cell, and its Day == highlightDay
                        int highlightedCount = days.Count(d => d.IsHighlighted);
                        Assert.Equal(1, highlightedCount);

                        var highlightedCell = days.Single(d => d.IsHighlighted);
                        Assert.Equal(highlightDay, highlightedCell.Day);
                    });
                }
                finally
                {
                    dashboard.Close();
                }
            });
        }

        // -----------------------------------------------------------------------
        // Property 3 — Employee entry structural invariant
        //
        // For any EmployeeEntry constructed with a non-empty Name and non-empty
        // Position, reading back the properties must return the original values
        // unchanged (round-trip identity).
        //
        // Validates: Requirements 9.5, 9.6
        // -----------------------------------------------------------------------

        /// <summary>
        /// For any non-empty (name, position) pair, constructing an EmployeeEntry
        /// and reading back Name and Position must return the original values.
        /// </summary>
        [Fact(DisplayName = "Property 3: EmployeeEntry round-trips Name and Position unchanged")]
        public void EmployeeEntry_RoundTrip_NameAndPositionUnchanged()
        {
            // Generator: pairs of non-null, non-empty strings
            var gen =
                Gen.String
                   .Where(s => !string.IsNullOrEmpty(s))
                   .SelectMany(name =>
                       Gen.String
                          .Where(s => !string.IsNullOrEmpty(s))
                          .Select(pos => (name, pos)));

            gen.Sample(pair =>
            {
                var (name, pos) = pair;
                var entry = new EmployeeEntry(name, pos);

                Assert.Equal(name, entry.Name);
                Assert.Equal(pos,  entry.Position);
            });
        }

        // -----------------------------------------------------------------------
        // Tree-walking helper (local copy — avoids cross-class dependency)
        // -----------------------------------------------------------------------

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
    }
}
