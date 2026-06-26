using System.Collections.Generic;
using Fathom.Models.Entities.MetadataMatching;

namespace Fathom.Models.Entities.Interfaces;

public interface IHasKPlusMetadata
{
    /// <summary>
    /// Tracks which metadata has been set by K+
    /// </summary>
    public IList<MetadataSettingField> KPlusOverrides { get; set; }
}
