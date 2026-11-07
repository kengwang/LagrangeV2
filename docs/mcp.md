# MCP

Lagrange.Milky exposes its Milky API endpoints as MCP tools using
`FastEndpoints.Mcp 1.0.0-beta.5` and the official MCP C# SDK HTTP transport.

## Configuration

MCP is disabled by default. Merge these Milky settings into your existing
`appsettings.json` and restart Milky:

```json
{
  "Milky": {
    "AccessToken": "your-access-token",
    "HttpServer": {
      "Host": "127.0.0.1",
      "Port": 3000
    },
    "Api": {
      "Mcp": {
        "Enabled": true
      }
    }
  }
}
```

The environment variable `Milky__Api__Mcp__Enabled=true` also enables MCP.
The endpoint is `/mcp` on the existing Milky HTTP server, for example
`http://127.0.0.1:3000/mcp`. Configure an MCP client that supports HTTP transport
with that URL and this request header:

```text
Authorization: Bearer your-access-token
```

The route and tool visibility use the existing Milky authentication rules.
When `Milky.AccessToken` is empty or omitted, MCP is accessible without a token.
The existing `access_token` query parameter is also accepted; the authorization
header avoids placing the token in the connection URL.

## Tools and arguments

Every Milky `/api/*` endpoint is available under its route suffix. For example,
`/api/send_group_message` becomes `send_group_message`. The tool list includes
descriptions, input and output JSON Schemas, and behavior annotations.

Arguments use the existing Milky JSON property names. For example,
`send_group_message` accepts:

```json
{
  "group_id": 123456789,
  "message": [
    {"type": "text", "data": {"text": "Hello from MCP"}}
  ]
}
```

Request defaults, validation, polymorphic message segments and optional
`message_style` values follow the corresponding HTTP API. Tools without request
parameters, such as `get_impl_info`, accept `{}`. Unknown arguments are rejected.

The same bot session handles MCP and HTTP calls. MCP exposes the complete API,
including message sending, group administration, raw SSO packets, cookies and
database keys. Clients with access to MCP can perform these operations. Tool
annotations describe side effects; authorization is enforced by the route and
visibility filter.

## Results and errors

Tool results contain the existing Milky response as text. When the response
matches the advertised object schema, the result also includes `structuredContent`.

```json
{
  "status": "ok",
  "retcode": 0,
  "message": null,
  "data": {}
}
```

Always inspect `status` and `retcode`. Milky business failures use HTTP 200 and
therefore retain `isError=false` in the official bridge, with `status=failed` and
a nonzero `retcode` inside the response.

Binding and validation failures return `isError=true` with `validationErrors`.
Unhandled endpoint exceptions return `isError=true` with a generic error message
by default; exception details are logged on the server. Set
`Milky.Api.Mcp.IncludeExceptionDetails=true` (or the environment variable
`Milky__Api__Mcp__IncludeExceptionDetails=true`) and restart Milky to also return
the exception message, type, stack trace, tool name and trace ID to MCP clients.
HTTP service errors additionally include the service URL without its query,
HTTP status, business code and up to 4096 bytes of the upstream error response.
Enable this only for MCP clients allowed to read diagnostic details. Existing
binding and validation errors keep their original format. Requests are not
automatically retried by Milky's MCP integration.

## Native AOT verification

Native AOT publishing remains enabled. Milky registers generated JSON metadata for
MCP payloads and retains the official bridge's reflection metadata. JSON reflection
is explicitly enabled for the bridge's internal anonymous error responses.
The beta bridge and ASP.NET Core still emit trim/AOT analysis warnings for these
reflection paths. Runtime compatibility is checked by publishing and executing
the native smoke program.

Run the real HTTP transport smoke checks:

```powershell
dotnet run --project Lagrange.Milky.AotSmoke -- --mcp
dotnet publish Lagrange.Milky.AotSmoke -c Release -r win-x64
./Lagrange.Milky.AotSmoke/bin/Release/net10.0/win-x64/publish/Lagrange.Milky.AotSmoke.exe --mcp --require-native
```

The checks start a loopback Kestrel listener and connect with the official MCP
client. They use real endpoint discovery, binding, validation and local bot queries,
without logging in or sending QQ operations. They verify configuration, authentication,
tool coverage, schemas, successful calls, business failures and protocol errors.
