using System.Buffers.Binary;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using FastEndpoints;
using Lagrange.Core;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Cryptography;
using Lagrange.Milky.Api;
using Lagrange.Milky.Api.Handlers.System;
using Lagrange.Milky.Configurations;
using Lagrange.Milky.Extensions;
using Lagrange.Milky.Security;
using Lagrange.Milky.Serialization;
using Lagrange_Milky;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lagrange.Milky.Test;

public sealed class FriendsStatusTests
{
    private const string SelfUid = "u_test_self";
    private const string Route = "/api/get_friends_status";

    [Test]
    public void GeneratedContextsPreserveNullStatusAndRawValues()
    {
        var response = new MilkyApiResponse<GetFriendsStatusHandler.Result>(new()
        {
            SelfUid = SelfUid,
            Friends = [new() { Uid = "u_a", Status = null, StatusText = "", RawFields =
                [new() { Field = 7, WireType = 0, Value = "18446744073709551615" }, new() { Field = 21, WireType = 2, Value = "" }] }]
        });
        var options = new JsonSerializerOptions().AddSerializerContextsFromLagrange_Milky();
        string generated = JsonSerializer.Serialize(response, options.GetTypeInfo(response.GetType()));
        using var json = JsonDocument.Parse(Serializer.JsonSerialize(response));
        Assert.That(json.RootElement.GetProperty("data").GetProperty("self_uid").GetString(), Is.EqualTo(SelfUid));
        var entry = json.RootElement.GetProperty("data").GetProperty("friends")[0];
        Assert.That(entry.GetProperty("status").ValueKind, Is.EqualTo(JsonValueKind.Null));
        Assert.That(entry.GetProperty("raw_fields")[0].GetProperty("value").GetString(), Is.EqualTo("18446744073709551615"));
        Assert.That(entry.GetProperty("raw_fields")[1].GetProperty("value").GetString(), Is.Empty);
        using var other = JsonDocument.Parse(generated);
        Assert.That(other.RootElement.GetProperty("data").GetProperty("friends")[0].GetProperty("status").ValueKind, Is.EqualTo(JsonValueKind.Null));
    }

