using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class SetGroupMemberSpecialTitleHandler(BotContext lagrange) : Endpoint<SetGroupMemberSpecialTitleHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_group_member_special_title");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await _lagrange.GroupSetSpecialTitle(request.GroupId, request.UserId, request.SpecialTitle).WaitAsync(ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(long groupId, long userId, string specialTitle)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("user_id")] public required long UserId { get; init; } = userId;
        [JsonPropertyName("special_title")] public required string SpecialTitle { get; init; } = specialTitle;
    }
}
