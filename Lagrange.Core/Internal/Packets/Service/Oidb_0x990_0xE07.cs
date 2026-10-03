using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

[ProtoPackable]
internal partial class D990TranslateReq
{
    [ProtoMember(1)] public string SourceLanguage { get; set; } = string.Empty;
    [ProtoMember(2)] public string DestinationLanguage { get; set; } = string.Empty;
    [ProtoMember(3)] public List<string> Words { get; set; } = [];
}
[ProtoPackable]
internal partial class D990ReqBody
{
    [ProtoMember(1)] public D990TranslateReq TranslateReq { get; set; } = new();
    [ProtoMember(10)] public uint Tag10 { get; set; }
    [ProtoMember(12)] public uint Tag12 { get; set; }
}
[ProtoPackable]
internal partial class D990TranslateResp
{
    [ProtoMember(1)] public List<string> DestinationWords { get; set; } = [];
}
[ProtoPackable]
internal partial class D990RespBody
{
    [ProtoMember(2)] public D990TranslateResp? TranslateResp { get; set; }
}

[ProtoPackable]
internal partial class DE07OcrReqBody
{
    [ProtoMember(1)] public string ImageUrl { get; set; } = string.Empty;
    [ProtoMember(2)] public uint LanguageType { get; set; }
    [ProtoMember(3)] public uint Scene { get; set; }
    [ProtoMember(10)] public string OriginMd5 { get; set; } = string.Empty;
    [ProtoMember(11)] public string AfterCompressMd5 { get; set; } = string.Empty;
    [ProtoMember(12)] public string AfterCompressFileSize { get; set; } = string.Empty;
    [ProtoMember(13)] public string AfterCompressWeight { get; set; } = string.Empty;
    [ProtoMember(14)] public string AfterCompressHeight { get; set; } = string.Empty;
    [ProtoMember(15)] public bool IsCut { get; set; }
}
[ProtoPackable]
internal partial class DE07ReqBody
{
    [ProtoMember(1)] public uint Version { get; set; }
    [ProtoMember(2)] public uint Client { get; set; }
    [ProtoMember(3)] public uint Entrance { get; set; }
    [ProtoMember(10)] public DE07OcrReqBody OcrReqBody { get; set; } = new();
}
[ProtoPackable]
internal partial class DE07Coordinate { [ProtoMember(1)] public int X { get; set; } [ProtoMember(2)] public int Y { get; set; } }
[ProtoPackable]
internal partial class DE07Polygon { [ProtoMember(1)] public List<DE07Coordinate> Coordinates { get; set; } = []; }
[ProtoPackable]
internal partial class DE07Detection
{
    [ProtoMember(1)] public string DetectedText { get; set; } = string.Empty;
    [ProtoMember(2)] public uint Confidence { get; set; }
    [ProtoMember(3)] public DE07Polygon? Polygon { get; set; }
}
[ProtoPackable]
internal partial class DE07OcrRespBody { [ProtoMember(1)] public List<DE07Detection> TextDetections { get; set; } = []; [ProtoMember(2)] public string Language { get; set; } = string.Empty; }
[ProtoPackable]
internal partial class DE07RespBody { [ProtoMember(1)] public int RetCode { get; set; } [ProtoMember(2)] public string ErrMsg { get; set; } = string.Empty; [ProtoMember(3)] public string Wording { get; set; } = string.Empty; [ProtoMember(10)] public DE07OcrRespBody? OcrRspBody { get; set; } }
