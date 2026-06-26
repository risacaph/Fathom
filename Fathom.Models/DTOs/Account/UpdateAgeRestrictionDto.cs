using System.ComponentModel.DataAnnotations;
using Fathom.Models.Entities.Enums;

namespace Fathom.Models.DTOs.Account;

public sealed record UpdateAgeRestrictionDto
{
    [Required]
    public AgeRating AgeRating { get; set; }
    [Required]
    public bool IncludeUnknowns { get; set; }
}
