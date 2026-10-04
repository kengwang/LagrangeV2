using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class GetGroupTodoHandler(BotContext lagrange) : Endpoint<GetGroupTodoHandler.Request, MilkyApiResponse<GetGroupTodoHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_group_todo");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var todo = await _lagrange.GetGroupTodo(request.GroupId).WaitAsync(ct);
        return new MilkyApiResponse<Result>(new Result
        {
            GroupId = checked((long)todo.GroupUin),
            MessageSeq = checked((long)todo.Sequence),
            Preview = todo.Preview,
        });
    }

    public sealed class Request(long groupId)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
    }

    public sealed class Result
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; }
        [JsonPropertyName("message_seq")] public required long MessageSeq { get; init; }
        [JsonPropertyName("preview")] public required string Preview { get; init; }
    }
}
