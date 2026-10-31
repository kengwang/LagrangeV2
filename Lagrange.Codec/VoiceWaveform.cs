using System.Buffers.Binary;

namespace Lagrange.Codec;

public static class VoiceWaveform
{
    public const int BinCount = 30;

    public static byte[] FromPcm(ReadOnlySpan<byte> pcm)
    {
        if (pcm.Length == 0 || (pcm.Length & 1) != 0) throw new ArgumentException("Waveform requires complete mono S16LE PCM samples.", nameof(pcm));
        int frames = pcm.Length / 2;
        var result = new byte[BinCount];
        bool audible = false;
        for (int bin = 0; bin < BinCount; bin++)
        {
            int start = (int)((long)bin * frames / BinCount);
            int end = Math.Min(frames, Math.Max(start + 1, (int)((long)(bin + 1) * frames / BinCount)));
            int peak = 0;
            for (int frame = start; frame < end; frame++) peak = Math.Max(peak, Math.Abs((int)BinaryPrimitives.ReadInt16LittleEndian(pcm.Slice(frame * 2, 2))));
            result[bin] = (byte)Math.Min(255, (int)Math.Round(peak * 255d / short.MaxValue));
            audible |= result[bin] != 0;
        }
        if (!audible) Array.Fill(result, (byte)25);
        return result;
    }

    public static async Task<byte[]> FromAudioAsync(byte[] audio, CancellationToken ct = default) => FromPcm(await AudioCodec.ConvertAsync(audio, AudioOutputFormat.Pcm, ct));
}
