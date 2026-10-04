using FastEndpoints;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class DeleteCustomFaceHandler(BotContext lagrange) : Endpoint<DeleteCustomFaceHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/delete_custom_face");
    }
    private readonly BotContext _lagrange = lagrange;

    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        await _lagrange.DeleteCustomFace(request.FaceId, ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(string faceId)
    {
        [JsonPropertyName("face_id")] public required string FaceId { get; init; } = faceId;
    }
}
