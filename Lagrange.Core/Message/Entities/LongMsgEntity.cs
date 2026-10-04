using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Common.Entity;

namespace Lagrange.Core.Message.Entities;

public sealed class LongMsgEntity : IMessageEntity
{
    public string ResId { get; internal set; } = string.Empty;
    public List<BotMessage> Messages { get; } = [];
    public LongMsgEntity() { }
    public LongMsgEntity(string resId) => ResId = resId;

    public async Task Preprocess(BotContext context, BotMessage message)
    {
        if (!string.IsNullOrEmpty(ResId) || Messages.Count == 0) return;
        var result = await context.EventContext.SendEvent<LongMsgSendEventResp>(
            new LongMsgSendEventReq(message.Receiver, Messages));
        ResId = result.ResId;
    }

    public async Task Postprocess(BotContext context, BotMessage message)
    {
        if (string.IsNullOrEmpty(ResId)) return;
        try
        {
            var result = await context.EventContext.SendEvent<LongMsgRecvEventResp>(
                new LongMsgRecvEventReq(message.Contact is BotGroupMember, ResId));
            Messages.Clear();
            Messages.AddRange(result.Messages);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            context.LogWarning(nameof(LongMsgEntity), $"Failed to fetch long message {ResId}: {ex.Message}");
        }
    }

    Elem[] IMessageEntity.Build() => [new Elem { GeneralFlags = new GeneralFlags { LongTextFlag = 1, LongTextResId = ResId } }];

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target) =>
        target.GeneralFlags is { LongTextFlag: 1, LongTextResId: not null } flags
            ? new LongMsgEntity(flags.LongTextResId)
            : null;

    public string ToPreviewString() => "[聊天记录]";
}
