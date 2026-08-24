namespace Lagrange.Proto.Jce;

public sealed class ConfigPushPayload
{
    public int Type { get; init; }
    public byte[] Data { get; init; } = [];
    public SsoServerInfo[] Servers { get; init; } = [];
    public FileStoragePushFSSvcList? FileStorage { get; init; }
}

public static class JceConfigPushParser
{
    public static ConfigPushPayload ParsePushReq(ReadOnlySpan<byte> payload)
    {
        var packet = Primitives.JceReader.Parse<RequestPacket>(payload);
        var version2 = Primitives.JceReader.Parse<RequestDataVersion2>(packet.SBuffer);
        if (!version2.Map.TryGetValue("PushReq", out var values) ||
            !values.TryGetValue("ConfigPush.PushReq", out var body))
            return new ConfigPushPayload();

        var request = Primitives.JceReader.Parse<ConfigPushReq>(body);
        if (request.Type == 1)
        {
            var reader = new Primitives.JceReader(request.Buffer);
            var servers = reader.ReadStructList<SsoServerInfo>(1);
            return new ConfigPushPayload { Type = request.Type, Data = request.Buffer, Servers = servers };
        }

        if (request.Type == 2)
        {
            var reader = new Primitives.JceReader(request.Buffer);
            var fileStorage = FileStoragePushFSSvcList.Parse(ref reader);
            return new ConfigPushPayload { Type = request.Type, Data = request.Buffer, FileStorage = fileStorage };
        }

        return new ConfigPushPayload { Type = request.Type, Data = request.Buffer };
    }
}
