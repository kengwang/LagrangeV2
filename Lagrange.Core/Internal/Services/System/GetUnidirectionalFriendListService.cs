using System.Text.Json;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<GetUnidirectionalFriendListEventReq>(Protocols.All)]
[Service("MQUpdateSvc_com_qq_ti.web.OidbSvc.0xe17_0")]
internal sealed class GetUnidirectionalFriendListService : BaseService<GetUnidirectionalFriendListEventReq, GetUnidirectionalFriendListEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(GetUnidirectionalFriendListEventReq input, BotContext context) =>
        ValueTask.FromResult(ProtoHelper.Serialize(new DE17ReqBody { JsonBody = $"{{\"uint64_uin\":\"{context.BotUin}\",\"uint64_top\":0,\"uint32_req_num\":99,\"bytes_cookies\":\"\"}}" }));

    protected override ValueTask<GetUnidirectionalFriendListEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var response = ProtoHelper.Deserialize<DE17RespBody>(input.Span);
        if (response.ErrorCode != 0) throw new OperationException(response.ErrorCode, response.ErrorMessage);
        if (string.IsNullOrWhiteSpace(response.JsonBody)) throw new InvalidDataException("Single-direction friend response is empty.");
        return ValueTask.FromResult(new GetUnidirectionalFriendListEventResp(new BotUnidirectionalFriendResult { Entries = ReadEntries(response.JsonBody) }));
    }

    internal static IReadOnlyList<IReadOnlyDictionary<string, string>> ReadEntries(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var code = root.GetProperty("ErrorCode").GetInt32();
        if (root.GetProperty("ActionStatus").GetString() != "OK" || code != 0)
            throw new OperationException(code != 0 ? code : -1, root.TryGetProperty("ErrorInfo", out var error) ? error.ToString() : "Single-direction friend request failed.");
        var list = root.GetProperty("rpt_block_list");
        if (list.ValueKind != JsonValueKind.Array) throw new InvalidDataException("Single-direction friend list is invalid.");
        return list.EnumerateArray().Select(item => (IReadOnlyDictionary<string, string>)item.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.ToString())).ToArray();
    }
}
