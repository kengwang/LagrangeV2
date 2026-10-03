using System.Security.Cryptography;
using System.Text;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Events.Login;
using Lagrange.Core.Internal.Packets.Login;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Login;

[EventSubscribe<CheckGatewayCodeEventReq>(Protocols.Android)]
[Service("trpc.login.ecdh.EcdhService.SsoNTLoginCheckGateWayCode", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class CheckGatewayCodeService : BaseService<CheckGatewayCodeEventReq, CheckGatewayCodeEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(CheckGatewayCodeEventReq input, BotContext context)
    {
        // loginContext / appInfo do NOT enter the packet (sub_2963F90);
        // time_sign reuses the unix-seconds + MD5(random nonce) layout.
        var reqBody = new NTLoginCheckGatewayCodeReqBody
        {
            PhoneToken = input.PhoneToken,
            TimeSign = new NTLoginNonce
            {
                Timestamp = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds(),
                Md5Nonce = MD5.HashData(Encoding.ASCII.GetBytes(CheckSmsService.RandomString(16)))
            }
        };

        return new ValueTask<ReadOnlyMemory<byte>>(NTLoginCommon.EncodeAndroid(context, reqBody));
    }

    protected override ValueTask<CheckGatewayCodeEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var resp = NTLoginCommon.DecodeAndroidRaw<NTLoginCheckGatewayCodeRspBody>(context, input);
        NTLoginCommon.SaveShellCookie(context, resp.Shell);

        var info = resp.Shell.Info;
        var state = (NTLoginRetCode)info.ErrCode;

        return new ValueTask<CheckGatewayCodeEventResp>(state switch
        {
            NTLoginRetCode.LOGIN_SUCCESS => new CheckGatewayCodeEventResp(state, null, info, resp.BindUin?.Info),
            _ => new CheckGatewayCodeEventResp(state, (info.TipsTitle, info.ErrMsg), info, null)
        });
    }
}

[EventSubscribe<CheckThirdCodeEventReq>(Protocols.Android)]
[Service("trpc.login.ecdh.EcdhService.SsoNTLoginCheckThirdCode", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class CheckThirdCodeService : BaseService<CheckThirdCodeEventReq, CheckThirdCodeEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(CheckThirdCodeEventReq input, BotContext context)
    {
        // loginContext / appInfo do NOT enter the packet (sub_2964290).
        var reqBody = new NTLoginCheckThirdCodeReqBody
        {
            SecContext = new NTLoginNonce
            {
                Timestamp = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds(),
                Md5Nonce = MD5.HashData(Encoding.ASCII.GetBytes(CheckSmsService.RandomString(16)))
            },
            WechatReqBody = new NTLoginWechatReqBody { WechatProfileSig = input.WechatProfileSig }
        };

        return new ValueTask<ReadOnlyMemory<byte>>(NTLoginCommon.EncodeAndroid(context, reqBody));
    }

    protected override ValueTask<CheckThirdCodeEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        // The decoder (sub_29644D0) only reads field 2 of the response root.
        var resp = NTLoginCommon.DecodeAndroidRaw<NTLoginCheckThirdCodeRspBody>(context, input);

        return new ValueTask<CheckThirdCodeEventResp>(
            new CheckThirdCodeEventResp(NTLoginRetCode.LOGIN_SUCCESS, null, resp.WechatRspBody?.WechatProfile));
    }
}
