namespace Lagrange.Proto;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public class ProtoDerivedTypeAttribute : Attribute
{
    public ProtoDerivedTypeAttribute(Type derivedType)
    {
        DerivedType = derivedType;
    }

    public ProtoDerivedTypeAttribute(Type derivedType, string typeDiscriminator)
    {
        DerivedType = derivedType;
        TypeDiscriminator = typeDiscriminator;
    }
    
    public ProtoDerivedTypeAttribute(Type derivedType, int typeDiscriminator)
    {
        DerivedType = derivedType;
        TypeDiscriminator = typeDiscriminator;
    }

    /// <summary>
    /// A derived type that should be supported in polymorphic serialization of the declared base type.
    /// </summary>
    public Type DerivedType { get; init; }
 
    /// <summary>
    /// The type discriminator identifier to be used for the serialization of the subtype.
    /// </summary>
    public object? TypeDiscriminator { get; init; }
}