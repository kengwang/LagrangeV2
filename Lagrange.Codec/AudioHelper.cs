using System.Buffers.Binary;

namespace Lagrange.Codec;

public enum AudioFormat
{
    Unknown,
    Wav,
    Mp3,
    SilkV3,
    TenSilkV3,
    Amr,
    Ogg,
}

internal static class AudioHelper
{
    public static AudioFormat DetectAudio(byte[] data)
    {
        ReadOnlySpan<byte> span = data;
        if (span.Length >= 12 && span.StartsWith("RIFF"u8) && span.Slice(8, 4).SequenceEqual("WAVE"u8)) return AudioFormat.Wav;
        if (span.StartsWith("\x02#!SILK_V3"u8)) return AudioFormat.TenSilkV3;
        if (span.StartsWith("#!SILK_V3"u8)) return AudioFormat.SilkV3;
        if (span.StartsWith("#!AMR\n"u8)) return AudioFormat.Amr;
        if (span.StartsWith("OggS"u8)) return AudioFormat.Ogg;
        if (span.StartsWith("ID3"u8) || (span.Length >= 2 && span[0] == 0xff && (span[1] & 0xe0) == 0xe0)) return AudioFormat.Mp3;
        return AudioFormat.Unknown;
    }

    public static float GetSilkTime(byte[] data, int offset = 0) => CountSilkPackets(data, 9 + offset) * 0.02f;

    public static float GetTenSilkTime(byte[] data) => CountSilkPackets(data, 10) * 0.02f;

    internal static int CountSilkPackets(ReadOnlySpan<byte> data, int headerLength)
    {
        if (data.Length < headerLength) throw new InvalidDataException("Truncated SILK header.");
        int count = 0;
        for (int offset = headerLength; offset < data.Length;)
        {
            if (data.Length - offset < 2) throw new InvalidDataException("Truncated SILK packet length.");
            int length = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset, 2));
            offset += 2;
            if (length == ushort.MaxValue)
            {
                if (offset != data.Length) throw new InvalidDataException("Unexpected data after SILK terminator.");
                break;
            }
            if (length > short.MaxValue || length > data.Length - offset) throw new InvalidDataException("Truncated or invalid SILK packet.");
            offset += length;
            count++;
        }
        return count;
    }
}
