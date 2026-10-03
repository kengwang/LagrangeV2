using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Events.Login;
using Lagrange.Core.Internal.Packets.Login;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Login;

// SsoQRLogin* commands use a FLAT request root { 1: head, 2: qr_sig, ... } with no
// body slot (login_codec.cc), so they go through EncodeAndroidRaw/DecodeAndroidRaw.
// NOTE: whether the encrypted payload is wrapped in NTLoginAndroidCommon(field 6106 = uin)
// exactly like the other commands is inferred, not capture-verified.

[EventSubscribe<ScanQrEventReq>(Protocols.Android)]
[Service("trpc.login.ecdh.EcdhService.SsoQRLoginScanQr", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class ScanQrService : BaseService<ScanQrEventReq, ScanQrEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(ScanQrEventReq input, BotContext context)
    {
        var request = new NTLoginScanQrRequest
        {
            Head = NTLoginCommon.BuildHead(context),
            QrSig = input.QrSig,
            Scene = input.Scene
        };

        return new ValueTask<ReadOnlyMemory<byte>>(NTLoginCommon.EncodeAndroidRaw(context, request));
    }

    protected override ValueTask<ScanQrEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var resp = NTLoginCommon.DecodeAndroidRaw<NTLoginScanQrResponse>(context, input);
        NTLoginCommon.SaveShellCookie(context, resp.RspInfo);

        var info = resp.RspInfo.Info;
        var state = (NTLoginRetCode)info.ErrCode;

        return new ValueTask<ScanQrEventResp>(state switch
        {
            NTLoginRetCode.LOGIN_SUCCESS => new ScanQrEventResp(state, null, resp),
            _ => new ScanQrEventResp(state, (info.TipsTitle, info.ErrMsg), resp)
        });
    }
}

[EventSubscribe<AuthQrEventReq>(Protocols.Android)]
[Service("trpc.login.ecdh.EcdhService.SsoQRLoginAuthQr", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class AuthQrService : BaseService<AuthQrEventReq, QrCommonEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(AuthQrEventReq input, BotContext context)
    {
        var sigs = context.Keystore.WLoginSigs;
        var request = new NTLoginAuthQrRequest
        {
            Head = NTLoginCommon.BuildHead(context),
            QrSig = input.QrSig,
            OpSwitch = input.OpSwitch,
            // A1 / NoPicSig are only written when the server flagged needA1 in ScanQr.
            A1 = input.NeedA1 ? sigs.A1 : null,
            NoPicSig = input.NeedA1 && sigs.NoPicSig.Length > 0 ? sigs.NoPicSig : null
        };

        return new ValueTask<ReadOnlyMemory<byte>>(NTLoginCommon.EncodeAndroidRaw(context, request));
    }

    protected override ValueTask<QrCommonEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var resp = NTLoginCommon.DecodeAndroidRaw<NTLoginQrCommonResponse>(context, input);
        NTLoginCommon.SaveShellCookie(context, resp.RspInfo);

        var info = resp.RspInfo.Info;
        var state = (NTLoginRetCode)info.ErrCode;

        return new ValueTask<QrCommonEventResp>(state switch
        {
            NTLoginRetCode.LOGIN_SUCCESS => new QrCommonEventResp(state, null),
            _ => new QrCommonEventResp(state, (info.TipsTitle, info.ErrMsg))
        });
    }
}

[EventSubscribe<CancelQrEventReq>(Protocols.Android)]
[Service("trpc.login.ecdh.EcdhService.SsoQRLoginCancleQr", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class CancelQrService : BaseService<CancelQrEventReq, QrCommonEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(CancelQrEventReq input, BotContext context)
    {
        // The command name is spelled "Cancle" on the wire (sub_2964FB0).
        var request = new NTLoginCancelQrRequest
        {
            Head = NTLoginCommon.BuildHead(context),
            QrSig = input.QrSig
        };

        return new ValueTask<ReadOnlyMemory<byte>>(NTLoginCommon.EncodeAndroidRaw(context, request));
    }

    protected override ValueTask<QrCommonEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var resp = NTLoginCommon.DecodeAndroidRaw<NTLoginQrCommonResponse>(context, input);
        NTLoginCommon.SaveShellCookie(context, resp.RspInfo);

        var info = resp.RspInfo.Info;
        var state = (NTLoginRetCode)info.ErrCode;

        return new ValueTask<QrCommonEventResp>(state switch
        {
            NTLoginRetCode.LOGIN_SUCCESS => new QrCommonEventResp(state, null),
            _ => new QrCommonEventResp(state, (info.TipsTitle, info.ErrMsg))
        });
    }
}

[EventSubscribe<RejectQrEventReq>(Protocols.Android)]
[Service("trpc.login.ecdh.EcdhService.SsoQRLoginRejectQr", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class RejectQrService : BaseService<RejectQrEventReq, QrCommonEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(RejectQrEventReq input, BotContext context)
    {
        var request = new NTLoginRejectQrRequest
        {
            Head = NTLoginCommon.BuildHead(context),
            QrSig = input.QrSig
        };

        return new ValueTask<ReadOnlyMemory<byte>>(NTLoginCommon.EncodeAndroidRaw(context, request));
    }

    protected override ValueTask<QrCommonEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var resp = NTLoginCommon.DecodeAndroidRaw<NTLoginQrCommonResponse>(context, input);
        NTLoginCommon.SaveShellCookie(context, resp.RspInfo);

        var info = resp.RspInfo.Info;
        var state = (NTLoginRetCode)info.ErrCode;

        return new ValueTask<QrCommonEventResp>(state switch
        {
            NTLoginRetCode.LOGIN_SUCCESS => new QrCommonEventResp(state, null),
            _ => new QrCommonEventResp(state, (info.TipsTitle, info.ErrMsg))
        });
    }
}
