using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class SendGroupMessageReactionHandler(BotContext lagrange) : Endpoint<SendGroupMessageReactionHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/send_group_message_reaction");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await _lagrange.SetGroupReaction(
            request.GroupId,
            (ulong)request.MessageSeq,
            request.Reaction,
            request.IsAdd,
            ct
        );
        return new MilkyApiResponse();
    }

    public sealed class Request(long groupId, long messageSeq, string reaction, string reactionType = "face", bool isAdd = true)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("message_seq")] public required long MessageSeq { get; init; } = messageSeq;
        [JsonPropertyName("reaction")] public required string Reaction { get; init; } = reaction;
        [JsonPropertyName("reaction_type")] public string ReactionType { get; init; } = reactionType;
        [JsonPropertyName("is_add")] public bool IsAdd { get; init; } = isAdd;
    }
}
