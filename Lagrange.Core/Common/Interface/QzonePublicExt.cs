using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Internal.Events.System;

namespace Lagrange.Core.Common.Interface;

public static class QzonePublicExt
{
    private const string Reader = "trpc.qzone.feeds_reader.FeedsReader";
    private const string Interaction = "trpc.qzone.interaction_writer.InteractionWriter";

    private static async Task<JsonElement> Rpc(BotContext context, string service, string method, string command, string subCommand, JsonObject parameters, CancellationToken ct) =>
        (await context.EventContext.SendEvent<QzoneRpcResult>(new QzoneRpcEvent(service, method, command, subCommand, parameters), ct)).Data;

    internal static JsonObject FeedParameters(JsonElement feed)
    {
        var union = feed.GetProperty("cell_common").GetProperty("union_id");
        if (union.ValueKind != JsonValueKind.Object || !union.EnumerateObject().Any()) throw new ArgumentException("The complete Qzone feed with cell_common.union_id is required.", nameof(feed));
        var result = new JsonObject { ["union_id"] = JsonNode.Parse(union.GetRawText()) };
        if (feed.TryGetProperty("cell_feed_transfer", out var transfer)) result["cell_feed_transfer"] = JsonNode.Parse(transfer.GetRawText());
        return result;
    }

    private static JsonNode Copy(JsonElement value) => value.ValueKind == JsonValueKind.Object
        ? JsonNode.Parse(value.GetRawText())! : throw new ArgumentException("A complete Qzone object is required.");

    private static string RequiredText(JsonElement value)
    {
        var text = value.GetString();
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        return text;
    }

    private static JsonElement Confirmed(JsonElement data, string field)
    {
        if (data.GetProperty(field).ValueKind is JsonValueKind.Null or JsonValueKind.Undefined) throw new InvalidDataException($"Qzone response has no {field} confirmation.");
        return data;
    }

    internal static string EncodeCursor(JsonObject state)
    {
        var cursor = Convert.ToBase64String(Encoding.UTF8.GetBytes(state.ToJsonString())).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        if (cursor.Length > 65536) throw new InvalidDataException("Qzone cursor is too large.");
        return cursor;
    }

    internal static JsonObject DecodeCursor(string cursor, long selfUin, string kind)
    {
        if (cursor.Length is 0 or > 65536 || cursor.Any(x => !char.IsAsciiLetterOrDigit(x) && x is not ('-' or '_'))) throw new ArgumentException("Invalid Qzone cursor.", nameof(cursor));
        string base64 = cursor.Replace('-', '+').Replace('_', '/');
        var state = JsonNode.Parse(Convert.FromBase64String(base64.PadRight((base64.Length + 3) / 4 * 4, '=')))?.AsObject()
            ?? throw new ArgumentException("Invalid Qzone cursor.", nameof(cursor));
        if (state["version"]?.GetValue<int>() != 1 || state["uin"]?.GetValue<string>() != selfUin.ToString(CultureInfo.InvariantCulture) || state["kind"]?.GetValue<string>() != kind || EncodeCursor(state) != cursor)
            throw new ArgumentException("Qzone cursor is invalid or belongs to another account or operation.", nameof(cursor));
        return state;
    }

    private static JsonObject InitialCursor(long selfUin, string kind) => new() { ["version"] = 1, ["kind"] = kind, ["uin"] = selfUin.ToString(CultureInfo.InvariantCulture), ["attach_info"] = "" };

    public static async Task<JsonElement> GetQzonePublicCapabilities(this BotContext context, CancellationToken ct = default)
    {
        var self = context.BotUin.ToString(CultureInfo.InvariantCulture);
        var data = await Rpc(context, "trpc.public_qzone.entrance_ctrl.EntranceCtrl", "BatchEntranceWhitelist", "0x9741", "0",
            new JsonObject { ["uins"] = new JsonArray(JsonValue.Create(self)), ["entrance_types"] = new JsonArray(JsonValue.Create(1), JsonValue.Create(15)) }, ct);
        var matches = data.GetProperty("results").EnumerateArray().Where(x => x.GetProperty("uin").ToString() == self).ToArray();
        if (matches.Length != 1 || matches[0].GetProperty("entrances").ValueKind != JsonValueKind.Object) throw new InvalidDataException("Qzone capability response has no matching account.");
        return matches[0].GetProperty("entrances").Clone();
    }

