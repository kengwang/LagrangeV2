using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class FetchBuddyArkEventReq(long userUin) : ProtocolEvent { public long UserUin { get; } = userUin; }
internal sealed class FetchBuddyArkEventResp(string ark) : ProtocolEvent { public string Ark { get; } = ark; }
internal sealed class FetchGroupArkEventReq(long groupUin) : ProtocolEvent { public long GroupUin { get; } = groupUin; }
internal sealed class FetchGroupArkEventResp(string ark) : ProtocolEvent { public string Ark { get; } = ark; }
