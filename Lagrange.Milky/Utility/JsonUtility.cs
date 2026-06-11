using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using Lagrange.Core.Common;
using Lagrange.Milky.Api.Handler.File;
using Lagrange.Milky.Api.Handler.Friend;
using Lagrange.Milky.Api.Handler.Group;
using Lagrange.Milky.Api.Handler.Message;
using Lagrange.Milky.Api.Handler.System;
using Lagrange.Milky.Api.Result;
using Lagrange.Milky.Entity.Event;
using Lagrange.Milky.Entity.Segment;

namespace Lagrange.Milky.Utility;

public static partial class JsonUtility
{
    private static readonly JsonSerializerOptions Options = new(JsonContext.Default.Options)
    {
        TypeInfoResolver = JsonContext.Default.WithAddedModifier(ApplySegmentPolymorphism)
    };

    [JsonSourceGenerationOptions(AllowOutOfOrderMetadataProperties = true)]

    // BotContext
    [JsonSerializable(typeof(BotKeystore))]
    [JsonSerializable(typeof(BotAppInfo))]

    // Signer
    [JsonSerializable(typeof(SecSignRequest))]
    [JsonSerializable(typeof(SignerResponse<SecSignResponse>))]

    // === api ===
    [JsonSerializable(typeof(ApiOkResult))]
    [JsonSerializable(typeof(ApiFailedResult))]
    // == system ==
    // get_login_info
    [JsonSerializable(typeof(GetLoginInfoResult))]
    // get_impl_info
    [JsonSerializable(typeof(GetImplInfoResult))]
    // get_user_profile
    [JsonSerializable(typeof(GetUserProfileParameter))]
    [JsonSerializable(typeof(GetUserProfileResult))]
    // get_friend_list
    [JsonSerializable(typeof(GetFriendListParameter))]
    [JsonSerializable(typeof(GetFriendListResult))]
    // get_friend_info
    [JsonSerializable(typeof(GetFriendInfoParameter))]
    [JsonSerializable(typeof(GetFriendInfoResult))]
    // get_group_list
    [JsonSerializable(typeof(GetGroupListParameter))]
    [JsonSerializable(typeof(GetGroupListResult))]
    // get_group_info
    [JsonSerializable(typeof(GetGroupInfoParameter))]
    [JsonSerializable(typeof(GetGroupInfoResult))]
    // get_group_member_list
    [JsonSerializable(typeof(GetGroupMemberListParameter))]
    [JsonSerializable(typeof(GetGroupMemberListResult))]
    // get_group_member_info
    [JsonSerializable(typeof(GetGroupMemberInfoParameter))]
    [JsonSerializable(typeof(GetGroupMemberInfoResult))]
    // get_cookies
    [JsonSerializable(typeof(GetCookiesParameter))]
    [JsonSerializable(typeof(GetCookiesResult))]
    // get_csrf_token
    [JsonSerializable(typeof(GetCsrfTokenResult))]
    // peer pins
    [JsonSerializable(typeof(GetPeerPinsResult))]
    [JsonSerializable(typeof(SetPeerPinParameter))]
    // set_avatar
    [JsonSerializable(typeof(SetAvatarParameter))]
    // get_custom_face_url_list
    [JsonSerializable(typeof(GetCustomFaceUrlListResult))]
    // == message ==
    // send_private_message
    [JsonSerializable(typeof(SendPrivateMessageParameter))]
    [JsonSerializable(typeof(SendPrivateMessageResult))]
    // send_group_message
    [JsonSerializable(typeof(SendGroupMessageParameter))]
    [JsonSerializable(typeof(SendGroupMessageResult))]
    // get_message
    [JsonSerializable(typeof(GetMessageParameter))]
    [JsonSerializable(typeof(GetMessageResult))]
    // get_history_messages
    [JsonSerializable(typeof(GetHistoryMessagesParameter))]
    [JsonSerializable(typeof(GetHistoryMessagesResult))]
    // get_resource_temp_url
    [JsonSerializable(typeof(GetResourceTempUrlParameter))]
    [JsonSerializable(typeof(GetResourceTempUrlResult))]
    // get_forwarded_messages
    [JsonSerializable(typeof(GetForwardedMessagesParameter))]
    [JsonSerializable(typeof(GetForwardedMessagesResult))]
    // mark_message_as_read
    [JsonSerializable(typeof(MarkMessageAsReadParameter))]
    // recall_private_message
    [JsonSerializable(typeof(RecallPrivateMessageParameter))]
    // recall_group_message
    [JsonSerializable(typeof(RecallGroupMessageParameter))]
    // == friend ==
    // send_friend_nudge
    [JsonSerializable(typeof(SendFriendNudgeParameter))]
    // friend actions
    [JsonSerializable(typeof(SendProfileLikeParameter))]
    [JsonSerializable(typeof(DeleteFriendParameter))]
    // friend requests
    [JsonSerializable(typeof(GetFriendRequestsParameter))]
    [JsonSerializable(typeof(GetFriendRequestsResult))]
    [JsonSerializable(typeof(AcceptFriendRequestParameter))]
    [JsonSerializable(typeof(RejectFriendRequestParameter))]
    // == group ==
    // send_group_nudge
    [JsonSerializable(typeof(SendGroupNudgeParameter))]
    // set_group_name
    [JsonSerializable(typeof(SetGroupNameParameter))]
    // set_group_avatar
    [JsonSerializable(typeof(SetGroupAvatarParameter))]
    // set_group_member_card
    [JsonSerializable(typeof(SetGroupMemberCardParameter))]
    // set_group_member_special_title
    [JsonSerializable(typeof(SetGroupMemberSpecialTitleParameter))]
    // quit_group
    [JsonSerializable(typeof(QuitGroupParameter))]
    // send_group_message_reaction
    [JsonSerializable(typeof(SendGroupMessageReactionParameter))]
    // get_group_notifications
    [JsonSerializable(typeof(GetGroupNotificationsParameter))]
    [JsonSerializable(typeof(GetGroupNotificationsResult))]
    // group moderation
    [JsonSerializable(typeof(SetGroupMemberAdminParameter))]
    [JsonSerializable(typeof(SetGroupMemberMuteParameter))]
    [JsonSerializable(typeof(SetGroupWholeMuteParameter))]
    [JsonSerializable(typeof(KickGroupMemberParameter))]
    // group request operations
    [JsonSerializable(typeof(GroupRequestParameter))]
    [JsonSerializable(typeof(RejectGroupRequestParameter))]
    [JsonSerializable(typeof(GroupInvitationOperateParameter))]
    // set_group_essence_message
    [JsonSerializable(typeof(SetGroupEssenceMessageParameter))]
    // == file ==
    // upload_group_file
    [JsonSerializable(typeof(UploadGroupFileParameter))]
    [JsonSerializable(typeof(UploadGroupFileResult))]
    // upload_private_file
    [JsonSerializable(typeof(UploadPrivateFileParameter))]
    [JsonSerializable(typeof(UploadPrivateFileResult))]
    // upload_flash_transfer
    [JsonSerializable(typeof(UploadFlashTransferParameter))]
    [JsonSerializable(typeof(UploadFlashTransferFileParameter))]
    [JsonSerializable(typeof(UploadFlashTransferResult))]
    // get_group_file_download_url
    [JsonSerializable(typeof(GetGroupFileDownloadUrlParameter))]
    [JsonSerializable(typeof(GetGroupFileDownloadUrlResult))]
    // get_private_file_download_url
    [JsonSerializable(typeof(GetPrivateFileDownloadUrlParameter))]
    [JsonSerializable(typeof(GetPrivateFileDownloadUrlResult))]
    // delete_group_file
    [JsonSerializable(typeof(DeleteGroupFileParameter))]
    // move_group_file
    [JsonSerializable(typeof(MoveGroupFileParameter))]
    // rename_group_file
    [JsonSerializable(typeof(RenameGroupFileParameter))]
    // group file folders
    [JsonSerializable(typeof(GetGroupFilesParameter))]
    [JsonSerializable(typeof(GetGroupFilesResult))]
    [JsonSerializable(typeof(CreateGroupFolderParameter))]
    [JsonSerializable(typeof(CreateGroupFolderResult))]
    [JsonSerializable(typeof(RenameGroupFolderParameter))]
    [JsonSerializable(typeof(DeleteGroupFolderParameter))]
    // === debug ===

