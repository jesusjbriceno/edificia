using Edificia.Domain.Primitives;
using FluentAssertions;

namespace Edificia.Domain.Tests.Primitives;

public class EntityTests
{
    private class TestEntity : Entity
    {
        public TestEntity(Guid id) : base(id) { }
    }

    [Fact]
    public void Entities_WithSameId_ShouldBeEqual()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        entity1.Should().Be(entity2);
        (entity1 == entity2).Should().BeTrue();
    }

    [Fact]
    public void Entities_WithDifferentIds_ShouldNotBeEqual()
    {
        var entity1 = new TestEntity(Guid.NewGuid());
        var entity2 = new TestEntity(Guid.NewGuid());

        entity1.Should().NotBe(entity2);
        (entity1 != entity2).Should().BeTrue();
    }

    [Fact]
    public void Entity_GetHashCode_ShouldBeBasedOnId()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);

        entity.GetHashCode().Should().Be(id.GetHashCode());
    }
}
