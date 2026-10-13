using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

[ProtoPackable]
internal partial class RequestEncryptKeyBody
{
    [ProtoMember(1)] public RequestEncryptKeyInfo Info { get; set; } = new();
}

[ProtoPackable]
internal partial class RequestEncryptKeyInfo
{
    [ProtoMember(10)] public uint Type { get; set; }
}

[ProtoPackable]
internal partial class RequestEncryptKeyResponse
{
    [ProtoMember(1)] public DatabaseKeyInfo? Info { get; set; }
}

[ProtoPackable]
internal partial class DatabaseKeyInfo
{
    [ProtoMember(1)] public string Key { get; set; } = string.Empty;
    [ProtoMember(2)] public string KeyMeta { get; set; } = string.Empty;
}
