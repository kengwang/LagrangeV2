using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Logic;

internal partial class OperationLogic
{
    public async Task SetGroupAvatar(long groupUin, Stream image, CancellationToken cancellationToken = default)
    {
        if (groupUin <= 0) throw new ArgumentOutOfRangeException(nameof(groupUin));
        if (image is null || !image.CanRead) throw new ArgumentException("Avatar stream is not readable.", nameof(image));
        if (!image.CanSeek) { var copy = new MemoryStream(); await image.CopyToAsync(copy, cancellationToken); copy.Position = 0; image = copy; }
        if (image.Length == 0) throw new ArgumentException("Avatar image cannot be empty.", nameof(image));
        var extra = new GroupAvatarExtra { Type = 101, GroupUin = checked((uint)groupUin), Field3 = new GroupAvatarExtraField3 { Field1 = 1 }, Field5 = 3, Field6 = 1 };
        if (!await context.HighwayContext.UploadFile(image, 3000, ProtoHelper.Serialize(extra), cancellationToken)) throw new Exceptions.OperationException(-1, "Group avatar upload failed.");
    }
}
