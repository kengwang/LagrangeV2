using Lagrange.Core.Common;
using System.Buffers.Binary;
using System.Net.Http.Headers;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Web;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Services.Http;

[HttpService("collection.get_list", "POST", "https://collector.weiyun.com/collector.fcg")]
[EventSubscribe<GetCollectionEventReq>(Protocols.All)]
internal sealed class CollectionHttpService : HttpService<GetCollectionEventReq, GetCollectionEventResp>
{

    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, GetCollectionEventReq request, CancellationToken cancellationToken)
    {
        if (request.Count is 0 or > 500) throw new ArgumentOutOfRangeException(nameof(request.Count));
        var pskey = await GetTicketAsync(context, cancellationToken);
        var head = ProtoHelper.Serialize(new CollectionHeadReq { Uin = checked((ulong)context.BotUin), Sequence = 1, CommandType = 1, OperationId = 20000, ClientVersion = 0x6105F5E164, Platform = 4, TicketType = 27, Ticket = pskey, Field14 = 8, Field15 = 9 });
        var body = ProtoHelper.Serialize(new CollectionRequestBody { Operation = new CollectionRequestOperation { GetCollectionList = new CollectionListReq { Timestamp = ulong.MaxValue, OrderType = 2, Count = request.Count, SearchDown = 1 } } });
        var envelope = new byte[16 + head.Length + body.Length];
        envelope[0] = 0x20; envelope[1] = 0x13; envelope[2] = 0x03; envelope[3] = 0x29; envelope[5] = 1;
        BinaryPrimitives.WriteUInt32BigEndian(envelope.AsSpan(6, 4), checked((uint)envelope.Length));
        BinaryPrimitives.WriteUInt32BigEndian(envelope.AsSpan(10, 4), checked((uint)body.Length));
        head.Span.CopyTo(envelope.AsSpan(16));
        body.Span.CopyTo(envelope.AsSpan(16 + head.Length));
        var message = new HttpRequestMessage(HttpMethod.Post, "https://collector.weiyun.com/collector.fcg") { Content = new ByteArrayContent(envelope) };
        message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        message.Headers.TryAddWithoutValidation("Cookie", $"uin={context.BotUin};vt=27;vi={pskey};appid=5004");
        message.Headers.TryAddWithoutValidation("Range", "bytes=0-");
        return message;
    }

    protected override Task<GetCollectionEventResp> ParseResponseAsync(BotContext context, GetCollectionEventReq request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        var bytes = payload.Span;
        if (bytes.Length <= 16 || !bytes[..4].SequenceEqual(new byte[] { 0x20, 0x13, 0x03, 0x29 }))
            throw new HttpServiceException("collection.get_list", $"Collection response envelope is invalid (length={bytes.Length}, prefix={Convert.ToHexString(bytes[..Math.Min(bytes.Length, 16)])}).");
        var bodyLength = BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(10, 4));
        if (bodyLength == 0 || bodyLength >= bytes.Length - 16) throw new HttpServiceException("collection.get_list", "Collection response body is invalid.");
        var head = ProtoHelper.Deserialize<CollectionResponseHead>(bytes.Slice(16, bytes.Length - 16 - (int)bodyLength));
        if (head.RetCode != 0) throw new HttpServiceException("collection.get_list", head.RetMsg ?? "Collection query failed.", businessCode: head.RetCode);
        var body = ProtoHelper.Deserialize<CollectionResponseBody>(bytes[^((int)bodyLength)..]);
        var page = body.Operation?.GetCollectionList ?? throw new HttpServiceException("collection.get_list", "Collection response operation is missing.");
        return Task.FromResult(new GetCollectionEventResp(new BotCollectionResult { TotalCount = page.TotalCount, ReachedBottom = page.ReachedBottom != 0, Items = [.. (page.Items ?? []).Where(x => !string.IsNullOrWhiteSpace(x.Id)).Select(x => new BotCollectionItem { Id = x.Id!, Type = x.Type, CreateTime = x.CreateTime, CollectTime = x.CollectTime, ModifyTime = x.ModifyTime, ShareUrl = x.ShareUrl, Text = x.Summary?.Text?.Text, AuthorUid = x.Author?.Uid, AuthorUin = x.Author is null ? 0 : checked((long)x.Author.NumId) })] }));
    }
}
