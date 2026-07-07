using STLMS.Domain.Entities;
using Xunit;

namespace STLMS.Domain.Tests;

public class BaseEntityTests
{
    [Fact]
    public void NewEntity_GetsARandomNonEmptyId()
    {
        var a = new Role();
        var b = new Role();

        Assert.NotEqual(Guid.Empty, a.Id);
        Assert.NotEqual(a.Id, b.Id);
    }

    [Fact]
    public void NewAuditableEntity_DefaultsToNotDeleted()
    {
        var role = new Role();

        Assert.False(role.IsDeleted);
        Assert.Null(role.DeletedAt);
    }
}
