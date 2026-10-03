using System.Security.Cryptography;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Cryptography;
using Lagrange.Core.Utility.Extension;

namespace Lagrange.Core.Internal.Context;

public sealed class FlashTransferContext : IDisposable
{
    private const string Tag = nameof(FlashTransferContext);
    private readonly BotContext _botContext;
    private readonly HttpClient _client;
    private readonly string? _url;
    private const uint ChunkSize = 1024 * 1024;

    internal FlashTransferContext(BotContext botContext)
    {
        _botContext = botContext;
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
        _url = "https://multimedia.qfile.qq.com/sliceupload";
    }

    public async Task<bool> UploadFile(string uKey, uint appId, Stream bodyStream, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uKey);
        ArgumentNullException.ThrowIfNull(bodyStream);
        if (!bodyStream.CanSeek) throw new ArgumentException("Flash transfer requires a seekable stream.", nameof(bodyStream));
        if (bodyStream.Length == 0) throw new ArgumentException("Flash transfer cannot upload an empty stream.", nameof(bodyStream));
        var sha1StateVs = new FlashTransferSha1StateV { State = [] };
        var chunkCount = (uint)((bodyStream.Length + ChunkSize - 1) / ChunkSize);

        var sha1Stream = new Sha1Stream();
        for (uint i = 0; i < chunkCount; i++)
        {
            if (i != chunkCount - 1)
            {
                var accLength = (int)((i + 1) * ChunkSize);
                var accBuffer = new byte[accLength];
            
                bodyStream.Position = 0;
                await bodyStream.ReadExactlyAsync(accBuffer, 0, accLength, cancellationToken);
            
                var accSpan = accBuffer.AsSpan();
                var digest = new byte[20];
                sha1Stream.Update(accSpan);
                sha1Stream.Hash(digest, false);
                sha1Stream.Reset();
                sha1StateVs.State.Add(digest.ToArray());
            }
            else
            {
                bodyStream.Position = 0;
                sha1StateVs.State.Add(bodyStream.Sha1());
            }
        }

        for (uint i = 0; i < chunkCount; i++)
        {
            var chunkStart = (long)(i * ChunkSize);
            var chunkLength = (int)Math.Min(ChunkSize, bodyStream.Length - chunkStart);

            bodyStream.Position = chunkStart;
            var uploadBuffer = new byte[chunkLength];
            await bodyStream.ReadExactlyAsync(uploadBuffer, 0, chunkLength, cancellationToken);

            var success = await UploadChunk(uKey, appId, (uint)chunkStart, sha1StateVs, uploadBuffer, cancellationToken);
            if (!success) return false;
        }

        return true;
    }

    private async Task<bool> UploadChunk(string uKey, uint appId, uint start, FlashTransferSha1StateV chunkSha1S, byte[] body, CancellationToken cancellationToken)
    {
        var req = new FlashTransferUploadReq
        {
            FieId1 = 0,
            AppId = appId,
            FileId3 = 2,
            Body = new FlashTransferUploadBody
            {
                FieId1 = [],
                UKey = uKey,
                Start = start,
                End = (uint)(start + body.Length - 1),
                Sha1 = SHA1.HashData(body),
                Sha1StateV = chunkSha1S,
                Body = body
            }
        };
        var payload = ProtoHelper.Serialize(req).ToArray();
        using var request = new HttpRequestMessage(HttpMethod.Post, _url)
        {
            Headers =
            {
                { "Accept", "*/*" },
                { "Expect", "100-continue" },
                { "Connection", "Keep-Alive" }
            },
            Content = new ByteArrayContent(payload)
        };
        using var response = await _client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var responseBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var resp = ProtoHelper.Deserialize<FlashTransferUploadResp>(responseBytes);

        if (resp.Status != "success")
        {
            _botContext.LogError(Tag,
                $"FlashTransfer Upload chunk {start} failed: {resp.Status}");
            return false;
        }

        return true;
    }

    public void Dispose() => _client.Dispose();
}
