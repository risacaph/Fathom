using System.ComponentModel.DataAnnotations;

namespace Fathom.Models.DTOs.KavitaPlus.License;
#nullable enable

public sealed record RenewKavitaPlusLicenseDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The billing cadence to renew on. Only <see cref="KavitaPlusBillingInterval.Month"/> and
    /// <see cref="KavitaPlusBillingInterval.Year"/> are supported.
    /// </summary>
    [Required]
    public KavitaPlusBillingInterval BillingInterval { get; set; }
}
