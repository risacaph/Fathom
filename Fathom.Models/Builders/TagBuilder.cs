using System.Collections.Generic;
using Fathom.Common.Extensions;
using Fathom.Models.Entities;
using Fathom.Models.Entities.Metadata;

namespace Fathom.Models.Builders;

public class TagBuilder : IEntityBuilder<Tag>
{
    private readonly Tag _tag;
    public Tag Build() => _tag;

    public TagBuilder(string name)
    {
        _tag = new Tag()
        {
            Title = name.Trim(),
            NormalizedTitle = name.ToNormalized(),
            Chapters = [],
            SeriesMetadatas = []
        };
    }

    public TagBuilder WithSeriesMetadata(SeriesMetadata seriesMetadata)
    {
        _tag.SeriesMetadatas ??= new List<SeriesMetadata>();
        _tag.SeriesMetadatas.Add(seriesMetadata);
        return this;
    }
}
