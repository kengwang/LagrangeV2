using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<FetchQidianCorpEventReq>(Protocols.All)]
[Service("trpc.basic.corp.Datacard.SsoCorpInfo")]
internal sealed class FetchQidianCorpService : BaseService<FetchQidianCorpEventReq, FetchQidianCorpEventResp>
{
    protected override async ValueTask<ReadOnlyMemory<byte>> Build(FetchQidianCorpEventReq input, BotContext context)
    {
        if (input.UserUin <= 0 || input.UserUin > uint.MaxValue) throw new InvalidTargetException(input.UserUin);
        return ProtoHelper.Serialize(new QidianCorpInfoRequest { Uin = (uint)input.UserUin });
    }

    protected override ValueTask<FetchQidianCorpEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        if (input.IsEmpty) throw new OperationException(-1, "Qidian corp response is empty.");
        var value = ProtoHelper.Deserialize<QidianCorpInfoResponse>(input.Span);
        if (string.IsNullOrWhiteSpace(value.CorpName)) throw new OperationException(-1, "Qidian corp information is unavailable.");
        return ValueTask.FromResult(new FetchQidianCorpEventResp(new BotQidianCorpResult { Name = value.CorpName, Intro = value.Intro, Website = value.Website, Slogan = value.Slogan, Address = value.Address, Phone = value.Phone, Email = value.Email }));
    }
}
