using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Lagrange.Codec;
using Lagrange.Milky.Converters;

namespace Lagrange.Milky.Api.Handlers.File;

/// <summary>Converts an audio resource with the bundled LagrangeCodec.</summary>
/// <param name="resourceConverter">Resolves supported resource URIs.</param>
public sealed class ConvertRecordHandler(ResourceConverter resourceConverter) : Endpoint<ConvertRecordHandler.Request, MilkyApiResponse<ConvertRecordHandler.Result>>
{
    private readonly ResourceConverter _resourceConverter = resourceConverter ?? throw new ArgumentNullException(nameof(resourceConverter));

    /// <inheritdoc />
    public override void Configure()
    {
        AuthSchemes("Milky");
        Post("/api/convert_record");
    }

    /// <inheritdoc />
    public override async Task<MilkyApiResponse<Result>> ExecuteAsync(Request request, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FileUri);
        AudioOutputFormat format = request.Format switch
        {
            "pcm" => AudioOutputFormat.Pcm,
            "wav" => AudioOutputFormat.Wav,
            "silk" => AudioOutputFormat.Silk,
            _ => throw new NotSupportedException("Supported output formats are pcm, wav and silk.")
        };
        using var input = await _resourceConverter.UriToStreamAsync(request.FileUri, ct).ConfigureAwait(false);
        using var buffer = new MemoryStream();
        await input.CopyToAsync(buffer, ct).ConfigureAwait(false);
        byte[] output = await AudioCodec.ConvertAsync(buffer.ToArray(), format, ct).ConfigureAwait(false);
        return new(new Result { Base64 = Convert.ToBase64String(output), Size = output.Length, Format = request.Format });
    }

    /// <summary>Audio conversion parameters.</summary>
    public sealed class Request
    {
        private string? _format;
        /// <summary>Source file://, http(s):// or base64:// resource URI.</summary>
        [JsonPropertyName("file_uri")] public required string FileUri { get; init; }

        /// <summary>Output format: pcm, wav or silk. PCM and WAV are mono, 24 kHz and 16-bit.</summary>
        [JsonPropertyName("format")] public string Format { get => _format ?? "wav"; init => _format = value; }
    }

    /// <summary>Converted audio returned inline without writing a server file.</summary>
    public sealed class Result
    {
        /// <summary>Base64 encoded output bytes.</summary>
        [JsonPropertyName("base64")] public required string Base64 { get; init; }

        /// <summary>Output size in bytes before Base64 encoding.</summary>
        [JsonPropertyName("size")] public required int Size { get; init; }

        /// <summary>Output format.</summary>
        [JsonPropertyName("format")] public required string Format { get; init; }
    }
}
