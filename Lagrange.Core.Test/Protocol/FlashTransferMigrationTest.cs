using System.Net;
using System.Security.Cryptography;
using System.Text;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Events;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Context;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Internal.Services.System;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Test.Protocol;

public sealed class FlashTransferMigrationTest
{
    [Test]
    public void PrepareMatchesCapturedProtocolPacket()
    {
        var request = FlashTransferMigrationBuilder.Prepare("285d4e0b-64e4-49ad-9d0c-fb4898990bab", "241f5d88-3675-b00a-02a0-0c050a8402ee", 1, 2, "屏幕录制 2025-12-22 151429.mp4", 5843232, "d1837bbc6fa1e019a62c0474ac28ec61145eea8a", null);
        request.Head!.Sub!.Seq = 8;
        Assert.That(Convert.ToHexString(ProtoHelper.Serialize(request).Span).ToLowerInvariant(), Is.EqualTo(PrepareCapture));
    }

    [Test]
    public void DownloadMetadataUsesMainFileIdFromCapture()
    {
        var envelope = ProtoHelper.Deserialize<Oidb>(Convert.FromHexString(DownloadCapture));
        var metadata = ProtoHelper.Deserialize<MigrationFlashGetDownloadUrlResp>(envelope.Body.Span);
        var file = metadata.Entry!.FileInfo.Single();
        Assert.Multiple(() =>
        {
            Assert.That(file.FileSize, Is.EqualTo(7928764));
            Assert.That(file.Field6, Is.EqualTo(1));
            Assert.That(file.MainFile!.FileId, Is.EqualTo("EhQ-cRbsTMS80LyHF7tVXbl8SlZNcBi89-MDILV0KIqT0ZmGlZUDMgRwcm9kUIDqSVoQVcvHCMSXXeUkTrl8NU_JAXoDLkb_ggECZ3o"));
            Assert.That(FlashTransferMigrationBuilder.Download(file).Payload!.Field2!.FilesetWrap!.Field3, Is.EqualTo(26));
        });
    }

    [TestCase("abc", "https://qfile.qq.com/q/abc")]
    [TestCase("https://qfile.qq.com/q/abc/", "https://qfile.qq.com/q/abc")]
    public void OfficialShareCodes(string input, string expected) => Assert.That(FlashTransferExt.OfficialShareUri(input).AbsoluteUri, Is.EqualTo(expected));

    [TestCase("http://qfile.qq.com/q/a")]
    [TestCase("https://evil.example/q/a")]
    [TestCase("https://user@qfile.qq.com/q/a")]
    [TestCase("https://qfile.qq.com:444/q/a")]
    [TestCase("https://qfile.qq.com/q/a%2fb")]
    [TestCase("a b")]
    public void InvalidShareCodes(string input) => Assert.Throws<ArgumentException>(() => FlashTransferExt.OfficialShareUri(input));

    [TestCase("{\"fileset_id\":\"12345678-1234-1234-1234-123456789abc\"}")]
    [TestCase("{\\\"fileset_id\\\":\\\"12345678-1234-1234-1234-123456789abc\\\"}")]
    public void ParseSharePages(string html) => Assert.That(FlashTransferExt.ParseSharePage(html), Is.EqualTo("12345678-1234-1234-1234-123456789abc"));

    [Test]
    public void PrimaryDownloadCombinesSeparateKey()
    {
        byte[] data = Encoding.Latin1.GetBytes("\u0001rkey=abc-123\u0001multimedia.qfile.qq.com\u0001/download?appid=14901&fileid=a\u0001");
        Assert.That(FlashTransferExt.ParseDownload(data), Is.EqualTo("https://multimedia.qfile.qq.com/download?appid=14901&fileid=a&rkey=abc-123"));
    }

