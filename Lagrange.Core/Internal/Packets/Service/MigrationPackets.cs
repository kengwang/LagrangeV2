// QQ wire definitions for capability extensions; see docs/capability-migration.md.
using Lagrange.Proto;
namespace Lagrange.Core.Internal.Packets.Service.Migration;

[ProtoPackable]
internal partial class Oidb0x112eReq
{
    [ProtoMember(3)] public ulong BotAppid { get; set; }
    [ProtoMember(4)] public ulong MsgSeq { get; set; }
    [ProtoMember(5)] public string ButtonId { get; set; } = string.Empty;
    [ProtoMember(6)] public string CallbackData { get; set; } = string.Empty;
    [ProtoMember(7)] public uint Unknown7 { get; set; }
    [ProtoMember(8)] public ulong GroupId { get; set; }
    [ProtoMember(9)] public uint Unknown9 { get; set; }
}

[ProtoPackable]
internal partial class Oidb0x112eResp
{
    [ProtoMember(3)] public uint Result { get; set; }
    [ProtoMember(4)] public string PromptText { get; set; } = string.Empty;
    [ProtoMember(5)] public string ErrMsg { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class Oidb0xcdeReq
{
    [ProtoMember(2)] public Oidb0xcdeReqBodyInfo? Info { get; set; }
    [ProtoMember(10)] public byte[] SessionData { get; set; } = [];
}

[ProtoPackable]
internal partial class Oidb0xcdeReqBodyInfo
{
    [ProtoMember(1)] public string Db_salt { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class Oidb0xcdeResp
{
    [ProtoMember(2)] public Oidb0xcdeRespBodyInfo? Info { get; set; }
}

[ProtoPackable]
internal partial class Oidb0xcdeRespBodyInfo
{
    [ProtoMember(1)] public string DbKey { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class OidbStrangerStatusReq
{
    [ProtoMember(1)] public uint Uin { get; set; }
    [ProtoMember(3)] public List<OidbStrangerStatusKey> Key { get; set; } = [];
}

[ProtoPackable]
internal partial class OidbStrangerStatusKey
{
    [ProtoMember(1)] public uint Key { get; set; }
}

[ProtoPackable]
internal partial class OidbStrangerStatusResp
{
    [ProtoMember(1)] public OidbStrangerStatusRespData? Data { get; set; }
}

[ProtoPackable]
internal partial class OidbStrangerStatusRespData
{
    [ProtoMember(1)] public uint TargetUin { get; set; }
    [ProtoMember(2)] public OidbStrangerStatusRespProperties? Properties { get; set; }
    [ProtoMember(3)] public uint Uin { get; set; }
}

[ProtoPackable]
internal partial class OidbStrangerStatusRespProperties
{
    [ProtoMember(1)] public List<OidbStrangerStatusRespProperty> Entries { get; set; } = [];
}

[ProtoPackable]
internal partial class OidbStrangerStatusRespProperty
{
    [ProtoMember(1)] public uint Key { get; set; }
    [ProtoMember(2)] public ulong Value { get; set; }
}

[ProtoPackable]
internal partial class MiniAppShareReq
{
    [ProtoMember(2)] public string SdkVersion { get; set; } = string.Empty;
    [ProtoMember(4)] public MiniAppShareReqBody? Body { get; set; }
}

[ProtoPackable]
internal partial class MiniAppShareReqBody
{
    [ProtoMember(2)] public string Appid { get; set; } = string.Empty;
    [ProtoMember(3)] public string Title { get; set; } = string.Empty;
    [ProtoMember(4)] public string Desc { get; set; } = string.Empty;
    [ProtoMember(9)] public string PicUrl { get; set; } = string.Empty;
    [ProtoMember(11)] public string JumpUrl { get; set; } = string.Empty;
    [ProtoMember(12)] public string IconUrl { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class MiniAppShareResp
{
    [ProtoMember(2)] public uint Status { get; set; }
    [ProtoMember(3)] public string Msg { get; set; } = string.Empty;
    [ProtoMember(4)] public MiniAppShareRespBody? Body { get; set; }
}

[ProtoPackable]
internal partial class MiniAppShareRespBody
{
    [ProtoMember(2)] public string JsonStr { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class OidbAiVoiceListReq
{
    [ProtoMember(1)] public uint GroupUin { get; set; }
    [ProtoMember(2)] public uint ChatType { get; set; }
}

[ProtoPackable]
internal partial class OidbAiVoiceListResp
{
    [ProtoMember(1)] public List<OidbAiVoiceListCategory> Content { get; set; } = [];
}

[ProtoPackable]
internal partial class OidbAiVoiceListCategory
{
    [ProtoMember(1)] public string Category { get; set; } = string.Empty;
    [ProtoMember(2)] public List<OidbAiVoiceListEntry> Voices { get; set; } = [];
}

[ProtoPackable]
internal partial class OidbAiVoiceListEntry
{
    [ProtoMember(1)] public string VoiceId { get; set; } = string.Empty;
    [ProtoMember(2)] public string VoiceDisplayName { get; set; } = string.Empty;
    [ProtoMember(3)] public string VoiceExampleUrl { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class OidbAiVoiceReq
{
    [ProtoMember(1)] public uint GroupUin { get; set; }
    [ProtoMember(2)] public string VoiceId { get; set; } = string.Empty;
    [ProtoMember(3)] public string Text { get; set; } = string.Empty;
    [ProtoMember(4)] public uint ChatType { get; set; }
    [ProtoMember(5)] public OidbAiVoiceSession? Session { get; set; }
}

[ProtoPackable]
internal partial class OidbAiVoiceSession
{
    [ProtoMember(1)] public uint SessionId { get; set; }
}

[ProtoPackable]
internal partial class OidbAiVoiceResp
{
    [ProtoMember(1)] public uint StatusCode { get; set; }
    [ProtoMember(2)] public uint Field2 { get; set; }
    [ProtoMember(3)] public uint Field3 { get; set; }
    [ProtoMember(4)] public OidbAiVoiceMsgInfo? MsgInfo { get; set; }
}

[ProtoPackable]
internal partial class OidbAiVoiceMsgInfo
{
    [ProtoMember(1)] public List<OidbAiVoiceMsgInfoBody> MsgInfoBody { get; set; } = [];
}

[ProtoPackable]
internal partial class OidbAiVoiceMsgInfoBody
{
    [ProtoMember(1)] public OidbAiVoiceIndexNode? Index { get; set; }
}

[ProtoPackable]
internal partial class OidbAiVoiceIndexNode
{
    [ProtoMember(1)] public OidbAiVoiceFileInfo? Info { get; set; }
    [ProtoMember(2)] public string FileUuid { get; set; } = string.Empty;
    [ProtoMember(3)] public uint StoreId { get; set; }
    [ProtoMember(4)] public uint UploadTime { get; set; }
    [ProtoMember(5)] public uint Ttl { get; set; }
    [ProtoMember(6)] public uint SubType { get; set; }
}

[ProtoPackable]
internal partial class OidbAiVoiceFileInfo
{
    [ProtoMember(1)] public uint FileSize { get; set; }
    [ProtoMember(2)] public string FileHash { get; set; } = string.Empty;
    [ProtoMember(3)] public string FileSha1 { get; set; } = string.Empty;
    [ProtoMember(4)] public string FileName { get; set; } = string.Empty;
    [ProtoMember(5)] public OidbAiVoiceFileType? Type { get; set; }
    [ProtoMember(6)] public uint Width { get; set; }
    [ProtoMember(7)] public uint Height { get; set; }
    [ProtoMember(8)] public uint Time { get; set; }
    [ProtoMember(9)] public uint Original { get; set; }
}

[ProtoPackable]
internal partial class OidbAiVoiceFileType
{
    [ProtoMember(1)] public uint Type { get; set; }
    [ProtoMember(2)] public uint PicFormat { get; set; }
    [ProtoMember(3)] public uint VideoFormat { get; set; }
    [ProtoMember(4)] public uint VoiceFormat { get; set; }
}

[ProtoPackable]
internal partial class PttTransReq
{
    [ProtoMember(1)] public uint Type { get; set; }
    [ProtoMember(2)] public GroupPttTransItem? GroupItem { get; set; }
    [ProtoMember(3)] public C2CPttTransItem? C2cItem { get; set; }
}

[ProtoPackable]
internal partial class GroupPttTransItem
{
    [ProtoMember(1)] public ulong MsgId { get; set; }
    [ProtoMember(2)] public ulong SenderUin { get; set; }
    [ProtoMember(3)] public ulong GroupUin { get; set; }
    [ProtoMember(4)] public uint FileId { get; set; }
    [ProtoMember(5)] public string Md5 { get; set; } = string.Empty;
    [ProtoMember(6)] public uint Duration { get; set; }
    [ProtoMember(7)] public uint Size { get; set; }
    [ProtoMember(8)] public uint Format { get; set; }
    [ProtoMember(9)] public string Uuid { get; set; } = string.Empty;
    [ProtoMember(10)] public uint EventType { get; set; }
    [ProtoMember(11)] public uint Ext { get; set; }
}

[ProtoPackable]
internal partial class C2CPttTransItem
{
    [ProtoMember(1)] public ulong MsgId { get; set; }
    [ProtoMember(2)] public ulong SenderUin { get; set; }
    [ProtoMember(3)] public ulong ReceiverUin { get; set; }
    [ProtoMember(4)] public string Uuid { get; set; } = string.Empty;
    [ProtoMember(5)] public uint Duration { get; set; }
    [ProtoMember(6)] public uint Size { get; set; }
    [ProtoMember(7)] public uint Format { get; set; }
    [ProtoMember(8)] public uint EventType { get; set; }
    [ProtoMember(9)] public string Md5 { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class PttTransResp
{
    [ProtoMember(1)] public uint Type { get; set; }
    [ProtoMember(2)] public GroupPttTransResult? GroupResult { get; set; }
    [ProtoMember(3)] public C2CPttTransResult? C2cResult { get; set; }
}

[ProtoPackable]
internal partial class GroupPttTransResult
{
    [ProtoMember(2)] public uint ErrCode { get; set; }
    [ProtoMember(9)] public string Text { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class C2CPttTransResult
{
    [ProtoMember(2)] public uint ErrCode { get; set; }
    [ProtoMember(8)] public string Text { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class PttTransPush
{
    [ProtoMember(1)] public uint Field1 { get; set; }
    [ProtoMember(2)] public PttTransPushItem? Item { get; set; }
}

[ProtoPackable]
internal partial class PttTransPushItem
{
    [ProtoMember(1)] public ulong MsgId { get; set; }
    [ProtoMember(8)] public string Text { get; set; } = string.Empty;
    [ProtoMember(9)] public ulong SenderUin { get; set; }
    [ProtoMember(10)] public ulong ReceiverUin { get; set; }
    [ProtoMember(13)] public string Uuid { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class OidbQueryGroupTopBannersReq
{
    [ProtoMember(1)] public ulong GroupId { get; set; }
    [ProtoMember(2)] public uint BannerFlag { get; set; }
}

[ProtoPackable]
internal partial class OidbQueryGroupTopBannersResp
{
    [ProtoMember(1)] public List<OidbOnlineBanner> Banners { get; set; } = [];
    [ProtoMember(3)] public ulong NewSeq { get; set; }
}

[ProtoPackable]
internal partial class OidbOnlineBanner
{
    [ProtoMember(1)] public uint BizType { get; set; }
    [ProtoMember(2)] public uint BannerType { get; set; }
    [ProtoMember(3)] public byte[] MsgId { get; set; } = [];
    [ProtoMember(4)] public bool IsDisappear { get; set; }
    [ProtoMember(8)] public ulong ExpireTime { get; set; }
    [ProtoMember(12)] public OidbGroupTodoLegacyBanner? TodoBanner { get; set; }
    [ProtoMember(20)] public OidbGroupTopBannerCommon? CommonBanner { get; set; }
    [ProtoMember(40)] public OidbGroupTopBannerPriority? BannerPriority { get; set; }
    [ProtoMember(41)] public int BizId { get; set; }
    [ProtoMember(42)] public int Priority { get; set; }
    [ProtoMember(43)] public int LeftShowTimes { get; set; }
    [ProtoMember(44)] public bool SupportMultiBanners { get; set; }
    [ProtoMember(45)] public bool SupportLongPress { get; set; }
    [ProtoMember(46)] public bool NoNeedReportShow { get; set; }
}

[ProtoPackable]
internal partial class OidbGroupTodoLegacyBanner
{
    [ProtoMember(1)] public string RedText { get; set; } = string.Empty;
    [ProtoMember(2)] public string Text { get; set; } = string.Empty;
    [ProtoMember(3)] public string Url { get; set; } = string.Empty;
    [ProtoMember(4)] public bool IsExposure { get; set; }
    [ProtoMember(5)] public bool IsComplete { get; set; }
}

[ProtoPackable]
internal partial class OidbGroupTopBannerCommon
{
    [ProtoMember(1)] public OidbGroupTopBannerUi? Ui { get; set; }
    [ProtoMember(2)] public OidbGroupTopBannerJumpInfo? JumpInfo { get; set; }
    [ProtoMember(3)] public ulong CreateTime { get; set; }
    [ProtoMember(4)] public ulong UpdateTime { get; set; }
}

[ProtoPackable]
internal partial class OidbGroupTopBannerUi
{
    [ProtoMember(1)] public string IconUrl { get; set; } = string.Empty;
    [ProtoMember(2)] public string PreText { get; set; } = string.Empty;
    [ProtoMember(3)] public string Text { get; set; } = string.Empty;
    [ProtoMember(4)] public string HighText { get; set; } = string.Empty;
    [ProtoMember(5)] public uint AccessoryType { get; set; }
    [ProtoMember(6)] public uint IconColor { get; set; }
    [ProtoMember(7)] public bool NeedTranslation { get; set; }
}

[ProtoPackable]
internal partial class OidbGroupTopBannerJumpInfo
{
    [ProtoMember(1)] public uint JumpType { get; set; }
    [ProtoMember(2)] public string JumpUrl { get; set; } = string.Empty;
    [ProtoMember(3)] public byte[] JumpParam { get; set; } = [];
}

[ProtoPackable]
internal partial class OidbGroupTopBannerPriority
{
    [ProtoMember(1)] public uint CategoryType { get; set; }
    [ProtoMember(2)] public int Priority { get; set; }
}
