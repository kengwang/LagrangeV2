using System.Diagnostics;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Message.Entities;

public abstract class RichMediaEntityBase : IMessageEntity, IAsyncDisposable
{
    private MsgInfo? _msgInfo;
    private bool _uploadStarted, _uploadComplete;
    private (Type Kind, long Uin, long? TempGroup)? _uploadTarget;
    private readonly HashSet<Stream> _ownedStreams = new(ReferenceEqualityComparer.Instance);

    protected Stream Own(Stream stream)
    {
        _ownedStreams.Add(stream);
        return stream;
    }

    /// <summary>Releases only streams owned by this media entity.</summary>
    public ValueTask DisposeAsync() => new(ReleaseOwnedResourcesAsync());

    internal async Task ReleaseOwnedResourcesAsync()
    {
        var streams = _ownedStreams.ToArray();
        _ownedStreams.Clear();
        List<Exception>? failures = null;
        foreach (var stream in streams)
        {
            try { await stream.DisposeAsync(); }
            catch (Exception e) { (failures ??= []).Add(e); }
        }
        if (failures is not null) throw new AggregateException("Media cleanup failed.", failures);
    }

    protected bool BeginPreparation(BotMessage message)
    {
        if (Stream is null)
        {
            if (MsgInfo is null) throw new InvalidOperationException("Media has neither an upload stream nor a protocol reference.");
            return false;
        }
        var target = (message.Receiver.GetType(), message.Receiver.Uin, message.TempGroupUin);
        if (_uploadComplete && _uploadTarget == target) return false;
        if (_uploadStarted) throw new InvalidOperationException("This media upload was already attempted; create a fresh media entity to retry or change recipients.");
        _uploadStarted = true;
        _uploadTarget = target;
        return true;
    }

    protected void CompletePreparation() => _uploadComplete = true;

    private void EnsureReady()
    {
        if (_uploadStarted && !_uploadComplete) throw new InvalidOperationException("Media upload did not complete.");
    }


    protected byte[]? _compat;

    private protected bool IsGroup;

    internal MsgInfo? MsgInfo
    {
        get => _msgInfo;
        set
        {
            Debug.Assert(value != null);

            var fileInfo = value.MsgInfoBody[0].Index.Info;
            FileUuid = value.MsgInfoBody[0].Index.FileUuid;
            FileMd5 = fileInfo.FileHash;
            FileSha1 = fileInfo.FileSha1;
            FileSize = fileInfo.FileSize;
            FileName = fileInfo.FileName;
            _msgInfo = value;
        }
    }

    private protected bool DisposeOnCompletion;

    internal abstract Lazy<Stream>? Stream { get; }

    public string FileUuid { get; internal set; } = string.Empty;

    public string FileUrl { get; internal set; } = string.Empty;

    public string FileName { get; internal set; } = string.Empty;

    public string FileSha1 { get; internal set; } = string.Empty;

    public uint FileSize { get; internal set; }

    public string FileMd5 { get; internal set; } = string.Empty;

    public abstract Task Preprocess(BotContext context, BotMessage message);

    public abstract Task Postprocess(BotContext context, BotMessage message);

    internal abstract Elem[] Build();

    internal abstract IMessageEntity? Parse(List<Elem> elements, Elem target);

    public abstract string ToPreviewString();

    Elem[] IMessageEntity.Build() { EnsureReady(); return Build(); }

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target) => Parse(elements, target);

    string IMessageEntity.ToPreviewString() => ToPreviewString();
}