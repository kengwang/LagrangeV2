using System.Buffers;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Internal.Services.Http;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Cryptography;
using Lagrange.Core.Utility.Extension;
using Lagrange.Proto.Primitives;
using Lagrange.Proto.Serialization;
using FileInfo = Lagrange.Core.Internal.Packets.Service.FileInfo;
using Lagrange.Core.Internal.Packets.Web;

namespace Lagrange.Core.Internal.Logic;

internal partial class OperationLogic(BotContext context) : ILogic
{
    private const string Tag = nameof(OperationLogic);
    private readonly SemaphoreSlim _customFaceGate = new(1, 1);
    private BotCustomFaceListResult? _customFaceCache;
    private readonly SemaphoreSlim _systemFaceGate = new(1, 1);
    private IReadOnlyList<BotSystemFacePack>? _systemFaceCache;

    public async Task<Dictionary<string, string>> FetchCookies(List<string> domains, CancellationToken cancellationToken = default) =>
        (await context.EventContext.SendEvent<FetchCookiesEventResp>(new FetchCookiesEventReq(domains), cancellationToken)).Cookies;

    public async Task MarkMessageAsRead(string scene, long peerUin, ulong lastReadSeq, CancellationToken cancellationToken = default)
    {
        await context.EventContext.SendEvent<MarkMessageReadEventResp>(new MarkMessageReadEventReq(scene, peerUin, lastReadSeq), cancellationToken);
    }

    public async Task MarkAllMessagesAsRead(IReadOnlyList<long> groupUins, IReadOnlyList<long> privateUins, CancellationToken cancellationToken = default)
    {
        if (groupUins.Any(x => x <= 0) || privateUins.Any(x => x <= 0)) throw new ArgumentOutOfRangeException(nameof(groupUins), "Read targets must be positive UINs.");
        if (groupUins.Count == 0 && privateUins.Count == 0) return;
        var first = await context.EventContext.SendEvent<MarkAllMessagesReadEventResp>(new MarkAllMessagesReadEventReq(groupUins, privateUins), cancellationToken);
        foreach (var group in first.Groups) await MarkMessageAsRead("group", group.PeerUin, group.LatestSeq, cancellationToken);
        foreach (var peer in first.Privates) await MarkMessageAsRead("private", peer.PeerUin, peer.LatestSeq, cancellationToken);
    }

    public async Task<ulong> GetLatestPrivateMessageSequence(long peerUin, CancellationToken cancellationToken = default)
    {
        if (peerUin <= 0) throw new ArgumentOutOfRangeException(nameof(peerUin));
        var response = await context.EventContext.SendEvent<MarkAllMessagesReadEventResp>(
            new MarkAllMessagesReadEventReq([], [peerUin]), cancellationToken);
        return response.Privates.FirstOrDefault(x => x.PeerUin == peerUin).LatestSeq;
    }

    public async Task SetGroupAddOption(long groupUin, uint addType, string? question, string? answer, CancellationToken cancellationToken = default)
    {
        if (groupUin <= 0) throw new ArgumentOutOfRangeException(nameof(groupUin));
        if (addType > 55) throw new ArgumentOutOfRangeException(nameof(addType));
        await context.EventContext.SendEvent<SetGroupAddOptionEventResp>(new SetGroupAddOptionEventReq(groupUin, addType, question ?? string.Empty, answer ?? string.Empty), cancellationToken);
    }

    public async Task SetGroupSearch(long groupUin, uint? noFingerOpen = null, uint? noCodeFingerOpen = null, CancellationToken cancellationToken = default)
    {
        if (groupUin <= 0) throw new ArgumentOutOfRangeException(nameof(groupUin));
        if (noFingerOpen is null && noCodeFingerOpen is null) throw new ArgumentException("At least one search flag is required.");
        await context.EventContext.SendEvent<SetGroupSearchEventResp>(new SetGroupSearchEventReq(groupUin, noFingerOpen, noCodeFingerOpen), cancellationToken);
    }

    public async Task SetGroupNewMemberHistoryVisibility(long groupUin, bool visible, CancellationToken cancellationToken = default)
    {
        if (groupUin <= 0) throw new ArgumentOutOfRangeException(nameof(groupUin));
        await context.EventContext.SendEvent<SetGroupNewMemberHistoryEventResp>(new SetGroupNewMemberHistoryEventReq(groupUin, visible), cancellationToken);
    }

    public async Task SetGroupInvitePolicy(long groupUin, uint currentPrivilegeFlag, string policy, CancellationToken cancellationToken = default)
    {
        if (groupUin <= 0) throw new ArgumentOutOfRangeException(nameof(groupUin));
        ArgumentException.ThrowIfNullOrWhiteSpace(policy);
        await context.EventContext.SendEvent<SetGroupInvitePolicyEventResp>(new SetGroupInvitePolicyEventReq(groupUin, currentPrivilegeFlag, policy), cancellationToken);
    }

    public async Task SetGroupRobotAddOption(long groupUin, uint? memberSwitch = null, uint? memberExamine = null, CancellationToken cancellationToken = default)
    {
        if (groupUin <= 0) throw new ArgumentOutOfRangeException(nameof(groupUin));
        if (memberSwitch is null && memberExamine is null) throw new ArgumentException("At least one robot option is required.");
        await context.EventContext.SendEvent<SetGroupRobotOptionEventResp>(new SetGroupRobotOptionEventReq(groupUin, memberSwitch, memberExamine), cancellationToken);
    }

    public async Task<BotGroupAdminSettingsResult> GetGroupAdminSettings(long groupUin, CancellationToken cancellationToken = default)
    {
        if (groupUin <= 0) throw new ArgumentOutOfRangeException(nameof(groupUin));
        var settings = (await context.EventContext.SendEvent<FetchGroupAdminSettingsEventResp>(new FetchGroupAdminSettingsEventReq(groupUin), cancellationToken)).Settings;
        var robot = await context.EventContext.SendEvent<FetchGroupRobotOptionEventResp>(new FetchGroupRobotOptionEventReq(groupUin), cancellationToken);
        return new BotGroupAdminSettingsResult { AddType = settings.AddType, GroupQuestion = settings.GroupQuestion, GroupAnswer = settings.GroupAnswer, MemberInvitePolicy = settings.MemberInvitePolicy, NewMemberHistoryVisible = settings.NewMemberHistoryVisible, NoFingerOpen = settings.NoFingerOpen, NoCodeFingerOpen = settings.NoCodeFingerOpen, PrivilegeFlag = settings.PrivilegeFlag, RobotMemberSwitch = robot.MemberSwitch, RobotMemberExamine = robot.MemberExamine };
    }

