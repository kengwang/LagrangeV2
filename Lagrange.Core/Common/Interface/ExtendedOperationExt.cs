using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service.Migration;
namespace Lagrange.Core.Common.Interface;

/// <summary>Additional QQ protocol operations exposed without a transport compatibility layer.</summary>
public static partial class ExtendedOperationExt
{
    /// <summary>Relays a group keyboard button interaction to its owning application.</summary>
    /// <param name="context">The authenticated bot session.</param>
    /// <param name="groupUin">The group containing the button message.</param>
    /// <param name="appId">The keyboard application's identifier.</param>
    /// <param name="sequence">The original group's message sequence.</param>
    /// <param name="buttonId">The button identifier from the keyboard payload.</param>
    /// <param name="callbackData">The unchanged callback payload supplied by the button.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <returns>The application's prompt text after a successful interaction.</returns>
    public static async Task<BotKeyboardClickResult> ClickInlineKeyboardButton(this BotContext context, long groupUin, ulong appId, ulong sequence, string buttonId, string callbackData = "", CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(groupUin);
        ArgumentOutOfRangeException.ThrowIfZero(appId);
        ArgumentOutOfRangeException.ThrowIfZero(sequence);
        ArgumentException.ThrowIfNullOrWhiteSpace(buttonId);
        var response = (await context.EventContext.SendEvent<ClickKeyboardEventResp>(new ClickKeyboardEventReq(new Oidb0x112eReq { GroupId = (ulong)groupUin, BotAppid = appId, MsgSeq = sequence, ButtonId = buttonId, CallbackData = callbackData, Unknown9 = 1 }), ct)).Body;
        if (response.Result != 0) throw new OperationException((int)response.Result, response.ErrMsg);
        return new(response.PromptText);
    }
    /// <summary>Requests the current account's QQ database key using its hexadecimal salt.</summary>
    /// <param name="context">The authenticated bot session.</param>
    /// <param name="dbSalt">The database salt as exactly 128 hexadecimal characters.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <returns>The server-provided database key; throws if none is returned.</returns>
    public static async Task<string> RequestDatabaseKey(this BotContext context, string dbSalt, CancellationToken ct = default)
    {
        if (dbSalt is null || dbSalt.Length != 128 || dbSalt.Any(x => !Uri.IsHexDigit(x))) throw new ArgumentException("Database salt must contain 128 hexadecimal characters.", nameof(dbSalt));
        var response = (await context.EventContext.SendEvent<RequestDatabaseKeyEventResp>(new RequestDatabaseKeyEventReq(new Oidb0xcdeReq { Info = new() { Db_salt = dbSalt } }), ct)).Body;
        return !string.IsNullOrEmpty(response.Info?.DbKey) ? response.Info.DbKey : throw new OperationException(-1, "Database key was not returned.");
    }
    /// <summary>Queries the user's online status; null means the server omitted the status property.</summary>
    /// <param name="context">The authenticated bot session.</param>
    /// <param name="userUin">The positive user identifier to query.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <returns>The decoded status, or null when unavailable.</returns>
    public static async Task<BotUserStatus?> GetUserStatus(this BotContext context, long userUin, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userUin);
        var response = (await context.EventContext.SendEvent<GetUserStatusEventResp>(new GetUserStatusEventReq(new OidbStrangerStatusReq { Uin = checked((uint)userUin), Key = [new() { Key = 27372 }] }), ct)).Body;
        var property = response.Data?.Properties?.Entries.FirstOrDefault(x => x.Key == 27372);
        return property is null ? null : DecodeUserStatus(property.Value);
    }
    internal static BotUserStatus DecodeUserStatus(ulong value) => value <= 10 ? new((uint)value * 10, 0) : new(10, (uint)((value & 0xff00) + ((value >> 16) & 0xff)));

    /// <summary>Builds an authenticated bilibili or Weibo mini-application share card.</summary>
    /// <param name="context">The authenticated bot session.</param>
    /// <param name="type">The supported application key: bili or weibo.</param>
    /// <param name="title">The card title.</param>
    /// <param name="description">The card description.</param>
    /// <param name="pictureUrl">The card's preview image URL.</param>
    /// <param name="jumpUrl">The destination URL opened by the card.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <returns>The authenticated JSON payload suitable for a light application message.</returns>
    public static async Task<string> GetMiniAppArk(this BotContext context, string type, string title, string description, string pictureUrl, string jumpUrl, CancellationToken ct = default)
    {
        var (appid, icon) = type switch
        {
            "bili" => ("1109937557", "51f90239b78a2e4994c11215f4c4ba15"),
            "weibo" => ("1109224783", "35bbb44dc68e65194cfacfb206b8f1f7"),
            _ => throw new ArgumentException("Supported mini applications: bili, weibo.", nameof(type))
        };
        var response = (await context.EventContext.SendEvent<MiniAppShareEventResp>(new MiniAppShareEventReq(new MiniAppShareReq
        {
            SdkVersion = "V1_PC_MINISDK_99.99.99_1_APP_A",
            Body = new() { Appid = appid, Title = title, Desc = description, PicUrl = pictureUrl, JumpUrl = jumpUrl, IconUrl = $"http://miniapp.gtimg.cn/public/appicon/{icon}_200.jpg" }
        }), ct)).Body;
        if (response.Status != 0) throw new OperationException((int)response.Status, response.Msg);
        var source = JsonNode.Parse(response.Body?.JsonStr ?? throw new OperationException(-1, "Mini application card was not returned."))!;
        return new JsonObject { ["ver"] = source["ver"]?.DeepClone(), ["prompt"] = source["prompt"]?.DeepClone(), ["config"] = source["config"]?.DeepClone(), ["app"] = source["appName"]?.DeepClone(), ["view"] = source["appView"]?.DeepClone(), ["meta"] = source["metaData"]?.DeepClone(), ["miniappShareOrigin"] = 3, ["miniappOpenRefer"] = "10002" }.ToJsonString();
    }
    /// <summary>Lists active group todos using the current banner protocol.</summary>
    /// <param name="context">The authenticated bot session.</param>
    /// <param name="groupUin">The group to query.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <returns>Active todos with validated original message identities.</returns>
    public static async Task<IReadOnlyList<BotGroupTodoItem>> GetGroupTodoList(this BotContext context, long groupUin, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(groupUin);
        var response = (await context.EventContext.SendEvent<GetGroupTodoListEventResp>(new GetGroupTodoListEventReq(new OidbQueryGroupTopBannersReq { GroupId = (ulong)groupUin, BannerFlag = 1 }), ct)).Body;
        return response.Banners.Where(x => !x.IsDisappear).Select(ParseTodo).ToArray();
    }
    internal static BotGroupTodoItem ParseTodo(OidbOnlineBanner banner)
    {
        var parts = Encoding.UTF8.GetString(banner.MsgId).Split('_');
        if (parts.Length != 2 || !ulong.TryParse(parts[0], out var sequence) || sequence == 0 || !long.TryParse(parts[1], out var random)) throw new OperationException(-1, "Invalid todo message identity.");
        if (banner.CommonBanner?.JumpInfo?.JumpParam is { Length: > 0 } jump)
        {
            using var json = JsonDocument.Parse(jump);
            var root = json.RootElement;
            if (!root.TryGetProperty("seq", out var seq) || seq.GetUInt64() != sequence || !root.TryGetProperty("random", out var rnd) || rnd.GetInt64() != random) throw new OperationException(-1, "Todo sequence mismatch.");
        }
        return new(sequence, random, banner.CommonBanner?.Ui?.Text ?? banner.TodoBanner?.Text ?? string.Empty, banner.CommonBanner?.CreateTime ?? 0, banner.CommonBanner?.UpdateTime ?? 0);
    }
    /// <summary>Completes the todo belonging to an exact group message.</summary>
    /// <param name="context">The authenticated bot session.</param>
    /// <param name="groupUin">The group containing the todo.</param>
    /// <param name="sequence">The todo's authoritative source message sequence.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <returns>A task that completes after the server acknowledges the operation.</returns>
    public static async Task FinishGroupTodo(this BotContext context, long groupUin, ulong sequence, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(groupUin); ArgumentOutOfRangeException.ThrowIfZero(sequence);
        await context.EventContext.SendEvent<GroupFinishTodoEventResp>(new GroupFinishTodoEventReq(groupUin, sequence), ct);
    }
    /// <summary>Cancels the todo belonging to an exact group message.</summary>
    /// <param name="context">The authenticated bot session.</param>
    /// <param name="groupUin">The group containing the todo.</param>
    /// <param name="sequence">The todo's authoritative source message sequence.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <returns>A task that completes after the server acknowledges the operation.</returns>
    public static async Task RemoveGroupTodo(this BotContext context, long groupUin, ulong sequence, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(groupUin); ArgumentOutOfRangeException.ThrowIfZero(sequence);
        await context.EventContext.SendEvent<GroupRemoveTodoEventResp>(new GroupRemoveTodoEventReq(groupUin, sequence), ct);
    }
}
