using System.Text.Json.Serialization;
using System;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.File;

internal static class MediaUrlHandlerCore
{
    public static async ValueTask<MilkyApiResponse<T>> Resolve<T>(BotContext context, string resourceId, CancellationToken ct, Func<string, T> result) where T : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceId);
        var url = await context.GetNTV2RichMediaUrl(resourceId).WaitAsync(ct);
        return new(result(url));
    }
}

[ApiHandler("get_group_ptt_url")]
public sealed class GetGroupPttUrlHandler(BotContext context) : IApiHandler<GetGroupPttUrlHandler.Request, GetGroupPttUrlHandler.Result>
{
    public ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct) => MediaUrlHandlerCore.Resolve(context, request.ResourceId, ct, url => new Result { Url = url });
    public sealed class Request(string resourceId) { [JsonPropertyName("resource_id")] public required string ResourceId { get; init; } = resourceId; }
    public sealed class Result { [JsonPropertyName("url")] public required string Url { get; init; } }
}

[ApiHandler("get_group_video_url")]
public sealed class GetGroupVideoUrlHandler(BotContext context) : IApiHandler<GetGroupVideoUrlHandler.Request, GetGroupVideoUrlHandler.Result>
{
    public ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct) => MediaUrlHandlerCore.Resolve(context, request.ResourceId, ct, url => new Result { Url = url });
    public sealed class Request(string resourceId) { [JsonPropertyName("resource_id")] public required string ResourceId { get; init; } = resourceId; }
    public sealed class Result { [JsonPropertyName("url")] public required string Url { get; init; } }
}

[ApiHandler("get_private_ptt_url")]
public sealed class GetPrivatePttUrlHandler(BotContext context) : IApiHandler<GetPrivatePttUrlHandler.Request, GetPrivatePttUrlHandler.Result>
{
    public ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct) => MediaUrlHandlerCore.Resolve(context, request.ResourceId, ct, url => new Result { Url = url });
    public sealed class Request(string resourceId) { [JsonPropertyName("resource_id")] public required string ResourceId { get; init; } = resourceId; }
    public sealed class Result { [JsonPropertyName("url")] public required string Url { get; init; } }
}

[ApiHandler("get_private_video_url")]
public sealed class GetPrivateVideoUrlHandler(BotContext context) : IApiHandler<GetPrivateVideoUrlHandler.Request, GetPrivateVideoUrlHandler.Result>
{
    public ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct) => MediaUrlHandlerCore.Resolve(context, request.ResourceId, ct, url => new Result { Url = url });
    public sealed class Request(string resourceId) { [JsonPropertyName("resource_id")] public required string ResourceId { get; init; } = resourceId; }
    public sealed class Result { [JsonPropertyName("url")] public required string Url { get; init; } }
}
