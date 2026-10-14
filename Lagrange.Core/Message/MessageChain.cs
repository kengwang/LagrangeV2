using Lagrange.Core.Message.Entities;

namespace Lagrange.Core.Message;

public class MessageChain : List<IMessageEntity>
{
    /// <summary>Optional presentation parameters for this message.</summary>
    public Lagrange.Core.Common.Entity.MessageStyle? Style { get; set; }
}
