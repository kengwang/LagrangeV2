using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("set_qzone_black")]
public sealed class SetQzoneBlackHandler(BotContext lagrange) : INoResultApiHandler<SetQzoneBlackHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
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
