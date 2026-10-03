using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;
namespace Lagrange.Core.Internal.Events.System;
internal sealed class FetchGroupAdminSettingsEventReq(long groupUin) : ProtocolEvent { public long GroupUin { get; } = groupUin; }
internal sealed class FetchGroupAdminSettingsEventResp(BotGroupAdminSettingsResult settings) : ProtocolEvent { public BotGroupAdminSettingsResult Settings { get; } = settings; }
