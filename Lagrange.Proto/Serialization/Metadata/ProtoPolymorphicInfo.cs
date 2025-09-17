namespace Lagrange.Proto.Serialization.Metadata;


public class ProtoPolymorphicInfoBase<T>
{
    public uint PolymorphicIndicateIndex { get; set; } = 0;
    public bool PolymorphicFallbackToBaseType { get; set; } = true;

    public virtual Type? GetTypeFromDiscriminator(object discriminator)
    {
        return null;
    }
    
    public virtual bool SetTypeDiscriminator(object discriminator, Type type)
    {
        return false;
    }
}


public class ProtoPolymorphicObjectInfo<T, TKey> : ProtoPolymorphicInfoBase<T> where TKey : IEquatable<TKey>
{
    public override Type? GetTypeFromDiscriminator(object discriminator)
    {
        return PolymorphicDerivedTypes.GetValueOrDefault((TKey)discriminator);
    }

    public override bool SetTypeDiscriminator(object discriminator, Type type)
    {
        PolymorphicDerivedTypes[(TKey)discriminator] = type;
        return true;
    }

    public Dictionary<TKey, Type> PolymorphicDerivedTypes { get; } = [];
}