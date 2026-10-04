using System.Reflection;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Milky.Caching;
using Lagrange.Milky.Converters;
using Lagrange.Milky.Models.Segments;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lagrange.Milky.Test;

public sealed class MissingReplyTests
{
    // Bind the private lookup seam only in tests; the product still uses normal typed APIs.
    private static readonly Func<Func<Task<List<BotMessage>>>, CancellationToken, Task<BotMessage?>> Resolve =
        typeof(MilkyConverter).GetMethod("ResolveReplyTargetAsync", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Func<Func<Task<List<BotMessage>>>, CancellationToken, Task<BotMessage?>>>();

    [Test]
    public async Task ExplicitMissingStorageReplyUsesMetadataFallback()
    {
        var message = await Resolve(() => Task.FromException<List<BotMessage>>(new OperationException(100000301, "msgstorage rsp msgs size is empty")), default);
        Assert.That(message, Is.Null);
        Assert.That(await Resolve(() => Task.FromResult(new List<BotMessage>()), default), Is.Null);
    }

    [TestCase(1)]
    [TestCase(100000302)]
    public void OtherProtocolFailuresRemainVisible(int code)
    {
        var error = new OperationException(code, "another error");
        Assert.That(Assert.ThrowsAsync<OperationException>(async () => await Resolve(() => Task.FromException<List<BotMessage>>(error), default)), Is.SameAs(error));
    }

    [Test]
    public void CancellationIsNotTreatedAsMissingReply()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.ThrowsAsync<OperationCanceledException>(async () => await Resolve(() => throw new AssertionException("lookup must not start"), cancellation.Token));
        Assert.ThrowsAsync<TaskCanceledException>(async () => await Resolve(() => Task.FromCanceled<List<BotMessage>>(cancellation.Token), default));
    }

    [Test]
    public async Task UnavailableReferencePreservesMetadataAndOtherSegments()
    {
        var reply = new ReplyEntity();
        typeof(ReplyEntity).GetProperty(nameof(ReplyEntity.SrcSequence))!.SetValue(reply, 77UL);
        typeof(ReplyEntity).GetProperty(nameof(ReplyEntity.SourceUin))!.SetValue(reply, 123L);
        typeof(ReplyEntity).GetProperty(nameof(ReplyEntity.SourceTime))!.SetValue(reply, 456L);
        var converter = new MilkyConverter(null!, new MessageCache(null!, NullLoggerFactory.Instance), null!);
        var result = await converter.ToIncomingSegmentsAsync([reply, new TextEntity("still present")], MessageType.Temp, 123);
        var data = ((ReplyIncomingSegment)result[0]).Data;
        Assert.Multiple(() =>
        {
            Assert.That(data.MessageSeq, Is.EqualTo(77));
            Assert.That(data.SenderId, Is.EqualTo(123));
            Assert.That(data.Time, Is.EqualTo(456));
            Assert.That(data.Segments, Is.Empty);
            Assert.That(result[1], Is.TypeOf<TextIncomingSegment>());
        });
    }
}
