using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Lagrange.Proto.Nodes;

namespace Lagrange.Proto.Serialization.Metadata;

public static partial class ProtoTypeResolver
{
    private static MethodInfo PopulateFieldInfoMethod
    {
        [RequiresUnreferencedCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
        get
        {
            return _populateFieldInfoMethod ?? Initialize();
            static MethodInfo Initialize()
            {
                var value = typeof(ProtoTypeResolver).GetMethod(nameof(PopulateFieldInfo), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException($"Unable to find method {nameof(PopulateFieldInfo)}");
                return Interlocked.CompareExchange(ref _populateFieldInfoMethod, value, null) ?? value;
            }
        }
    }

    private static MethodInfo? _populateFieldInfoMethod;
    
    private static MethodInfo PopulateMapFieldInfoMethod
    {
        [RequiresUnreferencedCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
        get
        {
            return _populateMapFieldInfoMethod ?? Initialize();
            static MethodInfo Initialize()
            {
                var value = typeof(ProtoTypeResolver).GetMethod(nameof(PopulateMapFieldInfo), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException($"Unable to find method {nameof(PopulateFieldInfo)}");
                return Interlocked.CompareExchange(ref _populateMapFieldInfoMethod, value, null) ?? value;
            }
        }
    }
    
    private static MethodInfo? _populateMapFieldInfoMethod;
    
    private static MemberAccessor MemberAccessor
    {
        [RequiresUnreferencedCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
        [RequiresDynamicCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
        get
        {
            return _memberAccessor ?? Initialize();
            static MemberAccessor Initialize()
            {
                MemberAccessor value = RuntimeFeature.IsDynamicCodeSupported ?
                    new ReflectionEmitCachingMemberAccessor() : 
                    new ReflectionMemberAccessor();

                return Interlocked.CompareExchange(ref _memberAccessor, value, null) ?? value;
            }
        }
    }
    
    private static MemberAccessor? _memberAccessor;
    
    [RequiresUnreferencedCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
    [RequiresDynamicCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
    internal static ProtoObjectInfo<T> CreateObjectInfo<T>()
    {
        var objType = typeof(T);
        var ctor = objType.IsValueType ? null : objType.GetConstructor(Type.EmptyTypes);
        bool ignoreDefaultFields = objType.GetCustomAttribute<ProtoPackableAttribute>()?.IgnoreDefaultFields == true;
        var fields = CreateTypeFieldInfo(objType);
        
        return new ProtoObjectInfo<T>
        {
            ObjectCreator = MemberAccessor.CreateParameterlessConstructor<T>(ctor),
            IgnoreDefaultFields = ignoreDefaultFields,
            Fields = fields.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value),
            PolymorphicInfo = PopulatePolymorphicInfo<T>()
        };
    }

    internal static ProtoPolymorphicInfoBase? PopulatePolymorphicInfo<T>()
    {
        var type = typeof(T);
        ProtoPolymorphicInfoBase? objectInfo = null;
        // goto root
        var checkingType = type;
        while (checkingType.BaseType != null)
        {
            // check if base type has polymorphic attribute
            var basePolymorphicAttributes = checkingType.BaseType.GetCustomAttributes(typeof(ProtoDerivedTypeAttribute<>))
                .OfType<ProtoDerivedTypeAttribute>().ToArray();
            if (basePolymorphicAttributes.Length > 0) checkingType = checkingType.BaseType;
            else break;
        }

        if (checkingType == type) checkingType = null;
        
        var polymorphicAttributes = type.GetCustomAttributes(typeof(ProtoDerivedTypeAttribute<>))
            .OfType<ProtoDerivedTypeAttribute>().ToArray();
        if (polymorphicAttributes.Length > 0)
        {
            var polymorphicConfigAttribute = type.GetCustomAttribute<ProtoPolymorphicAttribute>();
            var polymorphicFieldNumber = polymorphicConfigAttribute?.FieldNumber ?? 0;
            var fallbackToBaseType = polymorphicConfigAttribute?.FallbackToBaseType ?? true;
            if (polymorphicFieldNumber == 0) polymorphicFieldNumber = 1; // use first for default

            // get the TKey from first
            var firstAttr = polymorphicAttributes[0];
            var keyType = firstAttr.GetType().GetGenericArguments()[0];
            objectInfo = (ProtoPolymorphicInfoBase?) typeof(ProtoPolymorphicObjectInfo<>).MakeGenericType(keyType)
                    .GetConstructor(Type.EmptyTypes)?.Invoke(null);

            Debug.Assert(objectInfo != null);
            objectInfo.PolymorphicIndicateIndex = polymorphicFieldNumber;
            objectInfo.PolymorphicFallbackToBaseType = fallbackToBaseType;
            
            foreach (var attr in polymorphicAttributes)
            {
                var key = attr.GetType().GetProperty(nameof(ProtoDerivedTypeAttribute<int>.TypeDiscriminator), BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetValue(attr);
                if (key == null) ThrowHelper.ThrowInvalidOperationException_UnknownPolymorphicType(type, attr.DerivedType);
                objectInfo.SetDerivedTypeDescriptor(key, ProtoSerializer.GetObjectInfoReflection(attr.DerivedType));
            }
        }

        if (checkingType is not null)
        {
            objectInfo ??= new ProtoPolymorphicInfoBase()
            {
                PolymorphicIndicateIndex = 0,
                PolymorphicFallbackToBaseType = true
            };
            objectInfo.RootTypeDescriptorGetter = () => ProtoSerializer.GetObjectInfoReflection(checkingType);
        }
        
        return objectInfo;
    }

    internal static Dictionary<uint, ProtoFieldInfo> CreateTypeFieldInfo(Type type)
    {
        var fields = new Dictionary<uint, ProtoFieldInfo>();
        
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (field.IsStatic) continue;
            var fieldInfo = CreateFieldInfo(type, field);
            if (fieldInfo == null) continue;

            uint tag = ((uint)fieldInfo.Field << 3) | (byte)fieldInfo.WireType;
            if (fields.ContainsKey(tag)) ThrowHelper.ThrowInvalidOperationException_DuplicateField(type, fieldInfo.Field);
            fields[tag] = fieldInfo;
        }
        
        foreach (var field in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var fieldInfo = CreateFieldInfo(type, field);
            if (fieldInfo == null) continue;
            
            uint tag = ((uint)fieldInfo.Field << 3) | (byte)fieldInfo.WireType;
            if (fields.ContainsKey(tag)) ThrowHelper.ThrowInvalidOperationException_DuplicateField(type, fieldInfo.Field);
            fields[tag] = fieldInfo;
        }

        return fields;
    }


    [RequiresUnreferencedCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
    [RequiresDynamicCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
    internal static ProtoFieldInfo? CreateFieldInfo(Type declared, MemberInfo member)
    {
        Debug.Assert(member.DeclaringType != null);
        var targetType = member switch
        {
            PropertyInfo p => p.PropertyType,
            FieldInfo f => f.FieldType,
            _ => throw new NotSupportedException($"Unsupported member type: {member.MemberType}")
        };
        
        var method = IsMapType(targetType, out var keyType, out var valueType) 
            ? PopulateMapFieldInfoMethod.MakeGenericMethod(targetType, keyType, valueType) 
            : PopulateFieldInfoMethod.MakeGenericMethod(targetType);
        return (ProtoFieldInfo?)method.Invoke(null, [declared, member]);
    }


    [RequiresUnreferencedCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
    [RequiresDynamicCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
    internal static ProtoFieldInfo<TField>? PopulateFieldInfo<TField>(Type declared, MemberInfo member)
    {
        var type = typeof(TField);
        var attribute = member.GetCustomAttribute<ProtoMemberAttribute>();
        if (attribute == null) return null;
        
        var wireType = DetermineWireType(member.Name, type, attribute.NumberHandling, attribute.NodesWireType);
        var fieldInfo = new ProtoFieldInfo<TField>(attribute.Field, wireType, declared)
        {
            NumberHandling = attribute.NumberHandling
        };
        
        DetermineAccessors(fieldInfo, member, false);

        return fieldInfo;
    }
    
    [RequiresUnreferencedCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
    [RequiresDynamicCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
    internal static ProtoMapFieldInfo<TMap, TKey, TValue>? PopulateMapFieldInfo<TMap, TKey, TValue>(Type declared, MemberInfo member)
        where TMap : IDictionary<TKey, TValue>, new()
        where TKey : notnull
    {
        var attribute = member.GetCustomAttribute<ProtoMemberAttribute>();
        if (attribute == null) return null;
        
        var keyWireType = DetermineWireType(member.Name, typeof(TKey), attribute.NumberHandling, attribute.NodesWireType);
        var valueAttribute = member.GetCustomAttribute<ProtoValueMemberAttribute>();
        var valueWireType = DetermineWireType(member.Name, typeof(TValue), valueAttribute?.NumberHandling ?? ProtoNumberHandling.Default, valueAttribute?.NodesWireType);
        
        var fieldInfo = new ProtoMapFieldInfo<TMap, TKey, TValue>(attribute.Field, keyWireType, valueWireType, declared)
        {
            NumberHandling = attribute.NumberHandling,
            ValueNumberHandling = valueAttribute?.NumberHandling ?? ProtoNumberHandling.Default
        };
        
        DetermineAccessors(fieldInfo, member, false);
        return fieldInfo;
    }
    
    [RequiresUnreferencedCode(ProtoSerializer.SerializationUnreferencedCodeMessage)]
    [RequiresDynamicCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
    private static void DetermineAccessors<T>(ProtoFieldInfo<T> jsonPropertyInfo, MemberInfo memberInfo, bool useNonPublicAccessors)
    {
        Debug.Assert(memberInfo is FieldInfo or PropertyInfo);

        switch (memberInfo)
        {
            case PropertyInfo propertyInfo:
            {
                var getMethod = propertyInfo.GetMethod;
                if (getMethod != null && (getMethod.IsPublic || useNonPublicAccessors)) jsonPropertyInfo.Get = MemberAccessor.CreatePropertyGetter<T>(propertyInfo);
                
                var setMethod = propertyInfo.SetMethod;
                if (setMethod != null && (setMethod.IsPublic || useNonPublicAccessors)) jsonPropertyInfo.Set = MemberAccessor.CreatePropertySetter<T>(propertyInfo);
                break;
            }
            case FieldInfo fieldInfo:
            {
                Debug.Assert(fieldInfo.IsPublic || useNonPublicAccessors);

                jsonPropertyInfo.Get = MemberAccessor.CreateFieldGetter<T>(fieldInfo);
                if (!fieldInfo.IsInitOnly) jsonPropertyInfo.Set = MemberAccessor.CreateFieldSetter<T>(fieldInfo);
                break;
            }
            default:
            {
                Debug.Fail($"Invalid MemberInfo type: {memberInfo.MemberType}");
                break;
            }
        }
    }
    
    [RequiresUnreferencedCode(ProtoSerializer.SerializationUnreferencedCodeMessage)]
    [RequiresDynamicCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
    private static void DetermineAccessors<T, TKey, TValue>(ProtoMapFieldInfo<T, TKey, TValue> jsonPropertyInfo, MemberInfo memberInfo, bool useNonPublicAccessors) 
        where T : IDictionary<TKey, TValue>, new()
        where TKey : notnull
    {
        Debug.Assert(memberInfo is FieldInfo or PropertyInfo);

        switch (memberInfo)
        {
            case PropertyInfo propertyInfo:
            {
                var getMethod = propertyInfo.GetMethod;
                if (getMethod != null && (getMethod.IsPublic || useNonPublicAccessors)) jsonPropertyInfo.Get = MemberAccessor.CreatePropertyGetter<T>(propertyInfo);
                
                var setMethod = propertyInfo.SetMethod;
                if (setMethod != null && (setMethod.IsPublic || useNonPublicAccessors)) jsonPropertyInfo.Set = MemberAccessor.CreatePropertySetter<T>(propertyInfo);
                break;
            }
            case FieldInfo fieldInfo:
            {
                Debug.Assert(fieldInfo.IsPublic || useNonPublicAccessors);

                jsonPropertyInfo.Get = MemberAccessor.CreateFieldGetter<T>(fieldInfo);
                if (!fieldInfo.IsInitOnly) jsonPropertyInfo.Set = MemberAccessor.CreateFieldSetter<T>(fieldInfo);
                break;
            }
            default:
            {
                Debug.Fail($"Invalid MemberInfo type: {memberInfo.MemberType}");
                break;
            }
        }
    }

    private static WireType DetermineWireType(string fieldName, Type fieldType, ProtoNumberHandling numberHandling, WireType? nodesWireType = null)
    {
        if (fieldType.IsEnum) fieldType = Enum.GetUnderlyingType(fieldType);
        
        if (fieldType.IsArray && fieldType.GetElementType() != typeof(byte)) fieldType = fieldType.GetElementType()!;

        if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
        {
            fieldType = fieldType.GetGenericArguments()[0];
        }

        if (fieldType is { IsValueType: true, IsGenericType: true })
        {
            var genericType = fieldType.GetGenericTypeDefinition();
            if (genericType == typeof(Nullable<>))
            {
                fieldType = Nullable.GetUnderlyingType(fieldType)!;
            }
        }
        
        if (fieldType == typeof(ProtoNode) || fieldType == typeof(ProtoRawValue) || fieldType == typeof(ProtoArray))
        {
            if (nodesWireType is null or WireType.Unknown) ThrowHelper.ThrowInvalidOperationException_InvalidNodesWireType(fieldName);
            return nodesWireType.Value;
        }

        var result = fieldType switch
        {
            { IsEnum: true } => WireType.VarInt,
            { IsValueType: true } when fieldType == typeof(bool) => WireType.VarInt,
            { IsValueType: true } when fieldType == typeof(byte) => WireType.VarInt,
            { IsValueType: true } when fieldType == typeof(sbyte) => WireType.VarInt,
            { IsValueType: true } when fieldType == typeof(short) => WireType.VarInt,
            { IsValueType: true } when fieldType == typeof(ushort) => WireType.VarInt,
            { IsValueType: true } when fieldType == typeof(int) => WireType.VarInt,
            { IsValueType: true } when fieldType == typeof(uint) => WireType.VarInt,
            { IsValueType: true } when fieldType == typeof(long) => WireType.VarInt,
            { IsValueType: true } when fieldType == typeof(ulong) => WireType.VarInt,
            { IsValueType: true } when fieldType == typeof(float) => WireType.Fixed32,
            { IsValueType: true } when fieldType == typeof(double) => WireType.Fixed64,
            _ => WireType.LengthDelimited
        };

        if (numberHandling != default)
        {
            if (result != WireType.VarInt) ThrowHelper.ThrowInvalidOperationException_InvalidNumberHandling(fieldType);

            if ((numberHandling & ProtoNumberHandling.Fixed32) != 0) result = WireType.Fixed32;
            else if ((numberHandling & ProtoNumberHandling.Fixed64) != 0) result = WireType.Fixed64;
        }

        return result;
    }

    private static bool IsMapType(
        Type type,
        [NotNullWhen(true)] out Type? keyType, 
        [NotNullWhen(true)] out Type? valueType)
    {
        keyType = null; valueType = null;
        
        if (!type.IsGenericType) return false;
        if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var genericArgs = type.GetGenericArguments();
            if (genericArgs.Length != 2) return false;
            
            keyType = genericArgs[0];
            valueType = genericArgs[1];
            return true;
        }

        return false;
    }
}