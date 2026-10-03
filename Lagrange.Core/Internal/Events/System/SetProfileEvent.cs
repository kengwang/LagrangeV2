using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal class SetProfileEventReq(string? nickname, string? personalNote, int? sex) : ProtocolEvent
{
    public string? Nickname { get; } = nickname;
    public string? PersonalNote { get; } = personalNote;
    public int? Sex { get; } = sex;
}

internal class SetProfileEventResp : ProtocolEvent
{
    public static readonly SetProfileEventResp Default = new();
}

internal class SetSelfLongNickEventReq(string longNick) : ProtocolEvent
{
    public string LongNick { get; } = longNick;
}

internal class SetSelfLongNickEventResp : ProtocolEvent
{
    public static readonly SetSelfLongNickEventResp Default = new();
}
