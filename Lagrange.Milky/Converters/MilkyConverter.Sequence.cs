using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Milky.Extensions;
using Lagrange.Milky.Models.Messages;
using Lagrange.Milky.Models.Segments;

namespace Lagrange.Milky.Converters;

public partial class MilkyConverter
{
    public async Task<IReadOnlyList<IncomingSegmentBase>> ToIncomingSegmentsAsync(MessageChain chain, MessageType type, long ownerPeerUin, CancellationToken ct = default)
    {
        List<IncomingSegmentBase> result = new(chain.Count);
        foreach (var entity in chain)
        {
            var segment = await ToIncomingSegmentAsync(entity, type, ownerPeerUin, ct);
            if (segment != null) result.Add(segment);
        }
        return result;
    }

    private async Task<IncomingSegmentBase?> ToIncomingSegmentAsync(IMessageEntity entity, MessageType type, long ownerPeerUin, CancellationToken ct = default) => entity switch
    {
        TextEntity text => new TextIncomingSegment { Data = new TextIncomingSegmentData { Text = text.Text } },
        MentionEntity mention => mention.Uin == 0
            ? new MentionAllIncomingSegment { Data = new object(), }
            : new MentionIncomingSegment
            {
                Data = new MentionIncomingSegmentData
                {
                    UserId = mention.Uin,
                    Name = mention.Display ?? string.Empty,
                }
            },
        FaceEntity { FaceId: 358 } face => new DiceIncomingSegment { Data = new DiceIncomingSegmentData { FaceId = face.FaceId } },
        FaceEntity { FaceId: 359 } face => new RpsIncomingSegment { Data = new RpsIncomingSegmentData { FaceId = face.FaceId } },
        FaceEntity face => new FaceIncomingSegment
        {
            Data = new FaceIncomingSegmentData { FaceId = face.FaceId, Raw = face.Raw }
        },
        XmlEntity xml => new XmlIncomingSegment
        {
            Data = new XmlIncomingSegmentData { Xml = xml.Xml }
        },
        ReplyEntity reply => await ToReplyIncomingSegmentAsync(reply, type, ownerPeerUin, ct),
        ImageEntity image => new ImageIncomingSegment
        {
            Data = new ImageIncomingSegmentData
            {
                ResourceId = image.FileUuid,
                TempUrl = image.FileUrl,
                Width = (int)image.ImageSize.X,
                Height = (int)image.ImageSize.Y,
                Summary = image.Summary,
                FileMd5 = image.FileMd5,
                FileSha1 = image.FileSha1,
                FileSize = image.FileSize,
                SubType = image.SubType switch
                {
                    0 => "normal",
                    _ => "sticker",
                }
            }
        },
        RecordEntity record => new RecordIncomingSegment
        {
            Data = new RecordIncomingSegmentData
            {
                ResourceId = record.FileUuid,
                TempUrl = record.FileUrl,
                Duration = (int)record.RecordLength,
                FileMd5 = record.FileMd5,
                FileSha1 = record.FileSha1,
                FileSize = record.FileSize,
            }
        },
        VideoEntity video => new VideoIncomingSegment
        {
            Data = new VideoIncomingSegmentData
            {
                ResourceId = video.FileUuid,
                TempUrl = video.FileUrl,
                Width = (int)video.VideoSize.X,
                Height = (int)video.VideoSize.Y,
                Duration = (int)video.VideoLength,
                FileMd5 = video.FileMd5,
                FileSha1 = video.FileSha1,
                FileSize = video.FileSize,
            }
        },
        GroupFileEntity file => new FileIncomingSegment
        {
            Data = new FileIncomingSegmentData
            {
                FileId = file.FileId,
                FileName = file.FileName,
                FileSize = file.FileSize,
                FileHash = string.IsNullOrWhiteSpace(file.FileMd5) ? null : file.FileMd5,
                Url = string.IsNullOrWhiteSpace(file.FileUrl) ? null : file.FileUrl,
                Title = file.FileName,
                Preview = file.ToPreviewString(),
                Summary = file.ToPreviewString(),
            }
        },
        MultiMsgEntity forward => new ForwardIncomingSegment
        {
            Data = new ForwardIncomingSegmentData
            {
                ForwardId = forward.ResId ?? throw new Exception(""),
                Title = forward.Title ?? $"{forward.Messages.Count}条转发消息",
                Preview = forward.Preview ?? forward.Messages.Take(4).Select(x => $"{x.Contact.Nickname}: {x.Entities.Count}个消息段").ToArray(),
                Summary = forward.Summary ?? $"查看{forward.Messages.Count}条转发消息",
                Prompt = forward.Prompt,
            }
        },
        MarketFaceEntity marketFace => new MarketFaceIncomingSegment
        {
            Data = new MarketFaceIncomingSegmentData { FaceId = marketFace.FaceId, Name = marketFace.Name, Url = marketFace.Url, Summary = marketFace.Summary }
        },
        PokeEntity poke => new PokeIncomingSegment
        {
            Data = new PokeIncomingSegmentData { Type = poke.Type, Strength = poke.Strength }
        },
        MarkdownEntity markdown => new MarkdownIncomingSegment
        {
            Data = new MarkdownIncomingSegmentData { Content = markdown.Content }
        },
        KeyboardEntity keyboard => new KeyboardIncomingSegment
        {
            Data = new KeyboardIncomingSegmentData
            {
                Data = keyboard.ToJson()
            }
        },
        SpecialPokeEntity special => new SpecialPokeIncomingSegment
        {
            Data = new SpecialPokeIncomingSegmentData { FaceId = special.FaceId, Count = special.Count, FaceName = special.FaceName }
        },
        BounceFaceEntity bounce => new BounceFaceIncomingSegment { Data = new BounceFaceIncomingSegmentData { FaceId = bounce.FaceId, Count = bounce.Count, Name = bounce.Name } },
        GroupReactionEntity reaction => new GroupReactionIncomingSegment { Data = new GroupReactionIncomingSegmentData { Reactions = reaction.Reactions.Select(x => new GroupReactionItem { FaceId = x.FaceId, Type = x.Type, Count = x.Count, IsAdded = x.IsAdded }).ToArray() } },
        JsonEntity json => new JsonIncomingSegment { Data = new JsonIncomingSegmentData { Data = json.Data } },
        LocationEntity location => new LocationIncomingSegment { Data = new LocationIncomingSegmentData { Latitude = location.Latitude, Longitude = location.Longitude, Title = location.Title, Content = location.Content } },
        MusicEntity music => new MusicIncomingSegment { Data = new MusicIncomingSegmentData { Type = music.Type, Id = music.Id, Url = music.Url, Audio = music.Audio, Title = music.Title, Content = music.Content, Image = music.Image } },
        ShareEntity share => new ShareIncomingSegment { Data = new ShareIncomingSegmentData { Url = share.Url, Title = share.Title, Content = share.Content, Image = share.Image } },
        ContactEntity contact => new ContactIncomingSegment { Data = new ContactIncomingSegmentData { ContactType = contact.ContactType, Id = contact.Id } },
        LongMsgEntity longMsg => await ToLongMsgIncomingSegmentAsync(longMsg, type, ownerPeerUin, ct),
        GreyTipEntity tip => new GreyTipIncomingSegment { Data = new GreyTipSegmentData { Text = tip.GreyTip } },
        StreamEntity stream => new StreamIncomingSegment { Data = new StreamIncomingSegmentData { Text = stream.Text } },
        LightAppEntity lightApp => new LightAppIncomingSegment
        {
            Data = new LightAppIncomingSegmentData
            {
                AppName = lightApp.AppName,
                JsonPayload = lightApp.Payload,
            }
        },
        _ => null,
    };

