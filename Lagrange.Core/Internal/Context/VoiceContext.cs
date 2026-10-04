using System.Collections.Concurrent;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service.Migration;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
namespace Lagrange.Core.Internal.Context;

internal sealed class VoiceContext(BotContext context,
    Func<PttTransReq, bool, CancellationToken, Task<PttTransResp>>? transcriptionTransport = null,
    TimeSpan? transcriptionTimeout = null,
    Func<OidbAiVoiceReq, CancellationToken, Task<OidbAiVoiceResp>>? synthesisTransport = null,
    TimeSpan? receiptTimeout = null, long? selfUin = null)
{
    private readonly ConcurrentDictionary<ulong, TranscriptionWaiter> _transcriptions = new();
    private readonly object _gate = new();
    private readonly List<VoiceReceipt> _receipts = [];
    private readonly HashSet<(long, ulong)> _claimed = [];
    private readonly Queue<(long, ulong)> _claimOrder = new();

    internal void CompleteTranscription(ulong id, string text, ulong senderUin = 0, string? uuid = null)
    {
        if (!string.IsNullOrEmpty(text) && _transcriptions.TryGetValue(id, out var waiter)
            && (senderUin == 0 || senderUin == (ulong)waiter.Input.SenderUin)
            && (string.IsNullOrEmpty(uuid) || uuid == waiter.Input.FileUuid)) waiter.Source.TrySetResult(text);
    }
    internal void Disconnect()
    {
        foreach (var (key, value) in _transcriptions)
            if (((ICollection<KeyValuePair<ulong, TranscriptionWaiter>>)_transcriptions).Remove(new(key, value))) value.Disconnect();
        VoiceReceipt[] receipts;
        lock (_gate)
        {
            receipts = _receipts.ToArray();
            _receipts.Clear(); _claimed.Clear(); _claimOrder.Clear();
        }
        // Cancellation can synchronously resume SendAiVoice and remove its receipt.
        foreach (var receipt in receipts) receipt.Disconnect();
    }
    internal async Task<string> Transcribe(BotVoiceTranscription input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.MessageId == 0 || input.PeerUin <= 0 || input.SenderUin <= 0) throw new ArgumentException("A complete message identity is required.", nameof(input));
        ArgumentException.ThrowIfNullOrWhiteSpace(input.FileUuid);
        if (input.Md5 is null || input.Md5.Length != 32 || input.Md5.Any(x => !Uri.IsHexDigit(x))) throw new ArgumentException("MD5 must contain 32 hexadecimal characters.", nameof(input));
        ct.ThrowIfCancellationRequested();
        using var waiter = new TranscriptionWaiter(input, ct);
        if (!_transcriptions.TryAdd(input.MessageId, waiter)) throw new InvalidOperationException("Transcription is already pending for this message.");
        try
        {
            waiter.Cancellation.CancelAfter(transcriptionTimeout ?? TimeSpan.FromSeconds(30));
            var token = waiter.Cancellation.Token;
            var request = new PttTransReq { Type = input.IsGroup ? 1u : 2u };
            if (input.IsGroup) request.GroupItem = new() { MsgId = input.MessageId, SenderUin = (ulong)input.SenderUin, GroupUin = (ulong)input.PeerUin, FileId = input.FileId, Md5 = input.Md5.ToLowerInvariant(), Duration = input.Duration, Size = input.Size, Format = input.Format, Uuid = input.FileUuid };
            else request.C2cItem = new() { MsgId = input.MessageId, SenderUin = (ulong)input.SenderUin, ReceiverUin = (ulong)input.PeerUin, Md5 = input.Md5.ToLowerInvariant(), Duration = input.Duration, Size = input.Size, Format = input.Format, Uuid = input.FileUuid };
            var response = await (transcriptionTransport is not null
                ? transcriptionTransport(request, input.IsGroup, token)
                : SendTranscription(request, input.IsGroup, token)).WaitAsync(token);
            var code = input.IsGroup ? response.GroupResult?.ErrCode : response.C2cResult?.ErrCode;
            if (code is > 0) throw new OperationException(unchecked((int)code.Value), "Voice transcription failed.");
            var text = input.IsGroup ? response.GroupResult?.Text : response.C2cResult?.Text;
            return !string.IsNullOrEmpty(text) ? text : await waiter.Source.Task.WaitAsync(token);
        }
        catch (OperationCanceledException) when (waiter.IsDisconnected)
        {
            throw new IOException("Disconnected during voice transcription.");
        }
        finally { ((ICollection<KeyValuePair<ulong, TranscriptionWaiter>>)_transcriptions).Remove(new(input.MessageId, waiter)); }
    }
    private async Task<PttTransResp> SendTranscription(PttTransReq request, bool group, CancellationToken ct) => group
        ? (await context.EventContext.SendEvent<TranscribeVoiceEventResp>(new TranscribeGroupVoiceEventReq(request), ct)).Body
        : (await context.EventContext.SendEvent<TranscribeVoiceEventResp>(new TranscribePrivateVoiceEventReq(request), ct)).Body;

    private sealed class TranscriptionWaiter(BotVoiceTranscription input, CancellationToken ct) : IDisposable
    {
        public BotVoiceTranscription Input { get; } = input;
        public TaskCompletionSource<string> Source { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public CancellationTokenSource Cancellation { get; } = CancellationTokenSource.CreateLinkedTokenSource(ct);
        public bool IsDisconnected => Volatile.Read(ref _disconnected) != 0;
        private int _disconnected;
        public void Disconnect()
        {
            Interlocked.Exchange(ref _disconnected, 1);
            try { Cancellation.Cancel(); }
            catch (ObjectDisposedException) { } // A concurrently completed request already disposed its waiter.
        }
        public void Dispose() => Cancellation.Dispose();
    }
    internal async Task<OidbAiVoiceIndexNode> Synthesize(long groupUin, string voiceId, string text, uint chatType, CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(groupUin);
        ArgumentException.ThrowIfNullOrWhiteSpace(voiceId); ArgumentException.ThrowIfNullOrWhiteSpace(text);
        var request = new OidbAiVoiceReq { GroupUin = checked((uint)groupUin), VoiceId = voiceId, Text = text, ChatType = chatType, Session = new() { SessionId = (uint)Random.Shared.NextInt64(1, uint.MaxValue) } };
        for (int attempt = 0; attempt < 30; attempt++)
        {
            ct.ThrowIfCancellationRequested();
            var response = synthesisTransport is not null
                ? await synthesisTransport(request, ct).WaitAsync(ct)
                : (await context.EventContext.SendEvent<SynthesizeAiVoiceEventResp>(new SynthesizeAiVoiceEventReq(request), ct)).Body;
            // The inner status is render progress, not an OIDB error code. Keep polling the same session.
            if (response.MsgInfo?.MsgInfoBody.FirstOrDefault()?.Index is { FileUuid.Length: > 0 } node) return node;
            await Task.Delay(TimeSpan.FromMilliseconds(300), ct);
        }
        throw new TimeoutException("AI voice synthesis did not return media; no new synthesis request was started.");
    }
    internal async Task<BotMessage> SendAiVoice(long groupUin, string voiceId, string text, uint chatType, CancellationToken ct)
    {
        using var receipt = new VoiceReceipt(groupUin, ct);
        receipt.Cancellation.CancelAfter(receiptTimeout ?? TimeSpan.FromSeconds(45));
        lock (_gate) _receipts.Add(receipt);
        try
        {
            var node = await Synthesize(groupUin, voiceId, text, chatType, receipt.Cancellation.Token);
            lock (_gate)
            {
                receipt.Uuid = node.FileUuid.Trim();
                foreach (var message in receipt.Early) TryClaim(receipt, message);
                receipt.Early.Clear();
            }
            return await receipt.Source.Task.WaitAsync(receipt.Cancellation.Token);
        }
        catch (OperationCanceledException) when (receipt.IsDisconnected)
        {
            throw new IOException("Disconnected while waiting for AI voice receipt.");
        }
        finally { lock (_gate) _receipts.Remove(receipt); }
    }
    internal void OnMessage(BotMessage message)
    {
        if (message.Type != MessageType.Group || message.Contact.Uin != (selfUin ?? context.BotUin) || !message.Entities.OfType<RecordEntity>().Any()) return;
        lock (_gate)
            foreach (var receipt in _receipts)
            {
                if (receipt.Group != ((BotGroupMember)message.Contact).Group.GroupUin || receipt.Source.Task.IsCompleted) continue;
                if (receipt.Uuid is null)
                {
                    if (receipt.Early.Count == 256) receipt.Source.TrySetException(new InvalidOperationException("AI voice receipt buffer exceeded its limit."));
                    else receipt.Early.Add(message);
                }
                else TryClaim(receipt, message);
            }
    }
    private void TryClaim(VoiceReceipt receipt, BotMessage message)
    {
        if (receipt.Source.Task.IsCompleted || message.Sequence == 0 || !message.Entities.OfType<RecordEntity>().Any(x => x.FileUuid.Trim() == receipt.Uuid)) return;
        var key = (receipt.Group, message.Sequence);
        if (!_claimed.Add(key)) return;
        _claimOrder.Enqueue(key);
        while (_claimOrder.Count > 1024) _claimed.Remove(_claimOrder.Dequeue());
        receipt.Source.TrySetResult(message);
    }
    private sealed class VoiceReceipt(long group, CancellationToken ct) : IDisposable
    {
        public CancellationTokenSource Cancellation { get; } = CancellationTokenSource.CreateLinkedTokenSource(ct);
        public bool IsDisconnected { get; private set; }
        public void Disconnect() { IsDisconnected = true; try { Cancellation.Cancel(); } catch (ObjectDisposedException) { } }
        public void Dispose() => Cancellation.Dispose();
        public long Group { get; } = group;
        public string? Uuid { get; set; }
        public List<BotMessage> Early { get; } = [];
        public TaskCompletionSource<BotMessage> Source { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
