using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Group;

[ApiHandler("set_group_todo")]
public sealed class SetGroupTodoHandler(BotContext lagrange) : INoResultApiHandler<SetGroupTodoHandler.Request>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await _lagrange.SetGroupTodo(request.GroupId, checked((ulong)request.MessageSeq)).WaitAsync(ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(long groupId, long messageSeq)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("message_seq")] public required long MessageSeq { get; init; } = messageSeq;
    }
}
