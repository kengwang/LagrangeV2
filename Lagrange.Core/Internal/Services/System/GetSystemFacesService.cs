using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<GetSystemFacesEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x9154_1")]
internal sealed class GetSystemFacesService : OidbService<GetSystemFacesEventReq, GetSystemFacesEventResp, D9154Req, D9154Resp>
{
    protected override uint Command => 0x9154;
    protected override uint Service => 1;
    protected override Task<D9154Req> ProcessRequest(GetSystemFacesEventReq request, BotContext context) => Task.FromResult(new D9154Req { Field1 = 0, Field2 = 7, Field3 = 0 });

    protected override Task<GetSystemFacesEventResp> ProcessResponse(D9154Resp response, BotContext context)
    {
        var packs = new List<BotSystemFacePack>();
        Add(packs, response.Common);
        Add(packs, response.SpecialBig);
        if (response.Magic?.List?.Emojis is { Count: > 0 } magic) packs.Add(new BotSystemFacePack { PackName = "MagicFace", Faces = Convert(magic) });
        return Task.FromResult(new GetSystemFacesEventResp(packs));
    }

    private static void Add(List<BotSystemFacePack> packs, D9154Content? content)
    {
        foreach (var list in content?.Lists ?? []) packs.Add(new BotSystemFacePack { PackName = list.PackName ?? string.Empty, Faces = Convert(list.Emojis ?? []) });
    }

    private static IReadOnlyList<BotSystemFace> Convert(IEnumerable<D9154Emoji> emojis) => [.. emojis.Where(face => !string.IsNullOrWhiteSpace(face.Sid)).Select(face => new BotSystemFace
    {
        Sid = face.Sid!, Description = face.Description ?? string.Empty, EmCode = face.EmCode ?? string.Empty,
        CategoryId = face.CategoryId == 0 ? null : face.CategoryId, Url = face.Url?.BaseUrl, Aliases = face.Aliases ?? [],
    })];
}
