using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility;
using Lagrange.Proto;

namespace Lagrange.Core.Message.Entities;

/// <summary>A QQ system-face element.</summary>
public sealed class FaceEntity : IMessageEntity
{
    /// <summary>QQ system face identifier.</summary>
    public uint FaceId { get; init; }
    /// <summary>Textual face label.</summary>
    public string Raw { get; init; } = string.Empty;
    /// <summary>Whether to send an animated face when metadata is available.</summary>
    public bool Large { get; init; } = true;
    /// <summary>Optional animation result identifier.</summary>
    public string? ResultId { get; init; }
    /// <summary>Animation pack identifier preserved from incoming messages.</summary>
    public string? PackId { get; private set; }
    /// <summary>Animation sticker identifier preserved from incoming messages.</summary>
    public string? StickerId { get; private set; }
    /// <summary>Animation sticker category.</summary>
    public int? StickerType { get; private set; }

    /// <summary>Source type preserved from incoming animation metadata.</summary>
    public int SourceType { get; private set; } = 1;
    /// <summary>Randomization type preserved from incoming animation metadata.</summary>
    public int RandomType { get; private set; } = 1;

    async Task IMessageEntity.Preprocess(BotContext context, BotMessage message)
    {
        if (Large && PackId is null)
        {
            var catalog = await Lagrange.Core.Common.Interface.OperationExt.GetSystemFaces(context);
            var face = catalog.SelectMany(x => x.Faces).FirstOrDefault(x => x.Sid == FaceId.ToString(System.Globalization.CultureInfo.InvariantCulture));
            if (face?.AnimationPackId is { } pack && face.AnimationStickerId is { } sticker && face.AnimationType is { } type)
            { PackId = pack.ToString(System.Globalization.CultureInfo.InvariantCulture); StickerId = sticker.ToString(System.Globalization.CultureInfo.InvariantCulture); StickerType = type; }
        }
        if (ResultId is not null && (!Large || StickerType is null)) throw new ArgumentException("Animation result requires catalog metadata.");
    }

    Elem[] IMessageEntity.Build() => Large && StickerType is { } type
        ? [new Elem { CommonElem = new CommonElem { ServiceType = 37, BusinessType = (uint)(type < 4 ? type : 1),
            PbElem = ProtoHelper.Serialize(new AnimatedFaceExtra { PackId = PackId ?? string.Empty, StickerId = StickerId ?? string.Empty,
                FaceId = checked((int)FaceId), SourceType = SourceType, StickerType = type, ResultId = ResultId, Text = Raw, RandomType = RandomType }) } }]
        : FaceId >= 260 || FaceId > int.MaxValue
        ? [new Elem
        {
            CommonElem = new CommonElem
            {
                ServiceType = 33,
                BusinessType = 1,
                PbElem = ProtoHelper.Serialize(new SmallFaceExtra { FaceId = checked((uint)FaceId) })
            }
        }]
        : [new Elem { Face = new Face { Index = (int)FaceId } }];

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.CommonElem is { ServiceType: 37 } animated)
        {
            try
            {
                var data = ProtoHelper.Deserialize<AnimatedFaceExtra>(animated.PbElem.Span);
                if (data.FaceId is not >= 0) return null;
                return new FaceEntity { FaceId = (uint)data.FaceId.Value, PackId = data.PackId, StickerId = data.StickerId,
                    StickerType = data.StickerType, ResultId = data.ResultId, Raw = data.Text, Large = true, SourceType = data.SourceType, RandomType = data.RandomType };
            }
            catch (InvalidDataException)
            {
                // A malformed field or negative int32 (ten-byte varint) is not a valid face ID.
                return null;
            }
        }
        if (target.Face is { } face && face.Index >= 0)
            return new FaceEntity { FaceId = (uint)face.Index, Large = false, Raw = string.IsNullOrEmpty(Raw) ? $"face{face.Index}" : Raw };

        if (target.CommonElem is { ServiceType: 33 } small)
        {
            try { return new FaceEntity { FaceId = ProtoHelper.Deserialize<SmallFaceExtra>(small.PbElem.Span).FaceId, Large = false }; }
            catch { return null; }
        }

        // Keep compatibility with legacy clients which still send CustomFace.
        if (target.CustomFace is { BizType: 0 } legacy)
        {
            return new FaceEntity { FaceId = legacy.FileId, Raw = legacy.Shortcut ?? string.Empty };
        }

        return null;
    }

    /// <summary>Returns the face label.</summary>
    public string ToPreviewString() => $"[{Raw}]";
}

[ProtoPackable]
internal partial class SmallFaceExtra
{
    [ProtoMember(1)] public uint FaceId { get; set; }
    [ProtoMember(2)] public string Preview { get; set; } = string.Empty;
    [ProtoMember(3)] public string Preview2 { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class AnimatedFaceExtra
{
    [ProtoMember(1)] public string PackId { get; set; } = string.Empty;
    [ProtoMember(2)] public string StickerId { get; set; } = string.Empty;
    [ProtoMember(3)] public int? FaceId { get; set; }
    [ProtoMember(4)] public int SourceType { get; set; }
    [ProtoMember(5)] public int StickerType { get; set; }
    [ProtoMember(6)] public string? ResultId { get; set; }
    [ProtoMember(7)] public string Text { get; set; } = string.Empty;
    [ProtoMember(9)] public int RandomType { get; set; }
}
