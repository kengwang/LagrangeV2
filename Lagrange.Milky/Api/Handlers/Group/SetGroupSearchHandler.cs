using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Group;

[ApiHandler("set_group_search")]
public sealed class SetGroupSearchHandler(BotContext lagrange) : INoResultApiHandler<SetGroupSearchHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await lagrange.SetGroupSearch(request.GroupId, request.NoFingerOpen, request.NoCodeFingerOpen, ct).WaitAsync(ct);
        return new MilkyApiResponse();
    }
    public sealed class Request
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; }
        [JsonPropertyName("no_finger_open")] public uint? NoFingerOpen { get; init; }
        [JsonPropertyName("no_code_finger_open")] public uint? NoCodeFingerOpen { get; init; }
    }
}
