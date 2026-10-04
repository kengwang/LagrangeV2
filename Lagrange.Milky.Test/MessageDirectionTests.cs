using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Milky.Models.Segments;
using Lagrange.Milky.Caching;
using Lagrange.Milky.Converters;
using Lagrange.Milky.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
namespace Lagrange.Milky.Test;
public sealed class MessageDirectionTests
{
    [Test]
    public void ManifestMatchesMilkyPolymorphismAndCoreEntityInventory()
    {
        var input = typeof(OutgoingSegmentBase).GetCustomAttributes<JsonDerivedTypeAttribute>().Select(x => (string)x.TypeDiscriminator!).ToArray();
        var report = typeof(IncomingSegmentBase).GetCustomAttributes<JsonDerivedTypeAttribute>().Select(x => (string)x.TypeDiscriminator!).ToArray();
        var entries = MessageElementManifest.Entries;
        Assert.That(input, Is.EquivalentTo(entries.Where(x => x.Directions.HasFlag(MessageElementDirections.Input)).Select(x => x.Segment)));
        Assert.That(report, Is.EquivalentTo(entries.Where(x => x.Directions.HasFlag(MessageElementDirections.Report)).Select(x => x.Segment)));
        var entities = typeof(IMessageEntity).Assembly.GetTypes().Where(x => !x.IsAbstract && typeof(IMessageEntity).IsAssignableFrom(x)).Select(x => x.Name);
        Assert.That(entries.Select(x => x.Entity).Distinct(), Is.EquivalentTo(entities));
    }
    [Test]
    public async Task FileInputIsForwardOnlyAndRetainsMetadata()
    {
        var converter = new MilkyConverter(null!, new MessageCache(null!, NullLoggerFactory.Instance), null!);
        var file = Serializer.JsonDeserialize<OutgoingSegmentBase>("""{"type":"file","data":{"file_id":"id","file_name":"report","file_size":12,"file_hash":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","url":"https://example.test/file"}}""")!;
        Assert.That(file, Is.TypeOf<FileOutgoingSegment>());
        Assert.ThrowsAsync<NotSupportedException>(() => converter.FromOutgoingSegmentsAsync([file], MessageType.Group, 123));
        var forward = Serializer.JsonDeserialize<OutgoingSegmentBase>("""{"type":"forward","data":{"title":"title","preview":["preview"],"summary":"summary","prompt":"prompt","messages":[{"user_id":123,"sender_name":"user","segments":[{"type":"file","data":{"file_id":"id","file_name":"report","file_size":12}}]}]}}""")!;
        var result = (MultiMsgEntity)(await converter.FromOutgoingSegmentsAsync([forward], MessageType.Group, 123)).Single();
        Assert.That(result.Title, Is.EqualTo("title"));
        Assert.That(result.Preview, Is.EqualTo(new[] { "preview" }));
        Assert.That(result.Messages.Single().Entities.Single(), Is.TypeOf<GroupFileEntity>());
        Assert.That(((GroupFileEntity)result.Messages.Single().Entities.Single()).FileSize, Is.EqualTo(12));
    }
    [Test]
    public void GroupCardEventIsSerializableWithoutReflection()
    {
        var data = new Lagrange.Milky.Events.Converters.GroupCardChangeEventConverter.Data(123, 456, "old", "new");
        using var json = JsonDocument.Parse(Serializer.JsonSerialize(data));
        Assert.That(json.RootElement.GetProperty("new_card").GetString(), Is.EqualTo("new"));
    }
}
