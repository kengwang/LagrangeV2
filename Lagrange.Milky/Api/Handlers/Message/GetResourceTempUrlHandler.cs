using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Message;

public sealed class GetResourceTempUrlHandler(BotContext lagrange) : Endpoint<GetResourceTempUrlHandler.Request, MilkyApiResponse<GetResourceTempUrlHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_resource_temp_url");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        string url = await _lagrange.GetNTV2RichMediaUrl(request.ResourceId).WaitAsync(ct);
        return new MilkyApiResponse<Result>(new Result
        {
            Url = url,
        });
    }

    public sealed class Request(string resourceId)
    {
        [JsonPropertyName("resource_id")] public required string ResourceId { get; init; } = resourceId;
    }

    public sealed class Result
    {
        [JsonPropertyName("url")] public required string Url { get; init; }
    }
}
