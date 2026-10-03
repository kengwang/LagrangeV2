using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class SendTuwenArkEventReq(long targetId, bool group, string title, string description, string summary, string jumpUrl, string previewUrl) : ProtocolEvent
{
    public long TargetId { get; } = targetId;
    public bool Group { get; } = group;
    public string Title { get; } = title;
    public string Description { get; } = description;
    public string Summary { get; } = summary;
    public string JumpUrl { get; } = jumpUrl;
    public string PreviewUrl { get; } = previewUrl;
}
internal sealed class SendTuwenArkEventResp : ProtocolEvent { public static SendTuwenArkEventResp Instance { get; } = new(); }
