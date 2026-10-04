using Lagrange.Core.Events;

using Lagrange.Core.Internal.Packets.Service.Migration;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class ClickKeyboardEventReq(Oidb0x112eReq body) : ProtocolEvent { public Oidb0x112eReq Body { get; } = body; }

internal sealed class ClickKeyboardEventResp(Oidb0x112eResp body) : ProtocolEvent { public Oidb0x112eResp Body { get; } = body; }

internal sealed class RequestDatabaseKeyEventReq(Oidb0xcdeReq body) : ProtocolEvent { public Oidb0xcdeReq Body { get; } = body; }

internal sealed class RequestDatabaseKeyEventResp(Oidb0xcdeResp body) : ProtocolEvent { public Oidb0xcdeResp Body { get; } = body; }

internal sealed class GetUserStatusEventReq(OidbStrangerStatusReq body) : FetchStrangerEventReqBase { public OidbStrangerStatusReq Body { get; } = body; }

internal class GetUserStatusEventResp(OidbStrangerStatusResp body) : ProtocolEvent { public OidbStrangerStatusResp Body { get; } = body; }

internal sealed class GetGroupTodoListEventReq(OidbQueryGroupTopBannersReq body) : ProtocolEvent { public OidbQueryGroupTopBannersReq Body { get; } = body; }

internal sealed class GetGroupTodoListEventResp(OidbQueryGroupTopBannersResp body) : ProtocolEvent { public OidbQueryGroupTopBannersResp Body { get; } = body; }

internal sealed class GetAiVoiceListEventReq(OidbAiVoiceListReq body) : ProtocolEvent { public OidbAiVoiceListReq Body { get; } = body; }

internal sealed class GetAiVoiceListEventResp(OidbAiVoiceListResp body) : ProtocolEvent { public OidbAiVoiceListResp Body { get; } = body; }

internal sealed class SynthesizeAiVoiceEventReq(OidbAiVoiceReq body) : ProtocolEvent { public OidbAiVoiceReq Body { get; } = body; }

internal sealed class SynthesizeAiVoiceEventResp(OidbAiVoiceResp body) : ProtocolEvent { public OidbAiVoiceResp Body { get; } = body; }

internal sealed class MiniAppShareEventReq(MiniAppShareReq body) : ProtocolEvent { public MiniAppShareReq Body { get; } = body; }

internal sealed class MiniAppShareEventResp(MiniAppShareResp body) : ProtocolEvent { public MiniAppShareResp Body { get; } = body; }
internal sealed class TranscribeGroupVoiceEventReq(PttTransReq body) : ProtocolEvent { public PttTransReq Body { get; } = body; }
internal sealed class TranscribePrivateVoiceEventReq(PttTransReq body) : ProtocolEvent { public PttTransReq Body { get; } = body; }
internal sealed class TranscribeVoiceEventResp(PttTransResp body) : ProtocolEvent { public PttTransResp Body { get; } = body; }
