using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Lagrange.Core.Common.Response;

namespace Lagrange.Core.Internal.Http;

internal static class QzoneFeedContent
{
    internal static void Populate(BotQzoneFeed feed)
    {
        var document = new HtmlParser().ParseDocument(feed.Html);
        var body = document.QuerySelectorAll(".f-item").FirstOrDefault(x => x.GetAttribute("data-key") == feed.Key);
        if (body is null) return;
        bool Own(IElement element) => !element.Ancestors().TakeWhile(x => x != body).OfType<IElement>().Any(x => x.ClassList.Any(c => c is "mod-comments" or "comments-list" or "f-item-passive" or "f-ct-video"));
        var metadata = body.QuerySelectorAll("[name=feed_data]").FirstOrDefault(x => Own(x) && x.GetAttribute("data-uin") == feed.UserId.ToString() && x.GetAttribute("data-fkey") == feed.Key);
        if (feed.AppId == 311) feed.MessageId = metadata?.GetAttribute("data-tid");
        var content = body.QuerySelectorAll(".f-info").Where(Own).ToArray();
        var complete = content.FirstOrDefault(x => x.ClassList.Contains("qz_info_complete") && ReadText(x).Length > 0);
        var selected = complete ?? content.FirstOrDefault(x => !x.ClassList.Contains("qz_info_complete"));
        if (selected is not null)
        {
            feed.Content = ReadText(selected);
            feed.ContentComplete = complete is not null || selected.QuerySelector("[data-cmd=qz_toggle]") is null;
        }
        var images = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in body.QuerySelectorAll(".img-item").Where(Own))
        {
            if (item.HasAttribute("data-v_itemid") || item.HasAttribute("data-videotype")) continue;
            var full = ImageUrl(item.GetAttribute("data-originurl") ?? item.GetAttribute("data-url3"));
            if (full is not null) { images.Add(full); continue; }
            foreach (var image in item.QuerySelectorAll("img"))
                if (ImageUrl(image.GetAttribute("truesrc") ?? image.GetAttribute("src")) is { } url) images.Add(url);
        }
        feed.Images = images.ToArray();
        var root = body.Ancestors().OfType<IElement>().FirstOrDefault(x => x.ClassList.Contains("f-single")) ?? body;
        if (feed.LikeCount is null && long.TryParse(root.QuerySelector(".qz_like_btn_v3")?.GetAttribute("data-likecnt"), out var count) && count >= 0) feed.LikeCount = count;
    }

    private static string? ImageUrl(string? value) => value is not null && Uri.TryCreate(value.StartsWith("//", StringComparison.Ordinal) ? "https:" + value : value, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https" ? uri.AbsoluteUri : null;

    private static string ReadText(INode node)
    {
        var text = new StringBuilder();
        void Visit(INode current)
        {
            if (current is IText content) { text.Append(content.Data); return; }
            if (current is IElement element)
            {
                if (element.GetAttribute("data-cmd") == "qz_toggle" || element.ClassList.Contains("load_img")) return;
                if (element.LocalName == "br") { text.Append('\n'); return; }
                if (element.LocalName == "img") { text.Append(element.GetAttribute("alt")); return; }
                if (element.LocalName is "div" or "p" or "li") text.Append('\n');
            }
            foreach (var child in current.ChildNodes) Visit(child);
            if (current is IElement { LocalName: "div" or "p" or "li" }) text.Append('\n');
        }
        Visit(node);
        return text.ToString().Trim();
    }
}