    // === event ===
    // bot_offline
    [JsonSerializable(typeof(BotOfflineEvent))]
    // message_receive
    [JsonSerializable(typeof(MessageReceiveEvent))]
    // group_nudge
    [JsonSerializable(typeof(GroupNudgeEvent))]
    // group_member_increase
    [JsonSerializable(typeof(GroupMemberIncreaseEvent))]
    // group_member_decrease
    [JsonSerializable(typeof(GroupMemberDecreaseEvent))]
    // friend_request
    [JsonSerializable(typeof(FriendRequestEvent))]
    // group_invitation
    [JsonSerializable(typeof(GroupInvitationEvent))]
    // group_join_request
    [JsonSerializable(typeof(GroupJoinRequestEvent))]
    // group_invited_join_request
    [JsonSerializable(typeof(GroupInvitedJoinRequestEvent))]
    // group_message_reaction
    [JsonSerializable(typeof(GroupMessageReactionEvent))]
    // message_recall
    [JsonSerializable(typeof(MessageRecallEvent))]
    private partial class JsonContext : JsonSerializerContext;

    public static string Serialize<T>(T value) where T : class
    {
        return JsonSerializer.Serialize(value, Options.GetTypeInfo(typeof(T)));
    }

    public static byte[] SerializeToUtf8Bytes<T>(T value) where T : class
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, Options.GetTypeInfo(typeof(T)));
    }

    public static byte[] SerializeToUtf8Bytes(Type type, object? value)
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, Options.GetTypeInfo(type));
    }

    public static T? Deserialize<T>(byte[] json) where T : class
    {
        return JsonSerializer.Deserialize(json, Options.GetTypeInfo(typeof(T))) as T;
    }
    public static object? Deserialize(Type type, byte[] json)
    {
        return JsonSerializer.Deserialize(json, Options.GetTypeInfo(type));
    }

    public static T? Deserialize<T>(Stream json) where T : class
    {
        return JsonSerializer.Deserialize(json, Options.GetTypeInfo(typeof(T))) as T;
    }

    private static void ApplySegmentPolymorphism(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type == typeof(IIncomingSegment))
        {
            typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "type",
                DerivedTypes =
                {
                    new JsonDerivedType(typeof(TextIncomingSegment), "text"),
                    new JsonDerivedType(typeof(MentionIncomingSegment), "mention"),
                    new JsonDerivedType(typeof(MentionAllIncomingSegment), "mention_all"),
                    new JsonDerivedType(typeof(FaceIncomingSegment), "face"),
                    new JsonDerivedType(typeof(ReplyIncomingSegment), "reply"),
                    new JsonDerivedType(typeof(ImageIncomingSegment), "image"),
                    new JsonDerivedType(typeof(RecordIncomingSegment), "record"),
                    new JsonDerivedType(typeof(VideoIncomingSegment), "video"),
                    new JsonDerivedType(typeof(FileIncomingSegment), "file"),
                    new JsonDerivedType(typeof(ForwardIncomingSegment), "forward"),
                    new JsonDerivedType(typeof(MarketFaceIncomingSegment), "market_face"),
                    new JsonDerivedType(typeof(LightAppIncomingSegment), "light_app"),
                    new JsonDerivedType(typeof(XmlIncomingSegment), "xml"),
                    new JsonDerivedType(typeof(MarkdownIncomingSegment), "markdown")
                }
            };
        }
        else if (typeInfo.Type == typeof(IOutgoingSegment))
        {
            typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "type",
                DerivedTypes =
                {
                    new JsonDerivedType(typeof(TextOutgoingSegment), "text"),
                    new JsonDerivedType(typeof(MentionOutgoingSegment), "mention"),
                    new JsonDerivedType(typeof(MentionAllOutgoingSegment), "mention_all"),
                    new JsonDerivedType(typeof(FaceOutgoingSegment), "face"),
                    new JsonDerivedType(typeof(ReplyOutgoingSegment), "reply"),
                    new JsonDerivedType(typeof(ImageOutgoingSegment), "image"),
                    new JsonDerivedType(typeof(RecordOutgoingSegment), "record"),
                    new JsonDerivedType(typeof(VideoOutgoingSegment), "video"),
                    new JsonDerivedType(typeof(ForwardOutgoingSegment), "forward"),
                    new JsonDerivedType(typeof(LightAppOutgoingSegment), "light_app"),
                    new JsonDerivedType(typeof(MarkdownOutgoingSegment), "markdown")
                }
            };
        }
    }
}
