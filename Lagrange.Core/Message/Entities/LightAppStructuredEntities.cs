using System.Text;
using System.Text.Json.Nodes;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Compression;

namespace Lagrange.Core.Message.Entities;

public abstract class LightAppStructuredEntity : IMessageEntity
{
    protected abstract string App { get; }
    protected abstract string View { get; }
    protected abstract JsonObject BuildJson();
    protected abstract IMessageEntity? ParseJson(JsonNode json);

    internal string Payload => BuildJson().ToJsonString();

    Elem[] IMessageEntity.Build()
    {
        using var payload = new BinaryPacket();
        payload.Write<byte>(1);
        payload.Write(ZCompression.ZCompress(Encoding.UTF8.GetBytes(Payload)));
        return [new Elem { LightAppElem = new LightAppElem { BytesData = payload.ToArray() } }];
    }

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.LightAppElem is not { } light || light.BytesData.Length <= 1) return null;
        try
        {
            var json = JsonNode.Parse(Encoding.UTF8.GetString(ZCompression.ZDecompress(light.BytesData.Span[1..], false)));
            if (json?["app"]?.GetValue<string>() != App) return null;
            if (!string.IsNullOrEmpty(View) && json["view"]?.GetValue<string>() is string view && view != View) return null;
            return json is null ? null : ParseJson(json);
        }
        catch { return null; }
    }

    public virtual string ToPreviewString() => $"[{App}]";
}

public sealed class JsonEntity : LightAppStructuredEntity
{
    protected override string App => "com.tencent.json";
    protected override string View => "Json";
    public string Data { get; init; } = string.Empty;
    public JsonEntity() { }
    public JsonEntity(string data) => Data = data;
    protected override JsonObject BuildJson() => new() { ["app"] = App, ["view"] = View, ["data"] = Data };
    protected override IMessageEntity? ParseJson(JsonNode json) => new JsonEntity(json["data"]?.ToString() ?? json.ToJsonString());
}

public sealed class LocationEntity : LightAppStructuredEntity
{
    protected override string App => "com.tencent.map";
    protected override string View => "LocationShare";
    public string Latitude { get; init; } = string.Empty;
    public string Longitude { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public LocationEntity() { }
    public LocationEntity(string latitude, string longitude, string title, string content) => (Latitude, Longitude, Title, Content) = (latitude, longitude, title, content);
    protected override JsonObject BuildJson() => new() { ["app"] = App, ["view"] = View, ["meta"] = new JsonObject { ["locationSearch"] = new JsonObject { ["lat"] = Latitude, ["lng"] = Longitude, ["name"] = Title, ["address"] = Content, ["enum_relation_type"] = 1, ["from"] = "plusPanel" } } };
    protected override IMessageEntity? ParseJson(JsonNode json) { var m = json["meta"]?["locationSearch"] ?? json["meta"]; return new LocationEntity(m?["lat"]?.ToString() ?? string.Empty, m?["lng"]?.ToString() ?? string.Empty, m?["name"]?.ToString() ?? string.Empty, m?["address"]?.ToString() ?? string.Empty); }
}

public sealed class MusicEntity : LightAppStructuredEntity
{
    protected override string App => "com.tencent.music";
    protected override string View => "MusicShare";
    public string Type { get; init; } = string.Empty;
    public string Id { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Audio { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Image { get; init; } = string.Empty;
    public MusicEntity() { }
    protected override JsonObject BuildJson() => new() { ["app"] = App, ["view"] = View, ["type"] = Type, ["meta"] = new JsonObject { ["music"] = new JsonObject { ["id"] = Id, ["url"] = Url, ["audio"] = Audio, ["title"] = Title, ["content"] = Content, ["image"] = Image } } };
    protected override IMessageEntity? ParseJson(JsonNode json) { var m = json["meta"]?["music"] ?? json; return new MusicEntity { Type = json["type"]?.ToString() ?? string.Empty, Id = m?["id"]?.ToString() ?? string.Empty, Url = m?["url"]?.ToString() ?? string.Empty, Audio = m?["audio"]?.ToString() ?? string.Empty, Title = m?["title"]?.ToString() ?? string.Empty, Content = m?["content"]?.ToString() ?? string.Empty, Image = m?["image"]?.ToString() ?? string.Empty }; }
}

public sealed class ShareEntity : LightAppStructuredEntity
{
    protected override string App => "com.tencent.share";
    protected override string View => "Share";
    public string Url { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Image { get; init; } = string.Empty;
    public ShareEntity() { }
    protected override JsonObject BuildJson() => new() { ["app"] = App, ["view"] = View, ["url"] = Url, ["title"] = Title, ["content"] = Content, ["image"] = Image };
    protected override IMessageEntity? ParseJson(JsonNode json) => new ShareEntity { Url = json["url"]?.ToString() ?? string.Empty, Title = json["title"]?.ToString() ?? string.Empty, Content = json["content"]?.ToString() ?? string.Empty, Image = json["image"]?.ToString() ?? string.Empty };
}

public sealed class ContactEntity : LightAppStructuredEntity
{
    protected override string App => "com.tencent.contact";
    protected override string View => "Contact";
    public string ContactType { get; init; } = string.Empty;
    public string Id { get; init; } = string.Empty;
    public ContactEntity() { }
    protected override JsonObject BuildJson() => new() { ["app"] = App, ["view"] = View, ["type"] = ContactType, ["id"] = Id };
    protected override IMessageEntity? ParseJson(JsonNode json) => new ContactEntity { ContactType = json["type"]?.ToString() ?? string.Empty, Id = json["id"]?.ToString() ?? string.Empty };
}
