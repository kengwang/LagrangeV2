using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Lagrange.Codec.Exceptions;

namespace Lagrange.Codec.Streams;

/// <summary>Buffers input and converts it once when output is first read.</summary>
public abstract class NativeCodecStream : MemoryStream
{
    private bool _initialized;
    private ExceptionDispatchInfo? _failure;
    private readonly Func<nint, int, nint, nint, int> _encodeFunc;

    internal NativeCodecStream(Func<nint, int, nint, nint, int> encodeFunc) => _encodeFunc = encodeFunc;

    private sealed class CallbackState(NativeCodecStream stream, CancellationToken cancellationToken)
    {
        public NativeCodecStream Stream { get; } = stream;
        public CancellationToken CancellationToken { get; } = cancellationToken;
        public ExceptionDispatchInfo? Failure { get; set; }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void Receive(nint userData, nint pointer, int length)
    {
        var state = (CallbackState)GCHandle.FromIntPtr(userData).Target!;
        if (state.Failure != null) return;
        try
        {
            state.CancellationToken.ThrowIfCancellationRequested();
            if (length < 0 || (pointer == 0 && length != 0)) throw new CodecException("Invalid native audio output.");
            state.Stream.Write(new ReadOnlySpan<byte>((void*)pointer, length));
        }
        catch (Exception exception)
        {
            // Managed exceptions must never unwind through the native codec.
            state.Failure = ExceptionDispatchInfo.Capture(exception);
        }
    }

    private unsafe void EnsureInitialized(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(!CanRead, this);
        cancellationToken.ThrowIfCancellationRequested();
        _failure?.Throw();
        if (_initialized) return;

        var source = ToArray();
        var state = new CallbackState(this, cancellationToken);
        var handle = GCHandle.Alloc(state);
        try
        {
            SetLength(0);
            Position = 0;
            fixed (byte* pointer = source)
            {
                int result = _encodeFunc((nint)pointer, source.Length,
                    (nint)(delegate* unmanaged[Cdecl]<nint, nint, int, void>)&Receive, GCHandle.ToIntPtr(handle));
                state.Failure?.Throw();
                cancellationToken.ThrowIfCancellationRequested();
                if (result != 0) throw new CodecException($"Native audio conversion failed. Error code: {result}");
            }
            Position = 0;
            _initialized = true;
        }
        catch (Exception exception)
        {
            _failure = ExceptionDispatchInfo.Capture(exception);
            throw;
        }
        finally
        {
            handle.Free();
        }
    }

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        EnsureInitialized();
        return base.Read(buffer, offset, count);
    }

    /// <inheritdoc />
    public override int Read(Span<byte> buffer)
    {
        EnsureInitialized();
        return base.Read(buffer);
    }

    /// <inheritdoc />
    public override int ReadByte()
    {
        EnsureInitialized();
        return base.ReadByte();
    }

    /// <inheritdoc />
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        EnsureInitialized(cancellationToken);
        return base.ReadAsync(buffer, offset, count, cancellationToken);
    }

    /// <inheritdoc />
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        EnsureInitialized(cancellationToken);
        return base.ReadAsync(buffer, cancellationToken);
    }

    /// <inheritdoc />
    public override void CopyTo(Stream destination, int bufferSize)
    {
        EnsureInitialized();
        base.CopyTo(destination, bufferSize);
    }

    /// <inheritdoc />
    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        EnsureInitialized(cancellationToken);
        return base.CopyToAsync(destination, bufferSize, cancellationToken);
    }
}
