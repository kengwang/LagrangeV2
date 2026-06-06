using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.System;

[Api("get_custom_face_url_list")]
public class GetCustomFaceUrlListHandler(BotContext bot) : IEmptyParameterApiHandler<GetCustomFaceUrlListResult>
{
    private readonly BotContext _bot = bot;

    public async Task<GetCustomFaceUrlListResult> HandleAsync(CancellationToken token)
    {
        return new GetCustomFaceUrlListResult(await _bot.FetchCustomFace());
    }
}

public class GetCustomFaceUrlListResult(IEnumerable<string> urls)
{
    [JsonPropertyName("urls")]
    public IEnumerable<string> Urls { get; } = urls;
}
