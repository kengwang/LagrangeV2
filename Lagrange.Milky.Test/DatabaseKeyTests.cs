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

public sealed class DatabaseKeyTests
{
    private const string Key = "synthetic-db-key";
    private static readonly string Meta = new('a', 128);

    [Test]
    public void GeneratedSerializationIncludesBothFieldsAndTracksExplicitNull()
    {
        var options = new JsonSerializerOptions().AddSerializerContextsFromLagrange_Milky();
        var info = options.GetTypeInfo(typeof(RequestDecryptKeyHandler.Request));
        var omitted = (RequestDecryptKeyHandler.Request)JsonSerializer.Deserialize("""{"mode":"encrypt"}""", info)!;
        var explicitNull = (RequestDecryptKeyHandler.Request)JsonSerializer.Deserialize("""{"mode":"encrypt","key_meta":null}""", info)!;
        Assert.That(omitted.HasKeyMeta, Is.False);
        Assert.That(explicitNull.HasKeyMeta, Is.True);
        var response = new MilkyApiResponse<RequestDecryptKeyHandler.Result>(new() { Key = Key, KeyMeta = Meta });
        using var json = JsonDocument.Parse(Serializer.JsonSerialize(response));
        Assert.That(json.RootElement.GetProperty("data").GetProperty("key").GetString(), Is.EqualTo(Key));
        Assert.That(json.RootElement.GetProperty("data").GetProperty("key_meta").GetString(), Is.EqualTo(Meta));
        // The hand-maintained AOT context must also contain the request type.
        Assert.That(Serializer.JsonDeserialize<RequestDecryptKeyHandler.Request>("""{"mode":"decrypt"}""")!.Mode, Is.EqualTo("decrypt"));
    }

