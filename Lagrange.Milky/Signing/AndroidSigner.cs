using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Lagrange.Core.Common;
using Lagrange.Milky.Configurations;
using Lagrange.Milky.Serialization;

namespace Lagrange.Milky.Signing;

public sealed class AndroidSigner : AndroidBotSignProvider, IDisposable
{
    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "WhiteListCommand")]
    private static extern ref HashSet<string> GetWhiteList(
        [UnsafeAccessorType("Lagrange.Core.Common.DefaultAndroidBotSignProvider, Lagrange.Core")] object? _);

    private readonly HttpClient _http = new();
    private readonly string _baseUrl;

    public AndroidSigner(LagrangeConfiguration configuration)
    {
        _baseUrl = configuration.Protocol.Signer.NormalizedBaseUrl.TrimEnd('/');
        _http = new HttpClient(new HttpClientHandler
        {
            Proxy = configuration.Protocol.Signer.ProxyUrl != null
                ? new WebProxy { Address = new Uri(configuration.Protocol.Signer.ProxyUrl) }
                : null
        });
        if (!string.IsNullOrEmpty(configuration.Protocol.Signer.Token))
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", configuration.Protocol.Signer.Token);
        }
    }

    public override bool IsWhiteListCommand(string cmd) => GetWhiteList(null).Contains(cmd);

    public override async Task<SsoSecureInfo?> GetSecSign(long uin, string cmd, int seq, ReadOnlyMemory<byte> body)
    {
        var payload = new JsonObject
        {
            ["uin"] = uin,
            ["cmd"] = cmd,
            ["seq"] = seq,
            ["buffer"] = Convert.ToHexString(body.Span),
            ["guid"] = Convert.ToHexString(Context.Keystore.Guid),
            ["version"] = Context.AppInfo.PtVersion,
            ["qua"] = Context.AppInfo.Qua
        };
        var response = await _http.PostAsync($"{_baseUrl}/sign", Json(payload));
        if (!response.IsSuccessStatusCode) return null;
        var result =
            await Serializer.JsonDeserializeAsync<ResponseRoot<SignResponse>>(
                await response.Content.ReadAsStreamAsync());
        return result?.Value is { } value
            ? new SsoSecureInfo
            {
                SecSign = Convert.FromHexString(value.Sign),
                SecToken = Convert.FromHexString(value.Token),
                SecExtra = Convert.FromHexString(value.Extra)
            }
            : null;
    }

    public override Task<byte[]> GetEnergy(long uin, string data) => GetBytes("energy",
        new JsonObject
        {
            ["uin"] = uin,
            ["data"] = data,
            ["guid"] = Convert.ToHexString(Context.Keystore.Guid),
            ["ver"] = Context.AppInfo.SdkInfo.SdkVersion,
            ["version"] = Context.AppInfo.PtVersion,
            ["qua"] = Context.AppInfo.Qua
        });

    public override Task<byte[]> GetDebugXwid(long uin, string data) => GetBytes("get_tlv553",
        new JsonObject
        {
            ["uin"] = uin,
            ["data"] = data,
            ["guid"] = Convert.ToHexString(Context.Keystore.Guid),
            ["version"] = Context.AppInfo.PtVersion,
            ["qua"] = Context.AppInfo.Qua
        });

    private async Task<byte[]> GetBytes(string endpoint, JsonObject payload)
    {
        var response = await _http.PostAsync($"{_baseUrl}/{endpoint}", Json(payload));
        if (!response.IsSuccessStatusCode) return [];
        var result =
            await Serializer.JsonDeserializeAsync<ResponseRoot<string>>(await response.Content.ReadAsStreamAsync());
        return result?.Value is { } value ? Convert.FromHexString(value) : [];
    }

    private static StringContent Json(JsonObject payload) =>
        new(payload.ToJsonString(), Encoding.UTF8, "application/json");

    public void Dispose() => _http.Dispose();

    internal sealed class ResponseRoot<T>
    {
        [JsonPropertyName("data")] public T Value { get; set; } = default!;
    }

    internal sealed class SignResponse
    {
        [JsonPropertyName("sign")] public string Sign { get; set; } = string.Empty;
        [JsonPropertyName("token")] public string Token { get; set; } = string.Empty;
        [JsonPropertyName("extra")] public string Extra { get; set; } = string.Empty;
    }
}