    public Task<BotPeerPinsResult> GetPeerPins(CancellationToken cancellationToken = default) =>
        throw new OperationException(-1, "Peer pin list has no reliable QQ wire endpoint in the current protocol profile.");

    public async Task<BotGroupAlbumMediaResult> GetGroupAlbumMedia(long groupUin, string albumId, string attachInfo = "", CancellationToken cancellationToken = default)
    {
        var response = await context.EventContext.SendEvent<GetGroupAlbumMediaEventResp>(new GetGroupAlbumMediaEventReq(groupUin, albumId, attachInfo), cancellationToken);
        return response.Result;
    }

    public async Task SetGroupAlbumLike(long groupUin, string albumId, string batchId, string? mediaId, bool isLike, CancellationToken cancellationToken = default)
    {
        await context.EventContext.SendEvent<SetGroupAlbumLikeEventResp>(new SetGroupAlbumLikeEventReq(groupUin, albumId, batchId, mediaId, isLike), cancellationToken);
    }

    public async Task DeleteGroupAlbumMedia(long groupUin, string albumId, string mediaId, string? batchId = null, CancellationToken cancellationToken = default)
    {
        await context.EventContext.SendEvent<DeleteGroupAlbumMediaEventResp>(new DeleteGroupAlbumMediaEventReq(groupUin, albumId, mediaId, batchId), cancellationToken);
    }

    public async Task<string> CommentGroupAlbumMedia(long groupUin, string albumId, string mediaId, string content, CancellationToken cancellationToken = default)
    {
        var response = await context.EventContext.SendEvent<CommentGroupAlbumMediaEventResp>(new CommentGroupAlbumMediaEventReq(groupUin, albumId, mediaId, content), cancellationToken);
        return response.CommentId;
    }

    private static string ComputeBkn(string skey)
    {
        uint hash = 5381;
        foreach (var c in skey) hash += (hash << 5) + c;
        return (hash & 0x7FFFFFFF).ToString();
    }

    public ValueTask<BotSsoPacket> SendPacket(BotSsoPacket packet, RequestType requestType, EncryptType encryptType)
    {
        int sequence = packet.Sequence != 0 ? packet.Sequence : context.ServiceContext.GetNewSequence();
        packet.Sequence = sequence;
        return context.PacketContext.SendPacket(packet, new ServiceAttribute(packet.Command, requestType, encryptType));
    }

    public async Task<(string, uint)> FetchClientKey(CancellationToken cancellationToken = default)
    {
        var result = await context.EventContext.SendEvent<FetchClientKeyEventResp>(new FetchClientKeyEventReq(), cancellationToken);
        return (result.ClientKey, result.Expiration);
    }

    public async Task<string> FetchCsrfToken(CancellationToken cancellationToken = default)
    {
        var cookies = await FetchCookies(["qzone.qq.com"], cancellationToken);
        var pskey = cookies.GetValueOrDefault("qzone.qq.com");
        if (string.IsNullOrWhiteSpace(pskey)) throw new OperationException(-1, "QZone p_skey is unavailable.");
        return ComputeBkn(pskey);
    }

    public async Task<bool> SendNudge(bool isGroup, long peerUin, long targetUin, CancellationToken cancellationToken = default)
    {
        await context.EventContext.SendEvent<NudgeEventResp>(new NudgeEventReq(isGroup, peerUin, targetUin), cancellationToken);
        return true;
    }

    public async Task DeleteFriend(long userUin, bool block)
    {
        await context.EventContext.SendEvent<DeleteFriendEventResp>(new DeleteFriendEventReq(userUin, block));
        await context.CacheContext.GetFriendList(true);
    }

    public async Task SetFriendRemark(long userUin, string remark)
    {
        await context.EventContext.SendEvent<SetFriendRemarkEventResp>(new SetFriendRemarkEventReq(userUin, remark));
        await context.CacheContext.GetFriendList(true);
    }

    public async Task SetGroupAdmin(long groupUin, long userUin, bool enable)
    {
        await context.EventContext.SendEvent<SetGroupAdminEventResp>(new SetGroupAdminEventReq(groupUin, userUin, enable));
        await context.CacheContext.GetMemberList(groupUin, true);
    }

    public async Task HandleFriendRequest(string uidOrFlag, bool approve)
    {
        await context.EventContext.SendEvent<HandleFriendRequestEventResp>(new HandleFriendRequestEventReq(uidOrFlag, approve));
        await context.CacheContext.GetFriendList(true);
    }

    public async Task SetFriendCategory(long userUin, int categoryId)
    {
        await context.EventContext.SendEvent<SetFriendCategoryEventResp>(new SetFriendCategoryEventReq(userUin, categoryId));
        await context.CacheContext.GetFriendList(true);
    }

    public async Task SendLike(long userUin, uint count, CancellationToken cancellationToken = default)
    {
        if (count is 0 or > 20) throw new ArgumentOutOfRangeException(nameof(count), "Like count must be between 1 and 20.");
        await context.EventContext.SendEvent<SendLikeEventResp>(new SendLikeEventReq(userUin, count), cancellationToken);
    }

    public async Task<BotProfileLikeResult> GetProfileLike(long? userUin = null, int start = 0, int limit = 10, CancellationToken cancellationToken = default)
    {
        if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
        if (limit is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(limit));
        return (await context.EventContext.SendEvent<GetProfileLikeEventResp>(new GetProfileLikeEventReq(userUin, start, limit), cancellationToken)).Result;
    }

    public async Task SetGroupMemberPermission(long groupUin, string permission, bool allow, uint currentPrivilegeFlag = 0, CancellationToken cancellationToken = default)
    {
        if (groupUin <= 0) throw new ArgumentOutOfRangeException(nameof(groupUin));
        var mask = permission switch
        {
            "upload_album" => 0x1u,
            "temporary_session" => 0x10000u,
            "create_group" => 0x8000u,
            _ => throw new ArgumentException("Unknown group member permission.", nameof(permission)),
        };
        var flag = allow ? currentPrivilegeFlag & ~mask : currentPrivilegeFlag | mask;
        await context.EventContext.SendEvent<SetGroupMemberPermissionEventResp>(new SetGroupMemberPermissionEventReq(groupUin, flag, mask), cancellationToken);
    }

