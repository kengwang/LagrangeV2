using System.Text.Json;
using System.Text.RegularExpressions;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility;
using Lagrange.Proto;
namespace Lagrange.Core.Message.Entities;

/// <summary>A received QQ flash-transfer file set. Send through SendFlashMessage.</summary>
public sealed partial class FlashFileEntity : IMessageEntity
{
    /// <summary>Fileset identifier.</summary>
    public string FilesetId { get; init; } = string.Empty;
    /// <summary>Display filename.</summary>
    public string FileName { get; init; } = string.Empty;
    /// <summary>Optional thumbnail URL.</summary>
    public string? ThumbnailUrl { get; init; }
    /// <summary>QQ source scene identifier.</summary>
    public uint SceneType { get; init; }
    Elem[] IMessageEntity.Build() => throw new NotSupportedException("Use SendFlashMessage to share a file set.");
    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.CommonElem is not { ServiceType: 45 } common) return null;
        var data = ProtoHelper.Deserialize<MarkdownData>(common.PbElem.Span);
        var extra = data.ExtType == 1 ? data.ExtInfo : null;
        if (extra is null && common.BusinessType != 3 && !data.Content.Contains("FlashTransfer") && !data.Content.Contains("flash_transfer")) return null;
        string? id = null, name = null, thumb = null;
        uint scene = 0;
        var content = data.Content;
        var embedded = EmbeddedJson().Match(content);
        if (embedded.Success) content = Uri.UnescapeDataString(embedded.Groups[1].Value);
        if (extra is null && !content.Contains("FlashTransfer", StringComparison.Ordinal) && !content.Contains("flash_transfer", StringComparison.Ordinal)) return null;
        try
        {
            using var json = JsonDocument.Parse(content);
            id = Find(json.RootElement, "fileSetId", "filesetId", "fileset_id", "file_set_id");
            name = Find(json.RootElement, "title", "fileName", "name");
            thumb = Find(json.RootElement, "thumbUrl", "thumbnailUrl");
            uint.TryParse(Find(json.RootElement, "sceneType", "scene_type"), out scene);
        }
        catch (JsonException) { }
        if (string.IsNullOrWhiteSpace(id))
        {
            var match = FilesetQuery().Match(content);
            if (match.Success) id = Uri.UnescapeDataString(match.Groups[1].Value);
        }
        if (scene == 0 && SceneQuery().Match(content) is { Success: true } sceneMatch) uint.TryParse(sceneMatch.Groups[1].Value, out scene);
        if (!string.IsNullOrWhiteSpace(extra?.FilesetId)) id = extra.FilesetId;
        if (!string.IsNullOrWhiteSpace(extra?.Name)) name = extra.Name;
        thumb = extra?.Thumbnail?.Download?.Url ?? thumb;
        return string.IsNullOrWhiteSpace(id) ? null : new FlashFileEntity { FilesetId = id, FileName = name ?? string.Empty, ThumbnailUrl = thumb, SceneType = scene };
    }
    private static string? Find(JsonElement value, params string[] keys)
    {
        if (value.ValueKind == JsonValueKind.Object)
            foreach (var property in value.EnumerateObject())
            {
                if (keys.Contains(property.Name) && property.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number) return property.Value.ToString();
                if (Find(property.Value, keys) is { } found) return found;
            }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (var child in value.EnumerateArray()) if (Find(child, keys) is { } found) return found;
        return null;
    }
    [GeneratedRegex(@"[?&]json=([^&\s)]+)", RegexOptions.CultureInvariant, 100)] private static partial Regex EmbeddedJson();
    [GeneratedRegex(@"fileset_id=([^&\s)""<>]+)", RegexOptions.CultureInvariant, 100)] private static partial Regex FilesetQuery();
    [GeneratedRegex(@"[?&]scene_type=(\d+)", RegexOptions.CultureInvariant, 100)] private static partial Regex SceneQuery();
    /// <summary>Returns a short textual preview.</summary>
    public string ToPreviewString() => $"[Flash transfer: {FileName}]";
}
[ProtoPackable] internal partial class FlashFileExtra
{
    [ProtoMember(1)] public string FilesetId { get; set; } = string.Empty;
    [ProtoMember(2)] public string Name { get; set; } = string.Empty;
    [ProtoMember(3)] public uint FileSize { get; set; }
    [ProtoMember(4)] public FlashFileThumbnail? Thumbnail { get; set; }
}
[ProtoPackable] internal partial class FlashFileThumbnail
{
    [ProtoMember(2)] public FlashFileThumbnailDownload? Download { get; set; }
}
[ProtoPackable] internal partial class FlashFileThumbnailDownload
{
    [ProtoMember(2)] public string Url { get; set; } = string.Empty;
}
