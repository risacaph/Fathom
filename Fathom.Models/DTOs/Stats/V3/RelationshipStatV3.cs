using Fathom.Models.Entities.Enums;

namespace Fathom.Models.DTOs.Stats.V3;

/// <summary>
/// KavitaStats - Information about Series Relationships
/// </summary>
public sealed record RelationshipStatV3
{
    public int Count { get; set; }
    public RelationKind Relationship { get; set; }
}
