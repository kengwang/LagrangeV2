using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Context;
using Lagrange.Core.Internal.Packets.Service.Migration;

namespace Lagrange.Core.Test.Protocol;

public sealed class VoiceTranscriptionLifecycleTest
{
    private static BotVoiceTranscription Input(ulong id = 1) => new(true, id, 123, 456, $"uuid-{id}", new string('a', 32), 1, 2, 1);
    private static PttTransResp Pending() => new() { GroupResult = new(), C2cResult = new() };

    [Test]
    public async Task ServerFailurePropagatesAndReleasesWaiter()
    {
        int attempts = 0;
        var voice = new VoiceContext(null!, (_, _, _) => Task.FromResult(new PttTransResp
        {
            GroupResult = ++attempts == 1 ? new() { ErrCode = 5 } : new() { Text = "recovered" }
        }));
        Assert.ThrowsAsync<OperationException>(() => voice.Transcribe(Input(), CancellationToken.None));
        Assert.That(await voice.Transcribe(Input(), CancellationToken.None), Is.EqualTo("recovered"));
    }

    [Test]
    public async Task PushBeforeResponseIsRetainedAndMetadataMismatchIgnored()
    {
        var response = new TaskCompletionSource<PttTransResp>();
        var voice = new VoiceContext(null!, (_, _, _) => response.Task);
        var pending = voice.Transcribe(Input(), CancellationToken.None);
        voice.CompleteTranscription(1, "wrong sender", 999, "uuid-1");
        voice.CompleteTranscription(1, "wrong media", 456, "other");
        voice.CompleteTranscription(1, "correct", 456, "uuid-1");
        voice.CompleteTranscription(1, "duplicate", 456, "uuid-1");
        response.SetResult(Pending());
        Assert.That(await pending.WaitAsync(TimeSpan.FromSeconds(2)), Is.EqualTo("correct"));
    }

    [Test]
    public async Task ConcurrentMessagesKeepIndependentResultsAndRejectDuplicateId()
    {
        var voice = new VoiceContext(null!, (_, _, _) => Task.FromResult(Pending()));
        var first = voice.Transcribe(Input(1), CancellationToken.None);
        var second = voice.Transcribe(Input(2), CancellationToken.None);
        Assert.ThrowsAsync<InvalidOperationException>(() => voice.Transcribe(Input(1), CancellationToken.None));
        voice.CompleteTranscription(2, "second");
        voice.CompleteTranscription(1, "first");
        Assert.That(await first.WaitAsync(TimeSpan.FromSeconds(2)), Is.EqualTo("first"));
        Assert.That(await second.WaitAsync(TimeSpan.FromSeconds(2)), Is.EqualTo("second"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CancellationOrTimeoutRemovesWaiterEvenWhileTransportIsPending(bool cancel)
    {
        int attempts = 0;
        var blocked = new TaskCompletionSource<PttTransResp>();
        var voice = new VoiceContext(null!, (_, _, _) => ++attempts == 1
            ? blocked.Task : Task.FromResult(new PttTransResp { GroupResult = new() { Text = "retry" } }),
            cancel ? TimeSpan.FromSeconds(10) : TimeSpan.FromMilliseconds(30));
        using var cancellation = new CancellationTokenSource();
        var pending = voice.Transcribe(Input(), cancellation.Token);
        if (cancel) cancellation.Cancel();
        Assert.That(async () => await pending.WaitAsync(TimeSpan.FromSeconds(2)), Throws.InstanceOf<OperationCanceledException>());
        Assert.That(await voice.Transcribe(Input(), CancellationToken.None), Is.EqualTo("retry"));
        blocked.SetResult(Pending());
    }

    [Test]
    public async Task DisconnectAbortsTransportWaitAndAllowsNewSessionRequest()
    {
        int attempts = 0;
        var blocked = new TaskCompletionSource<PttTransResp>();
        var voice = new VoiceContext(null!, (_, _, _) => ++attempts == 1 ? blocked.Task : Task.FromResult(Pending()));
        var old = voice.Transcribe(Input(), CancellationToken.None);
        voice.Disconnect();
        // Reuse the same ID before the prior call's finally block executes.
        var fresh = voice.Transcribe(Input(), CancellationToken.None);
        Assert.ThrowsAsync<IOException>(async () => await old.WaitAsync(TimeSpan.FromSeconds(2)));
        voice.CompleteTranscription(1, "new session");
        Assert.That(await fresh.WaitAsync(TimeSpan.FromSeconds(2)), Is.EqualTo("new session"));
        blocked.SetResult(Pending());
    }

    [Test]
    public async Task ImmediatePrivateResponseUsesPrivateRequestAndReleasesIdentity()
    {
        var voice = new VoiceContext(null!, (request, group, _) =>
        {
            Assert.That(group, Is.False);
            Assert.That(request.C2cItem!.ReceiverUin, Is.EqualTo(123));
            Assert.That(request.C2cItem.SenderUin, Is.EqualTo(456));
            Assert.That(request.C2cItem.Uuid, Is.EqualTo("uuid-1"));
            return Task.FromResult(new PttTransResp { C2cResult = new() { Text = "immediate" } });
        });
        Assert.That(await voice.Transcribe(Input() with { IsGroup = false }, CancellationToken.None), Is.EqualTo("immediate"));
        Assert.That(await voice.Transcribe(Input() with { IsGroup = false }, CancellationToken.None), Is.EqualTo("immediate"));
    }
}
