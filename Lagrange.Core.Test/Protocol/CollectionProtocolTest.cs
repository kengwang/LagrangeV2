using Lagrange.Core.Internal.Packets.Web;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Test.Protocol;

public sealed class CollectionProtocolTest
{
    [Test]
    public void RequestRoundTripsWithStableFieldNumbers()
    {
        var request = new CollectionRequestBody
        {
            Operation = new CollectionRequestOperation
            {
                GetCollectionList = new CollectionListReq
                {
                    Timestamp = ulong.MaxValue,
                    OrderType = 2,
                    Count = 50,
                    SearchDown = 1
                }
            }
        };

        var encoded = ProtoHelper.Serialize(request);
        var decoded = ProtoHelper.Deserialize<CollectionRequestBody>(encoded.Span);
        var page = decoded.Operation.GetCollectionList!;
        Assert.Multiple(() =>
        {
            Assert.That(page.Timestamp, Is.EqualTo(ulong.MaxValue));
            Assert.That(page.OrderType, Is.EqualTo(2));
            Assert.That(page.Count, Is.EqualTo(50));
            Assert.That(page.SearchDown, Is.EqualTo(1));
        });
    }

    [Test]
    public void ResponsePreservesCollectionMetadata()
    {
        var response = new CollectionResponseBody
        {
            Operation = new CollectionResponseOperation
            {
                GetCollectionList = new CollectionListResp
                {
                    TotalCount = 1,
                    ReachedBottom = 1,
                    Items =
                    [
                        new CollectionItem
                        {
                            Id = "id",
                            Type = 3,
                            CreateTime = 11,
                            CollectTime = 12,
                            ModifyTime = 13,
                            Summary = new CollectionSummary { Text = new CollectionTextSummary { Text = "hello" } },
                            Author = new CollectionAuthor { NumId = 123 }
                        }
                    ]
                }
            }
        };

        var decoded = ProtoHelper.Deserialize<CollectionResponseBody>(ProtoHelper.Serialize(response).Span);
        var item = decoded.Operation.GetCollectionList!.Items!.Single();
        Assert.Multiple(() =>
        {
            Assert.That(item.Id, Is.EqualTo("id"));
            Assert.That(item.Type, Is.EqualTo(3));
            Assert.That(item.Summary!.Text!.Text, Is.EqualTo("hello"));
            Assert.That(item.Author!.NumId, Is.EqualTo(123));
        });
    }
}
