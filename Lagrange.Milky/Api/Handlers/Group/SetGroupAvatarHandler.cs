using FastEndpoints;
using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Converters;

namespace Lagrange.Milky.Api.Handlers.Group;

public sealed class SetGroupAvatarHandler(BotContext lagrange, ResourceConverter resources) : Endpoint<SetGroupAvatarHandler.Request, MilkyApiResponse>
{

    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/set_group_avatar");
    }
    public override async Task<MilkyApiResponse> ExecuteAsync(Request request, CancellationToken ct)
    {
        if (request.GroupId <= 0) throw new ArgumentOutOfRangeException(nameof(request.GroupId));
        using var image = await resources.UriToStreamAsync(request.ImageUri, ct);
        await lagrange.SetGroupAvatar(request.GroupId, image, ct).WaitAsync(ct);
        return new MilkyApiResponse();
    }
    public sealed class Request(long groupId, string imageUri)
    {
        [JsonPropertyName("group_id")] public long GroupId { get; init; } = groupId;
        [JsonPropertyName("image_uri")] public string ImageUri { get; init; } = imageUri;
    }
}
