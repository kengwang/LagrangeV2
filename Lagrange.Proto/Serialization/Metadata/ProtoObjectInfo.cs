using System.Diagnostics;

namespace Lagrange.Proto.Serialization.Metadata;

[DebuggerDisplay("Fields = {Fields.Count}")]
public class ProtoObjectInfo<T>
{
    public Dictionary<uint, ProtoFieldInfo> Fields { get; init; } = new();
    
    public Func<T>? ObjectCreator { get; init; }
    
    public bool IgnoreDefaultFields { get; init; }
    public uint PolymorphicIndicateIndex { get; init; } = 0;
    public Dictionary<object, (Func<T>? objectCreator,Dictionary<uint, ProtoFieldInfo> fields)>? PolymorphicFields { get; init; }
}