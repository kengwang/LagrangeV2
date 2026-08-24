namespace Lagrange.Proto;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class JceMemberAttribute(int tag) : Attribute
{
    public int Tag { get; } = tag;
}
