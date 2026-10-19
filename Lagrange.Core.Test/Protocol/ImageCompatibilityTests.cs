using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Test.Protocol;

public sealed class ImageCompatibilityTests
{
    [TestCase(0u)]
    [TestCase(123u)]
    [TestCase(uint.MaxValue)]
    public void GroupImageCompatibilityElementDoesNotInventFace(uint fileId)
    {
        // Group images contain a legacy CustomFace followed by the modern image.
        var info = new MsgInfo
        {
            MsgInfoBody = [new() { Index = new() { Info = new() { Width = 1, Height = 1 }, FileUuid = "image" } }],
            ExtBizInfo = new() { Pic = new() { TextSummary = "[图片]" } }
        };
        List<Elem> elements =
        [
            new() { CustomFace = new() { BizType = 0, FileId = fileId } },
            new() { CommonElem = new() { ServiceType = 48, BusinessType = 20, PbElem = ProtoHelper.Serialize(info) } }
        ];
        var factories = MessageEntityRegistry.Create();
        var entities = elements.Select(elem => factories.Select(factory => factory.Parse(elements, elem))
            .FirstOrDefault(entity => entity is not null)).Where(entity => entity is not null).ToList();

        Assert.That(entities.OfType<FaceEntity>(), Is.Empty);
        Assert.That(entities.Single(), Is.TypeOf<ImageEntity>());
    }
}
