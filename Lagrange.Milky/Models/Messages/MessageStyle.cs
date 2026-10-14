using System.Text.Json.Serialization;

namespace Lagrange.Milky.Models.Messages;

/// <summary>Lagrange extension carrying message presentation parameters.</summary>
public sealed class MessageStyle
{
    /// <summary>Chat bubble identifier.</summary>
    [JsonPropertyName("bubble_id")] public ulong BubbleId { get; init; }
    /// <summary>Avatar pendant identifier.</summary>
    [JsonPropertyName("pendant_id")] public ulong PendantId { get; init; }
    /// <summary>Font identifier.</summary>
    [JsonPropertyName("font_id")] public ushort FontId { get; init; }
    /// <summary>Font effect identifier.</summary>
    [JsonPropertyName("font_effect_id")] public uint FontEffectId { get; init; }
    /// <summary>Whether the font grows and shrinks.</summary>
    [JsonPropertyName("is_cs_font_effect_enabled")] public bool IsCsFontEffectEnabled { get; init; }
    /// <summary>Custom bubble sticker or text archive identifier.</summary>
    [JsonPropertyName("bubble_diy_text_id")] public uint BubbleDiyTextId { get; init; }

    /// <summary>Converts presentation parameters to the Core model.</summary>
    /// <returns>The Core style.</returns>
    public Lagrange.Core.Common.Entity.MessageStyle ToCore() => new()
    {
        BubbleId = BubbleId, PendantId = PendantId, FontId = FontId,
        FontEffectId = FontEffectId, IsCsFontEffectEnabled = IsCsFontEffectEnabled,
        BubbleDiyTextId = BubbleDiyTextId
    };

    /// <summary>Copies presentation parameters from the Core model.</summary>
    /// <param name="style">The Core style, or null.</param>
    /// <returns>The copied style, or null.</returns>
    public static MessageStyle? FromCore(Lagrange.Core.Common.Entity.MessageStyle? style) => style is null ? null : new()
    {
        BubbleId = style.BubbleId, PendantId = style.PendantId, FontId = style.FontId,
        FontEffectId = style.FontEffectId, IsCsFontEffectEnabled = style.IsCsFontEffectEnabled,
        BubbleDiyTextId = style.BubbleDiyTextId
    };
}
