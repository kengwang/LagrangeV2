using FastEndpoints;
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handlers.System;

public sealed class GetSystemFaceHandler(BotContext bot) : Endpoint<GetSystemFaceHandler.Request, MilkyApiResponse<GetSystemFaceHandler.Result>>
{
    public override void Configure() { AuthSchemes("Milky"); Post("/api/get_system_face"); }
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request req, CancellationToken ct)
    { var face = await bot.GetSystemFace(req.FaceId, ct); return new(new Result { Face = face is null ? null : GetSystemFacesHandler.SystemFace.FromCore(face) }); }
    public sealed class Request { [JsonPropertyName("face_id")] public required uint FaceId { get; init; } }
    public sealed class Result { [JsonPropertyName("face")] public GetSystemFacesHandler.SystemFace? Face { get; init; } }
}
