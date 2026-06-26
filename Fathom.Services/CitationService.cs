using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Fathom.API.Database;
using Fathom.API.Services;
using Fathom.Models.Entities.Enums;

namespace Fathom.Services;

/// <summary>
/// Builds citations from a Series' title, writers (authors), release year, publisher and DOI.
/// </summary>
public partial class CitationService(IUnitOfWork unitOfWork) : ICitationService
{
    public async Task<string?> GenerateAsync(int userId, int seriesId, CitationFormat format, CancellationToken ct = default)
    {
        var series = await unitOfWork.SeriesRepository.GetSeriesDtoByIdAsync(seriesId, userId, ct);
        if (series == null) return null;

        var meta = await unitOfWork.SeriesRepository.GetSeriesMetadataAsync(seriesId, ct);

        var title = series.Name ?? string.Empty;
        var authors = meta?.Writers.Select(w => w.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList() ?? [];
        var year = meta?.ReleaseYear ?? 0;
        var publisher = meta?.Publishers.FirstOrDefault()?.Name;
        var doi = string.IsNullOrWhiteSpace(meta?.Doi) ? null : meta!.Doi.Trim();

        return format switch
        {
            CitationFormat.BibTeX => BuildBibTeX(seriesId, title, authors, year, publisher, doi),
            CitationFormat.Ris => BuildRis(title, authors, year, publisher, doi),
            CitationFormat.Apa => BuildApa(title, authors, year, publisher, doi),
            CitationFormat.Mla => BuildMla(title, authors, year, publisher, doi),
            _ => null
        };
    }

    private static string BuildBibTeX(int seriesId, string title, IList<string> authors, int year, string? publisher, string? doi)
    {
        var surname = authors.FirstOrDefault()?.Split(' ').LastOrDefault();
        var key = NonAlphaNum().Replace((surname ?? "fathom") + (year > 0 ? year.ToString() : seriesId.ToString()), string.Empty);

        var sb = new StringBuilder();
        sb.Append("@misc{").Append(key).Append(",\n");
        sb.Append("  title = {").Append(title).Append("},\n");
        if (authors.Count > 0) sb.Append("  author = {").Append(string.Join(" and ", authors)).Append("},\n");
        if (year > 0) sb.Append("  year = {").Append(year).Append("},\n");
        if (!string.IsNullOrWhiteSpace(publisher)) sb.Append("  publisher = {").Append(publisher).Append("},\n");
        if (!string.IsNullOrWhiteSpace(doi))
        {
            sb.Append("  doi = {").Append(doi).Append("},\n");
            sb.Append("  url = {https://doi.org/").Append(doi).Append("},\n");
        }
        sb.Append("}\n");
        return sb.ToString();
    }

    private static string BuildRis(string title, IList<string> authors, int year, string? publisher, string? doi)
    {
        var sb = new StringBuilder();
        sb.Append("TY  - GEN\n");
        sb.Append("TI  - ").Append(title).Append('\n');
        foreach (var a in authors) sb.Append("AU  - ").Append(a).Append('\n');
        if (year > 0) sb.Append("PY  - ").Append(year).Append('\n');
        if (!string.IsNullOrWhiteSpace(publisher)) sb.Append("PB  - ").Append(publisher).Append('\n');
        if (!string.IsNullOrWhiteSpace(doi))
        {
            sb.Append("DO  - ").Append(doi).Append('\n');
            sb.Append("UR  - https://doi.org/").Append(doi).Append('\n');
        }
        sb.Append("ER  - \n");
        return sb.ToString();
    }

    private static string BuildApa(string title, IList<string> authors, int year, string? publisher, string? doi)
    {
        var sb = new StringBuilder();
        if (authors.Count > 0) sb.Append(string.Join(", ", authors)).Append(' ');
        sb.Append('(').Append(year > 0 ? year.ToString() : "n.d.").Append("). ");
        sb.Append(title).Append(". ");
        if (!string.IsNullOrWhiteSpace(publisher)) sb.Append(publisher).Append(". ");
        if (!string.IsNullOrWhiteSpace(doi)) sb.Append("https://doi.org/").Append(doi);
        return sb.ToString().Trim();
    }

    private static string BuildMla(string title, IList<string> authors, int year, string? publisher, string? doi)
    {
        var sb = new StringBuilder();
        if (authors.Count > 0) sb.Append(string.Join(", ", authors)).Append(". ");
        sb.Append('"').Append(title).Append(".\" ");
        if (!string.IsNullOrWhiteSpace(publisher)) sb.Append(publisher).Append(", ");
        if (year > 0) sb.Append(year).Append(". ");
        if (!string.IsNullOrWhiteSpace(doi)) sb.Append("https://doi.org/").Append(doi).Append('.');
        return sb.ToString().Trim();
    }

    [GeneratedRegex("[^A-Za-z0-9]")]
    private static partial Regex NonAlphaNum();
}
