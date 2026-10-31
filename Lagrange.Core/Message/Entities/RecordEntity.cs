using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Extension;

namespace Lagrange.Core.Message.Entities;

public class RecordEntity : RichMediaEntityBase
{
    internal override Lazy<Stream>? Stream { get; }
    
    public uint RecordLength { get; set; }

    public byte[]? Waveform { get; init; }
    internal ReadOnlyMemory<byte> VoiceCompatibility => _compat ?? [];

    internal byte[] EncodeWaveform()
    {
        if (Waveform is null) return [];
        if (Waveform.Length != 30) throw new ArgumentException("Voice waveforms require 30 amplitude values.", nameof(Waveform));
        return ProtoHelper.Serialize(new RecordWaveform { Size = (uint)Waveform.Length, Amplitudes = Waveform }).ToArray();
    }
    
    public RecordEntity() { }
    
    public RecordEntity(Stream stream, bool disposeOnCompletion = false)
    {
        if (disposeOnCompletion) Own(stream);
        Stream = new Lazy<Stream>(() => stream, disposeOnCompletion);
        DisposeOnCompletion = disposeOnCompletion;
    }
    
    public override async Task Preprocess(BotContext context, BotMessage message)
    {
        if (!BeginPreparation(message)) return;
        ArgumentNullException.ThrowIfNull(Stream);

        Exception? failure = null;
        try
        {
            IsGroup = message.IsGroup();
            NTV2RichMediaUploadEventResp result = IsGroup
                ? await context.EventContext.SendEvent<RecordGroupUploadEventResp>(new RecordGroupUploadEventReq(message, this))
                : await context.EventContext.SendEvent<RecordUploadEventResp>(new RecordUploadEventReq(message, this));

            _compat = result.Compat;
            if (message.Type == MessageType.Private && message.Entities.Count == 1 && !VoiceCompatibility.IsEmpty && ProtoHelper.Deserialize<RawPttCompatibility>(VoiceCompatibility.Span).Ptt.Length == 0)
                throw new InvalidDataException("Private voice upload response lacks playback information.");
            if (Waveform is not null)
            {
                result.Info.ExtBizInfo ??= new ExtBizInfo();
                result.Info.ExtBizInfo.Ptt ??= new PttExtBizInfo();
                result.Info.ExtBizInfo.Ptt.Waveform = EncodeWaveform();
            }
            MsgInfo = result.Info;

            if (result.Ext != null)
            {
                await context.HighwayContext.UploadFile(Stream.Value, message.IsGroup() ? 1008 : 1007, ProtoHelper.Serialize(result.Ext));
            }
            CompletePreparation();
        }  
        catch (Exception exception) { failure = exception; throw; }
        finally
        {
            try { await ReleaseOwnedResourcesAsync(); }
            catch (Exception exception) when (failure is not null)
            { context.LogWarning(nameof(RecordEntity), "Media cleanup also failed: {0}", null, exception.Message); }
        }
    }

    public override async Task Postprocess(BotContext context, BotMessage message)
    {
        if (string.IsNullOrEmpty(FileUrl))
        {
            NTV2RichMediaDownloadEventResp result = message.IsGroup()
                ? await context.EventContext.SendEvent<RecordGroupDownloadEventResp>(new RecordGroupDownloadEventReq(message, this))
                : await context.EventContext.SendEvent<RecordDownloadEventResp>(new RecordDownloadEventReq(message, this));

            FileUrl = result.Url;
        }
    }

    public override string ToPreviewString() => $"[语音] {RecordLength}\"";

    internal override Elem[] Build() =>
    [
        new()
        {
            CommonElem = new CommonElem
            {
                ServiceType = 48,
                PbElem = ProtoHelper.Serialize(MsgInfo ?? throw new ArgumentNullException(nameof(MsgInfo))),
                BusinessType = IsGroup ? 22u : 12u,
            }
        }
    ];

    internal override IMessageEntity? Parse(List<Elem> elements, Elem target)
    {
        if (target.CommonElem is { BusinessType: 12 or 22 } commonElem)
        {
            var msgInfo = ProtoHelper.Deserialize<MsgInfo>(commonElem.PbElem.Span);
            var info = msgInfo.MsgInfoBody[0].Index.Info;
            
            return new RecordEntity
            {
                MsgInfo = msgInfo,
                IsGroup = commonElem.BusinessType == 22,
                RecordLength = info.Time
            };
        }
        
        return null;
    }
}

[Lagrange.Proto.ProtoPackable]
internal partial class RecordWaveform
{
    [Lagrange.Proto.ProtoMember(1)] public uint Size { get; set; }
    [Lagrange.Proto.ProtoMember(2)] public byte[] Amplitudes { get; set; } = [];
}
