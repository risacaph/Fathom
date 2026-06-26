using System;

namespace Fathom.Models.DTOs.Statistics;

/// <summary>
/// Reading streak statistics for a user — consecutive days with at least one reading event.
/// </summary>
public class ReadingStreakDto
{
    /// <summary>Consecutive days read up to today (or yesterday). 0 if no recent activity.</summary>
    public int CurrentStreak { get; set; }
    /// <summary>The longest run of consecutive days ever read.</summary>
    public int LongestStreak { get; set; }
    /// <summary>Total distinct days with reading activity.</summary>
    public int TotalDaysRead { get; set; }
    public DateTime? LastReadDateUtc { get; set; }
}
