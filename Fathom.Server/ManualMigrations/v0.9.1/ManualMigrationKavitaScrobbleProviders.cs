using System.Linq;
using System.Threading.Tasks;
using Fathom.Database;
using Fathom.Models.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fathom.Server.ManualMigrations.v0._9._1;

/// <summary>
/// Kavita can have some ScrobbleEvents with Kavita as the provider. These should be rewritten to AniList
/// </summary>
public class ManualMigrationKavitaScrobbleProviders : ManualMigration
{
    protected override string MigrationName => nameof(ManualMigrationKavitaScrobbleProviders);
    protected override async Task ExecuteAsync(DataContext context, ILogger<Program> logger)
    {
        await context.ScrobbleEvent.Where(s => s.ScrobbleProvider == ScrobbleProvider.Kavita)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.ScrobbleProvider, ScrobbleProvider.AniList));
    }
}
