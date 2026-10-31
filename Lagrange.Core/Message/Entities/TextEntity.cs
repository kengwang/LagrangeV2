using Lagrange.Core.Internal.Packets.Message;

namespace Lagrange.Core.Message.Entities;

public class TextEntity(string text) : IMessageEntity
{
    public string Text { get; } = text;

    public TextEntity() : this(string.Empty) { }
    
    Elem[] IMessageEntity.Build()
    {
        return
        [
            new Elem { Text = new Text { TextMsg = Text } }
        ];
    }
    
    string IMessageEntity.ToPreviewString() => Text;
    
    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.Text is { } text && !text.PbReserve.IsEmpty)
        {
            int index = elements.IndexOf(target);
            if (index > 0 && elements[index - 1].CommonElem is { ServiceType: 37 } animated)
            {
                var face = Lagrange.Core.Utility.ProtoHelper.Deserialize<AnimatedFaceExtra>(animated.PbElem.Span);
                var compatibility = Lagrange.Core.Utility.ProtoHelper.Deserialize<Text>(text.PbReserve.Span);
                if (face.FaceId is >= 0 && !string.IsNullOrEmpty(face.Text) && face.Text == text.TextMsg &&
                    compatibility.TextMsg.StartsWith('[') && compatibility.TextMsg.EndsWith("]请使用最新版手机QQ体验新功能", StringComparison.Ordinal)) return null;
            }
        }
        if (target.Text is not { } plain || plain.Attr6Buf is { Length: > 0 }) return null;
        if (!plain.PbReserve.IsEmpty && Lagrange.Core.Utility.ProtoHelper.Deserialize<TextResvAttr>(plain.PbReserve.Span).AtType is 1 or 2) return null;
        return new TextEntity(plain.TextMsg);
    }
}
