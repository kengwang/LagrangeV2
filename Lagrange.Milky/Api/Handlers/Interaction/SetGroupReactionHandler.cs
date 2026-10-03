using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Interaction;

[ApiHandler("set_group_reaction")]
public sealed class SetGroupReactionHandler(BotContext lagrange) : INoResultApiHandler<SetGroupReactionHandler.Request>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await _lagrange.SetGroupReaction(request.GroupId, checked((ulong)request.MessageSeq), request.Code, request.IsSet, ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(long groupId, long messageSeq, string code, bool isSet = true)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("message_seq")] public required long MessageSeq { get; init; } = messageSeq;
        [JsonPropertyName("code")] public required string Code { get; init; } = code;
        [JsonPropertyName("is_set")] public bool IsSet { get; init; } = isSet;
    }
}
