using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class SetOnlineStatusHandler(BotContext lagrange) : Endpoint<SetOnlineStatusHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_online_status");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        if (!await _lagrange.SetStatus(request.Status).WaitAsync(ct)) return new MilkyApiResponse(-1, "set online status failed");
        return new MilkyApiResponse();
    }

    public sealed class Request(uint status)
    {
        [JsonPropertyName("status")] public uint Status { get; init; } = status;
    }
}
