using FastEndpoints;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;
namespace Lagrange.Milky.Api.Handlers.Message;
public sealed class GetAiVoiceListHandler(BotContext lagrange) : Endpoint<GetAiVoiceListHandler.Request, MilkyApiResponse<GetAiVoiceListHandler.Result>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_ai_voice_list"); }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var values = await lagrange.GetAiVoiceList(request.GroupId, request.ChatType ?? 1, ct);
        return new(new Result { Categories = values.Select(x => new Category(x.Name, x.Voices.Select(v => new Voice(v.Id, v.Name, v.ExampleUrl)).ToArray())).ToArray() });
    }
    public sealed class Request
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; }
        [JsonPropertyName("chat_type")] public uint? ChatType { get; init; }
    }
    public sealed class Result { [JsonPropertyName("categories")] public IReadOnlyList<Category> Categories { get; init; } = []; }
    public sealed record Category([property: JsonPropertyName("name")] string Name, [property: JsonPropertyName("voices")] IReadOnlyList<Voice> Voices);
    public sealed record Voice([property: JsonPropertyName("id")] string Id, [property: JsonPropertyName("name")] string Name, [property: JsonPropertyName("example_url")] string ExampleUrl);
}
