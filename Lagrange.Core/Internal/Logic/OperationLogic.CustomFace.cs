using System.Security.Cryptography;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;

namespace Lagrange.Core.Internal.Logic;

internal partial class OperationLogic
{
    public async Task<string> AddCustomFace(Stream image, CancellationToken cancellationToken = default)
    {
        if (image is null || !image.CanRead || !image.CanSeek) throw new ArgumentException("Custom face image must be a seekable readable stream.", nameof(image));
        image.Position = 0;
        using var copy = new MemoryStream();
        await image.CopyToAsync(copy, cancellationToken);
        var bytes = copy.ToArray();
        if (bytes.Length == 0) throw new ArgumentException("Custom face image cannot be empty.", nameof(image));
        var md5 = MD5.HashData(bytes);
        var emojiId = $"{context.BotUin}_0_0_0_{Convert.ToHexString(md5)}_0_0";
        var token = await context.EventContext.SendEvent<AddCustomFaceEventResp>(new AddCustomFaceEventReq(md5, checked((uint)bytes.Length)), cancellationToken);
        await using var upload = new MemoryStream(bytes, writable: false);
        if (!await context.HighwayContext.UploadCustomFace(upload, emojiId, token.Token, cancellationToken)) throw new OperationException(-1, "Custom face upload failed.");
        return emojiId;
    }
}