    [Test]
    public async Task HttpRoundTripPreservesListAndHandlesErrorsTimeoutAndCancellation()
    {
        var signer = new Signer();
        using var bot = BotFactory.Create(new BotConfig { GetOptimumServer = false, SignProvider = signer },
            new BotKeystore { Uin = 123456 }, BotAppInfo.ProtocolToAppInfo[Protocols.Linux]);
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton(bot);
        builder.Services.AddSingleton(new MilkyConfiguration(new("127.0.0.1", 0), "test-token"));
        builder.Services.AddAuthentication("Milky").AddScheme<AuthenticationSchemeOptions, MilkyAuthenticationHandler>("Milky", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddFastEndpoints(options =>
        {
            options.DisableAutoDiscovery = true;
            options.Assemblies = [typeof(GetFriendsStatusHandler).Assembly];
            options.Filter = type => type == typeof(GetFriendsStatusHandler);
        });
        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseFastEndpoints(options => options.ConfigureMilkyBinding());
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
        try
        {
            using var unauthorized = await client.PostAsync(Route, null);
            Assert.That(unauthorized.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
            using (var missingUid = await Post())
                Assert.That(missingUid.RootElement.GetProperty("retcode").GetInt32(), Is.EqualTo(-500));
            Assert.That(signer.Calls, Is.Zero);
            Assert.That(bot.PacketContext.PendingCount, Is.Zero);
            bot.Keystore.Uid = SelfUid;

            using var server = new TcpListener(IPAddress.Loopback, 0);
            server.Start();
            var accepted = server.AcceptTcpClientAsync();
            Assert.That(await bot.SocketContext.Connect("127.0.0.1", (ushort)((IPEndPoint)server.LocalEndpoint).Port), Is.True);
            using var peer = await accepted;
            var stream = peer.GetStream();
            byte[] self = [0x0a, 0x0b, .. Encoding.ASCII.GetBytes(SelfUid), .. Convert.FromHexString("100A18017A0CE794B5E88491E59CA8E7BABFAA01020800")];
            byte[] friend = Convert.FromHexString("0A03755F6238FFFFFFFFFFFFFFFFFF01");
            var body = ProtoHelper.Serialize(new FriendsStatusResponse { SelfUid = SelfUid, Friends = [self, friend, self] });
            using (var success = await Exchange(body))
            {
                Assert.That(success.RootElement.GetProperty("retcode").GetInt32(), Is.Zero, success.RootElement.GetRawText());
                var data = success.RootElement.GetProperty("data");
                Assert.That(data.GetProperty("self_uid").GetString(), Is.EqualTo(SelfUid));
                var friends = data.GetProperty("friends");
                Assert.That(friends.GetArrayLength(), Is.EqualTo(3));
                Assert.That(friends[0].GetProperty("uid").GetString(), Is.EqualTo(SelfUid));
                Assert.That(friends[2].GetProperty("uid").GetString(), Is.EqualTo(SelfUid));
                Assert.That(friends[0].GetProperty("status").GetInt32(), Is.EqualTo(10));
                Assert.That(friends[0].GetProperty("status_text").GetString(), Is.EqualTo("电脑在线"));
                Assert.That(friends[0].GetProperty("raw_fields")[1].GetProperty("value").GetString(), Is.EqualTo("0800"));
                Assert.That(friends[1].GetProperty("uid").GetString(), Is.EqualTo("u_b"));
                Assert.That(friends[1].GetProperty("status").ValueKind, Is.EqualTo(JsonValueKind.Null));
                Assert.That(friends[1].GetProperty("raw_fields")[0].GetProperty("value").GetString(), Is.EqualTo("18446744073709551615"));
            }
            using (var serverError = await Exchange(ReadOnlyMemory<byte>.Empty, 42, "status unavailable"))
            {
                Assert.That(serverError.RootElement.GetProperty("retcode").GetInt32(), Is.EqualTo(42));
                Assert.That(serverError.RootElement.GetProperty("message").GetString(), Is.EqualTo("status unavailable"));
            }
            using (var malformed = await Exchange(ProtoHelper.Serialize(new FriendsStatusResponse
                { SelfUid = SelfUid, Friends = [new byte[] { 0x10, 0x0a }] })))
                Assert.That(malformed.RootElement.GetProperty("retcode").GetInt32(), Is.EqualTo(-500));
            using (var empty = await Exchange(ProtoHelper.Serialize(new FriendsStatusResponse { SelfUid = SelfUid })))
                Assert.That(empty.RootElement.GetProperty("data").GetProperty("friends").GetArrayLength(), Is.Zero);

            var timed = Post();
            var timedPacket = await ReadRequest(stream);
            using (var timeout = await timed.WaitAsync(TimeSpan.FromSeconds(15)))
                Assert.That(timeout.RootElement.GetProperty("retcode").GetInt32(), Is.EqualTo(-408));
            await stream.WriteAsync(Response(timedPacket, ProtoHelper.Serialize(new Oidb { Body = body })));

            using var canceled = new CancellationTokenSource();
            var canceledRequest = client.PostAsync(Route, null, canceled.Token);
            var canceledPacket = await ReadRequest(stream);
            canceled.Cancel();
            Assert.That(async () => await canceledRequest, Throws.InstanceOf<OperationCanceledException>());
            await stream.WriteAsync(Response(canceledPacket, ProtoHelper.Serialize(new Oidb { Body = body })));
            Assert.That(signer.Calls, Is.EqualTo(6), "Exactly one signed send per operation; no retries or friend-list lookups.");

            async Task<JsonDocument> Exchange(ReadOnlyMemory<byte> payload, uint result = 0, string message = "")
            {
                var pending = Post();
                var packet = await ReadRequest(stream);
                Assert.That(packet.Command, Is.EqualTo("OidbSvcTrpcTcp.0x116d_1"));
                var request = ProtoHelper.Deserialize<Oidb>(packet.Data.Span);
                Assert.That(ProtoHelper.Deserialize<FriendsStatusRequest>(request.Body.Span).SelfUid, Is.EqualTo(SelfUid));
                await stream.WriteAsync(Response(packet, ProtoHelper.Serialize(new Oidb { Body = payload, Result = result, Message = message })));
                return await pending.WaitAsync(TimeSpan.FromSeconds(5));
            }
        }
        finally { await app.StopAsync(); }

        async Task<JsonDocument> Post()
        {
            using var response = await client.PostAsync(Route, null);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        }
    }

    [Test]
    public void HandlerPropagatesCallerCancellation()
    {
        using var bot = BotFactory.Create(new BotConfig { GetOptimumServer = false },
            new BotKeystore { Uid = SelfUid }, BotAppInfo.ProtocolToAppInfo[Protocols.Linux]);
        using var ct = new CancellationTokenSource();
        ct.Cancel();
        Assert.ThrowsAsync<OperationCanceledException>(async () => await new GetFriendsStatusHandler(bot).ExecuteAsync(ct.Token));
    }

    private sealed class Signer : BotSignProvider
    {
        public int Calls;
        public override bool IsWhiteListCommand(string cmd) => cmd == "OidbSvcTrpcTcp.0x116d_1";
        public override Task<SsoSecureInfo?> GetSecSign(long uin, string cmd, int seq, ReadOnlyMemory<byte> body)
        {
            Interlocked.Increment(ref Calls);
            return Task.FromResult<SsoSecureInfo?>(null);
        }
    }

    private static async Task<BotSsoPacket> ReadRequest(NetworkStream stream)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        byte[] header = new byte[4];
        await stream.ReadExactlyAsync(header, timeout.Token);
        byte[] frame = new byte[BinaryPrimitives.ReadInt32BigEndian(header) - 4];
        await stream.ReadExactlyAsync(frame, timeout.Token);
        return DecodeRequest(frame);
    }

    private static BotSsoPacket DecodeRequest(byte[] frame)
    {
        var reader = new BinaryPacket(frame);
        Assert.That(reader.Read<int>(), Is.EqualTo(12));
        reader.Read<byte>();
        reader.ReadBytes(Prefix.Int32 | Prefix.WithPrefix);
        reader.Read<byte>();
        reader.ReadString(Prefix.Int32 | Prefix.WithPrefix);
        var sso = new BinaryPacket(TeaProvider.Decrypt(reader.ReadBytes(), new byte[16]));
        var head = new BinaryPacket(sso.ReadBytes(Prefix.Int32 | Prefix.WithPrefix));
        int sequence = head.Read<int>();
        head.Read<int>();
        head.Read<int>();
        head.ReadBytes(stackalloc byte[12]);
        head.ReadBytes(Prefix.Int32 | Prefix.WithPrefix);
        string command = head.ReadString(Prefix.Int32 | Prefix.WithPrefix);
        return new(command, sso.ReadBytes(Prefix.Int32 | Prefix.WithPrefix).ToArray(), sequence);
    }

    private static byte[] Response(BotSsoPacket packet, ReadOnlyMemory<byte> body)
    {
        using var head = new BinaryPacket(256);
        head.Write(packet.Sequence);
        head.Write(0);
        head.Write("", Prefix.Int32 | Prefix.WithPrefix);
        head.Write(packet.Command, Prefix.Int32 | Prefix.WithPrefix);
        head.Write(ReadOnlySpan<byte>.Empty, Prefix.Int32 | Prefix.WithPrefix);
        head.Write(0);
        head.Write(ReadOnlySpan<byte>.Empty, Prefix.Int32 | Prefix.WithPrefix);
        using var sso = new BinaryPacket(512);
        sso.Write(head.CreateReadOnlySpan(), Prefix.Int32 | Prefix.WithPrefix);
        sso.Write(body.Span, Prefix.Int32 | Prefix.WithPrefix);
        using var frame = new BinaryPacket(1024);
        frame.EnterLengthBarrier<int>();
        frame.Write(12);
        frame.Write((byte)EncryptType.NoEncrypt);
        frame.Write((byte)0);
        frame.Write("123", Prefix.Int32 | Prefix.WithPrefix);
        frame.Write(sso.CreateReadOnlySpan());
        frame.ExitLengthBarrier<int>(true);
        return frame.CreateReadOnlySpan().ToArray();
    }
}
