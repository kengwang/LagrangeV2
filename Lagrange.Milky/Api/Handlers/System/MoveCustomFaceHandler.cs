using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("move_custom_face")]
public sealed class MoveCustomFaceHandler(BotContext lagrange) : INoResultApiHandler<MoveCustomFaceHandler.Request>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
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
