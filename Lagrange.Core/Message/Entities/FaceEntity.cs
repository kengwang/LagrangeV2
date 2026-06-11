using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Message.Entities;

public class FaceEntity(ushort faceId, bool isLargeFace) : IMessageEntity
{
    public ushort FaceId { get; set; } = faceId;

    public bool IsLargeFace { get; set; } = isLargeFace;

    public FaceEntity() : this(0, false) { }

    Elem[] IMessageEntity.Build()
    {
        if (IsLargeFace)
        {
            var qBigFace = new QBigFaceExtra
            {
                AniStickerPackId = "1",
                AniStickerId = "8",
                FaceId = FaceId,
                Field4 = 1,
                AniStickerType = 1,
                Field6 = string.Empty,
                Preview = string.Empty,
                Field9 = 1
            };

            return
            [
                new Elem
                {
                    CommonElem = new CommonElem
                    {
                        ServiceType = 37,
                        PbElem = ProtoHelper.Serialize(qBigFace),
                        BusinessType = 1
                    }
                }
            ];
        }

        if (FaceId >= 260)
        {
            var qSmallFace = new QSmallFaceExtra
            {
                FaceId = FaceId,
                Text = string.Empty,
                CompatText = string.Empty
            };

            return
            [
                new Elem
                {
                    CommonElem = new CommonElem
                    {
                        ServiceType = 33,
                        PbElem = ProtoHelper.Serialize(qSmallFace),
                        BusinessType = 1
                    }
                }
            ];
        }

        return [new Elem { Face = new Face { Index = FaceId } }];
    }

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.Face is { Index: { } faceId })
        {
            return new FaceEntity((ushort)faceId, false);
        }

        if (target.CommonElem is { ServiceType: 37, PbElem: { IsEmpty: false } } largeFace)
        {
            var qBigFace = ProtoHelper.Deserialize<QBigFaceExtra>(largeFace.PbElem.Span);
            return qBigFace.FaceId is { } largeFaceId ? new FaceEntity((ushort)largeFaceId, true) : null;
        }

        if (target.CommonElem is { ServiceType: 33, PbElem: { IsEmpty: false } } smallFace)
        {
            var qSmallFace = ProtoHelper.Deserialize<QSmallFaceExtra>(smallFace.PbElem.Span);
            return new FaceEntity((ushort)qSmallFace.FaceId, false);
        }

        return null;
    }

    string IMessageEntity.ToPreviewString() => $"[表情:{FaceId}]";
}
