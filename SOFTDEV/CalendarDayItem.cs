namespace SOFTDEV;

/// <summary>
/// Represents a single cell in the calendar grid.
/// <para>
/// <c>Day == 0</c> indicates a leading padding cell outside the current month.
/// </para>
/// </summary>
public record CalendarDayItem(int Day, bool IsCurrentMonth, bool IsHighlighted);
