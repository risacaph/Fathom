using Fathom.Models.Entities.Enums;

namespace Fathom.Models.DTOs.KavitaPlus.Scrobble;

public class UpdateScrobbleProviderDto
{
    public required ScrobbleProvider Provider { get; set; }
    public string UserName { get; set; }
    public string AuthenticationToken { get; set; }
    public string RefreshToken { get; set; }
}
