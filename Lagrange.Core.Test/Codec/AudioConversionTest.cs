using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Lagrange.Codec;
using Lagrange.Codec.Streams;

namespace Lagrange.Core.Test.Codec;

[TestFixture]
public class AudioConversionTest
{
    [Test]
    public void DetectionAcceptsShortAndNonCanonicalWaveHeaders()
    {
        Assert.That(AudioHelper.DetectAudio([]), Is.EqualTo(AudioFormat.Unknown));
        Assert.That(AudioHelper.DetectAudio("RIFF\0\0\0\0WAVEJUNK"u8.ToArray()), Is.EqualTo(AudioFormat.Wav));
        Assert.That(AudioHelper.DetectAudio("not audio"u8.ToArray()), Is.EqualTo(AudioFormat.Unknown));
    }

    [Test]
    public async Task StandardSilkNormalizesHeaderAndOnlyRemovesActualTerminator()
    {
        byte[] packet = [2, 0, 0xff, 0xff];
        byte[] expected = [0x02, .. "#!SILK_V3"u8, .. packet];
        byte[] source = [.. "#!SILK_V3"u8, .. packet, 0xff, 0xff];
        Assert.That(await AudioCodec.ConvertAsync(source, AudioOutputFormat.Silk), Is.EqualTo(expected));
        Assert.That(await AudioCodec.ConvertAsync(expected, AudioOutputFormat.Silk), Is.EqualTo(expected));
    }

    [Test]
    public void InvalidInputsAndUnsupportedFormatsFailBeforeNativeLoad()
    {
        Assert.Throws<ArgumentNullException>(() => AudioCodec.ConvertAsync(null!, AudioOutputFormat.Wav));
        Assert.Throws<ArgumentException>(() => AudioCodec.ConvertAsync([], AudioOutputFormat.Wav));
        Assert.Throws<NotSupportedException>(() => AudioCodec.ConvertAsync([1], (AudioOutputFormat)99));
        Assert.ThrowsAsync<InvalidDataException>(async () => await AudioCodec.ConvertAsync([.. "#!SILK_V3"u8, 5, 0, 1], AudioOutputFormat.Silk));
        Assert.ThrowsAsync<InvalidDataException>(async () => await AudioCodec.ConvertAsync([.. "#!SILK_V3"u8, 1], AudioOutputFormat.Silk));
        Assert.ThrowsAsync<TaskCanceledException>(async () => await AudioCodec.ConvertAsync([1], AudioOutputFormat.Wav, new CancellationToken(true)));
    }

    [Test]
    public void WaveOutputDescribesTheNativePcmContract()
    {
        byte[] pcm = new byte[48000];
        byte[] wave = AudioCodec.WrapWave(pcm);
        Assert.Multiple(() =>
        {
            Assert.That(wave.Length, Is.EqualTo(48044));
            Assert.That(BinaryPrimitives.ReadUInt32LittleEndian(wave.AsSpan(4)), Is.EqualTo(48036));
            Assert.That(BinaryPrimitives.ReadUInt16LittleEndian(wave.AsSpan(20)), Is.EqualTo(1));
            Assert.That(BinaryPrimitives.ReadUInt16LittleEndian(wave.AsSpan(22)), Is.EqualTo(1));
            Assert.That(BinaryPrimitives.ReadUInt32LittleEndian(wave.AsSpan(24)), Is.EqualTo(24000));
            Assert.That(BinaryPrimitives.ReadUInt32LittleEndian(wave.AsSpan(28)), Is.EqualTo(48000));
            Assert.That(BinaryPrimitives.ReadUInt16LittleEndian(wave.AsSpan(34)), Is.EqualTo(16));
            Assert.That(BinaryPrimitives.ReadUInt32LittleEndian(wave.AsSpan(40)), Is.EqualTo(pcm.Length));
        });
        Assert.Throws<InvalidDataException>(() => AudioCodec.WrapWave([1]));
    }

