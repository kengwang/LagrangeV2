using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class FetchQidianCorpEventReq(long userUin) : ProtocolEvent { public long UserUin { get; } = userUin; }
internal sealed class FetchQidianCorpEventResp(BotQidianCorpResult result) : ProtocolEvent { public BotQidianCorpResult Result { get; } = result; }
