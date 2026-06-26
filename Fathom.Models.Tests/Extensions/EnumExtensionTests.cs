using Fathom.Common.Extensions;
using Fathom.Models.Entities.Enums;

namespace Fathom.Models.Tests.Extensions;

public class EnumExtensionTests
{

    [Theory]
    [InlineData("Early Childhood", AgeRating.EarlyChildhood, true)]
    [InlineData("M", AgeRating.Mature, true)]
    [InlineData("ThisIsNotAnAgeRating", default(AgeRating), false)]
    public void TryParse<TEnum>(string? value, TEnum expected, bool success) where TEnum : struct, Enum
    {
        Assert.Equal(EnumExtensions.TryParse(value, out TEnum got), success);
        Assert.Equal(expected, got);
    }

}
