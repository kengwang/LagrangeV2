using FastEndpoints;
using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class GetImplInfoHandler(BotContext lagrange) : EndpointWithoutRequest<MilkyApiResponse<GetImplInfoHandler.Result>>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/get_impl_info");
    }
    private readonly BotContext _lagrange = lagrange;

    public override Task<MilkyApiResponse<Result>> ExecuteAsync(CancellationToken ct)
    {
        return Task.FromResult(new MilkyApiResponse<Result>(new Result
        {
            ImplName = "Lagrange.Core",
            ImplVersion = Constants.GitHash,
            QqProtocolVersion = _lagrange.AppInfo.CurrentVersion,
            QqProtocolType = _lagrange.Config.Protocol switch
            {
                Protocols.Windows => "windows",
                Protocols.MacOs => "macos",
                Protocols.Linux => "linux",
                _ => throw new NotSupportedException(),

            },
            MilkyVersion = Constants.MilkyVersion
        }));
    }

    public sealed class Result
    {
        [JsonPropertyName("impl_name")] public required string ImplName { get; init; }
        [JsonPropertyName("impl_version")] public required string ImplVersion { get; init; }
        [JsonPropertyName("qq_protocol_version")] public required string QqProtocolVersion { get; init; }
        [JsonPropertyName("qq_protocol_type")] public required string QqProtocolType { get; init; }
        [JsonPropertyName("milky_version")] public required string MilkyVersion { get; init; }
    }
}
