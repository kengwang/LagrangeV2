using Lagrange.Milky.Api.Handlers.Message;
using Lagrange.Milky.Models.Messages;

namespace Lagrange.Milky.Test;

public sealed class HistoryMessagesTests
{
    [Test]
    public void EmptyHistoryReturnsEmptyMessagesAndNoCursor()
    {
        var result = GetHistoryMessagesHandler.CreateResult([]);
        Assert.That(result.Messages, Is.Empty);
        Assert.That(result.NextMessageSeq, Is.Null);
    }

    [TestCase(0, null)]
    [TestCase(1, 0L)]
    [TestCase(10, 9L)]
    public void HistoryIsSortedAndCursorPrecedesEarliestMessage(long first, long? expectedCursor)
    {
        IncomingMessageBase Message(long sequence) => new FriendIncomingMessage
        {
            PeerId = 123, SenderId = 123, Time = 1, MessageSeq = sequence, Segments = [], Friend = null!
        };
        var result = GetHistoryMessagesHandler.CreateResult([Message(first + 2), Message(first)]);
        Assert.That(result.Messages.Select(x => x.MessageSeq), Is.EqualTo(new[] { first, first + 2 }));
        Assert.That(result.NextMessageSeq, Is.EqualTo(expectedCursor));
    }
}
