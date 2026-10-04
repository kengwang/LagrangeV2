using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class SetGroupRemarkHandler(BotContext lagrange) : Endpoint<SetGroupRemarkHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_group_remark");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await _lagrange.RemarkGroup(request.GroupId, request.Remark).WaitAsync(ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(long groupId, string remark)
    {
        [JsonPropertyName("group_id")] public required long GroupId { get; init; } = groupId;
        [JsonPropertyName("remark")] public required string Remark { get; init; } = remark;
    }
}
