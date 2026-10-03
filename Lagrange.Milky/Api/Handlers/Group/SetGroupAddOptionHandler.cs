using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Group;

[ApiHandler("set_group_add_option")]
public sealed class SetGroupAddOptionHandler(BotContext lagrange) : INoResultApiHandler<SetGroupAddOptionHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        if (request.GroupId <= 0) throw new ArgumentOutOfRangeException(nameof(request.GroupId));
        await lagrange.SetGroupAddOption(request.GroupId, request.AddType, request.GroupQuestion, request.GroupAnswer, ct).WaitAsync(ct);
        return new MilkyApiResponse();
    }
    public sealed class Request
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; }
        [JsonPropertyName("add_type")] public uint AddType { get; init; }
        [JsonPropertyName("group_question")] public string? GroupQuestion { get; init; }
        [JsonPropertyName("group_answer")] public string? GroupAnswer { get; init; }
    }
}
