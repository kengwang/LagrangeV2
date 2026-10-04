using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class SetGroupWholeMuteHandler(BotContext lagrange) : Endpoint<SetGroupWholeMuteHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_group_whole_mute");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        return !await _lagrange.MuteGroupGlobal(request.GroupId, request.IsMute).WaitAsync(ct)
            ? new MilkyApiResponse(-500, "unknown error")
            : new MilkyApiResponse();
    }

    public sealed class Request(long groupId, bool isMute = true)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("is_mute")] public bool IsMute { get; init; } = isMute;
    }
}
