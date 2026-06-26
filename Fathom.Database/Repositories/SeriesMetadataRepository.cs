using Fathom.API.Repositories;
using Fathom.Models.Entities.Metadata;

namespace Fathom.Database.Repositories;



public class SeriesMetadataRepository(DataContext context) : ISeriesMetadataRepository
{
    public void Update(SeriesMetadata seriesMetadata)
    {
        context.SeriesMetadata.Update(seriesMetadata);
    }
}
