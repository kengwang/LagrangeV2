using System.Buffers.Binary;
using Lagrange.Codec.Streams;

namespace Lagrange.Codec;

/// <summary>Output formats supported by the bundled audio codec.</summary>
public enum AudioOutputFormat
{
    /// <summary>Raw signed 16-bit little-endian, mono PCM at 24 kHz.</summary>
    Pcm,
    /// <summary>A RIFF/WAVE container with 24 kHz mono signed 16-bit PCM.</summary>
    Wav,
    /// <summary>Tencent SILK V3 audio.</summary>
    Silk
}

/// <summary>Converts encoded audio using LagrangeCodec.</summary>
public static class AudioCodec
{
    // Native contract: LagrangeDev/LagrangeCodec src/audio.cpp and src/silk.cpp.
    internal const int PcmSampleRate = 24000;

    /// <summary>Prepares audio for QQ voice upload, retaining the existing AMR passthrough behavior.</summary>
    /// <param name="raw">Encoded audio data.</param>
    /// <returns>QQ-compatible audio and its duration in seconds.</returns>
    public static async Task<(byte[], float)> EncodeSilkV3(byte[] raw)
    {
        ArgumentNullException.ThrowIfNull(raw);
        if (AudioHelper.DetectAudio(raw) == AudioFormat.Amr) return (raw, raw.Length / 1607f);
        byte[] result = await ConvertAsync(raw, AudioOutputFormat.Silk).ConfigureAwait(false);
        return (result, AudioHelper.GetTenSilkTime(result));
    }

    /// <summary>Converts encoded audio to PCM, WAV or Tencent SILK V3.</summary>
    /// <remarks>PCM and WAV output is mono, 24 kHz, signed 16-bit little-endian. Raw PCM input has no format header and is not accepted. Native work cannot be forcibly interrupted; cancellation is observed before, during output callbacks and after conversion.</remarks>
    /// <param name="audio">Encoded input audio supported by the native decoder, including SILK.</param>
    /// <param name="outputFormat">Requested output format.</param>
    /// <param name="cancellationToken">Cancels reading and prevents returning a cancelled conversion.</param>
    /// <returns>The converted audio bytes.</returns>
    /// <exception cref="NotSupportedException">The requested output format is not supported.</exception>
    public static Task<byte[]> ConvertAsync(byte[] audio, AudioOutputFormat outputFormat, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(audio);
        if (audio.Length == 0) throw new ArgumentException("Audio must not be empty.", nameof(audio));
        if (outputFormat is not (AudioOutputFormat.Pcm or AudioOutputFormat.Wav or AudioOutputFormat.Silk))
            throw new NotSupportedException($"Audio output format '{outputFormat}' is not supported by LagrangeCodec.");
        return Task.Run(() => Convert(audio, outputFormat, cancellationToken), cancellationToken);
    }

    private static byte[] Convert(byte[] audio, AudioOutputFormat outputFormat, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        AudioFormat inputFormat = AudioHelper.DetectAudio(audio);
        bool silk = inputFormat is AudioFormat.SilkV3 or AudioFormat.TenSilkV3;
        if (silk)
        {
            audio = NormalizeSilk(audio, inputFormat);
            if (outputFormat == AudioOutputFormat.Silk) return audio;
            // The native decoder preloads two packets and reads a final packet length.
            if (AudioHelper.CountSilkPackets(audio, 10) < 2)
                throw new NotSupportedException("The native SILK decoder requires at least two packets (40 ms).");
            for (int offset = 10; offset < audio.Length;)
            {
                int packetLength = BinaryPrimitives.ReadUInt16LittleEndian(audio.AsSpan(offset));
                // include/public/silk.h: MAX_BYTES_PER_FRAME (250) * MAX_INPUT_FRAMES (5).
                // The decoder copies three packets into a fixed-size native buffer.
                if (packetLength is 0 or > 1250)
                    throw new InvalidDataException("SILK packet exceeds the native decoder's supported packet size.");
                offset += 2 + packetLength;
            }
            audio = [.. audio, 0xff, 0xff];
        }
        using NativeCodecStream decoder = silk ? new SilkDecodeStream() : new PCMStream();
        byte[] pcm = RunCodec(decoder, audio, cancellationToken);
        if (pcm.Length == 0 || (pcm.Length & 1) != 0) throw new InvalidDataException("The audio decoder did not produce valid 16-bit PCM.");
        if (outputFormat == AudioOutputFormat.Pcm) return pcm;
        if (outputFormat == AudioOutputFormat.Wav) return WrapWave(pcm);
        using var encoder = new SilkEncodeStream();
        return RunCodec(encoder, pcm, cancellationToken);
    }

    private static byte[] RunCodec(NativeCodecStream codec, byte[] audio, CancellationToken cancellationToken)
    {
        codec.Write(audio);
        using var output = new MemoryStream();
        // NativeCodecStream overrides CopyToAsync so MemoryStream's optimized copy cannot bypass conversion.
        codec.CopyToAsync(output, 81920, cancellationToken).GetAwaiter().GetResult();
        cancellationToken.ThrowIfCancellationRequested();
        return output.ToArray();
    }

    private static byte[] NormalizeSilk(byte[] audio, AudioFormat format)
    {
        int headerLength = format == AudioFormat.TenSilkV3 ? 10 : 9;
        if (AudioHelper.CountSilkPackets(audio, headerLength) == 0) throw new InvalidDataException("SILK audio contains no packets.");
        ReadOnlySpan<byte> packets = audio.AsSpan(headerLength);
        for (int offset = 0; offset < packets.Length;)
        {
            int length = BinaryPrimitives.ReadUInt16LittleEndian(packets[offset..]);
            if (length == ushort.MaxValue)
            {
                packets = packets[..offset];
                break;
            }
            offset += 2 + length;
        }
        return [0x02, .. "#!SILK_V3"u8, .. packets];
    }

    internal static byte[] WrapWave(ReadOnlySpan<byte> pcm)
    {
        if ((pcm.Length & 1) != 0) throw new InvalidDataException("PCM must contain complete 16-bit samples.");
        byte[] result = new byte[checked(pcm.Length + 44)];
        Span<byte> header = result.AsSpan(0, 44);
        "RIFF"u8.CopyTo(header);
        BinaryPrimitives.WriteUInt32LittleEndian(header[4..], checked((uint)pcm.Length + 36));
        "WAVEfmt "u8.CopyTo(header[8..]);
        BinaryPrimitives.WriteUInt32LittleEndian(header[16..], 16);
        BinaryPrimitives.WriteUInt16LittleEndian(header[20..], 1);
        BinaryPrimitives.WriteUInt16LittleEndian(header[22..], 1);
        BinaryPrimitives.WriteUInt32LittleEndian(header[24..], PcmSampleRate);
        BinaryPrimitives.WriteUInt32LittleEndian(header[28..], PcmSampleRate * 2);
        BinaryPrimitives.WriteUInt16LittleEndian(header[32..], 2);
        BinaryPrimitives.WriteUInt16LittleEndian(header[34..], 16);
        "data"u8.CopyTo(header[36..]);
        BinaryPrimitives.WriteUInt32LittleEndian(header[40..], (uint)pcm.Length);
        pcm.CopyTo(result.AsSpan(44));
        return result;
    }
}
