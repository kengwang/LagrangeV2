using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Events.Login;
using Lagrange.Core.Internal.Packets.Login;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Login;

[EventSubscribe<OptimusLoginEventReq>(Protocols.Android)]
[Service("trpc.login.ecdh.EcdhService.SsoNTLoginOptimusLogin", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class OptimusLoginService : BaseService<OptimusLoginEventReq, PasswordLoginEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(OptimusLoginEventReq input, BotContext context)
    {
        var reqBody = new NTLoginOptimusLoginReqBody
        {
            ClientA1Sig = input.A1Sig,
            EncryptUin = input.EncryptUin,
            VerifyCredentialA = input.VerifyCredA,
            VerifyCredentialB = input.VerifyCredB,
            LoginProcessReq = new NTLoginLoginProcessReqBody
            {
                NeedRemindCancellatedStatus = true
            }
        };

        return new ValueTask<ReadOnlyMemory<byte>>(NTLoginCommon.EncodeAndroid(context, reqBody));
    }

    protected override ValueTask<PasswordLoginEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        // The response body is identical to SsoNTLoginPasswordLogin (sub_2961D18 ≡ sub_29624DC).
        var state = NTLoginCommon.DecodeAndroid<NTLoginPasswordLoginRspBody>(context, input, out var head, out var resp);
        if (state == NTLoginRetCode.LOGIN_SUCCESS) NTLoginCommon.SaveTicket(context, resp.Tickets);

        return new ValueTask<PasswordLoginEventResp>(state switch
        {
            NTLoginRetCode.LOGIN_SUCCESS => new PasswordLoginEventResp(state, null, null),
            NTLoginRetCode.LOGIN_ERROR_PROOF_WATER when head is not null => new PasswordLoginEventResp(state, null, head.ErrorInfo.StrJumpUrl),
            _ when head is not null => new PasswordLoginEventResp(state, (head.ErrorInfo.StrTipsTitle, head.ErrorInfo.StrTipsContent), head.ErrorInfo.StrJumpUrl, resp.SecProtect),
            _ => new PasswordLoginEventResp(state, null, null, resp.SecProtect)
        });
    }
}

[EventSubscribe<RapidLoginEventReq>(Protocols.Android)]
[Service("trpc.login.ecdh.EcdhService.SsoNTLoginRapidLogin", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class RapidLoginService : BaseService<RapidLoginEventReq, RapidLoginEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(RapidLoginEventReq input, BotContext context)
    {
        // destSubAppid / scheme / publicKey never enter the packet (sub_29664C4).
        var reqBody = new NTLoginRapidLoginReqBody
        {
            A1 = context.Keystore.WLoginSigs.A1,
            NoPicSig = context.Keystore.WLoginSigs.NoPicSig.Length > 0 ? context.Keystore.WLoginSigs.NoPicSig : null,
            Dest = new NTLoginRapidLoginDest
            {
                Inner = new NTLoginRapidLoginDestInner
                {
                    DestAppid = input.DestAppid,
                    BundleId = input.BundleId
                }
            }
        };

        return new ValueTask<ReadOnlyMemory<byte>>(NTLoginCommon.EncodeAndroid(context, reqBody));
    }

    protected override ValueTask<RapidLoginEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var resp = NTLoginCommon.DecodeAndroidRaw<NTLoginRapidLoginRspBody>(context, input);
        NTLoginCommon.SaveShellCookie(context, resp.Head);

        var info = resp.Head.Info;
        return new ValueTask<RapidLoginEventResp>(new RapidLoginEventResp(info.ErrCode, info.ErrMsg, resp.Data.Inner.Sig));
    }
}
