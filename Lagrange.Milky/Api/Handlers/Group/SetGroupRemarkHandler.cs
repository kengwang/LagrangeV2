using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Group;

[ApiHandler("set_group_remark")]
public sealed class SetGroupRemarkHandler(BotContext lagrange) : INoResultApiHandler<SetGroupRemarkHandler.Request>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
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
