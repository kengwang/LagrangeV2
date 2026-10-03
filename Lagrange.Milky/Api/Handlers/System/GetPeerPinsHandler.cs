using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;
namespace Lagrange.Milky.Api.Handlers.System;
[ApiHandler("get_peer_pins")]
public sealed class GetPeerPinsHandler(BotContext lagrange) : INoRequestApiHandler<GetPeerPinsHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(CancellationToken ct) { var x = await lagrange.GetPeerPins(ct).WaitAsync(ct); return new(new Result { PeerUins = [.. x.PeerUins] }); }
    public sealed class Result { [JsonPropertyName("peer_uins")] public List<long> PeerUins { get; init; } = []; }
}
