using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Internal.Logic;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Message;

namespace Lagrange.Core.Common.Interface;

public static class OperationExt
{
    public static Task<BotQrCodeInfo?> FetchQrCodeInfo(this BotContext context, byte[] k) =>
        context.EventContext.GetLogic<WtExchangeLogic>().FetchQrCodeInfo(k);

    public static Task<(bool Success, string Message)> CloseQrCode(this BotContext context, byte[] k, bool confirm) =>
        context.EventContext.GetLogic<WtExchangeLogic>().CloseQrCode(k, confirm);

    public static Task<Dictionary<string, string>> FetchCookies(this BotContext context, params List<string> domains) =>
        context.EventContext.GetLogic<OperationLogic>().FetchCookies(domains);

    public static Task<Dictionary<string, string>> FetchCookies(this BotContext context, CancellationToken cancellationToken, params List<string> domains) =>
        context.EventContext.GetLogic<OperationLogic>().FetchCookies(domains, cancellationToken);

    public static async Task RefreshHttpCookies(this BotContext context, params string[] domains) =>
        await context.EventContext.SendEvent<RefreshHttpCookiesEventResp>(new RefreshHttpCookiesEventReq(domains));

    public static async Task<BotCollectionResult> GetCollection(this BotContext context, uint count = 50, CancellationToken cancellationToken = default) =>
        (await context.EventContext.SendEvent<GetCollectionEventResp>(new GetCollectionEventReq(count), cancellationToken)).Result;

    /// <summary>Reads one group album page from QQ NT AlbumService.</summary>
    /// <param name="context">The logged-in bot.</param>
    /// <param name="groupUin">Group number.</param>
    /// <param name="attachInfo">The previous page's continuation cursor, or empty for the first page.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>Albums and the server-provided continuation state.</returns>
    public static async Task<BotGroupAlbumResult> GetGroupAlbums(this BotContext context, long groupUin, string attachInfo = "", CancellationToken cancellationToken = default)
    {
        var result = (await context.EventContext.SendEvent<GetGroupAlbumsEventResp>(new GetGroupAlbumsEventReq(groupUin, attachInfo), cancellationToken)).Result;
        GroupAlbumExt.ValidatePage(result, attachInfo);
        return result;
    }

    public static async Task<BotQzoneMessageListResult> GetQzoneMessageList(this BotContext context, long? targetUin = null, int position = 0, int count = 20, CancellationToken cancellationToken = default) =>
        (await context.EventContext.SendEvent<GetQzoneMessageListEventResp>(new GetQzoneMessageListEventReq(targetUin.GetValueOrDefault(context.BotUin), position, count), cancellationToken)).Result;

    public static async Task<BotQzoneFeedResult> GetQzoneFeeds(this BotContext context, long? targetUin = null, int page = 1, int count = 10, CancellationToken cancellationToken = default) =>
        (await context.EventContext.SendEvent<GetQzoneFeedsEventResp>(new GetQzoneFeedsEventReq(targetUin.GetValueOrDefault(context.BotUin), page, count), cancellationToken)).Result;

    public static async Task<BotQzoneUploadResult> UploadQzoneImage(this BotContext context, Stream image, CancellationToken cancellationToken = default) =>
        (await context.EventContext.SendEvent<UploadQzoneImageEventResp>(new UploadQzoneImageEventReq(image), cancellationToken)).Result;

    public static async Task SetQzoneLike(this BotContext context, long targetUin, string messageId, bool like, long abstime = 0, CancellationToken cancellationToken = default) =>
        await context.EventContext.SendEvent<SetQzoneLikeEventResp>(new SetQzoneLikeEventReq(targetUin, messageId, like, abstime), cancellationToken);

    public static async Task<BotQzoneCommentResult> CommentQzoneMessage(this BotContext context, long targetUin, string messageId, string content, CancellationToken cancellationToken = default) =>
        (await context.EventContext.SendEvent<QzoneMutationEventResp>(new QzoneMutationEventReq(QzoneMutationKind.Comment, targetUin, messageId, content, string.Empty, false), cancellationToken)).Comment!;

