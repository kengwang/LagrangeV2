using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Group;

[ApiHandler("set_group_new_member_history_visibility")]
public sealed class SetGroupNewMemberHistoryHandler(BotContext lagrange) : INoResultApiHandler<SetGroupNewMemberHistoryHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await lagrange.SetGroupNewMemberHistoryVisibility(request.GroupId, request.Visible, ct).WaitAsync(ct);
        return new MilkyApiResponse();
    }
    public sealed class Request
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; }
        [JsonPropertyName("visible")] public bool Visible { get; init; }
    }
}
