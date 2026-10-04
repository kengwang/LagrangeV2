using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class CancelGroupTodoHandler(BotContext lagrange) : Endpoint<CancelGroupTodoHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/cancel_group_todo");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        if (request.MessageSeq is { } sequence) await _lagrange.RemoveGroupTodo(request.GroupId, sequence, ct);
        else await _lagrange.RemoveGroupTodo(request.GroupId).WaitAsync(ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(long groupId)
    {
        [JsonPropertyName("message_seq")] public ulong? MessageSeq { get; init; }
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
    }
}