    public static async Task DeleteQzoneMessage(this BotContext context, string messageId, CancellationToken cancellationToken = default) =>
        await context.EventContext.SendEvent<QzoneMutationEventResp>(new QzoneMutationEventReq(QzoneMutationKind.Delete, 0, messageId, string.Empty, string.Empty, false), cancellationToken);

    public static async Task<BotQzonePublishResult> PublishQzoneMessage(this BotContext context, string content, string richValue = "", CancellationToken cancellationToken = default) =>
        (await context.EventContext.SendEvent<QzoneMutationEventResp>(new QzoneMutationEventReq(QzoneMutationKind.Publish, 0, string.Empty, content, richValue, false), cancellationToken)).Publish!;

    public static async Task SetQzoneBlack(this BotContext context, long targetUin, bool ban, CancellationToken cancellationToken = default) =>
        await context.EventContext.SendEvent<QzoneMutationEventResp>(new QzoneMutationEventReq(QzoneMutationKind.Black, targetUin, string.Empty, string.Empty, string.Empty, ban), cancellationToken);

    public static async Task<BotFriendDressResult> GetFriendDress(this BotContext context, long targetUin, CancellationToken cancellationToken = default) =>
        (await context.EventContext.SendEvent<GetFriendDressEventResp>(new GetFriendDressEventReq(targetUin), cancellationToken)).Result;

    public static async Task<BotGroupEssenceResult> GetGroupEssence(this BotContext context, long groupUin, int pageStart = 0, int pageLimit = 50, CancellationToken cancellationToken = default) =>
        (await context.EventContext.SendEvent<GetGroupEssenceEventResp>(new GetGroupEssenceEventReq(groupUin, pageStart, pageLimit), cancellationToken)).Result;

    public static async Task<BotGroupHonorResult> GetGroupHonor(this BotContext context, long groupUin, string honorType = "all", CancellationToken cancellationToken = default) =>
        (await context.EventContext.SendEvent<GetGroupHonorEventResp>(new GetGroupHonorEventReq(groupUin, honorType.Trim().ToLowerInvariant()), cancellationToken)).Result;

    public static async Task<BotGroupAnnouncementResult> GetGroupAnnouncements(this BotContext context, long groupUin, int start = -1, int count = 20, CancellationToken cancellationToken = default) =>
        (await context.EventContext.SendEvent<GetGroupAnnouncementsEventResp>(new GetGroupAnnouncementsEventReq(groupUin, start, count), cancellationToken)).Result;

    public static Task PublishGroupAnnouncement(this BotContext context, long groupUin, string content, bool pinned = false, CancellationToken cancellationToken = default) =>
        context.PublishGroupAnnouncement(groupUin, content, new BotGroupAnnouncementOptions { Pinned = pinned }, cancellationToken);

    public static async Task PublishGroupAnnouncement(this BotContext context, long groupUin, string content, BotGroupAnnouncementOptions options, CancellationToken cancellationToken = default) =>
        await context.EventContext.SendEvent<PublishGroupAnnouncementEventResp>(new PublishGroupAnnouncementEventReq(groupUin, content, options), cancellationToken);

    public static async Task<BotGroupAnnouncementImage> UploadGroupAnnouncementImage(this BotContext context, Stream image, CancellationToken cancellationToken = default) =>
        (await context.EventContext.SendEvent<UploadGroupAnnouncementImageEventResp>(new UploadGroupAnnouncementImageEventReq(image), cancellationToken)).Result;

    public static async Task DeleteGroupAnnouncement(this BotContext context, long groupUin, string announcementId, CancellationToken cancellationToken = default) =>
        await context.EventContext.SendEvent<DeleteGroupAnnouncementEventResp>(new DeleteGroupAnnouncementEventReq(groupUin, announcementId), cancellationToken);

    public static async Task<BotGroupSignInResult> GetGroupSignIn(this BotContext context, long groupUin, DateTime? day = null, CancellationToken cancellationToken = default) =>
        (await context.EventContext.SendEvent<GetGroupSignInEventResp>(new GetGroupSignInEventReq(groupUin, day), cancellationToken)).Result;

