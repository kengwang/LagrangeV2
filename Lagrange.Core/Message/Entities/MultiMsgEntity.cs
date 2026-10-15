using System.IO.Compression;
using System.Text;
using System.Text.Json.Nodes;
using System.Xml;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Compression;


namespace Lagrange.Core.Message.Entities;

public class MultiMsgEntity(string? resId) : IMessageEntity
{
    public List<BotMessage> Messages { get; } = [];
    
    public string? ResId { get; private set; } = resId;

    internal string FileName { get; set; } = Guid.NewGuid().ToString();
    internal bool MessagesLoaded { get; private set; }

    internal void SetMessages(IEnumerable<BotMessage> messages)
    {
        var snapshot = messages.ToArray();
        Messages.Clear();
        Messages.AddRange(snapshot);
        MessagesLoaded = true;
    }

    /// <summary>Optional presentation metadata carried by forwarded-message payloads.</summary>
    public string? Title { get; set; }
    public IReadOnlyList<string>? Preview { get; set; }
    public string? Summary { get; set; }
    public string? Prompt { get; set; }
    
    public MultiMsgEntity(List<BotMessage> messages) : this(default(string))
    {
        Messages = messages;
        MessagesLoaded = true;
    }
    
    public MultiMsgEntity() : this(default(string)) { }

    public async Task Preprocess(BotContext context, BotMessage message)
    {
        if (string.IsNullOrEmpty(ResId))
        {
            var result = await context.EventContext.SendEvent<LongMsgSendEventResp>(new LongMsgSendEventReq(message.Receiver, Messages) { TempGroupUin = message.TempGroupUin });
            ResId = result.ResId;

        }
    }
    
    public async Task Postprocess(BotContext context, BotMessage message)
    {
        if (string.IsNullOrEmpty(ResId)) return;

        if (MessagesLoaded) return;
        SetMessages(await ForwardedMessageResolver.Load(context, message.Contact is BotGroupMember, ResId));
    }
    
    string IMessageEntity.ToPreviewString() => "[聊天记录]";

    Elem[] IMessageEntity.Build() => string.IsNullOrEmpty(ResId) ? [] : BuildCard(ResId, FileName, Messages.Count);

    internal Elem[] BuildCard(string resId, string fileName, int messageCount)
    {
        int count = Math.Clamp(Messages.Count, 0, 4);
        var extra = new JsonObject { { "filename", fileName }, { "tsum", messageCount } };
        var preview = Preview ?? Messages[..count].Select(x => $"{x.Contact.Nickname}: {string.Join(' ', x.Entities.Select(e => e.ToPreviewString()))}").ToArray();
        var news = new JsonArray(preview.Select(x => (JsonNode)new JsonObject { ["text"] = x }).ToArray());
        var detail = new JsonObject
        {
            { "news", news },
            { "resid", resId },
            { "source", Title ?? "聊天记录" },
            { "title", Title ?? "聊天记录" },
            { "summary", Summary ?? $"查看{messageCount}条转发消息" },
            { "uniseq", fileName }
        };

        var lightApp = new LightApp
        {
            App = "com.tencent.multimsg",
            Config = new Config
            {
                Autosize = 1,
                Forward = 1,
                Round = 1,
                Type = "normal",
                Width = 300
            },
            Meta = new JsonObject { { "detail", detail } },
            Desc = "[聊天记录]",
            Extra = JsonHelper.Serialize(extra),
            Prompt = Prompt ?? "[聊天记录]",
            Ver = "0.0.0.5",
            View = "contact"
        };
        
        var data = JsonHelper.SerializeToUtf8Bytes(lightApp).Span;
        using var dest = new MemoryStream();
        dest.WriteByte(0x01);
        using var zlib = new ZLibStream(dest, CompressionLevel.Optimal, true);
        zlib.Write(data);
        zlib.Close();

        return [new Elem { LightAppElem = new LightAppElem { BytesData = dest.ToArray() } }];
    }

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.LightAppElem is { } light && light.BytesData.Length > 1)
        {
            try
            {
                var payload = Encoding.UTF8.GetString(ZCompression.ZDecompress(light.BytesData.Span[1..], false));
                var json = JsonNode.Parse(payload);
                if (json?["app"]?.ToString() == "com.tencent.multimsg")
                {
                    var detail = json["meta"]?["detail"];
                    var extraText = json["extra"]?.ToString();
                    var fileName = string.IsNullOrEmpty(extraText) ? null : JsonNode.Parse(extraText)?["filename"]?.ToString();
                    var result = new MultiMsgEntity(detail?["resid"]?.ToString() ?? string.Empty)
                    {
                        FileName = fileName ?? detail?["uniseq"]?.ToString() ?? string.Empty,
                        Title = detail?["title"]?.ToString() ?? detail?["source"]?.ToString(),
                        Preview = (detail?["news"] as JsonArray)?.Select(x => x?["text"]?.ToString() ?? string.Empty).ToArray(),
                        Summary = detail?["summary"]?.ToString(),
                        Prompt = json["prompt"]?.ToString()
                    };
                    return result;
                }
            }
            catch { }
        }

        if (target.RichMsg is { ServiceId: 35 } richMsg)
        {
            using var source = new MemoryStream();
            using var dest = new MemoryStream();
            using var inflate = new DeflateStream(source, CompressionMode.Decompress);
            source.Write(richMsg.BytesTemplate1.Span[3..^4]);
            source.Seek(0, SeekOrigin.Begin);
            
            inflate.CopyTo(dest);
            dest.Seek(0, SeekOrigin.Begin);
            
            using var xmlReader = XmlReader.Create(dest);
            xmlReader.Read();
            
            var doc = new XmlDocument();
            doc.Load(xmlReader);
            var root = doc["msg"];
            var titles = root?.SelectNodes("item/title")?.Cast<XmlNode>().ToArray() ?? [];
            return new MultiMsgEntity(root?.GetAttribute("m_resid") ?? string.Empty)
            {
                FileName = root?.GetAttribute("m_fileName") ?? string.Empty,
                Title = titles.FirstOrDefault()?.InnerText,
                Preview = titles.Skip(1).Select(x => x.InnerText).ToArray(),
                Summary = root?.SelectSingleNode("item/summary")?.InnerText,
                Prompt = root?.GetAttribute("brief")
            };
        }

        return null;
    }

}
