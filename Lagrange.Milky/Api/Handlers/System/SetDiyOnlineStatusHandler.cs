using FastEndpoints;
using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class SetDiyOnlineStatusHandler(BotContext lagrange) : Endpoint<SetDiyOnlineStatusHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_diy_online_status");
    }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Text);
        await lagrange.SetCustomStatus(checked((uint)request.FaceId), request.Text).WaitAsync(ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(int faceId, string text)
    {
        [JsonPropertyName("face_id")] public int FaceId { get; init; } = faceId;
        [JsonPropertyName("text")] public required string Text { get; init; } = text;
    }
}
