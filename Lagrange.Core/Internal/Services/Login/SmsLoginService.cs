using System.Security.Cryptography;
using System.Text;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Events.Login;
using Lagrange.Core.Internal.Packets.Login;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Login;

[EventSubscribe<GetSmsEventReq>(Protocols.Android)]
[Service("trpc.login.ecdh.EcdhService.SsoNTLoginGetSms", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class GetSmsService : BaseService<GetSmsEventReq, GetSmsEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(GetSmsEventReq input, BotContext context)
    {
        // The verify-sign wrap is only written when verifyType == 1; the plain
        // request carries no body fields (sub_2962CAC).
        var reqBody = new NTLoginGetSmsReqBody();

        return new ValueTask<ReadOnlyMemory<byte>>(NTLoginCommon.EncodeAndroid(context, reqBody));
    }

    protected override ValueTask<GetSmsEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var resp = NTLoginCommon.DecodeAndroidRaw<NTLoginGetSmsRspBody>(context, input);
        NTLoginCommon.SaveShellCookie(context, resp.Shell);

        var info = resp.Shell.Info;
        var state = (NTLoginRetCode)info.ErrCode;

        return new ValueTask<GetSmsEventResp>(state switch
        {
            NTLoginRetCode.LOGIN_SUCCESS => new GetSmsEventResp(state, null, info),
            _ => new GetSmsEventResp(state, (info.TipsTitle, info.ErrMsg), info)
        });
    }
}

[EventSubscribe<CheckSmsEventReq>(Protocols.Android)]
[Service("trpc.login.ecdh.EcdhService.SsoNTLoginCheckSms", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class CheckSmsService : BaseService<CheckSmsEventReq, CheckSmsEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(CheckSmsEventReq input, BotContext context)
    {
        // Self-send SMS verification: the caller supplies the nonce that was sent
        // by SMS, and the packet carries timestamp + MD5(nonce) (writer sub_29625EC).
        var nonce = new NTLoginNonce
        {
            Timestamp = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds(),
            Md5Nonce = MD5.HashData(Encoding.ASCII.GetBytes(input.Nonce))
        };
        var reqBody = new NTLoginCheckSmsReqBody
        {
            Account = input.Account,
            Nonce = nonce
        };

        return new ValueTask<ReadOnlyMemory<byte>>(NTLoginCommon.EncodeAndroid(context, reqBody));
    }

    protected override ValueTask<CheckSmsEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var resp = NTLoginCommon.DecodeAndroidRaw<NTLoginCheckSmsRspBody>(context, input);
        NTLoginCommon.SaveShellCookie(context, resp.Shell);

        var info = resp.Shell.Info;
        var state = (NTLoginRetCode)info.ErrCode;

        return new ValueTask<CheckSmsEventResp>(state switch
        {
            NTLoginRetCode.LOGIN_SUCCESS => new CheckSmsEventResp(state, null, info, resp.BindUin?.Info),
            _ => new CheckSmsEventResp(state, (info.TipsTitle, info.ErrMsg), info, null)
        });
    }

    internal static string RandomString(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return string.Create(length, chars, static (span, source) =>
        {
            for (int i = 0; i < span.Length; i++) span[i] = source[Random.Shared.Next(source.Length)];
        });
    }
}
