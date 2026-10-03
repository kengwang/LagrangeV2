using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("set_online_status")]
public sealed class SetOnlineStatusHandler(BotContext lagrange) : INoResultApiHandler<SetOnlineStatusHandler.Request>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        if (!await _lagrange.SetStatus(request.Status).WaitAsync(ct)) return new MilkyApiResponse(-1, "set online status failed");
        return new MilkyApiResponse();
    }

    public sealed class Request(uint status)
    {
        [JsonPropertyName("status")] public uint Status { get; init; } = status;
    }
}
