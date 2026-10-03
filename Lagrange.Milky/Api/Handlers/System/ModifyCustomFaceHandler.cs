using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Attributes;

namespace Lagrange.Milky.Api.Handlers.System;

[ApiHandler("modify_custom_face")]
public sealed class ModifyCustomFaceHandler(BotContext lagrange) : INoResultApiHandler<ModifyCustomFaceHandler.Request>
{
    private readonly BotContext _lagrange = lagrange;

    public async ValueTask<MilkyApiResponse> HandleAsync(Request request, CancellationToken ct)
    {
        await _lagrange.ModifyCustomFace(request.FaceId, request.Md5, request.Description, ct);
        return new MilkyApiResponse();
    }

    public sealed class Request(string faceId, string md5, string description)
    {
        [JsonPropertyName("face_id")] public required string FaceId { get; init; } = faceId;
        [JsonPropertyName("md5")] public required string Md5 { get; init; } = md5;
        [JsonPropertyName("description")] public required string Description { get; init; } = description;
    }
}
