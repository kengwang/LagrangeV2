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
        BaseClass original = new DerivedClassA { NameProperty = "TestName" };

        byte[] bytes = ProtoSerializer.Serialize(original);
        BaseClass deserialized = ProtoSerializer.Deserialize<BaseClass>(bytes);

        Assert.Multiple(() =>
        {
            Assert.That(deserialized, Is.AssignableTo<DerivedClassA>());
            Assert.That(deserialized, Is.AssignableTo<BaseClass>());
            Assert.That(deserialized.IdentifierProperty, Is.EqualTo(2));
            Assert.That(((DerivedClassA)deserialized).NameProperty, Is.EqualTo("TestName"));
        });
    }

    #endregion
}

#region Test Classes

[ProtoPolymorphic(FieldNumber = 1)]
[ProtoDerivedType(typeof(DerivedClassA), 2)]
[ProtoDerivedType(typeof(DerivedClassB), 3)]
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
    [ProtoMember(2)] public float ValueProperty { get; set; }
}

#endregion