using Lagrange.Core.Common.Response;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service.Migration;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Utility;
namespace Lagrange.Core.Common.Interface;
/// <summary>QQ voice recognition and synthesis operations.</summary>
public static class VoiceExt
{
    /// <summary>Recognizes a received voice; waits at most thirty seconds for asynchronous transcription.</summary>
    /// <param name="context">The authenticated bot session.</param>
    /// <param name="input">The original message and media identities required by QQ.</param>
    /// <param name="ct">Cancels both transport and asynchronous result waiting.</param>
    /// <returns>The server's recognized text.</returns>
    public static Task<string> TranscribeVoice(this BotContext context, BotVoiceTranscription input, CancellationToken ct = default) => context.VoiceContext.Transcribe(input, ct);
    /// <summary>Recognizes the voice in a received message using its original protocol identity.</summary>
    /// <param name="context">The authenticated bot session.</param>
    /// <param name="message">A received message containing exactly one recording and its media metadata.</param>
    /// <param name="ct">Cancels both transport and asynchronous result waiting.</param>
    /// <returns>The server's recognized text.</returns>
    public static Task<string> TranscribeVoice(this BotContext context, BotMessage message, CancellationToken ct = default)
        => context.TranscribeVoice(CreateTranscriptionInput(message), ct);

    internal static BotVoiceTranscription CreateTranscriptionInput(BotMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        var record = message.Entities.OfType<RecordEntity>().SingleOrDefault()
            ?? throw new ArgumentException("The message must contain one voice recording.", nameof(message));
        var index = record.MsgInfo?.MsgInfoBody.FirstOrDefault()?.Index
            ?? throw new ArgumentException("The voice message has no media metadata.", nameof(message));
        var peer = message.Contact is BotGroupMember member ? member.Group.GroupUin : message.Receiver.Uin;
        // The transcription correlation key is protobuf field 4 (Random), not field 12 (MsgUid).
        // pttTrans echoes this request correlation key; history may omit it, so allocate one locally.
        ulong correlation = message.Random & 0x7fffffff;
        if (correlation == 0) correlation = (ulong)Random.Shared.Next(1, int.MaxValue);
        return new BotVoiceTranscription(message.Type == MessageType.Group, correlation,
            peer, message.Contact.Uin, record.FileUuid, record.FileMd5, record.RecordLength,
            record.FileSize, index.Info.Type.VoiceFormat);
    }
    /// <summary>Returns voice categories available in a group.</summary>
    /// <param name="context">The authenticated bot session.</param>
    /// <param name="groupUin">The group for which voices are requested.</param>
    /// <param name="chatType">The QQ protocol chat type; defaults to 1.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <returns>The voice catalog grouped by category.</returns>
    public static async Task<IReadOnlyList<BotAiVoiceCategory>> GetAiVoiceList(this BotContext context, long groupUin, uint chatType = 1, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(groupUin);
        var response = (await context.EventContext.SendEvent<GetAiVoiceListEventResp>(new GetAiVoiceListEventReq(new OidbAiVoiceListReq { GroupUin = checked((uint)groupUin), ChatType = chatType }), ct)).Body;
        return response.Content.Select(x => new BotAiVoiceCategory(x.Category, x.Voices.Select(v => new BotAiVoice(v.VoiceId, v.VoiceDisplayName, v.VoiceExampleUrl)).ToArray())).ToArray();
    }
    /// <summary>Starts QQ synthesis and returns its exact media identity. QQ may publish the synthesized voice.</summary>
    /// <param name="context">The authenticated bot session.</param>
    /// <param name="groupUin">The target group.</param>
    /// <param name="voiceId">A voice identifier returned by the voice catalog.</param>
    /// <param name="text">The text to synthesize.</param>
    /// <param name="chatType">The QQ protocol chat type; defaults to 1.</param>
    /// <param name="ct">Cancels synthesis polling and download-URL resolution.</param>
    /// <returns>The resulting media identity, format, duration, size, and download URL.</returns>
    public static async Task<BotAiVoiceMedia> SynthesizeAiVoice(this BotContext context, long groupUin, string voiceId, string text, uint chatType = 1, CancellationToken ct = default)
    {
        var node = await context.VoiceContext.Synthesize(groupUin, voiceId, text, chatType, ct);
        if (node.Info is null) throw new InvalidDataException("AI voice response is missing media metadata.");
        var member = await context.CacheContext.ResolveMember(groupUin, context.BotUin).WaitAsync(ct)
            ?? throw new InvalidOperationException("The bot is not a member of the requested group.");
        var record = new RecordEntity { MsgInfo = new MsgInfo
        {
            MsgInfoBody = [new MsgInfoBody { Index = ProtoHelper.Deserialize<IndexNode>(ProtoHelper.Serialize(node).Span) }]
        } };
        var message = new BotMessage(member.Item2, member.Item1, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        var download = await context.EventContext.SendEvent<RecordGroupDownloadEventResp>(new RecordGroupDownloadEventReq(message, record), ct);
        return new(node.FileUuid, node.Info.FileName, node.Info.FileSize, node.Info.Time, node.Info.Type?.VoiceFormat ?? 0, download.Url);
    }
    /// <summary>Publishes an AI voice and waits for its matching server message receipt without retrying publication.</summary>
    /// <param name="context">The authenticated bot session.</param>
    /// <param name="groupUin">The target group.</param>
    /// <param name="voiceId">A voice identifier returned by the voice catalog.</param>
    /// <param name="text">The text to synthesize and publish.</param>
    /// <param name="chatType">The QQ protocol chat type; defaults to 1.</param>
    /// <param name="ct">Cancels synthesis and receipt waiting; already published messages are not recalled.</param>
    /// <returns>The matching server message with its real sequence and media identity.</returns>
    public static Task<BotMessage> SendAiVoice(this BotContext context, long groupUin, string voiceId, string text, uint chatType = 1, CancellationToken ct = default) => context.VoiceContext.SendAiVoice(groupUin, voiceId, text, chatType, ct);
}
