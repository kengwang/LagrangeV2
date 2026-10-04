using FastEndpoints;
using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.File;

public sealed class GetGroupFileUrlHandler(BotContext lagrange) : Endpoint<GetGroupFileUrlHandler.Request, MilkyApiResponse<GetGroupFileUrlHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_group_file_url");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FileId);
        string url = await _lagrange.GroupFSDownload(request.GroupId, request.FileId, ct);
        return new MilkyApiResponse<Result>(new Result { Url = url });
    }

    public sealed class Request(long groupId, string fileId)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("file_id")] public required string FileId { get; init; } = fileId;
    }

    public sealed class Result
    {
        [JsonPropertyName("url")] public required string Url { get; init; }
    }
}
