using Fathom.Models.Entities.Interfaces;
using Fathom.Models.Entities.MetadataMatching;

namespace Fathom.Services.Extensions;

public static class IHasKPlusMetadataExtensions
{

    public static bool HasSetKPlusMetadata(this IHasKPlusMetadata hasKPlusMetadata, MetadataSettingField field)
    {
        return hasKPlusMetadata.KPlusOverrides.Contains(field);
    }

    public static void AddKPlusOverride(this IHasKPlusMetadata hasKPlusMetadata, MetadataSettingField field)
    {
        if (hasKPlusMetadata.KPlusOverrides.Contains(field)) return;

        hasKPlusMetadata.KPlusOverrides.Add(field);
    }

}