    public static async Task<BotQzonePublicFeedsPage> GetQzoneHomeFeeds(this BotContext context, long? targetUin = null, string? cursor = null, CancellationToken ct = default)
    {
        var host = targetUin ?? context.BotUin;
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(host);
        var state = cursor is null ? InitialCursor(context.BotUin, "home") : DecodeCursor(cursor, context.BotUin, "home");
        if (cursor is not null && state["host_uin"]?.GetValue<string>() != host.ToString(CultureInfo.InvariantCulture)) throw new ArgumentException("Qzone cursor belongs to another homepage.");
        var attach = state["attach_info"]!.GetValue<string>();
        var data = await Rpc(context, Reader, "GetMainPageFeeds", "0x9794", "2", new JsonObject
        { ["host_uid"] = "", ["host_uin"] = host.ToString(CultureInfo.InvariantCulture), ["attach_info"] = attach, ["refresh_attach_info"] = "", ["feed_action_domain_type"] = 1 }, ct);
        state["host_uin"] = host.ToString(CultureInfo.InvariantCulture);
        return ReadPage(data, state, attach, true);
    }

    public static async Task<BotQzonePublicFeedsPage> GetQzonePublicFeeds(this BotContext context, string? cursor = null, CancellationToken ct = default)
    {
        var state = cursor is null ? InitialCursor(context.BotUin, "public") : DecodeCursor(cursor, context.BotUin, "public");
        if (cursor is null) state["refresh_id"] = $"{context.BotUin}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var refresh = state["refresh_id"]?.GetValue<string>();
        ArgumentException.ThrowIfNullOrWhiteSpace(refresh);
        var parameters = new JsonObject { ["refresh_id"] = refresh };
        var attach = state["attach_info"]!.GetValue<string>();
        if (cursor is not null)
        {
            parameters["attach_info"] = attach;
            if (state["last_client_feed_union_id"] is { } last) parameters["last_client_feed_union_id"] = last.DeepClone();
        }
        var data = await Rpc(context, Reader, "GetPublicRecomFeeds", "0x9a38", "0", parameters, ct);
        return ReadPage(data, state, attach, false);
    }

    internal static BotQzonePublicFeedsPage ReadPage(JsonElement data, JsonObject state, string previousAttach, bool home)
    {
        var feeds = data.GetProperty("feeds").EnumerateArray().Select(x => { var feed = x.GetProperty("feed"); if (feed.ValueKind != JsonValueKind.Object) throw new InvalidDataException("Qzone feed is not an object."); return feed.Clone(); }).ToArray();
        bool hasMore = data.GetProperty("has_more").GetBoolean();
        if (!home)
            foreach (var feed in feeds)
                if (feed.TryGetProperty("cell_common", out var common) && common.ValueKind == JsonValueKind.Object &&
                    (!common.TryGetProperty("feed_type", out var type) || type.GetInt32() != 3) && common.TryGetProperty("union_id", out var union) && union.ValueKind == JsonValueKind.Object)
                    state["last_client_feed_union_id"] = JsonNode.Parse(union.GetRawText());
        string? next = null;
        if (hasMore)
        {
            var attach = RequiredText(data.GetProperty("attach_info"));
            if (attach == previousAttach) throw new InvalidDataException("Qzone pagination did not advance.");
            state["attach_info"] = attach;
            next = EncodeCursor(state);
        }
        return new(feeds, hasMore, next);
    }

    public static Task<JsonElement> GetQzonePublicFeedDetail(this BotContext context, JsonElement feed, CancellationToken ct = default)
    { var p = FeedParameters(feed); p["feed_action_domain_type"] = 3; p["from_page"] = 1; return Rpc(context, Reader, "GetFeedDetail", "0x97b1", "1", p, ct); }

    public static Task<JsonElement> GetQzonePublicComments(this BotContext context, JsonElement feed, string cursor = "", CancellationToken ct = default)
    { var p = FeedParameters(feed); p["attach_info"] = cursor; return Rpc(context, Reader, "GetComment", "0x9bfd", "1", p, ct); }

    public static Task<JsonElement> GetQzonePublicReplies(this BotContext context, JsonElement feed, JsonElement comment, string cursor = "", int pageSize = 20, CancellationToken ct = default)
    {
        if (pageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(pageSize));
        var p = FeedParameters(feed); p["cur_comment"] = Copy(comment); p["comment_attach_info"] = "";
        p["reply_attach_info"] = cursor.Length > 0 ? cursor : comment.TryGetProperty("reply_attach_info", out var attach) ? attach.GetString() ?? "" : "";
        p["page_size"] = pageSize;
        return Rpc(context, Reader, "GetReplies", "0x9bfe", "1", p, ct);
    }

