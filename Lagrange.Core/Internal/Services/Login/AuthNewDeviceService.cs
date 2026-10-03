using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Events.Login;
using Lagrange.Core.Internal.Packets.Login;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Login;

[EventSubscribe<AuthNewDeviceEventReq>(Protocols.Android)]
[Service("trpc.login.ecdh.EcdhService.SsoNTLoginAuthNewDevice", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class AuthNewDeviceService : BaseService<AuthNewDeviceEventReq, AuthNewDeviceEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(AuthNewDeviceEventReq input, BotContext context)
    {
        var reqBody = new NTLoginAuthNewDeviceReqBody
        {
            NewDeviceVerifySig = input.Sig,
            Credential = PasswordLoginCommon.GenerateClientA1(input.Password, context)
        };

        return new ValueTask<ReadOnlyMemory<byte>>(NTLoginCommon.EncodeAndroid(context, reqBody));
    }

    protected override ValueTask<AuthNewDeviceEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var resp = NTLoginCommon.DecodeAndroidRaw<NTLoginAuthNewDeviceRspBody>(context, input);
        NTLoginCommon.SaveShellCookie(context, resp.LoginResult);

        var info = resp.LoginResult.Info;
        var state = (NTLoginRetCode)info.ErrCode;

        return new ValueTask<AuthNewDeviceEventResp>(state switch
        {
            NTLoginRetCode.LOGIN_SUCCESS => new AuthNewDeviceEventResp(state, null, resp.Sig?.Data),
            _ => new AuthNewDeviceEventResp(state, (info.TipsTitle, info.ErrMsg), resp.Sig?.Data)
        });
    }
}
