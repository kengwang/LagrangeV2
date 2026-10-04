using FastEndpoints;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;
using Lagrange.Milky.Converters;
using Lagrange.Milky.Models.Messages;
namespace Lagrange.Milky.Api.Handlers.Message;

/// <summary>QQ extension: inspect history positions without marking messages read.</summary>
public sealed class ProbeHistorySyncStateHandler(BotContext context) : Endpoint<ProbeHistorySyncStateHandler.Request, MilkyApiResponse<ProbeHistorySyncStateHandler.Result>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/probe_history_sync_state"); }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var state = await context.ProbeHistorySyncState(request.GroupIds, request.PrivateTargets.Select(x => new BotHistoryPrivateTarget(x.UserId, x.Uid)).ToArray(), ct);
        return new(new Result
        {
            Groups = state.Groups.Select(x => new GroupState(x.GroupUin, x.ReadSequence, x.LatestSequence)).ToArray(),
            PrivateUsers = state.PrivateUsers.Select(x => new PrivateState(x.UserUin, x.Uid, x.ReadSequence, x.LatestSequence, x.LastMessageTime)).ToArray()
        });
    }
    public sealed class Request
    {
        private IReadOnlyList<long>? _groupIds;
        private IReadOnlyList<PrivateTarget>? _privateTargets;
        [JsonPropertyName("group_ids")] public IReadOnlyList<long> GroupIds { get => _groupIds ?? []; init => _groupIds = value; }
        [JsonPropertyName("private_targets")] public IReadOnlyList<PrivateTarget> PrivateTargets { get => _privateTargets ?? []; init => _privateTargets = value; }
    }
    public sealed record PrivateTarget([property: JsonPropertyName("user_id")] long UserId, [property: JsonPropertyName("uid")] string Uid);
    public sealed record GroupState([property: JsonPropertyName("group_id")] long GroupId, [property: JsonPropertyName("read_seq")] ulong ReadSequence, [property: JsonPropertyName("latest_seq")] ulong LatestSequence);
    public sealed record PrivateState([property: JsonPropertyName("user_id")] long UserId, [property: JsonPropertyName("uid")] string Uid, [property: JsonPropertyName("read_seq")] ulong ReadSequence, [property: JsonPropertyName("latest_seq")] ulong LatestSequence, [property: JsonPropertyName("last_message_time")] ulong LastMessageTime);
    public sealed class Result
    {
        [JsonPropertyName("groups")] public IReadOnlyList<GroupState> Groups { get; init; } = [];
        [JsonPropertyName("private_users")] public IReadOnlyList<PrivateState> PrivateUsers { get; init; } = [];
    }
}

/// <summary>QQ extension: retrieve one bounded forward sequence page.</summary>
public sealed class GetHistorySyncPageHandler(BotContext context, MilkyConverter converter) : Endpoint<GetHistorySyncPageHandler.Request, MilkyApiResponse<GetHistorySyncPageHandler.Result>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_history_sync_page"); }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var page = request.MessageScene switch
        {
            "group" => await context.GetGroupHistorySyncPage(request.PeerId, request.NextSequence, request.LatestSequence, request.Limit ?? 20, ct),
            "friend" => await context.GetPrivateHistorySyncPage(request.PeerId, request.NextSequence, request.LatestSequence, request.Limit ?? 20, ct),
            _ => throw new ArgumentException("message_scene must be group or friend.")
        };
        var messages = new List<IncomingMessageBase>();
        foreach (var message in page.Messages) messages.Add(await converter.ToIncomingMessageAsync(message, ct));
        return new(new Result { Messages = messages, NextSequence = page.NextSequence, IsComplete = page.IsComplete });
    }
    public sealed class Request
    {
        [JsonPropertyName("message_scene")] public string? MessageScene { get; init; }
        [JsonPropertyName("peer_id")] public long PeerId { get; init; }
        [JsonPropertyName("next_seq")] public ulong NextSequence { get; init; }
        [JsonPropertyName("latest_seq")] public ulong LatestSequence { get; init; }
        [JsonPropertyName("limit")] public uint? Limit { get; init; }
    }
    public sealed class Result
    {
        [JsonPropertyName("messages")] public IReadOnlyList<IncomingMessageBase> Messages { get; init; } = [];
        [JsonPropertyName("next_seq")] public ulong? NextSequence { get; init; }
        [JsonPropertyName("is_complete")] public bool IsComplete { get; init; }
    }
}

/// <summary>QQ extension: retrieve a private roam page using the full server cursor.</summary>
public sealed class GetPrivateHistoryRoamPageHandler(BotContext context, MilkyConverter converter) : Endpoint<GetPrivateHistoryRoamPageHandler.Request, MilkyApiResponse<GetPrivateHistoryRoamPageHandler.Result>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_private_history_roam_page"); }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var page = await context.GetPrivateHistoryRoamPage(request.UserId, new(request.Time, request.Random), request.Limit ?? 20, ct);
        var messages = new List<IncomingMessageBase>();
        foreach (var message in page.Messages) messages.Add(await converter.ToIncomingMessageAsync(message, ct));
        return new(new Result { Messages = messages, NextCursor = page.NextCursor is { } next ? new(next.Time, next.Random) : null, IsComplete = page.IsComplete });
    }
    public sealed class Request
    {
        [JsonPropertyName("user_id")] public long UserId { get; init; }
        [JsonPropertyName("time")] public uint Time { get; init; }
        [JsonPropertyName("random")] public uint Random { get; init; }
        [JsonPropertyName("limit")] public uint? Limit { get; init; }
    }
    public sealed record Cursor([property: JsonPropertyName("time")] uint Time, [property: JsonPropertyName("random")] uint Random);
    public sealed class Result
    {
        [JsonPropertyName("messages")] public IReadOnlyList<IncomingMessageBase> Messages { get; init; } = [];
        [JsonPropertyName("next_cursor")] public Cursor? NextCursor { get; init; }
        [JsonPropertyName("is_complete")] public bool IsComplete { get; init; }
    }
}