    [Test]
    public void SilkDecoderRejectsPacketsOutsideItsNativeBufferContract()
    {
        byte[] tooLarge = [.. "#!SILK_V3"u8, 0xe3, 4, .. new byte[1251], 1, 0, 1];
        Assert.ThrowsAsync<InvalidDataException>(async () => await AudioCodec.ConvertAsync(tooLarge, AudioOutputFormat.Pcm));
        byte[] emptyPacket = [.. "#!SILK_V3"u8, 0, 0, 1, 0, 1];
        Assert.ThrowsAsync<InvalidDataException>(async () => await AudioCodec.ConvertAsync(emptyPacket, AudioOutputFormat.Pcm));
        byte[] onePacket = [.. "#!SILK_V3"u8, 1, 0, 1];
        Assert.ThrowsAsync<NotSupportedException>(async () => await AudioCodec.ConvertAsync(onePacket, AudioOutputFormat.Pcm));
    }

    private sealed class FailedCodecStream() : NativeCodecStream((_, _, _, _) => 17);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void OutputCallback(nint userData, nint data, int length);

    private sealed class SuccessfulCodecStream() : NativeCodecStream((_, _, callback, userData) =>
    {
        byte[] converted = [8, 9, 10, 11];
        var pin = GCHandle.Alloc(converted, GCHandleType.Pinned);
        try
        {
            Marshal.GetDelegateForFunctionPointer<OutputCallback>(callback)(userData, pin.AddrOfPinnedObject(), converted.Length);
            return 0;
        }
        finally
        {
            pin.Free();
        }
    });

    [Test]
    public async Task AsyncCopyReturnsCallbackOutputAndReadMemoryUsesConvertedBuffer()
    {
        using var input = new SuccessfulCodecStream();
        input.Write([1, 2, 3]);
        using var output = new MemoryStream();
        await input.CopyToAsync(output);
        Assert.That(output.ToArray(), Is.EqualTo(new byte[] { 8, 9, 10, 11 }));
        input.Position = 0;
        byte[] buffer = new byte[4];
        Assert.That(await input.ReadAsync(buffer.AsMemory()), Is.EqualTo(4));
        Assert.That(buffer, Is.EqualTo(output.ToArray()));
    }

    [Test]
    public void AsyncCopyMustInvokeNativeConversionInsteadOfCopyingEncodedInput()
    {
        using var input = new FailedCodecStream();
        input.Write([1, 2, 3]);
        using var output = new MemoryStream();
        Assert.ThrowsAsync<Lagrange.Codec.Exceptions.CodecException>(async () => await input.CopyToAsync(output));
        Assert.That(output.Length, Is.Zero);
        Assert.Throws<Lagrange.Codec.Exceptions.CodecException>(() => input.ReadByte());
    }

    [Test]
    public async Task NativeWaveSilkAndPcmConversionsPreserveDurationAndProduceDecodableOutput()
    {
        if (!NativeLibrary.TryLoad("LagrangeCodec", typeof(AudioCodec).Assembly, null, out nint library))
            Assert.Ignore("Native LagrangeCodec is not installed; native codec fixture requires its platform library.");
        NativeLibrary.Free(library);
        byte[] pcm = new byte[48000];
        for (int sample = 0; sample < 24000; sample++)
            BinaryPrimitives.WriteInt16LittleEndian(pcm.AsSpan(sample * 2), (short)(Math.Sin(2 * Math.PI * 440 * sample / 24000) * 8000));
        byte[] wav = AudioCodec.WrapWave(pcm);
        byte[] silk = await AudioCodec.ConvertAsync(wav, AudioOutputFormat.Silk);
        Assert.That(AudioHelper.DetectAudio(silk), Is.EqualTo(AudioFormat.TenSilkV3));
        Assert.That(AudioHelper.GetTenSilkTime(silk), Is.EqualTo(1).Within(0.04));
        byte[] decoded = await AudioCodec.ConvertAsync(silk, AudioOutputFormat.Pcm);
        Assert.That(decoded.Length / 48000d, Is.EqualTo(1).Within(0.06));
        Assert.That(decoded.Any(value => value != 0), Is.True);
        byte[] convertedWave = await AudioCodec.ConvertAsync(wav, AudioOutputFormat.Wav);
        Assert.That(AudioHelper.DetectAudio(convertedWave), Is.EqualTo(AudioFormat.Wav));
        Assert.That((convertedWave.Length - 44) / 48000d, Is.EqualTo(1).Within(0.04));
    }
}
