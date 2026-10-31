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

internal class HighwayContext : IDisposable
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

        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("Accept-Encoding", "identity");
        _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2)");
        
        _sequence = 0;
        _chunkSize = context.Config.HighwayChunkSize;
        _concurrent = (int)context.Config.HighwayConcurrent;
        if (_chunkSize is 0 or > 1048576 || _concurrent < 1) throw new ArgumentOutOfRangeException(nameof(context.Config.HighwayChunkSize));
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
                        _url = NormalizeAddress($"{ProtocolHelper.UInt32ToIPV4Addr(pushed.Ip)}:{pushed.Port}", _context.BotUin);
                }
            }
            catch (Exception e)
            {
                _context.LogDebug(Tag, "Invalid pushed highway session payload: {0}", null, e.Message);
            }
        }
        var address = channel.IPLists.FirstOrDefault(x => x.ServiceType == 10)?.IPList.FirstOrDefault();
        if (address is not null && !string.IsNullOrWhiteSpace(address.Server) && address.Port > 0)
            _url = NormalizeAddress($"{address.Server}:{address.Port}", _context.BotUin);
    }

    public async Task<bool> UploadFile(Stream stream, int commandId, ReadOnlyMemory<byte> extendInfo, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!stream.CanRead || !stream.CanSeek || stream.Length == 0) throw new ArgumentException("Highway upload requires a nonempty seekable stream.", nameof(stream));
        using var uploadCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cancellationToken = uploadCancellation.Token;
        if (_ticket == null || _url == null || DateTime.Now - _ticket.Value.Item2 > TimeSpan.FromDays(0.5))
        {
            var resp = await _context.EventContext.SendEvent<HighwaySessionEventResp>(new HighwaySessionEventReq(), cancellationToken);
            _ticket = (resp.SigSession, DateTime.Now);
            _url = NormalizeAddress(resp.HighwayUrls[1][0], _context.BotUin);
        }

        var tasks = new List<Task<bool>>();
        int concurrency = commandId is 1001 or 1005 ? _context.Config.VideoUploadConcurrency : _concurrent;
        var sessionTicket = _ticket.Value.Item1;
        var uploadUrl = _url;

        ulong fileSize = (ulong)stream.Length;
        ulong offset = 0;
        stream.Position = 0;
        var fileMd5 = await MD5.HashDataAsync(stream, cancellationToken);
        stream.Position = 0;
        while (offset < fileSize)
        {
            var buffer = ArrayPool<byte>.Shared.Rent((int)_chunkSize);
            ulong payload = Math.Min(_chunkSize, fileSize - offset);
            try { await stream.ReadExactlyAsync(buffer.AsMemory(0, (int)payload), cancellationToken); }
            catch
            {
                ArrayPool<byte>.Shared.Return(buffer);
                uploadCancellation.Cancel();
                try { await Task.WhenAll(tasks); }
                catch (Exception error) { _context.LogDebug(Tag, "Pending upload also failed: {0}", error.Message); }
                throw;
            }

            ulong currentBlockOffset = offset;
            var task = Task.Run(async () => // closure
            {
                byte[]? upload = null;
                HttpRequestMessage? request = null;
                try
                {
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
                    ServiceTicket = sessionTicket,
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
                upload = ArrayPool<byte>.Shared.Rent(1 + 1 + 4 + 4 + headProto.Length + (int)payload);
                var memory = upload.AsMemory(0, 1 + 1 + 4 + 4 + headProto.Length + (int)payload);

                memory.Span[0] = 0x28;
                BinaryPrimitives.WriteUInt32BigEndian(memory.Span[1..], (uint)headProto.Length);
                BinaryPrimitives.WriteUInt32BigEndian(memory.Span[5..], (uint)payload);
                headProto.Span.CopyTo(memory.Span[9..]);
                bufferSpan.CopyTo(memory.Span[(9 + headProto.Length)..]);
                memory.Span[^1] = 0x29;

                request = new HttpRequestMessage(HttpMethod.Post, uploadUrl)
                {
                    Content = new ReadOnlyMemoryContent(memory), Headers = { { "Connection", end ? "close" : "keep-alive" } }
                };

                    using var response = await _client.SendAsync(request, cancellationToken);
                    response.EnsureSuccessStatusCode();
                    var responseBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                    var reader = new BinaryPacket(responseBytes.AsSpan());

                    if (reader.Read<byte>() == 0x28)
                    {
                        int headLen = reader.Read<int>();
                        int bodyLen = reader.Read<int>();
                        if (headLen < 1 || bodyLen < 0 || (long)headLen + bodyLen + 10 != responseBytes.Length) throw new InvalidDataException("Highway response frame lengths do not match the payload.");
                        var respHead = reader.CreateSpan(headLen);
                        var body = GC.AllocateUninitializedArray<byte>(bodyLen);
                        reader.ReadBytes(body.AsSpan());

                        if (reader.Read<byte>() == 0x29)
                        {
                            var obj = ProtoHelper.Deserialize<RespDataHighwayHead>(respHead);
                            _context.LogDebug(Tag, "Highway Block Result: {0} | {1} | {2}", obj.ErrorCode, obj.MsgSegHead?.RetCode, Convert.ToHexString(body));
                            ValidateAcknowledgement(obj, fileSize, currentBlockOffset, checked((uint)payload), checked((uint)sequence));
                            return true;
                        }
                    }
                    throw new InvalidDataException("Invalid Highway response frame.");
                }
                catch
                {
                    uploadCancellation.Cancel();
                    throw;
                }
                finally
                {
                    request?.Dispose();
                    ArrayPool<byte>.Shared.Return(buffer);
                    if (upload is not null) ArrayPool<byte>.Shared.Return(upload);
                }

            });
            offset += payload;

            tasks.Add(task);
            if (tasks.Count == concurrency)
            {
                await Task.WhenAll(tasks);
                tasks.Clear();
            }
        }
        await Task.WhenAll(tasks);
        return true;
    }

    internal static string NormalizeAddress(string address, long uin)
    {
        var uri = new UriBuilder(address.Contains("://", StringComparison.Ordinal) ? address : "http://" + address);
        if (uri.Scheme is not ("http" or "https")) throw new InvalidDataException("Invalid Highway address scheme.");
        if (uri.Path is "" or "/") { uri.Path = "/cgi-bin/httpconn"; uri.Query = $"htcmd=0x6FF0087&uin={uin}"; }
        return uri.Uri.AbsoluteUri;
    }

    internal static void ValidateAcknowledgement(RespDataHighwayHead response, ulong size, ulong offset, uint length, uint? sequence = null)
    {
        if (response.ErrorCode != 0 || response.MsgSegHead?.RetCode is > 0) throw new Lagrange.Core.Exceptions.OperationException(checked((int)(response.ErrorCode != 0 ? response.ErrorCode : response.MsgSegHead!.RetCode!.Value)), "Highway upload was rejected.");
        if (response.MsgBaseHead is { Seq: > 0 } header && sequence is { } expected && header.Seq != expected)
            throw new InvalidDataException("Highway acknowledgement has a mismatched sequence.");
        if (response.MsgSegHead is { } head && (head.Filesize is { } actualSize && actualSize != size || head.DataOffset is { } actualOffset && actualOffset != offset || head.DataLength is { } actualLength && actualLength != length))
            throw new InvalidDataException("Highway acknowledgement does not match the uploaded block.");
    }

    internal async Task<bool> UploadCustomFace(Stream stream, string emojiId, byte[] serviceTicket, CancellationToken cancellationToken = default)
    {
        if (!stream.CanSeek) throw new ArgumentException("Custom face upload requires a seekable stream.", nameof(stream));
        if (serviceTicket.Length == 0) throw new ArgumentException("Upload ticket is empty.", nameof(serviceTicket));
        if (_url is null)
        {
            var session = await _context.EventContext.SendEvent<HighwaySessionEventResp>(new HighwaySessionEventReq(), cancellationToken);
            _url = NormalizeAddress(session.HighwayUrls[1][0], _context.BotUin);
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
        using var request = new HttpRequestMessage(HttpMethod.Post, _url) { Content = new ByteArrayContent(payload) };
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

    public void Dispose() => _client.Dispose();
}
