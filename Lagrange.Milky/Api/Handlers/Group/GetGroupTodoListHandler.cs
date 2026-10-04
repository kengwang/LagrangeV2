using FastEndpoints;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
namespace Lagrange.Milky.Api.Handlers.Group;
/// <summary>QQ extension endpoint: get_group_todo_list.</summary>
public sealed class GetGroupTodoListHandler(BotContext lagrange) : Endpoint<GetGroupTodoListHandler.Request, MilkyApiResponse<GetGroupTodoListHandler.Result>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_group_todo_list"); }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.GetGroupTodoList(request.GroupId, ct);
        return new(new Result { Items = result.Select(x => new Item(x.Sequence, x.Random, x.Text, x.CreatedAt, x.UpdatedAt)).ToArray() });
    }
    public sealed class Request
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; }
    }
    public sealed class Result
    {
        [JsonPropertyName("items")] public IReadOnlyList<Item> Items { get; init; } = [];
    }
    public sealed record Item([property: JsonPropertyName("message_seq")] ulong MessageSeq, [property: JsonPropertyName("random")] long Random,
        [property: JsonPropertyName("text")] string Text, [property: JsonPropertyName("created_at")] ulong CreatedAt, [property: JsonPropertyName("updated_at")] ulong UpdatedAt);
}
