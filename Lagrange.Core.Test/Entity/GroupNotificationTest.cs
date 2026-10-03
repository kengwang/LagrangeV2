using Lagrange.Core.Common.Entity;

namespace Lagrange.Core.Test.Entity;

public sealed class GroupNotificationTest
{
    [Test]
    public void UnknownNotificationPreservesWireTypeAndPayload()
    {
        var notification = new BotGroupUnknownNotification(123, 456, 99, 789, "uid", "pending");

        Assert.That(notification.Type, Is.EqualTo(BotGroupNotificationType.Unknown));
        Assert.That(notification.RawType, Is.EqualTo(99));
        Assert.That(notification.GroupUin, Is.EqualTo(123));
        Assert.That(notification.Sequence, Is.EqualTo(456));
        Assert.That(notification.Comment, Is.EqualTo("pending"));
    }
}
