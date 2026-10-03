using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Logic;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Binary;

namespace Lagrange.Core.Message.Entities;

public class GroupFileEntity : IMessageEntity
{
    public string FileId { get; internal init; } = string.Empty;

    public string FileName { get; internal init; } = string.Empty;

    public long FileSize { get; internal init; }

    public string FileMd5 { get; internal init; } = string.Empty;

    public string FileUrl { get; set; }  = string.Empty;

    public async Task Postprocess(BotContext context, BotMessage message)
    {
        if (string.IsNullOrWhiteSpace(FileUrl) && message.Contact is BotGroupMember member)
            FileUrl = await context.EventContext.GetLogic<OperationLogic>().GroupFSDownload(member.Group.Uin, FileId);
    }

    Elem[] IMessageEntity.Build()
    {
        var extra = new GroupFileExtra
        {
            Field1 = 4,
            FileName = FileName,
            Display = FileName,
            Inner = new GroupFileExtraInner
            {
                Info = new GroupFileExtraInfo
                {
                    FileId = FileId,
                    FileName = FileName,
                    FileSize = FileSize,
                    FileMd5 = FileMd5,
                    Field7 = FileUrl
                }
            }
        };
        using var payload = new BinaryPacket(256);
        payload.Write<byte>(1);
        payload.Write(ProtoHelper.Serialize(extra).Span, Prefix.Int16 | Prefix.LengthOnly);
        return [new Elem { TransElemInfo = new TransElem { ElemType = 24, ElemValue = payload.ToArray() } }];
    }

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.TransElemInfo is { ElemType: 24 } trans)
        {
            var payload = new BinaryPacket(trans.ElemValue.AsSpan());
            payload.Skip(1);
            var data = payload.ReadBytes(Prefix.Int16 | Prefix.LengthOnly);
            var extra = ProtoHelper.Deserialize<GroupFileExtra>(data).Inner.Info;

            return new GroupFileEntity
            {
                FileId = extra.FileId,
                FileName = extra.FileName,
                FileSize = extra.FileSize,
                FileMd5 = extra.FileMd5,
            };
        }

        return null;
    }

    public string ToPreviewString() => $"[群文件 {FileName}]";
}
