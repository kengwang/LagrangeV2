namespace Lagrange.Core.Common.Entity;

/// <summary>Presentation parameters carried by a message.</summary>
public class MessageStyle
{
    /// <summary>Chat bubble identifier.</summary>
    public ulong BubbleId { get; set; }
    /// <summary>Avatar pendant identifier.</summary>
    public ulong PendantId { get; set; }
    /// <summary>Font identifier.</summary>
    public ushort FontId { get; set; }
    /// <summary>Font effect identifier (2000 denotes super large text).</summary>
    public uint FontEffectId { get; set; }
    /// <summary>Whether the font grows and shrinks.</summary>
    public bool IsCsFontEffectEnabled { get; set; }
    /// <summary>Custom bubble sticker or text archive identifier.</summary>
    public uint BubbleDiyTextId { get; set; }
}
