using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class SetQzoneBlackHandler(BotContext lagrange) : Endpoint<SetQzoneBlackHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_qzone_black");
    }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await lagrange.SetQzoneBlack(request.TargetUin, request.Ban, ct).WaitAsync(ct);
        return new MilkyApiResponse();
    }
    public sealed class Request(long targetUin, bool ban = true)
    {
        [JsonPropertyName("target_uin")] public long TargetUin { get; init; } = targetUin;
        [JsonPropertyName("ban")] public bool Ban { get; init; } = ban;
    }
}
