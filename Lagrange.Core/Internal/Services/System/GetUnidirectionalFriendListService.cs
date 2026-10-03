using System.Text.Json;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<GetUnidirectionalFriendListEventReq>(Protocols.All)]
[Service("MQUpdateSvc_com_qq_ti.web.OidbSvc.0xe17_0")]
internal sealed class GetUnidirectionalFriendListService : OidbService<GetUnidirectionalFriendListEventReq, GetUnidirectionalFriendListEventResp, DE17ReqBody, DE17RespBody>
{
    protected override uint Command => 0xe17;
    protected override uint Service => 0;

    protected override Task<DE17ReqBody> ProcessRequest(GetUnidirectionalFriendListEventReq request, BotContext context) =>
        Task.FromResult(new DE17ReqBody
        {
            JsonBody = $"{{\"uint64_uin\":\"{context.BotUin}\",\"uint64_top\":0,\"uint32_req_num\":99,\"bytes_cookies\":\"\"}}",
        });

    protected override Task<GetUnidirectionalFriendListEventResp> ProcessResponse(DE17RespBody response, BotContext context)
    {
        if (string.IsNullOrWhiteSpace(response.JsonBody)) throw new OperationException(-1, "0xe17 returned an empty friend list body.");
        try
        {
            using var document = JsonDocument.Parse(response.JsonBody);
            var entries = new List<IReadOnlyDictionary<string, string>>();
            if (document.RootElement.TryGetProperty("rpt_block_list", out var list) && list.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in list.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object) continue;
                    entries.Add(item.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.ToString()));
                }
            }
            return Task.FromResult(new GetUnidirectionalFriendListEventResp(new BotUnidirectionalFriendResult { Entries = entries }));
        }
        catch (JsonException exception)
        {
            throw new OperationException(-1, $"0xe17 returned invalid friend list JSON: {exception.Message}");
        }
    }
}
