using System.Reflection;
using System.Text;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Services.Http;

namespace Lagrange.Core.Test.Protocol;

public sealed class FriendDressTest
{
    [Test]
    public async Task AllDressTypesIncludingUnknownAppsAreReturned()
    {
        const string html = """
            <script>window.__INITIAL_ASYNCDATA__={"targetUin":"1136772134","isSvip":true,"rawUsingList":[
              {"appId":2,"itemId":2160922,"name":"月薪喵","image":"https://example/bubble","extrainfo":{"price":600}},
              {"appId":5,"name":"默认字体"},{"appId":23,"name":"默认头像"},
              {"appId":4,"itemId":123},{"appId":3,"itemId":1000},{"appId":20,"itemId":2406},
              {"appId":999,"itemId":42,"name":"未来装扮"}
            ]};(function(){})();</script>
            """;
        var service = new FriendDressHttpService();
        var parse = typeof(FriendDressHttpService).GetMethod("ParseResponseAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
        using var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        var task = (Task<GetFriendDressEventResp>)parse.Invoke(service,
            [null, new GetFriendDressEventReq(1136772134), response, new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(html)), CancellationToken.None])!;
        var result = (await task).Result;
        Assert.That(result.Items.Select(item => item.AppId), Is.EqualTo(new[] { 2, 5, 23, 4, 3, 20, 999 }));
        Assert.That(result.Items[0].ItemId, Is.EqualTo(2160922));
        Assert.That(result.Items[0].Price, Is.EqualTo(600));
        Assert.That(result.Items[0].Kind, Is.EqualTo("气泡"));
        Assert.That(result.Items[1].Kind, Is.EqualTo("字体"));
        Assert.That(result.Items[2].Kind, Is.EqualTo("头像"));
        Assert.That(result.Items[^1].Kind, Is.EqualTo("appId=999"));
    }
}
