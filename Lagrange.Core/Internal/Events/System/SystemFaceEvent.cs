using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal class GetSystemFacesEventReq(bool refresh) : ProtocolEvent
{
    public bool Refresh { get; } = refresh;
}

internal class GetSystemFacesEventResp(IReadOnlyList<BotSystemFacePack> packs) : ProtocolEvent
{
    public IReadOnlyList<BotSystemFacePack> Packs { get; } = packs;
}
