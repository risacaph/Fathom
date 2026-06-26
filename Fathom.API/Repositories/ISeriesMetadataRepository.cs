using Fathom.Models.Entities.Metadata;

namespace Fathom.API.Repositories;

public interface ISeriesMetadataRepository
{
    void Update(SeriesMetadata seriesMetadata);
}
