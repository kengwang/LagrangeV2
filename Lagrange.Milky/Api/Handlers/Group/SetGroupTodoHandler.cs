using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class SetGroupTodoHandler(BotContext lagrange) : Endpoint<SetGroupTodoHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_group_todo");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
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
