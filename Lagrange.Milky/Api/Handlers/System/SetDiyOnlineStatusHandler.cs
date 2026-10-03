using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("set_diy_online_status")]
public sealed class SetDiyOnlineStatusHandler(BotContext lagrange) : INoResultApiHandler<SetDiyOnlineStatusHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
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
