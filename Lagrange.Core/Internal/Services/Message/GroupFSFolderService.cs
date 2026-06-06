using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Internal.Services.Message;

[EventSubscribe<GroupFSListEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x6d8_1")]
internal class GroupFSListService : OidbService<GroupFSListEventReq, GroupFSListEventResp, GroupFSListRequest, GroupFSListResponse>
{
    private protected override uint Command => 0x6d8;

    private protected override uint Service => 1;

    private protected override Task<GroupFSListRequest> ProcessRequest(GroupFSListEventReq request, BotContext context)
    {
        return Task.FromResult(new GroupFSListRequest
        {
            List = new GroupFSListRequestBody
            {
                GroupUin = request.GroupUin,
                AppId = 7,
                TargetDirectory = request.ParentDirectory,
                FileCount = request.FileCount,
                SortBy = 1,
                StartIndex = request.StartIndex,
                Field17 = 2,
                Field18 = 0
            }
        });
    }

    private protected override Task<GroupFSListEventResp> ProcessResponse(GroupFSListResponse response, BotContext context)
    {
        var list = response.List ?? throw new OperationException(-1, "Group file list response is empty");
        if (list.RetCode != 0) throw new OperationException(list.RetCode, list.RetMsg);

        var entries = new List<IBotFSEntry>();
        foreach (var item in list.Items ?? [])
        {
            switch (item.Type)
            {
                case 1:
                {
                    var file = item.FileInfo;
                    entries.Add(new BotFileEntry(
                        file.FileId,
                        file.FileName,
                        file.ParentDirectory,
                        file.FileSize,
                        DateTimeOffset.FromUnixTimeSeconds(file.ExpireTime).UtcDateTime,
                        DateTimeOffset.FromUnixTimeSeconds(file.ModifiedTime).UtcDateTime,
                        file.UploaderUin,
                        DateTimeOffset.FromUnixTimeSeconds(file.UploadedTime).UtcDateTime,
                        file.DownloadedTimes
                    ));
                    break;
                }
                case 2:
                {
                    var folder = item.FolderInfo;
                    entries.Add(new BotFolderEntry(
                        folder.FolderId,
                        folder.ParentDirectoryId,
                        folder.FolderName,
                        DateTimeOffset.FromUnixTimeSeconds(folder.CreateTime).UtcDateTime,
                        DateTimeOffset.FromUnixTimeSeconds(folder.ModifiedTime).UtcDateTime,
                        folder.CreatorUin,
                        folder.TotalFileCount
                    ));
                    break;
                }
            }
        }

        return Task.FromResult(new GroupFSListEventResp(entries, list.IsEnd));
    }
}

[EventSubscribe<GroupFSCreateFolderEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x6d7_0")]
internal class GroupFSCreateFolderService : OidbService<GroupFSCreateFolderEventReq, GroupFSCreateFolderEventResp, GroupFSFolderRequest, GroupFSFolderResponse>
{
    private protected override uint Command => 0x6d7;

    private protected override uint Service => 0;

    private protected override Task<GroupFSFolderRequest> ProcessRequest(GroupFSCreateFolderEventReq request, BotContext context)
    {
        return Task.FromResult(new GroupFSFolderRequest
        {
            Create = new GroupFSCreateFolderRequestBody
            {
                GroupUin = request.GroupUin,
                RootDirectory = "/",
                FolderName = request.FolderName
            }
        });
    }

    private protected override Task<GroupFSCreateFolderEventResp> ProcessResponse(GroupFSFolderResponse response, BotContext context)
    {
        var create = response.Create ?? throw new OperationException(-1, "Group folder create response is empty");
        if (create.RetCode != 0) throw new OperationException(create.RetCode, create.RetMsg);
        return Task.FromResult(new GroupFSCreateFolderEventResp(create.FolderInfo.FolderId));
    }
}

[EventSubscribe<GroupFSRenameFolderEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x6d7_2")]
internal class GroupFSRenameFolderService : OidbService<GroupFSRenameFolderEventReq, GroupFSRenameFolderEventResp, GroupFSFolderRequest, GroupFSFolderResponse>
{
    private protected override uint Command => 0x6d7;

    private protected override uint Service => 2;

    private protected override Task<GroupFSFolderRequest> ProcessRequest(GroupFSRenameFolderEventReq request, BotContext context)
    {
        return Task.FromResult(new GroupFSFolderRequest
        {
            Rename = new GroupFSRenameFolderRequestBody
            {
                GroupUin = request.GroupUin,
                FolderId = request.FolderId,
                NewFolderName = request.NewFolderName
            }
        });
    }

    private protected override Task<GroupFSRenameFolderEventResp> ProcessResponse(GroupFSFolderResponse response, BotContext context)
    {
        var rename = response.Rename ?? throw new OperationException(-1, "Group folder rename response is empty");
        if (rename.RetCode != 0) throw new OperationException(rename.RetCode, rename.RetMsg);
        return Task.FromResult(new GroupFSRenameFolderEventResp());
    }
}

[EventSubscribe<GroupFSDeleteFolderEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x6d7_1")]
internal class GroupFSDeleteFolderService : OidbService<GroupFSDeleteFolderEventReq, GroupFSDeleteFolderEventResp, GroupFSFolderRequest, GroupFSFolderResponse>
{
    private protected override uint Command => 0x6d7;

    private protected override uint Service => 1;

    private protected override Task<GroupFSFolderRequest> ProcessRequest(GroupFSDeleteFolderEventReq request, BotContext context)
    {
        return Task.FromResult(new GroupFSFolderRequest
        {
            Delete = new GroupFSDeleteFolderRequestBody
            {
                GroupUin = request.GroupUin,
                FolderId = request.FolderId
            }
        });
    }

    private protected override Task<GroupFSDeleteFolderEventResp> ProcessResponse(GroupFSFolderResponse response, BotContext context)
    {
        var delete = response.Delete ?? throw new OperationException(-1, "Group folder delete response is empty");
        if (delete.RetCode != 0) throw new OperationException(delete.RetCode, delete.RetMsg);
        return Task.FromResult(new GroupFSDeleteFolderEventResp());
    }
}
