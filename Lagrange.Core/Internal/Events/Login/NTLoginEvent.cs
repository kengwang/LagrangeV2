using Lagrange.Core.Events;
using Lagrange.Core.Internal.Packets.Login;

namespace Lagrange.Core.Internal.Events.Login;

internal interface INTLoginEventResp
{
    public NTLoginRetCode State { get; }
    
    public (string, string) Tips { get; }
}

internal class EasyLoginEventReq : ProtocolEvent;

internal class UnusualEasyLoginEventReq : ProtocolEvent;

internal class NewDeviceLoginEventReq(byte[] sig) : ProtocolEvent
{
    public byte[] Sig { get; } = sig;
};

internal class RefreshTicketEventReq : ProtocolEvent;

internal class RefreshA2EventReq : ProtocolEvent;

internal class PasswordLoginEventReq(string password, (string, string, string)? captcha, byte[]? newDeviceSig = null) : ProtocolEvent
{
    public string Password { get; } = password;

    public (string, string, string)? Captcha { get; } = captcha;

    /// <summary>newDeviceCheckSucceedSig obtained after a new-device verify (Android AuthNewDevice).</summary>
    public byte[]? NewDeviceSig { get; } = newDeviceSig;
}

internal class PasswordLoginEventResp(
    NTLoginRetCode state,
    (string, string)? tips,
    string? jumpingUrl,
    NTLoginSecProtect? secProtect = null,
    bool allowGatewayVerify = false,
    bool requiresSms = false) : ProtocolEvent, INTLoginEventResp
{
    public NTLoginRetCode State { get; } = state;

    public (string, string) Tips { get; } = tips ?? (string.Empty, string.Empty);

    public string JumpingUrl { get; } = jumpingUrl ?? string.Empty;

    /// <summary>Device-check signatures carried on NEW_DEVICE / UNUSUAL_DEVICE errors (Android).</summary>
    public NTLoginSecProtect? SecProtect { get; } = secProtect;

    public bool AllowGatewayVerify { get; } = allowGatewayVerify;

    public bool RequiresSms { get; } = requiresSms;
}

internal class EasyLoginEventResp(NTLoginRetCode state, (string, string)? tips, byte[]? unusualSigs) : ProtocolEvent, INTLoginEventResp
{
    public NTLoginRetCode State { get; } = state;

    public (string, string) Tips { get; } = tips ?? (string.Empty, string.Empty);
    
    public byte[]? UnusualSigs { get; } = unusualSigs;
}

internal class UnusualEasyLoginEventResp(NTLoginRetCode state, (string, string)? tips) : ProtocolEvent
{
    public NTLoginRetCode State { get; } = state;

    public (string, string) Tips { get; } = tips ?? (string.Empty, string.Empty);
}

internal class NewDeviceLoginEventResp(NTLoginRetCode state, (string, string)? tips) : ProtocolEvent
{
    public NTLoginRetCode State { get; } = state;

    public (string, string) Tips { get; } = tips ?? (string.Empty, string.Empty);
}

internal class RefreshTicketEventResp(NTLoginRetCode state, (string, string)? tips) : ProtocolEvent
{
    public NTLoginRetCode State { get; } = state;

    public (string, string) Tips { get; } = tips ?? (string.Empty, string.Empty);
}

internal class RefreshA2EventResp(NTLoginRetCode state, (string, string)? tips) : ProtocolEvent
{
    public NTLoginRetCode State { get; } = state;

    public (string, string) Tips { get; } = tips ?? (string.Empty, string.Empty);
}

#region Android NTLogin extended events (QQ 9.2.35 libkernel.so reverse)

internal class GetSaltListEventReq(byte[]? encryptUin, byte[]? newDeviceSign) : ProtocolEvent
{
    public byte[]? EncryptUin { get; } = encryptUin;

    public byte[]? NewDeviceSign { get; } = newDeviceSign;
}

internal class GetSaltListEventResp(NTLoginRetCode state, (string, string)? tips, List<NTLoginSaltEntry>? entries) : ProtocolEvent, INTLoginEventResp
{
    public NTLoginRetCode State { get; } = state;

    public (string, string) Tips { get; } = tips ?? (string.Empty, string.Empty);

    public List<NTLoginSaltEntry>? Entries { get; } = entries;
}

internal class CheckA1ListEventReq(List<NTLoginA1Candidate> candidates) : ProtocolEvent
{
    public List<NTLoginA1Candidate> Candidates { get; } = candidates;
}

internal class CheckA1ListEventResp(NTLoginRetCode state, (string, string)? tips, NTLoginBindUinInfo? bindUinInfo) : ProtocolEvent, INTLoginEventResp
{
    public NTLoginRetCode State { get; } = state;

    public (string, string) Tips { get; } = tips ?? (string.Empty, string.Empty);

    public NTLoginBindUinInfo? BindUinInfo { get; } = bindUinInfo;
}

/// <summary>Login with a server-issued clientA1Sig + encryptUin instead of a password.</summary>
internal class OptimusLoginEventReq(byte[] a1Sig, byte[] encryptUin, byte[]? verifyCredA, byte[]? verifyCredB) : ProtocolEvent
{
    public byte[] A1Sig { get; } = a1Sig;

    public byte[] EncryptUin { get; } = encryptUin;

    /// <summary>Verify credential written at field 2 (verify union type == 1).</summary>
    public byte[]? VerifyCredA { get; } = verifyCredA;

