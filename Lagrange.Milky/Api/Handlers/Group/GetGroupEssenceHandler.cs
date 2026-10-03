using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Group;

[ApiHandler("get_group_essence")]
public sealed class GetGroupEssenceHandler(BotContext lagrange) : IApiHandler<GetGroupEssenceHandler.Request, GetGroupEssenceHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.GetGroupEssence(request.GroupId, request.PageStart, request.PageLimit, ct).WaitAsync(ct);
        return new(new Result(result));
    }
    public sealed class Request(long groupId, int pageStart = 0, int pageLimit = 50)
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; } = groupId;
        [JsonPropertyName("page_start")] public int PageStart { get; init; } = pageStart;
        [JsonPropertyName("page_limit")] public int PageLimit { get; init; } = pageLimit;
    }
    public sealed class Result(Lagrange.Core.Common.Response.BotGroupEssenceResult result)
    {
        [JsonPropertyName("messages")] public Message[] Messages { get; } = result.Messages.Select(x => new Message(x)).ToArray();
        [JsonPropertyName("is_end")] public bool IsEnd { get; } = result.IsEnd;
        [JsonPropertyName("group_role")] public int GroupRole { get; } = result.GroupRole;
    }
    public sealed class Message(Lagrange.Core.Common.Response.BotGroupEssenceMessage message)
    {
        [JsonPropertyName("group_code")] public string GroupCode { get; } = message.GroupCode;
        [JsonPropertyName("message_sequence")] public ulong MessageSequence { get; } = message.MessageSequence;
        [JsonPropertyName("message_random")] public uint MessageRandom { get; } = message.MessageRandom;
        [JsonPropertyName("sender_uin")] public string SenderUin { get; } = message.SenderUin;
        [JsonPropertyName("sender_nick")] public string SenderNick { get; } = message.SenderNick;
        [JsonPropertyName("sender_time")] public long SenderTime { get; } = message.SenderTime;
        [JsonPropertyName("add_digest_uin")] public string AddDigestUin { get; } = message.AddDigestUin;
        [JsonPropertyName("add_digest_nick")] public string AddDigestNick { get; } = message.AddDigestNick;
        [JsonPropertyName("add_digest_time")] public long AddDigestTime { get; } = message.AddDigestTime;
        [JsonPropertyName("can_be_removed")] public bool CanBeRemoved { get; } = message.CanBeRemoved;
        [JsonPropertyName("text")] public string? Text { get; } = message.Text;
    }
}