    public async Task<IReadOnlyList<string>> TranslateEnToZh(IReadOnlyList<string> words, CancellationToken cancellationToken = default)
    {
        if (words.Count == 0) throw new ArgumentException("At least one word is required.", nameof(words));
        return (await context.EventContext.SendEvent<TranslateEnToZhEventResp>(new TranslateEnToZhEventReq(words), cancellationToken)).Words;
    }

    public async Task<BotOcrResult> ImageOcr(string imageUrl, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out _)) throw new ArgumentException("A valid image URL is required.", nameof(imageUrl));
        return (await context.EventContext.SendEvent<ImageOcrEventResp>(new ImageOcrEventReq(imageUrl), cancellationToken)).Result;
    }

    public async Task<BotEmojiLikesResult> GetEmojiLikes(long groupUin, ulong sequence, string code, string cookie = "", uint count = 20, CancellationToken cancellationToken = default)
    {
        var response = await context.EventContext.SendEvent<GetEmojiLikesEventResp>(new GetEmojiLikesEventReq(groupUin, sequence, code, cookie, count), cancellationToken);
        return response.Result;
    }

    public async Task<BotGroupReactionSummaryResult> GetGroupReactionSummary(long groupUin, ulong sequence, CancellationToken cancellationToken = default)
    {
        var response = await context.EventContext.SendEvent<GetGroupReactionSummaryEventResp>(new GetGroupReactionSummaryEventReq(groupUin, sequence), cancellationToken);
        return response.Result;
    }

    public async Task SetInputStatus(long userUin, uint eventType, CancellationToken cancellationToken = default) =>
        await context.EventContext.SendEvent<SetInputStatusEventResp>(new SetInputStatusEventReq(userUin, eventType), cancellationToken);

    public async Task<BotUnidirectionalFriendResult> GetUnidirectionalFriendList(CancellationToken cancellationToken = default)
    {
        var response = await context.EventContext.SendEvent<GetUnidirectionalFriendListEventResp>(new GetUnidirectionalFriendListEventReq(), cancellationToken);
        return response.Result;
    }

    public Task GroupRecallPoke(ulong groupUin, ulong messageSequence, ulong messageTime, ulong tipsSeqId) =>
        RecallPoke(true, groupUin, messageSequence, messageTime, tipsSeqId);

    public Task FriendRecallPoke(ulong peerUin, ulong messageSequence, ulong messageTime, ulong tipsSeqId) =>
        RecallPoke(false, peerUin, messageSequence, messageTime, tipsSeqId);

    private async Task RecallPoke(bool isGroup, ulong peerUin, ulong messageSequence, ulong messageTime, ulong tipsSeqId) =>
        await context.EventContext.SendEvent<RecallPokeEventResp>(new RecallPokeEventReq(isGroup, peerUin, messageSequence, messageTime, tipsSeqId));

    public async Task<bool> SetStatus(uint status)
    {
        if (status > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(status));

        var result = await context.EventContext.SendEvent<SetStatusEventResp>(new SetStatusEventReq((int)status, 0, 0));
        return result.Success;
    }

    public async Task<bool> SetCustomStatus(uint faceId, string text, CancellationToken cancellationToken = default)
    {
        var result = await context.EventContext.SendEvent<SetStatusEventResp>(new SetStatusEventReq(10, 2000, 0, new SetStatusCustomExtEvent(faceId, text, 1)), cancellationToken);
        return result.Success;
    }

    public async Task GroupRename(long groupUin, string name)
    {
        await context.EventContext.SendEvent<GroupRenameEventResp>(new GroupRenameEventReq(groupUin, name));
    }

    public async Task RemarkGroup(long groupUin, string remark)
    {
        await context.EventContext.SendEvent<GroupRemarkEventResp>(new GroupRemarkEventReq(groupUin, remark));
    }

    public async Task<BotGroupClockInResult> GroupClockIn(long groupUin)
    {
        var response = await context.EventContext.SendEvent<GroupClockInEventResp>(new GroupClockInEventReq(groupUin));
        return response.Result;
    }

    public async Task<bool> MuteGroupGlobal(long groupUin, bool isMute)
    {
        await context.EventContext.SendEvent<GroupMuteGlobalEventResp>(new GroupMuteGlobalEventReq(groupUin, isMute));
        return true;
    }

    public async Task<bool> MuteGroupMember(long groupUin, long targetUin, uint duration)
    {
        if (context.CacheContext.ResolveCachedUid(targetUin) is not { } uid)
        {
            await context.CacheContext.GetMemberList(groupUin, true);
            uid = context.CacheContext.ResolveCachedUid(targetUin);
        }

        if (uid == null) return false;

        await context.EventContext.SendEvent<GroupMuteMemberEventResp>(new GroupMuteMemberEventReq(groupUin, uid, duration));
        return true;
    }

    public async Task<bool> GroupTransfer(long groupUin, long targetUin)
    {
        await context.EventContext.SendEvent<GroupTransferEventResp>(new GroupTransferEventReq(groupUin, targetUin));
        return true;
    }

    public async Task<(uint RemainAtAllCountForUin, uint RemainAtAllCountForGroup)> GroupRemainAtAll(long groupUin)
    {
        var response = await context.EventContext.SendEvent<FetchGroupAtAllRemainEventResp>(new FetchGroupAtAllRemainEventReq(groupUin));
        return (response.RemainAtAllCountForUin, response.RemainAtAllCountForGroup);
    }

    public async Task GroupSetSpecialTitle(long groupUin, long targetUin, string title)
    {
        if (context.CacheContext.ResolveCachedUid(targetUin) is not { } uid)
        {
            await context.CacheContext.GetMemberList(groupUin, true);
            uid = context.CacheContext.ResolveCachedUid(targetUin) ?? throw new InvalidTargetException(targetUin);
        }
        await context.EventContext.SendEvent<GroupSetSpecialTitleEventResp>(new GroupSetSpecialTitleEventReq(groupUin, uid, title));
    }

    public async Task GroupMemberRename(long groupUin, long targetUin, string name)
    {
        if (context.CacheContext.ResolveCachedUid(targetUin) is not { } uid)
        {
            await context.CacheContext.GetMemberList(groupUin, true);
            uid = context.CacheContext.ResolveCachedUid(targetUin) ?? throw new InvalidTargetException(targetUin);
        }
        await context.EventContext.SendEvent<GroupMemberRenameEventResp>(new GroupMemberRenameEventReq(groupUin, uid, name));
    }

    public async Task<bool> KickGroupMember(long groupUin, long targetUin, bool rejectAddRequest, string reason)
    {
        if (context.CacheContext.ResolveCachedUid(targetUin) is not { } uid)
        {
            await context.CacheContext.GetMemberList(groupUin, true);
            uid = context.CacheContext.ResolveCachedUid(targetUin);
        }

        if (uid == null) return false;

        var response = await context.EventContext.SendEvent<GroupKickMemberEventResp>(new GroupKickMemberEventReq(groupUin, uid, rejectAddRequest, reason));

        return response.ResultCode == 0;
    }

    public async Task GroupQuit(long groupUin)
    {
        await context.EventContext.SendEvent<GroupQuitEventResp>(new GroupQuitEventReq(groupUin));
    }

    public async Task SetGroupTodo(long groupUin, ulong sequence)
    {
        await context.EventContext.SendEvent<GroupSetTodoEventResp>(new GroupSetTodoEventReq(groupUin, sequence));
    }

    public async Task<BotGetGroupTodoResult> GetGroupTodo(long groupUin)
    {
        var response = await context.EventContext.SendEvent<GroupGetTodoEventResp>(new GroupGetTodoEventReq(groupUin));
        return response.Result;
    }

    public async Task FinishGroupTodo(long groupUin)
    {
        await context.EventContext.SendEvent<GroupFinishTodoEventResp>(new GroupFinishTodoEventReq(groupUin));
    }

    public async Task RemoveGroupTodo(long groupUin)
    {
        await context.EventContext.SendEvent<GroupRemoveTodoEventResp>(new GroupRemoveTodoEventReq(groupUin));
    }

    public async Task SetPinFriend(long friendUin, bool isPin, CancellationToken cancellationToken = default)
    {
        var friend = await context.CacheContext.ResolveFriend(friendUin) ?? throw new InvalidTargetException(friendUin);
        await context.EventContext.SendEvent<SetPinFriendEventResp>(new SetPinFriendEventReq(friend.Uid, isPin), cancellationToken);
        context.EventInvoker.PostEvent(new BotPeerPinChangeEvent("friend", friendUin, isPin));
    }

    public async Task SetPinGroup(long groupUin, bool isPin, CancellationToken cancellationToken = default)
    {
        await context.EventContext.SendEvent<SetPinGroupEventResp>(new SetPinGroupEventReq(groupUin, isPin), cancellationToken);
        context.EventInvoker.PostEvent(new BotPeerPinChangeEvent("group", groupUin, isPin));
    }

    public async Task<string> GroupFSDownload(long groupUin, string fileId, CancellationToken cancellationToken = default)
    {
        var request = new GroupFSDownloadEventReq(groupUin, fileId);
        var response = await context.EventContext.SendEvent<GroupFSDownloadEventResp>(request, cancellationToken);
        return response.FileUrl;
    }

    public async Task<ulong> FetchGroupFSSpace(long groupUin, CancellationToken cancellationToken = default)
    {
        var result = await FetchGroupFSSpaceInfo(groupUin, cancellationToken);
        return result.TotalSpace >= result.UsedSpace ? result.TotalSpace - result.UsedSpace : 0;
    }

    public async Task<(ulong UsedSpace, ulong TotalSpace)> FetchGroupFSSpaceInfo(long groupUin, CancellationToken cancellationToken = default)
    {
        var response = await context.EventContext.SendEvent<GroupFSSpaceEventResp>(new GroupFSSpaceEventReq(groupUin), cancellationToken);
        if (response.ResultCode != 0) throw new OperationException(response.ResultCode, response.RetMsg);

        return (response.UsedSpace, response.TotalSpace);
    }

    public async Task<uint> FetchGroupFSCount(long groupUin, CancellationToken cancellationToken = default)
    {
        var response = await context.EventContext.SendEvent<GroupFSCountEventResp>(new GroupFSCountEventReq(groupUin), cancellationToken);
        if (response.ResultCode != 0) throw new OperationException(response.ResultCode, response.RetMsg);

        return response.FileCount;
    }

    public async Task<List<IBotFSEntry>> FetchGroupFSList(long groupUin, string targetDirectory, CancellationToken cancellationToken = default)
    {
        const uint fileCount = 20;
        uint startIndex = 0;
        var entries = new List<IBotFSEntry>();

        while (true)
        {
            var response = await context.EventContext.SendEvent<GroupFSListEventResp>(new GroupFSListEventReq(groupUin, targetDirectory, startIndex, fileCount), cancellationToken);

            if (response.ResultCode != 0) throw new OperationException(response.ResultCode, response.RetMsg);

            entries.AddRange(response.FileEntries);
            if (response.IsEnd) break;
            startIndex += fileCount;
        }

        return entries;
    }

    public async Task GroupFSMove(long groupUin, string fileId, string parentDirectory, string targetDirectory, CancellationToken cancellationToken = default) => await context.EventContext.SendEvent<GroupFSMoveEventResp>(new GroupFSMoveEventReq(groupUin, fileId, parentDirectory, targetDirectory), cancellationToken);

    public async Task GroupFSDelete(long groupUin, string fileId, CancellationToken cancellationToken = default) => await context.EventContext.SendEvent<GroupFSDeleteEventResp>(new GroupFSDeleteEventReq(groupUin, fileId), cancellationToken);

    public async Task<string> GroupFSCreateFolder(long groupUin, string name, string parentFolderId = "/", CancellationToken cancellationToken = default)
    {
        var result = await context.EventContext.SendEvent<GroupFSCreateFolderEventResp>(new GroupFSCreateFolderEventReq(groupUin, name, parentFolderId), cancellationToken);
        if (string.IsNullOrWhiteSpace(result.FolderId)) throw new OperationException(-1, "Group folder creation response is missing folder id.");
        return result.FolderId;
    }

    public async Task GroupFSDeleteFolder(long groupUin, string folderId, CancellationToken cancellationToken = default)
    {
        await context.EventContext.SendEvent<GroupFSDeleteFolderEventResp>(new GroupFSDeleteFolderEventReq(groupUin, folderId), cancellationToken);
    }

    public async Task GroupFSRenameFolder(long groupUin, string folderId, string newFolderName, CancellationToken cancellationToken = default)
    {
        await context.EventContext.SendEvent<GroupFSRenameFolderEventResp>(new GroupFSRenameFolderEventReq(groupUin, folderId, newFolderName), cancellationToken);
    }

    public async Task<(ulong, long)> SendFriendFile(long targetUin, Stream fileStream, string? fileName, CancellationToken cancellationToken = default)
    {
        fileName = ResolveFileName(fileStream, fileName);

        var friend = await context.CacheContext.ResolveFriend(targetUin) ?? throw new InvalidTargetException(targetUin);

        var buffer = ArrayPool<byte>.Shared.Rent(10002432);
        int payload = await fileStream.ReadAsync(buffer.AsMemory(0, 10002432), cancellationToken);
        var md510m = MD5.HashData(buffer[..payload]);
        ArrayPool<byte>.Shared.Return(buffer);
        fileStream.Seek(0, SeekOrigin.Begin);

        var request = new FileUploadEventReq(friend.Uid, fileStream, fileName, md510m);
        var result = await context.EventContext.SendEvent<FileUploadEventResp>(request, cancellationToken);


        if (!result.IsExist)
        {
            var ext = new FileUploadExt
            {
                Unknown1 = 100,
                Unknown2 = 1,
                Entry = new FileUploadEntry
                {
                    BusiBuff = new ExcitingBusiInfo { SenderUin = context.Keystore.Uin },
                    FileEntry = new ExcitingFileEntry
                    {
                        FileSize = fileStream.Length,
                        Md5 = request.FileMd5,
                        CheckKey = request.FileSha1,
                        Md510M = md510m,
                        Sha3 = TriSha1Provider.CalculateTriSha1(fileStream),
                        FileId = result.FileId,
                        UploadKey = result.UploadKey
                    },
                    ClientInfo = new ExcitingClientInfo
                    {
                        ClientType = 3,
                        AppId = "100",
                        TerminalType = 3,
                        ClientVer = "1.1.1",
                        Unknown = 4
                    },
                    FileNameInfo = new ExcitingFileNameInfo { FileName = fileName },
                    Host = new ExcitingHostConfig
                    {
                        Hosts = result.RtpMediaPlatformUploadAddress.Select(x => new ExcitingHostInfo
                        {
                            Url = new ExcitingUrlInfo { Unknown = 1, Host = x.Item1 },
                            Port = x.Item2
                        }).ToList()
                    }
                },
                Unknown200 = 1
            };

            bool success = await context.HighwayContext.UploadFile(fileStream, 95, ProtoHelper.Serialize(ext), cancellationToken);
            if (!success) throw new OperationException(-1, "File upload failed");
        }

        ulong sequence = (ulong)Random.Shared.NextInt64(10000, 99999);
        uint random = (uint)Random.Shared.Next();
        var sendResult = await context.EventContext.SendEvent<SendMessageEventResp>(new SendFriendFileEventReq(friend, request, result, sequence, random), cancellationToken);
        if (sendResult.Result != 0) throw new OperationException(sendResult.Result);

        return (sequence, sendResult.SendTime);
    }

    private static string ResolveFileName(Stream fileStream, string? fileName)
    {
        if (fileName == null)
        {
            if (fileStream is FileStream file)
            {
                fileName = Path.GetFileName(file.Name);
            }
            else
            {
                Span<byte> bytes = stackalloc byte[16];
                Random.Shared.NextBytes(bytes);
                fileName = Convert.ToHexString(bytes);
            }
        }

        return fileName;
    }

    public async Task<string> SendGroupFile(long groupUin, Stream fileStream, string? fileName, string parentDirectory, CancellationToken cancellationToken = default)
    {
        fileName = ResolveFileName(fileStream, fileName);

        var md5 = fileStream.Md5();
        var request = new GroupFSUploadEventReq(groupUin, fileName, fileStream, parentDirectory, md5);
        var uploadResp = await context.EventContext.SendEvent<GroupFSUploadEventResp>(request, cancellationToken);

        var buffer = ArrayPool<byte>.Shared.Rent(10002432);
        int payload = await fileStream.ReadAsync(buffer.AsMemory(0, 10002432), cancellationToken);
        var md510m = MD5.HashData(buffer[..payload]);
        ArrayPool<byte>.Shared.Return(buffer);
        fileStream.Seek(0, SeekOrigin.Begin);

        if (!uploadResp.FileExist)
        {
            var ext = new FileUploadExt
            {
                Unknown1 = 100,
                Unknown2 = 1,
                Entry = new FileUploadEntry
                {
                    BusiBuff = new ExcitingBusiInfo
                    {
                        SenderUin = context.Keystore.Uin,
                        ReceiverUin = groupUin,
                        GroupCode = groupUin
                    },
                    FileEntry = new ExcitingFileEntry
                    {
                        FileSize = fileStream.Length,
                        Md5 = md5,
                        CheckKey = uploadResp.FileKey,
                        Md510M = md510m,
                        FileId = uploadResp.FileId,
                        UploadKey = uploadResp.CheckKey
                    },
                    ClientInfo = new ExcitingClientInfo
                    {
                        ClientType = 3,
                        AppId = "100",
                        TerminalType = 3,
                        ClientVer = "1.1.1",
                        Unknown = 4
                    },
                    FileNameInfo = new ExcitingFileNameInfo { FileName = fileName },
                    Host = new ExcitingHostConfig
                    {
                        Hosts =
                        [
                            new ExcitingHostInfo
                            {
                                Url = new ExcitingUrlInfo { Unknown = 1, Host = uploadResp.Addr.ip },
                                Port = uploadResp.Addr.uploadPort
                            }
                        ]
                    }
                }
            };

            bool success = await context.HighwayContext.UploadFile(fileStream, 71, ProtoHelper.Serialize(ext), cancellationToken);
            if (!success) throw new OperationException(-1, "File upload failed");
        }

        uint random = (uint)Random.Shared.Next();
        var feedResult = await context.EventContext.SendEvent<GroupFileSendEventResp>(new GroupFileSendEventReq(groupUin, uploadResp.FileId, random), cancellationToken);
        if (feedResult.RetCode != 0) throw new OperationException(feedResult.RetCode, feedResult.RetMsg);

        return uploadResp.FileId;
    }

    public async Task RenameGroupFile(long groupUin, string fileId, string parentDirectory, string newFileName, CancellationToken cancellationToken = default) =>
        await context.EventContext.SendEvent<GroupFSRenameEventResp>(new GroupFSRenameEventReq(groupUin, fileId, parentDirectory, newFileName), cancellationToken);

    public async Task PublishGroupFile(long groupUin, string fileId, CancellationToken cancellationToken = default)
    {
        var response = await context.EventContext.SendEvent<GroupFileSendEventResp>(new GroupFileSendEventReq(groupUin, fileId, (uint)Random.Shared.Next()), cancellationToken);
        if (response.RetCode != 0) throw new OperationException(response.RetCode, response.RetMsg);
    }

    public async Task<BotGroupFileTransferResult> TransferGroupFile(long groupUin, string fileId, CancellationToken cancellationToken = default)
    {
        var response = await context.EventContext.SendEvent<GroupFSTransferEventResp>(new GroupFSTransferEventReq(groupUin, fileId), cancellationToken);
        return new BotGroupFileTransferResult { SaveBusId = response.SaveBusId, SaveFilePath = response.SaveFilePath };
    }

    public async Task SetProfile(string? nickname, string? personalNote, int? sex, CancellationToken cancellationToken = default)
    {
        if (nickname is null && personalNote is null && sex is null) return;
        await context.EventContext.SendEvent<SetProfileEventResp>(new SetProfileEventReq(nickname, personalNote, sex), cancellationToken);
    }

    public async Task SetSelfLongNick(string longNick, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(longNick)) throw new ArgumentException("Long nick cannot be empty.", nameof(longNick));
        await context.EventContext.SendEvent<SetSelfLongNickEventResp>(new SetSelfLongNickEventReq(longNick), cancellationToken);
    }

    public async Task<BotCustomFaceListResult> GetCustomFaceList(bool refresh = false, CancellationToken cancellationToken = default)
    {
        if (!refresh && _customFaceCache is not null) return _customFaceCache;
        await _customFaceGate.WaitAsync(cancellationToken);
        try
        {
            if (!refresh && _customFaceCache is not null) return _customFaceCache;
            var response = await context.EventContext.SendEvent<GetCustomFaceListEventResp>(new GetCustomFaceListEventReq(), cancellationToken);
            return _customFaceCache = response.Result;
        }
        finally { _customFaceGate.Release(); }
    }

    public async Task<BotCustomFaceDetailResult> GetCustomFaceDetail(IReadOnlyList<CustomFaceLookup> entries, CancellationToken cancellationToken = default)
    {
        var response = await context.EventContext.SendEvent<GetCustomFaceDetailEventResp>(new GetCustomFaceDetailEventReq(entries), cancellationToken);
        return response.Result;
    }

    public async Task ModifyCustomFace(string faceId, string md5, string description, CancellationToken cancellationToken = default)
    {
        await context.EventContext.SendEvent<ModifyCustomFaceEventResp>(new ModifyCustomFaceEventReq(faceId, md5, description), cancellationToken);
        _customFaceCache = null;
    }

    public async Task DeleteCustomFace(string faceId, CancellationToken cancellationToken = default)
    {
        await context.EventContext.SendEvent<DeleteCustomFaceEventResp>(new DeleteCustomFaceEventReq(faceId), cancellationToken);
        _customFaceCache = null;
    }

    public async Task MoveCustomFace(string faceId, uint position, IReadOnlyList<CustomFaceLookup> entries, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(faceId) || entries.Count == 0) throw new ArgumentException("Custom face move requires a face id and a non-empty order.");
        if (position == 0 || position > entries.Count) throw new ArgumentOutOfRangeException(nameof(position));
        await context.EventContext.SendEvent<MoveCustomFaceEventResp>(new MoveCustomFaceEventReq(faceId, position, entries), cancellationToken);
        await context.EventContext.SendEvent<MoveCustomFaceOrderEventResp>(new MoveCustomFaceOrderEventReq(entries), cancellationToken);
        _customFaceCache = null;
    }

    public async Task<IReadOnlyList<BotSystemFacePack>> GetSystemFaces(bool refresh = false, CancellationToken cancellationToken = default)
    {
        if (!refresh && _systemFaceCache is not null) return _systemFaceCache;
        await _systemFaceGate.WaitAsync(cancellationToken);
        try
        {
            if (!refresh && _systemFaceCache is not null) return _systemFaceCache;
            var response = await context.EventContext.SendEvent<GetSystemFacesEventResp>(new GetSystemFacesEventReq(refresh), cancellationToken);
            return _systemFaceCache = response.Packs;
        }
        finally { _systemFaceGate.Release(); }
    }

    public async Task<IReadOnlyList<BotSystemFace>> SearchSystemFaces(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query)) throw new ArgumentException("System face search query is required.", nameof(query));
        var faces = (await GetSystemFaces(false, cancellationToken)).SelectMany(x => x.Faces);
        return faces.Where(face => face.Sid.Contains(query, StringComparison.OrdinalIgnoreCase)
            || face.Description.Contains(query, StringComparison.OrdinalIgnoreCase)
            || face.EmCode.Contains(query, StringComparison.OrdinalIgnoreCase)
            || face.Aliases.Any(alias => alias.Contains(query, StringComparison.OrdinalIgnoreCase))).ToArray();
    }

    public async Task<BotFlashFilesetResult> ListFlashFilesets(uint limit = 10, CancellationToken cancellationToken = default)
    {
        if (limit == 0 || limit > 100) throw new ArgumentOutOfRangeException(nameof(limit));
        return (await context.EventContext.SendEvent<ListFlashFilesetsEventResp>(new ListFlashFilesetsEventReq(limit), cancellationToken)).Result;
    }

    public async Task<BotFlashFilesetResult> GetFlashFileset(string filesetUuid, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filesetUuid)) throw new ArgumentException("Fileset UUID is required.", nameof(filesetUuid));
        return (await context.EventContext.SendEvent<GetFlashFilesetEventResp>(new GetFlashFilesetEventReq(filesetUuid), cancellationToken)).Result;
    }

    public async Task<string> GetFlashFileUrl(string filesetUuid, string? fileId = null, CancellationToken cancellationToken = default)
    {
        var result = await GetFlashFileset(filesetUuid, cancellationToken);
        var entry = fileId is null ? result.Entries.FirstOrDefault() : result.Entries.FirstOrDefault(x => x.FileId == fileId);
        return entry?.DownloadUrl ?? throw new OperationException(-1, "Flash file download URL is unavailable.");
    }

    public async Task<BotFlashCreateResult> CreateFlashTask(string fileName, ulong fileSize, uint fileType = 7, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name is required.", nameof(fileName));
        if (fileSize == 0) throw new ArgumentOutOfRangeException(nameof(fileSize));
        return (await context.EventContext.SendEvent<CreateFlashTaskEventResp>(new CreateFlashTaskEventReq(fileName, fileSize, fileType), cancellationToken)).Result;
    }

    public async Task CompleteFlashTask(string filesetUuid, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filesetUuid)) throw new ArgumentException("Fileset UUID is required.", nameof(filesetUuid));
        await context.EventContext.SendEvent<CompleteFlashTaskEventResp>(new CompleteFlashTaskEventReq(filesetUuid), cancellationToken);
    }

    public async Task CommitFlashFile(string filesetUuid, string uploadKey, string fileUuid, string fileName, ulong fileSize, uint index = 1, uint formatCode = 26, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filesetUuid) || string.IsNullOrWhiteSpace(uploadKey) || string.IsNullOrWhiteSpace(fileUuid)) throw new ArgumentException("Flash file identifiers are required.");
        if (string.IsNullOrWhiteSpace(fileName) || fileSize == 0) throw new ArgumentException("File metadata is invalid.");
        if (index == 0) throw new ArgumentOutOfRangeException(nameof(index));
        await context.EventContext.SendEvent<CommitFlashFileEventResp>(new CommitFlashFileEventReq(filesetUuid, uploadKey, fileUuid, fileName, fileSize, index, formatCode), cancellationToken);
    }

    public async Task SetFlashTaskStatus(string filesetUuid, uint status = 6, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filesetUuid)) throw new ArgumentException("Fileset UUID is required.", nameof(filesetUuid));
        await context.EventContext.SendEvent<SetFlashTaskStatusEventResp>(new SetFlashTaskStatusEventReq(filesetUuid, status), cancellationToken);
    }

    public async Task<List<BotMessage>> GetForwardedMessages(string resId, bool isGroup = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resId)) throw new ArgumentException("Forward resource ID is required.", nameof(resId));
        return (await context.EventContext.SendEvent<LongMsgRecvEventResp>(new LongMsgRecvEventReq(isGroup, resId), cancellationToken)).Messages;
    }

    public async Task DeleteFlashFile(string filesetUuid, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filesetUuid)) throw new ArgumentException("Fileset UUID is required.", nameof(filesetUuid));
        await context.EventContext.SendEvent<DeleteFlashFileEventResp>(new DeleteFlashFileEventReq(filesetUuid), cancellationToken);
    }

    public async Task RenameFlashFile(string filesetUuid, string newName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filesetUuid)) throw new ArgumentException("Fileset UUID is required.", nameof(filesetUuid));
        if (string.IsNullOrWhiteSpace(newName)) throw new ArgumentException("New name is required.", nameof(newName));
        await context.EventContext.SendEvent<RenameFlashFileEventResp>(new RenameFlashFileEventReq(filesetUuid, newName), cancellationToken);
    }

    public async Task SendFlashMessage(long? userUin, long? groupUin, string filesetUuid, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filesetUuid)) throw new ArgumentException("Fileset UUID is required.", nameof(filesetUuid));
        if (userUin is null == (groupUin is null)) throw new ArgumentException("Exactly one target is required.");
        await context.EventContext.SendEvent<SendFlashMessageEventResp>(new SendFlashMessageEventReq(userUin, groupUin, filesetUuid), cancellationToken);
    }

    public async Task<BotGroupExtra> FetchGroupExtra(long groupUin, CancellationToken cancellationToken = default)
    {
        if (groupUin <= 0) throw new ArgumentOutOfRangeException(nameof(groupUin));
        var req = new FetchGroupExtraEventReq(groupUin);
        var resp = await context.EventContext.SendEvent<FetchGroupExtraEventResp>(req, cancellationToken);
        return resp.Extra;
    }

    public async Task<BotStrangerGroupInfo> FetchStrangerGroupInfo(ulong groupUin)
    {
        var response = await context.EventContext.SendEvent<FetchStrangerGroupInfoEventResp>(new FetchStrangerGroupInfoEventReq(groupUin));
        return response.Info;
    }

    public async Task<List<BotGroupNotificationBase>> FetchGroupNotifications(ulong count, ulong start, CancellationToken cancellationToken = default)
    {
        var req = new FetchGroupNotificationsEventReq(count, start);
        var resp = await context.EventContext.SendEvent<FetchGroupNotificationsEventResp>(req, cancellationToken);
        return resp.GroupNotifications;
    }

    public async Task<List<BotGroupNotificationBase>> FetchFilteredGroupNotifications(ulong count, ulong start, CancellationToken cancellationToken = default)
    {
        var req = new FetchFilteredGroupNotificationsEventReq(count, start);
        var resp = await context.EventContext.SendEvent<FetchFilteredGroupNotificationsEventResp>(req, cancellationToken);
        return resp.GroupNotifications;
    }

    public async Task<List<BotFriendRequest>> FetchFriendRequests(CancellationToken cancellationToken = default)
    {
        var response = await context.EventContext.SendEvent<FetchFriendRequestsEventResp>(new FetchFriendRequestsEventReq(), cancellationToken);
        return response.Requests;
    }

    public async Task<BotQidianCorpResult> FetchQidianCorp(long userUin, CancellationToken cancellationToken = default)
    {
        var response = await context.EventContext.SendEvent<FetchQidianCorpEventResp>(new FetchQidianCorpEventReq(userUin), cancellationToken);
        return response.Result;
    }

    public async Task<string> FetchBuddyArk(long userUin, CancellationToken cancellationToken = default)
    {
        var response = await context.EventContext.SendEvent<FetchBuddyArkEventResp>(new FetchBuddyArkEventReq(userUin), cancellationToken);
        return response.Ark;
    }

    public async Task<string> FetchGroupArk(long groupUin, CancellationToken cancellationToken = default)
    {
        var response = await context.EventContext.SendEvent<FetchGroupArkEventResp>(new FetchGroupArkEventReq(groupUin), cancellationToken);
        return response.Ark;
    }

    public async Task SendTuwenArk(long targetId, bool group, string title, string description, string summary, string jumpUrl, string previewUrl = "", CancellationToken cancellationToken = default)
    {
        await context.EventContext.SendEvent<SendTuwenArkEventResp>(new SendTuwenArkEventReq(targetId, group, title, description, summary, jumpUrl, previewUrl), cancellationToken);
    }

    public async Task<IReadOnlyList<BotDoubtFriendRequest>> FetchDoubtFriendRequests(CancellationToken cancellationToken = default)
    {
        var response = await context.EventContext.SendEvent<FetchDoubtFriendRequestsEventResp>(new FetchDoubtFriendRequestsEventReq(), cancellationToken);
        return response.Requests;
    }

    public async Task HandleDoubtFriendRequest(string uid, bool approve, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(uid)) throw new ArgumentException("Request uid is required.", nameof(uid));
        if (approve)
            await context.EventContext.SendEvent<HandleDoubtApprovalEventResp>(new HandleDoubtApprovalEventReq(uid), cancellationToken);
        else
            await context.EventContext.SendEvent<HandleDoubtDeleteEventResp>(new HandleDoubtDeleteEventReq(uid), cancellationToken);
    }

    public async Task<BotStranger> FetchStranger(long uid)
    {
        var req = new FetchStrangerByUinEventReq(uid);
        var resp = await context.EventContext.SendEvent<FetchStrangerEventResp>(req);
        return resp.Stranger;
    }

    public async Task SetGroupNotification(long groupUin, ulong sequence, BotGroupNotificationType type, bool isFiltered, GroupNotificationOperate operate, string message)
    {
        if (isFiltered)
        {
            await context.EventContext.SendEvent<SetFilteredGroupNotificationEventResp>(
                new SetFilteredGroupNotificationEventReq(
                    groupUin,
                    sequence,
                    type,
                    operate,
                    message
                )
            );
        }
        else
        {
            await context.EventContext.SendEvent<SetGroupNotificationEventResp>(
                new SetGroupNotificationEventReq(
                    groupUin,
                    sequence,
                    type,
                    operate,
                    message
                )
            );
        }
    }

    public async Task SetGroupReaction(long groupUin, ulong sequence, string code, bool isAdd, CancellationToken cancellationToken = default)
    {
        if (isAdd) await context.EventContext.SendEvent<AddGroupReactionEventResp>(
            new AddGroupReactionEventReq(groupUin, sequence, code)
        , cancellationToken);
        else await context.EventContext.SendEvent<ReduceGroupReactionEventResp>(
            new ReduceGroupReactionEventReq(groupUin, sequence, code)
        , cancellationToken);
    }

    public async Task<string> GetNTV2RichMediaUrl(string uuid)
    {
        int remainder = uuid.Length % 4;
        int length = remainder == 0 ? uuid.Length : uuid.Length + (4 - remainder);
        string base64 = uuid.Replace('-', '+').Replace('_', '/').PadRight(length, '=');

        ulong appId = 0;
        uint ttl = 0;
        var reader = new ProtoReader(Convert.FromBase64String(base64));
        while (!reader.IsCompleted)
        {
            uint tag = reader.DecodeVarIntUnsafe<uint>();
            switch (tag >>> 3)
            {
                case 4:
                {
                    appId = reader.DecodeVarInt<ulong>();
                    break;
                }
                case 10:
                {
                    ttl = reader.DecodeVarInt<uint>();
                    break;
                }
                default:
                {
                    reader.SkipField((WireType)(tag & 0b111));
                    break;
                }
            }
        }

        NTV2RichMediaDownloadEventResp response = appId switch
        {
            // Record
            1402 => await context.EventContext.SendEvent<RecordDownloadEventResp>(new RecordDownloadEventReq(
                CreateFakeFriendMessage(context.BotUin),
                CreateFakeEntity<RecordEntity>(uuid, ttl)
            )),
            1403 => await context.EventContext.SendEvent<RecordGroupDownloadEventResp>(new RecordGroupDownloadEventReq(
                CreateFakeGroupMessage(context.BotUin),
                CreateFakeEntity<RecordEntity>(uuid, ttl)
            )),
            // Video
            1413 => await context.EventContext.SendEvent<VideoDownloadEventResp>(new VideoDownloadEventReq(
                CreateFakeFriendMessage(context.BotUin),
                CreateFakeEntity<VideoEntity>(uuid, ttl)
            )),
            1415 => await context.EventContext.SendEvent<VideoGroupDownloadEventResp>(new VideoGroupDownloadEventReq(
                CreateFakeGroupMessage(context.BotUin),
                CreateFakeEntity<VideoEntity>(uuid, ttl)
            )),
            // Image
            1406 => await context.EventContext.SendEvent<ImageDownloadEventResp>(new ImageDownloadEventReq(
                CreateFakeFriendMessage(context.BotUin),
                CreateFakeEntity<ImageEntity>(uuid, ttl)
            )),
            1407 => await context.EventContext.SendEvent<ImageGroupDownloadEventResp>(new ImageGroupDownloadEventReq(
                CreateFakeGroupMessage(context.BotUin),
                CreateFakeEntity<ImageEntity>(uuid, ttl)
            )),
            _ => throw new NotSupportedException($"Unsupported AppId: {appId}"),
        };

        return response.Url;

        static BotMessage CreateFakeFriendMessage(long uin) => BotMessage.CreateCustomFriend(uin, string.Empty, uin, string.Empty, DateTimeOffset.Now.ToUnixTimeSeconds(), []);
        static BotMessage CreateFakeGroupMessage(long uin) => BotMessage.CreateCustomGroup(uin, 0, string.Empty, DateTimeOffset.Now.ToUnixTimeSeconds(), []);
        static RichMediaEntityBase CreateFakeEntity<T>(string fileUuid, uint ttl) where T : RichMediaEntityBase, new()
        {
            return new T
            {
                MsgInfo = new MsgInfo
                {
                    MsgInfoBody = [new MsgInfoBody {
                        Index = new IndexNode {
                            Info = new FileInfo {},
                            FileUuid = fileUuid,
                            StoreId = 1,
                            UploadTime = 0,
                            Ttl = ttl,
                            SubType = 0,
                        }
                    }]
                }
            };
        }
    }
}
