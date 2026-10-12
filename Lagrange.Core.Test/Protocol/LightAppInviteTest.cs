using System.Reflection;
using System.Text.Json;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Logic.Processors;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;

namespace Lagrange.Core.Test.Protocol;

public sealed class LightAppInviteTest
{
    private static readonly Func<BotContext, BotMessage, LightAppEntity, bool> HandleLightApp =
        typeof(RichTextMsgProcessor).GetMethod("TryHandleLightApp", BindingFlags.Instance | BindingFlags.NonPublic)!
            .CreateDelegate<Func<BotContext, BotMessage, LightAppEntity, bool>>(new RichTextMsgProcessor());

    [Test]
    public void MusicShareFallsThroughToNormalMessageHandling()
    {
        const string payload = """
            {"app":"com.tencent.tuwen.lua","bizsrc":"qqconnect.sdkshare","config":{"ctime":1791280625,"forward":1,"token":"ece14ac9ec0de0252b8ac4cf91fe7877","type":"normal"},"extra":{"app_type":1,"appid":100497308,"msg_seq":7693491700157858382,"uin":3314748068},"meta":{"news":{"app_type":1,"appid":100497308,"ctime":1791280625,"desc":"林俊杰","jumpUrl":"https://i.y.qq.com/v8/playsong.html?platform=11&appshare=android_qq&appversion=20080008&hosteuin=oK6kowEAoK4z7K65oiokoivs7v**&songmid=000qNoOm0bI7HE&type=0&appsongtype=1&_wv=1&source=qq&ADTAG=qfshare","preview":"https://pic.ugcimg.cn/a8f92015a851ca108bb5978482948946/jpg1","tag":"QQ音乐","tagIcon":"https://p.qpic.cn/qqconnect/0/app_100497308_1626060999/100?max-age=2592000&t=0","title":"小瓶子（《假如王子睡着了》电影印象曲）","uin":3314748068}},"prompt":"[分享]小瓶子（《假如王子睡着了》电影印象曲）","ver":"0.0.0.1","view":"news"}
            """;
        Assert.That(HandleLightApp(null!, null!, new LightAppEntity(payload)), Is.False);
    }

    [TestCase("{}")]
    [TestCase("{\"bizsrc\":\"qun.invite\"}")]
    [TestCase("{\"bizsrc\":\"qun.invite\",\"meta\":null}")]
    [TestCase("{\"bizsrc\":\"qun.invite\",\"meta\":{\"news\":{}}}")]
    [TestCase("{\"bizsrc\":\"qun.invite\",\"meta\":{\"news\":{\"jumpUrl\":42}}}")]
    public void IncompleteCardFallsThrough(string payload)
    {
        var app = new LightAppEntity { AppName = "com.tencent.tuwen.lua", Payload = payload };
        Assert.That(HandleLightApp(null!, null!, app), Is.False);
    }

    [TestCase("not a url")]
    [TestCase("/invite?groupcode=123&msgseq=456")]
    [TestCase("https://example.com/invite?groupcode=123")]
    [TestCase("https://example.com/invite?msgseq=456")]
    [TestCase("https://example.com/invite?groupcode=abc&msgseq=456")]
    [TestCase("https://example.com/invite?groupcode=4294967296&msgseq=456")]
    [TestCase("https://example.com/invite?groupcode=123&msgseq=-1")]
    [TestCase("https://example.com/invite?groupcode=123&msgseq=18446744073709551616")]
    public void InvalidInviteUrlFallsThrough(string url)
    {
        Assert.That(HandleLightApp(null!, null!, CreateCard("com.tencent.tuwen.lua", "qun.invite", url)), Is.False);
    }

    [Test]
    public void DedicatedInviteWithDifferentBusinessSourceFallsThrough()
    {
        var app = CreateCard("com.tencent.qun.invite", "qqconnect.sdkshare", "https://example.com/?groupcode=123&msgseq=456");
        Assert.That(HandleLightApp(null!, null!, app), Is.False);
    }

    [TestCase("com.tencent.qun.invite")]
    [TestCase("com.tencent.tuwen.lua")]
    public async Task ValidInviteStillPostsNotification(string appName)
    {
        using var bot = new BotContext(new BotConfig(), new BotKeystore { Uin = 2, Uid = "self" }, new BotAppInfo());
        var sender = new BotFriend(1, "sender", "sender_uid", "", "", "", null!);
        var receiver = new BotFriend(2, "self", "self", "", "", "", null!);
        var message = new BotMessage(sender, receiver, 0);
        var received = new TaskCompletionSource<BotGroupInviteNotificationEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
        bot.EventInvoker.RegisterEvent<BotGroupInviteNotificationEvent>((_, e) => { received.TrySetResult(e); });

        var app = CreateCard(appName, "qun.invite", "https://example.com/invite?groupcode=123&msgseq=18446744073709551615");
        Assert.That(HandleLightApp(bot, message, app), Is.True);
        var notification = (await received.Task.WaitAsync(TimeSpan.FromSeconds(5))).Notification;
        Assert.Multiple(() =>
        {
            Assert.That(notification.GroupUin, Is.EqualTo(123));
            Assert.That(notification.Sequence, Is.EqualTo(ulong.MaxValue));
            Assert.That(notification.InviterUin, Is.EqualTo(1));
            Assert.That(notification.InviterUid, Is.EqualTo("sender_uid"));
            Assert.That(notification.TargetUin, Is.EqualTo(2));
        });
    }

    private static LightAppEntity CreateCard(string appName, string bizsrc, string url) =>
        new(JsonSerializer.Serialize(new { app = appName, bizsrc, meta = new { news = new { jumpUrl = url } } }));
}