    public static Task MarkMessageAsRead(this BotContext context, string scene, long peerUin, ulong lastReadSeq, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().MarkMessageAsRead(scene, peerUin, lastReadSeq, cancellationToken);

    public static Task MarkAllMessagesAsRead(this BotContext context, IReadOnlyList<long> groupUins, IReadOnlyList<long> privateUins, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().MarkAllMessagesAsRead(groupUins, privateUins, cancellationToken);

    public static Task<ulong> GetLatestPrivateMessageSequence(this BotContext context, long peerUin, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().GetLatestPrivateMessageSequence(peerUin, cancellationToken);

    public static Task SetGroupAddOption(this BotContext context, long groupUin, uint addType, string? question = null, string? answer = null, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().SetGroupAddOption(groupUin, addType, question, answer, cancellationToken);

    public static Task SetGroupSearch(this BotContext context, long groupUin, uint? noFingerOpen = null, uint? noCodeFingerOpen = null, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().SetGroupSearch(groupUin, noFingerOpen, noCodeFingerOpen, cancellationToken);

    public static Task SetGroupNewMemberHistoryVisibility(this BotContext context, long groupUin, bool visible, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().SetGroupNewMemberHistoryVisibility(groupUin, visible, cancellationToken);

    public static Task SetGroupInvitePolicy(this BotContext context, long groupUin, uint currentPrivilegeFlag, string policy, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().SetGroupInvitePolicy(groupUin, currentPrivilegeFlag, policy, cancellationToken);

    public static Task SetGroupRobotAddOption(this BotContext context, long groupUin, uint? memberSwitch = null, uint? memberExamine = null, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().SetGroupRobotAddOption(groupUin, memberSwitch, memberExamine, cancellationToken);

    public static Task<BotGroupAdminSettingsResult> GetGroupAdminSettings(this BotContext context, long groupUin, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().GetGroupAdminSettings(groupUin, cancellationToken);

    public static Task<BotPeerPinsResult> GetPeerPins(this BotContext context, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().GetPeerPins(cancellationToken);

    public static Task<BotGroupAlbumMediaResult> GetGroupAlbumMedia(this BotContext context, long groupUin, string albumId, string attachInfo = "", CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().GetGroupAlbumMedia(groupUin, albumId, attachInfo, cancellationToken);

    public static Task SetGroupAlbumLike(this BotContext context, long groupUin, string albumId, string batchId, string? mediaId, bool isLike, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().SetGroupAlbumLike(groupUin, albumId, batchId, mediaId, isLike, cancellationToken);

    public static Task DeleteGroupAlbumMedia(this BotContext context, long groupUin, string albumId, string mediaId, string? batchId = null, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().DeleteGroupAlbumMedia(groupUin, albumId, mediaId, batchId, cancellationToken);

    public static Task<string> CommentGroupAlbumMedia(this BotContext context, long groupUin, string albumId, string mediaId, string content, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().CommentGroupAlbumMedia(groupUin, albumId, mediaId, content, cancellationToken);

    public static async Task<BotGroupAlbumUploadResult> UploadGroupAlbumImage(this BotContext context, long groupUin, string albumId, string albumName, Stream image, string fileName = "image.jpg", CancellationToken cancellationToken = default) =>
        (await context.EventContext.SendEvent<UploadGroupAlbumImageEventResp>(new UploadGroupAlbumImageEventReq(groupUin, albumId, albumName, image, fileName), cancellationToken)).Result;

    public static async Task<byte[]> PostHttpBytes(this BotContext context, Uri endpoint, ReadOnlyMemory<byte> payload, string contentType = "application/octet-stream", IReadOnlyDictionary<string, string>? headers = null, CancellationToken cancellationToken = default) =>
        (await context.EventContext.SendEvent<PostHttpBytesEventResp>(new PostHttpBytesEventReq(endpoint, payload, contentType, headers), cancellationToken)).Payload;

    public static Task<(string Key, uint Expiration)> FetchClientKey(this BotContext context, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().FetchClientKey(cancellationToken);

    public static Task<string> FetchCsrfToken(this BotContext context, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().FetchCsrfToken(cancellationToken);

    public static Task<bool> SetStatus(this BotContext context, uint status) =>
        context.EventContext.GetLogic<OperationLogic>().SetStatus(status);

    public static Task<bool> SetCustomStatus(this BotContext context, uint faceId, string text, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().SetCustomStatus(faceId, text, cancellationToken);

    public static Task DeleteFriend(this BotContext context, long userUin, bool block = false) =>
        context.EventContext.GetLogic<OperationLogic>().DeleteFriend(userUin, block);

    public static Task SetFriendRemark(this BotContext context, long userUin, string remark) =>
        context.EventContext.GetLogic<OperationLogic>().SetFriendRemark(userUin, remark);

    public static Task HandleFriendRequest(this BotContext context, string uidOrFlag, bool approve) =>
        context.EventContext.GetLogic<OperationLogic>().HandleFriendRequest(uidOrFlag, approve);

    public static Task<BotQidianCorpResult> FetchQidianCorp(this BotContext context, long userUin, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().FetchQidianCorp(userUin, cancellationToken);

    public static Task<string> FetchBuddyArk(this BotContext context, long userUin, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().FetchBuddyArk(userUin, cancellationToken);

    public static Task<string> FetchGroupArk(this BotContext context, long groupUin, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().FetchGroupArk(groupUin, cancellationToken);

    public static Task SendTuwenArk(this BotContext context, long targetId, bool group, string title, string description, string summary, string jumpUrl, string previewUrl = "", CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().SendTuwenArk(targetId, group, title, description, summary, jumpUrl, previewUrl, cancellationToken);

    public static Task<IReadOnlyList<BotDoubtFriendRequest>> FetchDoubtFriendRequests(this BotContext context, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().FetchDoubtFriendRequests(cancellationToken);

    public static Task HandleDoubtFriendRequest(this BotContext context, string uid, bool approve, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().HandleDoubtFriendRequest(uid, approve, cancellationToken);

    public static Task SetFriendCategory(this BotContext context, long userUin, int categoryId) =>
        context.EventContext.GetLogic<OperationLogic>().SetFriendCategory(userUin, categoryId);

    public static Task SendLike(this BotContext context, long userUin, uint count = 1, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().SendLike(userUin, count, cancellationToken);

    public static Task<BotProfileLikeResult> GetProfileLike(this BotContext context, long? userUin = null, int start = 0, int limit = 10, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().GetProfileLike(userUin, start, limit, cancellationToken);

    public static Task SetGroupMemberPermission(this BotContext context, long groupUin, string permission, bool allow, uint currentPrivilegeFlag = 0, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().SetGroupMemberPermission(groupUin, permission, allow, currentPrivilegeFlag, cancellationToken);

    public static Task<IReadOnlyList<string>> TranslateEnToZh(this BotContext context, IReadOnlyList<string> words, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().TranslateEnToZh(words, cancellationToken);

    public static Task<BotOcrResult> ImageOcr(this BotContext context, string imageUrl, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().ImageOcr(imageUrl, cancellationToken);

    public static Task<BotEmojiLikesResult> GetEmojiLikes(this BotContext context, long groupUin, ulong sequence, string code, string cookie = "", uint count = 20, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().GetEmojiLikes(groupUin, sequence, code, cookie, count, cancellationToken);

    public static Task<BotGroupReactionSummaryResult> GetGroupReactionSummary(this BotContext context, long groupUin, ulong sequence, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().GetGroupReactionSummary(groupUin, sequence, cancellationToken);

    public static Task SetInputStatus(this BotContext context, long userUin, uint eventType, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().SetInputStatus(userUin, eventType, cancellationToken);

    public static Task<BotUnidirectionalFriendResult> GetUnidirectionalFriendList(this BotContext context, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().GetUnidirectionalFriendList(cancellationToken);

    public static Task RenameGroupFile(this BotContext context, long groupUin, string fileId, string parentDirectory, string newFileName, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().RenameGroupFile(groupUin, fileId, parentDirectory, newFileName, cancellationToken);

    public static Task PublishGroupFile(this BotContext context, long groupUin, string fileId, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().PublishGroupFile(groupUin, fileId, cancellationToken);

    public static Task<BotGroupFileTransferResult> TransferGroupFile(this BotContext context, long groupUin, string fileId, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().TransferGroupFile(groupUin, fileId, cancellationToken);

    public static Task SetProfile(this BotContext context, string? nickname = null, string? personalNote = null, int? sex = null, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().SetProfile(nickname, personalNote, sex, cancellationToken);

    public static Task SetSelfLongNick(this BotContext context, string longNick, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().SetSelfLongNick(longNick, cancellationToken);

    public static Task<BotCustomFaceListResult> GetCustomFaceList(this BotContext context, bool refresh = false, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().GetCustomFaceList(refresh, cancellationToken);

    public static Task<string> AddCustomFace(this BotContext context, Stream image, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().AddCustomFace(image, cancellationToken);

    public static Task SetGroupAvatar(this BotContext context, long groupUin, Stream image, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().SetGroupAvatar(groupUin, image, cancellationToken);

    public static Task<BotCustomFaceDetailResult> GetCustomFaceDetail(this BotContext context, IReadOnlyList<CustomFaceLookup> entries, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().GetCustomFaceDetail(entries, cancellationToken);

    public static Task ModifyCustomFace(this BotContext context, string faceId, string md5, string description, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().ModifyCustomFace(faceId, md5, description, cancellationToken);

    public static Task DeleteCustomFace(this BotContext context, string faceId, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().DeleteCustomFace(faceId, cancellationToken);

    public static Task MoveCustomFace(this BotContext context, string faceId, uint position, IReadOnlyList<CustomFaceLookup> entries, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().MoveCustomFace(faceId, position, entries, cancellationToken);

    public static Task<IReadOnlyList<BotSystemFacePack>> GetSystemFaces(this BotContext context, bool refresh = false, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().GetSystemFaces(refresh, cancellationToken);

    public static Task<IReadOnlyList<BotSystemFace>> SearchSystemFaces(this BotContext context, string query, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().SearchSystemFaces(query, cancellationToken);

    public static Task<BotFlashFilesetResult> ListFlashFilesets(this BotContext context, uint limit = 10, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().ListFlashFilesets(limit, cancellationToken);

    public static Task<BotFlashFilesetResult> GetFlashFileset(this BotContext context, string filesetUuid, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().GetFlashFileset(filesetUuid, cancellationToken);

    public static Task<string> GetFlashFileUrl(this BotContext context, string filesetUuid, string? fileId = null, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().GetFlashFileUrl(filesetUuid, fileId, cancellationToken);

    public static Task<BotFlashCreateResult> CreateFlashTask(this BotContext context, string fileName, ulong fileSize, uint fileType = 7, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().CreateFlashTask(fileName, fileSize, fileType, cancellationToken);

    public static Task CompleteFlashTask(this BotContext context, string filesetUuid, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().CompleteFlashTask(filesetUuid, cancellationToken);

    public static Task CommitFlashFile(this BotContext context, string filesetUuid, string uploadKey, string fileUuid, string fileName, ulong fileSize, uint index = 1, uint formatCode = 26, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().CommitFlashFile(filesetUuid, uploadKey, fileUuid, fileName, fileSize, index, formatCode, cancellationToken);

    public static Task SetFlashTaskStatus(this BotContext context, string filesetUuid, uint status = 6, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().SetFlashTaskStatus(filesetUuid, status, cancellationToken);

    public static Task<List<BotMessage>> GetForwardedMessages(this BotContext context, string resId, bool isGroup = false, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().GetForwardedMessages(resId, isGroup, cancellationToken);

    public static Task DeleteFlashFile(this BotContext context, string filesetUuid, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().DeleteFlashFile(filesetUuid, cancellationToken);

    public static Task RenameFlashFile(this BotContext context, string filesetUuid, string newName, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().RenameFlashFile(filesetUuid, newName, cancellationToken);

    public static Task SendFlashMessage(this BotContext context, long? userUin, long? groupUin, string filesetUuid, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().SendFlashMessage(userUin, groupUin, filesetUuid, cancellationToken);

    public static Task GroupRecallPoke(this BotContext context, ulong groupUin, ulong messageSequence, ulong messageTime, ulong tipsSeqId) => 
        context.EventContext.GetLogic<OperationLogic>().GroupRecallPoke(groupUin, messageSequence, messageTime, tipsSeqId);

    public static Task FriendRecallPoke(this BotContext context, ulong peerUin, ulong messageSequence, ulong messageTime, ulong tipsSeqId) => 
        context.EventContext.GetLogic<OperationLogic>().FriendRecallPoke(peerUin, messageSequence, messageTime, tipsSeqId);

    public static Task<BotGroupClockInResult> GroupClockIn(this BotContext context, long groupUin) =>
        context.EventContext.GetLogic<OperationLogic>().GroupClockIn(groupUin);

    public static Task<List<BotFriend>> FetchFriends(this BotContext context, bool refresh = false) =>
        context.CacheContext.GetFriendList(refresh);

    public static Task<List<BotGroup>> FetchGroups(this BotContext context, bool refresh = false) =>
        context.CacheContext.GetGroupList(refresh);

    public static Task<BotGroupExtra> FetchGroupExtra(this BotContext context, long groupUin, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().FetchGroupExtra(groupUin, cancellationToken);

    public static Task<BotStrangerGroupInfo> FetchStrangerGroupInfo(this BotContext context, ulong groupUin) =>
        context.EventContext.GetLogic<OperationLogic>().FetchStrangerGroupInfo(groupUin);

    public static Task<List<BotGroupMember>> FetchMembers(this BotContext context, long groupUin, bool refresh = false) =>
        context.CacheContext.GetMemberList(groupUin, refresh);

    public static Task<List<BotGroupNotificationBase>> FetchGroupNotifications(this BotContext context, ulong count, ulong start = 0, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().FetchGroupNotifications(count, start, cancellationToken);

    public static Task<List<BotGroupNotificationBase>> FetchFilteredGroupNotifications(this BotContext context, ulong count, ulong start = 0, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().FetchFilteredGroupNotifications(count, start, cancellationToken);

    public static Task<List<BotFriendRequest>> FetchFriendRequests(this BotContext context, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().FetchFriendRequests(cancellationToken);

    public static Task<BotStranger> FetchStranger(this BotContext context, long uin) =>
        context.EventContext.GetLogic<OperationLogic>().FetchStranger(uin);

    public static Task SetGroupNotification(this BotContext context, long groupUin, ulong sequence, BotGroupNotificationType type, bool isFiltered, GroupNotificationOperate operate, string message = "") =>
        context.EventContext.GetLogic<OperationLogic>().SetGroupNotification(groupUin, sequence, type, isFiltered, operate, message);

    public static Task SetGroupReaction(this BotContext context, long groupUin, ulong sequence, string code, bool isAdd, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().SetGroupReaction(groupUin, sequence, code, isAdd, cancellationToken);

    public static Task SetGroupTodo(this BotContext context, long groupUin, ulong sequence) =>
        context.EventContext.GetLogic<OperationLogic>().SetGroupTodo(groupUin, sequence);

    public static Task<BotGetGroupTodoResult> GetGroupTodo(this BotContext context, long groupUin) =>
        context.EventContext.GetLogic<OperationLogic>().GetGroupTodo(groupUin);

    public static Task FinishGroupTodo(this BotContext context, long groupUin) =>
        context.EventContext.GetLogic<OperationLogic>().FinishGroupTodo(groupUin);

    public static Task RemoveGroupTodo(this BotContext context, long groupUin) =>
        context.EventContext.GetLogic<OperationLogic>().RemoveGroupTodo(groupUin);

    public static Task SetPinFriend(this BotContext context, long friendUin, bool isPin, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().SetPinFriend(friendUin, isPin, cancellationToken);

    public static Task SetPinGroup(this BotContext context, long groupUin, bool isPin, CancellationToken cancellationToken = default) =>
        context.EventContext.GetLogic<OperationLogic>().SetPinGroup(groupUin, isPin, cancellationToken);

    public static Task<string> GetNTV2RichMediaUrl(this BotContext context, string fileUuid) =>
        context.EventContext.GetLogic<OperationLogic>().GetNTV2RichMediaUrl(fileUuid);

    public static Task<bool> SetBotAvatar(this BotContext context, Stream stream) =>
        context.HighwayContext.UploadFile(stream, 90, Array.Empty<byte>());
}