    /// <summary>Verify credential written at field 4 (verify union type == 2).</summary>
    public byte[]? VerifyCredB { get; } = verifyCredB;
}

internal class RapidLoginEventReq(ulong destAppid, string bundleId) : ProtocolEvent
{
    public ulong DestAppid { get; } = destAppid;

    public string BundleId { get; } = bundleId;
}

internal class RapidLoginEventResp(long retCode, string? errorMsg, string? url) : ProtocolEvent
{
    public long RetCode { get; } = retCode;

    public string ErrorMsg { get; } = errorMsg ?? string.Empty;

    /// <summary>wtlogin-style TLV stream (TLV 0x106/0x10C/0x16A), consumed by the open-app jump.</summary>
    public string Url { get; } = url ?? string.Empty;
}

internal class AuthNewDeviceEventReq(string password, byte[] sig) : ProtocolEvent
{
    public string Password { get; } = password;

    /// <summary>newDeviceCheckSucceedSig obtained from a new-device verify flow.</summary>
    public byte[] Sig { get; } = sig;
}

internal class AuthNewDeviceEventResp(NTLoginRetCode state, (string, string)? tips, byte[]? sig) : ProtocolEvent, INTLoginEventResp
{
    public NTLoginRetCode State { get; } = state;

    public (string, string) Tips { get; } = tips ?? (string.Empty, string.Empty);

    public byte[]? Sig { get; } = sig;
}

internal class GetSmsEventReq : ProtocolEvent;

internal class GetSmsEventResp(NTLoginRetCode state, (string, string)? tips, NTLoginRspInfo? info) : ProtocolEvent, INTLoginEventResp
{
    public NTLoginRetCode State { get; } = state;

    public (string, string) Tips { get; } = tips ?? (string.Empty, string.Empty);

    public NTLoginRspInfo? Info { get; } = info;
}

/// <summary>Self-send SMS verification: the client hashes a random nonce the user sent by SMS.</summary>
internal class CheckSmsEventReq(string account, string nonce) : ProtocolEvent
{
    public string Account { get; } = account;

    public string Nonce { get; } = nonce;
}

internal class CheckSmsEventResp(NTLoginRetCode state, (string, string)? tips, NTLoginRspInfo? info, NTLoginBindUinInfo? bindUinInfo) : ProtocolEvent, INTLoginEventResp
{
    public NTLoginRetCode State { get; } = state;

    public (string, string) Tips { get; } = tips ?? (string.Empty, string.Empty);

    public NTLoginRspInfo? Info { get; } = info;

    public NTLoginBindUinInfo? BindUinInfo { get; } = bindUinInfo;
}

internal class CheckGatewayCodeEventReq(string phoneToken) : ProtocolEvent
{
    public string PhoneToken { get; } = phoneToken;
}

internal class CheckGatewayCodeEventResp(NTLoginRetCode state, (string, string)? tips, NTLoginRspInfo? info, NTLoginBindUinInfo? bindUinInfo) : ProtocolEvent, INTLoginEventResp
{
    public NTLoginRetCode State { get; } = state;

    public (string, string) Tips { get; } = tips ?? (string.Empty, string.Empty);

    public NTLoginRspInfo? Info { get; } = info;

    public NTLoginBindUinInfo? BindUinInfo { get; } = bindUinInfo;
}

internal class CheckThirdCodeEventReq(string wechatProfileSig) : ProtocolEvent
{
    public string WechatProfileSig { get; } = wechatProfileSig;
}

internal class CheckThirdCodeEventResp(NTLoginRetCode state, (string, string)? tips, string? wechatProfile) : ProtocolEvent, INTLoginEventResp
{
    public NTLoginRetCode State { get; } = state;

    public (string, string) Tips { get; } = tips ?? (string.Empty, string.Empty);

    public string? WechatProfile { get; } = wechatProfile;
}

internal class ScanQrEventReq(byte[] qrSig, uint scene) : ProtocolEvent
{
    public byte[] QrSig { get; } = qrSig;

    public uint Scene { get; } = scene;
}

internal class ScanQrEventResp(NTLoginRetCode state, (string, string)? tips, NTLoginScanQrResponse? response) : ProtocolEvent, INTLoginEventResp
{
    public NTLoginRetCode State { get; } = state;

    public (string, string) Tips { get; } = tips ?? (string.Empty, string.Empty);

    public NTLoginScanQrResponse? Response { get; } = response;
}

internal class AuthQrEventReq(byte[] qrSig, bool needA1, uint opSwitch) : ProtocolEvent
{
    public byte[] QrSig { get; } = qrSig;

    public bool NeedA1 { get; } = needA1;

    public uint OpSwitch { get; } = opSwitch;
}

internal class CancelQrEventReq(byte[] qrSig) : ProtocolEvent
{
    public byte[] QrSig { get; } = qrSig;
}

internal class RejectQrEventReq(byte[] qrSig) : ProtocolEvent
{
    public byte[] QrSig { get; } = qrSig;
}

/// <summary>Shared response for AuthQr / CancleQr / RejectQr (shell only).</summary>
internal class QrCommonEventResp(NTLoginRetCode state, (string, string)? tips) : ProtocolEvent, INTLoginEventResp
{
    public NTLoginRetCode State { get; } = state;

    public (string, string) Tips { get; } = tips ?? (string.Empty, string.Empty);
}

#endregion
