using Fathom.Models.Entities.Enums;

namespace Fathom.Models.DTOs.Metadata;

public sealed record PublicationStatusDto
{
    public PublicationStatus Value { get; set; }
    public required string Title { get; set; }
}
