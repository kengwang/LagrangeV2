using Lagrange.Core.Common.Entity;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Message.Entities;

public class MentionEntity(long uin, string? display) : IMessageEntity
{
    public long Uin { get; private set; } = uin;

    public string? Display { get; private set; } = display;
    
    internal string? Uid { get; private set; }
    
    public MentionEntity() : this(0, null) { }

    async Task IMessageEntity.Preprocess(BotContext context, BotMessage message)
    {
        if (Uin == 0) { Display ??= "@全体成员"; return; }
        if (Uin != 0)
        {
            BotContact contact;
            switch (message.Receiver)
            {
                case BotGroup group: 
                    (_, contact) = await context.CacheContext.ResolveMember(group.Uin, Uin) ?? throw new InvalidTargetException(Uin, group.Uin);
                    break;
                case BotFriend friend:
                    contact = await context.CacheContext.ResolveFriend(Uin) ?? throw new InvalidTargetException(Uin, friend.Uin);
                    break;
                case BotStranger stranger:
                    contact = stranger;
                    break;
                default:
                    throw new InvalidTargetException(Uin, message.Contact.Uin);
            }

            Display ??= contact.Nickname;
            if (!Display.StartsWith('@')) Display = '@' + Display;
            Uid = contact.Uid;
        }
    }

    internal async Task<IMessageEntity> PrepareForward(BotContext context, BotContact receiver)
    {
        if (Uin == 0) return new MentionEntity(0, "@全体成员");
        if (!string.IsNullOrEmpty(Uid))
            return new MentionEntity(Uin, Label(Display ?? Uin.ToString())) { Uid = Uid };
        BotContact? contact = receiver switch
        {
            BotGroup group => (await context.CacheContext.ResolveMember(group.Uin, Uin))?.Item2,
            BotFriend => await context.CacheContext.ResolveFriend(Uin),
            BotStranger stranger when stranger.Uin == Uin => stranger,
            _ => null
        };
        var text = Label(string.IsNullOrWhiteSpace(Display) ? contact?.Nickname ?? Uin.ToString() : Display);
        if (string.IsNullOrEmpty(contact?.Uid)) return new TextEntity(text);
        return new MentionEntity(Uin, text) { Uid = contact.Uid };
    }

    private static string Label(string text) => text.StartsWith('@') ? text : '@' + text;

    Task IMessageEntity.Postprocess(BotContext context, BotMessage message)
    {
        if (Uin == 0 && Uid != null)
        {
            Uin = context.CacheContext.ResolveUin(Uid);
        }
        
        return Task.CompletedTask;
    }
    
    string IMessageEntity.ToPreviewString() => Display ?? "";

    Elem[] IMessageEntity.Build()
    {
        var resvAttr = new TextResvAttr
        {
            AtType = Uin == 0 ? 1u : 2u, // 1 for mention all
            AtMemberUin = (ulong)Uin,
            AtMemberTinyid = 0,
            AtMemberUid = Uid
        };
        
        return
        [
            new Elem
            {
                Text = new Text
                {
                    TextMsg = Display ?? throw new InvalidOperationException("Display cannot be null"),
                    PbReserve = ProtoHelper.Serialize(resvAttr),
                }
            }
        ];
    }
    
    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.Text is { Attr6Buf.Length: > 0, PbReserve: { Length: > 0 } reserve })
        {
            var attr = ProtoHelper.Deserialize<TextResvAttr>(reserve.Span);
            var entity = new MentionEntity((long)attr.AtMemberUin, target.Text.TextMsg);
            if (attr.AtType == 2) // AtType: 1 = mention_all, 2 = mention specific user
            {
                entity.Uid = attr.AtMemberUid;
            }
            return entity;
        }

        return null;
    }
}