    private async Task<LongMsgIncomingSegment> ToLongMsgIncomingSegmentAsync(LongMsgEntity entity, MessageType type, long ownerPeerUin, CancellationToken ct)
    {
        IReadOnlyList<IncomingForwardedMessage>? messages = null;
        if (entity.Messages.Count > 0)
        {
            var list = new List<IncomingForwardedMessage>(entity.Messages.Count);
            foreach (var message in entity.Messages)
            {
                list.Add(new IncomingForwardedMessage
                {
                    MessageSeq = message.Type == MessageType.Private ? (long)message.ClientSequence : (long)message.Sequence,
                    SenderName = message.Contact.Nickname,
                    AvatarUrl = string.Empty,
                    Time = message.Time,
                    Segments = await ToIncomingSegmentsAsync(message.Entities, message.Type, ownerPeerUin, ct)
                });
            }
            messages = list;
        }
        return new LongMsgIncomingSegment { Data = new LongMsgIncomingSegmentData { ResId = entity.ResId, Messages = messages } };
    }

    private async Task<ReplyIncomingSegment> ToReplyIncomingSegmentAsync(ReplyEntity reply, MessageType type, long ownerPeerUin, CancellationToken ct = default)
    {
        var message = _cache.Get(type, ownerPeerUin, reply.SrcSequence)
            ?? (type switch
            {
                MessageType.Private => await _lagrange.GetC2CMessage(
                    ownerPeerUin,
                    reply.SrcSequence,
                    reply.SrcSequence
                ).WaitAsync(ct),
                MessageType.Group => await _lagrange.GetGroupMessage(
                    ownerPeerUin,
                    reply.SrcSequence,
                    reply.SrcSequence
                ).WaitAsync(ct),
                _ => throw new NotSupportedException(),
            }).First();

        return new ReplyIncomingSegment
        {
            Data = new ReplyIncomingSegmentData
            {
                MessageSeq = message.Type switch
                {
                    MessageType.Private => (long)message.ClientSequence,
                    _ => (long)message.Sequence,
                },
                SenderId = message.Contact.Uin,
                SenderName = message.Contact switch
                {
                    BotFriend sender => sender.Nickname,
                    BotGroupMember member => member.Nickname,
                    BotStranger stranger => stranger.Nickname,
                    _ => throw new NotSupportedException(),
                },
                Time = message.Time,
                Segments = await ToIncomingSegmentsAsync(message.Entities, type, ownerPeerUin, ct),
            }
        };
    }

