using Lagrange.Core.Internal.Packets.Message;

namespace Lagrange.Core.Message.Entities;

public class ElemFlags2Entity(uint bubbleId) : IMessageEntity
{
    public ElemFlags2Entity() : this(0) { }
    
    public uint BubbleId { get; } = bubbleId;
    
    Elem[] IMessageEntity.Build()
    {
        return
        [
            new Elem {  ElemFlags2 = new ElemFlags2() { BubbleId = BubbleId } }
        ];
    }

    string IMessageEntity.ToPreviewString() => string.Empty;
    
    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        return target.ElemFlags2?.BubbleId is { } bId
            ? new ElemFlags2Entity(bId)
            : null;
    }

    
}