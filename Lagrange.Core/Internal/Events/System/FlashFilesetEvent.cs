using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class ListFlashFilesetsEventReq(uint limit) : ProtocolEvent { public uint Limit { get; } = limit; }
internal sealed class ListFlashFilesetsEventResp(BotFlashFilesetResult result) : ProtocolEvent { public BotFlashFilesetResult Result { get; } = result; }
internal sealed class GetFlashFilesetEventReq(string filesetUuid) : ProtocolEvent { public string FilesetUuid { get; } = filesetUuid; }
internal sealed class GetFlashFilesetEventResp(BotFlashFilesetResult result) : ProtocolEvent { public BotFlashFilesetResult Result { get; } = result; }
