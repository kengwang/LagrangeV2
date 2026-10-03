using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("translate_en_to_zh")]
public sealed class TranslateEnToZhHandler(BotContext lagrange) : IApiHandler<TranslateEnToZhHandler.Request, TranslateEnToZhHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        if (request.Words is null || request.Words.Count == 0) throw new ArgumentException("words is required.");
        var words = await lagrange.TranslateEnToZh(request.Words, ct).WaitAsync(ct);
        return new(new Result { Words = words });
    }
    public sealed class Request { [JsonPropertyName("words")] public required IReadOnlyList<string> Words { get; init; } }
    public sealed class Result { [JsonPropertyName("words")] public required IReadOnlyList<string> Words { get; init; } }
}
