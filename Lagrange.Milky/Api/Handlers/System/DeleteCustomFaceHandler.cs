using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("delete_custom_face")]
public sealed class DeleteCustomFaceHandler(BotContext lagrange) : INoResultApiHandler<DeleteCustomFaceHandler.Request>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await _lagrange.DeleteCustomFace(request.FaceId, ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(string faceId)
    {
        [JsonPropertyName("face_id")] public required string FaceId { get; init; } = faceId;
    }
}
