using Lagrange.Proto;
namespace Lagrange.Core.Internal.Packets.Service;

[ProtoPackable]
internal partial class MigrationFlashGetDownloadUrlReqInner
{
    [ProtoMember(1)] public string Field1 { get; set; } = string.Empty;
    [ProtoMember(2)] public uint Field2 { get; set; }
    [ProtoMember(3)] public uint Field3 { get; set; }
    [ProtoMember(4)] public string Field4 { get; set; } = string.Empty;
    [ProtoMember(5)] public MigrationFlashGetDownloadUrlReqInner5? Field5 { get; set; }
    [ProtoMember(6)] public MigrationFlashGetDownloadUrlReqInner6? Field6 { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashGetDownloadUrlReqInner5
{
    [ProtoMember(1)] public uint Field1 { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashGetDownloadUrlReqInner6
{
    [ProtoMember(1)] public uint Field1 { get; set; }
    [ProtoMember(2)] public uint Field2 { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashGetDownloadUrlReq
{
    [ProtoMember(1)] public string FilesetUuid { get; set; } = string.Empty;
    [ProtoMember(2)] public MigrationFlashGetDownloadUrlReqInner? Inner { get; set; }
    [ProtoMember(3)] public uint Field3 { get; set; }
    [ProtoMember(4)] public uint Field4 { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashDownloadUrlWrap
{
    [ProtoMember(1)] public uint Field1 { get; set; }
    [ProtoMember(2)] public string DownloadUrl { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class MigrationFlashDownloadUrlInfo
{
    [ProtoMember(1)] public string FileId { get; set; } = string.Empty;
    [ProtoMember(2)] public MigrationFlashDownloadUrlWrap? Download { get; set; }
    [ProtoMember(3)] public string Sha1 { get; set; } = string.Empty;
    [ProtoMember(4)] public uint Field4 { get; set; }
    [ProtoMember(5)] public string Md5 { get; set; } = string.Empty;
    [ProtoMember(6)] public uint Width { get; set; }
    [ProtoMember(7)] public uint Height { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashDownloadMainFile
{
    [ProtoMember(1)] public string FileId { get; set; } = string.Empty;
    [ProtoMember(3)] public uint Field3 { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashDownloadFileInfo
{
    [ProtoMember(1)] public string FilesetUuid { get; set; } = string.Empty;
    [ProtoMember(2)] public string FileUuid { get; set; } = string.Empty;
    [ProtoMember(5)] public uint Field5 { get; set; }
    [ProtoMember(6)] public uint Field6 { get; set; }
    [ProtoMember(7)] public uint Field7 { get; set; }
    [ProtoMember(8)] public string FileName { get; set; } = string.Empty;
    [ProtoMember(9)] public string OrigName { get; set; } = string.Empty;
    [ProtoMember(11)] public ulong FileSize { get; set; }
    [ProtoMember(13)] public MigrationFlashDownloadUrlInfo? DownloadInfo { get; set; }
    [ProtoMember(14)] public MigrationFlashDownloadMainFile? MainFile { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashDownloadEntry
{
    [ProtoMember(2)] public uint Field2 { get; set; }
    [ProtoMember(3)] public List<MigrationFlashDownloadFileInfo> FileInfo { get; set; } = [];
    [ProtoMember(4)] public string Field4 { get; set; } = string.Empty;
    [ProtoMember(5)] public uint Field5 { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashGetDownloadUrlResp
{
    [ProtoMember(1)] public MigrationFlashDownloadEntry? Entry { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashEmpty
{
}

[ProtoPackable]
internal partial class MigrationFlashFileId
{
    [ProtoMember(2)] public byte[] Sha1 { get; set; } = [];
    [ProtoMember(3)] public uint FileSize { get; set; }
    [ProtoMember(4)] public uint Appid { get; set; }
    [ProtoMember(5)] public ulong Timestamp { get; set; }
    [ProtoMember(6)] public string Env { get; set; } = string.Empty;
    [ProtoMember(10)] public uint Ttl { get; set; }
    [ProtoMember(11)] public byte[] SessionId { get; set; } = [];
    [ProtoMember(15)] public byte[] Field15 { get; set; } = [];
    [ProtoMember(16)] public string Region { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class MigrationFlashApplyFileInfo5
{
    [ProtoMember(1)] public uint? Field1 { get; set; } = 0;
    [ProtoMember(2)] public uint? Field2 { get; set; } = 0;
    [ProtoMember(3)] public uint? Field3 { get; set; } = 0;
    [ProtoMember(4)] public uint? Field4 { get; set; } = 0;
}

[ProtoPackable]
internal partial class MigrationFlashApplyPayloadField3
{
    [ProtoMember(1)] public uint? Field1 { get; set; } = 0;
    [ProtoMember(2)] public uint? Field2 { get; set; } = 0;
    [ProtoMember(3)] public uint? Field3 { get; set; } = 0;
    [ProtoMember(4)] public MigrationFlashEmpty? Field4 { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashApplyFilesetWrap
{
    [ProtoMember(1)] public string FilesetUuid { get; set; } = string.Empty;
    [ProtoMember(2)] public string UploadKey { get; set; } = string.Empty;
    [ProtoMember(3)] public string FileUuid { get; set; } = string.Empty;
    [ProtoMember(4)] public uint? Field4 { get; set; } = 0;
    [ProtoMember(5)] public uint? Field5 { get; set; } = 0;
    [ProtoMember(6)] public uint? Field6 { get; set; } = 0;
    [ProtoMember(7)] public uint? Field7 { get; set; } = 0;
    [ProtoMember(8)] public MigrationFlashEmpty? Field8 { get; set; }
    [ProtoMember(9)] public uint? Field9 { get; set; } = 0;
    [ProtoMember(10)] public uint? Field10 { get; set; } = 0;
    [ProtoMember(11)] public uint? Field11 { get; set; } = 0;
    [ProtoMember(12)] public uint? Field12 { get; set; } = 0;
    [ProtoMember(13)] public uint? Field13 { get; set; } = 0;
    [ProtoMember(14)] public uint? Field14 { get; set; } = 0;
}

[ProtoPackable]
internal partial class MigrationFlashApplyFileInfo
{
    [ProtoMember(1)] public uint? FileSize { get; set; } = 0;
    [ProtoMember(2)] public string Md5 { get; set; } = string.Empty;
    [ProtoMember(3)] public string Sha1 { get; set; } = string.Empty;
    [ProtoMember(4)] public string FileName { get; set; } = string.Empty;
    [ProtoMember(5)] public MigrationFlashApplyFileInfo5? Field5 { get; set; }
    [ProtoMember(6)] public uint? Field6 { get; set; } = 0;
    [ProtoMember(7)] public uint? Field7 { get; set; } = 0;
    [ProtoMember(8)] public uint? Field8 { get; set; } = 0;
    [ProtoMember(9)] public uint? Field9 { get; set; } = 0;
}

[ProtoPackable]
internal partial class MigrationFlashApplyUploadWrapper
{
    [ProtoMember(1)] public MigrationFlashApplyFileInfo? FileInfo { get; set; }
    [ProtoMember(2)] public string FileId { get; set; } = string.Empty;
    [ProtoMember(3)] public uint? Field3 { get; set; } = 0;
    [ProtoMember(4)] public uint? Field4 { get; set; } = 0;
    [ProtoMember(5)] public uint? Field5 { get; set; } = 0;
    [ProtoMember(6)] public uint? Field6 { get; set; } = 0;
}

[ProtoPackable]
internal partial class MigrationFlashApplyUploadPayload
{
    [ProtoMember(1)] public MigrationFlashApplyUploadWrapper? Wrapper { get; set; }
    [ProtoMember(2)] public MigrationFlashApplyFlag2? Flag2 { get; set; }
    [ProtoMember(3)] public MigrationFlashApplyPayloadField3? Field3 { get; set; }
    [ProtoMember(10)] public MigrationFlashApplyFilesetWrap? FilesetWrap { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashApplyFlag2
{
    [ProtoMember(1)] public uint Field1 { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashApplyHeadSub
{
    [ProtoMember(1)] public uint Seq { get; set; }
    [ProtoMember(2)] public uint Sub { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashApplyHeadConfig
{
    [ProtoMember(101)] public uint Field101 { get; set; }
    [ProtoMember(102)] public uint Field102 { get; set; }
    [ProtoMember(103)] public uint Field103 { get; set; }
    [ProtoMember(200)] public uint Field200 { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashApplyHead
{
    [ProtoMember(1)] public MigrationFlashApplyHeadSub? Sub { get; set; }
    [ProtoMember(2)] public MigrationFlashApplyHeadConfig? Config { get; set; }
    [ProtoMember(3)] public MigrationFlashApplyFlag2? Field3 { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashApplyUploadReq
{
    [ProtoMember(1)] public MigrationFlashApplyHead? Head { get; set; }
    [ProtoMember(12)] public MigrationFlashApplyUploadPayload? Payload { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashApplyHeadResp
{
    [ProtoMember(1)] public MigrationFlashApplyHeadSub? Sub { get; set; }
    [ProtoMember(3)] public string Msg { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class MigrationFlashRkeyWrap
{
    [ProtoMember(1)] public string Rkey { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class MigrationFlashPreparePayloadF6F1
{
    [ProtoMember(1)] public uint? Field1 { get; set; } = 0;
    [ProtoMember(2)] public MigrationFlashEmpty? Field2 { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashPreparePayloadF6F2
{
    [ProtoMember(3)] public MigrationFlashEmpty? Field3 { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashPreparePayloadF6F3
{
    [ProtoMember(11)] public MigrationFlashEmpty? Field11 { get; set; }
    [ProtoMember(12)] public MigrationFlashEmpty? Field12 { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashPreparePayloadF6
{
    [ProtoMember(1)] public MigrationFlashPreparePayloadF6F1? Field1 { get; set; }
    [ProtoMember(2)] public MigrationFlashPreparePayloadF6F2? Field2 { get; set; }
    [ProtoMember(3)] public MigrationFlashPreparePayloadF6F3? Field3 { get; set; }
    [ProtoMember(10)] public uint? Field10 { get; set; } = 0;
}

[ProtoPackable]
internal partial class MigrationFlashPrepareWrapper
{
    [ProtoMember(1)] public MigrationFlashApplyFileInfo? FileInfo { get; set; }
    [ProtoMember(2)] public uint? Field2 { get; set; } = 0;
}

[ProtoPackable]
internal partial class MigrationFlashPrepareUploadPayload
{
    [ProtoMember(1)] public MigrationFlashPrepareWrapper? Wrapper { get; set; }
    [ProtoMember(2)] public uint? Field2 { get; set; } = 0;
    [ProtoMember(3)] public uint? Field3 { get; set; } = 0;
    [ProtoMember(4)] public uint? Field4 { get; set; } = 0;
    [ProtoMember(5)] public uint? Field5 { get; set; } = 0;
    [ProtoMember(6)] public MigrationFlashPreparePayloadF6? Field6 { get; set; }
    [ProtoMember(7)] public uint? Field7 { get; set; } = 0;
    [ProtoMember(8)] public uint? Field8 { get; set; } = 0;
    [ProtoMember(9)] public MigrationFlashApplyFilesetWrap? FilesetWrap { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashPrepareUploadReq
{
    [ProtoMember(1)] public MigrationFlashApplyHead? Head { get; set; }
    [ProtoMember(2)] public MigrationFlashPrepareUploadPayload? Payload { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashPrepareUploadResp
{
    [ProtoMember(1)] public MigrationFlashApplyHeadResp? Head { get; set; }
    [ProtoMember(2)] public MigrationFlashRkeyWrap? RkeyWrap { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashGetDownloadFileInfo
{
    [ProtoMember(1)] public uint Field1 { get; set; }
    [ProtoMember(4)] public string Field4 { get; set; } = string.Empty;
    [ProtoMember(5)] public MigrationFlashApplyFileInfo5? Field5 { get; set; }
    [ProtoMember(6)] public uint Field6 { get; set; }
    [ProtoMember(7)] public uint Field7 { get; set; }
    [ProtoMember(8)] public uint Field8 { get; set; }
    [ProtoMember(9)] public uint Field9 { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashGetDownloadWrapper
{
    [ProtoMember(1)] public MigrationFlashGetDownloadFileInfo? FileInfo { get; set; }
    [ProtoMember(2)] public string FileId { get; set; } = string.Empty;
    [ProtoMember(3)] public uint Field3 { get; set; }
    [ProtoMember(4)] public uint Field4 { get; set; }
    [ProtoMember(5)] public uint Field5 { get; set; }
    [ProtoMember(6)] public uint Field6 { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashGetDownloadParam
{
    [ProtoMember(1)] public uint Field1 { get; set; }
    [ProtoMember(3)] public uint Field3 { get; set; }
    [ProtoMember(5)] public uint Field5 { get; set; }
    [ProtoMember(6)] public MigrationFlashGetDownloadParam6? Field6 { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashGetDownloadParam6
{
    [ProtoMember(1)] public uint Field1 { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashGetDownloadFlag4
{
    [ProtoMember(1)] public uint Field1 { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashGetDownloadFilesetWrap
{
    [ProtoMember(1)] public string FilesetUuid { get; set; } = string.Empty;
    [ProtoMember(2)] public string FileUuid { get; set; } = string.Empty;
    [ProtoMember(3)] public uint Field3 { get; set; }
    [ProtoMember(4)] public string Field4 { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class MigrationFlashGetDownloadPayload2
{
    [ProtoMember(2)] public MigrationFlashGetDownloadParam? Field2 { get; set; }
    [ProtoMember(4)] public MigrationFlashGetDownloadFlag4? Field4 { get; set; }
    [ProtoMember(10)] public MigrationFlashGetDownloadFilesetWrap? FilesetWrap { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashGetDownloadPayload
{
    [ProtoMember(1)] public MigrationFlashGetDownloadWrapper? Wrapper { get; set; }
    [ProtoMember(2)] public MigrationFlashGetDownloadPayload2? Field2 { get; set; }
    [ProtoMember(3)] public uint Field3 { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashGetDownloadReq
{
    [ProtoMember(1)] public MigrationFlashApplyHead? Head { get; set; }
    [ProtoMember(3)] public MigrationFlashGetDownloadPayload? Payload { get; set; }
}

[ProtoPackable]
internal partial class MigrationFlashGetDownloadResp
{
    [ProtoMember(1)] public MigrationFlashApplyHeadResp? Head { get; set; }
    [ProtoMember(3)] public byte[] Body { get; set; } = [];
}
