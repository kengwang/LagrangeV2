using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class SetGroupSearchHandler(BotContext lagrange) : Endpoint<SetGroupSearchHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_group_search");
    }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
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
