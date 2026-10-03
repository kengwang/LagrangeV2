using Lagrange.Proto;

#pragma warning disable CS8618

namespace Lagrange.Core.Internal.Packets.Login;

#region Enum

internal enum NTLoginPlatform {
    PLATFORM_UNKNOWN = 0,
    PLATFORM_IOS = 1,
    PLATFORM_ANDROID = 2,
    PLATFROM_SYMBIAN = 3,
    PLATFORM_WINDOWS = 4,
    PLATFORM_MAC = 5,
    PLATFORM_IPAD = 6,
    PLATFORM_LINUX = 7,
    PLATFORM_HARMONY = 8
}

internal enum NTLoginRetCode
{
    LOGIN_SUCCESS = 0,
    LOGIN_ERROR_DEFAULT = 140022000,
    LOGIN_ERROR_INVALID_PARAMETER = 140022001,
    LOGIN_ERROR_SYSTEM_FAILED = 140022002,
    LOGIN_ERROR_TIMEOUT_RETRY = 140022003,
    LOGIN_ERROR_NEED_UPDATE = 140022004,
    LOGIN_ERROR_FROZEN = 140022005,
    LOGIN_ERROR_PROTECT = 140022006,
    LOGIN_ERROR_STRICT = 140022007,
    LOGIN_ERROR_PROOF_WATER = 140022008,
    LOGIN_ERROR_REFUSE_PASSWORD_LOGIN = 140022009,
    LOGIN_ERROR_NEW_DEVICE = 140022010,
    LOGIN_ERROR_UNUSUAL_DEVICE = 140022011,
    LOGIN_ERROR_INVALID_COOKIE = 140022012,
    LOGIN_ERROR_ACCOUNT_OR_PASSWORD_ERROR = 140022013,
    LOGIN_ERROR_EXPIRE_TICKET = 140022014,
    LOGIN_ERROR_KICKED_TICKET = 140022015,
    LOGIN_ERROR_ILLEGAL_TICKET = 140022016,
    LOGIN_ERROR_SEC_BEAT = 140022017,
    LOGIN_ERROR_ACCOUNT_NOT_UIN = 140022018,
    LOGIN_ERROR_NEED_VERIFY_REAL_NAME = 140022019,

    LOGIN_ERROR_NICE_ACCOUNT_EXPIRED = 150022020,
    LOGIN_ERROR_BLACK_ACCOUNT = 150022021,
    LOGIN_ERROR_TOO_OFTEN = 150022022,
    LOGIN_ERROR_TOO_MANY_TIMES_TODAY = 150022023,
    LOGIN_ERROR_UNREGISTERED = 150022024,
    LOGIN_ERROR_NICE_ACCOUNT_PARENT_CHILD_EXPIRED = 150022025,
    LOGIN_ERROR_SMS_INVALID = 150022026,
    LOGIN_ERROR_TGTGT_EXCHANGE_A1_FORBID = 150022027,
    LOGIN_ERROR_REMIND_CANCELLED_STATUS = 150022028,
    LOGIN_ERROR_MULTIPLE_PASSWORD_INCORRECT = 150022029
}

internal enum NTLoginPasswordVerifyResult
{
    LOGIN_ERROR_SUCCESS = 0,
    LOGIN_ERROR_SCAN = 1,
    LOGIN_ERROR_PASSWORD = 2
}

internal enum NTLoginCodeType
{
    CODE_TYPE_AUTHCODE = 0,
    CODE_TYPE_TGT = 1,
    CODE_TYPE_A2 = 2,
    CODE_TYPE_TGTGT = 3
}

#endregion