    public async Task<MessageChain> FromOutgoingSegmentsAsync(IReadOnlyList<OutgoingSegmentBase> segments, MessageType type, long ownerPeerUin, CancellationToken ct = default)
    {
        MessageChain result = [];
        foreach (var segment in segments)
        {
            result.Add(await FromOutgoingSegmentAsync(segment, type, ownerPeerUin, ct));
        }
        return result;
    }

    private async Task<IMessageEntity> FromOutgoingSegmentAsync(OutgoingSegmentBase segment, MessageType type, long ownerPeerUin, CancellationToken ct) => segment switch
    {
        TextOutgoingSegment text => new TextEntity(text.Data.Text),
        MentionOutgoingSegment mention => new MentionEntity(mention.Data.UserId, null),
        MentionAllOutgoingSegment => new MentionEntity(0, null),
        ReplyOutgoingSegment reply => await FromReplyOutgoingSegmentAsync(reply, type, ownerPeerUin, ct),
        ImageOutgoingSegment image => new ImageEntity(
            await _resourceConverter.UriToStreamAsync(image.Data.Uri, ct),
            image.Data.Summary,
            image.Data.SubType switch
            {
                "sticker" => 1,
                _ => 0,
            },
            disposeOnCompletion: true
        ),
        RecordOutgoingSegment record => new RecordEntity(
            await _resourceConverter.UriToStreamAsync(record.Data.Uri, ct),
            disposeOnCompletion: true
        ),
        VideoOutgoingSegment video => new VideoEntity(
            await _resourceConverter.UriToStreamAsync(video.Data.Uri, ct),
            video.Data.ThumbUri == null ? null : await _resourceConverter.UriToStreamAsync(video.Data.ThumbUri, ct),
            disposeOnCompletion: true
        ),
        ForwardOutgoingSegment forward => new MultiMsgEntity(await FromOutgoingForwardedMessagesAsync(
            forward.Data.Messages,
            ct
        )),
        LightAppOutgoingSegment lightApp => new LightAppEntity(lightApp.Data.JsonPayload),
        FaceOutgoingSegment face => new FaceEntity { FaceId = face.Data.FaceId, Raw = face.Data.Raw ?? string.Empty },
        MarketFaceOutgoingSegment marketFace => new MarketFaceEntity
        {
            FaceId = marketFace.Data.FaceId,
            Name = marketFace.Data.Name ?? string.Empty,
            Url = marketFace.Data.Url ?? string.Empty,
            Summary = marketFace.Data.Summary ?? string.Empty,
        },
        XmlOutgoingSegment xml => new XmlEntity { Xml = xml.Data.Xml },
        PokeOutgoingSegment poke => new PokeEntity(poke.Data.Type, poke.Data.Strength),
        MarkdownOutgoingSegment markdown => new MarkdownEntity(markdown.Data.Content),
        KeyboardOutgoingSegment keyboard => new KeyboardEntity(keyboard.Data.Data),
        SpecialPokeOutgoingSegment special => new SpecialPokeEntity(special.Data.FaceId, special.Data.Count, special.Data.FaceName ?? string.Empty),
        BounceFaceOutgoingSegment bounce => new BounceFaceEntity(bounce.Data.FaceId, bounce.Data.Count, bounce.Data.Name ?? string.Empty),
        GroupReactionOutgoingSegment reaction => new GroupReactionEntity(reaction.Data.Reactions.Select(x => new GroupReaction(x.FaceId, x.Type, x.Count, x.IsAdded))),
        JsonOutgoingSegment json => new JsonEntity(json.Data.Data),
        LocationOutgoingSegment location => new LocationEntity(location.Data.Latitude, location.Data.Longitude, location.Data.Title ?? string.Empty, location.Data.Content ?? string.Empty),
        MusicOutgoingSegment music => new MusicEntity { Type = music.Data.Type, Id = music.Data.Id, Url = music.Data.Url, Audio = music.Data.Audio, Title = music.Data.Title, Content = music.Data.Content, Image = music.Data.Image },
        ShareOutgoingSegment share => new ShareEntity { Url = share.Data.Url, Title = share.Data.Title, Content = share.Data.Content, Image = share.Data.Image },
        ContactOutgoingSegment contact => new ContactEntity { ContactType = contact.Data.ContactType, Id = contact.Data.Id },
        DiceOutgoingSegment dice => new FaceEntity { FaceId = dice.Data.FaceId },
        RpsOutgoingSegment rps => new FaceEntity { FaceId = rps.Data.FaceId },
        LongMsgOutgoingSegment longMsg => new LongMsgEntity(longMsg.Data.ResId),
        GreyTipOutgoingSegment tip => new GreyTipEntity(tip.Data.Text),
        StreamOutgoingSegment stream => new StreamEntity(stream.Data.Text),
        _ => throw new NotSupportedException(),
    };