    [Test]
    public async Task SliceUploadChecksRangesAndCumulativeHash()
    {
        using var bot = new BotContext(new(), new(), new());
        var handler = new RecordingHandler(false);
        using var transfer = new FlashTransferContext(bot, new HttpClient(handler));
        byte[] data = new byte[1024 * 1024 + 11];
        for (int i = 0; i < data.Length; i++) data[i] = (byte)i;
        using var stream = new MemoryStream(data);
        Assert.That(await transfer.UploadFile("key", 14901, stream), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(handler.Requests, Has.Count.EqualTo(2));
            Assert.That(handler.Requests[0].Body.Start, Is.EqualTo(0));
            Assert.That(handler.Requests[0].Body.End, Is.EqualTo(1024 * 1024 - 1));
            Assert.That(handler.Requests[1].Body.Start, Is.EqualTo(1024 * 1024));
            Assert.That(handler.Requests[1].Body.End, Is.EqualTo(data.Length - 1));
            Assert.That(handler.Requests[1].Body.Sha1StateV.State.Last(), Is.EqualTo(SHA1.HashData(data)));
            Assert.That(handler.Requests[1].Body.Sha1, Is.EqualTo(SHA1.HashData(data.AsSpan(1024 * 1024))));
        });
    }

    [Test]
    public async Task RejectedSliceStopsRemainingUploads()
    {
        using var bot = new BotContext(new(), new(), new());
        var handler = new RecordingHandler(true);
        using var transfer = new FlashTransferContext(bot, new HttpClient(handler));
        using var stream = new MemoryStream(new byte[1024 * 1024 + 1]);
        Assert.That(await transfer.UploadFile("key", 14901, stream), Is.False);
        Assert.That(handler.Requests, Has.Count.EqualTo(1));
    }

    [Test]
    public void CancelledUploadSendsNoSlices()
    {
        using var bot = new BotContext(new(), new(), new());
        var handler = new RecordingHandler(false);
        using var transfer = new FlashTransferContext(bot, new HttpClient(handler));
        using var stream = new MemoryStream([1]);
        Assert.ThrowsAsync<TaskCanceledException>(async () => await transfer.UploadFile("key", 14901, stream, new CancellationToken(true)));
        Assert.That(handler.Requests, Is.Empty);
    }


    [Test]
    public async Task MultiFileUploadCommitsTogetherAndRegistersBeforeSlices()
    {
        var transport = new UploadTransport(false, false);
        using var first = new MemoryStream([1, 2]);
        using var second = new MemoryStream([3, 4, 5]);
        var result = await FlashTransferExt.UploadCore(transport, [new("a.txt", first), new("b.zip", second)], null, default);
        Assert.Multiple(() =>
        {
            Assert.That(result.Files.Select(x => x.FileIndex), Is.EqualTo(new uint[] { 1, 2 }));
            Assert.That(transport.Commit!.Entries!.Count, Is.EqualTo(2));
            Assert.That(transport.Created!.FileSize, Is.EqualTo(5));
            Assert.That(transport.Calls.Take(8), Is.EqualTo(new[] { "create", "commit", "complete", "prepare", "apply", "prepare", "apply", "slice" }));
            Assert.That(transport.Calls.Last(), Is.EqualTo("status"));
            Assert.That(first.CanRead, Is.True);
        });
    }

    [Test]
    public void MultiFileSliceFailureDoesNotFinalizeOrUploadThumbnails()
    {
        var transport = new UploadTransport(true, false);
        using var first = new MemoryStream([1]);
        using var second = new MemoryStream([2]);
        Assert.ThrowsAsync<OperationException>(async () => await FlashTransferExt.UploadCore(transport, [new("a", first), new("b", second)], null, default));
        Assert.Multiple(() =>
        {
            Assert.That(transport.Calls.Count(x => x == "slice"), Is.EqualTo(1));
            Assert.That(transport.Calls, Does.Not.Contain("status"));
            Assert.That(transport.Calls.Count(x => x == "prepare"), Is.EqualTo(2));
        });
    }

    [Test]
    public async Task InstantUploadStillRegistersFiles()
    {
        var transport = new UploadTransport(false, true);
        using var first = new MemoryStream([1]);
        await FlashTransferExt.UploadCore(transport, [new("a", first)], null, default);
        Assert.Multiple(() =>
        {
            Assert.That(transport.Calls, Does.Not.Contain("slice"));
            Assert.That(transport.Calls.Count(x => x == "apply"), Is.EqualTo(3));
            Assert.That(transport.Calls.Last(), Is.EqualTo("status"));
        });
    }

    private sealed class UploadTransport(bool failSlice, bool instant) : IFlashTransferTransport
    {
        internal List<string> Calls { get; } = [];
        internal CommitFlashFileEventReq? Commit { get; private set; }
        internal CreateFlashTaskEventReq? Created { get; private set; }
        public Task<T> Send<T>(ProtocolEvent request, CancellationToken ct) where T : ProtocolEvent
        {
            ProtocolEvent response;
            switch (request)
            {
                case CreateFlashTaskEventReq create:
                    Calls.Add("create"); Created = create;
                    response = new CreateFlashTaskEventResp(new BotFlashCreateResult { FilesetUuid = "set", UploadKey = "key" }); break;
                case CommitFlashFileEventReq commit:
                    Calls.Add("commit"); Commit = commit; response = new CommitFlashFileEventResp(); break;
                case CompleteFlashTaskEventReq:
                    Calls.Add("complete"); response = new CompleteFlashTaskEventResp(); break;
                case FlashTransferPrepareReq:
                    Calls.Add("prepare"); response = new FlashTransferPrepareResp(new() { RkeyWrap = instant ? null : new() { Rkey = "key" } }); break;
                case FlashTransferApplyReq:
                    Calls.Add("apply"); response = new FlashTransferApplyResp(new()); break;
                case SetFlashTaskStatusEventReq:
                    Calls.Add("status"); response = new SetFlashTaskStatusEventResp(); break;
                default: throw new InvalidOperationException("Unexpected flash request.");
            }
            return Task.FromResult((T)response);
        }
        public Task<bool> Upload(string key, uint appId, Stream stream, CancellationToken ct) { Calls.Add("slice"); return Task.FromResult(!failSlice); }
    }

    private sealed class RecordingHandler(bool reject) : HttpMessageHandler
    {
        internal List<FlashTransferUploadReq> Requests { get; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Requests.Add(ProtoHelper.Deserialize<FlashTransferUploadReq>(await request.Content!.ReadAsByteArrayAsync(ct)));
            return new(HttpStatusCode.OK) { Content = new ByteArrayContent(ProtoHelper.Serialize(new FlashTransferUploadResp { Status = reject ? "failed" : "success" }).ToArray()) };
        }
    }

    // Protocol byte-oracle fixtures captured with QQ 9.9.26-44343.
    private const string PrepareCapture = "0a180a0408081064120ca80602b00604b80616c00c051a0208011298020a6b0a6708a0d2e40212001a28643138333762626336666131653031396136326330343734616332386563363131343565656138612222e5b18fe5b995e5bd95e588b620323032352d31322d3232203135313432392e6d70342a08080010001800200030003800400048011000100118002000280032120a040800120012021a001a045a0062005000380040004a88010a2432383564346530622d363465342d343961642d396430632d666234383938393930626162122432383564346530622d363465342d343961642d396430632d6662343839383939306261621a2432343166356438382d333637352d623030612d303261302d30633035306138343032656520012800300038024200480150005800600068007000";
    private const string DownloadCapture = "08d4a7021001180022b8090ab50910011afa080a2465373435333337372d633865612d343034652d393238352d353361613566616431393832122466613038663938332d323337662d303833632d656536332d6139646164336161616565352801300138024222e5b18fe5b995e5bd95e588b620323032352d30352d3033203135313234312e6d70344a2162696e676d756c757a686920323032352d30352d3033203135313234312e6d703458bcf7e3036ae9050a6645685330442d4c3261494b306b74314d51424f764f5377746465753574426a7939774567746e516f724c6d736d5961566c514d794248427962325251674f704a5768424d3744724a3276364843594c766576376b6f30587565674d4d61502d4341514a6e656712a804080212a30468747470733a2f2f6d756c74696d656469612e7166696c652e71712e636f6d2f646f776e6c6f61643f61707069643d313439303226636c69656e745f747970653d77696e26636c69656e745f7665723d392e322e33342666696c6569643d45685330442d4c3261494b306b74314d51424f764f5377746465753574426a7939774567746e516f724c6d736d5961566c514d794248427962325251674f704a5768424d3744724a3276364843594c766576376b6f30587565674d4d61502d4341514a6e656726666c64633d696358456459355a6b5346395f59347239514f526e58562d6a6b31536d397753546b4a316a6876334f357a5f747331484966612d4c6a7662416b52494531526c4373704a4776336c453469635434467437414a794c33577242304b78696a555f423270386a75367a3371314d26726b65793d43414d5371414878697a55514532647437517975726675552d78446d3568666a4d727073325a69723764535f42354d55694349644e594c495167792d5264776d495a797a584a6f31714a687834464d756b346a337042306a5f37787850315a7a6748706469315f3158376178422d736541556c5f3646546f4679597638564d5337645a577036414275597a3875517a79306c487574676a6762714271477138336d70417a6465426475555833513043732d426b54676952767768664b3835696661774a3874477371576e70784871394f444c762d5f37566a78526e424456587046314f306343551a286234306665326636363838326234393264643463343031336166333932633264373565626239623420f2f7012a20393365643461306234383164623936343335323034363339626366393133313830800438c002726b0a674568512d63526273544d5338304c79484637745658626c38536c5a4e63426938392d4d44494c56304b497154305a6d476c5a55444d675277636d396b5549447153566f5156637648434d53585865556b54726c384e555f4a41586f444c6b625f676745435a336f18027a2466613038663938332d323337662d303833632d656536332d613964616433616161656535900103a2012833653731313665633463633462636430626338373137626235353564623937633461353634643730a801bcf7e303b2010308d547ca0120346365613333363331386338303864646431646630613333633938316339316122327b226f6666736574223a31382c22637572736f725f74696d65223a22303030312d30312d30315430303a30303a30305a227d28012a005a1d0a0f636c69656e745f636f6e6e5f736571120a313738313932393033395a200a1a747270632d6f632d7072696f726974792d706173732d6261636b12022d315a1c0a0e73736f5f647965696e675f6b6579120a323535303431393036385a160a0c73736f5f636c69656e74696412063133333034375a9c040a08747270632d73736f128f04000000147061d586907300000ba05fe3000207b7000001fb0073c9510000000e3235353034313930363820071b1b000008040000000000000000000003000000004c668066ca67e24ac9a81bd08aa343840338fa2f632ec020158218d5d2a35f1a2ab856dcbdcc486af66c8ba961db5532e2a065d882604a01539050c7ad17748f95521b4dac5e1834160000001b4f696462537663547270635463702e3078393364345f31000000188e0800005f3a366a000000008d0b00000000000000000000000000002462376234353837623661333666373535346564353235613563396230303166660000000000000000000000040000011d1a007a91020a047061d58610081a06392e322e333420c19e0c4220633165633037346532323533303539633230303530333039653965643932666552005a10b7b4587b6a36f7554ed525a5c9b001ff62206237623435383762366133366637353534656435323561356339623030316666688008720757696e646f7773820118755f6c5253732d3243757359575a6a455851707045694d518a01009001019a015208081248668066ca67e24ac9a81bd08aa343840338fa2f632ec020158218d5d2a35f1a2ab856dcbdcc486af66c8ba961db5532e2a065d882604a01539050c7ad17748f95521b4dac5e18341618c4acf8fa05a00100b80165c00100d201174f696462537663547270635463702e3078393364345f3190010d";
}
