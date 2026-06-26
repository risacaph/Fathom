using System.Collections.Generic;
using System.Linq;
using Fathom.Models.DTOs.ReadingLists.CBL.Internal;
using Fathom.Models.Entities.ReadingLists;

namespace Fathom.Services.Extensions;

public static class ReadingListRemapRuleExtensions
{
    public static ReadingListRemapRule? FirstMatchVolumeAndIssueOrDefault(this List<ReadingListRemapRule> rules, ParsedCblItem item)
    {
        return rules.FirstOrDefault(r =>
            !string.IsNullOrEmpty(r.CblVolume) && r.CblVolume == item.Volume &&
            !string.IsNullOrEmpty(r.CblNumber) && r.CblNumber == item.Number);
    }

    public static ReadingListRemapRule? FirstMatchIssueOrDefault(this List<ReadingListRemapRule> rules, ParsedCblItem item)
    {
        return rules.FirstOrDefault(r =>
            !string.IsNullOrEmpty(r.CblNumber) && r.CblNumber == item.Number &&
            string.IsNullOrEmpty(r.CblVolume));
    }

    public static ReadingListRemapRule? FirstMatchVolumeOrDefault(this List<ReadingListRemapRule> rules, ParsedCblItem item)
    {
        return rules.FirstOrDefault(r =>
            !string.IsNullOrEmpty(r.CblVolume) && r.CblVolume == item.Volume &&
            string.IsNullOrEmpty(r.CblNumber));
    }
}
