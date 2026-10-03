using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal class GetCustomFaceListEventReq : ProtocolEvent;

internal class GetCustomFaceListEventResp(BotCustomFaceListResult result) : ProtocolEvent
{
    public BotCustomFaceListResult Result { get; } = result;
}

public record CustomFaceLookup(string FaceId, string Md5);

internal class GetCustomFaceDetailEventReq(IReadOnlyList<CustomFaceLookup> entries) : ProtocolEvent
{
    public IReadOnlyList<CustomFaceLookup> Entries { get; } = entries;
}

internal class GetCustomFaceDetailEventResp(BotCustomFaceDetailResult result) : ProtocolEvent
{
    public BotCustomFaceDetailResult Result { get; } = result;
}

internal class ModifyCustomFaceEventReq(string faceId, string md5, string description) : ProtocolEvent
{
    public string FaceId { get; } = faceId;
    public string Md5 { get; } = md5;
    public string Description { get; } = description;
}

internal class ModifyCustomFaceEventResp : ProtocolEvent
{
    public static readonly ModifyCustomFaceEventResp Default = new();
}

internal class DeleteCustomFaceEventReq(string faceId) : ProtocolEvent
{
    public string FaceId { get; } = faceId;
}

internal class DeleteCustomFaceEventResp : ProtocolEvent
{
    public static readonly DeleteCustomFaceEventResp Default = new();
}

internal class MoveCustomFaceEventReq(string faceId, uint position, IReadOnlyList<CustomFaceLookup> entries) : ProtocolEvent
{
    public string FaceId { get; } = faceId;
    public uint Position { get; } = position;
    public IReadOnlyList<CustomFaceLookup> Entries { get; } = entries;
}

internal class MoveCustomFaceEventResp : ProtocolEvent
{
    public static readonly MoveCustomFaceEventResp Default = new();
}
