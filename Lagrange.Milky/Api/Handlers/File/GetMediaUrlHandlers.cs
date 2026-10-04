using FastEndpoints;
using System.Text.Json.Serialization;
using System;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.File;

internal static class MediaUrlHandlerCore
{
    public static async Task<MilkyApiResponse<T>> Resolve<T>(BotContext context, string resourceId, CancellationToken ct, Func<string, T> result) where T : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);
        string url = await context.GetNTV2RichMediaUrl(resourceId).WaitAsync(ct);
        return new(result(url));
    }
}

public sealed class GetGroupPttUrlHandler(BotContext context) : Endpoint<GetGroupPttUrlHandler.Request, MilkyApiResponse<GetGroupPttUrlHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_group_ptt_url");
    }
    public override Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct) => MediaUrlHandlerCore.Resolve(context, request.ResourceId, ct, url => new Result { Url = url });
    public sealed class Request(string resourceId) { [JsonPropertyName("resource_id")] public required string ResourceId { get; init; } = resourceId; }
    public sealed class Result { [JsonPropertyName("url")] public required string Url { get; init; } }
}

public sealed class GetGroupVideoUrlHandler(BotContext context) : Endpoint<GetGroupVideoUrlHandler.Request, MilkyApiResponse<GetGroupVideoUrlHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_group_video_url");
    }
    public override Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct) => MediaUrlHandlerCore.Resolve(context, request.ResourceId, ct, url => new Result { Url = url });
    public sealed class Request(string resourceId) { [JsonPropertyName("resource_id")] public required string ResourceId { get; init; } = resourceId; }
    public sealed class Result { [JsonPropertyName("url")] public required string Url { get; init; } }
}

public sealed class GetPrivatePttUrlHandler(BotContext context) : Endpoint<GetPrivatePttUrlHandler.Request, MilkyApiResponse<GetPrivatePttUrlHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_private_ptt_url");
    }
    public override Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct) => MediaUrlHandlerCore.Resolve(context, request.ResourceId, ct, url => new Result { Url = url });
    public sealed class Request(string resourceId) { [JsonPropertyName("resource_id")] public required string ResourceId { get; init; } = resourceId; }
    public sealed class Result { [JsonPropertyName("url")] public required string Url { get; init; } }
}

public sealed class GetPrivateVideoUrlHandler(BotContext context) : Endpoint<GetPrivateVideoUrlHandler.Request, MilkyApiResponse<GetPrivateVideoUrlHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_private_video_url");
    }
    public override Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct) => MediaUrlHandlerCore.Resolve(context, request.ResourceId, ct, url => new Result { Url = url });
    public sealed class Request(string resourceId) { [JsonPropertyName("resource_id")] public required string ResourceId { get; init; } = resourceId; }
    public sealed class Result { [JsonPropertyName("url")] public required string Url { get; init; } }
}
