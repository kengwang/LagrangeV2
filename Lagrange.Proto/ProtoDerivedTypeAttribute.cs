namespace Lagrange.Proto;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public class ProtoDerivedTypeAttribute<T> : ProtoDerivedTypeAttribute where T : IEquatable<T>
{
    public ProtoDerivedTypeAttribute(Type derivedType, T typeDiscriminator) : base(derivedType)
    {
        TypeDiscriminator = typeDiscriminator;
    }

    /// <summary>
    /// The type discriminator identifier to be used for the serialization of the subtype.
    /// </summary>
    public T TypeDiscriminator { get; init; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public class ProtoDerivedTypeAttribute : Attribute
{
    public ProtoDerivedTypeAttribute(Type derivedType)
    {
        DerivedType = derivedType;
    }

    /// <summary>
    /// A derived type that should be supported in polymorphic serialization of the declared base type.
    /// </summary>
    public Type DerivedType { get; init; }
}