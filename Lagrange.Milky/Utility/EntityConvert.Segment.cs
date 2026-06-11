using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Milky.Entity.Segment;

namespace Lagrange.Milky.Utility;

public partial class EntityConvert
{
    private IReadOnlyList<IIncomingSegment> Segments(MessageChain entities)
    {
        var segments = new List<IIncomingSegment>();
        foreach (var entity in entities)
        {
            segments.Add(Segment(entity));
        }
        return segments;
    }
    public async Task<MessageChain> GroupSegmentsAsync(IReadOnlyList<IOutgoingSegment> segments, long uin, CancellationToken token)
    {
        var entities = new MessageChain();
        foreach (var segment in segments)
        {
            entities.Add(await GroupSegmentAsync(segment, uin, token));
        }
        return entities;
    }
    public async Task<MessageChain> FriendSegmentsAsync(IReadOnlyList<IOutgoingSegment> segments, long uin, CancellationToken token)
    {
        var entities = new MessageChain();
        foreach (var segment in segments)
        {
            entities.Add(await FriendSegmentAsync(segment, uin, token));
        }
        return entities;
    }
    public async Task<MessageChain> FakeSegmentsAsync(IReadOnlyList<IOutgoingSegment> segments, CancellationToken token)
    {
        var entities = new MessageChain();
        foreach (var segment in segments)
        {
            entities.Add(await FakeSegmentAsync(segment, token));
        }
        return entities;
    }

    private IIncomingSegment Segment(IMessageEntity entities) => entities switch
    {
        TextEntity text => new TextIncomingSegment(text.Text),
        MentionEntity mention when mention.Uin != 0 => new MentionIncomingSegment(mention.Uin, mention.Display ?? string.Empty),
        MentionEntity mention when mention.Uin == 0 => new MentionAllIncomingSegment(),
        FaceEntity face => new FaceIncomingSegment(face.FaceId.ToString(), face.IsLargeFace),
        ReplyEntity reply => new ReplyIncomingSegment(
            (long)reply.SrcSequence,
            reply.SourceUin,
            reply.Source?.Nickname,
            reply.SourceTime,
            Segments(reply.SourceEntities)
        ),
        ImageEntity image => new ImageIncomingSegment(
            image.FileUuid,
            image.FileUrl,
            (int)image.ImageSize.X,
            (int)image.ImageSize.Y,
            image.Summary,
            image.SubType switch
            {
                0 => "normal",
                _ => "sticker",
            }
        ),
        RecordEntity record => new RecordIncomingSegment(record.FileUuid, record.FileUrl, (int)record.RecordLength),
        VideoEntity video => new VideoIncomingSegment(
            video.FileUuid,
            video.FileUrl,
            (int)video.VideoSize.X,
            (int)video.VideoSize.Y,
            (int)video.VideoLength
        ),
        GroupFileEntity groupFile => new FileIncomingSegment(groupFile.FileId, groupFile.FileName, groupFile.FileSize),
        MultiMsgEntity multiMsg => new ForwardIncomingSegment(
            multiMsg.ResId ?? string.Empty,
            multiMsg.Title ?? string.Empty,
            multiMsg.Preview?.ToArray() ?? [],
            multiMsg.Summary ?? string.Empty
        ),
        LightAppEntity lightApp => new LightAppIncomingSegment(lightApp.AppName, lightApp.Payload),
        MarketFaceEntity marketFace => new MarketFaceIncomingSegment(marketFace.EmojiPackageId, marketFace.EmojiId, marketFace.Key, marketFace.Summary, marketFace.Url),
        XmlEntity xml => new XmlIncomingSegment(xml.ServiceId, xml.Xml),
        MarkdownEntity markdown => new MarkdownIncomingSegment(markdown.Content),
        _ => new TextIncomingSegment($"[Unsupported message entity: {entities.GetType().Name}]"),
    };
    private Task<IMessageEntity> GroupSegmentAsync(IOutgoingSegment segment, long uin, CancellationToken token) => segment switch
    {
        ReplyOutgoingSegment reply => ReplyGroupSegmentAsync(reply, uin, token),
        _ => CommonSegmentAsync(segment, token),
    };
    private Task<IMessageEntity> FriendSegmentAsync(IOutgoingSegment segment, long uin, CancellationToken token) => segment switch
    {
        ReplyOutgoingSegment reply => ReplyFriendSegmentAsync(reply, uin, token),
        _ => CommonSegmentAsync(segment, token),
    };
    private Task<IMessageEntity> FakeSegmentAsync(IOutgoingSegment segment, CancellationToken token) => segment switch
    {
        ReplyOutgoingSegment reply => Task.FromResult<IMessageEntity>(new ReplyEntity()),
        _ => CommonSegmentAsync(segment, token),
    };
    private async Task<IMessageEntity> CommonSegmentAsync(IOutgoingSegment segment, CancellationToken token) => segment switch
    {
        TextOutgoingSegment text => new TextEntity(text.Data.Text),
        MentionOutgoingSegment mention => new MentionEntity(mention.Data.UserId, null),
        MentionAllOutgoingSegment => new MentionEntity(0, "@全体成员"),
        FaceOutgoingSegment face => new FaceEntity(ParseFaceId(face.Data.FaceId), face.Data.IsLarge),
        // ReplySegment => 
        ImageOutgoingSegment image => new ImageEntity(
            await _resolver.ToMemoryStreamAsync(image.Data.Uri, token),
            image.Data.Summary,
            image.Data.SubType switch
            {
                "normal" => 0,
                "sticker" => 1,
                _ => throw new NotSupportedException(),
            },
            disposeOnCompletion: true
        ),
        RecordOutgoingSegment record => new RecordEntity(
            await _resolver.ToMemoryStreamAsync(record.Data.Uri, token),
            disposeOnCompletion: true
        ),
        VideoOutgoingSegment video => new VideoEntity(
            await _resolver.ToMemoryStreamAsync(video.Data.Uri, token),
            video.Data.ThumbUri != null ? await _resolver.ToMemoryStreamAsync(video.Data.ThumbUri, token) : null,
            disposeOnCompletion: true
        ),
        ForwardOutgoingSegment forwardOutgoingSegment => await BuildMultiMsgEntityAsync(forwardOutgoingSegment.Data, token),
        LightAppOutgoingSegment lightApp => new LightAppEntity(lightApp.Data.JsonPayload),
        MarkdownOutgoingSegment markdown => new MarkdownEntity(markdown.Data.Content),
        _ => throw new NotSupportedException(),
    };

