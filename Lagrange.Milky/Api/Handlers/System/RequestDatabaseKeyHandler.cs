using FastEndpoints;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
namespace Lagrange.Milky.Api.Handlers.System;
/// <summary>QQ extension endpoint: request_database_key.</summary>
public sealed class RequestDatabaseKeyHandler(BotContext lagrange) : Endpoint<RequestDatabaseKeyHandler.Request, MilkyApiResponse<RequestDatabaseKeyHandler.Result>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/request_database_key"); }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.RequestDatabaseKey(request.DbSalt ?? string.Empty, ct);
        return new(new Result { Key = result });
    }
    public sealed class Request
    {
        [JsonPropertyName("db_salt")] public string? DbSalt { get; init; }
    }
    public sealed class Result
    {
        [JsonPropertyName("key")] public string Key { get; init; } = string.Empty;
    }
}
