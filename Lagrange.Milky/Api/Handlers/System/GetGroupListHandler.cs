using FastEndpoints;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Converters;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class GetGroupListHandler(BotContext lagrange, MilkyConverter converter) : Endpoint<GetGroupListHandler.Request, MilkyApiResponse<GetGroupListHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_group_list");
    }
    private readonly BotContext _lagrange = lagrange;
    private readonly MilkyConverter _converter = converter;

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var groups = await _lagrange.FetchGroups(request.NoCache).WaitAsync(ct);
        return new MilkyApiResponse<Result>(new Result
        {
            Groups = [.. groups.Select(_converter.ToGroup)]
        });
    }

    public sealed class Request(bool noCache = false)
    {
        [JsonPropertyName("no_cache")] public bool NoCache { get; init; } = noCache;
    }

    public sealed class Result
    {
        [JsonPropertyName("groups")] public required IReadOnlyList<Lagrange.Milky.Models.Group> Groups { get; init; }
    }
}