    [Test]
    public async Task HttpContractSupportsBothModesErrorsAndLegacyRoute()
    {
        var signer = new Signer();
        using var bot = BotFactory.Create(new BotConfig { GetOptimumServer = false, SignProvider = signer },
            new BotKeystore(), BotAppInfo.ProtocolToAppInfo[Protocols.Linux]);
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
            options.Assemblies = [typeof(RequestDecryptKeyHandler).Assembly];
            options.Filter = type => type == typeof(RequestDecryptKeyHandler) || type == typeof(RequestDatabaseKeyHandler);
        });
        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseFastEndpoints(options => options.ConfigureMilkyBinding());
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
        try
        {
            using var unauthorized = await client.PostAsync("/api/request_decrypt_key", Body("{}"));
            Assert.That(unauthorized.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
            string[] invalid = ["{}", """{"mode":null}""", """{"mode":"Encrypt"}""", """{"mode":"other"}""",
                """{"mode":"encrypt","key_meta":null}""", """{"mode":"encrypt","key_meta":""}""",
                """{"mode":"decrypt"}""", """{"mode":"decrypt","key_meta":null}""", """{"mode":"decrypt","key_meta":""}""",
                "{\"mode\":\"decrypt\",\"key_meta\":\"" + new string('g', 128) + "\"}",
                "{\"mode\":\"decrypt\",\"key_meta\":\"" + new string('a', 127) + "\"}",
                "{\"mode\":\"decrypt\",\"key_meta\":\"" + Meta + " \"}"];
            foreach (string body in invalid)
            {
                using var json = await Post(body);
                Assert.That(json.RootElement.GetProperty("retcode").GetInt32(), Is.EqualTo(-400), body);
                Assert.That(bot.PacketContext.PendingCount, Is.Zero);
                Assert.That(signer.Calls, Is.Zero);
            }
            using var server = new TcpListener(IPAddress.Loopback, 0);
            server.Start();
            var accepted = server.AcceptTcpClientAsync();
            Assert.That(await bot.SocketContext.Connect("127.0.0.1", (ushort)((IPEndPoint)server.LocalEndpoint).Port), Is.True);
            using var peer = await accepted;
            var stream = peer.GetStream();
            Assert.That(bot.Keystore.WLoginSigs.A2, Is.Empty);

            using (var encrypt = await Exchange("""{"mode":"encrypt"}""", 1, "0A025000",
                ProtoHelper.Serialize(new RequestEncryptKeyResponse { Info = new() { Key = Key, KeyMeta = Meta } })))
            {
                AssertSuccess(encrypt, Meta);
            }
            string decryptBody = "{\"mode\":\"decrypt\",\"key_meta\":\"" + Meta + "\"}";
            // Synthesized body field2 -> key, without metadata.
            byte[] decryptResponse = [0x12, 0x12, 0x0a, 0x10, .. Encoding.ASCII.GetBytes(Key)];
            using (var decrypt = await Exchange(decryptBody, 2, null, decryptResponse)) AssertSuccess(decrypt, "");
            using (var mismatch = await Exchange(decryptBody, 2, null, ReadOnlyMemory<byte>.Empty, 1006, "database mismatch"))
            {
                Assert.That(mismatch.RootElement.GetProperty("retcode").GetInt32(), Is.EqualTo(1006));
                Assert.That(mismatch.RootElement.GetProperty("message").GetString(), Is.EqualTo("database mismatch"));
            }
            foreach (uint subcommand in new uint[] { 1, 2 })
            {
                using var missingKey = await Exchange(subcommand == 1 ? """{"mode":"encrypt"}""" : decryptBody, subcommand, null, ReadOnlyMemory<byte>.Empty);
                Assert.That(missingKey.RootElement.GetProperty("retcode").GetInt32(), Is.EqualTo(-500));
            }
            using (var missingMeta = await Exchange("""{"mode":"encrypt"}""", 1, null,
                ProtoHelper.Serialize(new RequestEncryptKeyResponse { Info = new() { Key = Key } })))
                Assert.That(missingMeta.RootElement.GetProperty("retcode").GetInt32(), Is.EqualTo(-500));

            using (var legacy = await Exchange("{\"db_salt\":\"" + Meta + "\"}", 2, null, decryptResponse,
                route: "/api/request_database_key"))
            {
                Assert.That(legacy.RootElement.GetProperty("data").GetProperty("key").GetString(), Is.EqualTo(Key));
                Assert.That(legacy.RootElement.GetProperty("data").EnumerateObject().Count(), Is.EqualTo(1));
            }

            var timed = Post(decryptBody);
            var timedPacket = await ReadRequest(stream);
            using (var timeout = await timed.WaitAsync(TimeSpan.FromSeconds(15)))
                Assert.That(timeout.RootElement.GetProperty("retcode").GetInt32(), Is.EqualTo(-408));
            // Drain the original typed protocol pending request after the endpoint timed out.
            await stream.WriteAsync(Response(timedPacket, ProtoHelper.Serialize(new Oidb { Body = decryptResponse })));

            using var canceled = new CancellationTokenSource();
            var canceledRequest = client.PostAsync("/api/request_decrypt_key", Body(decryptBody), canceled.Token);
            var canceledPacket = await ReadRequest(stream);
            canceled.Cancel();
            Assert.That(async () => await canceledRequest, Throws.InstanceOf<OperationCanceledException>());
            await stream.WriteAsync(Response(canceledPacket, ProtoHelper.Serialize(new Oidb { Body = decryptResponse })));
            Assert.That(signer.Calls, Is.EqualTo(9), "One signed send per valid operation, with no automatic retries.");

            async Task<JsonDocument> Exchange(string body, uint subcommand, string? expectedBody, ReadOnlyMemory<byte> responseBody,
                uint result = 0, string message = "", string route = "/api/request_decrypt_key")
            {
                var pending = Post(body, route);
                var packet = await ReadRequest(stream);
                Assert.That(packet.Command, Is.EqualTo($"OidbSvcTrpcTcp.0xcde_{subcommand}"));
                var envelope = ProtoHelper.Deserialize<Oidb>(packet.Data.Span);
                Assert.That(envelope.Service, Is.EqualTo(subcommand));
                if (expectedBody is not null) Assert.That(Convert.ToHexString(envelope.Body.Span), Is.EqualTo(expectedBody));
                await stream.WriteAsync(Response(packet, ProtoHelper.Serialize(new Oidb { Body = responseBody, Result = result, Message = message })));
                return await pending.WaitAsync(TimeSpan.FromSeconds(5));
            }
        }
        finally { await app.StopAsync(); }

        async Task<JsonDocument> Post(string body, string route = "/api/request_decrypt_key")
        {
            using var response = await client.PostAsync(route, Body(body));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        }
    }

    private static void AssertSuccess(JsonDocument json, string meta)
    {
        Assert.That(json.RootElement.GetProperty("retcode").GetInt32(), Is.Zero, json.RootElement.GetRawText());
        Assert.That(json.RootElement.GetProperty("data").GetProperty("key").GetString(), Is.EqualTo(Key));
        Assert.That(json.RootElement.GetProperty("data").GetProperty("key_meta").GetString(), Is.EqualTo(meta));
    }

    private static StringContent Body(string json) => new(json, Encoding.UTF8, "application/json");

    private sealed class Signer : BotSignProvider
    {
        public int Calls;
        public override bool IsWhiteListCommand(string cmd) => cmd is "OidbSvcTrpcTcp.0xcde_1" or "OidbSvcTrpcTcp.0xcde_2";
        public override Task<SsoSecureInfo?> GetSecSign(long uin, string cmd, int seq, ReadOnlyMemory<byte> body)
        {
            Interlocked.Increment(ref Calls);
            return Task.FromResult<SsoSecureInfo?>(null);
        }
    }

    private static async Task<Lagrange.Core.Common.Entity.BotSsoPacket> ReadRequest(NetworkStream stream)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        byte[] header = new byte[4];
        await stream.ReadExactlyAsync(header, timeout.Token);
        byte[] frame = new byte[BinaryPrimitives.ReadInt32BigEndian(header) - 4];
        await stream.ReadExactlyAsync(frame, timeout.Token);
        return DecodeRequest(frame);
    }

    private static Lagrange.Core.Common.Entity.BotSsoPacket DecodeRequest(byte[] frame)
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

    private static byte[] Response(Lagrange.Core.Common.Entity.BotSsoPacket packet, ReadOnlyMemory<byte> body)
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
