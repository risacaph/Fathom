using Fathom.Common.Extensions;
using Fathom.Models.Entities.Person;

namespace Fathom.Models.Builders;

public class PersonAliasBuilder : IEntityBuilder<PersonAlias>
{
    private readonly PersonAlias _alias;
    public PersonAlias Build() => _alias;

    public PersonAliasBuilder(string name)
    {
        _alias = new PersonAlias()
        {
            Alias = name.Trim(),
            NormalizedAlias = name.ToNormalized(),
        };
    }
}
