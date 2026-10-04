using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class SetGroupNameHandler(BotContext lagrange) : Endpoint<SetGroupNameHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_group_name");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await _lagrange.GroupRename(request.GroupId, request.NewGroupName).WaitAsync(ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(long groupId, string newGroupName)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("new_group_name")] public required string NewGroupName { get; init; } = newGroupName;
    }
}
