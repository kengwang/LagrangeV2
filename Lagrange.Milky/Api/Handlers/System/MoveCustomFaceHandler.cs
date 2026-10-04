using FastEndpoints;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Internal.Events.System;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class MoveCustomFaceHandler(BotContext lagrange) : Endpoint<MoveCustomFaceHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/move_custom_face");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await _lagrange.MoveCustomFace(request.FaceId, checked((uint)request.Position), request.Entries.Select(entry => new CustomFaceLookup(entry.FaceId, entry.Md5)).ToArray(), ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(string faceId, int position, IReadOnlyList<MoveFaceEntry> entries)
    {
        [JsonPropertyName("face_id")] public required string FaceId { get; init; } = faceId;
        [JsonPropertyName("position")] public required int Position { get; init; } = position;
        [JsonPropertyName("entries")] public required IReadOnlyList<MoveFaceEntry> Entries { get; init; } = entries;
    }

    public sealed class MoveFaceEntry
    {
        [JsonPropertyName("face_id")] public required string FaceId { get; init; }
        [JsonPropertyName("md5")] public required string Md5 { get; init; }
    }
}
