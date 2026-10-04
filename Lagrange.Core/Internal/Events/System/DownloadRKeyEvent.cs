using Lagrange.Core.Events;
using Lagrange.Core.Common.Response;
namespace Lagrange.Core.Internal.Events.System;
internal sealed class DownloadRKeyEventReq : ProtocolEvent;
internal sealed class DownloadRKeyEventResp(IReadOnlyList<BotDownloadRKey> keys) : ProtocolEvent { public IReadOnlyList<BotDownloadRKey> Keys { get; } = keys; }
