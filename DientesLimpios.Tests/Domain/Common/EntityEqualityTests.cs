using DientesLimpios.Domain.Common;
using DientesLimpios.Domain.Entities;
using FluentAssertions;

namespace DientesLimpios.Tests.Domain.Common
{
    public class EntityEqualityTests
    {
        private sealed class TestEntity : Entity
        {
            public TestEntity(Guid id) : base(id) { }
            public TestEntity() { }  // simulates EF Core materialisation before the Id is set
        }

        private sealed class OtherTestEntity : Entity
        {
            public OtherTestEntity(Guid id) : base(id) { }
        }

        [Fact]
        public void Equals_TwoInstancesWithSameId_AreEqual()
        {
            // Arrange
            var id = Guid.CreateVersion7();
            var a = new TestEntity(id);
            var b = new TestEntity(id);

            // Act & Assert
            a.Equals(b).Should().BeTrue();
            (a == b).Should().BeTrue();
            (a != b).Should().BeFalse();
            a.GetHashCode().Should().Be(b.GetHashCode());
        }

        [Fact]
        public void Equals_DifferentIds_AreNotEqual()
        {
            // Arrange
            var a = new TestEntity(Guid.CreateVersion7());
            var b = new TestEntity(Guid.CreateVersion7());

            // Act & Assert
            (a == b).Should().BeFalse();
        }

        [Fact]
        public void Equals_SameIdDifferentEntityType_AreNotEqual()
        {
            // Arrange
            var id = Guid.CreateVersion7();
            Entity a = new TestEntity(id);
            Entity b = new OtherTestEntity(id);

            // Act & Assert
            a.Equals(b).Should().BeFalse();
        }

        [Fact]
        public void Equals_TransientEntities_AreEqualOnlyToThemselves()
        {
            // Arrange
            var a = new TestEntity();
            var b = new TestEntity();

            // Act & Assert
            (a == b).Should().BeFalse();
            a.Equals(a).Should().BeTrue();
        }

        [Fact]
        public void EqualityOperator_NullOperands_BehaveLikeReferenceTypes()
        {
            // Arrange
            var a = new TestEntity(Guid.CreateVersion7());

            // Act & Assert
            OperatorEquals(null, null).Should().BeTrue();
            OperatorEquals(a, null).Should().BeFalse();
            OperatorEquals(null, a).Should().BeFalse();
            OperatorNotEquals(a, null).Should().BeTrue();
            EntityEquals(a, null).Should().BeFalse();
        }

        // The operands arrive as parameters so their nullness is unknown at the comparison;
        // written inline, CA1508 sees "null == null" as a constant and flags it as dead code.
        private static bool OperatorEquals(Entity? left, Entity? right) => left == right;
        private static bool OperatorNotEquals(Entity? left, Entity? right) => left != right;
        private static bool EntityEquals(Entity left, Entity? right) => left.Equals(right); 


        [Fact]
        public void HashSet_TwoCopiesOfSameAggregate_HoldOneElement()
        {
            // Arrange
            var office = Office.Create("Main").Value;
            var id = office.Id;
            var copy = new TestEntity(id);   // stand-in for "same row loaded elsewhere"
            var set = new HashSet<Entity> { new TestEntity(id) };

            // Act
            var added = set.Add(copy);

            // Assert
            added.Should().BeFalse();
        }
    }
}
