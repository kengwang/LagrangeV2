using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class RefreshHttpCookiesEventReq(string[] domains) : ProtocolEvent
{
    public string[] Domains { get; } = domains;
}

internal sealed class RefreshHttpCookiesEventResp : ProtocolEvent;

internal sealed class PostHttpBytesEventReq(Uri endpoint, ReadOnlyMemory<byte> payload, string contentType, IReadOnlyDictionary<string, string>? headers) : ProtocolEvent
{
    public Uri Endpoint { get; } = endpoint;
    public ReadOnlyMemory<byte> Payload { get; } = payload;
    public string ContentType { get; } = contentType;
    public IReadOnlyDictionary<string, string>? Headers { get; } = headers;
}

internal sealed class PostHttpBytesEventResp(byte[] payload) : ProtocolEvent
{
    public byte[] Payload { get; } = payload;
}
