using Lagrange.Core.Internal.Packets.Message;

namespace Lagrange.Core.Message.Entities;

public class GeneralFlagsEntity : IMessageEntity
{
    public uint BubbleDiyTextId { get; set; }
    public uint BubbleSubId { get; set; }
    public ulong PendantId { get; set; }
    
    
    Elem[] IMessageEntity.Build()
    {
        return 
        [
            new Elem { GeneralFlags = new GeneralFlags
                {
                    BubbleDiyTextId = BubbleDiyTextId,
                    BubbleSubId = BubbleSubId,
                    PendantId = PendantId
                }
            }
        ];
    }

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        return target.GeneralFlags is { } flags
            ? new GeneralFlagsEntity
            {
                BubbleDiyTextId = flags.BubbleDiyTextId,
                BubbleSubId = flags.BubbleSubId,
                PendantId = flags.PendantId
            }
            : null;
    }

    string IMessageEntity.ToPreviewString() => string.Empty;
}