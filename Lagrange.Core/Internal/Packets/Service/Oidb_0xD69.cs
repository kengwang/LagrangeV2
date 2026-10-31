using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class D69GetReq
{
    [ProtoMember(1)] public uint Operation { get; set; }
    [ProtoMember(2)] public D69GetReqInner Inner { get; set; }
}

[ProtoPackable]
internal partial class D69GetReqInner
{
    [ProtoMember(1)] public uint Count { get; set; }
    [ProtoMember(2)] public string Cookie { get; set; }
}

[ProtoPackable]
internal partial class D69GetResp
{
    [ProtoMember(1)] public uint Status { get; set; }
    [ProtoMember(2)] public D69GetRespBody Body { get; set; }
}

[ProtoPackable]
internal partial class D69GetRespBody
{
    [ProtoMember(1)] public List<byte[]> EncodedItems { get; set; } = [];
    public IReadOnlyList<D69DoubtItem> Items => EncodedItems.Select(D69DoubtItem.Parse).ToArray();
    [ProtoMember(2)] public string Reason { get; set; }
}

[ProtoPackable]
internal partial class D69DoubtItem
{
    public string Uid { get; set; } = string.Empty;
    [ProtoMember(2)] public string Nick { get; set; }
    [ProtoMember(3)] public uint Age { get; set; }
    [ProtoMember(4)] public uint Sex { get; set; }
    [ProtoMember(5)] public string Message { get; set; }
    [ProtoMember(6)] public string Source { get; set; }
    [ProtoMember(7)] public string Reason { get; set; }
    [ProtoMember(8)] public ulong RequestTime { get; set; }
    [ProtoMember(9)] public ulong GroupCode { get; set; }
    [ProtoMember(10)] public uint CommonFriendCount { get; set; }

    internal static D69DoubtItem Parse(byte[] bytes)
    {
        var item = Lagrange.Core.Utility.ProtoHelper.Deserialize<D69DoubtItem>(bytes);
        var identity = Lagrange.Proto.Nodes.ProtoObject.Parse(bytes)[1];
        item.Uid = identity.WireType switch
        {
            Lagrange.Proto.Serialization.WireType.VarInt => identity.GetValue<ulong>().ToString(global::System.Globalization.CultureInfo.InvariantCulture),
            Lagrange.Proto.Serialization.WireType.LengthDelimited => identity.GetValue<string>(),
            _ => throw new InvalidDataException("Unsupported filtered friend identity wire type.")
        };
        return item;
    }
}

[ProtoPackable]
internal partial class D69ApproveReq
{
    [ProtoMember(1)] public string Uid { get; set; }
    [ProtoMember(2)] public string TargetUid { get; set; }
    [ProtoMember(3)] public uint? Field3 { get; set; } = 0;
    [ProtoMember(4)] public string? Field4 { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class D69DeleteReq
{
    [ProtoMember(1)] public uint Operation { get; set; }
    [ProtoMember(3)] public D69DeleteReqInner Inner { get; set; }
}

[ProtoPackable]
internal partial class D69DeleteReqInner
{
    [ProtoMember(1)] public string Uid { get; set; }
}
