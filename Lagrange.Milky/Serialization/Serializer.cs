using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core.Common;
using Lagrange.Milky.Api;
using Lagrange.Milky.Api.Handlers.File;
using Lagrange.Milky.Api.Handlers.Friend;
using Lagrange.Milky.Api.Handlers.Group;
using Lagrange.Milky.Api.Handlers.Message;
using Lagrange.Milky.Api.Handlers.Interaction;
using Lagrange.Milky.Api.Handlers.System;
using Lagrange.Milky.Events;
using Lagrange.Milky.Events.Converters;
using Lagrange.Milky.Models.Messages;
using Lagrange.Milky.Models.Segments;
using Lagrange.Milky.Models;
using Lagrange.Milky.Signing;

namespace Lagrange.Milky.Serialization;

public static partial class Serializer
{
    public static string JsonSerialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, typeof(T), JsonContext.Default);
    }
    public static byte[] JsonSerializeToUtf8Bytes<T>(T value)
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, typeof(T), JsonContext.Default);
    }
    public static Task JsonSerializableAsync<T>(Stream stream, T value, CancellationToken ct)
    {
        return JsonSerializer.SerializeAsync(stream, value, typeof(T), JsonContext.Default, ct);
    }

    public static T? JsonDeserialize<T>(string json)
    {
        return (T?)JsonSerializer.Deserialize(json, typeof(T), JsonContext.Default);
    }
    public static ValueTask<T?> JsonDeserializeAsync<T>(Stream stream, CancellationToken ct = default)
    {
        var vt = JsonSerializer.DeserializeAsync(stream, typeof(T), JsonContext.Default, ct);
        return vt.IsCompleted ? ValueTask.FromResult((T?)vt.GetAwaiter().GetResult()) : CastAwait(vt);

        static async ValueTask<T?> CastAwait(ValueTask<object?> vt) => (T?)await vt;
    }
    public static ValueTask<object?> JsonDeserializeAsync(Stream stream, Type type, CancellationToken ct)
    {
        return JsonSerializer.DeserializeAsync(stream, type, JsonContext.Default, ct);
    }

    // Signer
    [JsonSerializable(typeof(BotKeystore))]
    [JsonSerializable(typeof(SecSignRequest))]
    [JsonSerializable(typeof(SignerResponse<SecSignResult>))]
    [JsonSerializable(typeof(AndroidSigner.ResponseRoot<AndroidSigner.SignResponse>))]
    [JsonSerializable(typeof(AndroidSigner.ResponseRoot<string>))]
    // Event
    [JsonSerializable(typeof(MilkyEvent))]
    [JsonSerializable(typeof(IncomingMessageBase))]
    [JsonSerializable(typeof(FriendIncomingMessage))]
    [JsonSerializable(typeof(GroupIncomingMessage))]
    [JsonSerializable(typeof(IncomingSegmentBase))]
    [JsonSerializable(typeof(OutgoingSegmentBase))]
    [JsonSerializable(typeof(GroupCardChangeEventConverter.Data), TypeInfoPropertyName = "GroupCardChangeEventData")]
    [JsonSerializable(typeof(Friend))]
    [JsonSerializable(typeof(FriendCategory))]
    [JsonSerializable(typeof(FriendRequest))]
    [JsonSerializable(typeof(Group))]
    [JsonSerializable(typeof(GroupMember))]
    [JsonSerializable(typeof(GroupAnnouncement))]
    [JsonSerializable(typeof(GroupFile))]
    [JsonSerializable(typeof(GroupFolder))]
    [JsonSerializable(typeof(GroupNotificationBase))]
    [JsonSerializable(typeof(JoinRequestGroupNotification))]
    [JsonSerializable(typeof(AdminChangeGroupNotification))]
    [JsonSerializable(typeof(KickGroupNotification))]
    [JsonSerializable(typeof(QuitGroupNotification))]
    [JsonSerializable(typeof(InvitedJoinRequestGroupNotification))]
    [JsonSerializable(typeof(IncomingForwardedMessage))]
    [JsonSerializable(typeof(FaceIncomingSegment))]
    [JsonSerializable(typeof(XmlIncomingSegment))]
    [JsonSerializable(typeof(PokeIncomingSegment))]
    [JsonSerializable(typeof(MarkdownIncomingSegment))]
    [JsonSerializable(typeof(KeyboardIncomingSegment))]
    [JsonSerializable(typeof(SpecialPokeIncomingSegment))]
    [JsonSerializable(typeof(JsonIncomingSegment))]
    [JsonSerializable(typeof(LocationIncomingSegment))]
    [JsonSerializable(typeof(MusicIncomingSegment))]
    [JsonSerializable(typeof(ShareIncomingSegment))]
    [JsonSerializable(typeof(ContactIncomingSegment))]
    [JsonSerializable(typeof(DiceIncomingSegment))]
    [JsonSerializable(typeof(RpsIncomingSegment))]
    [JsonSerializable(typeof(LongMsgIncomingSegment))]
    [JsonSerializable(typeof(StreamIncomingSegment))]
    [JsonSerializable(typeof(BounceFaceIncomingSegment))]
    [JsonSerializable(typeof(GroupReactionIncomingSegment))]
    [JsonSerializable(typeof(GreyTipIncomingSegment))]
    [JsonSerializable(typeof(GreyTipOutgoingSegment))]
    [JsonSerializable(typeof(MarketFaceIncomingSegment))]
    [JsonSerializable(typeof(FaceOutgoingSegment))]
    [JsonSerializable(typeof(MarketFaceOutgoingSegment))]
    [JsonSerializable(typeof(XmlOutgoingSegment))]
    [JsonSerializable(typeof(PokeOutgoingSegment))]
    [JsonSerializable(typeof(MarkdownOutgoingSegment))]
    [JsonSerializable(typeof(KeyboardOutgoingSegment))]
    [JsonSerializable(typeof(SpecialPokeOutgoingSegment))]
    [JsonSerializable(typeof(JsonOutgoingSegment))]
    [JsonSerializable(typeof(LocationOutgoingSegment))]
    [JsonSerializable(typeof(MusicOutgoingSegment))]
    [JsonSerializable(typeof(ShareOutgoingSegment))]
    [JsonSerializable(typeof(ContactOutgoingSegment))]
    [JsonSerializable(typeof(DiceOutgoingSegment))]
    [JsonSerializable(typeof(RpsOutgoingSegment))]
    [JsonSerializable(typeof(LongMsgOutgoingSegment))]
    [JsonSerializable(typeof(StreamOutgoingSegment))]
    [JsonSerializable(typeof(BounceFaceOutgoingSegment))]
    [JsonSerializable(typeof(GroupReactionOutgoingSegment))]
    [JsonSerializable(typeof(BotOfflineEventConverter.Data), TypeInfoPropertyName = "BotOfflineEventData")]
    [JsonSerializable(typeof(MessageRecallEventConverter.Data), TypeInfoPropertyName = "MessageRecallEventData")]
    [JsonSerializable(typeof(FriendRequestEventConverter.Data), TypeInfoPropertyName = "FriendRequestEventData")]
    [JsonSerializable(typeof(GroupJoinRequestEventConverter.Data), TypeInfoPropertyName = "GroupJoinRequestEventData")]
    [JsonSerializable(typeof(GroupInvitedJoinRequestEventConverter.Data), TypeInfoPropertyName = "GroupInvitedJoinRequestEventData")]
    [JsonSerializable(typeof(GroupInvitationEventConverter.Data), TypeInfoPropertyName = "GroupInvitationEventData")]
    [JsonSerializable(typeof(GroupMemberIncreaseEventConverter.Data), TypeInfoPropertyName = "GroupMemberIncreaseEventData")]
    [JsonSerializable(typeof(GroupMemberDecreaseEventConverter.Data), TypeInfoPropertyName = "GroupMemberDecreaseEventData")]
    [JsonSerializable(typeof(GroupMessageReactionEventConverter.Data), TypeInfoPropertyName = "GroupMessageReactionEventData")]
    [JsonSerializable(typeof(GroupNudgeEventConverter.Data), TypeInfoPropertyName = "GroupNudgeEventData")]
    [JsonSerializable(typeof(FriendNudgeEventConverter.Data), TypeInfoPropertyName = "FriendNudgeEventData")]
    [JsonSerializable(typeof(FriendFileUploadEventConverter.Data), TypeInfoPropertyName = "FriendFileUploadEventData")]
    [JsonSerializable(typeof(GroupAdminEventConverter.Data), TypeInfoPropertyName = "GroupAdminEventData")]
    [JsonSerializable(typeof(GroupEssenceEventConverter.Data), TypeInfoPropertyName = "GroupEssenceEventData")]
    [JsonSerializable(typeof(GroupFileUploadEventConverter.Data), TypeInfoPropertyName = "GroupFileUploadEventData")]
    [JsonSerializable(typeof(GroupMuteEventConverter.Data), TypeInfoPropertyName = "GroupMuteEventData")]
    [JsonSerializable(typeof(GroupNameChangeEventConverter.Data), TypeInfoPropertyName = "GroupNameChangeEventData")]
    [JsonSerializable(typeof(FriendFileUploadEventConverter.Data), TypeInfoPropertyName = "FriendFileUploadEventConverterData")]
    [JsonSerializable(typeof(FriendNudgeEventConverter.Data), TypeInfoPropertyName = "FriendNudgeEventConverterData")]
    [JsonSerializable(typeof(FriendRemarkChangedEventConverter.Data), TypeInfoPropertyName = "FriendRemarkChangedEventConverterData")]
    [JsonSerializable(typeof(GroupAdminEventConverter.Data), TypeInfoPropertyName = "GroupAdminEventConverterData")]
    [JsonSerializable(typeof(GroupEssenceEventConverter.Data), TypeInfoPropertyName = "GroupEssenceEventConverterData")]
    [JsonSerializable(typeof(GroupFileUploadEventConverter.Data), TypeInfoPropertyName = "GroupFileUploadEventConverterData")]
    [JsonSerializable(typeof(GroupMuteEventConverter.Data), TypeInfoPropertyName = "GroupMuteEventConverterData")]
    [JsonSerializable(typeof(GroupNameChangeEventConverter.Data), TypeInfoPropertyName = "GroupNameChangeEventConverterData")]
    [JsonSerializable(typeof(PeerPinChangeEventConverter.Data), TypeInfoPropertyName = "PeerPinChangeEventConverterData")]
    [JsonSerializable(typeof(FriendAddedEventConverter.Data), TypeInfoPropertyName = "FriendAddedEventConverterEventData")]
    [JsonSerializable(typeof(FriendFileUploadEventConverter.Data), TypeInfoPropertyName = "FriendFileUploadEventConverterEventData")]
    [JsonSerializable(typeof(FriendInputStatusEventConverter.Data), TypeInfoPropertyName = "FriendInputStatusEventConverterEventData")]
    [JsonSerializable(typeof(FriendNudgeEventConverter.Data), TypeInfoPropertyName = "FriendNudgeEventConverterEventData")]
    [JsonSerializable(typeof(FriendProfileLikeEventConverter.Data), TypeInfoPropertyName = "FriendProfileLikeEventConverterEventData")]
    [JsonSerializable(typeof(FriendRemarkChangedEventConverter.Data), TypeInfoPropertyName = "FriendRemarkChangedEventConverterEventData")]
    [JsonSerializable(typeof(GroupAdminEventConverter.Data), TypeInfoPropertyName = "GroupAdminEventConverterEventData")]
    [JsonSerializable(typeof(GroupEssenceEventConverter.Data), TypeInfoPropertyName = "GroupEssenceEventConverterEventData")]
    [JsonSerializable(typeof(GroupFileUploadEventConverter.Data), TypeInfoPropertyName = "GroupFileUploadEventConverterEventData")]
    [JsonSerializable(typeof(GroupMuteEventConverter.Data), TypeInfoPropertyName = "GroupMuteEventConverterEventData")]
    [JsonSerializable(typeof(GroupNameChangeEventConverter.Data), TypeInfoPropertyName = "GroupNameChangeEventConverterEventData")]
    [JsonSerializable(typeof(GroupSelfJoinedEventConverter.Data), TypeInfoPropertyName = "GroupSelfJoinedEventConverterEventData")]
    [JsonSerializable(typeof(GroupSpecialTitleChangeEventConverter.Data), TypeInfoPropertyName = "GroupSpecialTitleChangeEventConverterEventData")]
    [JsonSerializable(typeof(GroupWholeMuteEventConverter.Data), TypeInfoPropertyName = "GroupWholeMuteEventConverterEventData")]
    [JsonSerializable(typeof(OnlineDevicesChangedEventConverter.Data), TypeInfoPropertyName = "OnlineDevicesChangedEventConverterEventData")]
    [JsonSerializable(typeof(PeerPinChangeEventConverter.Data), TypeInfoPropertyName = "PeerPinChangeEventConverterEventData")]
    private sealed partial class JsonContext : JsonSerializerContext;
}
