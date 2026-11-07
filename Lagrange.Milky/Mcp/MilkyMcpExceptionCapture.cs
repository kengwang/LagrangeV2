using System.Diagnostics;
using System.Text.Json.Nodes;
using FastEndpoints;
using Lagrange.Core.Exceptions;
using ModelContextProtocol.Protocol;

namespace Lagrange.Milky.Mcp;

internal sealed class MilkyMcpExceptionCapture : IGlobalPostProcessor
{
    // The bridge catches endpoint exceptions before the MCP filter sees them.
    internal static readonly AsyncLocal<MilkyMcpExceptionCapture?> Current = new();
    private Exception? _exception;

    public Task PostProcessAsync(IPostProcessorContext context, CancellationToken ct)
    {
        if (Current.Value is { } capture && context.ExceptionDispatchInfo is { } exception)
            capture._exception = exception.SourceException;
        return Task.CompletedTask;
    }

    internal CallToolResult WithDetails(CallToolResult result, string toolName)
    {
        if (_exception is null || result.IsError != true) return result;

        var cause = _exception.GetBaseException();
        var error = new JsonObject
        {
            ["error"] = cause.Message,
            ["exceptionType"] = cause.GetType().FullName,
            ["details"] = _exception.ToString(),
            ["tool"] = toolName,
            ["traceId"] = Activity.Current?.TraceId.ToString()
        };
        if (_exception is HttpServiceException http)
        {
            error["service"] = http.Service;
            error["statusCode"] = http.StatusCode;
            error["businessCode"] = http.BusinessCode;
            error["responseBody"] = http.ResponseBody;
        }
        return new CallToolResult
        {
            IsError = true,
            Content = [new TextContentBlock { Text = error.ToJsonString() }]
        };
    }
}
