using System.Buffers;
using System.Buffers.Binary;
using System.Security.Cryptography;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Extension;
using Lagrange.Proto.Jce;

namespace Lagrange.Core.Internal.Context;

internal class HighwayContext
{
    private const string Tag = nameof(HighwayContext);
    
    private readonly BotContext _context;

    private readonly HttpClient _client;
    
    private readonly ulong _chunkSize;

    private readonly int _concurrent;
    
    private int _sequence;

    private (byte[], DateTime)? _ticket;

    private string? _url;
    private byte[]? _sessionKey;
    
    public HighwayContext(BotContext context)
    {
        _context = context;

        _client = new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true });
        _client.DefaultRequestHeaders.Add("Accept-Encoding", "identity");
        _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2)");
        
        _sequence = 0;
        _chunkSize = context.Config.HighwayChunkSize;
        _concurrent = (int)context.Config.HighwayConcurrent;
    }

    internal void ApplyPush(FileStoragePushFSSvcList storage)
    {
        if (storage.BigDataChannel is not { } channel || channel.SigSession.Length == 0) return;
        _ticket = (channel.SigSession, DateTime.Now);
        _sessionKey = channel.KeySession;
        if (channel.PbBuf.Length > 0)
        {
            try
            {
                var response = ProtoHelper.Deserialize<C501RspBody>(channel.PbBuf);
                if (response.RspBody is { } body)
                {
                    _ticket = (body.SigSession.Length > 0 ? body.SigSession : channel.SigSession, DateTime.Now);
                    _sessionKey = body.SessionKey.Length > 0 ? body.SessionKey : channel.KeySession;
                    var pushed = body.Addrs.FirstOrDefault(x => x.ServiceType == 10)?.Addrs.FirstOrDefault();
                    if (pushed is not null && pushed.Port > 0)
                        _url = $"{ProtocolHelper.UInt32ToIPV4Addr(pushed.Ip)}:{pushed.Port}/cgi-bin/httpconn?htcmd=0x6FF0087&uin={_context.Keystore.Uin}";
                }
            }
            catch (Exception e)
            {
                _context.LogDebug(Tag, "Invalid pushed highway session payload: {0}", null, e.Message);
            }
        }
        var address = channel.IPLists.FirstOrDefault(x => x.ServiceType == 10)?.IPList.FirstOrDefault();
        if (address is not null && !string.IsNullOrWhiteSpace(address.Server) && address.Port > 0)
            _url = $"{address.Server}:{address.Port}";
    }

    public async Task<bool> UploadFile(Stream stream, int commandId, ReadOnlyMemory<byte> extendInfo, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_ticket == null || _url == null || DateTime.Now - _ticket.Value.Item2 > TimeSpan.FromDays(0.5))
        {
            var resp = await _context.EventContext.SendEvent<HighwaySessionEventResp>(new HighwaySessionEventReq(), cancellationToken);
            _ticket = (resp.SigSession, DateTime.Now);
            _url = resp.HighwayUrls[1][0];
        }

        var tasks = new List<Task<bool>>();
        bool result = true;

        ulong fileSize = (ulong)stream.Length;
        ulong offset = 0;
        var fileMd5 = stream.Md5();
        while (offset < fileSize)
        {
            var buffer = ArrayPool<byte>.Shared.Rent((int)_chunkSize);
            ulong payload = (ulong)await stream.ReadAsync(buffer.AsMemory(0, (int)_chunkSize), cancellationToken);

            ulong currentBlockOffset = offset;
            var task = Task.Run(async () => // closure
            {
                cancellationToken.ThrowIfCancellationRequested();
                var bufferSpan = buffer.AsSpan(0, (int)payload);
                int sequence = GetNewSequence();

                var head = new DataHighwayHead
                {
                    Version = 1,
                    Uin = _context.Keystore.Uin.ToString(),
                    Command = "PicUp.DataUp",
                    Seq = (uint)sequence,
                    AppId = (uint)_context.AppInfo.AppId,
                    DataFlag = 16,
                    CommandId = (uint)commandId,
                };
                var segHead = new SegHead
                {
                    Filesize = fileSize,
                    DataOffset = currentBlockOffset,
                    DataLength = (uint)payload,
                    ServiceTicket = _ticket.Value.Item1,
                    Md5 = MD5.HashData(bufferSpan),
                    FileMd5 = fileMd5,
                };
                var loginHead = new LoginSigHead
                {
                    Uint32LoginSigType = 8,
                    BytesLoginSig = _context.Keystore.WLoginSigs.A2,
                    AppId = (uint)_context.AppInfo.AppId
                };
                var highwayHead = new ReqDataHighwayHead
                {
                    MsgBaseHead = head,
                    MsgSegHead = segHead,
                    BytesReqExtendInfo = extendInfo,
                    Timestamp = 0,
                    MsgLoginSigHead = loginHead
                };
                var headProto = ProtoHelper.Serialize(highwayHead);

                bool end = currentBlockOffset + payload >= fileSize;
                var upload = ArrayPool<byte>.Shared.Rent(1 + 1 + 4 + 4 + headProto.Length + (int)payload);
                var memory = upload.AsMemory(0, 1 + 1 + 4 + 4 + headProto.Length + (int)payload);

                memory.Span[0] = 0x28;
                BinaryPrimitives.WriteUInt32BigEndian(memory.Span[1..], (uint)headProto.Length);
                BinaryPrimitives.WriteUInt32BigEndian(memory.Span[5..], (uint)payload);
                headProto.Span.CopyTo(memory.Span[9..]);
                bufferSpan.CopyTo(memory.Span[(9 + headProto.Length)..]);
                memory.Span[^1] = 0x29;

                var request = new HttpRequestMessage(HttpMethod.Post, $"http://{_url}")
                {
                    Content = new ReadOnlyMemoryContent(memory), Headers = { { "Connection", end ? "close" : "keep-alive" } }
                };

                try
                {
                    var response = await _client.SendAsync(request, cancellationToken);
                    var reader = new BinaryPacket((await response.Content.ReadAsByteArrayAsync(cancellationToken)).AsSpan());

                    if (reader.Read<byte>() == 0x28)
                    {
                        int headLen = reader.Read<int>();
                        int bodyLen = reader.Read<int>();
                        var respHead = reader.CreateSpan(headLen);
                        var body = GC.AllocateUninitializedArray<byte>(bodyLen);
                        reader.ReadBytes(body.AsSpan());

                        if (reader.Read<byte>() == 0x29)
                        {
                            var obj = ProtoHelper.Deserialize<RespDataHighwayHead>(respHead);
                            _context.LogDebug(Tag, "Highway Block Result: {0} | {1} | {2}", obj.ErrorCode, obj.MsgSegHead?.RetCode, Convert.ToHexString(body));
                            return obj.ErrorCode == 0;
                        }
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception e)
                {
                    _context.LogError(Tag, "Highway HTTP error: {0}", e, e.Message);
                    if (e.StackTrace is { } stack) _context.LogDebug(Tag, stack);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                    ArrayPool<byte>.Shared.Return(upload);
                    request.Dispose();
                    new ReadOnlyMemoryContent(memory).Dispose();
                }

                return false;
            });
            offset += payload;

            tasks.Add(task);
            if (tasks.Count == (_concurrent))
            {
                var successBlocks = await Task.WhenAll(tasks);
                foreach (bool t in successBlocks) result &= t;
                tasks.Clear();
            }
            
            if (tasks.Count != 0)
            {
                var finalBlocks = await Task.WhenAll(tasks);
                foreach (bool t in finalBlocks) result &= t;
                tasks.Clear();
            }
        }

        return result;
    }

    internal async Task<bool> UploadCustomFace(Stream stream, string emojiId, byte[] serviceTicket, CancellationToken cancellationToken = default)
    {
        if (!stream.CanSeek) throw new ArgumentException("Custom face upload requires a seekable stream.", nameof(stream));
        if (serviceTicket.Length == 0) throw new ArgumentException("Upload ticket is empty.", nameof(serviceTicket));
        if (_url is null)
        {
            var session = await _context.EventContext.SendEvent<HighwaySessionEventResp>(new HighwaySessionEventReq(), cancellationToken);
            _url = session.HighwayUrls[1][0];
        }
        stream.Position = 0;
        var bytes = new byte[checked((int)stream.Length)];
        await stream.ReadExactlyAsync(bytes, cancellationToken);
        var md5 = MD5.HashData(bytes);
        var head = new FavEmojiHighwayHead
        {
            BaseHead = new FavEmojiHighwayBaseHead { Version = 1, Uin = _context.Keystore.Uin.ToString(), Command = "PicUp.DataUp", Sequence = (uint)GetNewSequence(), FileSize = (ulong)bytes.Length, DataFlag = 16, CommandId = 9 },
            SegHead = new FavEmojiHighwaySegHead { FileSize = (ulong)bytes.Length, DataLength = (ulong)bytes.Length, ServiceTicket = serviceTicket, Md5 = md5, FileMd5 = md5 },
            EmojiIdWrap = new FavEmojiIdWrap { EmojiId = emojiId }, Field8 = 9
        };
        var headBytes = ProtoHelper.Serialize(head);
        var payload = new byte[1 + 4 + 4 + headBytes.Length + bytes.Length + 1];
        payload[0] = 0x28;
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(1), (uint)headBytes.Length);
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(5), (uint)bytes.Length);
        headBytes.Span.CopyTo(payload.AsSpan(9)); bytes.CopyTo(payload.AsSpan(9 + headBytes.Length)); payload[^1] = 0x29;
        using var request = new HttpRequestMessage(HttpMethod.Post, $"http://{_url}") { Content = new ByteArrayContent(payload) };
        using var response = await _client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var responseBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var reader = new BinaryPacket(responseBytes.AsSpan());
        if (reader.Read<byte>() != 0x28) return false;
        var headLength = reader.Read<int>(); var bodyLength = reader.Read<int>();
        var responseHead = reader.CreateSpan(headLength); reader.ReadBytes(new byte[bodyLength]);
        if (reader.Read<byte>() != 0x29) return false;
        return ProtoHelper.Deserialize<RespDataHighwayHead>(responseHead).ErrorCode == 0;
    }

    private int GetNewSequence()
    {
        Interlocked.CompareExchange(ref _sequence, 0, 100000);
        return Interlocked.Increment(ref _sequence);
    }
}
