using System.Numerics;
using System.Runtime.InteropServices;
using Lagrange.Codec.Entities;
using Lagrange.Codec.Interop;

namespace Lagrange.Codec;

public static class VideoCodec
{
    public static byte[] FirstFrame(byte[] video)
    {
        var handle = Marshal.AllocHGlobal(video.Length);
        Marshal.Copy(video, 0, handle, video.Length);
        
        int length = 0;
        var outPtr = IntPtr.Zero;
        int result = VideoInterop.VideoFirstFrame(handle, video.Length, ref outPtr, ref length);
        Marshal.FreeHGlobal(handle);
        
        if (result != 0)
        {
            if (outPtr != IntPtr.Zero) Marshal.FreeHGlobal(outPtr);
            throw new Exception("Failed to get first frame");
        }
        
        var output = new byte[length];
        Marshal.Copy(outPtr, output, 0, length);
        Marshal.FreeHGlobal(outPtr);
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        
        return output;
    }
    
    /// <summary>Reads video dimensions and playback duration from the native codec.</summary>
    /// <param name="video">Encoded video bytes.</param>
    /// <returns>Width, height, and the native duration in whole seconds.</returns>
    public static VideoInfo GetSize(byte[] video)
    {
        ArgumentNullException.ThrowIfNull(video);
        if (video.Length == 0) throw new ArgumentException("Video must not be empty.", nameof(video));
        var handle = Marshal.AllocHGlobal(video.Length);
        try
        {
            Marshal.Copy(video, 0, handle, video.Length);
            var result = new VideoInfo();
            int code = VideoInterop.VideoGetSize(handle, video.Length, ref result);
            if (code != 0 || result.Width <= 0 || result.Height <= 0 || result.Duration < 0)
                throw new Exceptions.CodecException($"Failed to get video information. Error code: {code}");
            return result;
        }
        finally
        {
            Marshal.FreeHGlobal(handle);
        }
    }
}
