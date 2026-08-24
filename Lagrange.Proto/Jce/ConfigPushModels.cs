using Lagrange.Proto;

namespace Lagrange.Proto.Jce;

[JcePackable]
public partial class RequestPacket
{
    [JceMember(1)] public short IVersion { get; set; }
    [JceMember(7)] public byte[] SBuffer { get; set; } = [];
}

[JcePackable]
public partial class RequestDataVersion2
{
    [JceMember(0)] public Dictionary<string, Dictionary<string, byte[]>> Map { get; set; } = [];
}

[JcePackable]
public partial class ConfigPushReq
{
    [JceMember(1)] public int Type { get; set; }
    [JceMember(2)] public byte[] Buffer { get; set; } = [];
}

[JcePackable]
public partial class SsoServerInfo
{
    [JceMember(1)] public string Server { get; set; } = string.Empty;
    [JceMember(2)] public int Port { get; set; }
    [JceMember(8)] public string Location { get; set; } = string.Empty;
}

[JcePackable]
public partial class FileStorageServerInfo
{
    [JceMember(1)] public string Server { get; set; } = string.Empty;
    [JceMember(2)] public int Port { get; set; }
}

[JcePackable]
public partial class BigDataIPInfo
{
    [JceMember(0)] public long Type { get; set; }
    [JceMember(1)] public string Server { get; set; } = string.Empty;
    [JceMember(2)] public long Port { get; set; }
}

[JcePackable]
public partial class BigDataIPList
{
    [JceMember(0)] public long ServiceType { get; set; }
    [JceMember(1)] public BigDataIPInfo[] IPList { get; set; } = [];
    [JceMember(3)] public long FragmentSize { get; set; }
}

[JcePackable]
public partial class BigDataChannel
{
    [JceMember(0)] public BigDataIPList[] IPLists { get; set; } = [];
    [JceMember(1)] public byte[] SigSession { get; set; } = [];
    [JceMember(2)] public byte[] KeySession { get; set; } = [];
    [JceMember(3)] public long SigUin { get; set; }
    [JceMember(4)] public int ConnectFlag { get; set; }
    [JceMember(5)] public byte[] PbBuf { get; set; } = [];
}

[JcePackable]
public partial class FileStoragePushFSSvcList
{
    [JceMember(0)] public FileStorageServerInfo[] UploadList { get; set; } = [];
    [JceMember(1)] public FileStorageServerInfo[] PicDownloadList { get; set; } = [];
    [JceMember(2)] public FileStorageServerInfo[] GPicDownloadList { get; set; } = [];
    [JceMember(3)] public FileStorageServerInfo[] QZoneProxyServiceList { get; set; } = [];
    [JceMember(4)] public FileStorageServerInfo[] UrlEncodeServiceList { get; set; } = [];
    [JceMember(5)] public BigDataChannel? BigDataChannel { get; set; }
    [JceMember(6)] public FileStorageServerInfo[] VipEmotionList { get; set; } = [];
    [JceMember(7)] public FileStorageServerInfo[] C2CPicDownList { get; set; } = [];
    [JceMember(10)] public byte[] PttList { get; set; } = [];
}
