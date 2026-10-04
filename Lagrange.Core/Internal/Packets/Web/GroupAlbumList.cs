using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Web;

[ProtoPackable]
internal partial class GetAlbumListRequest
{
    [ProtoMember(1)] public int Sequence { get; set; } = 3331;
    [ProtoMember(2)] public byte[] Field2 { get; set; } = [];
    [ProtoMember(3)] public byte[] Field3 { get; set; } = [];
    [ProtoMember(4)] public GetAlbumListRequestData Data { get; set; } = new();
    [ProtoMember(5)] public string TraceId { get; set; } = string.Empty;
    [ProtoMember(10)] public List<AlbumExtMapEntry> ExtMap { get; set; } = [];
}

[ProtoPackable]
internal partial class GetAlbumListRequestData
{
    [ProtoMember(1)] public string GroupId { get; set; } = string.Empty;
    [ProtoMember(2)] public string AttachInfo { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class GetAlbumListResponse
{
    [ProtoMember(1)] public int Sequence { get; set; }
    [ProtoMember(2)] public int Result { get; set; }
    [ProtoMember(3)] public string ErrorText { get; set; } = string.Empty;
    [ProtoMember(4)] public GetAlbumListResponseData? Data { get; set; }
}

[ProtoPackable]
internal partial class GetAlbumListResponseData
{
    [ProtoMember(1)] public List<GroupAlbumInfo> Albums { get; set; } = [];
    [ProtoMember(2)] public string AttachInfo { get; set; } = string.Empty;
    [ProtoMember(3)] public bool HasMore { get; set; }
}

[ProtoPackable]
internal partial class GroupAlbumInfo
{
    [ProtoMember(1)] public string AlbumId { get; set; } = string.Empty;
    [ProtoMember(2)] public string Owner { get; set; } = string.Empty;
    [ProtoMember(3)] public string Name { get; set; } = string.Empty;
    [ProtoMember(4)] public string Description { get; set; } = string.Empty;
    [ProtoMember(5)] public ulong CreateTime { get; set; }
    [ProtoMember(6)] public ulong ModifyTime { get; set; }
    [ProtoMember(7)] public ulong LastUploadTime { get; set; }
    [ProtoMember(8)] public ulong UploadNumber { get; set; }
    [ProtoMember(9)] public AlbumMediaInfo? Cover { get; set; }
}