    public static async Task<JsonElement> SetQzonePublicUpvote(this BotContext context, JsonElement feed, bool enable, CancellationToken ct = default)
    {
        var data = await Rpc(context, "trpc.public_qzone.interaction.UFOComment", enable ? "Upvote" : "Downvote", enable ? "0x9d18" : "0x9d19", "1", FeedParameters(feed), ct);
        return Confirmed(data, "upvote");
    }

    private static async Task<JsonElement> SetLike(BotContext context, JsonElement feed, string likeKey, int type, bool enable, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(likeKey);
        var p = FeedParameters(feed); p["like_key"] = likeKey; p["like_type"] = type;
        var data = await Rpc(context, Interaction, enable ? "DoLike" : "UnLike", enable ? "0x9699" : "0x969c", "1", p, ct);
        return Confirmed(data, "like");
    }

    public static Task<JsonElement> SetQzonePublicLike(this BotContext context, JsonElement feed, bool enable, CancellationToken ct = default) =>
        SetLike(context, feed, RequiredText(feed.GetProperty("cell_common").GetProperty("cur_like_key")), 1, enable, ct);

    public static Task<JsonElement> SetQzonePublicCommentLike(this BotContext context, JsonElement feed, JsonElement comment, bool enable, JsonElement? reply = null, CancellationToken ct = default) =>
        SetLike(context, feed, RequiredText((reply ?? comment).GetProperty("like").GetProperty("id")), reply is null ? 2 : 3, enable, ct);

    private static JsonNode Content(JsonElement content)
    {
        if (content.ValueKind == JsonValueKind.String)
        {
            var text = RequiredText(content);
            return new JsonArray(new JsonObject { ["type"] = 0, ["content"] = text });
        }
        if (content.ValueKind != JsonValueKind.Array || content.GetArrayLength() == 0 || content.EnumerateArray().Any(x => x.ValueKind != JsonValueKind.Object))
            throw new ArgumentException("Qzone content must be nonempty text or an array of content objects.");
        return JsonNode.Parse(content.GetRawText())!;
    }

    private static JsonObject Self(BotContext context) => new() { ["uin"] = context.BotUin.ToString(CultureInfo.InvariantCulture), ["uid"] = context.Keystore.Uid, ["nick"] = context.BotInfo?.Name ?? string.Empty };

    public static async Task<JsonElement> CommentQzonePublicFeed(this BotContext context, JsonElement feed, JsonElement content, CancellationToken ct = default)
    {
        var p = FeedParameters(feed); p["comment"] = new JsonObject { ["user"] = Self(context), ["content"] = Content(content) };
        var data = await Rpc(context, Interaction, "AddComment", "0x9676", "1", p, ct); return Confirmed(data, "comment");
    }

    public static async Task<JsonElement> ReplyQzonePublicFeed(this BotContext context, JsonElement feed, JsonElement comment, JsonElement content, JsonElement? reply = null, CancellationToken ct = default)
    {
        var p = FeedParameters(feed); p["comment"] = Copy(comment);
        p["reply"] = new JsonObject { ["content"] = Content(content), ["target_user"] = Copy((reply ?? comment).GetProperty("user")), ["user"] = Self(context) };
        var data = await Rpc(context, Interaction, "AddReply", "0x9677", "1", p, ct); return Confirmed(data, "reply");
    }

    public static Task<JsonElement> DeleteQzonePublicComment(this BotContext context, JsonElement feed, JsonElement comment, CancellationToken ct = default)
    { var p = FeedParameters(feed); p["comment"] = Copy(comment); return Rpc(context, Interaction, "DeleteComment", "0x9989", "0", p, ct); }

    public static Task<JsonElement> DeleteQzonePublicReply(this BotContext context, JsonElement feed, JsonElement comment, JsonElement reply, CancellationToken ct = default)
    { var p = FeedParameters(feed); p["comment"] = Copy(comment); p["reply"] = Copy(reply); return Rpc(context, Interaction, "DeleteReply", "0x998a", "0", p, ct); }

