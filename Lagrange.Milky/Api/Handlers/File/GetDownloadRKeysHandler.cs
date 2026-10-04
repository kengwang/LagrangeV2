using FastEndpoints;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
namespace Lagrange.Milky.Api.Handlers.File;
public sealed class GetDownloadRKeysHandler(BotContext lagrange) : EndpointWithoutRequest<MilkyApiResponse<GetDownloadRKeysHandler.Result>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_download_rkeys"); }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(CancellationToken ct)
    {
        var keys = await lagrange.GetDownloadRKeys(ct);
        return new(new Result { Keys = keys.Select(x => new Key(x.Type, x.Key, x.CreatedAt, x.TtlSeconds)).ToArray() });
    }
    public sealed class Result { [JsonPropertyName("keys")] public IReadOnlyList<Key> Keys { get; init; } = []; }
    public sealed record Key([property: JsonPropertyName("type")] uint? Type, [property: JsonPropertyName("key")] string Value,
        [property: JsonPropertyName("created_at")] uint? CreatedAt, [property: JsonPropertyName("ttl_seconds")] ulong TtlSeconds);
}
