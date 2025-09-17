namespace Lagrange.Proto.Serialization.Metadata;


public class ProtoPolymorphicInfoBase<T>
{
    public uint PolymorphicIndicateIndex { get; set; } = 0;
    public bool PolymorphicFallbackToBaseType { get; set; } = true;

    public virtual ProtoPolymorphicDerivedTypeInfo<T>? GetTypeFromDiscriminator(object discriminator)
    {
        return null;
    }
    
    public virtual bool SetTypeDiscriminator(object discriminator, ProtoPolymorphicDerivedTypeInfo<T> info)
    {
        return false;
    }
}

public class ProtoPolymorphicDerivedTypeInfo<T>
{
    public required Type DerivedType { get; init; }
    public Func<T>? ObjectCreator { get; init; }
    public Dictionary<uint, ProtoFieldInfo> Fields { get; init; } = new();
}

public class ProtoPolymorphicObjectInfo<T, TKey> : ProtoPolymorphicInfoBase<T> where TKey : IEquatable<TKey>
{
    public override ProtoPolymorphicDerivedTypeInfo<T>? GetTypeFromDiscriminator(object discriminator)
    {
        return PolymorphicDerivedTypes.GetValueOrDefault((TKey)discriminator);
    }

    public override bool SetTypeDiscriminator(object discriminator, ProtoPolymorphicDerivedTypeInfo<T> info)
    {
        PolymorphicDerivedTypes[(TKey)discriminator] = info;
        return true;
    }

    public Dictionary<TKey, ProtoPolymorphicDerivedTypeInfo<T>> PolymorphicDerivedTypes { get; } = [];
}