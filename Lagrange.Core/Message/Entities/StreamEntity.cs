using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility;
using Lagrange.Proto;

namespace Lagrange.Core.Message.Entities;

public sealed class StreamEntity : IMessageEntity
{
    public string Text { get; init; } = string.Empty;
    public ulong StreamId { get; init; }
    public ulong Sequence { get; init; } = 1;
    public StreamEntity() { }
    public StreamEntity(string text)
    {
        Text = text;
        StreamId = (ulong)Random.Shared.NextInt64(1, long.MaxValue);
    }

    Elem[] IMessageEntity.Build() =>
    [
        new Elem { Text = new Text { TextMsg = Text } },
        new Elem { GeneralFlags = new GeneralFlags
        {
            PbReserve = ProtoHelper.Serialize(new StreamExtra { StreamId = StreamId, Sequence = Sequence })
        } }
    ];

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.GeneralFlags is null || target.GeneralFlags.PbReserve.IsEmpty) return null;
        var extra = ProtoHelper.Deserialize<StreamExtra>(target.GeneralFlags.PbReserve.Span);
        if (extra.StreamId == 0 || extra.Sequence == 0) return null;
        var text = elements.FirstOrDefault(x => x.Text is not null)?.Text?.TextMsg;
        return text is null ? null : new StreamEntity { Text = text, StreamId = extra.StreamId, Sequence = extra.Sequence };
    }

    public string ToPreviewString() => $"[Stream] {Text}";
}

[ProtoPackable]
internal partial class StreamExtra
{
    [ProtoMember(43)] public ulong StreamId { get; set; }
    [ProtoMember(103)] public ulong Sequence { get; set; }
}
