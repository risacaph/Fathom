using System.ComponentModel;

namespace Fathom.Models.Entities.Enums.User;

public enum AuthKeyProvider
{
    /// <summary>
    /// Provided by the User
    /// </summary>
    [Description("User")]
    User = 0,
    /// <summary>
    /// Provided by System
    /// </summary>
    [Description("System")]
    System = 1,
}
