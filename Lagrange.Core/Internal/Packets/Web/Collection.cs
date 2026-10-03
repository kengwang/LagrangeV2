using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Web;

#pragma warning disable CS8618
[ProtoPackable] internal partial class CollectionHeadReq { [ProtoMember(1)] public ulong Uin { get; set; } [ProtoMember(2)] public uint Sequence { get; set; } [ProtoMember(3)] public uint CommandType { get; set; } [ProtoMember(4)] public uint OperationId { get; set; } [ProtoMember(5)] public ulong ClientVersion { get; set; } [ProtoMember(6)] public uint Platform { get; set; } [ProtoMember(7)] public uint TicketType { get; set; } [ProtoMember(10)] public uint Reserved { get; set; } [ProtoMember(11)] public string Ticket { get; set; } = string.Empty; [ProtoMember(14)] public uint Field14 { get; set; } [ProtoMember(15)] public uint Field15 { get; set; } }
[ProtoPackable] internal partial class CollectionListReq { [ProtoMember(1)] public uint Field1 { get; set; } [ProtoMember(2)] public uint Field2 { get; set; } [ProtoMember(3)] public uint Field3 { get; set; } [ProtoMember(4)] public ulong Timestamp { get; set; } [ProtoMember(5)] public uint OrderType { get; set; } [ProtoMember(6)] public ulong GroupId { get; set; } [ProtoMember(7)] public uint Count { get; set; } [ProtoMember(8)] public uint SearchDown { get; set; } [ProtoMember(9)] public uint Field9 { get; set; } }
[ProtoPackable] internal partial class CollectionRequestOperation { [ProtoMember(20000)] public CollectionListReq? GetCollectionList { get; set; } }
[ProtoPackable] internal partial class CollectionRequestBody { [ProtoMember(1)] public CollectionRequestOperation Operation { get; set; } = new(); }
[ProtoPackable] internal partial class CollectionResponseHead { [ProtoMember(101)] public int RetCode { get; set; } [ProtoMember(102)] public string? RetMsg { get; set; } }
[ProtoPackable] internal partial class CollectionResponseOperation { [ProtoMember(20000)] public CollectionListResp? GetCollectionList { get; set; } }
[ProtoPackable] internal partial class CollectionResponseBody { [ProtoMember(2)] public CollectionResponseOperation Operation { get; set; } = new(); }
[ProtoPackable] internal partial class CollectionListResp { [ProtoMember(1)] public List<CollectionItem>? Items { get; set; } [ProtoMember(2)] public uint TotalCount { get; set; } [ProtoMember(3)] public uint ReachedBottom { get; set; } }
[ProtoPackable] internal partial class CollectionItem { [ProtoMember(1)] public string? Id { get; set; } [ProtoMember(2)] public uint Type { get; set; } [ProtoMember(9)] public ulong CreateTime { get; set; } [ProtoMember(10)] public ulong CollectTime { get; set; } [ProtoMember(11)] public ulong ModifyTime { get; set; } [ProtoMember(15)] public CollectionSummary? Summary { get; set; } [ProtoMember(18)] public string? ShareUrl { get; set; } [ProtoMember(4)] public CollectionAuthor? Author { get; set; } }
[ProtoPackable] internal partial class CollectionAuthor { [ProtoMember(2)] public ulong NumId { get; set; } [ProtoMember(6)] public string? Uid { get; set; } }
[ProtoPackable] internal partial class CollectionSummary { [ProtoMember(1)] public CollectionTextSummary? Text { get; set; } }
[ProtoPackable] internal partial class CollectionTextSummary { [ProtoMember(1)] public string? Text { get; set; } }
