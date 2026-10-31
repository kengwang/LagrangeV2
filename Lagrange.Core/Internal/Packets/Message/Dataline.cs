using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Message;

[ProtoPackable] internal partial class DatalineBody
{
    [ProtoMember(1)] public uint SubCommand { get; set; }
    [ProtoMember(2)] public DatalineHeader? Header { get; set; }
    [ProtoMember(3)] public List<DatalineFile> Files { get; set; } = [];
    [ProtoMember(6)] public DatalineGeneric? Generic { get; set; }
}
[ProtoPackable] internal partial class DatalineHeader
{
    [ProtoMember(1)] public uint SourceAppId { get; set; }
    [ProtoMember(2)] public uint SourceInstanceId { get; set; } = 1;
    [ProtoMember(3)] public uint DestinationAppId { get; set; }
    [ProtoMember(4)] public uint DestinationInstanceId { get; set; } = 1;
    [ProtoMember(5)] public long DestinationUin { get; set; }
    [ProtoMember(6)] public long SourceUin { get; set; }
    [ProtoMember(9)] public uint SourceTerminal { get; set; }
    [ProtoMember(10)] public uint DestinationTerminal { get; set; }
}
[ProtoPackable] internal partial class DatalineFile
{
    [ProtoMember(1)] public ulong SessionId { get; set; }
    [ProtoMember(2)] public string FileName { get; set; } = string.Empty;
    [ProtoMember(3)] public string FileIndex { get; set; } = string.Empty;
    [ProtoMember(4)] public byte[] FileMd5 { get; set; } = [];
    [ProtoMember(5)] public string FileKey { get; set; } = string.Empty;
    [ProtoMember(6)] public ulong FileLength { get; set; }
}
[ProtoPackable] internal partial class DatalineGeneric
{
    [ProtoMember(1)] public ulong SessionId { get; set; }
    [ProtoMember(2)] public uint Size { get; set; } = 1;
    [ProtoMember(3)] public uint Index { get; set; }
    [ProtoMember(4)] public uint Type { get; set; } = 1;
    [ProtoMember(5)] public byte[] Buffer { get; set; } = [];
}
[ProtoPackable] internal partial class DatalineText
{
    [ProtoMember(1)] public List<DatalineTextItem> Items { get; set; } = [];
}
[ProtoPackable] internal partial class DatalineTextItem
{
    [ProtoMember(1)] public uint Type { get; set; } = 1;
    [ProtoMember(2)] public string Text { get; set; } = string.Empty;
}