    public static Task<JsonElement> ForwardQzonePublicFeed(this BotContext context, JsonElement feed, string reason = "", CancellationToken ct = default)
    { var p = FeedParameters(feed); p["reason"] = reason; return Rpc(context, "trpc.qzone.feeds_writer.FeedsWriter", "ForwardFeed", "0x969a", "1", p, ct); }

    public static async Task<JsonElement> SetQzonePublicFollow(this BotContext context, JsonElement feed, bool enable, CancellationToken ct = default)
    {
        var p = FeedParameters(feed);
        var user = feed.GetProperty("cell_user_info").GetProperty("user");
        string uid = user.TryGetProperty("uid", out var id) ? id.GetString() ?? "" : "";
        string uin = user.TryGetProperty("uin", out var account) ? account.ToString() : "";
        bool useUid = uin.Length == 0 || feed.GetProperty("cell_common").TryGetProperty("domain_type", out var domain) && domain.GetInt32() == 3 && uid.Length > 0;
        if (useUid) ArgumentException.ThrowIfNullOrWhiteSpace(uid);
        else if (!long.TryParse(uin, out var number) || number <= 0) throw new ArgumentException("Qzone feed author has no valid UIN.");
        var parameters = new JsonObject { ["follow_from_type"] = 1, [useUid ? "target_uid" : "target_uin"] = useUid ? uid : uin };
        if (p["union_id"]?["uf_key"] is { } key) parameters["follow_feed_id"] = key.DeepClone();
        if (p["cell_feed_transfer"] is { } transfer) parameters["cell_feed_transfer"] = transfer.DeepClone();
        var data = await Rpc(context, "trpc.public_qzone.relation.RelationService", enable ? "Follow" : "Unfollow", enable ? "0x9d1a" : "0x9d1b", "1", parameters, ct);
        if (data.TryGetProperty("ret_code", out var code) && code.ValueKind != JsonValueKind.Null && code.ToString() != "0") throw new InvalidDataException($"Qzone follow failed: {code}.");
        return data;
    }

    public static async Task<BotQzoneShareArk> GetQzoneShareArk(this BotContext context, JsonElement feed, CancellationToken ct = default)
    {
        var common = feed.GetProperty("cell_common");
        var share = feed.GetProperty("cell_operation").GetProperty("share_outside");
        if (common.TryGetProperty("operate_mask", out var mask) && (mask.GetUInt64() & (1UL << 17)) != 0)
            throw new InvalidOperationException(share.TryGetProperty("share_out_button_toast", out var toast) ? toast.ToString() : "Qzone feed cannot be shared.");
        var resource = share.GetProperty("share_access_resource");
        var p = new JsonObject { ["info"] = new JsonObject { ["busi_id"] = RequiredText(resource.GetProperty("busi_id")), ["resource_id"] = RequiredText(resource.GetProperty("resource_id")),
            ["template"] = "ark_busi_select", ["busi_data"] = RequiredText(feed.GetProperty("cell_feed_transfer")), ["plat"] = 1, ["target"] = 3 } };
        var data = await Rpc(context, "trpc.qq.share.Service", "CreateShare", "0x9409", "1", p, ct);
        if (data.TryGetProperty("err", out var error) && error.TryGetProperty("code", out var code) && code.GetInt32() != 0)
            throw new InvalidDataException($"Qzone sharing failed: {error}.");
        var ark = RequiredText(data.GetProperty("template_data"));
        using (var document = JsonDocument.Parse(ark)) Copy(document.RootElement);
        bool confirmation = false;
        string? notice = null;
        if (data.TryGetProperty("info", out var info) && info.TryGetProperty("busi_data", out var business) && business.GetString() is { Length: > 0 } encoded)
        {
            var bytes = Convert.FromBase64String(encoded.PadRight((encoded.Length + 3) / 4 * 4, '='));
            if (Convert.ToBase64String(bytes).TrimEnd('=') != encoded.TrimEnd('=')) throw new InvalidDataException("Invalid Qzone sharing notice encoding.");
            using var permission = JsonDocument.Parse(bytes);
            if (permission.RootElement.ValueKind != JsonValueKind.Null)
            {
                confirmation = permission.RootElement.TryGetProperty("need_break", out var need) && need.GetBoolean();
                if (permission.RootElement.TryGetProperty("tips", out var tips) && tips.TryGetProperty("content", out var text)) notice = text.GetString();
                if (confirmation && string.IsNullOrEmpty(notice)) notice = "你的空间有权限，分享出去的页面所有人都可以看。";
            }
        }
        return new(ark, confirmation, notice);
    }
}