    private async Task<IMessageEntity> ReplyGroupSegmentAsync(ReplyOutgoingSegment reply, long uin, CancellationToken token)
    {
        long sequence = reply.Data.MessageSeq;
        var message = await _cache.GetMessageAsync(MessageType.Group, uin, (ulong)sequence, token);
        if (message == null) throw new Exception("message not found");

        return new ReplyEntity(message);
    }
    private async Task<IMessageEntity> ReplyFriendSegmentAsync(ReplyOutgoingSegment reply, long uin, CancellationToken token)
    {
        long sequence = reply.Data.MessageSeq;
        var message = await _cache.GetMessageAsync(MessageType.Private, uin, (ulong)sequence, token);
        if (message == null) throw new Exception("message not found");

        return new ReplyEntity(message);
    }

    private async Task<MultiMsgEntity> BuildMultiMsgEntityAsync(ForwardOutgoingSegmentData data, CancellationToken token)
    {
        var multiMsgEntity = new MultiMsgEntity
        {
            Title = data.Title,
            Preview = data.Preview?.ToList(),
            Summary = data.Summary,
            Prompt = data.Prompt
        };
        foreach (var segment in data.Messages)
        {
            var msgChain = await FakeSegmentsAsync(segment.Segments, token);
            var msg = BotMessage.CreateCustomFriend(segment.UserId, segment.SenderName, 0, string.Empty, DateTime.Now, msgChain);
            multiMsgEntity.Messages.Add(msg);
        }

        return multiMsgEntity;
    }

    private static ushort ParseFaceId(string faceId)
    {
        return ushort.TryParse(faceId, out ushort id)
            ? id
            : throw new NotSupportedException($"Invalid face_id: {faceId}");
    }

}
