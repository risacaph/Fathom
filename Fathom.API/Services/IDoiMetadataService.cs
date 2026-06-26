using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.DTOs.Metadata;

namespace Fathom.API.Services;

/// <summary>
/// Resolves academic metadata from open providers (CrossRef) by DOI. Makes the paid KavitaPlus
/// metadata optional for Research / Regulations libraries.
/// </summary>
public interface IDoiMetadataService
{
    /// <summary>
    /// Resolve academic metadata for a DOI via CrossRef. Returns null if not found or the provider is unreachable.
    /// </summary>
    Task<AcademicMetadataDto?> LookupByDoiAsync(string doi, CancellationToken ct = default);
}
