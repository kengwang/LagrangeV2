using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Friend;

[ApiHandler("set_friend_remark")]
public sealed class SetFriendRemarkHandler(BotContext lagrange) : INoResultApiHandler<SetFriendRemarkHandler.Request>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await _lagrange.SetFriendRemark(request.UserId, request.Remark).WaitAsync(ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(long userId, string remark)
    {
        [JsonPropertyName("user_id")] public required long UserId { get; init; } = userId;
        [JsonPropertyName("remark")] public required string Remark { get; init; } = remark;
    }
}
