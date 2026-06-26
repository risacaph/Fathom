using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.DTOs.ReadingLists.CBL.RemapRules;
using Fathom.Models.Entities;
using Fathom.Models.Entities.ReadingLists;

namespace Fathom.API.Repositories;

public interface IReadingListRemapRuleRepository
{
    /// <summary>
    /// Returns all remap rules matching the given normalized CBL series names,
    /// ordered with user-specific rules before global rules.
    /// </summary>
    Task<IList<ReadingListRemapRule>> GetRulesForNamesAsync(IList<string> normalizedNames, int userId, CancellationToken ct = default);
    Task<IList<RemapRuleDto>> GetRuleDtosForUserAsync(int userId, CancellationToken ct = default);
    Task<ReadingListRemapRule?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<RemapRuleDto?> GetDtoByIdAsync(int id, CancellationToken ct = default);
    /// <summary>
    /// Admin-only: returns all rules across all users, with usernames.
    /// </summary>
    Task<IList<RemapRuleDto>> GetAllRuleDtosAsync(CancellationToken ct = default);
    /// <summary>
    /// Finds an existing rule for the same user with the same CBL matching key (normalized name + volume + number).
    /// </summary>
    Task<ReadingListRemapRule?> GetExactRuleAsync(string normalizedCblSeriesName, string? cblVolume, string? cblNumber, int userId, CancellationToken ct = default);
    void Add(ReadingListRemapRule rule);
    void Remove(ReadingListRemapRule rule);
}
