namespace Lagrange.Proto;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public class ProtoPolymorphicAttribute : Attribute
{
    public uint FieldNumber { get; init; }
    public bool FallbackToBaseType { get; init; } = true;
}