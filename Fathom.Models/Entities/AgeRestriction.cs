using Fathom.Models.Entities.Enums;

namespace Fathom.Models.Entities;

public class AgeRestriction
{
    public AgeRating AgeRating { get; set; }
    public bool IncludeUnknowns { get; set; }
}
