using FastEndpoints;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
namespace Lagrange.Milky.Api.Handlers.System;
public sealed class SetQzoneMessageVisibilityHandler(BotContext lagrange) : Endpoint<SetQzoneMessageVisibilityHandler.Request, MilkyApiResponse>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/set_qzone_message_visibility"); }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    { await lagrange.SetQzoneMessageVisibility(request.MessageId ?? string.Empty, request.Right, request.Users, ct); return new(); }
    public sealed class Request
    {
        private IReadOnlyList<long>? _users;
        [JsonPropertyName("message_id")] public string? MessageId { get; init; }
        [JsonPropertyName("right")] public uint Right { get; init; }
        [JsonPropertyName("users")] public IReadOnlyList<long> Users { get => _users ?? []; init => _users = value; }
    }
}
