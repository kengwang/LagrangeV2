using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api;
using Lagrange.Milky.Api.Attributes;
using Lagrange.Milky.Converters;
using Lagrange.Milky.Models;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("get_group_system_msg")]
public sealed class GetGroupSystemMessageHandler(BotContext lagrange, MilkyConverter converter) : IApiHandler<GetGroupSystemMessageHandler.Request, GetGroupSystemMessageHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        if (request.GroupId <= 0) throw new ArgumentOutOfRangeException(nameof(request.GroupId));
        var notifications = await lagrange.FetchGroupNotifications(100, 0, ct);
        return new(new Result { Messages = [.. notifications.Where(x => x.GroupUin == request.GroupId).Select(converter.ToGroupNotification)] });
    }

    public sealed class Request
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; }
        [JsonPropertyName("message_type")] public string? MessageType { get; init; }
    }

    public sealed class Result
    {
        [JsonPropertyName("messages")] public required IReadOnlyList<GroupNotificationBase> Messages { get; init; }
    }
}

[ApiHandler("get_group_shut_list")]
public sealed class GetGroupShutListHandler(BotContext lagrange) : IApiHandler<GetGroupShutListHandler.Request, GetGroupShutListHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        var members = await lagrange.FetchMembers(request.GroupId, true);
        return new(new Result { Members = [.. members.Where(x => x.ShutUpTimestamp > DateTimeOffset.UtcNow.ToUnixTimeSeconds()).Select(x => new Member { UserId = x.Uin, ShutUpTimestamp = x.ShutUpTimestamp })] });
    }

    public sealed class Request
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; }
    }

    public sealed class Result
    {
        [JsonPropertyName("members")] public required IReadOnlyList<Member> Members { get; init; }
    }
    public sealed class Member { [JsonPropertyName("user_id")] public long UserId { get; init; } [JsonPropertyName("shut_up_timestamp")] public long ShutUpTimestamp { get; init; } }
}

[ApiHandler("get_doubt_friends_add_request")]
public sealed class GetDoubtFriendRequestsHandler(BotContext lagrange) : INoRequestApiHandler<GetDoubtFriendRequestsHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(CancellationToken ct)
    {
        var requests = await lagrange.FetchDoubtFriendRequests(ct);
        return new(new Result { Requests = [.. requests.Select(x => new Item { Uid = x.Uid, UserId = x.UserId, Nick = x.Nick, Source = x.Source, Reason = x.Reason, Message = x.Message, GroupCode = x.GroupCode, RequestTime = x.RequestTime })] });
    }

    public sealed class Result
    {
        [JsonPropertyName("requests")] public required IReadOnlyList<Item> Requests { get; init; }
    }
    public sealed class Item { [JsonPropertyName("uid")] public required string Uid { get; init; } [JsonPropertyName("user_id")] public long UserId { get; init; } [JsonPropertyName("nick")] public string Nick { get; init; } = string.Empty; [JsonPropertyName("source")] public string Source { get; init; } = string.Empty; [JsonPropertyName("reason")] public string Reason { get; init; } = string.Empty; [JsonPropertyName("message")] public string Message { get; init; } = string.Empty; [JsonPropertyName("group_code")] public string GroupCode { get; init; } = string.Empty; [JsonPropertyName("request_time")] public long RequestTime { get; init; } }
}

[ApiHandler("set_doubt_friends_add_request")]
public sealed class SetDoubtFriendRequestHandler(BotContext lagrange) : INoResultApiHandler<SetDoubtFriendRequestHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RequestId)) throw new ArgumentException("request_id is required.", nameof(request.RequestId));
        await lagrange.HandleDoubtFriendRequest(request.RequestId, request.Approve, ct);
        return new();
    }

    public sealed class Request
    {
        [JsonPropertyName("request_id")] public required string RequestId { get; init; }
        [JsonPropertyName("approve")] public bool Approve { get; init; }
    }
}

[ApiHandler("get_qidian_corp_info")]
public sealed class GetQidianCorpInfoHandler(BotContext lagrange) : IApiHandler<GetQidianCorpInfoHandler.Request, GetQidianCorpInfoHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        if (request.UserId <= 0) throw new ArgumentOutOfRangeException(nameof(request.UserId));
        var info = await lagrange.FetchQidianCorp(request.UserId, ct);
        return new(new Result { Info = new Corp { Name = info.Name, Intro = info.Intro, Website = info.Website, Slogan = info.Slogan, Address = info.Address, Phone = info.Phone, Email = info.Email } });
    }

    public sealed class Request
    {
        [JsonPropertyName("user_id")] public long UserId { get; init; }
    }

    public sealed class Result
    {
        [JsonPropertyName("info")] public required Corp Info { get; init; }
    }
    public sealed class Corp { [JsonPropertyName("name")] public string Name { get; init; } = string.Empty; [JsonPropertyName("intro")] public string Intro { get; init; } = string.Empty; [JsonPropertyName("website")] public string Website { get; init; } = string.Empty; [JsonPropertyName("slogan")] public string Slogan { get; init; } = string.Empty; [JsonPropertyName("address")] public string Address { get; init; } = string.Empty; [JsonPropertyName("phone")] public string Phone { get; init; } = string.Empty; [JsonPropertyName("email")] public string Email { get; init; } = string.Empty; }
}

[ApiHandler("send_ark_share")]
public sealed class SendArkShareHandler(BotContext lagrange) : IApiHandler<SendArkShareHandler.Request, SendArkShareHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        if (request.UserId <= 0 && (!request.GroupId.HasValue || request.GroupId <= 0)) throw new ArgumentException("A valid user_id or group_id is required.");
        var ark = request.GroupId is > 0 ? await lagrange.FetchGroupArk(request.GroupId.Value, ct) : await lagrange.FetchBuddyArk(request.UserId, ct);
        return new(new Result { Ark = ark });
    }

    public sealed class Request
    {
        [JsonPropertyName("user_id")] public long UserId { get; init; }
        [JsonPropertyName("group_id")] public long? GroupId { get; init; }
    }

    public sealed class Result
    {
        [JsonPropertyName("ark")] public string? Ark { get; init; }
    }
}

[ApiHandler("send_tuwen_ark")]
public sealed class SendTuwenArkHandler(BotContext lagrange) : IApiHandler<SendTuwenArkHandler.Request, SendTuwenArkHandler.Result>
{
    public async ValueTask<MilkyApiResponse<Result>> HandleAsync(Request request, CancellationToken ct)
    {
        if (request.PeerId <= 0) throw new ArgumentOutOfRangeException(nameof(request.PeerId));
        await lagrange.SendTuwenArk(request.PeerId, request.GroupId.HasValue, request.Title, request.Description, request.Summary, request.Url, request.PreviewUrl, ct);
        return new(new Result { MessageId = 0 });
    }

    public sealed class Request
    {
        [JsonPropertyName("peer_id")] public long PeerId { get; init; }
        [JsonPropertyName("group_id")] public long? GroupId { get; init; }
        [JsonPropertyName("title")] public string Title { get; init; } = string.Empty;
        [JsonPropertyName("description")] public string Description { get; init; } = string.Empty;
        [JsonPropertyName("summary")] public string Summary { get; init; } = string.Empty;
        [JsonPropertyName("url")] public string Url { get; init; } = string.Empty;
        [JsonPropertyName("preview_url")] public string PreviewUrl { get; init; } = string.Empty;
    }

    public sealed class Result
    {
        [JsonPropertyName("message_id")] public long MessageId { get; init; }
    }
}
