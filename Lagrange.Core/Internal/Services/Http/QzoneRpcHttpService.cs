using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Lagrange.Core.Common;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.Http;

[HttpServiceAttribute("qzone.rpc", "POST", "/http2rpc/gotrpc/v1", "qzone.qq.com", AuthInjection = HttpAuthInjection.UrlBknFromPSkey)]
[EventSubscribe<QzoneRpcEvent>(Protocols.All)]
internal sealed class QzoneRpcHttpService : HttpService<QzoneRpcEvent, QzoneRpcResult>
{
    protected override async Task<HttpRequestMessage> BuildRequestAsync(BotContext context, QzoneRpcEvent request, CancellationToken ct)
    {
        var message = await CreateJsonRequestAsync(context, HttpMethod.Post,
            new Uri($"https://h5.qzone.qq.com/http2rpc/gotrpc/v1/{request.Service}/{request.Method}"), Encoding.UTF8.GetBytes(request.Parameters.ToJsonString()), ct);
        message.Headers.Referrer = new Uri("https://h5.qzone.qq.com/pc-qzone/index.html");
        message.Headers.Add("x-web-qua", "V1_HT5_QZ_9.9.32_001_IDC_B");
        message.Headers.Add("x-oidb", new JsonObject { ["uint32_command"] = request.Command, ["uint32_service_type"] = request.SubCommand }.ToJsonString());
        return message;
    }

    protected override Task<QzoneRpcResult> ParseResponseAsync(BotContext context, QzoneRpcEvent request, HttpResponseMessage response, ReadOnlyMemory<byte> payload, CancellationToken ct)
    {
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        int code = root.GetProperty("retcode").GetInt32();
        if (code != 0) throw new HttpServiceException(request.Method, root.TryGetProperty("message", out var message) ? message.ToString() : "Qzone RPC request failed.", businessCode: code);
        var data = root.GetProperty("data");
        if (data.ValueKind != JsonValueKind.Object) throw new InvalidDataException("Qzone RPC data must be an object.");
        return Task.FromResult(new QzoneRpcResult(data.Clone()));
    }
}
