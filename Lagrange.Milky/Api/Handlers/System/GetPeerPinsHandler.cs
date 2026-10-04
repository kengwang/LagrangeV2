using FastEndpoints;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
namespace Lagrange.Milky.Api.Handlers.System;
public sealed class GetPeerPinsHandler(BotContext lagrange) : EndpointWithoutRequest<MilkyApiResponse<GetPeerPinsHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_peer_pins");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(CancellationToken ct) { var x = await lagrange.GetPeerPins(ct).WaitAsync(ct); return new(new Result { PeerUins = [.. x.PeerUins] }); }
    public sealed class Result { [JsonPropertyName("peer_uins")] public List<long> PeerUins { get; init; } = []; }
}