#region Head

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginHead
{
    [ProtoMember(1)] public NTLoginUserInfo UserInfo { get; set; }

    [ProtoMember(2)] public NTLoginClientInfo ClientInfo { get; set; }
    
    [ProtoMember(3)] public NTLoginAppInfo AppInfo { get; set; }
    
    [ProtoMember(4)] public NTLoginErrorInfo ErrorInfo { get; set; }
    
    [ProtoMember(5)] public NTLoginCookie? Cookie { get; set; }
    
    [ProtoMember(6)] public NTLoginSecurityInfo SecurityInfo { get; set; }
    
    [ProtoMember(7)] public NTLoginSdkInfo SdkInfo { get; set; }
    
    [ProtoMember(8)] public NTLoginLongCookie LongCookie { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginUserInfo
{
    [ProtoMember(1)] public string Account { get; set; }
    
    [ProtoMember(2)] public uint CountryCode { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginClientInfo
{
    [ProtoMember(1)] public string DeviceType { get; set; }
    
    [ProtoMember(2)] public string DeviceName { get; set; }
    
    [ProtoMember(3)] public NTLoginPlatform Platform { get; set; }
    
    [ProtoMember(4)] public byte[] Guid { get; set; }
    
    [ProtoMember(5)] public uint Pubno { get; set; }
    
    [ProtoMember(6)] public uint ClientVer { get; set; }
    
    [ProtoMember(7)] public uint ClientType { get; set; }
    
    [ProtoMember(8)] public uint SsoVer { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginAppInfo
{
    [ProtoMember(1)] public string Version { get; set; }
    
    [ProtoMember(2)] public int AppId { get; set; }
    
    [ProtoMember(3)] public string AppName { get; set; }
    
    [ProtoMember(4)] public uint ClientA1Version { get; set; }
    
    [ProtoMember(5)] public string Qua { get; set; }
    
    [ProtoMember(6)] public NTLoginOpenInfo OpenInfo { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginOpenInfo
{
    [ProtoMember(1)] public uint AppId { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginSdkInfo
{
    [ProtoMember(1)] public uint Version { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginSecurityInfo;

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginCookie
{
    [ProtoMember(1)] public string CookieContent { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginLongCookie
{
    [ProtoMember(1)] public byte[] Content { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginErrorInfo
{
    [ProtoMember(1)] public ulong ErrCode { get; set; }
    
    [ProtoMember(2)] public string StrTipsTitle { get; set; }
    
    [ProtoMember(3)] public string StrTipsContent { get; set; }
    
    [ProtoMember(4)] public string StrJumpWording { get; set; }
    
    [ProtoMember(5)] public string StrJumpUrl { get; set; }
    
    [ProtoMember(6)] public NTLoginErrorDetail MsgDetail { get; set; }
    
    [ProtoMember(7)] public List<NTLoginButton> RptMsgButton { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginErrorDetail
{
    [ProtoMember(1)] public NTLoginErrorNeedVerifyNewDevice MsgNeedVerifyNewDevice { get; set; }
    
    [ProtoMember(2)] public NTLoginErrorUnregistered MsgUnregistered { get; set; }
    
    [ProtoMember(3)] public NTLoginErrorBeenForbiden MsgBeenForbiden { get; set; }
    
    [ProtoMember(4)] public NTLoginErrorNiceAccountExpire MsgNiceAccountExpire { get; set; }

    [ProtoMember(5)] public NTLoginStringWrap? CheckUpSms { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginErrorNeedVerifyNewDevice
{
    [ProtoMember(1)] public bool AllowGateWayVerify { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginErrorUnregistered
{
    [ProtoMember(1)] public string UnregisteredSig { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginErrorBeenForbiden
{
    [ProtoMember(1)] public uint Area { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginErrorNiceAccountExpire
{
    [ProtoMember(1)] public string ExpireSig { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginButton
{
    [ProtoMember(1)] public string Wording { get; set; }
    
    [ProtoMember(2)] public string Url { get; set; }
}
#endregion

#region Body

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginCommonInfo
{
    [ProtoMember(1)] public uint Face { get; set; }

    [ProtoMember(2)] public string Nick { get; set; }

    [ProtoMember(3)] public uint Gender { get; set; }

    [ProtoMember(4)] public uint Flag { get; set; }

    [ProtoMember(5)] public uint Age { get; set; }

    [ProtoMember(6)] public int SvrFlag { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginTgtInfo
{
    [ProtoMember(1)] public byte[] Tgt { get; set; }

    [ProtoMember(2)] public byte[] GtkeyTgt { get; set; }

    [ProtoMember(3)] public uint TgtVer { get; set; }

    [ProtoMember(4)] public ulong Priority { get; set; }

    [ProtoMember(5)] public ulong RefreshInterval { get; set; }

    [ProtoMember(6)] public ulong ValidateInterval { get; set; }

    [ProtoMember(7)] public ulong TryRefreshInterval { get; set; }

    [ProtoMember(8)] public ulong TryRefreshCount { get; set; }

    [ProtoMember(9)] public ulong DstAppid { get; set; }

    [ProtoMember(10)] public byte[] GtkeyTgtpwd { get; set; }

    [ProtoMember(11)] public NTLoginCommonInfo CommInfo { get; set; }

    [ProtoMember(12)] public byte[] SigSession { get; set; }

    [ProtoMember(13)] public byte[] SigSessionKey { get; set; }

    [ProtoMember(14)] public ulong NextRefreshGap { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginSTInfo
{
    [ProtoMember(1)] public byte[] St { get; set; }

    [ProtoMember(2)] public byte[] GtkeySt { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginSTHttpInfo
{
    [ProtoMember(1)] public uint AllowPtlogin { get; set; }

    [ProtoMember(2)] public byte[] StHttp { get; set; }

    [ProtoMember(3)] public byte[] GtkeyStHttp { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginControlRefreshTime
{
    [ProtoMember(1)] public ulong NextStartRefreshTime { get; set; }

    [ProtoMember(2)] public ulong ExpireTime { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginTickets
{
    [ProtoMember(3)] public byte[] A1 { get; set; }

    [ProtoMember(4)] public byte[] A2 { get; set; }

    [ProtoMember(5)] public byte[] D2 { get; set; }

    [ProtoMember(6)] public byte[] D2Key { get; set; }

    [ProtoMember(7)] public byte[] AuthCode { get; set; }

    [ProtoMember(8)] public NTLoginTgtInfo TgtInfo { get; set; }

    [ProtoMember(9)] public NTLoginSTInfo StInfo { get; set; }

    [ProtoMember(10)] public byte[] SecExtra { get; set; }

    [ProtoMember(11)] public NTLoginSTHttpInfo StHttpInfo { get; set; }

    [ProtoMember(12)] public NTLoginControlRefreshTime A1RefreshTime { get; set; }

    [ProtoMember(13)] public byte[] Nopicsig { get; set; }

    [ProtoMember(14)] public byte[] A1Key { get; set; }

    [ProtoMember(15)] public ulong A1Seq { get; set; }

    [ProtoMember(16)] public NTLoginControlRefreshTime A2RefreshTime { get; set; }

    [ProtoMember(17)] public byte[] A2Key { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginUserProfile
{
    [ProtoMember(1)] public byte[] NickName { get; set; }

    [ProtoMember(2)] public bool RegisterWithoutPassword { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginAccoutInfo
{
    [ProtoMember(1)] public ulong Uin { get; set; }

    [ProtoMember(2)] public string Uid { get; set; }

    [ProtoMember(3)] public NTLoginUserProfile UserProfile { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginIframe
{
    [ProtoMember(1)] public string IframeSig { get; set; }

    [ProtoMember(2)] public string IframeRandstr { get; set; }

    [ProtoMember(3)] public string IframeSid { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginSecProtect
{
    [ProtoMember(1)] public byte[] NewDeviceCheckSig { get; set; }

    [ProtoMember(2)] public byte[] UnusualDeviceCheckSig { get; set; }

    [ProtoMember(3)] public string UnusualDeviceQrSig { get; set; }

    [ProtoMember(4)] public string UinToken { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginSecCheck
{
    [ProtoMember(3)] public string IframeUrl { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginLoginProcessReqBody
{
    [ProtoMember(1)] public bool NeedRemindCancellatedStatus { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginLoginProcessRspBody
{
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginPasswordLoginReqBody
{
    [ProtoMember(1)] public byte[] A1 { get; set; }

    [ProtoMember(2)] public NTLoginIframe? Iframe { get; set; }

    [ProtoMember(3)] public byte[] NewDeviceCheckSucceedSig { get; set; }

    [ProtoMember(4)] public string RegisterSucceedSig { get; set; }

    [ProtoMember(5)] public NTLoginLoginProcessReqBody LoginProcessReq { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginPasswordLoginRspBody
{
    [ProtoMember(1)] public NTLoginTickets Tickets { get; set; }

    [ProtoMember(2)] public NTLoginSecCheck SecCheck { get; set; }

    [ProtoMember(3)] public NTLoginSecProtect SecProtect { get; set; }

    [ProtoMember(4)] public NTLoginAccoutInfo Account { get; set; }

    [ProtoMember(5)] public NTLoginLoginProcessRspBody LoginProcessRsp { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginPasswordLoginNewDeviceReqBody
{
    [ProtoMember(1)] public byte[] NewDeviceCheckSucceedSig { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginPasswordLoginNewDeviceRspBody
{
    [ProtoMember(1)] public NTLoginTickets Tickets { get; set; }

    [ProtoMember(3)] public NTLoginAccoutInfo Account { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginPasswordLoginUnusualDeviceReqBody
{
    [ProtoMember(1)] public byte[] A1 { get; set; }

    [ProtoMember(2)] public byte[] UnusualDeviceCheckSucceedSig { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginPasswordLoginUnusualDeviceRspBody
{
    [ProtoMember(1)] public NTLoginTickets Tickets { get; set; }

    [ProtoMember(2)] public NTLoginAccoutInfo Account { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginEasyLoginReqBody
{
    [ProtoMember(1)] public byte[] A1 { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginEasyLoginRspBody
{
    [ProtoMember(1)] public NTLoginTickets Tickets { get; set; }

    [ProtoMember(2)] public NTLoginSecCheck SecCheck { get; set; }

    [ProtoMember(3)] public NTLoginSecProtect SecProtect { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginEasyLoginUnusualDeviceReqBody
{
    [ProtoMember(1)] public byte[] A1 { get; set; }

    [ProtoMember(2)] public byte[] UnusualDeviceCheckSucceedSig { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginEasyLoginUnusualDeviceRspBody
{
    [ProtoMember(1)] public NTLoginTickets Tickets { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginTGTExchangeFastLoginReqBody
{
    [ProtoMember(1)] public byte[] Tgt { get; set; }

    [ProtoMember(2)] public byte[] SecExtra { get; set; }

    [ProtoMember(3)] public NTLoginCodeType CodeType { get; set; }
}


[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginTGTExchangeFastLoginRspBody
{
    [ProtoMember(1)] public NTLoginTickets Tickets { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginRefreshTicketReqBody
{
    [ProtoMember(1)] public byte[] A1 { get; set; }

    [ProtoMember(2)] public byte[] Nopicsig { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginRefreshTicketRspBody
{
    [ProtoMember(1)] public NTLoginTickets Tickets { get; set; }

    [ProtoMember(2)] public NTLoginAccoutInfo Account { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginRefreshA2ReqBody
{
    [ProtoMember(1)] public byte[] A2 { get; set; }

    [ProtoMember(2)] public byte[] D2 { get; set; }

    [ProtoMember(3)] public byte[] D2Key { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginRefreshA2RspBody
{
    [ProtoMember(1)] public NTLoginTickets Tickets { get; set; }

    [ProtoMember(2)] public NTLoginAccoutInfo Account { get; set; }
}

#endregion

[ProtoPackable]
internal partial class NTLoginCommon
{
    [ProtoMember(1)] public NTLoginHead Head { get; set; }
    
    [ProtoMember(2)] public ReadOnlyMemory<byte> Body { get; set; }
}

[ProtoPackable]
internal partial class NTLoginAndroidExt
{
    [ProtoMember(1)] public int Field1 { get; set; }
    
    [ProtoMember(2)] public string Uin { get; set; }
}

[ProtoPackable]
internal partial class NTLoginAndroidCommon
{
    [ProtoMember(2)] public NTLoginCommon Common { get; set; }
    
    [ProtoMember(3)] public NTLoginAndroidExt Ext { get; set; }
}

// Variant whose field 2 is the raw (already serialized) inner message,
// used by bodies that do not follow the { 1: head, 2: body } NTLoginCommon shape
// (e.g. SsoQRLogin* whose root is a flat { 1: head, 2: qr_sig, ... }).
[ProtoPackable]
internal partial class NTLoginAndroidCommonRaw
{
    [ProtoMember(2)] public ReadOnlyMemory<byte> Common { get; set; }

    [ProtoMember(3)] public NTLoginAndroidExt Ext { get; set; }
}

#region Android NTLogin extended bodies (QQ 9.2.35 libkernel.so reverse)

// ---------------------------------------------------------------------------
// Shared response shell (native parser sub_29610F8, "LoginRspShell").
// Appears as field 1 of most Android NTLogin response bodies.
// Maps to Java com.tencent.qqnt.kernel.nativeinterface.LoginRspInfo.
// ---------------------------------------------------------------------------

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginRspShell
{
    [ProtoMember(4)] public NTLoginRspInfo Info { get; set; }

    [ProtoMember(5)] public NTLoginBytesWrap? Cookie { get; set; } // server-updated longCookie

    [ProtoMember(8)] public NTLoginBytesWrap? Context { get; set; } // loginContext 回写
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginBytesWrap
{
    [ProtoMember(1)] public byte[] Data { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginStringWrap
{
    [ProtoMember(1)] public string Value { get; set; }
}

// Field order follows Java LoginRspInfo: errCode/errMsg/jumpUrl/jumpWord/tipsTitle.
// tipsContent is not read by the native parser (absent in this version).
[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginRspInfo
{
    [ProtoMember(1)] public long ErrCode { get; set; }

    [ProtoMember(2)] public string ErrMsg { get; set; }

    [ProtoMember(3)] public string JumpUrl { get; set; }

    [ProtoMember(4)] public string JumpWord { get; set; }

    [ProtoMember(5)] public string TipsTitle { get; set; }

    [ProtoMember(6)] public NTLoginRspMsgDetail MsgDetail { get; set; }

    [ProtoMember(7)] public List<NTLoginButton> MsgButton { get; set; }

    [ProtoMember(8)] public bool Flag { get; set; } // semantics UNCONFIRMED
}

// Maps to Java ErrorDetail{msgNeedVerifyNewDevice,msgUnregistered,msgBeenForbiden,msgNiceAccountExpire,checkUpSms}
[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginRspMsgDetail
{
    [ProtoMember(1)] public bool AllowGateWayVerify { get; set; }

    [ProtoMember(2)] public NTLoginStringWrap? Unregistered { get; set; } // {1: unregisteredSig}

    [ProtoMember(3)] public int Area { get; set; }

    [ProtoMember(4)] public NTLoginStringWrap? NiceAccountExpire { get; set; } // {1: expireSig}

    [ProtoMember(5)] public NTLoginStringWrap? CheckUpSms { get; set; } // {1: checkUpSmsBackupTips}
}

// ---------------------------------------------------------------------------
// Shared BindUinInfo (CheckA1List rsp / CheckSms rsp / CheckGateWayCode rsp).
// Maps to Java BindUinInfo / UinInfo; all field numbers confirmed in
// sub_29617E8 (0x296184C/0x296182C, entries 0x29618D4-0x296197C).
// ---------------------------------------------------------------------------

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginUinInfo
{
    [ProtoMember(1)] public string MaskUin { get; set; }

    [ProtoMember(2)] public string Nick { get; set; }

    [ProtoMember(3)] public string ImageUrl { get; set; }

    [ProtoMember(4)] public byte[] EncryptUin { get; set; }

    [ProtoMember(5)] public string KeyUin { get; set; }

    [ProtoMember(6)] public byte[] A1Sig { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginBindUinInfo
{
    [ProtoMember(1)] public List<NTLoginUinInfo> UinInfoList { get; set; }

    [ProtoMember(2)] public string UnbindWording { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginBindUinWrap
{
    [ProtoMember(1)] public NTLoginBindUinInfo Info { get; set; }
}

// ---------------------------------------------------------------------------
// SsoNTLoginGetSaltList (encoder sub_29609C4 "EncodeGetSaltListRequest",
// parser sub_2960E80). All request fields are conditional.
// ---------------------------------------------------------------------------

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginGetSaltListReqBody
{
    [ProtoMember(1)] public NTLoginVerifySignMsg? VerifySign { get; set; } // iframe, only when present

    [ProtoMember(2)] public byte[]? EncryptUin { get; set; } // "get salt list with encrypt uin: {}"

    [ProtoMember(3)] public byte[]? NewDeviceSign { get; set; } // only when new-device state present
}

// Three opaque byte triples written by sub_2960DD0 (verify credential container).
[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginVerifySignMsg
{
    [ProtoMember(1)] public byte[] Sig1 { get; set; }

    [ProtoMember(2)] public byte[] Sig2 { get; set; }

    [ProtoMember(3)] public byte[] Sig3 { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginGetSaltListRspBody
{
    [ProtoMember(1)] public List<NTLoginSaltEntry> Entries { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginSaltEntry
{
    [ProtoMember(1)] public byte[] Salt { get; set; }

    [ProtoMember(2)] public ulong Uin { get; set; } // semantics (uin/salt) UNCONFIRMED
}

// ---------------------------------------------------------------------------
// SsoNTLoginCheckA1List (builder sub_29615BC, parser sub_296173C/0x29617E8).
// ---------------------------------------------------------------------------

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginCheckA1ListReqBody
{
    [ProtoMember(1)] public List<NTLoginA1Candidate> Candidates { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginA1Candidate
{
    [ProtoMember(1)] public string Salt { get; set; } // server salt entry, echoed back

    [ProtoMember(2)] public byte[] A1 { get; set; } // GenerateClientA1 encrypted blob (not protobuf)
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginCheckA1ListRspBody
{
    [ProtoMember(1)] public NTLoginBindUinInfo BindUinInfo { get; set; }
}

// ---------------------------------------------------------------------------
// SsoNTLoginOptimusLogin (encoder sub_2961A88 "EncodeOptimusLoginRequest").
// Response reuses NTLoginPasswordLoginRspBody (sub_2961D18 ≡ sub_29624DC).
// ---------------------------------------------------------------------------

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginOptimusLoginReqBody
{
    [ProtoMember(1)] public byte[] ClientA1Sig { get; set; } // required, "has no client a1 sig!"

    [ProtoMember(2)] public byte[]? VerifyCredentialA { get; set; } // written when verify union type == 1

    [ProtoMember(3)] public byte[] EncryptUin { get; set; } // required, "has no encrypt_uin!"

    [ProtoMember(4)] public byte[]? VerifyCredentialB { get; set; } // written when verify union type == 2

    [ProtoMember(5)] public NTLoginLoginProcessReqBody? LoginProcessReq { get; set; }
}

// ---------------------------------------------------------------------------
// SsoNTLoginRapidLogin (encoder sub_29664C4 "EncodeRapidLoginRequest",
// parser sub_2966630 "DecodeRapidLoginResponse").
// Root is { 1: shared head, 2: auth }; only auth is modelled here (head goes
// into the NTLoginCommon envelope slot like every other command).
// ---------------------------------------------------------------------------

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginRapidLoginReqBody
{
    [ProtoMember(1)] public byte[] A1 { get; set; } // LoginSigRecord+0x30

    [ProtoMember(2)] public byte[]? NoPicSig { get; set; } // optional, "RapidLogin with empty no_pic_sig!"

    [ProtoMember(3)] public NTLoginRapidLoginDest Dest { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginRapidLoginDest
{
    [ProtoMember(1)] public NTLoginRapidLoginDestInner Inner { get; set; }
}

// destAppid / bundleId only; destSubAppid / scheme / publicKey are NOT sent on wire.
[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginRapidLoginDestInner
{
    [ProtoMember(2)] public ulong DestAppid { get; set; }

    [ProtoMember(3)] public string BundleId { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginRapidLoginRspBody
{
    [ProtoMember(1)] public NTLoginRspShell Head { get; set; }

    [ProtoMember(2)] public NTLoginRapidLoginRspData Data { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginRapidLoginRspData
{
    [ProtoMember(1)] public NTLoginRapidLoginRspDataInner Inner { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginRapidLoginRspDataInner
{
    // Native reads this field twice into "sig" and "universal_link";
    // sig content is a wtlogin-style TLV stream containing TLV 0x106(A1)/0x16A(NoPicSig)/0x10C(A1Key).
    [ProtoMember(1)] public string Sig { get; set; }
}

// ---------------------------------------------------------------------------
// SsoNTLoginAuthNewDevice (builder sub_2963B30 "EncodeAuthNewDeviceRequest",
// parser sub_2963C34). Root is { 1: shared head, 2: body }.
// ---------------------------------------------------------------------------

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginAuthNewDeviceReqBody
{
    [ProtoMember(1)] public byte[] NewDeviceVerifySig { get; set; } // "AuthNewDevice with newDeviceVerifySig size {}!"

    [ProtoMember(2)] public byte[] Credential { get; set; } // GenerateClientA1(maskuin, salt, pwd) output
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginAuthNewDeviceRspBody
{
    [ProtoMember(1)] public NTLoginRspShell LoginResult { get; set; }

    [ProtoMember(2)] public NTLoginBytesWrap Sig { get; set; } // {1: sig}, "AuthNewDevice rsp with sig: {} ."
}

// ---------------------------------------------------------------------------
// SsoNTLoginGetSms (encoder sub_2962CAC, parser sub_2962DF8).
// Root is { 1: shared head, 2: spec }; spec carries the verify-sign wrap.
// ---------------------------------------------------------------------------

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginGetSmsReqBody
{
    [ProtoMember(1)] public NTLoginVerifySignMsg? Sign { get; set; } // only when verifyType == 1
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginGetSmsRspBody
{
    [ProtoMember(1)] public NTLoginRspShell Shell { get; set; }
}

// ---------------------------------------------------------------------------
// SsoNTLoginCheckSms (encoder sub_2963494, parser sub_296355C).
// Root is { 1: shared body/head, 2: spec{ 1: account, 2: nonce } }.
// ---------------------------------------------------------------------------

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginCheckSmsReqBody
{
    [ProtoMember(1)] public string Account { get; set; }

    [ProtoMember(2)] public NTLoginNonce Nonce { get; set; }
}

// Written by sub_29625EC: unix-seconds timestamp + MD5(raw random nonce) digest.
// Same layout is reused as CheckGateWayCode.time_sign and CheckThirdCode.sec_context.
[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginNonce
{
    [ProtoMember(1)] public ulong Timestamp { get; set; }

    [ProtoMember(2)] public byte[] Md5Nonce { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginCheckSmsRspBody
{
    [ProtoMember(1)] public NTLoginRspShell Shell { get; set; }

    [ProtoMember(2)] public NTLoginBindUinWrap BindUin { get; set; }
}

// ---------------------------------------------------------------------------
// SsoNTLoginCheckGateWayCode (encoder sub_2963F90 "EncodeCheckGatewayCodeRequest",
// parser sub_2964180 "DecodeCheckGatewayCodeResponse").
// ---------------------------------------------------------------------------

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginCheckGatewayCodeReqBody
{
    [ProtoMember(1)] public string PhoneToken { get; set; } // "phone_token is empty!" when missing

    [ProtoMember(2)] public NTLoginNonce TimeSign { get; set; }

    [ProtoMember(3)] public byte[]? VerifySign { get; set; } // deviceCheckSucceedSig, only when verifyType == 1
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginCheckGatewayCodeRspBody
{
    [ProtoMember(1)] public NTLoginRspShell Shell { get; set; }

    [ProtoMember(2)] public NTLoginBindUinWrap BindUin { get; set; }
}

// ---------------------------------------------------------------------------
// SsoNTLoginCheckThirdCode (encoder sub_2964290 "EncodeCheckThirdCodeRequest",
// parser sub_29644D0 "DecodeCheckThirdCodeResponse").
// loginContext / appInfo do NOT enter the packet.
// ---------------------------------------------------------------------------

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginCheckThirdCodeReqBody
{
    [ProtoMember(1)] public NTLoginNonce SecContext { get; set; } // always written

    [ProtoMember(2)] public NTLoginWechatReqBody? WechatReqBody { get; set; }

    [ProtoMember(3)] public NTLoginVerifySignMsg? Iframe { get; set; } // only when verifyType == 1
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginWechatReqBody
{
    [ProtoMember(1)] public string WechatProfileSig { get; set; } // "wechat login req :{}!"
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginCheckThirdCodeRspBody
{
    [ProtoMember(2)] public NTLoginWechatRspBody WechatRspBody { get; set; } // field 1 unread by decoder
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginWechatRspBody
{
    [ProtoMember(1)] public string WechatProfile { get; set; } // "wechat login rsp :{}!"
}

// ---------------------------------------------------------------------------
// SsoQRLogin* (qr_login_mgr.cc / login_codec.cc).
// Unlike other NTLogin commands the request root is FLAT:
// { 1: shared head, 2: qr_sig, 3..: command fields } — there is no body slot,
// so these classes include the head inline and are encrypted as-is
// (see NTLoginCommon.EncodeAndroidRaw).
// ---------------------------------------------------------------------------

// ScanQr: encoder sub_2964668. JNI ScanQrReq{scene, qrSig, uin}.
[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginScanQrRequest
{
    [ProtoMember(1)] public NTLoginHead Head { get; set; }

    [ProtoMember(2)] public byte[] QrSig { get; set; }

    [ProtoMember(3)] public uint Scene { get; set; } // QrScanScene ordinal
}

// AuthQr: encoder sub_2964E84. JNI AuthQrReqInfo{needA1, qrSig, opSwitch, uin}.
[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginAuthQrRequest
{
    [ProtoMember(1)] public NTLoginHead Head { get; set; }

    [ProtoMember(2)] public byte[] QrSig { get; set; }

    [ProtoMember(3)] public byte[]? A1 { get; set; } // only when needA1

    [ProtoMember(4)] public uint OpSwitch { get; set; } // QrSwitchOperation ordinal

    [ProtoMember(5)] public byte[]? NoPicSig { get; set; } // only when needA1
}

// CancleQr: encoder sub_2964FB0 ("EncodeCancelQrRequest empty sig!" when missing).
[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginCancelQrRequest
{
    [ProtoMember(1)] public NTLoginHead Head { get; set; }

    [ProtoMember(2)] public byte[] QrSig { get; set; }
}

// RejectQr: encoder sub_2965070.
[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginRejectQrRequest
{
    [ProtoMember(1)] public NTLoginHead Head { get; set; }

    [ProtoMember(2)] public byte[] QrSig { get; set; }
}

// ScanQr response: decoder sub_29646F4 "DecodeScanQrResponse" (login_codec.cc:1047-1084).
// Maps 1:1 to Java ScanQrRsp. Field 8 is not read by the decoder (reserved).
[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginScanQrResponse
{
    [ProtoMember(1)] public NTLoginRspShell RspInfo { get; set; }

    [ProtoMember(2)] public string DstAppName { get; set; }

    [ProtoMember(3)] public string LoginCity { get; set; }

    [ProtoMember(4)] public string LoginDevType { get; set; }

    [ProtoMember(5)] public string LoginDevName { get; set; }

    [ProtoMember(6)] public bool NeedA1 { get; set; }

    [ProtoMember(7)] public NTLoginQrOpenAppInfo DstOpenAppInfo { get; set; }

    [ProtoMember(9)] public uint SecCheckResult { get; set; } // QrSecCheckResult: KSAFE/KRISK/KREJECT

    [ProtoMember(10)] public NTLoginQrTipsCtrl Tips { get; set; }

    [ProtoMember(11)] public NTLoginQrAutoLoginInfo AutoLogin { get; set; }

    [ProtoMember(12)] public uint LoginPlat { get; set; } // QrLoginPlat: DEFAULT/WINDOWS/MAC/IPAD/IWATCH
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginQrOpenAppInfo
{
    [ProtoMember(1)] public ulong OpenAppid { get; set; }

    [ProtoMember(2)] public uint AppType { get; set; } // QrOpenAppType

    [ProtoMember(3)] public string ComeFrom { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginQrTipsCtrl
{
    [ProtoMember(1)] public uint Color { get; set; } // QrTipsColor

    [ProtoMember(2)] public string Tips { get; set; }

    [ProtoMember(3)] public bool NeedSecCheck { get; set; }

    [ProtoMember(4)] public string SecCheckTips { get; set; }

    [ProtoMember(5)] public uint SecCheckTipsColor { get; set; }

    [ProtoMember(6)] public uint SecCheckConfirmTime { get; set; }
}

[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginQrAutoLoginInfo
{
    [ProtoMember(1)] public bool IsShowAutoLoginSwitch { get; set; }

    [ProtoMember(2)] public uint AutoLoginSwitchState { get; set; } // QrAutoLoginSwitchState

    [ProtoMember(3)] public string TipsTitle { get; set; }

    [ProtoMember(4)] public string TipsContent { get; set; }
}

// AuthQr/CancleQr/RejectQr share one response handler (sub_2978B7C) that only
// decodes the common shell (sub_2965130 -> sub_29610F8).
[ProtoPackable(IgnoreDefaultFields = true)]
internal partial class NTLoginQrCommonResponse
{
    [ProtoMember(1)] public NTLoginRspShell RspInfo { get; set; }
}

#endregion
