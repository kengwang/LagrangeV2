using Lagrange.Proto.Serialization;

namespace Lagrange.Proto.Test;

[TestFixture]
public class ProtoPolymorphismTest
{
    #region Basic Polymorphism

    [Test]
    public void BasicPolymorphism_SerializeAndDeserialize_ReturnsCorrectDerivedType()
    {
        // Arrange
        BaseClass originalA = new DerivedClassA { NameProperty = "TestName" };
        BaseClass originalB = new DerivedClassB { ValueProperty = 114514f };

        byte[] bytesA = ProtoSerializer.Serialize(originalA);
        BaseClass deserializedA = ProtoSerializer.Deserialize<BaseClass>(bytesA);
        
        byte[] bytesB = ProtoSerializer.Serialize(originalB);
        BaseClass deserializedB = ProtoSerializer.Deserialize<BaseClass>(bytesB);

        Assert.Multiple(() =>
        {
            Assert.That(deserializedA, Is.AssignableTo<DerivedClassA>());
            Assert.That(deserializedA, Is.AssignableTo<BaseClass>());
            Assert.That(deserializedA.IdentifierProperty, Is.EqualTo(2));
            Assert.That(((DerivedClassA)deserializedA).NameProperty, Is.EqualTo("TestName"));
            
            Assert.That(deserializedB, Is.AssignableTo<DerivedClassB>());
            Assert.That(deserializedB, Is.AssignableTo<BaseClass>());
            Assert.That(deserializedB.IdentifierProperty, Is.EqualTo(3));
            Assert.That(((DerivedClassB)deserializedB).ValueProperty, Is.EqualTo(114514f));
            
        });
    }

    #endregion
}

#region Test Classes

[ProtoPolymorphic(FieldNumber = 1)]
[ProtoDerivedType<int>(typeof(DerivedClassA), 2)]
[ProtoDerivedType<int>(typeof(DerivedClassB), 3)]
public class BaseClass
{
    public BaseClass() : this(-1) { }

    public BaseClass(int identifier)
    {
        IdentifierProperty = identifier;
    }

    [ProtoMember(1)] public int IdentifierProperty { get; set; }
}

public class DerivedClassA() : BaseClass(2)
{
    [ProtoMember(2)] public string NameProperty { get; set; }
}

public class DerivedClassB() : BaseClass(3)
{
    [ProtoMember(2)] public float ValueProperty { get; set; } = 0f;
}

#endregion