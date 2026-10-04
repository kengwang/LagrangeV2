using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Context;
using Lagrange.Core.Internal.Packets.Service.Migration;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
namespace Lagrange.Core.Test.Protocol;

public sealed class AiVoiceLifecycleTest
{
    private static OidbAiVoiceResp Response(string uuid) => new() { MsgInfo = new() { MsgInfoBody = [new() { Index = new() { FileUuid = uuid } }] } };
    private static BotMessage Message(string uuid, ulong sequence, long group = 123, long sender = 456)
    {
        var room = new BotGroup(group, "group", 1, 10, 0, null, null, null);
        var member = new BotGroupMember(room, sender, "uid", "self", GroupMemberPermission.Member, 0, null, null, 0, 0, 0);
        // Real push receiver may be a member, not the group itself.
        return new BotMessage([new RecordEntity { FileUuid = uuid }], member, member, 10) { Sequence = sequence };
    }
    [Test]
    public async Task EarlyReceiptMatchesExactGroupSenderAndMedia()
    {
        var response = new TaskCompletionSource<OidbAiVoiceResp>();
        var voice = new VoiceContext(null!, synthesisTransport: (_, _) => response.Task, selfUin: 456);
        var pending = voice.SendAiVoice(123, "voice", "text", 1, default);
        voice.OnMessage(Message("wanted", 1, group: 789));
        voice.OnMessage(Message("wanted", 2, sender: 789));
        voice.OnMessage(Message("other", 3));
        voice.OnMessage(Message("wanted", 0));
        voice.OnMessage(Message("wanted", 4));
        response.SetResult(Response("wanted"));
        Assert.That((await pending.WaitAsync(TimeSpan.FromSeconds(2))).Sequence, Is.EqualTo(4));
    }
    [Test]
    public async Task ConcurrentSameMediaRequiresDistinctRealReceipts()
    {
        var voice = new VoiceContext(null!, synthesisTransport: (_, _) => Task.FromResult(Response("same")), selfUin: 456);
        var first = voice.SendAiVoice(123, "voice", "one", 1, default);
        var second = voice.SendAiVoice(123, "voice", "two", 1, default);
        voice.OnMessage(Message("same", 1));
        voice.OnMessage(Message("same", 1));
        Assert.That(second.IsCompleted, Is.False);
        voice.OnMessage(Message("same", 2));
        Assert.That((await first.WaitAsync(TimeSpan.FromSeconds(2))).Sequence, Is.EqualTo(1));
        Assert.That((await second.WaitAsync(TimeSpan.FromSeconds(2))).Sequence, Is.EqualTo(2));
    }
    [Test]
    public async Task PendingRenderStatusPollsTheSameSession()
    {
        var sessions = new List<uint>();
        var voice = new VoiceContext(null!, synthesisTransport: (request, _) =>
        {
            sessions.Add(request.Session!.SessionId);
            return Task.FromResult(sessions.Count == 1 ? new OidbAiVoiceResp { StatusCode = 1 } : Response("rendered"));
        });
        Assert.That((await voice.Synthesize(123, "voice", "text", 1, default)).FileUuid, Is.EqualTo("rendered"));
        Assert.That(sessions, Has.Count.EqualTo(2));
        Assert.That(sessions.Distinct().Count(), Is.EqualTo(1));
    }

    [TestCase("cancel")]
    [TestCase("timeout")]
    [TestCase("disconnect")]
    public void InterruptedSynthesisNeverResends(string mode)
    {
        int sends = 0;
        var response = new TaskCompletionSource<OidbAiVoiceResp>();
        var voice = new VoiceContext(null!, synthesisTransport: (_, _) => { sends++; return response.Task; },
            receiptTimeout: mode == "timeout" ? TimeSpan.FromMilliseconds(30) : TimeSpan.FromSeconds(10), selfUin: 456);
        using var cancellation = new CancellationTokenSource();
        var pending = voice.SendAiVoice(123, "voice", "text", 1, cancellation.Token);
        if (mode == "cancel") cancellation.Cancel();
        if (mode == "disconnect") voice.Disconnect();
        if (mode == "disconnect") Assert.ThrowsAsync<IOException>(async () => await pending.WaitAsync(TimeSpan.FromSeconds(2)));
        else Assert.That(async () => await pending.WaitAsync(TimeSpan.FromSeconds(2)), Throws.InstanceOf<OperationCanceledException>());
        Assert.That(sends, Is.EqualTo(1));
        voice.OnMessage(Message("late", 123));
        response.SetResult(Response("late"));
    }
}
