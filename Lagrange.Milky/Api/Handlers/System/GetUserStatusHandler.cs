using FastEndpoints;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
namespace Lagrange.Milky.Api.Handlers.System;
/// <summary>QQ extension endpoint: get_user_status.</summary>
public sealed class GetUserStatusHandler(BotContext lagrange) : Endpoint<GetUserStatusHandler.Request, MilkyApiResponse<GetUserStatusHandler.Result>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_user_status"); }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        var result = await lagrange.GetUserStatus(request.UserId, ct);
        return new(new Result { Status = result?.Status, ExtendedStatus = result?.ExtendedStatus });
    }
    public sealed class Request
    {
        [JsonPropertyName("user_id")] public long UserId { get; init; }
    }
    public sealed class Result
    {
        [JsonPropertyName("status")] public uint? Status { get; init; }
        [JsonPropertyName("extended_status")] public uint? ExtendedStatus { get; init; }
    }
}
