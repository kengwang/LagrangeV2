using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Group;

[ApiHandler("set_group_member_admin")]
public sealed class SetGroupMemberAdminHandler(BotContext lagrange) : INoResultApiHandler<SetGroupMemberAdminHandler.Request>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await _lagrange.SetGroupAdmin(request.GroupId, request.UserId, request.IsSet).WaitAsync(ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(long groupId, long userId, bool isSet)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("user_id")] public required long UserId { get; init; } = userId;
        [JsonPropertyName("is_set")] public bool IsSet { get; init; } = isSet;
    }
}
