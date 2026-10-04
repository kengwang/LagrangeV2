using FastEndpoints;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
namespace Lagrange.Milky.Api.Handlers.System;
/// <summary>QQ extension endpoint: get_mini_app_ark.</summary>
public sealed class GetMiniAppArkHandler(BotContext lagrange) : Endpoint<GetMiniAppArkHandler.Request, MilkyApiResponse<GetMiniAppArkHandler.Result>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_mini_app_ark"); }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.GetMiniAppArk(request.Type ?? string.Empty, request.Title ?? string.Empty, request.Description ?? string.Empty,
            request.PictureUrl ?? string.Empty, request.JumpUrl ?? string.Empty, ct);
        return new(new Result { JsonPayload = result });
    }
    public sealed class Request
    {
        [JsonPropertyName("type")] public string? Type { get; init; }
        [JsonPropertyName("title")] public string? Title { get; init; }
        [JsonPropertyName("description")] public string? Description { get; init; }
        [JsonPropertyName("picture_url")] public string? PictureUrl { get; init; }
        [JsonPropertyName("jump_url")] public string? JumpUrl { get; init; }
    }
    public sealed class Result
    {
        [JsonPropertyName("json_payload")] public string JsonPayload { get; init; } = string.Empty;
    }
}
