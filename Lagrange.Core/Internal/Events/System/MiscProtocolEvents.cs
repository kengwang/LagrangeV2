using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class TranslateEnToZhEventReq(IReadOnlyList<string> words) : ProtocolEvent
{
    public IReadOnlyList<string> Words { get; } = words;
}
internal sealed class TranslateEnToZhEventResp(IReadOnlyList<string> words) : ProtocolEvent
{
    public IReadOnlyList<string> Words { get; } = words;
}
internal sealed class ImageOcrEventReq(string imageUrl) : ProtocolEvent
{
    public string ImageUrl { get; } = imageUrl;
}
internal sealed class ImageOcrEventResp(BotOcrResult result) : ProtocolEvent
{
    public BotOcrResult Result { get; } = result;
}
