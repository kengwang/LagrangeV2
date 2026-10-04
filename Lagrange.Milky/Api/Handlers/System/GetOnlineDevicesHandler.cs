using FastEndpoints;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
namespace Lagrange.Milky.Api.Handlers.System;
public sealed class GetOnlineDevicesHandler(BotContext lagrange) : EndpointWithoutRequest<MilkyApiResponse<GetOnlineDevicesHandler.Result>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_online_devices"); }
    public override Task<MilkyApiResponse<Result>> ExecuteAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var devices = lagrange.GetOnlineDevices();
        return Task.FromResult(new MilkyApiResponse<Result>(new Result { Observed = devices is not null,
            Devices = devices?.Select(x => new Device(x.AppId, x.InstanceId, x.ClientType, x.Platform, x.DeviceName)).ToArray() }));
    }
    public sealed class Result
    {
        [JsonPropertyName("observed")] public bool Observed { get; init; }
        [JsonPropertyName("devices")] public IReadOnlyList<Device>? Devices { get; init; }
    }
    public sealed record Device([property: JsonPropertyName("app_id")] uint AppId, [property: JsonPropertyName("instance_id")] uint InstanceId,
        [property: JsonPropertyName("client_type")] uint ClientType, [property: JsonPropertyName("platform")] uint Platform, [property: JsonPropertyName("device_name")] string DeviceName);
}