    private async Task<ReplyEntity> FromReplyOutgoingSegmentAsync(ReplyOutgoingSegment reply, MessageType type, long ownerPeerUin, CancellationToken ct)
    {
        var message = _cache.Get(type, ownerPeerUin, (ulong)reply.Data.MessageSeq)
            ?? (type switch
            {
                MessageType.Private => await _lagrange.GetC2CMessage(
                    ownerPeerUin,
                    (ulong)reply.Data.MessageSeq,
                    (ulong)reply.Data.MessageSeq
                ).WaitAsync(ct),
                MessageType.Group => await _lagrange.GetGroupMessage(
                    ownerPeerUin,
                    (ulong)reply.Data.MessageSeq,
                    (ulong)reply.Data.MessageSeq
                ).WaitAsync(ct),
                _ => throw new NotSupportedException(),
            }).First();

        return new ReplyEntity(message);
    }

    private async Task<List<BotMessage>> FromOutgoingForwardedMessagesAsync(IReadOnlyList<OutgoingForwardedMessage> messages, CancellationToken ct)
    {
        List<BotMessage> result = [];
        foreach (var message in messages)
        {
            result.Add(BotMessage.CreateCustomFriend(
                message.UserId,
                message.SenderName,
                0,
                string.Empty,
                DateTimeOffset.Now.ToUnixTimeSeconds(),
                await FromForwardOutgoingSegmentsAsync(message.Segments, ct)
            ));
        }
        return result;
    }

    private async Task<MessageChain> FromForwardOutgoingSegmentsAsync(IReadOnlyList<OutgoingSegmentBase> segments, CancellationToken ct)
    {
        MessageChain chain = [];
        foreach (var segment in segments)
        {
            chain.Add(await FromForwardOutgoingSegmentAsync(segment, ct));
        }
        return chain;
    }

