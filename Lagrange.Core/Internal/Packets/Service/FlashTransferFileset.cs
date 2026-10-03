using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class D93D2Req
{
    [ProtoMember(1)] public uint Field1 { get; set; }
    [ProtoMember(2)] public string Field2 { get; set; } = string.Empty;
    [ProtoMember(3)] public uint Field3 { get; set; }
}

[ProtoPackable]
internal partial class D93D3Req
{
    [ProtoMember(1)] public string FilesetUuid { get; set; } = string.Empty;
    [ProtoMember(2)] public uint Field2 { get; set; }
}

[ProtoPackable]
internal partial class D93DResp
{
    [ProtoMember(1)] public List<D93DEntry>? Entries { get; set; }
}

[ProtoPackable]
internal partial class D93DEntry
{
    [ProtoMember(1)] public string? FilesetUuid { get; set; }
    [ProtoMember(2)] public string? FileName { get; set; }
    [ProtoMember(3)] public string? OriginalName { get; set; }
    [ProtoMember(4)] public uint FileType { get; set; }
    [ProtoMember(5)] public ulong FileSize { get; set; }
    [ProtoMember(8)] public D93DUpload? Upload { get; set; }
    [ProtoMember(9)] public D93DFile? File { get; set; }
}

[ProtoPackable]
internal partial class D93DUpload { [ProtoMember(1)] public string? Url { get; set; } }

[ProtoPackable]
internal partial class D93DFile
{
    [ProtoMember(1)] public string? FileId { get; set; }
    [ProtoMember(2)] public D93DDownload? Download { get; set; }
}

[ProtoPackable]
internal partial class D93DDownload
{
    [ProtoMember(1)] public uint Field1 { get; set; }
    [ProtoMember(2)] public string? Url { get; set; }
}

[ProtoPackable]
internal partial class D9407Req
{
    [ProtoMember(1)] public string FilesetUuid { get; set; } = string.Empty;
    [ProtoMember(2)] public string Field2 { get; set; } = string.Empty;
    [ProtoMember(3)] public uint Field3 { get; set; }
}

[ProtoPackable]
internal partial class D9427Req
{
    [ProtoMember(1)] public string FilesetUuid { get; set; } = string.Empty;
    [ProtoMember(2)] public D9427Name Name { get; set; } = new();
    [ProtoMember(3)] public D9427Flag Flag { get; set; } = new();
}

[ProtoPackable]
internal partial class D9427Name
{
    [ProtoMember(1)] public string NewName { get; set; } = string.Empty;
    [ProtoMember(2)] public string DisplayName { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class D9427Flag { [ProtoMember(1)] public uint Field1 { get; set; } }

[ProtoPackable]
internal partial class DEmptyResp { }

[ProtoPackable]
internal partial class D93D7Req { [ProtoMember(1)] public D93D7Target Target { get; set; } = new(); [ProtoMember(2)] public string FilesetUuid { get; set; } = string.Empty; }
[ProtoPackable]
internal partial class D93D7Target { [ProtoMember(1)] public uint Field1 { get; set; } [ProtoMember(2)] public D93D7Uid? Uid { get; set; } [ProtoMember(3)] public D93D7Group? Group { get; set; } }
[ProtoPackable]
internal partial class D93D7Uid { [ProtoMember(1)] public string Uid { get; set; } = string.Empty; }
[ProtoPackable]
internal partial class D93D7Group { [ProtoMember(1)] public uint GroupId { get; set; } }

[ProtoPackable]
internal partial class D93CFReq { [ProtoMember(1)] public uint Field1 { get; set; } [ProtoMember(2)] public D93CFInfo FileInfo { get; set; } = new(); [ProtoMember(3)] public uint TypeCode { get; set; } [ProtoMember(12)] public uint Field12 { get; set; } }
[ProtoPackable]
internal partial class D93CFInfo { [ProtoMember(2)] public string FileName { get; set; } = string.Empty; [ProtoMember(3)] public string OriginalName { get; set; } = string.Empty; [ProtoMember(4)] public uint FileType { get; set; } [ProtoMember(5)] public ulong FileSize { get; set; } [ProtoMember(10)] public D93CFUploader Uploader { get; set; } = new(); [ProtoMember(16)] public uint Field16 { get; set; } = 1; [ProtoMember(20)] public uint Field20 { get; set; } [ProtoMember(21)] public uint Field21 { get; set; } }
[ProtoPackable]
internal partial class D93CFUploader { [ProtoMember(1)] public string Uin { get; set; } = string.Empty; [ProtoMember(2)] public string Nickname { get; set; } = string.Empty; [ProtoMember(3)] public string Uid { get; set; } = string.Empty; [ProtoMember(4)] public DEmptyResp Field4 { get; set; } = new(); }
[ProtoPackable]
internal partial class D93CFResp { [ProtoMember(1)] public string? FilesetUuid { get; set; } [ProtoMember(2)] public string? UploadKey { get; set; } [ProtoMember(3)] public string? UploadUrl { get; set; } [ProtoMember(4)] public ulong Expire { get; set; } [ProtoMember(5)] public uint Ttl { get; set; } }

[ProtoPackable]
internal partial class D93DBReq { [ProtoMember(1)] public string FilesetUuid { get; set; } = string.Empty; [ProtoMember(2)] public string Field2 { get; set; } = string.Empty; }

[ProtoPackable]
internal partial class D93D0Req { [ProtoMember(1)] public uint Field1 { get; set; } [ProtoMember(2)] public string FilesetUuid { get; set; } = string.Empty; [ProtoMember(3)] public string UploadKey { get; set; } = string.Empty; [ProtoMember(4)] public List<D93D0Info> CommitInfo { get; set; } = []; [ProtoMember(5)] public uint Field5 { get; set; } [ProtoMember(6)] public uint Field6 { get; set; } }
[ProtoPackable]
internal partial class D93D0Info
{
    [ProtoMember(1)] public string FilesetUuid { get; set; } = string.Empty;
    [ProtoMember(2)] public string FileUuid { get; set; } = string.Empty;
    [ProtoMember(3)] public uint? Field3 { get; set; } = 0;
    [ProtoMember(4)] public DEmptyResp Field4 { get; set; } = new();
    [ProtoMember(5)] public uint? Field5 { get; set; } = 1;
    [ProtoMember(6)] public uint? Index { get; set; }
    [ProtoMember(7)] public uint? FormatCode { get; set; }
    [ProtoMember(8)] public string FileName { get; set; } = string.Empty;
    [ProtoMember(9)] public string OriginalName { get; set; } = string.Empty;
    [ProtoMember(10)] public uint? Field10 { get; set; } = 0;
    [ProtoMember(11)] public ulong FileSize { get; set; }
    [ProtoMember(12)] public uint? Field12 { get; set; } = 0;
    [ProtoMember(24)] public DEmptyResp Field24 { get; set; } = new();
}
[ProtoPackable]
internal partial class D93D0Resp { [ProtoMember(1)] public uint Field1 { get; set; } [ProtoMember(2)] public string? FilesetUuid { get; set; } [ProtoMember(3)] public string? UploadKey { get; set; } }

[ProtoPackable]
internal partial class D93D1Req { [ProtoMember(1)] public string FilesetUuid { get; set; } = string.Empty; [ProtoMember(2)] public uint Status { get; set; } }
