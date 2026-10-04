using FastEndpoints;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class TranslateEnToZhHandler(BotContext lagrange) : Endpoint<TranslateEnToZhHandler.Request, MilkyApiResponse<TranslateEnToZhHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/translate_en_to_zh");
    }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        if (request.Words is null || request.Words.Count == 0) throw new ArgumentException("words is required.");
        var words = await lagrange.TranslateEnToZh(request.Words, ct).WaitAsync(ct);
        return new(new Result { Words = words });
    }
    public sealed class Request { [JsonPropertyName("words")] public required IReadOnlyList<string> Words { get; init; } }
    public sealed class Result { [JsonPropertyName("words")] public required IReadOnlyList<string> Words { get; init; } }
}
