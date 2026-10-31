using System.Text.Json;
using System.Text.Json.Nodes;
using Lagrange.Core.Events;

namespace Lagrange.Core.Internal.Events.System;

internal sealed class QzoneRpcEvent(string service, string method, string command, string subCommand, JsonObject parameters) : ProtocolEvent
{
    public string Service { get; } = service;
    public string Method { get; } = method;
    public string Command { get; } = command;
    public string SubCommand { get; } = subCommand;
    public JsonObject Parameters { get; } = parameters;
}
internal sealed class QzoneRpcResult(JsonElement data) : ProtocolEvent { public JsonElement Data { get; } = data; }
