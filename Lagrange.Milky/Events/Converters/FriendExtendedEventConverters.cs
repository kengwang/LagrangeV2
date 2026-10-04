using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Milky.Events.Attributes;

namespace Lagrange.Milky.Events.Converters;

[EventConverter]
public sealed class FriendRemarkChangedEventConverter : IEventConverter<BotFriendRemarkChangedEvent, FriendRemarkChangedEventConverter.Data>
{
    public string Name => "friend_remark_changed";
    public bool CanConvert(BotFriendRemarkChangedEvent @event) => true;
    public ValueTask<Data> ConvertAsync(BotFriendRemarkChangedEvent @event, CancellationToken ct) => ValueTask.FromResult(new Data
    {
        UserId = @event.UserUin, UserUid = @event.UserUid, Remark = @event.Remark
    });

    public sealed class Data
    {
        [JsonPropertyName("user_id")] public long UserId { get; init; }
        [JsonPropertyName("user_uid")] public required string UserUid { get; init; }
        [JsonPropertyName("remark")] public required string Remark { get; init; }
    }
}

[EventConverter]
public sealed class FriendInputStatusEventConverter : IEventConverter<BotFriendInputStatusEvent, FriendInputStatusEventConverter.Data>
{
    public string Name => "friend_input_status";
    public bool CanConvert(BotFriendInputStatusEvent @event) => true;
    public ValueTask<Data> ConvertAsync(BotFriendInputStatusEvent @event, CancellationToken ct) => ValueTask.FromResult(new Data
    {
        UserId = @event.UserUin, UserUid = @event.UserUid, EventType = @event.EventType, StatusText = @event.StatusText
    });

    public sealed class Data
    {
        [JsonPropertyName("user_id")] public long UserId { get; init; }
        [JsonPropertyName("user_uid")] public required string UserUid { get; init; }
        [JsonPropertyName("event_type")] public uint EventType { get; init; }
        [JsonPropertyName("status_text")] public required string StatusText { get; init; }
    }
}

[EventConverter]
public sealed class OnlineDevicesChangedEventConverter : IEventConverter<BotOnlineDevicesChangedEvent, OnlineDevicesChangedEventConverter.Data>
{
    public string Name => "online_devices_changed";
    public bool CanConvert(BotOnlineDevicesChangedEvent @event) => true;
    public ValueTask<Data> ConvertAsync(BotOnlineDevicesChangedEvent @event, CancellationToken ct) => ValueTask.FromResult(new Data
    {
        Devices = @event.Devices.Select(x => new Device
        {
            AppId = x.AppId, InstanceId = x.InstanceId, ClientType = x.ClientType,
            Platform = x.Platform, DeviceName = x.DeviceName
        }).ToArray()
    });

    public sealed class Data
    {
        [JsonPropertyName("devices")] public required IReadOnlyList<Device> Devices { get; init; }
    }

    public sealed class Device
    {
        [JsonPropertyName("app_id")] public uint AppId { get; init; }
        [JsonPropertyName("instance_id")] public uint InstanceId { get; init; }
        [JsonPropertyName("client_type")] public uint ClientType { get; init; }
        [JsonPropertyName("platform")] public uint Platform { get; init; }
        [JsonPropertyName("device_name")] public required string DeviceName { get; init; }
    }
}

[EventConverter]
public sealed class FriendProfileLikeEventConverter : IEventConverter<BotFriendProfileLikeEvent, FriendProfileLikeEventConverter.Data>
{
    public string Name => "friend_profile_like";
    public bool CanConvert(BotFriendProfileLikeEvent @event) => true;
    public ValueTask<Data> ConvertAsync(BotFriendProfileLikeEvent @event, CancellationToken ct) => ValueTask.FromResult(new Data
    {
        UserId = @event.OperatorUin, Nickname = @event.Nickname, Times = @event.Times
    });

    public sealed class Data
    {
        [JsonPropertyName("user_id")] public long UserId { get; init; }
        [JsonPropertyName("nickname")] public required string Nickname { get; init; }
        [JsonPropertyName("times")] public int Times { get; init; }
    }
}

[EventConverter]
public sealed class GroupSpecialTitleChangeEventConverter : IEventConverter<BotGroupSpecialTitleChangeEvent, GroupSpecialTitleChangeEventConverter.Data>
{
    public string Name => "group_special_title_change";
    public bool CanConvert(BotGroupSpecialTitleChangeEvent @event) => true;
    public ValueTask<Data> ConvertAsync(BotGroupSpecialTitleChangeEvent @event, CancellationToken ct) => ValueTask.FromResult(new Data
    {
        GroupId = @event.GroupUin, UserId = @event.MemberUin, OperatorId = @event.OperatorUin, Title = @event.Title
    });

    public sealed class Data
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; }
        [JsonPropertyName("user_id")] public long UserId { get; init; }
        [JsonPropertyName("operator_id")] public long OperatorId { get; init; }
        [JsonPropertyName("title")] public required string Title { get; init; }
    }
}

[EventConverter]
public sealed class FriendAddedEventConverter : IEventConverter<BotFriendAddedEvent, FriendAddedEventConverter.Data>
{
    public string Name => "friend_added";
    public bool CanConvert(BotFriendAddedEvent @event) => true;
    public ValueTask<Data> ConvertAsync(BotFriendAddedEvent @event, CancellationToken ct) => ValueTask.FromResult(new Data
    {
        UserId = @event.UserUin, UserUid = @event.UserUid, Nickname = @event.Nickname, Time = @event.Time
    });

    public sealed class Data
    {
        [JsonPropertyName("user_id")] public long UserId { get; init; }
        [JsonPropertyName("user_uid")] public required string UserUid { get; init; }
        [JsonPropertyName("nickname")] public required string Nickname { get; init; }
        [JsonPropertyName("time")] public long Time { get; init; }
    }
}

[EventConverter]
public sealed class GroupSelfJoinedEventConverter : IEventConverter<BotGroupSelfJoinedEvent, GroupSelfJoinedEventConverter.Data>
{
    public string Name => "group_self_joined";
    public bool CanConvert(BotGroupSelfJoinedEvent @event) => true;
    public ValueTask<Data> ConvertAsync(BotGroupSelfJoinedEvent @event, CancellationToken ct) => ValueTask.FromResult(new Data
    {
        GroupId = @event.GroupUin, OperatorId = @event.OperatorUin, OperatorUid = @event.OperatorUid
    });

    public sealed class Data
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; }
        [JsonPropertyName("operator_id")] public long OperatorId { get; init; }
        [JsonPropertyName("operator_uid")] public required string OperatorUid { get; init; }
    }
}