    private async Task<IMessageEntity> FromForwardOutgoingSegmentAsync(OutgoingSegmentBase segment, CancellationToken ct) => segment switch
    {
        TextOutgoingSegment text => new TextEntity(text.Data.Text),
        MentionOutgoingSegment mention => new MentionEntity(mention.Data.UserId, null),
        MentionAllOutgoingSegment => new MentionEntity(0, null),
        ReplyOutgoingSegment => new ReplyEntity(), // No information
        ImageOutgoingSegment image => new ImageEntity(
            await _resourceConverter.UriToStreamAsync(image.Data.Uri, ct),
            image.Data.Summary,
            image.Data.SubType switch
            {
                "sticker" => 1,
                _ => 0,
            },
            disposeOnCompletion: true
        ), // TODO: Unable to upload due to a bug in the core.
        RecordOutgoingSegment record => new RecordEntity(
            await _resourceConverter.UriToStreamAsync(record.Data.Uri, ct),
            disposeOnCompletion: true
        ), // TODO: Unable to upload due to a bug in the core.
        VideoOutgoingSegment video => new VideoEntity(
            await _resourceConverter.UriToStreamAsync(video.Data.Uri, ct),
            video.Data.ThumbUri == null ? null : await _resourceConverter.UriToStreamAsync(video.Data.ThumbUri, ct),
            disposeOnCompletion: true
        ), // TODO: Unable to upload due to a bug in the core.
        ForwardOutgoingSegment forward => await FromForwardOutgoingSegmentAsync(forward, ct),
        LightAppOutgoingSegment lightApp => new LightAppEntity(lightApp.Data.JsonPayload),
        FaceOutgoingSegment face => new FaceEntity { FaceId = face.Data.FaceId, Raw = face.Data.Raw ?? string.Empty },
        MarketFaceOutgoingSegment marketFace => new MarketFaceEntity
        {
            FaceId = marketFace.Data.FaceId,
            Name = marketFace.Data.Name ?? string.Empty,
            Url = marketFace.Data.Url ?? string.Empty,
            Summary = marketFace.Data.Summary ?? string.Empty,
        },
        XmlOutgoingSegment xml => new XmlEntity { Xml = xml.Data.Xml },
        PokeOutgoingSegment poke => new PokeEntity(poke.Data.Type, poke.Data.Strength),
        MarkdownOutgoingSegment markdown => new MarkdownEntity(markdown.Data.Content),
        KeyboardOutgoingSegment keyboard => new KeyboardEntity(keyboard.Data.Data),
        SpecialPokeOutgoingSegment special => new SpecialPokeEntity(special.Data.FaceId, special.Data.Count, special.Data.FaceName ?? string.Empty),
        BounceFaceOutgoingSegment bounce => new BounceFaceEntity(bounce.Data.FaceId, bounce.Data.Count, bounce.Data.Name ?? string.Empty),
        GroupReactionOutgoingSegment reaction => new GroupReactionEntity(reaction.Data.Reactions.Select(x => new GroupReaction(x.FaceId, x.Type, x.Count, x.IsAdded))),
        JsonOutgoingSegment json => new JsonEntity(json.Data.Data),
        LocationOutgoingSegment location => new LocationEntity(location.Data.Latitude, location.Data.Longitude, location.Data.Title ?? string.Empty, location.Data.Content ?? string.Empty),
        MusicOutgoingSegment music => new MusicEntity { Type = music.Data.Type, Id = music.Data.Id, Url = music.Data.Url, Audio = music.Data.Audio, Title = music.Data.Title, Content = music.Data.Content, Image = music.Data.Image },
        ShareOutgoingSegment share => new ShareEntity { Url = share.Data.Url, Title = share.Data.Title, Content = share.Data.Content, Image = share.Data.Image },
        ContactOutgoingSegment contact => new ContactEntity { ContactType = contact.Data.ContactType, Id = contact.Data.Id },
        DiceOutgoingSegment dice => new FaceEntity { FaceId = dice.Data.FaceId },
        RpsOutgoingSegment rps => new FaceEntity { FaceId = rps.Data.FaceId },
        LongMsgOutgoingSegment longMsg => new LongMsgEntity(longMsg.Data.ResId),
        GreyTipOutgoingSegment tip => new GreyTipEntity(tip.Data.Text),
        StreamOutgoingSegment stream => new StreamEntity(stream.Data.Text),
        _ => throw new NotSupportedException(),
    };

    private async Task<MultiMsgEntity> FromForwardOutgoingSegmentAsync(ForwardOutgoingSegment segment, CancellationToken ct)
    {
        var entity = new MultiMsgEntity(await FromOutgoingForwardedMessagesAsync(segment.Data.Messages, ct))
        {
            Title = segment.Data.Title,
            Preview = segment.Data.Preview,
            Summary = segment.Data.Summary,
            Prompt = segment.Data.Prompt,
        };
        return entity;
    }
}
