using FastEndpoints;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Exceptions;

namespace Lagrange.Milky.Api.Handlers.System;

/// <summary>Lagrange extension for a single QQ friends-status response.</summary>
public sealed class GetFriendsStatusHandler(BotContext lagrange)
    : EndpointWithoutRequest<MilkyApiResponse<GetFriendsStatusHandler.Result>>
{
    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_friends_status");
    }

    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(CancellationToken ct)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(ct, timeout.Token);
        try
        {
            var result = await lagrange.GetFriendsStatus(lifetime.Token);
            return new(new Result
            {
                SelfUid = result.SelfUid,
                Friends = result.Friends.Select(friend => new FriendStatus
                {
                    Uid = friend.Uid,
                    Status = friend.Status,
                    StatusText = friend.StatusText,
                    RawFields = friend.RawFields.Select(field => new RawField
                    {
                        Field = field.Field, WireType = field.WireType, Value = field.Value
                    }).ToArray()
                }).ToArray()
            });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested) { return new(-408, "Friends-status operation timed out after 10 seconds."); }
        catch (OperationException ex) { return new(ex.Result, ex.ErrMsg ?? ex.Message); }
        catch (Exception ex) { return new(-500, ex.Message); }
    }

    public sealed class Result
    {
        [JsonPropertyName("self_uid")] public required string SelfUid { get; init; }
        [JsonPropertyName("friends")] public required IReadOnlyList<FriendStatus> Friends { get; init; }
    }

    public sealed class FriendStatus
    {
        [JsonPropertyName("uid")] public required string Uid { get; init; }
        [JsonPropertyName("status")] public uint? Status { get; init; }
        [JsonPropertyName("status_text")] public required string StatusText { get; init; }
        [JsonPropertyName("raw_fields")] public required IReadOnlyList<RawField> RawFields { get; init; }
    }

    public sealed class RawField
    {
        [JsonPropertyName("field")] public uint Field { get; init; }
        [JsonPropertyName("wire_type")] public byte WireType { get; init; }
        [JsonPropertyName("value")] public required string Value { get; init; }
    }
}
