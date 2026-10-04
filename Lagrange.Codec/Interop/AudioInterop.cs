using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Lagrange.Codec.Interop;

internal static partial class AudioInterop
{
    [LibraryImport("LagrangeCodec", EntryPoint = "audio_to_pcm")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int AudioToPCM(
        IntPtr audioData,
        int dataLen,
        IntPtr callback,
        IntPtr userdata);
    
    [LibraryImport("LagrangeCodec", EntryPoint = "silk_decode")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int SilkDecode(
        IntPtr silkData,
        int dataLen,
        IntPtr callback,
        IntPtr userdata);
    
    [LibraryImport("LagrangeCodec", EntryPoint = "silk_encode")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int SilkEncode(
        IntPtr pcmData,
        int dataLen,
        IntPtr callback,
        IntPtr userdata);
}
