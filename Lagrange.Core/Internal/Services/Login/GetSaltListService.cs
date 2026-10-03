using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Events.Login;
using Lagrange.Core.Internal.Packets.Login;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Login;

[EventSubscribe<GetSaltListEventReq>(Protocols.Android)]
[Service("trpc.login.ecdh.EcdhService.SsoNTLoginGetSaltList", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class GetSaltListService : BaseService<GetSaltListEventReq, GetSaltListEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(GetSaltListEventReq input, BotContext context)
    {
        var reqBody = new NTLoginGetSaltListReqBody
        {
            EncryptUin = input.EncryptUin,
            NewDeviceSign = input.NewDeviceSign
        };

        return new ValueTask<ReadOnlyMemory<byte>>(NTLoginCommon.EncodeAndroid(context, reqBody));
    }

    protected override ValueTask<GetSaltListEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var state = NTLoginCommon.DecodeAndroid<NTLoginGetSaltListRspBody>(context, input, out var head, out var resp);

        return new ValueTask<GetSaltListEventResp>(state switch
        {
            NTLoginRetCode.LOGIN_SUCCESS => new GetSaltListEventResp(state, null, resp.Entries),
            _ when head is not null => new GetSaltListEventResp(state, (head.ErrorInfo.StrTipsTitle, head.ErrorInfo.StrTipsContent), resp.Entries),
            _ => new GetSaltListEventResp(state, null, null)
        });
    }
}

[EventSubscribe<CheckA1ListEventReq>(Protocols.Android)]
[Service("trpc.login.ecdh.EcdhService.SsoNTLoginCheckA1List", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class CheckA1ListService : BaseService<CheckA1ListEventReq, CheckA1ListEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(CheckA1ListEventReq input, BotContext context)
    {
        var reqBody = new NTLoginCheckA1ListReqBody { Candidates = input.Candidates };

        return new ValueTask<ReadOnlyMemory<byte>>(NTLoginCommon.EncodeAndroid(context, reqBody));
    }

    protected override ValueTask<CheckA1ListEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var state = NTLoginCommon.DecodeAndroid<NTLoginCheckA1ListRspBody>(context, input, out var head, out var resp);

        return new ValueTask<CheckA1ListEventResp>(state switch
        {
            NTLoginRetCode.LOGIN_SUCCESS => new CheckA1ListEventResp(state, null, resp.BindUinInfo),
            _ when head is not null => new CheckA1ListEventResp(state, (head.ErrorInfo.StrTipsTitle, head.ErrorInfo.StrTipsContent), null),
            _ => new CheckA1ListEventResp(state, null, null)
        });
    }
}
