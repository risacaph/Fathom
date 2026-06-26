using System;
using Fathom.Models.Entities.Interfaces;
using Fathom.Models.Entities.User;

namespace Fathom.Models.Entities.Scrobble;

public class ScrobbleHold : IEntityDate
{
    public int Id { get; set; }
    public int SeriesId { get; set; }
    public Series Series { get; set; }
    public int AppUserId { get; set; }
    public AppUser AppUser { get; set; }
    public DateTime Created { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime LastModified { get; set; }
    public DateTime LastModifiedUtc { get; set; }
}
