using FastEndpoints;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.Message;

public sealed class MarkAllMessagesAsReadHandler(BotContext lagrange) : Endpoint<MarkAllMessagesAsReadHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/mark_all_read");
    }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
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
