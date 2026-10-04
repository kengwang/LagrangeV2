using System.Buffers.Binary;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Lagrange.Core.Common;
using Lagrange.Core.Internal.Network;
using Lagrange.Proto.Jce;

namespace Lagrange.Core.Internal.Context;

internal class SocketContext : IClientListener, IDisposable
{
    private const string Tag = nameof(SocketContext);
    
    public uint HeaderSize => 4;
    
    public bool Connected => _client.Connected;
    
    private readonly ClientListener _client;
    
    private readonly BotConfig _config;
    
    private readonly BotContext _context;
    private readonly object _serverLock = new();
    private readonly List<(string Host, ushort Port)> _pushedServers = [];
    
    public SocketContext(BotContext context)
    {
        _client = new CallbackClientListener(this);
        _config = context.Config;
        _context = context;
    }

    public uint GetPacketLength(ReadOnlySpan<byte> header) => BinaryPrimitives.ReadUInt32BigEndian(header);

    public void OnRecvPacket(ReadOnlySpan<byte> packet) => _context.PacketContext.DispatchPacket(packet);

    public void OnDisconnect()
    {
        _context.VoiceContext.Disconnect();
        _context.CacheContext.ResetSessionNotifications();
    }

    public void OnSocketError(Exception e, ReadOnlyMemory<byte> data)
    {
        
    }
    
    public async Task<bool> Connect()
    {
        if (_client.Connected) return true;
        
        var servers = await ResolveDns();
        if (_config.GetOptimumServer) await SortServers(servers);
        bool connected = false;
        (string Host, ushort Port) connectedServer = default;
        foreach (var server in servers)
        {
            connected = await _client.Connect(server.Host, server.Port);
            if (connected) { connectedServer = server; break; }
        }

        if (connected) _context.LogInfo(Tag, "Connected to the server {0}:{1}", connectedServer.Host, connectedServer.Port);
        else _context.LogError(Tag, "Failed to connect to pushed/DNS servers.");
        
        return connected;
    }
    
    public void Disconnect() => _client.Disconnect();

    internal void SetPushedServers(IEnumerable<SsoServerInfo> servers)
    {
        lock (_serverLock)
        {
            foreach (var server in servers)
            {
                if (!string.IsNullOrWhiteSpace(server.Server) && server.Port is > 0 and <= ushort.MaxValue)
                    _pushedServers.Insert(0, (server.Server, (ushort)server.Port));
            }
        }
    }
    
    public ValueTask<int> Send(ReadOnlyMemory<byte> packet) => _client.Send(packet);
    
    private async Task SortServers(List<(string Host, ushort Port)> servers)
    {
        using var ping = new Ping();
        var sorted = new List<(long Latency, (string Host, ushort Port) Server)>(servers.Count);
        
        foreach (var server in servers)
        {
            var latency = await ping.SendPingAsync(server.Host, 1000);
            if (latency.Status == IPStatus.Success)
            {
                sorted.Add((latency.RoundtripTime, server));
                _context.LogDebug(Tag, "Server: {0}:{1} Latency: {2}ms", server.Host, server.Port, latency.RoundtripTime);
            }
        }
        
        sorted.Sort((a, b) => a.Item1.CompareTo(b.Item1));
        for (int i = 0; i < sorted.Count; i++) servers[i] = sorted[i].Server;
    }
    
    private async Task<List<(string Host, ushort Port)>> ResolveDns()
    {
        string host = _config.UseIPv6Network ? "msfwifiv6.3g.qq.com" : "msfwifi.3g.qq.com";
        var entry = await Dns.GetHostEntryAsync(host, _config.UseIPv6Network ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork);
        var result = new List<(string, ushort)>();
        lock (_serverLock) result.AddRange(_pushedServers);
        result.AddRange(entry.AddressList.Select(x => (x.ToString(), (ushort)8080)));
        return result;
    }
    
    public void Dispose()
    {
        _client.Disconnect();
    }
}
