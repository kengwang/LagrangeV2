using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.Message;

[ApiHandler("mark_all_read")]
public sealed class MarkAllMessagesAsReadHandler(BotContext lagrange) : INoResultApiHandler<MarkAllMessagesAsReadHandler.Request>
{
    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await lagrange.MarkAllMessagesAsRead(request.GroupIds ?? [], request.PrivateUserIds ?? [], ct).WaitAsync(ct);
        return new MilkyApiResponse();
    }

    public sealed class Request
    {
        [JsonPropertyName("group_ids")] public IReadOnlyList<long>? GroupIds { get; init; }
        [JsonPropertyName("private_user_ids")] public IReadOnlyList<long>? PrivateUserIds { get; init; }
    }
}
