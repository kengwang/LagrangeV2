using Lagrange.Core.Common;

using Lagrange.Core.Internal.Events.System;

using Lagrange.Core.Internal.Packets.Service.Migration;

using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<ClickKeyboardEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x112e_1")]
internal sealed class ClickKeyboardService : OidbService<ClickKeyboardEventReq, ClickKeyboardEventResp, Oidb0x112eReq, Oidb0x112eResp>
{
    protected override uint Command => 0x112e;
    protected override uint Service => 1;
    protected override uint Reserved => 0;
    protected override Task<Oidb0x112eReq> ProcessRequest(ClickKeyboardEventReq request, BotContext context) => Task.FromResult(request.Body);
    protected override Task<ClickKeyboardEventResp> ProcessResponse(Oidb0x112eResp response, BotContext context) => Task.FromResult(new ClickKeyboardEventResp(response));
}

[EventSubscribe<RequestDatabaseKeyEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0xcde_2")]
internal sealed class RequestDatabaseKeyService : OidbService<RequestDatabaseKeyEventReq, RequestDatabaseKeyEventResp, Oidb0xcdeReq, Oidb0xcdeResp>
{
    protected override uint Command => 0xcde;
    protected override uint Service => 2;
    protected override uint Reserved => 0;
    protected override Task<Oidb0xcdeReq> ProcessRequest(RequestDatabaseKeyEventReq request, BotContext context) => Task.FromResult(request.Body);
    protected override Task<RequestDatabaseKeyEventResp> ProcessResponse(Oidb0xcdeResp response, BotContext context) => Task.FromResult(new RequestDatabaseKeyEventResp(response));
}

[EventSubscribe<GetGroupTodoListEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x9474_0")]
internal sealed class GetGroupTodoListService : OidbService<GetGroupTodoListEventReq, GetGroupTodoListEventResp, OidbQueryGroupTopBannersReq, OidbQueryGroupTopBannersResp>
{
    protected override uint Command => 0x9474;
    protected override uint Service => 0;
    protected override uint Reserved => 0;
    protected override Task<OidbQueryGroupTopBannersReq> ProcessRequest(GetGroupTodoListEventReq request, BotContext context) => Task.FromResult(request.Body);
    protected override Task<GetGroupTodoListEventResp> ProcessResponse(OidbQueryGroupTopBannersResp response, BotContext context) => Task.FromResult(new GetGroupTodoListEventResp(response));
}

[EventSubscribe<GetAiVoiceListEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x929d_0")]
internal sealed class GetAiVoiceListService : OidbService<GetAiVoiceListEventReq, GetAiVoiceListEventResp, OidbAiVoiceListReq, OidbAiVoiceListResp>
{
    protected override uint Command => 0x929d;
    protected override uint Service => 0;
    protected override uint Reserved => 0;
    protected override Task<OidbAiVoiceListReq> ProcessRequest(GetAiVoiceListEventReq request, BotContext context) => Task.FromResult(request.Body);
    protected override Task<GetAiVoiceListEventResp> ProcessResponse(OidbAiVoiceListResp response, BotContext context) => Task.FromResult(new GetAiVoiceListEventResp(response));
}

[EventSubscribe<SynthesizeAiVoiceEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x929b_0")]
internal sealed class SynthesizeAiVoiceService : OidbService<SynthesizeAiVoiceEventReq, SynthesizeAiVoiceEventResp, OidbAiVoiceReq, OidbAiVoiceResp>
{
    protected override uint Command => 0x929b;
    protected override uint Service => 0;
    protected override uint Reserved => 0;
    protected override Task<OidbAiVoiceReq> ProcessRequest(SynthesizeAiVoiceEventReq request, BotContext context) => Task.FromResult(request.Body);
    protected override Task<SynthesizeAiVoiceEventResp> ProcessResponse(OidbAiVoiceResp response, BotContext context) => Task.FromResult(new SynthesizeAiVoiceEventResp(response));
}

[EventSubscribe<MiniAppShareEventReq>(Protocols.All)]
[Service("LightAppSvc.mini_app_share.AdaptShareInfo")]
internal sealed class MiniAppShareService : BaseService<MiniAppShareEventReq, MiniAppShareEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(MiniAppShareEventReq input, BotContext context) => ValueTask.FromResult(Lagrange.Core.Utility.ProtoHelper.Serialize(input.Body));
    protected override ValueTask<MiniAppShareEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context) => ValueTask.FromResult(new MiniAppShareEventResp(Lagrange.Core.Utility.ProtoHelper.Deserialize<MiniAppShareResp>(input.Span)));
}
[EventSubscribe<TranscribeGroupVoiceEventReq>(Protocols.All)]
[Service("pttTrans.TransGroupPttReq")]
internal sealed class TranscribeGroupVoiceService : BaseService<TranscribeGroupVoiceEventReq, TranscribeVoiceEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(TranscribeGroupVoiceEventReq input, BotContext context) => ValueTask.FromResult(Lagrange.Core.Utility.ProtoHelper.Serialize(input.Body));
    protected override ValueTask<TranscribeVoiceEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context) => ValueTask.FromResult(new TranscribeVoiceEventResp(Lagrange.Core.Utility.ProtoHelper.Deserialize<PttTransResp>(input.Span)));
}

[EventSubscribe<TranscribePrivateVoiceEventReq>(Protocols.All)]
[Service("pttTrans.TransC2CPttReq")]
internal sealed class TranscribePrivateVoiceService : BaseService<TranscribePrivateVoiceEventReq, TranscribeVoiceEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(TranscribePrivateVoiceEventReq input, BotContext context) => ValueTask.FromResult(Lagrange.Core.Utility.ProtoHelper.Serialize(input.Body));
    protected override ValueTask<TranscribeVoiceEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context) => ValueTask.FromResult(new TranscribeVoiceEventResp(Lagrange.Core.Utility.ProtoHelper.Deserialize<PttTransResp>(input.Span)));
}
