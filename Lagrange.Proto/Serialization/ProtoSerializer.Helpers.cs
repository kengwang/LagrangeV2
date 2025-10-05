using System.Diagnostics;
using System.Reflection;
using Lagrange.Proto.Serialization.Converter;
using Lagrange.Proto.Serialization.Metadata;

namespace Lagrange.Proto.Serialization;

public static partial class ProtoSerializer
{
    internal const string SerializationUnreferencedCodeMessage = "Proto serialization and deserialization might require types that cannot be statically analyzed. Use the SerializePackable<T> that takes a IProtoSerializable<T> to ensure generated code is used, or make sure all of the required types are preserved.";
    internal const string SerializationRequiresDynamicCodeMessage = "Proto serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use Lagrange.Proto source generation for native AOT applications.";

    internal static ProtoObjectConverter<T> GetConverterOf<T>()
    {
        ProtoObjectConverter<T> converter;
        if (ProtoTypeResolver.IsRegistered<T>())
        {
            if (ProtoTypeResolver.GetConverter<T>() as ProtoObjectConverter<T> is not { } c)
            {
                converter = new ProtoObjectConverter<T>(ProtoTypeResolver.CreateObjectInfo<T>());
                ProtoTypeResolver.Register(converter);
            }
            else
            {
                converter = c;
            }
        }
        else
        {
            ProtoTypeResolver.Register(converter = new ProtoObjectConverter<T>());
        }

        return converter;
    }
    
    internal static ProtoPolymorphicDerivedTypeDescriptor<object?> GetObjectInfoReflection(Type polyType)
    {
        var method = typeof(ProtoSerializer).GetMethod(nameof(GetConverterOf),
            BindingFlags.Static | BindingFlags.NonPublic);
        Debug.Assert(method != null);
        var genericMethod = method.MakeGenericMethod(polyType);
        var polyConverter = genericMethod.Invoke(null, null)!;
                
        // get creator and fields, oh my reflection!
        var polyObjectInfo = polyConverter.GetType()
            .GetField("ObjectInfo",BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(polyConverter)!;
        var polyCreator = polyObjectInfo.GetType()
            .GetProperty("ObjectCreator")!.GetValue(polyObjectInfo)!;
        var fieldInfos = (Dictionary<uint, ProtoFieldInfo>)polyObjectInfo.GetType()
            .GetProperty("Fields")!.GetValue(polyObjectInfo)!;
        var polymorphicInfo = (ProtoPolymorphicInfoBase)polyObjectInfo.GetType()
            .GetProperty("PolymorphicInfo")!.GetValue(polyObjectInfo)!;
        var ignoreDefaultFields = polyObjectInfo.GetType()
            .GetProperty("IgnoreDefaultFields")!.GetValue(polyObjectInfo) is true;
        return new ProtoPolymorphicDerivedTypeDescriptor<object?>
        {
            CurrentType = polyType,
            FieldsGetter = () => fieldInfos,
            IgnoreDefaultFieldsGetter = () => ignoreDefaultFields,
            PolymorphicInfoGetter = () => polymorphicInfo,
            ObjectCreator = ObjectCreator
        };
        object? ObjectCreator() => (object?)polyCreator.GetType().GetMethod("Invoke")!.Invoke(polyCreator, null)!;
    }
}