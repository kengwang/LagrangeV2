using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Internal.Context;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Internal.Packets.Service.Migration;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;

namespace Lagrange.Core.Test.Protocol;

public sealed class VoiceMessageIdentityTest
{
    private static BotMessage Message(uint random)
    {
        var group = new BotGroup(123, "test", 1, 100, 0, null, null, null);
        var sender = new BotGroupMember(group, 456, "sender", "test", GroupMemberPermission.Member, 0, null, null, 0, 0, 0);
        var message = new BotMessage(sender, group, 0) { Sequence = 1274, Random = random, MessageId = 0 };
        message.Entities.Add(new RecordEntity
        {
            RecordLength = 2,
            MsgInfo = new MsgInfo { MsgInfoBody = [new MsgInfoBody { Index = new IndexNode
            {
                FileUuid = "media",
                Info = new Lagrange.Core.Internal.Packets.Service.FileInfo
                {
                    FileName = "voice.silk", FileHash = new string('a', 32), FileSize = 42,
                    Type = new FileType { VoiceFormat = 1 }
                }
            } }] }
        });
        return message;
    }

    [Test]
    public void ReceivedVoiceUsesFieldFourRandomRatherThanAbsentMsgUid()
    {
        var input = VoiceExt.CreateTranscriptionInput(Message(0x80000007));
        Assert.That(input.MessageId, Is.EqualTo(7));
        Assert.That(input.PeerUin, Is.EqualTo(123));
        Assert.That(input.SenderUin, Is.EqualTo(456));
        Assert.That(input.FileUuid, Is.EqualTo("media"));
    }

    [Test]
    public async Task AiHistoryWithoutMsgUidOrRandomGetsAnEchoCorrelationToken()
    {
        var input = VoiceExt.CreateTranscriptionInput(Message(0));
        Assert.That(input.MessageId, Is.InRange(1UL, (ulong)int.MaxValue - 1));
        var voice = new VoiceContext(null!, (request, group, _) =>
        {
            Assert.That(group, Is.True);
            Assert.That(request.GroupItem!.MsgId, Is.EqualTo(input.MessageId));
            Assert.That(request.GroupItem.GroupUin, Is.EqualTo(123));
            Assert.That(request.GroupItem.SenderUin, Is.EqualTo(456));
            Assert.That(request.GroupItem.Uuid, Is.EqualTo("media"));
            return Task.FromResult(new PttTransResp { GroupResult = new() { Text = "recognized" } });
        });
        Assert.That(await voice.Transcribe(input, CancellationToken.None), Is.EqualTo("recognized"));
    }
}
